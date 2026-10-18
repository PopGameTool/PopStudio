using MoonSharp.Interpreter;
using PopStudio.Image.Convert;
using PopStudio.Platform;

namespace PopStudio.LuaScript
{
    internal class Function
    {
        public static void CloseException(params object[] args)
        {
            try
            {
                YFAPI.CloseException();
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
            }
        }

        public static void OpenException(params object[] args)
        {
            try
            {
                YFAPI.OpenException();
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
            }
        }

        public static void ThrowException(params object[] args)
        {
            try
            {
                YFAPI.ThrowException(args.Length > 0 ? args[0]?.ToString() : null);
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
            }
        }

        public static void Print(params object[] args)
        {
            try
            {
                YFAPI.Print(args);
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
            }
        }

        public static bool? Alert(params object[] args)
        {
            try
            {
                object[] n = new object[3];
                int min = args.Length > n.Length ? n.Length : args.Length;
                for (int i = 0; i < min; i++)
                {
                    n[i] = args[i];
                }
                return YFAPI.Alert(n[0]?.ToString() ?? "", n[1]?.ToString() ?? "PopStudio", (n[2] != null) && (n[2] is not bool boolean || (boolean == true)));
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
                return null;
            }
        }

        public static string Prompt(params object[] args)
        {
            try
            {
                object[] n = new object[3];
                int min = args.Length > n.Length ? n.Length : args.Length;
                for (int i = 0; i < min; i++)
                {
                    n[i] = args[i];
                }
                return YFAPI.Prompt(n[0]?.ToString() ?? "", n[1]?.ToString() ?? "PopStudio", n[2]?.ToString() ?? "");
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
                return null;
            }
        }

        public static string Sheet(params object[] args)
        {
            try
            {
                if (args.Length <= 1)
                {
                    string title = (args.Length == 0) ? "PopStudio" : args[0]?.ToString() ?? "PopStudio";
                    return YFAPI.Sheet(title);
                }
                else
                {
                    string[] n = new string[args.Length - 1];
                    int min = n.Length;
                    for (int i = 0; i < min; i++)
                    {
                        n[i] = args[i + 1]?.ToString() ?? "";
                    }
                    return YFAPI.Sheet(args[0]?.ToString() ?? "PopStudio", n);
                }
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
                return null;
            }
        }

        public static BinaryStream GetFileStream(params object[] args)
        {
            try
            {
                object[] n = new object[2];
                int min = args.Length > n.Length ? n.Length : args.Length;
                for (int i = 0; i < min; i++)
                {
                    n[i] = args[i];
                }
                return YFAPI.GetFileStream(n[0]?.ToString(), Convert.ToInt32(n[1] ?? "-1"));
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
                return null;
            }
        }

        public static BinaryStream GetMemoryStream(params object[] args)
        {
            try
            {
                byte[] arg = (args.Length >= 1 && args[0] is byte[] b) ? b : null;
                return YFAPI.GetMemoryStream(arg);
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
                return null;
            }
        }

        public static BinaryStream GetHttpStream(params object[] args)
        {
            try
            {
                return YFAPI.GetHttpStream(args.Length >= 1 ? args[0].ToString() : null);
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
                return null;
            }
        }

        public static TempFilePool GetTempFilePool(params object[] args)
        {
            try
            {
                return YFAPI.GetTempFilePool();
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
                return null;
            }
        }

        public static void Unpack(params object[] args)
        {
            try
            {
                object[] n = new object[5];
                int min = args.Length > n.Length ? n.Length : args.Length;
                for (int i = 0; i < min; i++)
                {
                    n[i] = args[i];
                }
                YFAPI.Unpack(n[0]?.ToString(), n[1]?.ToString(), Convert.ToInt32(n[2] ?? "-1"), (n[3] != null) && (n[3] is not bool boolean || (boolean == true)), (n[4] != null) && (n[4] is not bool boolean2 || (boolean2 == true)));
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
            }
        }

        public static void Pack(params object[] args)
        {
            try
            {
                object[] n = new object[3];
                int min = args.Length > n.Length ? n.Length : args.Length;
                for (int i = 0; i < min; i++)
                {
                    n[i] = args[i];
                }
                YFAPI.Pack(n[0]?.ToString(), n[1]?.ToString(), Convert.ToInt32(n[2] ?? "-1"));
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
            }
        }

        public static void CutImage(params object[] args)
        {
            try
            {
                object[] n = new object[5];
                int min = args.Length > n.Length ? n.Length : args.Length;
                for (int i = 0; i < min; i++)
                {
                    n[i] = args[i];
                }
                YFAPI.CutImage(n[0]?.ToString(), n[1]?.ToString(), n[2]?.ToString(), n[3]?.ToString(), Convert.ToInt32(n[4] ?? "-1"));
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
            }
        }

        public static void SpliceImage(params object[] args)
        {
            try
            {
                object[] n = new object[7];
                int min = args.Length > n.Length ? n.Length : args.Length;
                for (int i = 0; i < min; i++)
                {
                    n[i] = args[i];
                }
                YFAPI.SpliceImage(n[0]?.ToString(), n[1]?.ToString(), n[2]?.ToString(), n[3]?.ToString(), Convert.ToInt32(n[4] ?? "-1"), Convert.ToInt32(n[5] ?? "2048"), Convert.ToInt32(n[6] ?? "2048"));
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
            }
        }

        public static void Pam(params object[] args)
        {
            try
            {
                object[] n = new object[4];
                int min = args.Length > n.Length ? n.Length : args.Length;
                for (int i = 0; i < min; i++)
                {
                    n[i] = args[i];
                }
                YFAPI.Pam(n[0]?.ToString(), n[1]?.ToString(), Convert.ToInt32(n[2] ?? "-1"), Convert.ToInt32(n[3] ?? "-1"));
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
            }
        }

        public static void DecodeImage(params object[] args)
        {
            try
            {
                object[] n = new object[3];
                int min = args.Length > n.Length ? n.Length : args.Length;
                for (int i = 0; i < min; i++)
                {
                    n[i] = args[i];
                }
                YFAPI.DecodeImage(n[0]?.ToString(), n[1]?.ToString(), Convert.ToInt32(n[2] ?? "-1"));
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
            }
        }

        public static void EncodeImage(params object[] args)
        {
            try
            {
                object[] n = new object[4];
                int min = args.Length > n.Length ? n.Length : args.Length;
                for (int i = 0; i < min; i++)
                {
                    n[i] = args[i];
                }
                YFAPI.EncodeImage(n[0]?.ToString(), n[1]?.ToString(), Convert.ToInt32(n[2] ?? "-1"), Convert.ToInt32(n[3] ?? "-1"));
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
            }
        }

        public static void ParseReanim(params object[] args)
        {
            try
            {
                object[] n = new object[4];
                int min = args.Length > n.Length ? n.Length : args.Length;
                for (int i = 0; i < min; i++)
                {
                    n[i] = args[i];
                }
                YFAPI.ParseReanim(n[0]?.ToString(), n[1]?.ToString(), Convert.ToInt32(n[2] ?? "-1"), Convert.ToBoolean(n[3] ?? false));
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
            }
        }

        public static void ParseTrail(params object[] args)
        {
            try
            {
                object[] n = new object[3];
                int min = args.Length > n.Length ? n.Length : args.Length;
                for (int i = 0; i < min; i++)
                {
                    n[i] = args[i];
                }
                YFAPI.ParseTrail(n[0]?.ToString(), n[1]?.ToString(), Convert.ToInt32(n[2] ?? "-1"));
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
            }
        }

        public static void ParseParticles(params object[] args)
        {
            try
            {
                object[] n = new object[3];
                int min = args.Length > n.Length ? n.Length : args.Length;
                for (int i = 0; i < min; i++)
                {
                    n[i] = args[i];
                }
                YFAPI.ParseParticles(n[0]?.ToString(), n[1]?.ToString(), Convert.ToInt32(n[2] ?? "-1"));
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
            }
        }

        public static void Reanim(params object[] args)
        {
            try
            {
                object[] n = new object[5];
                int min = args.Length > n.Length ? n.Length : args.Length;
                for (int i = 0; i < min; i++)
                {
                    n[i] = args[i];
                }
                YFAPI.Reanim(n[0]?.ToString(), n[1]?.ToString(), Convert.ToInt32(n[2] ?? "-1"), Convert.ToInt32(n[3] ?? "-1"), Convert.ToBoolean(n[4] ?? false));
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
            }
        }

        public static void Particles(params object[] args)
        {
            try
            {
                object[] n = new object[4];
                int min = args.Length > n.Length ? n.Length : args.Length;
                for (int i = 0; i < min; i++)
                {
                    n[i] = args[i];
                }
                YFAPI.Particles(n[0]?.ToString(), n[1]?.ToString(), Convert.ToInt32(n[2] ?? "-1"), Convert.ToInt32(n[3] ?? "-1"));
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
            }
        }

        public static void Trail(params object[] args)
        {
            try
            {
                object[] n = new object[4];
                int min = args.Length > n.Length ? n.Length : args.Length;
                for (int i = 0; i < min; i++)
                {
                    n[i] = args[i];
                }
                YFAPI.Trail(n[0]?.ToString(), n[1]?.ToString(), Convert.ToInt32(n[2] ?? "-1"), Convert.ToInt32(n[3] ?? "-1"));
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
            }
        }

        public static void DecodeRTON(params object[] args)
        {
            try
            {
                object[] n = new object[3];
                int min = args.Length > n.Length ? n.Length : args.Length;
                for (int i = 0; i < min; i++)
                {
                    n[i] = args[i];
                }
                YFAPI.DecodeRTON(n[0]?.ToString(), n[1]?.ToString(), Convert.ToInt32(n[2] ?? "-1"));
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
            }
        }

        public static void EncodeRTON(params object[] args)
        {
            try
            {
                object[] n = new object[3];
                int min = args.Length > n.Length ? n.Length : args.Length;
                for (int i = 0; i < min; i++)
                {
                    n[i] = args[i];
                }
                YFAPI.EncodeRTON(n[0]?.ToString(), n[1]?.ToString(), Convert.ToInt32(n[2] ?? "-1"));
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
            }
        }

        public static void Decompress(params object[] args)
        {
            try
            {
                object[] n = new object[3];
                int min = args.Length > n.Length ? n.Length : args.Length;
                for (int i = 0; i < min; i++)
                {
                    n[i] = args[i];
                }
                YFAPI.Decompress(n[0]?.ToString(), n[1]?.ToString(), Convert.ToInt32(n[2] ?? "-1"));
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
            }
        }

        public static void Compress(params object[] args)
        {
            try
            {
                object[] n = new object[3];
                int min = args.Length > n.Length ? n.Length : args.Length;
                for (int i = 0; i < min; i++)
                {
                    n[i] = args[i];
                }
                YFAPI.Compress(n[0]?.ToString(), n[1]?.ToString(), Convert.ToInt32(n[2] ?? "-1"));
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
            }
        }

        public static void NewDir(params object[] args)
        {
            try
            {
                object[] n = new object[2];
                int min = args.Length > n.Length ? n.Length : args.Length;
                for (int i = 0; i < min; i++)
                {
                    n[i] = args[i];
                }
                YFAPI.NewDir(n[0]?.ToString(), (n[1] == null) || n[1] is not bool boolean || (boolean == true));
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
            }
        }

        public static string[] GetFiles(params object[] args)
        {
            try
            {
                return YFAPI.GetFiles(args.Length == 0 ? null : args[0]?.ToString());
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
                return null;
            }
        }

        public static string GetFileExtension(params object[] args)
        {
            try
            {
                return YFAPI.GetFileExtension(args.Length == 0 ? null : args[0]?.ToString());
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
                return null;
            }
        }

        public static string GetFileName(params object[] args)
        {
            try
            {
                return YFAPI.GetFileName(args.Length == 0 ? null : args[0]?.ToString());
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
                return null;
            }
        }

        public static string GetFilePath(params object[] args)
        {
            try
            {
                return YFAPI.GetFilePath(args.Length == 0 ? null : args[0]?.ToString());
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
                return null;
            }
        }

        public static string GetFileNameWithoutExtension(params object[] args)
        {
            try
            {
                return YFAPI.GetFileNameWithoutExtension(args.Length == 0 ? null : args[0]?.ToString());
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
                return null;
            }
        }

        public static int GetVersion(params object[] args)
        {
            try
            {
                return YFAPI.GetVersion();
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
                return -1;
            }
        }

        public static int GetSystem(params object[] args)
        {
            try
            {
                return YFAPI.GetSystem();
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
                return -1;
            }
        }

        public static int GetLanguage(params object[] args)
        {
            try
            {
                return YFAPI.GetLanguage();
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
                return -1;
            }
        }

        public static string FormatPath(params object[] args)
        {
            try
            {
                return YFAPI.FormatPath(args.Length == 0 ? null : args[0]?.ToString());
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
                return null;
            }
        }

        public static void DoFile(params object[] args)
        {
            try
            {
                if (args.Length <= 1)
                {
                    YFAPI.DoFile((args.Length == 0) ? null : args[0]?.ToString());
                }
                else
                {
                    object[] n = new object[args.Length - 1];
                    Array.Copy(args, 1, n, 0, n.Length);
                    YFAPI.DoFile(args[0]?.ToString(), n);
                }
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
            }
        }

        public static string ChooseOpenFile(params object[] args)
        {
            try
            {
                return YFAPI.ChooseOpenFile();
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
                return null;
            }
        }

        public static string ChooseSaveFile(params object[] args)
        {
            try
            {
                return YFAPI.ChooseSaveFile();
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
                return null;
            }
        }

        public static string ChooseFolder(params object[] args)
        {
            try
            {
                return YFAPI.ChooseFolder();
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
                return null;
            }
        }

        public static bool? DeleteFile(params object[] args)
        {
            try
            {
                return YFAPI.DeleteFile(args.Length == 0 ? null : args[0]?.ToString());
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
                return null;
            }
        }

        public static void OpenUrl(params object[] args)
        {
            try
            {
                YFAPI.OpenUrl(args.Length > 0 ? (args[0]?.ToString()) : null);
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
            }
        }

        public static int? FileExists(params object[] args)
        {
            try
            {
                return YFAPI.FileExists(args.Length == 0 ? null : args[0]?.ToString());
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
                return null;
            }
        }

        public static void Sleep(params object[] args)
        {
            try
            {
                YFAPI.Sleep(args.Length == 0 ? 0 : Convert.ToInt32(args[0]));
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
            }
        }

        public static string HttpGet(params object[] args)
        {
            try
            {
                return YFAPI.HttpGet(args.Length > 0 ? (args[0]?.ToString()) : null);
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
                return null;
            }
        }

        public static string GetAnsiName(params object[] args)
        {
            try
            {
                return YFAPI.GetAnsiName();
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// rainy.decodeimages(input, outdir, container[, recursive[, overwrite]])
        /// </summary>
        public static Table DecodeImages(params object[] args)
        {
            try
            {
                List<string> inputs = new List<string>();
                AddPath(args.Length > 0 ? args[0] : null, inputs);
                ImageConvertOptions options = new ImageConvertOptions
                {
                    Mode = ImageConvertMode.TextureToBitmap,
                    Container = ArgInt(args, 2, -1),
                    OutputDirectory = ArgString(args, 1),
                    IncludeSubdirectories = ArgBool(args, 3, true),
                    Conflict = ArgBool(args, 4, false) ? ImageConflictPolicy.Overwrite : ImageConflictPolicy.Skip
                };
                return BuildResult(ImageBatchConverter.Convert(inputs, options));
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// rainy.encodeimages(input, outdir, container, pixelFormat[, recursive[, overwrite]])
        /// pixelFormat 是像素格式列表的下标，不是标签里括号中的数字
        /// </summary>
        public static Table EncodeImages(params object[] args)
        {
            try
            {
                List<string> inputs = new List<string>();
                AddPath(args.Length > 0 ? args[0] : null, inputs);
                ImageConvertOptions options = new ImageConvertOptions
                {
                    Mode = ImageConvertMode.BitmapToTexture,
                    Container = ArgInt(args, 2, -1),
                    PixelFormat = ArgInt(args, 3, 0),
                    OutputDirectory = ArgString(args, 1),
                    IncludeSubdirectories = ArgBool(args, 4, true),
                    Conflict = ArgBool(args, 5, false) ? ImageConflictPolicy.Overwrite : ImageConflictPolicy.Skip
                };
                return BuildResult(ImageBatchConverter.Convert(inputs, options));
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// rainy.convertbitmaps(input, outdir, bitmapFormat[, quality[, maxEdge[, recursive[, overwrite]]]])
        /// bitmapFormat 收 0/1/2 或 "png"/"jpg"/"bmp"
        /// </summary>
        public static Table ConvertBitmaps(params object[] args)
        {
            try
            {
                List<string> inputs = new List<string>();
                AddPath(args.Length > 0 ? args[0] : null, inputs);
                YFBitmapFormat format = YFBitmapFormat.Png;
                object rawFormat = args.Length > 2 ? args[2] : null;
                if (rawFormat != null && !TryParseBitmapFormat(rawFormat, out format)) throw new Exception(Str.Obj.UnknownFormat);
                ImageConvertOptions options = new ImageConvertOptions
                {
                    Mode = ImageConvertMode.BitmapToBitmap,
                    OutputDirectory = ArgString(args, 1),
                    BitmapFormat = format,
                    JpegQuality = ArgInt(args, 3, 90),
                    MaxEdge = ArgInt(args, 4, 0),
                    IncludeSubdirectories = ArgBool(args, 5, true),
                    Conflict = ArgBool(args, 6, false) ? ImageConflictPolicy.Overwrite : ImageConflictPolicy.Skip
                };
                return BuildResult(ImageBatchConverter.Convert(inputs, options));
            }
            catch (Exception ex)
            {
                if (Script.luavm.ErrorThrow) throw;
                else Print(ex.Message);
                return null;
            }
        }

        /// <summary>输入可以是单个路径字符串，也可以是路径的数组</summary>
        static void AddPath(object value, List<string> paths)
        {
            if (value is Table table)
            {
                long length = table.Length;
                for (long i = 1; i <= length; i++)
                {
                    string item = table.Get((int)i)?.ToString();
                    if (!string.IsNullOrWhiteSpace(item)) paths.Add(item);
                }
                return;
            }
            string path = value?.ToString();
            if (!string.IsNullOrWhiteSpace(path)) paths.Add(path);
        }

        static int ArgInt(object[] args, int index, int fallback)
        {
            if (args == null || index >= args.Length || args[index] == null) return fallback;
            try
            {
                return Convert.ToInt32(args[index]);
            }
            catch (Exception)
            {
                return fallback;
            }
        }

        static string ArgString(object[] args, int index)
        {
            if (args == null || index >= args.Length || args[index] == null) return null;
            string value = args[index].ToString();
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        static bool ArgBool(object[] args, int index, bool fallback)
        {
            if (args == null || index >= args.Length || args[index] == null) return fallback;
            if (args[index] is bool value) return value;
            try
            {
                return Convert.ToInt32(args[index]) != 0;
            }
            catch (Exception)
            {
                return fallback;
            }
        }

        static bool TryParseBitmapFormat(object value, out YFBitmapFormat format)
        {
            format = YFBitmapFormat.Png;
            if (value == null) return false;
            if (value is string text)
            {
                BitmapFormatInfo info = ImageConvertCatalog.GetBitmapFormat(text.StartsWith(".") ? text : "." + text);
                if (info == null) return false;
                format = info.Format;
                return true;
            }
            int index;
            try
            {
                index = Convert.ToInt32(value);
            }
            catch (Exception)
            {
                return false;
            }
            if (ImageConvertCatalog.GetBitmapFormat((YFBitmapFormat)index) == null) return false;
            format = (YFBitmapFormat)index;
            return true;
        }

        static Table BuildResult(ImageConvertResult result)
        {
            if (result == null) return null;
            Table ans = new Table(Script.luavm);
            ans["total"] = DynValue.NewNumber(result.Total);
            ans["success"] = DynValue.NewNumber(result.Succeeded);
            ans["skipped"] = DynValue.NewNumber(result.Skipped);
            ans["failed"] = DynValue.NewNumber(result.Failed);
            ans["cancelled"] = DynValue.NewBoolean(result.Cancelled);
            ans["time"] = DynValue.NewNumber(result.Elapsed.TotalSeconds);
            Table failures = new Table(Script.luavm);
            IReadOnlyList<ImageConvertFailure> list = result.Failures;
            for (int i = 0; i < list.Count; i++)
            {
                Table item = new Table(Script.luavm);
                item["input"] = DynValue.NewString(list[i].InputPath ?? string.Empty);
                item["output"] = DynValue.NewString(list[i].OutputPath ?? string.Empty);
                item["message"] = DynValue.NewString(list[i].Message ?? string.Empty);
                failures[i + 1] = item;
            }
            ans["failures"] = failures;
            return ans;
        }
    }
}
