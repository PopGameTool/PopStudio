using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using PopStudio.Platform;

// dotnet run --project Tests/PopStudio.Texture.Tests -- [optional sample.dds] [output directory]
internal static class Program
{
    private static readonly Assembly Production = Assembly.Load("PopStudio");
    private static readonly Type Api = Production.GetType("PopStudio.Platform.YFAPI", true)!;
    private static readonly string Output = Path.Combine(AppContext.BaseDirectory, "cases");
    private static int passed;

    private static void Main(string[] args)
    {
        Directory.CreateDirectory(Output);
        object platform = Activator.CreateInstance(Production.GetType("PopStudio.Platform.ConsoleAPI", true)!, true)!;
        Api.GetMethod("RegistPlatform", new[] { typeof(object) })!.Invoke(null, new[] { platform });
        string bitmapType = OperatingSystem.IsWindows() ? "GDIBitmap" : "ImageBitmap";
        YFBitmap.RegistPlatform(Activator.CreateInstance(Production.GetType("PopStudio.Platform." + bitmapType, true)!, true)!);

        Run("ATC mode 0: endpoints, interpolation and index order", () =>
        {
            var pixels = Decode(Fixture("mode0", "ATC ", ColorBlock(0x7c00, 0x001f)));
            uint[] expected = { 0xffff0000, 0xff9f005f, 0xff5f009f, 0xff0000ff };
            for (int i = 0; i < 16; i++) Check(pixels[i] == expected[i % 4], $"mode 0 pixel {i}");
        });
        Run("ATC mode 1 clamps subtraction instead of taking absolute values", () =>
        {
            var pixels = Decode(Fixture("mode1", "ATC ", ColorBlock(0x8400, 0xffff)));
            uint[] expected = { 0xff000000, 0xff000000, 0xff080000, 0xffffffff };
            for (int i = 0; i < 16; i++) Check(pixels[i] == expected[i % 4], $"mode 1 pixel {i}");
        });
        Run("ATCA expands all 16 explicit alpha values", () =>
        {
            byte[] block = BitConverter.GetBytes(0xfedcba9876543210UL).Concat(ColorBlock(0x03e0, 0x07e0)).ToArray();
            var pixels = Decode(Fixture("explicit", "ATCA", block));
            for (int i = 0; i < 16; i++) Check(pixels[i] >> 24 == i * 17, $"alpha {i}");
        });
        Run("ATCI supports both alpha ramps and indices crossing byte boundaries", () =>
        {
            foreach (bool descending in new[] { true, false })
            {
                byte a = descending ? (byte)255 : (byte)0, b = descending ? (byte)0 : (byte)255;
                ulong alpha = a | ((ulong)b << 8);
                for (int i = 0; i < 16; i++) alpha |= (ulong)(i % 8) << (16 + i * 3);
                byte[] block = BitConverter.GetBytes(alpha).Concat(ColorBlock(0x03e0, 0x07e0)).ToArray();
                var pixels = Decode(Fixture("interpolated-" + descending, "ATCI", block));
                int[] expected = descending ? new[] { 255, 0, 219, 182, 146, 109, 73, 36 }
                    : new[] { 0, 255, 51, 102, 153, 204, 0, 255 };
                for (int i = 0; i < 16; i++) Check(pixels[i] >> 24 == expected[i % 8], $"alpha ramp {i}");
            }
        });
        Run("ATC encoder chooses the correct palette indices", () =>
        {
            uint[] colors = { 0xffff0000, 0xff9f005f, 0xff5f009f, 0xff0000ff };
            string png = SaveImage("palette", 4, 4, i => colors[i % 4]);
            foreach (int format in new[] { 1, 2, 3 })
            {
                string dds = Path.Combine(Output, $"palette-{format}.dds");
                Invoke("EncodeImage", png, dds, 6, format);
                var pixels = Decode(dds);
                for (int i = 0; i < 16; i++) Check(pixels[i] == colors[i % 4], $"encoded palette pixel {i}");
            }
        });
        Run("all DDS formats round-trip small and partial blocks", () =>
        {
            string[] tags = { "DXT5", "ATC ", "ATCA", "ATCI" };
            foreach (var (w, h) in new[] { (1, 1), (3, 5), (7, 9), (8, 4) })
            {
                string png = SaveImage($"size-{w}-{h}", w, h, i => 0xff00ff00);
                for (int format = 0; format < 4; format++)
                {
                    string dds = Path.Combine(Output, $"size-{w}-{h}-{format}.dds");
                    Invoke("EncodeImage", png, dds, 6, format);
                    byte[] data = File.ReadAllBytes(dds);
                    int size = ((w + 3) / 4) * ((h + 3) / 4) * (format == 1 ? 8 : 16);
                    Check(data.Length == 128 + size, "payload block size");
                    Check(BitConverter.ToInt32(data, 20) == size, "DDS linear size");
                    Check(Encoding.ASCII.GetString(data, 84, 4) == tags[format], "FourCC");
                    var pixels = Decode(dds);
                    Check(pixels.Length == w * h, "dimensions");
                    // The ATC encoder extends edge pixels; DXT5 retains its existing padding behavior.
                    if (format != 0) Check(pixels.All(p => p == 0xff00ff00), "partial block colors");
                }
            }
        });
        Run("ATCA and ATCI encode varying alpha; ATC RGB stays opaque", () =>
        {
            string png = SaveImage("alpha", 4, 4, i => ((uint)(i * 17) << 24) | 0x00ff00);
            foreach (int format in new[] { 1, 2, 3 })
            {
                string dds = Path.Combine(Output, $"alpha-{format}.dds");
                Invoke("EncodeImage", png, dds, 6, format);
                var pixels = Decode(dds);
                for (int i = 0; i < 16; i++)
                    Check(Math.Abs((int)(pixels[i] >> 24) - (format == 1 ? 255 : i * 17)) <= (format == 3 ? 18 : 0), "alpha fidelity");
            }
        });
        Run("DDS ignores writer metadata and decodes the largest mip", () =>
        {
            string file = Fixture("mips", "ATC ", ColorBlock(0x7c00, 0x001f).Concat(new byte[8]).ToArray());
            byte[] data = File.ReadAllBytes(file);
            BitConverter.GetBytes(2).CopyTo(data, 28);
            data[40] = 123;
            File.WriteAllBytes(file, data);
            Check(Decode(file)[0] == 0xffff0000, "base level");
        });
        Run("rejects truncated, unsupported and invalid DDS files before output", () =>
        {
            byte[] valid = File.ReadAllBytes(Fixture("valid", "ATCA", new byte[16]));
            foreach (int length in new[] { 0, 4, 80, 127, 128, 143 })
                Reject("truncated-" + length, valid[..length]);
            foreach (var (offset, value) in new[] { (4, 123), (12, 0), (16, -1), (16, int.MaxValue),
                (24, 2), (76, 31), (80, 0), (84, 0x30315844), (84, 0x31434241), (112, 0x200), (112, 0x200000) })
            {
                byte[] data = (byte[])valid.Clone();
                BitConverter.GetBytes(value).CopyTo(data, offset);
                Reject($"invalid-{offset}-{value}", data);
            }
        });
        Run("legacy PS3 DXT5 header and default format remain compatible", () =>
        {
            string input = SaveImage("legacy", 4, 4, _ => 0xffff0000);
            string output = Path.Combine(Output, "legacy.ptx");
            Type ptx = Production.GetType("PopStudio.Image.PtxPS3.Ptx", true)!;
            ptx.GetMethod("Encode")!.Invoke(null, new object[] { input, output, Type.Missing });
            byte[] data = File.ReadAllBytes(output);
            Check(Encoding.ASCII.GetString(data, 68, 4) == "NVTT", "legacy writer signature");
            Check(BitConverter.ToInt32(data, 8) == 528391, "legacy flags");
            Check(Decode(output).All(p => p == 0xffff0000), "legacy pixels");
        });
        Run("Lua batch scripts select ATCA and accept both DDS and PTX", () =>
        {
            string scriptRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../Tool/LuaScript"));
            foreach (bool interactive in new[] { false, true })
            {
                var lua = new MoonSharp.Interpreter.Script();
                lua.DoString(@"
                    calls = {}
                    args = {'C:/textures', 6, false, 2}
                    choices = {'dds / ptx(ps3)', 'ATC_RGBA4 (ATCA)'}
                    choice = 0
                    rainy = {
                        getversion = function() return 10 end,
                        getlanguage = function() return 1 end,
                        sheet = function() choice = choice + 1; return choices[choice] end,
                        getfiles = function() return {'a.png', 'b.dds', 'c.ptx'} end,
                        getfileextension = function(s) return s:sub(-4) end,
                        getfilenamewithoutextension = function(s) return s:sub(1, -5) end,
                        getfilepath = function() return 'C:/textures' end,
                        formatpath = function(s) return s end,
                        encodeimage = function(...) calls[#calls+1] = {...} end,
                        decodeimage = function(...) calls[#calls+1] = {...} end,
                        alert = function() return false end
                    }
                ");
                if (interactive) lua.DoString("args[2] = nil");
                lua.DoString(File.ReadAllText(Path.Combine(scriptRoot, "TextureBatchEncode.lua")));
                Check(lua.DoString("return #calls == 1 and calls[1][2]:sub(-4) == '.dds' and calls[1][3] == 6 and calls[1][4] == 2").Boolean, "Lua ATCA encoding");
                lua.DoString("calls = {}; choice = 0");
                lua.DoString(File.ReadAllText(Path.Combine(scriptRoot, "TextureBatchDecode.lua")));
                Check(lua.DoString("return #calls == 2 and calls[1][1] == 'b.dds' and calls[2][1] == 'c.ptx'").Boolean, "Lua DDS batch filtering");
            }
        });

        if (args.Length > 0)
        {
            string output = args.Length > 1 ? Path.GetFullPath(args[1]) : Output;
            Directory.CreateDirectory(output);
            string png = Path.Combine(output, Path.GetFileNameWithoutExtension(args[0]) + ".png");
            Invoke("DecodeImage", Path.GetFullPath(args[0]), png, 6);
            using var image = YFBitmap.Create(png);
            Check(image.Width == BitConverter.ToInt32(File.ReadAllBytes(args[0]), 16), "sample width");
            Check(image.Height == BitConverter.ToInt32(File.ReadAllBytes(args[0]), 12), "sample height");
            string encoded = Path.Combine(output, Path.GetFileNameWithoutExtension(args[0]) + ".roundtrip.dds");
            Invoke("EncodeImage", png, encoded, 6, 2);
            Invoke("DecodeImage", encoded, Path.ChangeExtension(encoded, ".png"), 6);
            Console.WriteLine($"Sample decoded and re-encoded: {png} ({image.Width} x {image.Height})");
        }
        Console.WriteLine($"Passed {passed} texture test groups.");
    }

    private static void Invoke(string method, params object[] args)
    {
        try { Api.GetMethod(method)!.Invoke(null, args); }
        catch (TargetInvocationException e) when (e.InnerException != null) { throw e.InnerException; }
    }

    private static string Fixture(string name, string tag, byte[] payload)
    {
        string path = Path.Combine(Output, name + ".dds");
        using var writer = new BinaryWriter(File.Create(path));
        writer.Write(Encoding.ASCII.GetBytes("DDS "));
        writer.Write(124); writer.Write(0xA1007); writer.Write(4); writer.Write(4);
        writer.Write(payload.Length); writer.Write(0); writer.Write(1);
        writer.Write(new byte[44]);
        writer.Write(32); writer.Write(5); writer.Write(Encoding.ASCII.GetBytes(tag));
        writer.Write(new byte[20]); writer.Write(0x401008); writer.Write(new byte[16]);
        writer.Write(payload);
        return path;
    }

    private static byte[] ColorBlock(ushort a, ushort b) =>
        BitConverter.GetBytes(a).Concat(BitConverter.GetBytes(b)).Concat(BitConverter.GetBytes(0xe4e4e4e4U)).ToArray();

    private static string SaveImage(string name, int w, int h, Func<int, uint> pixel)
    {
        string path = Path.Combine(Output, name + ".png");
        using var image = YFBitmap.Create(w, h);
        int[] pixels = Enumerable.Range(0, w * h).Select(i => unchecked((int)pixel(i))).ToArray();
        Marshal.Copy(pixels, 0, image.GetPixels(), pixels.Length);
        image.Save(path);
        return path;
    }

    private static uint[] Decode(string path)
    {
        string png = path + ".png";
        Invoke("DecodeImage", path, png, 6);
        using var image = YFBitmap.Create(png);
        int[] pixels = new int[image.Width * image.Height];
        Marshal.Copy(image.GetPixels(), pixels, 0, pixels.Length);
        return pixels.Select(p => unchecked((uint)p)).ToArray();
    }

    private static void Reject(string name, byte[] data)
    {
        string input = Path.Combine(Output, name + ".dds"), output = input + ".png";
        File.WriteAllBytes(input, data);
        try { Invoke("DecodeImage", input, output, 6); }
        catch (Exception) { Check(!File.Exists(output), "invalid DDS must not produce output"); return; }
        throw new Exception("Accepted invalid DDS: " + name);
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    private static void Run(string name, Action test)
    {
        test();
        passed++;
        Console.WriteLine("PASS " + name);
    }
}
