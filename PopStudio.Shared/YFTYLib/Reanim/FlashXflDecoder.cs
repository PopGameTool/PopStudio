// The XFL parsing and REANIM conversion behavior in this file is derived from
// Flash2Reanim by Wanxi, licensed under the MIT License. See THIRD_PARTY_NOTICES.md.

using System.Globalization;
using System.Xml.Linq;

namespace PopStudio.Reanim
{
    internal static class FlashXflDecoder
    {
        private const double Pi = Math.PI;
        private const double RadiansToDegrees = 180.0 / Pi;

        public static Reanim Decode(string inFile)
        {
            return Decode(XflProject.Load(inFile));
        }

        public static Reanim DecodeArchive(string inFile)
        {
            return Decode(XflProject.LoadArchive(inFile));
        }

        public static bool IsZipXflFile(string inFile)
        {
            if (!File.Exists(inFile)) return false;
            try
            {
                using FileStream stream = new FileStream(inFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                using ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Read, false);
                return archive.Entries.Any(entry => !string.IsNullOrEmpty(entry.Name)
                    && string.Equals(entry.Name, "DOMDocument.xml", StringComparison.OrdinalIgnoreCase));
            }
            catch (InvalidDataException)
            {
                return false;
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        private static Reanim Decode(XflProject project)
        {
            XflTimeline timeline = project.MainTimeline;
            int totalFrames = timeline.FrameCount;
            List<ReanimTrack> tracks = new List<ReanimTrack>();

            for (int layerIndex = timeline.Layers.Count - 1; layerIndex >= 0; layerIndex--)
            {
                XflLayer layer = timeline.Layers[layerIndex];
                if (layer.Name.StartsWith("_", StringComparison.Ordinal) && layer.Name != "_ground")
                {
                    continue;
                }

                List<XflFrame> frames = layer.BakeFrames(totalFrames);
                List<BaseResult>[] basesByFrame = new List<BaseResult>[frames.Count];
                int elementCount = 1;
                for (int frameIndex = 0; frameIndex < frames.Count; frameIndex++)
                {
                    basesByFrame[frameIndex] = FindBases(frames[frameIndex], project);
                    elementCount = Math.Max(elementCount, basesByFrame[frameIndex].Count);
                }

                for (int elementIndex = 0; elementIndex < elementCount; elementIndex++)
                {
                    FrameState previous = new FrameState();
                    ReanimTransform[] transforms = new ReanimTransform[totalFrames];
                    for (int frameIndex = 0; frameIndex < totalFrames; frameIndex++)
                    {
                        List<BaseResult> bases = basesByFrame[frameIndex];
                        transforms[frameIndex] = elementIndex < bases.Count
                            ? ConvertFrame(bases[elementIndex], frames.Count == 0 ? null : frames[0], previous, frameIndex, layer.Name, project)
                            : HideFrame(previous);
                    }
                    tracks.Add(new ReanimTrack
                    {
                        name = layer.Name + (elementIndex == 0 ? string.Empty : (elementIndex + 1).ToString(CultureInfo.InvariantCulture)),
                        transforms = transforms
                    });
                }
            }

            return new Reanim
            {
                fps = ToFloat(project.FrameRate),
                tracks = tracks.ToArray()
            };
        }

        private static ReanimTransform HideFrame(FrameState previous)
        {
            ReanimTransform transform = new ReanimTransform();
            if (previous.F != -1)
            {
                previous.F = -1;
                transform.f = -1;
            }
            return transform;
        }

        private static ReanimTransform ConvertFrame(BaseResult result, XflFrame root, FrameState previous,
            int frameIndex, string layerName, XflProject project)
        {
            XflElement element = result.Element;
            XflMatrix matrix = result.Matrix;
            MatrixParts parts = DecomposeMatrix(matrix);
            double kx = parts.XAngle * RadiansToDegrees;
            double ky = -parts.YAngle * RadiansToDegrees;

            if (frameIndex == 0)
            {
                if (kx < 0) kx += 360;
                if (ky < 0) ky += 360;
            }
            else
            {
                while (previous.Kx - kx > 180) kx += 360;
                while (previous.Kx - kx < -180) kx -= 360;
                while (previous.Ky - ky > 180) ky += 360;
                while (previous.Ky - ky < -180) ky -= 360;
            }

            string image = string.Empty;
            string font = string.Empty;
            string text = string.Empty;
            if (layerName == "_ground" || layerName == "fullscreen" || layerName.StartsWith("locator", StringComparison.Ordinal))
            {
                // These are transform-only tracks.
            }
            else if (layerName.StartsWith("attacher__", StringComparison.Ordinal))
            {
                XflLibraryItem symbolItem = result.Symbol == null ? null : project.FindLibraryItem(result.Symbol.LibraryItemName);
                XflLibraryItem elementItem = project.FindLibraryItem(element.LibraryItemName);
                if (symbolItem != null && symbolItem.Name.Contains("__", StringComparison.Ordinal))
                {
                    text = symbolItem.Name;
                }
                else if (elementItem != null && elementItem.Name.Contains("__", StringComparison.Ordinal))
                {
                    text = elementItem.Name;
                }
                else if (elementItem != null)
                {
                    text = BeforeFirstDot(elementItem.Name);
                    if (!text.StartsWith("attacher__", StringComparison.Ordinal))
                    {
                        image = "IMAGE_REANIM_" + text;
                        text = string.Empty;
                    }
                }
            }
            else if (element.IsText)
            {
                XElement run = Child(Child(element.Node, "textRuns"), "DOMTextRun");
                if (run != null)
                {
                    XElement attrs = Child(Child(run, "textAttrs"), "DOMTextAttrs");
                    string family = string.Empty;
                    if (attrs != null)
                    {
                        string url = Attr(attrs, "url");
                        family = string.IsNullOrEmpty(url)
                            ? Attr(attrs, "face") + Round(ParseDouble(Attr(attrs, "size"), 0) * 0.8, 0).ToString(CultureInfo.InvariantCulture)
                            : url;
                    }
                    font = "FONT_" + family;
                    text = Child(run, "characters")?.Value ?? string.Empty;
                    matrix.Tx -= element.Matrix.Tx;
                }
            }
            else
            {
                XflLibraryItem item = project.FindLibraryItem(element.LibraryItemName);
                bool emptySymbol = item?.Timeline != null
                    && item.Timeline.Layers.Count > 0
                    && item.Timeline.Layers[0].FrameAt(0)?.Elements.Count == 0;
                if (item != null && !emptySymbol)
                {
                    image = "IMAGE_REANIM_" + BeforeFirstDot(item.Name);
                }
                else if (item == null && root?.Elements.Count == 1)
                {
                    XflLibraryItem rootItem = project.FindLibraryItem(root.Elements[0].LibraryItemName);
                    if (rootItem != null)
                    {
                        image = "IMAGE_REANIM_" + rootItem.Name;
                    }
                }
            }

            image = image.ToUpperInvariant();
            font = font.ToUpperInvariant();
            float x = Round(matrix.Tx, 1);
            float y = Round(matrix.Ty, 1);
            float roundedKx = Round(kx, 1);
            float roundedKy = Round(ky, 1);
            float sx = Round(parts.ScaleX, 3);
            float sy = Round(parts.ScaleY, 3);
            float alpha = Round(result.Alpha, 2);
            const float visible = 0;

            ReanimTransform transform = new ReanimTransform();
            if (previous.X != x) { previous.X = x; transform.x = x; }
            if (previous.Y != y) { previous.Y = y; transform.y = y; }
            if (previous.Kx != roundedKx) { previous.Kx = roundedKx; transform.kx = roundedKx; }
            if (previous.Ky != roundedKy) { previous.Ky = roundedKy; transform.ky = roundedKy; }
            if (previous.Sx != sx) { previous.Sx = sx; transform.sx = sx; }
            if (previous.Sy != sy) { previous.Sy = sy; transform.sy = sy; }
            if (previous.F != visible) { previous.F = visible; transform.f = visible; }
            if (previous.Alpha != alpha) { previous.Alpha = alpha; transform.a = alpha; }
            if (previous.Image != image) { previous.Image = image; transform.i = image; }
            if (previous.Font != font) { previous.Font = font; transform.font = font; }
            if (previous.Text != text)
            {
                previous.Text = text;
                transform.text = string.IsNullOrEmpty(text) ? "_" : text;
            }
            return transform;
        }

        private static List<BaseResult> FindBases(XflFrame frame, XflProject project)
        {
            List<BaseResult> result = new List<BaseResult>();
            if (frame == null)
            {
                return result;
            }
            foreach (XflElement element in frame.Elements)
            {
                CollectBases(element, XflMatrix.Identity, 1, null, result, project, 0);
            }
            return result;
        }

        private static void CollectBases(XflElement element, XflMatrix outer, double outerAlpha, XflElement symbol,
            List<BaseResult> result, XflProject project, int depth)
        {
            XflMatrix combined = ConcatMatrix(element.Matrix, outer);
            double combinedAlpha = element.Alpha * outerAlpha;
            if (depth >= 64 || !element.IsInstance)
            {
                result.Add(new BaseResult(element, symbol, combined, combinedAlpha));
                return;
            }

            XflLibraryItem item = project.FindLibraryItem(element.LibraryItemName);
            if (item?.Timeline == null || (item.ItemType != "movie clip" && item.ItemType != "graphic"))
            {
                result.Add(new BaseResult(element, symbol, combined, combinedAlpha));
                return;
            }

            bool foundNested = false;
            foreach (XflLayer layer in item.Timeline.Layers)
            {
                XflFrame frame = layer.FrameAt(0);
                if (frame == null)
                {
                    continue;
                }
                foreach (XflElement nested in frame.Elements)
                {
                    foundNested = true;
                    CollectBases(nested, combined, combinedAlpha, element, result, project, depth + 1);
                }
            }
            if (!foundNested)
            {
                result.Add(new BaseResult(element, symbol, combined, combinedAlpha));
            }
        }

        private static MatrixParts DecomposeMatrix(XflMatrix matrix)
        {
            double xAngle = Math.Atan2(matrix.B, matrix.A);
            double scaleX = Math.Sqrt(matrix.A * matrix.A + matrix.B * matrix.B);
            double determinant = matrix.A * matrix.D - matrix.B * matrix.C;
            double scaleY = Math.Sqrt(matrix.C * matrix.C + matrix.D * matrix.D);
            if (determinant < 0) scaleY = -scaleY;
            double yAngle = Math.Atan2(matrix.C, matrix.D);
            if (scaleY < 0) yAngle += Pi;
            return new MatrixParts(xAngle, yAngle, scaleX, scaleY);
        }

        private static XflMatrix InterpolateMatrix(XflMatrix start, XflMatrix end, double progress)
        {
            MatrixParts first = DecomposeMatrix(start);
            MatrixParts last = DecomposeMatrix(end);
            double endX = Unwrap(first.XAngle, last.XAngle);
            double endY = Unwrap(first.YAngle, last.YAngle);
            double sx = Lerp(first.ScaleX, last.ScaleX, progress);
            double sy = Lerp(first.ScaleY, last.ScaleY, progress);
            double kx = Lerp(first.XAngle, endX, progress);
            double ky = Lerp(first.YAngle, endY, progress);
            return new XflMatrix(
                sx * Math.Cos(kx), sx * Math.Sin(kx),
                sy * Math.Sin(ky), sy * Math.Cos(ky),
                Lerp(start.Tx, end.Tx, progress), Lerp(start.Ty, end.Ty, progress));
        }

        private static XflMatrix ConcatMatrix(XflMatrix inner, XflMatrix outer)
        {
            return new XflMatrix(
                outer.A * inner.A + outer.C * inner.B,
                outer.B * inner.A + outer.D * inner.B,
                outer.A * inner.C + outer.C * inner.D,
                outer.B * inner.C + outer.D * inner.D,
                outer.A * inner.Tx + outer.C * inner.Ty + outer.Tx,
                outer.B * inner.Tx + outer.D * inner.Ty + outer.Ty);
        }

        private static double Unwrap(double start, double end)
        {
            while (start - end > Pi) end += Pi * 2;
            while (start - end < -Pi) end -= Pi * 2;
            return end;
        }

        private static double Lerp(double start, double end, double progress) => start + (end - start) * progress;
        private static double Clamp01(double value) => Math.Max(0, Math.Min(1, value));

        private static float Round(double value, int places)
        {
            if (double.IsNaN(value) || double.IsInfinity(value)) value = 0;
            return (float)Math.Round(value, places, MidpointRounding.AwayFromZero);
        }

        private static float ToFloat(double value)
        {
            return double.IsNaN(value) || double.IsInfinity(value) ? 0 : (float)value;
        }

        private static string BeforeFirstDot(string value)
        {
            int dot = value.IndexOf('.');
            return dot < 0 ? value : value[..dot];
        }

        private static string Attr(XElement element, string name, string fallback = "")
            => element?.Attribute(name)?.Value ?? fallback;

        private static bool HasAttr(XElement element, string name) => element?.Attribute(name) != null;

        private static XElement Child(XElement element, string localName)
            => element?.Elements().FirstOrDefault(child => child.Name.LocalName == localName);

        private static IEnumerable<XElement> Children(XElement element, string localName)
            => element == null ? Enumerable.Empty<XElement>()
                : localName == null ? element.Elements()
                : element.Elements().Where(child => child.Name.LocalName == localName);

        private static double ParseDouble(string value, double fallback)
            => double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double result) ? result : fallback;

        private static int ParseInt(string value, int fallback)
            => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result) ? result : fallback;

        private sealed class FrameState
        {
            public float X;
            public float Y;
            public float Kx;
            public float Ky;
            public float Sx = 1;
            public float Sy = 1;
            public float F;
            public float Alpha = 1;
            public string Image = string.Empty;
            public string Font = string.Empty;
            public string Text = string.Empty;
        }

        private sealed class BaseResult
        {
            public BaseResult(XflElement element, XflElement symbol, XflMatrix matrix, double alpha)
            {
                Element = element;
                Symbol = symbol;
                Matrix = matrix;
                Alpha = alpha;
            }

            public XflElement Element { get; }
            public XflElement Symbol { get; }
            public XflMatrix Matrix { get; }
            public double Alpha { get; }
        }

        private struct XflMatrix
        {
            public XflMatrix(double a, double b, double c, double d, double tx, double ty)
            {
                A = a;
                B = b;
                C = c;
                D = d;
                Tx = tx;
                Ty = ty;
            }

            public static XflMatrix Identity => new XflMatrix(1, 0, 0, 1, 0, 0);
            public double A;
            public double B;
            public double C;
            public double D;
            public double Tx;
            public double Ty;
        }

        private readonly struct MatrixParts
        {
            public MatrixParts(double xAngle, double yAngle, double scaleX, double scaleY)
            {
                XAngle = xAngle;
                YAngle = yAngle;
                ScaleX = scaleX;
                ScaleY = scaleY;
            }

            public double XAngle { get; }
            public double YAngle { get; }
            public double ScaleX { get; }
            public double ScaleY { get; }
        }

        private sealed class XflElement
        {
            public XflElement(XElement node, XflMatrix? matrix = null, double? alpha = null)
            {
                Node = node;
                Matrix = matrix ?? ReadMatrix(node);
                Alpha = alpha ?? ReadAlpha(node);
            }

            public XElement Node { get; }
            public XflMatrix Matrix { get; }
            public double Alpha { get; }
            public string LibraryItemName => Attr(Node, "libraryItemName");
            public bool IsText => Node.Name.LocalName is "DOMStaticText" or "DOMDynamicText" or "DOMInputText";
            public bool IsInstance => !IsText && Node.Name.LocalName != "DOMShape";

            private static XflMatrix ReadMatrix(XElement node)
            {
                XElement container = Child(node, "matrix");
                XElement value = Child(container, "Matrix") ?? container;
                return value == null
                    ? XflMatrix.Identity
                    : new XflMatrix(
                        ParseDouble(Attr(value, "a"), 1), ParseDouble(Attr(value, "b"), 0),
                        ParseDouble(Attr(value, "c"), 0), ParseDouble(Attr(value, "d"), 1),
                        ParseDouble(Attr(value, "tx"), 0), ParseDouble(Attr(value, "ty"), 0));
            }

            private static double ReadAlpha(XElement node)
            {
                XElement color = Child(Child(node, "color"), "Color");
                return color == null
                    ? 1
                    : ParseDouble(Attr(color, "alphaMultiplier"), 1) + ParseDouble(Attr(color, "alphaOffset"), 0) / 100.0;
            }
        }

        private sealed class XflFrame
        {
            public XflFrame(XElement node, int? index = null, List<XflElement> elements = null)
            {
                Node = node;
                Index = index ?? ParseInt(Attr(node, "index"), 0);
                Duration = ParseInt(Attr(node, "duration"), 1);
                Elements = elements ?? Children(Child(node, "elements"), null)
                    .Select(element => new XflElement(element)).ToList();
            }

            public XElement Node { get; }
            public int Index { get; }
            public int Duration { get; }
            public int End => Index + Duration;
            public string TweenType => Attr(Node, "tweenType");
            public List<XflElement> Elements { get; }

            public double ProgressAt(int frame)
            {
                double progress = Clamp01((double)(frame - Index) / Math.Max(1, Duration));
                List<XElement> points = Node.Descendants()
                    .Where(point => point.Name.LocalName == "Point"
                        && point.Attribute("x") != null
                        && point.Attribute("y") != null
                        && point.Ancestors().Any(parent => NormalizeName(parent.Name.LocalName) == "customease"))
                    .OrderBy(point => ParseDouble(Attr(point, "x"), 0))
                    .ToList();
                if (points.Count >= 2)
                {
                    for (int i = 1; i < points.Count; i++)
                    {
                        double currentX = ParseDouble(Attr(points[i], "x"), 0);
                        if (progress > currentX) continue;
                        double previousX = ParseDouble(Attr(points[i - 1], "x"), 0);
                        double previousY = ParseDouble(Attr(points[i - 1], "y"), 0);
                        double currentY = ParseDouble(Attr(points[i], "y"), 0);
                        double span = currentX - previousX;
                        return Clamp01(span == 0 ? previousY : Lerp(previousY, currentY, (progress - previousX) / span));
                    }
                    return Clamp01(ParseDouble(Attr(points[^1], "y"), 0));
                }

                string fallback = Attr(Node, "tweenEasing");
                string fallbackName = HasAttr(Node, "easingName") ? Attr(Node, "easingName") : fallback;
                if (HasAttr(Node, "easing") || HasAttr(Node, "tweenEasing") || HasAttr(Node, "easingName"))
                {
                    return Ease(Attr(Node, "easing", fallbackName), progress);
                }

                double acceleration = ParseDouble(Attr(Node, "acceleration"), 0);
                if (acceleration != 0)
                {
                    double amount = Math.Min(Math.Abs(acceleration) / 100.0, 1);
                    return Lerp(progress, Ease(acceleration > 0 ? "quadraticout" : "quadraticin", progress), amount);
                }
                return progress;
            }
        }

        private sealed class XflLayer
        {
            private readonly List<XflFrame> _keyframes;

            public XflLayer(XElement node)
            {
                Node = node;
                _keyframes = Children(Child(node, "frames"), "DOMFrame")
                    .Select(frame => new XflFrame(frame)).OrderBy(frame => frame.Index).ToList();
            }

            public XElement Node { get; }
            public string Name => Attr(Node, "name");
            public int FrameCount => _keyframes.Count == 0 ? 0 : _keyframes.Max(frame => frame.End);

            public XflFrame FrameAt(int index)
            {
                XflFrame previous = null;
                foreach (XflFrame frame in _keyframes)
                {
                    if (frame.Index > index) break;
                    previous = frame;
                    if (index < frame.End) return frame;
                }
                return previous;
            }

            public List<XflFrame> BakeFrames(int totalFrames)
            {
                List<XflFrame> result = new List<XflFrame>(totalFrames);
                for (int index = 0; index < totalFrames; index++)
                {
                    XflFrame current = null;
                    XflFrame next = null;
                    for (int keyIndex = 0; keyIndex < _keyframes.Count; keyIndex++)
                    {
                        XflFrame candidate = _keyframes[keyIndex];
                        if (candidate.Index <= index && index < candidate.End)
                        {
                            current = candidate;
                            if (keyIndex + 1 < _keyframes.Count) next = _keyframes[keyIndex + 1];
                            break;
                        }
                    }

                    List<XflElement> elements = new List<XflElement>();
                    if (current != null)
                    {
                        bool tween = next != null && next.Index == current.End && current.TweenType == "motion";
                        double progress = tween ? current.ProgressAt(index) : 0;
                        for (int elementIndex = 0; elementIndex < current.Elements.Count; elementIndex++)
                        {
                            XflElement source = current.Elements[elementIndex];
                            XflElement destination = next != null && elementIndex < next.Elements.Count ? next.Elements[elementIndex] : null;
                            bool sameElement = tween && destination != null
                                && source.Node.Name.LocalName == destination.Node.Name.LocalName
                                && source.LibraryItemName == destination.LibraryItemName;
                            elements.Add(sameElement
                                ? new XflElement(source.Node,
                                    InterpolateMatrix(source.Matrix, destination.Matrix, progress),
                                    Lerp(source.Alpha, destination.Alpha, progress))
                                : source);
                        }
                    }
                    result.Add(new XflFrame(current?.Node ?? new XElement("DOMFrame"), index, elements));
                }
                return result;
            }
        }

        private sealed class XflTimeline
        {
            public XflTimeline(XElement node)
            {
                if (node == null) throw new InvalidDataException("The XFL document has no timeline.");
                Node = node;
                Layers = Children(Child(node, "layers"), "DOMLayer").Select(layer => new XflLayer(layer)).ToList();
            }

            public XElement Node { get; }
            public List<XflLayer> Layers { get; }
            public int FrameCount => Layers.Count == 0 ? 0 : Layers.Max(layer => layer.FrameCount);
        }

        private sealed class XflLibraryItem
        {
            public XflLibraryItem(XElement node)
            {
                Node = node;
                Name = Attr(node, "name");
                string tag = node.Name.LocalName;
                ItemType = tag == "DOMSymbolItem" ? Attr(node, "symbolType", "movie clip")
                    : tag == "DOMBitmapItem" ? "bitmap"
                    : tag == "DOMSoundItem" ? "sound"
                    : tag == "DOMFolderItem" ? "folder"
                    : tag;
                XElement timeline = Child(Child(node, "timeline"), "DOMTimeline");
                Timeline = timeline == null ? null : new XflTimeline(timeline);
            }

            public XElement Node { get; }
            public string Name { get; }
            public string ItemType { get; }
            public XflTimeline Timeline { get; }
        }

        private sealed class XflProject
        {
            private readonly string _root;
            private readonly Dictionary<string, string> _archiveXml;
            private readonly Dictionary<string, XflLibraryItem> _libraryItems = new Dictionary<string, XflLibraryItem>(StringComparer.Ordinal);
            private readonly List<string> _includes = new List<string>();
            private readonly HashSet<string> _loadedIncludes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            private XflProject(string root, Dictionary<string, string> archiveXml, XDocument document)
            {
                _root = root;
                _archiveXml = archiveXml;
                XElement documentRoot = document.Root ?? throw new InvalidDataException("DOMDocument.xml is empty.");
                FrameRate = ParseDouble(Attr(documentRoot, "frameRate"), 0);
                XElement mainTimeline = Children(Child(documentRoot, "timelines"), "DOMTimeline").FirstOrDefault();
                MainTimeline = new XflTimeline(mainTimeline);

                foreach (XElement media in Child(documentRoot, "media")?.Elements() ?? Enumerable.Empty<XElement>())
                {
                    AddLibraryItem(new XflLibraryItem(media));
                }
                foreach (XElement include in Children(Child(documentRoot, "symbols"), "Include"))
                {
                    string href = Attr(include, "href");
                    if (!string.IsNullOrEmpty(href)) _includes.Add(href);
                }
            }

            public double FrameRate { get; }
            public XflTimeline MainTimeline { get; }

            public static XflProject Load(string inFile)
            {
                if (string.IsNullOrWhiteSpace(inFile)) throw new ArgumentException("An XFL path is required.", nameof(inFile));
                string source = Path.GetFullPath(inFile);
                if (IsZipXflFile(source))
                {
                    return LoadArchive(source);
                }

                string root = Directory.Exists(source) ? source : Path.GetDirectoryName(source);
                while (!string.IsNullOrEmpty(root) && !File.Exists(Path.Combine(root, "DOMDocument.xml")))
                {
                    DirectoryInfo parent = Directory.GetParent(root);
                    if (parent == null || string.Equals(parent.FullName, root, StringComparison.OrdinalIgnoreCase))
                    {
                        root = null;
                        break;
                    }
                    root = parent.FullName;
                }
                if (string.IsNullOrEmpty(root))
                {
                    throw new FileNotFoundException("Could not find DOMDocument.xml for the XFL project.", inFile);
                }
                return new XflProject(root, null, LoadDocument(Path.Combine(root, "DOMDocument.xml")));
            }

            public static XflProject LoadArchive(string inFile)
            {
                if (string.IsNullOrWhiteSpace(inFile)) throw new ArgumentException("A ZIP-XFL archive path is required.", nameof(inFile));
                string source = Path.GetFullPath(inFile);
                if (!File.Exists(source)) throw new FileNotFoundException("The ZIP-XFL archive was not found.", inFile);
                Dictionary<string, string> archive = ReadFlaXml(source);
                if (!archive.TryGetValue("domdocument.xml", out string xml))
                {
                    throw new InvalidDataException("The ZIP-XFL archive does not contain DOMDocument.xml.");
                }
                return new XflProject(Path.GetDirectoryName(source), archive, ParseDocument(xml));
            }

            public XflLibraryItem FindLibraryItem(string name)
            {
                if (string.IsNullOrEmpty(name)) return null;
                if (_libraryItems.TryGetValue(name, out XflLibraryItem item)) return item;
                foreach (string href in _includes)
                {
                    if (_loadedIncludes.Add(href))
                    {
                        XDocument document = LoadXml("LIBRARY/" + href);
                        if (document?.Root != null) AddLibraryItem(new XflLibraryItem(document.Root));
                    }
                    if (_libraryItems.TryGetValue(name, out item)) return item;
                }
                return null;
            }

            private void AddLibraryItem(XflLibraryItem item)
            {
                if (!string.IsNullOrEmpty(item.Name)) _libraryItems[item.Name] = item;
            }

            private XDocument LoadXml(string relativePath)
            {
                if (_archiveXml != null)
                {
                    return _archiveXml.TryGetValue(ArchiveKey(relativePath), out string xml) ? ParseDocument(xml) : null;
                }
                string file = Path.Combine(_root, relativePath.Replace('/', Path.DirectorySeparatorChar));
                return File.Exists(file) ? LoadDocument(file) : null;
            }

            private static Dictionary<string, string> ReadFlaXml(string path)
            {
                Dictionary<string, string> result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                ZipArchive archive;
                try
                {
                    archive = new ZipArchive(stream, ZipArchiveMode.Read, false);
                }
                catch (InvalidDataException exception)
                {
                    throw new InvalidDataException("The input is not a readable ZIP-XFL archive.", exception);
                }
                using (archive)
                {
                    ZipArchiveEntry documentEntry = archive.Entries
                        .Where(entry => !string.IsNullOrEmpty(entry.Name) && string.Equals(entry.Name, "DOMDocument.xml", StringComparison.OrdinalIgnoreCase))
                        .OrderBy(entry => entry.FullName.Length)
                        .FirstOrDefault();
                    if (documentEntry == null) return result;
                    string normalizedDocument = documentEntry.FullName.Replace('\\', '/');
                    int slash = normalizedDocument.LastIndexOf('/');
                    string prefix = slash < 0 ? string.Empty : normalizedDocument[..(slash + 1)];
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        string fullName = entry.FullName.Replace('\\', '/').TrimStart('/');
                        if (string.IsNullOrEmpty(entry.Name)
                            || !fullName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                            || !fullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                        string relative = fullName[prefix.Length..];
                        using StreamReader reader = new StreamReader(entry.Open(), detectEncodingFromByteOrderMarks: true);
                        result[ArchiveKey(relative)] = reader.ReadToEnd();
                    }
                }
                return result;
            }

            private static XDocument LoadDocument(string file)
            {
                using FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
                return XDocument.Load(stream, LoadOptions.None);
            }

            private static XDocument ParseDocument(string xml) => XDocument.Parse(xml, LoadOptions.None);

            private static string ArchiveKey(string path)
            {
                string key = path.Replace('\\', '/');
                while (key.StartsWith("./", StringComparison.Ordinal)) key = key[2..];
                return key.TrimStart('/').ToLowerInvariant();
            }
        }

        private static string NormalizeName(string value)
            => value.Replace("_", string.Empty).Replace("-", string.Empty).ToLowerInvariant();

        private static double Ease(string easingName, double progress)
        {
            double p = Clamp01(progress);
            string easing = NormalizeName(easingName);
            switch (easing)
            {
                case "stepped": return 0;
                case "quadraticin": return p * p;
                case "quadraticout": return -p * (p - 2);
                case "quadraticinout": { double q = p * 2; return q < 1 ? q * q / 2 : (-(q - 1) * (q - 3) + 1) / 2; }
                case "cubicin": return p * p * p;
                case "cubicout": { double q = p - 1; return q * q * q + 1; }
                case "cubicinout": { double q = p * 2; return q < 1 ? q * q * q / 2 : (Math.Pow(q - 2, 3) + 2) / 2; }
                case "quarticin": return Math.Pow(p, 4);
                case "quarticout": return -Math.Pow(p - 1, 4) + 1;
                case "quarticinout": { double q = p * 2; return q < 1 ? Math.Pow(q, 4) / 2 : (-Math.Pow(q - 2, 4) + 2) / 2; }
                case "quinticin": return Math.Pow(p, 5);
                case "quinticout": return Math.Pow(p - 1, 5) + 1;
                case "quinticinout": { double q = p * 2; return q < 1 ? Math.Pow(q, 5) / 2 : (Math.Pow(q - 2, 5) + 2) / 2; }
                case "sinusoidalin": return -Math.Cos(p * Pi / 2) + 1;
                case "sinusoidalout": return Math.Sin(p * Pi / 2);
                case "sinusoidalinout": return -Math.Cos(p * Pi) / 2 + 0.5;
                case "exponentialin": return p == 0 ? 0 : Math.Pow(2, 10 * (p - 1));
                case "exponentialout": return p == 1 ? 1 : -Math.Pow(2, -10 * p) + 1;
                case "exponentialinout": { if (p == 0 || p == 1) return p; double q = p * 2; return q < 1 ? Math.Pow(2, 10 * (q - 1)) / 2 : (-Math.Pow(2, -10 * (q - 1)) + 2) / 2; }
                case "circularin": return -(Math.Sqrt(Math.Max(0, 1 - p * p)) - 1);
                case "circularout": { double q = p - 1; return Math.Sqrt(Math.Max(0, 1 - q * q)); }
                case "circularinout": { double q = p * 2; return q < 1 ? -(Math.Sqrt(Math.Max(0, 1 - q * q)) - 1) / 2 : (Math.Sqrt(Math.Max(0, 1 - Math.Pow(q - 2, 2))) + 1) / 2; }
                case "backin": { const double s = 1.70158; return p * p * ((s + 1) * p - s); }
                case "backout": { const double s = 1.70158; double q = p - 1; return q * q * ((s + 1) * q + s) + 1; }
                case "backinout": { double q = p * 2; double s = 1.70158 * 1.525; if (q < 1) return q * q * ((s + 1) * q - s) / 2; q -= 2; return (q * q * ((s + 1) * q + s) + 2) / 2; }
                case "bounceout": return BounceOut(p);
                case "bouncein": return 1 - BounceOut(1 - p);
                case "bounceinout": return p < 0.5 ? (1 - BounceOut(1 - p * 2)) / 2 : (BounceOut(p * 2 - 1) + 1) / 2;
                case "elasticin":
                case "elasticout":
                case "elasticinout": return Elastic(easing, p);
                default: return p;
            }
        }

        private static double BounceOut(double value)
        {
            double p = value;
            if (p < 1 / 2.75) return 7.5625 * p * p;
            if (p < 2 / 2.75) { p -= 1.5 / 2.75; return 7.5625 * p * p + 0.75; }
            if (p < 2.5 / 2.75) { p -= 2.25 / 2.75; return 7.5625 * p * p + 0.9375; }
            p -= 2.625 / 2.75;
            return 7.5625 * p * p + 0.984375;
        }

        private static double Elastic(string easing, double p)
        {
            if (p <= 0.00001 || p >= 0.999) return p;
            double period = easing == "elasticinout" ? 0.45 : 0.3;
            double offset = period / 4;
            if (easing == "elasticin")
            {
                double q = p - 1;
                return -(Math.Pow(2, 10 * q) * Math.Sin((q - offset) * 2 * Pi / period));
            }
            if (easing == "elasticout")
            {
                return Math.Pow(2, -10 * p) * Math.Sin((p - offset) * 2 * Pi / period) + 1;
            }
            double value = p * 2;
            if (value < 1)
            {
                value -= 1;
                return -0.5 * Math.Pow(2, 10 * value) * Math.Sin((value - offset) * 2 * Pi / period);
            }
            value -= 1;
            return 0.5 * Math.Pow(2, -10 * value) * Math.Sin((value - offset) * 2 * Pi / period) + 1;
        }
    }
}
