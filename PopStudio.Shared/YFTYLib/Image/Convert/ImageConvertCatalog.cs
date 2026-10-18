using PopStudio.Platform;

namespace PopStudio.Image.Convert
{
    /// <summary>
    /// 位图格式（相对纹理容器而言）。与 <see cref="YFBitmapFormat"/> 一一对应。
    /// </summary>
    public sealed class BitmapFormatInfo
    {
        public YFBitmapFormat Format { get; }
        public string DisplayName { get; }
        /// <summary>输出扩展名</summary>
        public string Extension { get; }
        /// <summary>输入识别用的扩展名，含别名（如 .jpeg）</summary>
        public string[] Extensions { get; }
        public bool SupportsQuality { get; }

        public BitmapFormatInfo(YFBitmapFormat format, string displayName, string extension, string[] extensions, bool supportsQuality)
        {
            Format = format;
            DisplayName = displayName;
            Extension = extension;
            Extensions = extensions;
            SupportsQuality = supportsQuality;
        }
    }

    /// <summary>
    /// 纹理容器下的像素格式。
    /// <see cref="Index"/> 是传给 YFAPI.EncodeImage 的 format2 下标；
    /// <see cref="DisplayName"/> 括号里的数字是容器内部实际存储的格式号，两者不是一回事。
    /// </summary>
    public sealed class ImagePixelFormatInfo
    {
        public int Index { get; }
        public string DisplayName { get; }

        public ImagePixelFormatInfo(int index, string displayName)
        {
            Index = index;
            DisplayName = displayName;
        }
    }

    public sealed class ImageContainerInfo
    {
        /// <summary>YFAPI.DecodeImage / EncodeImage 的 format 参数</summary>
        public int Index { get; }
        public string DisplayName { get; }
        public string DefaultExtension { get; }
        /// <summary>输入识别用的扩展名</summary>
        public string[] Extensions { get; }
        public IReadOnlyList<ImagePixelFormatInfo> PixelFormats { get; }
        /// <summary>该容器是否真的会用到 format2</summary>
        public bool SupportsPixelFormat => PixelFormats.Count > 0;

        public ImageContainerInfo(int index, string displayName, string defaultExtension, string[] extensions, params string[] pixelFormats)
        {
            Index = index;
            DisplayName = displayName;
            DefaultExtension = defaultExtension;
            Extensions = extensions;
            ImagePixelFormatInfo[] list = new ImagePixelFormatInfo[pixelFormats.Length];
            for (int i = 0; i < pixelFormats.Length; i++)
            {
                list[i] = new ImagePixelFormatInfo(i, pixelFormats[i]);
            }
            PixelFormats = list;
        }

        /// <summary>
        /// 输出扩展名。容器 6（dds / ptx(PS3)）随像素格式在 .ptx 与 .dds 之间切换，其余容器固定。
        /// </summary>
        public string GetOutputExtension(int pixelFormat) => (Index == 6 && pixelFormat != 0) ? ".dds" : DefaultExtension;
    }

    /// <summary>
    /// 图片格式目录：把「容器索引 ↔ 扩展名 ↔ 像素格式列表」集中到一处。
    /// 容器的取值与 YFAPI.InternalDecodeImage / InternalEncodeImage 的 switch 严格一致；
    /// 像素格式列表以纹理页的列表为准，下标即传给 format2 的值。
    /// </summary>
    public static class ImageConvertCatalog
    {
        public static readonly IReadOnlyList<ImageContainerInfo> Containers = new ImageContainerInfo[]
        {
            // 0
            new ImageContainerInfo(0, "PTX(rsb)", ".ptx", new[] { ".ptx" },
                "ARGB8888(0)",
                "ABGR8888(0)",
                "RGBA4444(1)",
                "RGB565(2)",
                "RGBA5551(3)",
                "RGBA4444_Block(21)",
                "RGB565_Block(22)",
                "RGBA5551_Block(23)",
                "ARGB8888_A8(149)",
                "ABGR8888_A8(149)",
                "ARGB8888(BE)(0)",
                "ARGB8888_Padding(BE)(0)",
                "DXT1_RGB(35)",
                "DXT3_RGBA(36)",
                "DXT5_RGBA(37)",
                "DXT5_RGBA_MortonBlock(5)",
                "DXT5_RGBA(BE)(5)",
                "ETC1_RGB(32)",
                "ETC1_RGB_A8(147)",
                "ETC1_RGB_A_Palette(147)",
                "PVRTC_4BPP_RGBA(30)",
                "PVRTC_4BPP_RGBA_A8(148)",
                "PVRTC_2BPP_RGBA(31)",
                "ATC_RGB(38)",
                "ATC_RGBA4(39)"),
            // 1：加密外壳，format2 被忽略，所以不给像素格式项
            new ImageContainerInfo(1, "cdat(Android,iOS)", ".cdat", new[] { ".cdat" }),
            // 2
            new ImageContainerInfo(2, "tex(iOS)", ".tex", new[] { ".tex" },
                "ABGR8888(1)",
                "RGBA4444(2)",
                "RGBA5551(3)",
                "RGB565(4)"),
            // 3
            new ImageContainerInfo(3, "txz(Android,iOS)", ".txz", new[] { ".txz" },
                "ABGR8888(1)",
                "RGBA4444(2)",
                "RGBA5551(3)",
                "RGB565(4)"),
            // 4：下标 0..6 带 zlib，7..13 是同一批像素格式但不压缩
            new ImageContainerInfo(4, "tex(TV)", ".tex", new[] { ".tex" },
                "ARGB8888(2)",
                "ARGB4444(3)",
                "ARGB1555(4)",
                "RGB565(5)",
                "ABGR8888(6)",
                "RGBA4444(7)",
                "RGBA5551(8)",
                "ARGB8888(NoZlib)(2)",
                "ARGB4444(NoZlib)(3)",
                "ARGB1555(NoZlib)(4)",
                "RGB565(NoZlib)(5)",
                "ABGR8888(NoZlib)(6)",
                "RGBA4444(NoZlib)(7)",
                "RGBA5551(NoZlib)(8)"),
            // 5
            new ImageContainerInfo(5, "ptx(Xbox360)", ".ptx", new[] { ".ptx" }),
            // 6：像素格式 0 输出 .ptx，1..3 输出 .dds
            new ImageContainerInfo(6, "dds / ptx(PS3)", ".ptx", new[] { ".ptx", ".dds" },
                "DXT5_RGBA",
                "ATC_RGB (ATC)",
                "ATC_RGBA4 (ATCA)",
                "ATC_RGBA_Interpolated (ATCI)"),
            // 7
            new ImageContainerInfo(7, "ptx(PSV)", ".ptx", new[] { ".ptx" }),
            // 8
            new ImageContainerInfo(8, "xnb(Windows Phone)", ".xnb", new[] { ".xnb" }),
        };

        public static readonly IReadOnlyList<BitmapFormatInfo> BitmapFormats = new BitmapFormatInfo[]
        {
            new BitmapFormatInfo(YFBitmapFormat.Png, "PNG", ".png", new[] { ".png" }, false),
            new BitmapFormatInfo(YFBitmapFormat.Jpeg, "JPG", ".jpg", new[] { ".jpg", ".jpeg" }, true),
            new BitmapFormatInfo(YFBitmapFormat.Bmp, "BMP", ".bmp", new[] { ".bmp" }, false),
        };

        public static ImageContainerInfo GetContainer(int index)
        {
            if (index < 0 || index >= Containers.Count) return null;
            return Containers[index];
        }

        public static BitmapFormatInfo GetBitmapFormat(YFBitmapFormat format)
        {
            for (int i = 0; i < BitmapFormats.Count; i++)
            {
                if (BitmapFormats[i].Format == format) return BitmapFormats[i];
            }
            return null;
        }

        public static BitmapFormatInfo GetBitmapFormat(string extension)
        {
            for (int i = 0; i < BitmapFormats.Count; i++)
            {
                string[] exts = BitmapFormats[i].Extensions;
                for (int j = 0; j < exts.Length; j++)
                {
                    if (string.Equals(exts[j], extension, StringComparison.OrdinalIgnoreCase)) return BitmapFormats[i];
                }
            }
            return null;
        }

        public static bool IsValidContainer(int index) => index >= 0 && index < Containers.Count;

        public static bool IsValidPixelFormat(int container, int pixelFormat)
        {
            ImageContainerInfo info = GetContainer(container);
            if (info == null) return false;
            // 容器不接收 format2 时，编码器会忽略它，这里一律放行 0
            if (!info.SupportsPixelFormat) return pixelFormat == 0;
            return pixelFormat >= 0 && pixelFormat < info.PixelFormats.Count;
        }

        /// <summary>位图扩展名判断（.png / .jpg / .jpeg / .bmp）</summary>
        public static bool IsBitmapInput(string filePath) => GetBitmapFormat(GetExtension(filePath)) != null;

        public static bool TryGuessBitmapFormat(string filePath, out YFBitmapFormat format)
        {
            BitmapFormatInfo info = GetBitmapFormat(GetExtension(filePath));
            format = info == null ? YFBitmapFormat.Png : info.Format;
            return info != null;
        }

        /// <summary>按容器声明的扩展名判断某个文件是不是该容器的输入</summary>
        public static bool IsTextureInput(string filePath, int container)
        {
            ImageContainerInfo info = GetContainer(container);
            if (info == null) return false;
            string ext = GetExtension(filePath);
            for (int i = 0; i < info.Extensions.Length; i++)
            {
                if (string.Equals(info.Extensions[i], ext, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        /// <summary>
        /// 按扩展名找出所有可能的容器。.tex 同属 tex(iOS) 与 tex(TV)、.ptx 同属 0/5/6/7，
        /// 所以「纹理转位图」必须让用户显式指定源容器，不能只靠扩展名推断。
        /// </summary>
        public static List<ImageContainerInfo> FindContainers(string extension)
        {
            List<ImageContainerInfo> ans = new List<ImageContainerInfo>();
            for (int i = 0; i < Containers.Count; i++)
            {
                string[] exts = Containers[i].Extensions;
                for (int j = 0; j < exts.Length; j++)
                {
                    if (string.Equals(exts[j], extension, StringComparison.OrdinalIgnoreCase))
                    {
                        ans.Add(Containers[i]);
                        break;
                    }
                }
            }
            return ans;
        }

        /// <summary>
        /// 容器头部能表示的边长上限。tex(iOS)、txz、ptx(PSV) 的 Head 用 ushort 存 width/height，
        /// 超过 65535 会被静默截断，其余容器用 int。
        /// </summary>
        public static int GetMaxDimension(int container) => (container == 2 || container == 3 || container == 7) ? 65535 : int.MaxValue;

        /// <summary>取扩展名（含点号，可能为空字符串）</summary>
        public static string GetExtension(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return string.Empty;
            return Path.GetExtension(filePath) ?? string.Empty;
        }
    }
}
