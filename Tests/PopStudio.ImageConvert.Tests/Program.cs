using System.Reflection;
using System.Runtime.InteropServices;
using PopStudio.Image.Convert;
using PopStudio.Platform;

// dotnet run --project Tests/PopStudio.ImageConvert.Tests
internal static class Program
{
    private static readonly Assembly Production = Assembly.Load("PopStudio");
    private static readonly Type Api = Production.GetType("PopStudio.Platform.YFAPI", true)!;
    private static readonly string Output = Path.Combine(AppContext.BaseDirectory, "cases");
    private static int passed;

    private static void Main()
    {
        if (Directory.Exists(Output)) Directory.Delete(Output, true);
        Directory.CreateDirectory(Output);
        object platform = Activator.CreateInstance(Production.GetType("PopStudio.Platform.ConsoleAPI", true)!, true)!;
        Api.GetMethod("RegistPlatform", new[] { typeof(object) })!.Invoke(null, new[] { platform });
        string bitmapType = OperatingSystem.IsWindows() ? "GDIBitmap" : "ImageBitmap";
        YFBitmap.RegistPlatform(Activator.CreateInstance(Production.GetType("PopStudio.Platform." + bitmapType, true)!, true)!);

        Run("catalog matches the encoder switch", () =>
        {
            int[] formats = { 25, 0, 4, 4, 14, 0, 4, 0, 0 };
            Check(ImageConvertCatalog.Containers.Count == formats.Length, "container count");
            for (int i = 0; i < formats.Length; i++)
                Check(ImageConvertCatalog.Containers[i].PixelFormats.Count == formats[i], $"container {i} pixel format count");
            Check(ImageConvertCatalog.GetContainer(6).GetOutputExtension(0) == ".ptx", "PS3 default extension");
            Check(ImageConvertCatalog.GetContainer(6).GetOutputExtension(2) == ".dds", "PS3 ATC extension");
            Check(ImageConvertCatalog.GetMaxDimension(2) == 65535, "ushort sized containers are limited");
            Check(ImageConvertCatalog.GetMaxDimension(0) == int.MaxValue, "int sized containers are not");
            Check(ImageConvertCatalog.FindContainers(".tex").Count == 2, "tex belongs to two containers");
            Check(ImageConvertCatalog.FindContainers(".ptx").Count == 4, "ptx belongs to four containers");
            Check(ImageConvertCatalog.IsBitmapInput("a.JPEG") && !ImageConvertCatalog.IsBitmapInput("a.tga"), "bitmap input by extension");
        });
        Run("bitmap to bitmap writes PNG, JPG and BMP", () =>
        {
            string dir = NewFolder("bitmap");
            string png = SaveImage(Path.Combine(dir, "source.png"), 64, 64,
                (x, y) => 0xff000000u | ((uint)(255 - x * 4) << 16) | ((uint)(y * 4) << 8) | 0x40u);
            uint[] source = ReadPixels(png);
            string outDir = NewFolder("bitmap-out");
            var options = new ImageConvertOptions { Mode = ImageConvertMode.BitmapToBitmap, OutputDirectory = outDir };
            Check(ImageBatchConverter.Convert(new[] { png }, options).Succeeded == 1, "png copy");
            Check(ReadPixels(Path.Combine(outDir, "source.png")).SequenceEqual(source), "png round-trip");

            options.BitmapFormat = YFBitmapFormat.Bmp;
            Check(ImageBatchConverter.Convert(new[] { png }, options).Succeeded == 1, "bmp encode");
            string bmp = Path.Combine(outDir, "source.bmp");
            byte[] head = File.ReadAllBytes(bmp);
            Check(head[0] == 'B' && head[1] == 'M', "BMP magic");
            Check(BitConverter.ToInt32(head, 18) == 64 && BitConverter.ToInt32(head, 22) == 64, "BMP dimensions");
            Check(BitConverter.ToInt16(head, 28) == 32, "BMP bit depth");
            Check(ReadPixels(bmp).SequenceEqual(source), "bmp round-trip");

            options.BitmapFormat = YFBitmapFormat.Jpeg;
            options.JpegQuality = 90;
            Check(ImageBatchConverter.Convert(new[] { png }, options).Succeeded == 1, "jpeg high quality");
            long high = new FileInfo(Path.Combine(outDir, "source.jpg")).Length;
            options.OutputDirectory = NewFolder("jpeg-low");
            options.JpegQuality = 20;
            Check(ImageBatchConverter.Convert(new[] { png }, options).Succeeded == 1, "jpeg low quality");
            long low = new FileInfo(Path.Combine(options.OutputDirectory, "source.jpg")).Length;
            Check(low < high, $"jpeg quality changes the file size ({low} < {high})");
        });
        Run("bitmap to texture goes through the requested pixel format", () =>
        {
            string dir = NewFolder("texture");
            string png = SaveImage(Path.Combine(dir, "tile.png"), 4, 4,
                (x, y) => 0xff000000u | ((uint)(x * 60) << 16) | ((uint)(y * 60) << 8) | 0x20u);
            uint[] source = ReadPixels(png);
            string outDir = NewFolder("texture-out");
            var options = new ImageConvertOptions { Mode = ImageConvertMode.BitmapToTexture, OutputDirectory = outDir, Container = 0, PixelFormat = 0 };
            Check(ImageBatchConverter.Convert(new[] { png }, options).Succeeded == 1, "ptx encode");
            string ptx = Path.Combine(outDir, "tile.ptx");
            Check(File.Exists(ptx), "ptx output name");
            string direct = Path.Combine(dir, "direct.ptx");
            Invoke("EncodeImage", png, direct, 0, 0);
            Check(File.ReadAllBytes(direct).SequenceEqual(File.ReadAllBytes(ptx)), "engine matches the direct encoder");
            string back = Path.Combine(outDir, "tile.back.png");
            Invoke("DecodeImage", ptx, back, 0);
            Check(ReadPixels(back).SequenceEqual(source), "ARGB8888 round-trip");
        });
        Run("batch conversion mirrors subdirectories and can flatten them", () =>
        {
            string input = NewFolder("tree/in");
            Directory.CreateDirectory(Path.Combine(input, "sub"));
            SaveImage(Path.Combine(input, "a.png"), 4, 4, (x, y) => 0xff00ff00u);
            SaveImage(Path.Combine(input, "sub", "b.png"), 4, 4, (x, y) => 0xff00ffffu);
            var options = new ImageConvertOptions { Mode = ImageConvertMode.BitmapToBitmap, OutputDirectory = NewFolder("tree/mirror") };
            var result = ImageBatchConverter.Convert(new[] { input }, options, input);
            Check(result.Succeeded == 2, "two files converted");
            Check(File.Exists(Path.Combine(options.OutputDirectory, "a.png")), "top level file");
            Check(File.Exists(Path.Combine(options.OutputDirectory, "sub", "b.png")), "nested file");

            options.OutputDirectory = NewFolder("tree/flat");
            options.Layout = ImageOutputLayout.Flat;
            Check(ImageBatchConverter.Convert(new[] { input }, options, input).Succeeded == 2, "flat layout");
            Check(File.Exists(Path.Combine(options.OutputDirectory, "b.png")), "flat file");

            options.OutputDirectory = NewFolder("tree/top");
            options.IncludeSubdirectories = false;
            var shallow = ImageBatchConverter.Convert(new[] { input }, options, input);
            Check(shallow.Total == 1 && shallow.Succeeded == 1, "recursion off skips subdirectories");
        });
        Run("skip leaves existing files alone and overwrite replaces them", () =>
        {
            string input = NewFolder("conflict/in");
            SaveImage(Path.Combine(input, "a.png"), 4, 4, (x, y) => 0xff00ff00u);
            var options = new ImageConvertOptions { Mode = ImageConvertMode.BitmapToBitmap, OutputDirectory = NewFolder("conflict/out") };
            Check(ImageBatchConverter.Convert(new[] { input }, options, input).Succeeded == 1, "first pass");

            string target = Path.Combine(options.OutputDirectory, "a.png");
            byte[] damaged = { 1, 2, 3 };
            File.WriteAllBytes(target, damaged);
            Check(ImageBatchConverter.Convert(new[] { input }, options, input).Skipped == 1, "second pass skips");
            Check(File.ReadAllBytes(target).SequenceEqual(damaged), "skipped file is untouched");

            options.Conflict = ImageConflictPolicy.Overwrite;
            Check(ImageBatchConverter.Convert(new[] { input }, options, input).Succeeded == 1, "overwrite pass");
            Check(File.ReadAllBytes(target).Length > damaged.Length, "overwritten with a real file");
        });
        Run("a broken input fails alone and leaves no temporary files", () =>
        {
            string input = NewFolder("broken/in");
            SaveImage(Path.Combine(input, "good.png"), 4, 4, (x, y) => 0xffff0000u);
            File.WriteAllBytes(Path.Combine(input, "bad.png"), new byte[] { 0x89, 0x50, 0x4e, 0x47, 0, 0 });
            var options = new ImageConvertOptions { Mode = ImageConvertMode.BitmapToBitmap, OutputDirectory = NewFolder("broken/out") };
            var result = ImageBatchConverter.Convert(new[] { input }, options, input);
            Check(result.Total == 2 && result.Succeeded == 1 && result.Failed == 1, "one failure, one success");
            Check(result.Failures.Count == 1 && result.Failures[0].InputPath.EndsWith("bad.png"), "failure names the file");
            Check(!File.Exists(Path.Combine(options.OutputDirectory, "bad.png")), "no output for the broken file");
            Check(Directory.GetFiles(options.OutputDirectory, "*.tmp*", SearchOption.AllDirectories).Length == 0, "no temporary files left");
        });
        Run("max edge shrinks proportionally and never enlarges", () =>
        {
            string input = NewFolder("scale/in");
            SaveImage(Path.Combine(input, "wide.png"), 8, 4, (x, y) => 0xff0000ffu);
            var options = new ImageConvertOptions { Mode = ImageConvertMode.BitmapToBitmap, OutputDirectory = NewFolder("scale/out"), MaxEdge = 4 };
            Check(ImageBatchConverter.Convert(new[] { input }, options, input).Succeeded == 1, "downscale");
            using (YFBitmap scaled = YFBitmap.Create(Path.Combine(options.OutputDirectory, "wide.png")))
                Check(scaled.Width == 4 && scaled.Height == 2, $"downscaled to {scaled.Width}x{scaled.Height}");

            options.MaxEdge = 100;
            options.Conflict = ImageConflictPolicy.Overwrite;
            var second = ImageBatchConverter.Convert(new[] { input }, options, input);
            if (second.Succeeded != 1)
            {
                Console.WriteLine($"  [diag] total={second.Total} ok={second.Succeeded} skip={second.Skipped} fail={second.Failed}");
                foreach (var f in second.Failures) Console.WriteLine($"  [diag] {f.InputPath} -> {f.OutputPath} : {f.Message}");
            }
            Check(second.Succeeded == 1, "no upscale");
            using (YFBitmap kept = YFBitmap.Create(Path.Combine(options.OutputDirectory, "wide.png")))
                Check(kept.Width == 8 && kept.Height == 4, "kept the original size");
        });
        Run("default output folder sits next to the input", () =>
        {
            string input = NewFolder("default/in");
            string expected = Path.Combine(Path.GetDirectoryName(input)!, "in_converted");
            Check(ImageBatchConverter.GetDefaultOutputDirectory(input) == expected, "sibling folder");
            Check(ImageBatchConverter.GetDefaultOutputDirectory(null!) == null, "empty input has no default");
        });

        Console.WriteLine($"Passed {passed} image convert test groups." + (failed > 0 ? $" Failed {failed}." : string.Empty));
    }

    private static void Invoke(string method, params object[] args)
    {
        try { Api.GetMethod(method)!.Invoke(null, args); }
        catch (TargetInvocationException e) when (e.InnerException != null) { throw e.InnerException; }
    }

    private static string NewFolder(string name)
    {
        string path = Path.Combine(Output, name);
        Directory.CreateDirectory(path);
        return path;
    }

    private static string SaveImage(string path, int w, int h, Func<int, int, uint> pixel)
    {
        using YFBitmap image = YFBitmap.Create(w, h);
        int[] pixels = new int[w * h];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                pixels[y * w + x] = unchecked((int)pixel(x, y));
        Marshal.Copy(pixels, 0, image.GetPixels(), pixels.Length);
        image.Save(path);
        return path;
    }

    private static uint[] ReadPixels(string path)
    {
        using YFBitmap image = YFBitmap.Create(path);
        int[] pixels = new int[image.Width * image.Height];
        Marshal.Copy(image.GetPixels(), pixels, 0, pixels.Length);
        return pixels.Select(p => unchecked((uint)p)).ToArray();
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    private static int failed;

    private static void Run(string name, Action test)
    {
        try
        {
            test();
            passed++;
            Console.WriteLine("PASS " + name);
        }
        catch (Exception ex)
        {
            failed++;
            Console.WriteLine("FAIL " + name);
            Console.WriteLine("     " + ex.Message);
        }
    }
}
