namespace PopStudio.Platform
{
    internal abstract class YFAPI
    {
        internal static YFAPI InternalYFAPI;

        public static void RegistPlatform<T>() where T : YFAPI, new() => InternalYFAPI = new T();

        public static void RegistPlatform(object o) => InternalYFAPI = (o is YFAPI api) ? api : InternalYFAPI;

        public static void LoadTextBox(object o) => InternalYFAPI.InternalLoadTextBox(o);

        public static void CloseException() => InternalYFAPI.InternalCloseException();

        public static void OpenException() => InternalYFAPI.InternalOpenException();

        public static void ThrowException(string msg) => InternalYFAPI.InternalThrowException(msg);

        public static void DoScript(string script) => InternalYFAPI.InternalDoScript(script);

        public static void DisposeAll() => InternalYFAPI.InternalDisposeAll();

        public static void Print(params object[] os) => InternalYFAPI.InternalPrint(os);

        public static bool? Alert(string text = "", string title = "PopStudio", bool ask = false) => InternalYFAPI.InternalAlert(text, title, ask);

        public static string Prompt(string text = "", string title = "PopStudio", string defaulttext = "") => InternalYFAPI.InternalPrompt(text, title, defaulttext);

        public static string Sheet(string title = "PopStudio", params string[] items) => InternalYFAPI.InternalSheet(title, items);

        public static string ChooseFolder() => InternalYFAPI.InternalChooseFolder();

        public static string ChooseOpenFile() => InternalYFAPI.InternalChooseOpenFile();

        public static string ChooseSaveFile() => InternalYFAPI.InternalChooseSaveFile();

        public static void OpenUrl(string url = "") => InternalYFAPI.InternalOpenUrl(url);

        public static BinaryStream GetFileStream(string path, int mode) => InternalYFAPI.InternalGetFileStream(path, mode);

        public static BinaryStream GetMemoryStream(byte[] bytes = null) => InternalYFAPI.InternalGetMemoryStream(bytes);

        public static BinaryStream GetHttpStream(string url) => InternalYFAPI.InternalGetHttpStream(url);

        public static TempFilePool GetTempFilePool() => InternalYFAPI.InternalGetTempFilePool();

        public static void Unpack(string inFile, string outFile, int format, bool changeimage = false, bool delete = false) => InternalYFAPI.InternalUnpack(inFile, outFile, format, changeimage, delete);

        public static void Pack(string inFile, string outFile, int format) => InternalYFAPI.InternalPack(inFile, outFile, format);

        public static void Pam(string inFile, string outFile, int informat, int outformat) => InternalYFAPI.InternalPam(inFile, outFile, informat, outformat);

        public static void DecodeImage(string inFile, string outFile, int format) => InternalYFAPI.InternalDecodeImage(inFile, outFile, format);

        public static void EncodeImage(string inFile, string outFile, int format, int format2) => InternalYFAPI.InternalEncodeImage(inFile, outFile, format, format2);

        public static void ParseReanim(string inFile, string outFile, int outformat) => InternalYFAPI.InternalParseReanim(inFile, outFile, outformat);

        public static void ParseTrail(string inFile, string outFile, int outformat) => InternalYFAPI.InternalParseTrail(inFile, outFile, outformat);

        public static void ParseParticles(string inFile, string outFile, int outformat) => InternalYFAPI.InternalParseParticles(inFile, outFile, outformat);

        public static void Reanim(string inFile, string outFile, int informat, int outformat) => InternalYFAPI.InternalReanim(inFile, outFile, informat, outformat);

        public static void Trail(string inFile, string outFile, int informat, int outformat) => InternalYFAPI.InternalTrail(inFile, outFile, informat, outformat);

        public static void Particles(string inFile, string outFile, int informat, int outformat) => InternalYFAPI.InternalParticles(inFile, outFile, informat, outformat);

        public static void DecodeRTON(string inFile, string outFile, int format) => InternalYFAPI.InternalDecodeRTON(inFile, outFile, format);

        public static void EncodeRTON(string inFile, string outFile, int format) => InternalYFAPI.InternalEncodeRTON(inFile, outFile, format);

        public static void Decompress(string inFile, string outFile, int format) => InternalYFAPI.InternalDecompress(inFile, outFile, format);

        public static void Compress(string inFile, string outFile, int format) => InternalYFAPI.InternalCompress(inFile, outFile, format);

        public static bool CutImage(string inFile, string outFolder, string infoFile, string itemName, int format) => InternalYFAPI.InternalCutImage(inFile, outFolder, infoFile, itemName, format);

        public static bool SpliceImage(string inFile, string outFolder, string infoFile, string itemName, int format, int maxWidth = 2048, int maxHeight = 2048) => InternalYFAPI.InternalSpliceImage(inFile, outFolder, infoFile, itemName, format, maxWidth, maxHeight);

        public static void NewDir(string filePath, bool toEnd = true) => InternalYFAPI.InternalNewDir(filePath, toEnd);

        public static string[] GetFiles(string filePath) => InternalYFAPI.InternalGetFiles(filePath);

        public static string GetFileExtension(string fileName) => InternalYFAPI.InternalGetFileExtension(fileName);

        public static string GetFileName(string fileName) => InternalYFAPI.InternalGetFileName(fileName);

        public static string GetFilePath(string fileName) => InternalYFAPI.InternalGetFilePath(fileName);

        public static string GetFileNameWithoutExtension(string fileName) => InternalYFAPI.InternalGetFileNameWithoutExtension(fileName);

        public static int GetVersion() => InternalYFAPI.InternalGetVersion();

        public static int GetSystem() => InternalYFAPI.InternalGetSystem();

        public static int GetLanguage() => InternalYFAPI.InternalGetLanguage();

        public static string FormatPath(string path) => InternalYFAPI.InternalFormatPath(path);

        public static void DoFile(string filepath, params object[] args) => InternalYFAPI.InternalDoFile(filepath, args);

        public static bool DeleteFile(string filePath) => InternalYFAPI.InternalDeleteFile(filePath);

        public static int FileExists(string filePath) => InternalYFAPI.InternalFileExists(filePath);

        public static void Sleep(int ms) => InternalYFAPI.InternalSleep(ms);

        public static string HttpGet(string url) => InternalYFAPI.InternalHttpGet(url);

        public static string GetAnsiName() => InternalYFAPI.InternalGetAnsiName();


        static IDisposablePool Disposable = new IDisposablePool();

        public abstract void InternalLoadTextBox(object o);

        public virtual void InternalCloseException() => LuaScript.Script.luavm.ErrorThrow = false;

        public virtual void InternalOpenException() => LuaScript.Script.luavm.ErrorThrow = true;

        public virtual void InternalThrowException(string msg) => throw new Exception(msg);

        public virtual void InternalDoScript(string script) => LuaScript.Script.Do(script ?? string.Empty);

        public virtual void InternalDisposeAll() => Disposable.Dispose();

        public abstract void InternalPrint(params object[] os);

        public abstract bool? InternalAlert(string text, string title, bool ask);

        public abstract string InternalPrompt(string text, string title, string defaulttext);

        public abstract string InternalSheet(string title, params string[] items);

        public abstract string InternalChooseFolder();

        public abstract string InternalChooseOpenFile();

        public abstract string InternalChooseSaveFile();

        public abstract void InternalOpenUrl(string url);

        public virtual BinaryStream InternalGetFileStream(string path, int mode) => Disposable.Add(mode switch
        {
            0 => new BinaryStream(path, FileMode.Open),
            1 => new BinaryStream(path, FileMode.Create),
            2 => new BinaryStream(path, FileMode.OpenOrCreate),
            _ => throw new Exception(Str.Obj.TypeMisMatch)
        });

        public virtual BinaryStream InternalGetMemoryStream(byte[] bytes) => Disposable.Add(bytes == null ? new BinaryStream() : new BinaryStream(bytes));

        public virtual BinaryStream InternalGetHttpStream(string url)
        {
            try
            {
                using (var a = new System.Net.Http.HttpClient())
                {
                    return Disposable.Add(new BinaryStream(a.GetStreamAsync(url).Result));
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public virtual TempFilePool InternalGetTempFilePool() => Disposable.Add(new TempFilePool());

        public virtual void InternalUnpack(string inFile, string outFile, int format, bool changeimage, bool delete)
        {
            switch (format)
            {
                case 0: Package.Dz.Dz.Unpack(inFile, outFile, changeimage, delete); return;
                case 1: Package.Rsb.Rsb.Unpack(inFile, outFile, changeimage, delete); return;
                case 2: Package.Pak.Pak.Unpack(inFile, outFile, changeimage, delete); return;
                case 3: Package.Arcv.Arcv.Unpack(inFile, outFile); return;
                default: throw new Exception(Str.Obj.TypeMisMatch);
            }
        }

        public virtual void InternalPack(string inFile, string outFile, int format)
        {
            switch (format)
            {
                case 0: Package.Dz.Dz.Pack(inFile, outFile); return;
                case 1: Package.Rsb.Rsb.Pack(inFile, outFile); return;
                case 2: Package.Pak.Pak.Pack(inFile, outFile); return;
                case 3: Package.Arcv.Arcv.Pack(inFile, outFile); return;
                default: throw new Exception(Str.Obj.TypeMisMatch);
            }
        }

        public virtual void InternalDecodeImage(string inFile, string outFile, int format)
        {
            switch (format)
            {
                case 0: Image.Ptx.Ptx.Decode(inFile, outFile); return;
                case 1: Image.Cdat.Cdat.Decode(inFile, outFile); return;
                case 2: Image.Tex.Tex.Decode(inFile, outFile); return;
                case 3: Image.Txz.Txz.Decode(inFile, outFile); return;
                case 4: Image.TexTV.Tex.Decode(inFile, outFile); return;
                case 5: Image.PtxXbox360.Ptx.Decode(inFile, outFile); return;
                case 6: Image.PtxPS3.Ptx.Decode(inFile, outFile); return;
                case 7: Image.PtxPSV.Ptx.Decode(inFile, outFile); return;
                case 8: Image.Xnb.Xnb.Decode(inFile, outFile); return;
                default: throw new Exception(Str.Obj.TypeMisMatch);
            }
        }

        public virtual void InternalPam(string inFile, string outFile, int informat, int outformat)
        {
            PopAnim.PopAnimInfo pam = informat switch
            {
                0 => PopAnim.PamBinary.Decode(inFile),
                1 => PopAnim.PamJson.Decode(inFile),
                2 => PopAnim.PamXfl.Decode(inFile),
                _ => throw new NotImplementedException()
            };
            switch (outformat)
            {
                case 0: PopAnim.PamBinary.Encode(pam, outFile); break;
                case 1: PopAnim.PamJson.Encode(pam, outFile); break;
                case 2: PopAnim.PamXfl.Encode(pam, outFile, Setting.PamXflResolution); break;
                default: throw new NotImplementedException();
            }
        }

        public virtual void InternalEncodeImage(string inFile, string outFile, int format, int format2)
        {
            switch (format)
            {
                case 0: Image.Ptx.Ptx.Encode(inFile, outFile, format2); return;
                case 1: Image.Cdat.Cdat.Encode(inFile, outFile); return;
                case 2: Image.Tex.Tex.Encode(inFile, outFile, format2); return;
                case 3: Image.Txz.Txz.Encode(inFile, outFile, format2); return;
                case 4: Image.TexTV.Tex.Encode(inFile, outFile, format2); return;
                case 5: Image.PtxXbox360.Ptx.Encode(inFile, outFile); return;
                case 6: Image.PtxPS3.Ptx.Encode(inFile, outFile); return;
                case 7: Image.PtxPSV.Ptx.Encode(inFile, outFile); return;
                case 8: Image.Xnb.Xnb.Encode(inFile, outFile); return;
                default: throw new Exception(Str.Obj.TypeMisMatch);
            }
        }

        public virtual void InternalParseReanim(string inFile, string outFile, int outformat)
        {
            Reanim.Reanim reanim = null;
            bool isFla = PopStudio.Reanim.FlashFla.IsZipXfl(inFile);
            bool isXfl = (Directory.Exists(inFile) && File.Exists(Path.Combine(inFile, "DOMDocument.xml")))
                || string.Equals(Path.GetExtension(inFile), ".xfl", StringComparison.OrdinalIgnoreCase)
                || string.Equals(Path.GetFileName(inFile), "DOMDocument.xml", StringComparison.OrdinalIgnoreCase);
            if (isFla)
            {
                reanim = PopStudio.Reanim.FlashFla.Decode(inFile);
            }
            else if (isXfl)
            {
                reanim = PopStudio.Reanim.FlashXfl.Decode(inFile);
            }
            for (int i = 0; reanim == null && i < 8; i++)
            {
                try
                {
                    reanim = i switch
                    {
                        0 => PopStudio.Reanim.PC.Decode(inFile),
                        1 => PopStudio.Reanim.Phone32.Decode(inFile),
                        2 => PopStudio.Reanim.Phone64.Decode(inFile),
                        3 => PopStudio.Reanim.WP.Decode(inFile),
                        4 => PopStudio.Reanim.GameConsole.Decode(inFile),
                        5 => PopStudio.Reanim.TV.Decode(inFile),
                        6 => PopStudio.Reanim.ReanimJson.Decode(inFile),
                        7 => PopStudio.Reanim.RawXml.Decode(inFile),
                        _ => throw new NotImplementedException()
                    };
                }
                catch (Exception)
                {
                    reanim = null;
                }
                if (reanim != null) break;
            }
            if (reanim == null) throw new NotImplementedException();
            switch (outformat)
            {
                case 0: PopStudio.Reanim.PC.Encode(reanim, outFile); break;
                case 1: PopStudio.Reanim.Phone32.Encode(reanim, outFile); break;
                case 2: PopStudio.Reanim.Phone64.Encode(reanim, outFile); break;
                case 3: PopStudio.Reanim.WP.Encode(reanim, outFile); break;
                case 4: PopStudio.Reanim.GameConsole.Encode(reanim, outFile); break;
                case 5: PopStudio.Reanim.TV.Encode(reanim, outFile); break;
                case 6: PopStudio.Reanim.ReanimJson.Encode(reanim, outFile); break;
                case 7: PopStudio.Reanim.RawXml.Encode(reanim, outFile); break;
                case 8: PopStudio.Reanim.FlashXfl.Encode(reanim, outFile); break;
                case 9: PopStudio.Reanim.Godot.Encode(reanim, outFile); break;
                case 10: PopStudio.Reanim.FlashFla.Encode(reanim, outFile); break;
                default: throw new NotImplementedException();
            }
        }

        public virtual void InternalParseTrail(string inFile, string outFile, int outformat)
        {
            Trail.Trail trail = null;
            for (int i = 0; i < 8; i++)
            {
                try
                {
                    trail = i switch
                    {
                        0 => PopStudio.Trail.PC.Decode(inFile),
                        1 => PopStudio.Trail.Phone32.Decode(inFile),
                        2 => PopStudio.Trail.Phone64.Decode(inFile),
                        3 => PopStudio.Trail.WP.Decode(inFile),
                        4 => PopStudio.Trail.GameConsole.Decode(inFile),
                        5 => PopStudio.Trail.TV.Decode(inFile),
                        6 => PopStudio.Trail.TrailJson.Decode(inFile),
                        7 => PopStudio.Trail.RawXml.Decode(inFile),
                        _ => throw new NotImplementedException()
                    };
                }
                catch (Exception)
                {
                    trail = null;
                }
                if (trail != null) break;
            }
            switch (outformat)
            {
                case 0: PopStudio.Trail.PC.Encode(trail, outFile); break;
                case 1: PopStudio.Trail.Phone32.Encode(trail, outFile); break;
                case 2: PopStudio.Trail.Phone64.Encode(trail, outFile); break;
                case 3: PopStudio.Trail.WP.Encode(trail, outFile); break;
                case 4: PopStudio.Trail.GameConsole.Encode(trail, outFile); break;
                case 5: PopStudio.Trail.TV.Encode(trail, outFile); break;
                case 6: PopStudio.Trail.TrailJson.Encode(trail, outFile); break;
                case 7: PopStudio.Trail.RawXml.Encode(trail, outFile); break;
                default: throw new NotImplementedException();
            }
        }

        public virtual void InternalParseParticles(string inFile, string outFile, int outformat)
        {
            Particles.Particles particles = null;
            for (int i = 0; i < 8; i++)
            {
                try
                {
                    particles = i switch
                    {
                        0 => PopStudio.Particles.PC.Decode(inFile),
                        1 => PopStudio.Particles.Phone32.Decode(inFile),
                        2 => PopStudio.Particles.Phone64.Decode(inFile),
                        3 => PopStudio.Particles.WP.Decode(inFile),
                        4 => PopStudio.Particles.GameConsole.Decode(inFile),
                        5 => PopStudio.Particles.TV.Decode(inFile),
                        6 => PopStudio.Particles.ParticlesJson.Decode(inFile),
                        7 => PopStudio.Particles.RawXml.Decode(inFile),
                        _ => throw new NotImplementedException()
                    };
                }
                catch (Exception)
                {
                    particles = null;
                }
                if (particles != null) break;
            }
            switch (outformat)
            {
                case 0: PopStudio.Particles.PC.Encode(particles, outFile); break;
                case 1: PopStudio.Particles.Phone32.Encode(particles, outFile); break;
                case 2: PopStudio.Particles.Phone64.Encode(particles, outFile); break;
                case 3: PopStudio.Particles.WP.Encode(particles, outFile); break;
                case 4: PopStudio.Particles.GameConsole.Encode(particles, outFile); break;
                case 5: PopStudio.Particles.TV.Encode(particles, outFile); break;
                case 6: PopStudio.Particles.ParticlesJson.Encode(particles, outFile); break;
                case 7: PopStudio.Particles.RawXml.Encode(particles, outFile); break;
                default: throw new NotImplementedException();
            }
        }

        public virtual void InternalReanim(string inFile, string outFile, int informat, int outformat)
        {
            Reanim.Reanim reanim = informat switch
            {
                0 => PopStudio.Reanim.PC.Decode(inFile),
                1 => PopStudio.Reanim.Phone32.Decode(inFile),
                2 => PopStudio.Reanim.Phone64.Decode(inFile),
                3 => PopStudio.Reanim.WP.Decode(inFile),
                4 => PopStudio.Reanim.GameConsole.Decode(inFile),
                5 => PopStudio.Reanim.TV.Decode(inFile),
                6 => PopStudio.Reanim.ReanimJson.Decode(inFile),
                7 => PopStudio.Reanim.RawXml.Decode(inFile),
                8 => PopStudio.Reanim.FlashXfl.Decode(inFile),
                9 => PopStudio.Reanim.FlashFla.Decode(inFile),
                _ => throw new NotImplementedException()
            };
            switch (outformat)
            {
                case 0: PopStudio.Reanim.PC.Encode(reanim, outFile); break;
                case 1: PopStudio.Reanim.Phone32.Encode(reanim, outFile); break;
                case 2: PopStudio.Reanim.Phone64.Encode(reanim, outFile); break;
                case 3: PopStudio.Reanim.WP.Encode(reanim, outFile); break;
                case 4: PopStudio.Reanim.GameConsole.Encode(reanim, outFile); break;
                case 5: PopStudio.Reanim.TV.Encode(reanim, outFile); break;
                case 6: PopStudio.Reanim.ReanimJson.Encode(reanim, outFile); break;
                case 7: PopStudio.Reanim.RawXml.Encode(reanim, outFile); break;
                case 8: PopStudio.Reanim.FlashXfl.Encode(reanim, outFile); break;
                case 9: PopStudio.Reanim.Godot.Encode(reanim, outFile); break;
                case 10: PopStudio.Reanim.FlashFla.Encode(reanim, outFile); break;
                default: throw new NotImplementedException();
            }
        }

        public virtual void InternalTrail(string inFile, string outFile, int informat, int outformat)
        {
            Trail.Trail trail = informat switch
            {
                0 => PopStudio.Trail.PC.Decode(inFile),
                1 => PopStudio.Trail.Phone32.Decode(inFile),
                2 => PopStudio.Trail.Phone64.Decode(inFile),
                3 => PopStudio.Trail.WP.Decode(inFile),
                4 => PopStudio.Trail.GameConsole.Decode(inFile),
                5 => PopStudio.Trail.TV.Decode(inFile),
                6 => PopStudio.Trail.TrailJson.Decode(inFile),
                7 => PopStudio.Trail.RawXml.Decode(inFile),
                _ => throw new NotImplementedException()
            };
            switch (outformat)
            {
                case 0: PopStudio.Trail.PC.Encode(trail, outFile); break;
                case 1: PopStudio.Trail.Phone32.Encode(trail, outFile); break;
                case 2: PopStudio.Trail.Phone64.Encode(trail, outFile); break;
                case 3: PopStudio.Trail.WP.Encode(trail, outFile); break;
                case 4: PopStudio.Trail.GameConsole.Encode(trail, outFile); break;
                case 5: PopStudio.Trail.TV.Encode(trail, outFile); break;
                case 6: PopStudio.Trail.TrailJson.Encode(trail, outFile); break;
                case 7: PopStudio.Trail.RawXml.Encode(trail, outFile); break;
                default: throw new NotImplementedException();
            }
        }

        public virtual void InternalParticles(string inFile, string outFile, int informat, int outformat)
        {
            Particles.Particles particles = informat switch
            {
                0 => PopStudio.Particles.PC.Decode(inFile),
                1 => PopStudio.Particles.Phone32.Decode(inFile),
                2 => PopStudio.Particles.Phone64.Decode(inFile),
                3 => PopStudio.Particles.WP.Decode(inFile),
                4 => PopStudio.Particles.GameConsole.Decode(inFile),
                5 => PopStudio.Particles.TV.Decode(inFile),
                6 => PopStudio.Particles.ParticlesJson.Decode(inFile),
                7 => PopStudio.Particles.RawXml.Decode(inFile),
                _ => throw new NotImplementedException()
            };
            switch (outformat)
            {
                case 0: PopStudio.Particles.PC.Encode(particles, outFile); break;
                case 1: PopStudio.Particles.Phone32.Encode(particles, outFile); break;
                case 2: PopStudio.Particles.Phone64.Encode(particles, outFile); break;
                case 3: PopStudio.Particles.WP.Encode(particles, outFile); break;
                case 4: PopStudio.Particles.GameConsole.Encode(particles, outFile); break;
                case 5: PopStudio.Particles.TV.Encode(particles, outFile); break;
                case 6: PopStudio.Particles.ParticlesJson.Encode(particles, outFile); break;
                case 7: PopStudio.Particles.RawXml.Encode(particles, outFile); break;
                default: throw new NotImplementedException();
            }
        }

        public virtual void InternalDecodeRTON(string inFile, string outFile, int format)
        {
            switch (format)
            {
                case 0: RTON.RTON.Decode(inFile, outFile); return;
                case 1: RTON.RTON.DecodeAndDecrypt(inFile, outFile); return;
                default: throw new Exception(Str.Obj.TypeMisMatch);
            }
        }

        public virtual void InternalEncodeRTON(string inFile, string outFile, int format)
        {
            switch (format)
            {
                case 0: RTON.RTON.Encode(inFile, outFile); return;
                case 1: RTON.RTON.EncodeAndEncrypt(inFile, outFile); return;
                default: throw new Exception(Str.Obj.TypeMisMatch);
            }
        }

        public virtual void InternalDecompress(string inFile, string outFile, int format)
        {
            switch (format)
            {
                case 0: PopStudio.Compress.Zlib.Decompress(inFile, outFile); return;
                case 1: PopStudio.Compress.Gzip.Decompress(inFile, outFile); return;
                case 2: PopStudio.Compress.Deflate.Decompress(inFile, outFile); return;
                case 3: PopStudio.Compress.Brotli.Decompress(inFile, outFile); return;
                case 4: PopStudio.Compress.Lzma.Decompress(inFile, outFile); return;
                case 5: PopStudio.Compress.Lz4.Decompress(inFile, outFile); return;
                case 6: PopStudio.Compress.Bzip2.Decompress(inFile, outFile); return;
                default: throw new Exception(Str.Obj.TypeMisMatch);
            }
        }

        public virtual void InternalCompress(string inFile, string outFile, int format)
        {
            switch (format)
            {
                case 0: PopStudio.Compress.Zlib.Compress(inFile, outFile); return;
                case 1: PopStudio.Compress.Gzip.Compress(inFile, outFile); return;
                case 2: PopStudio.Compress.Deflate.Compress(inFile, outFile); return;
                case 3: PopStudio.Compress.Brotli.Compress(inFile, outFile); return;
                case 4: PopStudio.Compress.Lzma.Compress(inFile, outFile); return;
                case 5: PopStudio.Compress.Lz4.Compress(inFile, outFile); return;
                case 6: PopStudio.Compress.Bzip2.Compress(inFile, outFile); return;
                default: throw new Exception(Str.Obj.TypeMisMatch);
            }
        }

        public virtual bool InternalCutImage(string inFile, string outFolder, string infoFile, string itemName, int format) => format switch
        {
            0 => Atlas.NewXml.Cut(inFile, outFolder, infoFile, itemName),
            1 => Atlas.OldXml.Cut(inFile, outFolder, infoFile, itemName),
            2 => Atlas.AncientXml.Cut(inFile, outFolder, infoFile, itemName),
            3 => Atlas.Plist.Cut(inFile, outFolder, infoFile, itemName),
            4 => Atlas.AtlasImageDat.Cut(inFile, outFolder, infoFile, itemName),
            5 => Atlas.TVAtlasXml.Cut(inFile, outFolder, infoFile, itemName),
            6 => Atlas.ResRTON.Cut(inFile, outFolder, infoFile, itemName),
            _ => throw new Exception(Str.Obj.TypeMisMatch)
        };

        public virtual bool InternalSpliceImage(string inFile, string outFolder, string infoFile, string itemName, int format, int maxWidth, int maxHeight) => format switch
        {
            0 => Atlas.NewXml.Splice(inFile, outFolder, infoFile, itemName, maxWidth, maxHeight),
            1 => Atlas.OldXml.Splice(inFile, outFolder, infoFile, itemName, maxWidth, maxHeight),
            2 => Atlas.AncientXml.Splice(inFile, outFolder, infoFile, itemName, maxWidth, maxHeight),
            3 => Atlas.Plist.Splice(inFile, outFolder, infoFile, itemName, maxWidth, maxHeight),
            4 => Atlas.AtlasImageDat.Splice(inFile, outFolder, infoFile, itemName, maxWidth, maxHeight),
            5 => Atlas.TVAtlasXml.Splice(inFile, outFolder, infoFile, itemName, maxWidth, maxHeight),
            6 => Atlas.ResRTON.Splice(inFile, outFolder, infoFile, itemName, maxWidth, maxHeight),
            _ => throw new Exception(Str.Obj.TypeMisMatch)
        };

        public virtual void InternalNewDir(string filePath, bool toEnd) => Dir.NewDir(filePath, toEnd);

        public virtual string[] InternalGetFiles(string filePath) => Dir.GetFiles(Dir.FormatPath(filePath));

        public virtual string InternalGetFileExtension(string fileName) => Path.GetExtension(fileName);

        public virtual string InternalGetFileName(string fileName) => Path.GetFileName(Dir.FormatPath(fileName));

        public virtual string InternalGetFilePath(string fileName) => Path.GetDirectoryName(Dir.FormatPath(fileName));

        public virtual string InternalGetFileNameWithoutExtension(string fileName) => Path.GetFileNameWithoutExtension(Dir.FormatPath(fileName));

        public virtual int InternalGetVersion() => Const.RAINYVERSION;

        public virtual int InternalGetSystem() => Const.SYSTEM;

        public virtual int InternalGetLanguage() => (int)Setting.AppLanguage;

        public virtual string InternalFormatPath(string path) => Dir.FormatPath(path);

        public virtual void InternalDoFile(string filepath, params object[] args)
        {
            string s;
            (LuaScript.Script.luavm.Globals["rainy"] as MoonSharp.Interpreter.Table)["cache"] = args;
            using (StreamReader sr = new StreamReader(filepath.ToString()))
            {
                s = "local args = rainy.cache; rainy.cache = nil; " + sr.ReadToEnd();
            }
            LuaScript.Script.luavm.DoString(s);
        }

        public virtual bool InternalDeleteFile(string filePath)
        {
            if (Directory.Exists(filePath))
            {
                Directory.Delete(filePath, true);
                return true;
            }
            else if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }
            return false;
        }

        public virtual int InternalFileExists(string filePath) => (File.Exists(filePath) ? 0b1 : 0b0) | (Directory.Exists(filePath) ? 0b10 : 0b00);

        public virtual void InternalSleep(int ms) => Thread.Sleep(ms);

        public virtual string InternalHttpGet(string url)
        {
            try
            {
                using (var a = new System.Net.Http.HttpClient()) return a.GetStringAsync(url).Result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public virtual string InternalGetAnsiName() => EncodeHelper.ANSI.BodyName;
    }
}
