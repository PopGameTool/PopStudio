using System.Globalization;
using System.IO.Compression;
using System.Reflection;
using System.Xml.Linq;

// Run with: dotnet run --project Tests/PopStudio.Reanim.Tests
// Exercise the production conversion API without making its internal types public.
internal static class Program
{
    private static readonly Assembly Production = Assembly.Load("PopStudio");
    private static readonly Type Api = Production.GetType("PopStudio.Platform.YFAPI", true)!;
    private static readonly XNamespace Ns = "http://ns.adobe.com/xfl/2008/";
    private static readonly string Output = Path.Combine(AppContext.BaseDirectory, "cases");
    private static int _passed;

    private static void Main()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        Directory.CreateDirectory(Output);
        object platform = Activator.CreateInstance(Production.GetType("PopStudio.Platform.ConsoleAPI", true)!, true)!;
        Api.GetMethod("RegistPlatform", new[] { typeof(object) })!.Invoke(null, new[] { platform });

        Run("linear position, rotation, scale and alpha", () =>
        {
            string input = Input("linear", Track("sprite", Enumerable.Range(0, 21).Select(i =>
                Frame(i, ("x", i * 2), ("y", -i), ("kx", i * 3), ("ky", i * 3),
                    ("sx", 1 + i * 0.01), ("sy", 1 + i * 0.02), ("a", 1 - i * 0.02)))));
            var result = Export(input, "linear-on", true);
            var frames = Frames(result.Document);
            Check(frames.Length == 2 && IsTween(frames[0]) && Duration(frames[0]) == 20, "21 samples should become two keys");
            Check((string?)frames[0].Attribute("motionTweenRotate") == "auto", "explicit rotation mode");
            Check(frames.All(frame => frame.Descendants(Ns + "transformationPoint").Any()), "explicit origin pivots");
            RoundTrip(input, result.Path, "linear");

            var disabled = Export(input, "linear-off", false);
            var omitted = Export(input, "linear-default", null);
            Check(Frames(disabled.Document).Length == 21 && !Frames(disabled.Document).Any(IsTween), "disabled must retain every frame");
            Check(XNode.DeepEquals(disabled.Document, omitted.Document), "default must equal disabled");
        });

        Run("sparse samples and trailing holds", () =>
        {
            string input = Input("holds", Track("sprite", Enumerable.Range(0, 12).Select(i =>
                i == 0 ? Frame(i, ("x", 4), ("y", 8), ("a", 0.5)) : new XElement("t"))));
            var result = Export(input, "holds-on", true);
            Check(Frames(result.Document).Length == 1 && Duration(Frames(result.Document)[0]) == 12, "hold must preserve timeline length");
            RoundTrip(input, result.Path, "holds");
        });

        Run("image changes, blank frames and hidden spans", () =>
        {
            string input = Input("changes", Track("sprite", Enumerable.Range(0, 18).Select(i =>
            {
                XElement frame = Frame(i, ("x", i));
                if (i == 5) frame.Add(new XElement("i", "IMAGE_REANIM_OTHER"));
                if (i == 9) frame.Add(new XElement("f", -1));
                if (i == 13) frame.Add(new XElement("f", 0));
                return frame;
            })));
            var result = Export(input, "changes-on", true);
            foreach (int boundary in new[] { 5, 9, 13 }) AssertBoundary(result.Document, boundary);
            Check(!Frames(result.Document).Any(frame => Index(frame) >= 9 && Index(frame) < 13 && IsTween(frame)), "hidden frames cannot tween");
            RoundTrip(input, result.Path, "changes");
        });

        Run("action marker boundaries affect other layers", () =>
        {
            XElement marker = Track("anim_walk", Enumerable.Range(0, 15).Select(i =>
                new XElement("t", i == 0 || i == 7 ? new XElement("f", i == 0 ? -1 : 0) : null)));
            string input = Input("markers", Track("sprite", Enumerable.Range(0, 15).Select(i => Frame(i, ("x", i)))), marker);
            var result = Export(input, "markers-on", true);
            AssertBoundary(result.Document, 7);
        });

        Run("nonlinear motion keeps its intermediate poses", () =>
        {
            string input = Input("nonlinear", Track("sprite", Enumerable.Range(0, 21).Select(i => Frame(i, ("x", i * i)))));
            var result = Export(input, "nonlinear-on", true);
            Check(Frames(result.Document).Length == 21, "quadratic samples exceed linear tolerance");
            RoundTrip(input, result.Path, "nonlinear");
        });

        Run("stops and reversals", () =>
        {
            double[] positions = { 0, 5, 10, 15, 15, 15, 15, 10, 5, 0, 0, 0 };
            string input = Input("reverse", Track("sprite", positions.Select((x, i) => Frame(i, ("x", x)))));
            var result = Export(input, "reverse-on", true);
            Check(Frames(result.Document).Length < positions.Length, "piecewise linear motion should compress");
            RoundTrip(input, result.Path, "reverse");
        });

        Run("full turns and angle wrap", () =>
        {
            foreach (bool wrap in new[] { false, true })
            {
                string name = wrap ? "wrapped" : "full-turn";
                string input = Input(name, Track("sprite", Enumerable.Range(0, 37).Select(i =>
                {
                    double angle = wrap ? (350 + i * 10) % 360 : i * 10;
                    return Frame(i, ("kx", angle), ("ky", angle));
                })));
                var result = Export(input, name + "-on", true);
                Check(Frames(result.Document).Length > 2 && Frames(result.Document).Any(IsTween), "turns must be split into unambiguous spans");
                RoundTrip(input, result.Path, name);
            }
        });

        Run("changing skew and mirrored scales fall back", () =>
        {
            string input = Input("skew", Track("sprite", Enumerable.Range(0, 10).Select(i => Frame(i, ("kx", i * 3), ("ky", -i * 2)))));
            var result = Export(input, "skew-on", true);
            Check(Frames(result.Document).Length == 10 && !Frames(result.Document).Any(IsTween), "changing skew must stay explicit");
            RoundTrip(input, result.Path, "skew");
            input = Input("mirror", Track("sprite", Enumerable.Range(0, 10).Select(i => Frame(i, ("sx", -1 - i * 0.1)))));
            result = Export(input, "mirror-on", true);
            Check(!Frames(result.Document).Any(IsTween), "changing mirrored transforms must stay explicit");
            RoundTrip(input, result.Path, "mirror");
        });

        Run("position tolerance and export scaling", () =>
        {
            Type settings = Production.GetType("PopStudio.Setting", true)!;
            FieldInfo scale = settings.GetField("ReanimXflScaleX")!;
            string input = Input("noise", Track("sprite", new[] { 0.0, 1.04, 2.0 }.Select((x, i) => Frame(i, ("x", x)))));
            Check(Frames(Export(input, "noise-on", true).Document).Any(IsTween), "small quantization noise can fit");
            scale.SetValue(null, 2.0);
            try
            {
                Check(!Frames(Export(input, "noise-scaled", true).Document).Any(IsTween), "tolerance must use exported coordinates");
            }
            finally { scale.SetValue(null, 1.0); }
        });

        Run("FLA uses the same prediction and default", () =>
        {
            string input = Input("archive", Track("sprite", Enumerable.Range(0, 10).Select(i => Frame(i, ("x", i)))));
            foreach (bool enabled in new[] { false, true })
            {
                string path = Path.Combine(Output, "archive-" + enabled + ".fla");
                Convert(input, path, 7, 10, enabled);
                using ZipArchive archive = ZipFile.OpenRead(path);
                using Stream stream = archive.GetEntry("DOMDocument.xml")!.Open();
                XDocument document = XDocument.Load(stream);
                Check(Frames(document).Length == (enabled ? 2 : 10), "FLA keyframe count");
                Check(Frames(document).Any(IsTween) == enabled, "FLA option forwarding");
                RoundTrip(input, path, "archive-" + enabled, 9);
            }
            string automatic = Path.Combine(Output, "auto.fla");
            Api.GetMethod("ParseReanim")!.Invoke(null, new object[] { input, automatic, 10, true });
            using ZipArchive autoArchive = ZipFile.OpenRead(automatic);
            using Stream autoStream = autoArchive.GetEntry("DOMDocument.xml")!.Open();
            Check(Frames(XDocument.Load(autoStream)).Any(IsTween), "automatic input detection forwards option");
        });

        Run("empty, single-frame and unequal-length tracks", () =>
        {
            var empty = Export(Input("empty"), "empty-on", true);
            Check(!empty.Document.Descendants(Ns + "DOMFrame").Any(), "empty animation");
            string input = Input("short", Track("sprite", new[] { Frame(0, ("x", 7)) }), Track("empty", Array.Empty<XElement>()),
                Track("long", Enumerable.Range(0, 5).Select(i => Frame(i, ("x", i)))));
            var result = Export(input, "short-on", true);
            Check(Frames(result.Document).Length == 1 && Duration(Frames(result.Document)[0]) == 1, "single frame");
            Check(Frames(result.Document, "empty").Length == 0, "empty track");
            Check(Frames(result.Document, "long").Length == 2, "long track");
        });

        Run("long animation preserves the final frame", () =>
        {
            string input = Input("long", Track("sprite", Enumerable.Range(0, 10000).Select(i => Frame(i, ("x", i)))));
            var result = Export(input, "long-on", true);
            XElement[] frames = Frames(result.Document);
            Check(frames.Length < 100 && frames.Sum(Duration) == 10000, "long animation duration/compression");
            Check(Index(frames[^1]) == 9999, "last endpoint");
        });

        Console.WriteLine($"PASS: {_passed} regression groups. Fixtures: {Output}");
    }

    private static XElement Frame(int i, params (string Key, double Value)[] values) => new("t",
        i == 0 ? new XElement("i", "IMAGE_REANIM_SPRITE") : null,
        values.Select(value => new XElement(value.Key, value.Value.ToString(CultureInfo.InvariantCulture))));
    private static XElement Track(string name, IEnumerable<XElement> frames) => new("track", new XElement("name", name), frames);
    private static string Input(string name, params XElement[] tracks)
    {
        string path = Path.Combine(Output, name + ".reanim");
        File.WriteAllText(path, "<fps>12</fps>\n" + string.Join("\n", tracks.Select(track => track.ToString())));
        return path;
    }

    private static void Convert(string input, string output, int from, int to, bool? predict) =>
        Api.GetMethod("Reanim")!.Invoke(null, new[] { (object)input, output, from, to, predict.HasValue ? (object)predict.Value : Type.Missing });
    private static (string Path, XDocument Document) Export(string input, string name, bool? predict, int from = 7)
    {
        string path = Path.Combine(Output, name);
        Convert(input, path, from, 8, predict);
        return (path, XDocument.Load(Path.Combine(path, "DOMDocument.xml")));
    }

    private static XElement[] Frames(XDocument document, string layer = "sprite") => document.Descendants(Ns + "DOMLayer")
        .Single(element => (string?)element.Attribute("name") == layer).Element(Ns + "frames")!.Elements().ToArray();
    private static int Index(XElement frame) => (int)frame.Attribute("index")!;
    private static int Duration(XElement frame) => (int?)frame.Attribute("duration") ?? 1;
    private static bool IsTween(XElement frame) => (string?)frame.Attribute("tweenType") == "motion";
    private static void AssertBoundary(XDocument document, int boundary)
    {
        XElement[] frames = Frames(document);
        Check(frames.Any(frame => Index(frame) == boundary), $"missing boundary {boundary}");
        Check(!frames.Any(frame => IsTween(frame) && Index(frame) < boundary && Index(frame) + Duration(frame) >= boundary), $"tween crosses boundary {boundary}");
    }

    private static void RoundTrip(string source, string optimized, string name, int from = 8)
    {
        XElement[] expected = Frames(Export(source, name + "-reference", false).Document);
        XElement[] actual = Frames(Export(optimized, name + "-baked", false, from).Document);
        Check(actual.Length == expected.Length, name + ": changed duration");
        for (int i = 0; i < expected.Length; i++)
        {
            XElement? first = expected[i].Descendants(Ns + "DOMSymbolInstance").FirstOrDefault();
            XElement? last = actual[i].Descendants(Ns + "DOMSymbolInstance").FirstOrDefault();
            Check((first == null) == (last == null), name + ": changed visibility");
            if (first == null || last == null) continue;
            Check((string?)first.Attribute("libraryItemName") == (string?)last.Attribute("libraryItemName"), name + ": changed image");
            XElement a = first.Descendants(Ns + "Matrix").Single();
            XElement b = last.Descendants(Ns + "Matrix").Single();
            foreach (string attribute in new[] { "a", "b", "c", "d", "tx", "ty" })
            {
                double defaultValue = attribute is "a" or "d" ? 1 : 0;
                double error = Math.Abs(((double?)a.Attribute(attribute) ?? defaultValue) - ((double?)b.Attribute(attribute) ?? defaultValue));
                Check(error < (attribute is "tx" or "ty" ? 0.11 : 0.003), $"{name}: frame {i}, {attribute}, error {error}");
            }
            double alphaA = (double?)first.Descendants(Ns + "Color").FirstOrDefault()?.Attribute("alphaMultiplier") ?? 1;
            double alphaB = (double?)last.Descendants(Ns + "Color").FirstOrDefault()?.Attribute("alphaMultiplier") ?? 1;
            Check(Math.Abs(alphaA - alphaB) < 0.011, name + ": alpha mismatch");
        }
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
    private static void Run(string name, Action test)
    {
        test();
        _passed++;
        Console.WriteLine("PASS: " + name);
    }
}
