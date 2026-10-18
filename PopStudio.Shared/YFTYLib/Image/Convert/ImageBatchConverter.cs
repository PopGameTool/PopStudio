using PopStudio.Platform;
using System.Diagnostics;

namespace PopStudio.Image.Convert
{
    /// <summary>转换方向</summary>
    public enum ImageConvertMode
    {
        /// <summary>纹理容器 → 位图</summary>
        TextureToBitmap = 0,
        /// <summary>位图 → 纹理容器</summary>
        BitmapToTexture = 1,
        /// <summary>位图 → 位图</summary>
        BitmapToBitmap = 2
    }

    /// <summary>输出目录的组织方式</summary>
    public enum ImageOutputLayout
    {
        /// <summary>镜像输入目录的相对结构</summary>
        PreserveRelative = 0,
        /// <summary>全部平铺到输出目录</summary>
        Flat = 1
    }

    /// <summary>输出文件已存在时的处理方式</summary>
    public enum ImageConflictPolicy
    {
        /// <summary>覆盖</summary>
        Overwrite = 0,
        /// <summary>跳过并计数（默认）</summary>
        Skip = 1,
        /// <summary>改名成 xxx_1 另存</summary>
        Rename = 2
    }

    public sealed class ImageConvertOptions
    {
        public ImageConvertMode Mode { get; set; } = ImageConvertMode.TextureToBitmap;
        /// <summary>纹理侧容器索引，见 <see cref="ImageConvertCatalog.Containers"/></summary>
        public int Container { get; set; }
        /// <summary>像素格式下标（不是标签里括号中的数字）</summary>
        public int PixelFormat { get; set; }
        /// <summary>输出目录。留空则用 <see cref="ImageBatchConverter.GetDefaultOutputDirectory"/></summary>
        public string OutputDirectory { get; set; }
        public ImageOutputLayout Layout { get; set; } = ImageOutputLayout.PreserveRelative;
        public ImageConflictPolicy Conflict { get; set; } = ImageConflictPolicy.Skip;
        public bool IncludeSubdirectories { get; set; } = true;
        /// <summary>位图侧输出格式</summary>
        public YFBitmapFormat BitmapFormat { get; set; } = YFBitmapFormat.Png;
        /// <summary>JPEG 质量，1..100</summary>
        public int JpegQuality { get; set; } = 90;
        /// <summary>等比缩放的最大边长，0 表示不缩放；只缩小不放大</summary>
        public int MaxEdge { get; set; }
    }

    public sealed class ImageConvertProgress
    {
        /// <summary>已处理个数（含跳过与失败）</summary>
        public int Completed { get; }
        public int Total { get; }
        public string InputPath { get; }
        /// <summary>目标路径。收集输入阶段就失败时为 null</summary>
        public string OutputPath { get; }
        public bool Succeeded { get; }
        public bool Skipped { get; }

        internal ImageConvertProgress(int completed, int total, string inputPath, string outputPath, bool succeeded, bool skipped)
        {
            Completed = completed;
            Total = total;
            InputPath = inputPath;
            OutputPath = outputPath;
            Succeeded = succeeded;
            Skipped = skipped;
        }
    }

    public sealed class ImageConvertFailure
    {
        public string InputPath { get; }
        public string OutputPath { get; }
        public string Message { get; }

        public ImageConvertFailure(string inputPath, string outputPath, string message)
        {
            InputPath = inputPath;
            OutputPath = outputPath;
            Message = message;
        }
    }

    public sealed class ImageConvertResult
    {
        readonly List<ImageConvertFailure> m_failures = new List<ImageConvertFailure>();

        public int Total { get; internal set; }
        public int Succeeded { get; internal set; }
        public int Skipped { get; internal set; }
        /// <summary>失败个数，含收集输入阶段的失败（找不到的路径、读不了的目录）</summary>
        public int Failed { get; internal set; }
        public bool Cancelled { get; internal set; }
        public TimeSpan Elapsed { get; internal set; }
        public IReadOnlyList<ImageConvertFailure> Failures => m_failures;

        internal void AddFailure(ImageConvertFailure failure)
        {
            m_failures.Add(failure);
            Failed++;
        }

        /// <summary>
        /// 供命令行与脚本打印的纯诊断文本，不走本地化；
        /// 页面请用上面几个计数与 <see cref="Failures"/> 自行组织本地化文案。
        /// </summary>
        public string ToReportText()
        {
            List<string> lines = new List<string>
            {
                "total: " + Total,
                "success: " + Succeeded,
                "skipped: " + Skipped,
                "failed: " + Failed,
                "cancelled: " + Cancelled,
                "elapsed: " + Elapsed.TotalSeconds.ToString("F3") + "s"
            };
            for (int i = 0; i < m_failures.Count; i++)
            {
                ImageConvertFailure item = m_failures[i];
                lines.Add("  " + item.InputPath + " -> " + (item.OutputPath ?? "-") + " : " + item.Message);
            }
            return string.Join("\n", lines);
        }
    }

    /// <summary>
    /// 批量图片格式转换。单线程、可取消，同一时间只应该有一个任务在跑。
    /// 不看扩展名判断内容，扩展名只用来筛选输入与决定输出名；
    /// 输出先写同目录的临时文件，成功后再 Move 覆盖，失败不会动到已有文件。
    /// </summary>
    public static class ImageBatchConverter
    {
        static readonly StringComparer PathComparer = Path.DirectorySeparatorChar == '\\' ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

        /// <summary>
        /// 输出目录留空时的默认值：输入根目录的「同级目录、名字加 _converted」，
        /// 避免输出混进输入目录。
        /// </summary>
        public static string GetDefaultOutputDirectory(string inputDirectory)
        {
            if (string.IsNullOrWhiteSpace(inputDirectory)) return null;
            string trimmed = inputDirectory.Trim().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string name = Path.GetFileName(trimmed);
            if (string.IsNullOrEmpty(name)) return Path.Combine(inputDirectory.Trim(), "converted");
            string parent = Path.GetDirectoryName(trimmed);
            name += "_converted";
            return string.IsNullOrEmpty(parent) ? name : Path.Combine(parent, name);
        }

        /// <summary>
        /// 枚举目录下的输入文件。自己遍历而不用 <see cref="YFAPI.GetFiles"/>：
        /// 后者内部是固定长度 10000 的数组加静态字段，超过一万个文件会越界且不可重入。
        /// 无权限的子目录记进 <paramref name="result"/> 的失败清单，不中断整批。
        /// </summary>
        public static List<string> CollectFiles(string directory, bool includeSubdirectories, Func<string, bool> filter, ImageConvertResult result = null)
        {
            List<string> ans = new List<string>();
            if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory)) return ans;
            CollectDirectory(directory, includeSubdirectories, filter, 0, ans, result ?? new ImageConvertResult());
            ans.Sort(StringComparer.OrdinalIgnoreCase);
            return ans;
        }

        /// <summary>
        /// 批量转换。
        /// </summary>
        /// <param name="inputs">文件或目录路径</param>
        /// <param name="options">转换参数</param>
        /// <param name="inputRoot">输入根目录，用于「保留原目录结构」。为空时按输入自身推断</param>
        /// <param name="cancellationToken">取消标记。编码器内部不可取消，只在文件之间生效</param>
        /// <param name="onProgress">每处理完一个文件回调一次。回调抛异常会被吞掉，不影响转换</param>
        public static ImageConvertResult Convert(IEnumerable<string> inputs, ImageConvertOptions options, string inputRoot = null, CancellationToken cancellationToken = default, Action<ImageConvertProgress> onProgress = null)
        {
            if (inputs == null) throw new ArgumentNullException(nameof(inputs));
            if (options == null) throw new ArgumentNullException(nameof(options));
            ValidateOptions(options);

            Stopwatch watch = Stopwatch.StartNew();
            ImageConvertResult result = new ImageConvertResult();
            List<string> files = new List<string>();
            List<string> roots = new List<string>();

            // 先把文件列表整个快照下来再开转：输出目录可能就在输入目录内部，
            // 边转边扫会把自己刚写出来的文件也当成输入。
            CollectInputs(inputs, options, files, roots, result);
            // 收集阶段的失败（路径不存在、目录读不了）没有对应的文件，也算进总数，
            // 否则 Total 对不上 Succeeded + Skipped + Failed
            result.Total = files.Count + result.Failed;

            string relativeRoot = ResolveRelativeRoot(inputRoot, roots);
            string outputDirectory = ResolveOutputDirectory(options, relativeRoot);
            string extension = GetTargetExtension(options);

            for (int i = 0; i < files.Count; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    result.Cancelled = true;
                    break;
                }
                string input = files[i];
                string target = null;
                bool succeeded = false;
                bool skipped = false;
                try
                {
                    target = BuildOutputPath(input, outputDirectory, relativeRoot, extension, options.Layout);
                    if (options.Conflict == ImageConflictPolicy.Skip && File.Exists(target))
                    {
                        result.Skipped++;
                        skipped = true;
                    }
                    else
                    {
                        if (options.Conflict == ImageConflictPolicy.Rename) target = MakeUniquePath(target);
                        EnsureDirectory(target);
                        ConvertFile(input, target, options);
                        result.Succeeded++;
                        succeeded = true;
                    }
                }
                catch (Exception ex)
                {
                    result.AddFailure(new ImageConvertFailure(input, target, ex.Message));
                }
                if (onProgress != null)
                {
                    try
                    {
                        onProgress(new ImageConvertProgress(i + 1, files.Count, input, target, succeeded, skipped));
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            watch.Stop();
            result.Elapsed = watch.Elapsed;
            return result;
        }

        static void ValidateOptions(ImageConvertOptions options)
        {
            if (options.MaxEdge < 0) options.MaxEdge = 0;
            if (options.JpegQuality < 1) options.JpegQuality = 1;
            else if (options.JpegQuality > 100) options.JpegQuality = 100;
            if (options.Mode == ImageConvertMode.BitmapToTexture)
            {
                if (!ImageConvertCatalog.IsValidContainer(options.Container)) throw new Exception(Str.Obj.UnknownFormat);
                if (!ImageConvertCatalog.IsValidPixelFormat(options.Container, options.PixelFormat)) throw new Exception(Str.Obj.UnknownFormat);
                return;
            }
            if (options.Mode == ImageConvertMode.TextureToBitmap)
            {
                if (!ImageConvertCatalog.IsValidContainer(options.Container)) throw new Exception(Str.Obj.UnknownFormat);
            }
            if (ImageConvertCatalog.GetBitmapFormat(options.BitmapFormat) == null) throw new Exception(Str.Obj.UnknownFormat);
        }

        static Func<string, bool> GetInputFilter(ImageConvertOptions options)
        {
            int container = options.Container;
            if (options.Mode == ImageConvertMode.TextureToBitmap)
            {
                return path => ImageConvertCatalog.IsTextureInput(path, container);
            }
            return ImageConvertCatalog.IsBitmapInput;
        }

        static void CollectInputs(IEnumerable<string> inputs, ImageConvertOptions options, List<string> files, List<string> roots, ImageConvertResult result)
        {
            Func<string, bool> filter = GetInputFilter(options);
            HashSet<string> seen = new HashSet<string>(PathComparer);
            List<string> collected = new List<string>();
            foreach (string raw in inputs)
            {
                if (string.IsNullOrWhiteSpace(raw)) continue;
                string path = raw.Trim();
                if (Directory.Exists(path))
                {
                    collected.Clear();
                    CollectDirectory(path, options.IncludeSubdirectories, filter, 0, collected, result);
                    collected.Sort(StringComparer.OrdinalIgnoreCase);
                    roots.Add(path);
                    for (int i = 0; i < collected.Count; i++)
                    {
                        if (seen.Add(collected[i])) files.Add(collected[i]);
                    }
                }
                else if (File.Exists(path))
                {
                    // 显式给出的文件不看扩展名，用户既然选了就试着转
                    string parent = Path.GetDirectoryName(path);
                    if (!string.IsNullOrEmpty(parent)) roots.Add(parent);
                    if (seen.Add(path)) files.Add(path);
                }
                else
                {
                    result.AddFailure(new ImageConvertFailure(path, null, string.Format(Str.Obj.FileNotFound, path)));
                }
            }
        }

        static void CollectDirectory(string directory, bool recursive, Func<string, bool> filter, int depth, List<string> output, ImageConvertResult result)
        {
            if (depth > 64) return;
            string[] files;
            try
            {
                files = Directory.GetFiles(directory);
            }
            catch (Exception ex)
            {
                result.AddFailure(new ImageConvertFailure(directory, null, ex.Message));
                return;
            }
            for (int i = 0; i < files.Length; i++)
            {
                if (filter == null || filter(files[i])) output.Add(files[i]);
            }
            if (!recursive) return;
            string[] directories;
            try
            {
                directories = Directory.GetDirectories(directory);
            }
            catch (Exception ex)
            {
                result.AddFailure(new ImageConvertFailure(directory, null, ex.Message));
                return;
            }
            for (int i = 0; i < directories.Length; i++)
            {
                // 跳过符号链接/联接点，避免目录成环
                if (IsReparsePoint(directories[i])) continue;
                CollectDirectory(directories[i], true, filter, depth + 1, output, result);
            }
        }

        static bool IsReparsePoint(string directory)
        {
            try
            {
                return (File.GetAttributes(directory) & FileAttributes.ReparsePoint) != 0;
            }
            catch (Exception)
            {
                return true;
            }
        }

        /// <summary>
        /// 计算「保留原目录结构」的相对基准目录。给了 inputRoot 就用它，
        /// 否则取各个输入（目录本身、或文件的所在目录）的公共上级。
        /// </summary>
        static string ResolveRelativeRoot(string inputRoot, List<string> roots)
        {
            if (!string.IsNullOrWhiteSpace(inputRoot) && Directory.Exists(inputRoot)) return inputRoot.Trim();
            if (roots.Count == 0) return null;
            string ans = roots[0];
            for (int i = 1; i < roots.Count; i++)
            {
                while (!IsSameOrChildPath(ans, roots[i]))
                {
                    string parent = Path.GetDirectoryName(ans);
                    if (string.IsNullOrEmpty(parent) || parent == ans) return null;
                    ans = parent;
                }
            }
            return ans;
        }

        static bool IsSameOrChildPath(string parent, string child)
        {
            if (string.IsNullOrEmpty(parent)) return false;
            string relative;
            try
            {
                relative = Path.GetRelativePath(parent, child);
            }
            catch (Exception)
            {
                return false;
            }
            if (relative == ".") return true;
            if (Path.IsPathRooted(relative)) return false;
            if (relative == "..") return false;
            if (relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)) return false;
            if (relative.StartsWith("../", StringComparison.Ordinal)) return false;
            return true;
        }

        static string ResolveOutputDirectory(ImageConvertOptions options, string relativeRoot)
        {
            if (!string.IsNullOrWhiteSpace(options.OutputDirectory)) return options.OutputDirectory.Trim();
            string byRule = GetDefaultOutputDirectory(relativeRoot);
            return byRule ?? Path.Combine(Directory.GetCurrentDirectory(), "converted");
        }

        /// <summary>目标扩展名：位图输出看位图格式，纹理输出看容器（PS3 的 ATC 输出 .dds）</summary>
        static string GetTargetExtension(ImageConvertOptions options)
        {
            if (options.Mode == ImageConvertMode.BitmapToTexture)
            {
                return ImageConvertCatalog.GetContainer(options.Container).GetOutputExtension(options.PixelFormat);
            }
            return ImageConvertCatalog.GetBitmapFormat(options.BitmapFormat).Extension;
        }

        static string BuildOutputPath(string inputPath, string outputDirectory, string relativeRoot, string extension, ImageOutputLayout layout)
        {
            string relative = Path.GetFileName(inputPath);
            if (layout == ImageOutputLayout.PreserveRelative && !string.IsNullOrEmpty(relativeRoot))
            {
                string candidate = null;
                try
                {
                    candidate = Path.GetRelativePath(relativeRoot, inputPath);
                }
                catch (Exception)
                {
                }
                // 跨盘或多选到不同根时 GetRelativePath 会给出绝对路径或 .. 开头的结果，退化成平铺
                if (!string.IsNullOrEmpty(candidate) && candidate != "." && !Path.IsPathRooted(candidate)
                    && candidate != ".."
                    && !candidate.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                    && !candidate.StartsWith("../", StringComparison.Ordinal))
                {
                    relative = candidate;
                }
            }
            return ReplaceExtension(Path.Combine(outputDirectory, relative), extension);
        }

        static string ReplaceExtension(string filePath, string extension)
        {
            string name = Path.GetFileNameWithoutExtension(filePath);
            if (string.IsNullOrEmpty(name)) name = Path.GetFileName(filePath);
            string directory = Path.GetDirectoryName(filePath);
            string fileName = name + extension;
            return string.IsNullOrEmpty(directory) ? fileName : Path.Combine(directory, fileName);
        }

        static string MakeUniquePath(string filePath)
        {
            if (!File.Exists(filePath)) return filePath;
            string directory = Path.GetDirectoryName(filePath);
            string name = Path.GetFileNameWithoutExtension(filePath);
            string extension = Path.GetExtension(filePath);
            for (int i = 1; i < 100000; i++)
            {
                string candidate = Path.Combine(directory ?? string.Empty, name + "_" + i + extension);
                if (!File.Exists(candidate)) return candidate;
            }
            return Path.Combine(directory ?? string.Empty, name + "_" + Guid.NewGuid().ToString("N") + extension);
        }

        static void EnsureDirectory(string filePath)
        {
            string directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) Directory.CreateDirectory(directory);
        }

        static void ConvertFile(string inputPath, string outputPath, ImageConvertOptions options)
        {
            // 先写临时文件再改名。直接写最终路径的话，转换失败时清理会把用户原有的文件删掉。
            // 临时文件名不带目标扩展名，免得被下一轮的输入筛选当成输入。
            string temp = outputPath + ".tmp" + Guid.NewGuid().ToString("N");
            try
            {
                switch (options.Mode)
                {
                    case ImageConvertMode.TextureToBitmap: TextureToBitmap(inputPath, temp, options); break;
                    case ImageConvertMode.BitmapToTexture: BitmapToTexture(inputPath, temp, options); break;
                    default: BitmapToBitmap(inputPath, temp, options); break;
                }
                File.Move(temp, outputPath, true);
            }
            catch (Exception)
            {
                TryDelete(temp);
                throw;
            }
        }

        static void TextureToBitmap(string inputPath, string tempPath, ImageConvertOptions options)
        {
            if (options.BitmapFormat == YFBitmapFormat.Png && options.MaxEdge <= 0)
            {
                // 目标就是 PNG 且不缩放，直接解码，省一次解码再编码
                YFAPI.DecodeImage(inputPath, tempPath, options.Container);
                return;
            }
            string middle = tempPath + ".mid";
            try
            {
                YFAPI.DecodeImage(inputPath, middle, options.Container);
                using (YFBitmap bitmap = OpenBitmap(middle))
                {
                    SaveBitmap(bitmap, tempPath, options);
                }
            }
            finally
            {
                TryDelete(middle);
            }
        }

        static void BitmapToTexture(string inputPath, string tempPath, ImageConvertOptions options)
        {
            if (options.MaxEdge > 0)
            {
                using (YFBitmap bitmap = OpenBitmap(inputPath))
                {
                    FitMaxEdge(bitmap.Width, bitmap.Height, options.MaxEdge, out int width, out int height);
                    if (width != bitmap.Width || height != bitmap.Height)
                    {
                        CheckTextureSize(options.Container, width, height);
                        string middle = tempPath + ".mid";
                        try
                        {
                            ResizeAndSave(bitmap, width, height, middle, YFBitmapSaveOptions.Png);
                            YFAPI.EncodeImage(middle, tempPath, options.Container, options.PixelFormat);
                        }
                        finally
                        {
                            TryDelete(middle);
                        }
                        return;
                    }
                    CheckTextureSize(options.Container, bitmap.Width, bitmap.Height);
                }
            }
            else if (ImageConvertCatalog.GetMaxDimension(options.Container) != int.MaxValue)
            {
                // tex(iOS) / txz / ptx(PSV) 的头部用 ushort 存边长，超了会被静默截断
                using (YFBitmap probe = OpenBitmap(inputPath))
                {
                    CheckTextureSize(options.Container, probe.Width, probe.Height);
                }
            }
            YFAPI.EncodeImage(inputPath, tempPath, options.Container, options.PixelFormat);
        }

        static void BitmapToBitmap(string inputPath, string tempPath, ImageConvertOptions options)
        {
            using (YFBitmap bitmap = OpenBitmap(inputPath))
            {
                SaveBitmap(bitmap, tempPath, options);
            }
        }

        static void SaveBitmap(YFBitmap bitmap, string filePath, ImageConvertOptions options)
        {
            FitMaxEdge(bitmap.Width, bitmap.Height, options.MaxEdge, out int width, out int height);
            ResizeAndSave(bitmap, width, height, filePath, new YFBitmapSaveOptions(options.BitmapFormat, options.JpegQuality));
        }

        static void ResizeAndSave(YFBitmap bitmap, int width, int height, string filePath, YFBitmapSaveOptions saveOptions)
        {
            YFBitmap scaled = null;
            try
            {
                YFBitmap target = bitmap;
                if (width != bitmap.Width || height != bitmap.Height)
                {
                    scaled = bitmap.Resize(width, height);
                    target = scaled;
                }
                target.Save(filePath, saveOptions);
            }
            finally
            {
                scaled?.Dispose();
            }
        }

        /// <summary>等比缩放到最大边长，只缩小不放大</summary>
        static void FitMaxEdge(int width, int height, int maxEdge, out int outWidth, out int outHeight)
        {
            if (maxEdge <= 0 || (width <= maxEdge && height <= maxEdge))
            {
                outWidth = width;
                outHeight = height;
                return;
            }
            double scale = (double)maxEdge / Math.Max(width, height);
            outWidth = (int)Math.Round(width * scale);
            outHeight = (int)Math.Round(height * scale);
            if (outWidth < 1) outWidth = 1;
            if (outHeight < 1) outHeight = 1;
        }

        static void CheckTextureSize(int container, int width, int height)
        {
            int max = ImageConvertCatalog.GetMaxDimension(container);
            if (width > max || height > max) throw new Exception(string.Format(Str.Obj.TextureSizeLimit, width, height));
        }

        /// <summary>
        /// 打开位图。各平台在遇到不认识的格式时不是抛可读异常就是空引用，
        /// 统一换成带文件名的提示。
        /// </summary>
        static YFBitmap OpenBitmap(string filePath)
        {
            YFBitmap bitmap = null;
            try
            {
                bitmap = YFBitmap.Create(filePath);
            }
            catch (Exception ex)
            {
                throw new Exception(Str.Obj.UnknownFormat + " - " + Path.GetFileName(filePath) + ": " + ex.Message);
            }
            if (bitmap == null || bitmap.Width < 1 || bitmap.Height < 1)
            {
                bitmap?.Dispose();
                throw new Exception(Str.Obj.UnknownFormat + " - " + Path.GetFileName(filePath));
            }
            return bitmap;
        }

        static void TryDelete(string filePath)
        {
            try
            {
                if (File.Exists(filePath)) File.Delete(filePath);
            }
            catch (Exception)
            {
            }
        }
    }
}
