using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Platforms;
using System.Text;
using PopStudio.Platform;

namespace PopStudio.LuaScript
{
    internal class Script : MoonSharp.Interpreter.Script
    {
        public bool ErrorThrow = true;

        private class RainyPlatformAccessorBase : PlatformAccessorBase
        {
            //
            // 摘要:
            //     Gets an environment variable. Must be implemented, but an implementation is allowed
            //     to always return null if a more meaningful implementation cannot be achieved
            //     or is not desired.
            //
            // 参数:
            //   envvarname:
            //     The envvarname.
            //
            // 返回结果:
            //     The environment variable value, or null if not found
            public override string GetEnvironmentVariable(string envvarname)
            {
                return null;
            }

            //
            // 摘要:
            //     Filters the CoreModules enumeration to exclude non-supported operations
            //
            // 参数:
            //   module:
            //     The requested modules.
            //
            // 返回结果:
            //     The requested modules, with unsupported modules filtered out.
            public override CoreModules FilterSupportedCoreModules(CoreModules module)
            {
                return module & ~(CoreModules.OS_System | CoreModules.IO);
            }

            //
            // 摘要:
            //     A function used to open files in the 'io' module. LimitedPlatformAccessorBase
            //     does NOT offer a meaningful implementation of this method and thus does not support
            //     'io' and 'os' modules.
            //
            // 参数:
            //   script:
            //
            //   filename:
            //     The filename.
            //
            //   encoding:
            //     The encoding.
            //
            //   mode:
            //     The mode (as per Lua usage - e.g. 'w+', 'rb', etc.).
            //
            // 异常:
            //   T:System.NotImplementedException:
            //     The current platform accessor does not support 'io' and 'os' operations. Provide
            //     your own implementation of platform to work around this limitation, if needed.
            public override Stream IO_OpenFile(MoonSharp.Interpreter.Script script, string filename, Encoding encoding, string mode)
            {
                throw new NotImplementedException("The current platform accessor does not support 'io' and 'os' operations. Provide your own implementation of platform to work around this limitation, if needed.");
            }

            //
            // 摘要:
            //     Gets a standard stream (stdin, stdout, stderr). LimitedPlatformAccessorBase does
            //     NOT offer a meaningful implementation of this method and thus does not support
            //     'io' and 'os' modules.
            //
            // 参数:
            //   type:
            //     The type.
            //
            // 异常:
            //   T:System.NotImplementedException:
            //     The current platform accessor does not support 'io' and 'os' operations. Provide
            //     your own implementation of platform to work around this limitation, if needed.
            public override Stream IO_GetStandardStream(StandardFileType type)
            {
                throw new NotImplementedException("The current platform accessor does not support 'io' and 'os' operations. Provide your own implementation of platform to work around this limitation, if needed.");
            }

            //
            // 摘要:
            //     Gets a temporary filename.
            public override string IO_OS_GetTempFilename() => Path.GetTempFileName();

            //
            // 摘要:
            //     Exits the process, returning the specified exit code. 
            //
            // 参数:
            //   exitCode:
            //     The exit code.
            public override void OS_ExitFast(int exitCode) => Environment.Exit(exitCode);

            //
            // 摘要:
            //     Checks if a file exists. Used by the 'os' module. 
            //
            // 参数:
            //   file:
            //     The file.
            //
            // 返回结果:
            //     True if the file exists, false otherwise.
            public override bool OS_FileExists(string file) => File.Exists(file);

            //
            // 摘要:
            //     Deletes the specified file. Used by the 'os' module.
            //
            // 参数:
            //   file:
            //     The file.
            public override void OS_FileDelete(string file) => File.Delete(file);

            //
            // 摘要:
            //     Moves the specified file. Used by the 'os' module. LimitedPlatformAccessorBase
            //     does NOT offer a meaningful implementation of this method and thus does not support
            //     'io' and 'os' modules.
            //
            // 参数:
            //   src:
            //     The source.
            //
            //   dst:
            //     The DST.
            //
            // 异常:
            //   T:System.NotImplementedException:
            //     The current platform accessor does not support 'io' and 'os' operations. Provide
            //     your own implementation of platform to work around this limitation, if needed.
            public override void OS_FileMove(string src, string dst) => File.Move(src, dst);

            //
            // 摘要:
            //     Executes the specified command line, returning the child process exit code and
            //     blocking in the meantime. LimitedPlatformAccessorBase does NOT offer a meaningful
            //     implementation of this method and thus does not support 'io' and 'os' modules.
            //
            // 参数:
            //   cmdline:
            //     The cmdline.
            //
            // 异常:
            //   T:System.NotImplementedException:
            //     The current platform accessor does not support 'io' and 'os' operations. Provide
            //     your own implementation of platform to work around this limitation, if needed.
            public override int OS_Execute(string cmdline)
            {
                throw new NotImplementedException("The current platform accessor does not support 'io' and 'os' operations. Provide your own implementation of platform to work around this limitation, if needed.");
            }

            //
            // 摘要:
            //     Gets the platform name prefix
            //
            // 异常:
            //   T:System.NotImplementedException:
            public override string GetPlatformNamePrefix()
            {
                return "rainy";
            }

            //
            // 摘要:
            //     Default handler for 'print' calls. Can be customized in ScriptOptions
            //
            // 参数:
            //   content:
            //     The content.
            //
            // 异常:
            //   T:System.NotImplementedException:
            public override void DefaultPrint(string content) => YFAPI.Print(content);
        }

        static Script()
        {
            UserData.RegisterType<BinaryStream>();
            UserData.RegisterType<TempFilePool>();
            GlobalOptions.Platform = new RainyPlatformAccessorBase();
        }

        public Script()
        {
            Table rainy = new Table(null);
            rainy["closeexception"] = (Action<object[]>)Function.CloseException;
            rainy["openexception"] = (Action<object[]>)Function.OpenException;
            rainy["throwexception"] = (Action<object[]>)Function.ThrowException;
            rainy["alert"] = (Func<object[], bool?>)Function.Alert;
            rainy["prompt"] = (Func<object[], string>)Function.Prompt;
            rainy["sheet"] = (Func<object[], string>)Function.Sheet;
            rainy["getfilestream"] = (Func<object[], BinaryStream>)Function.GetFileStream;
            rainy["getmemorystream"] = (Func<object[], BinaryStream>)Function.GetMemoryStream;
            rainy["gethttpstream"] = (Func<object[], BinaryStream>)Function.GetHttpStream;
            rainy["gettempfilepool"] = (Func<object[], TempFilePool>)Function.GetTempFilePool;
            rainy["unpack"] = (Action<object[]>)Function.Unpack;
            rainy["pack"] = (Action<object[]>)Function.Pack;
            rainy["cutimage"] = (Action<object[]>)Function.CutImage;
            rainy["spliceimage"] = (Action<object[]>)Function.SpliceImage;
            rainy["decodeimage"] = (Action<object[]>)Function.DecodeImage;
            rainy["encodeimage"] = (Action<object[]>)Function.EncodeImage;
            rainy["decodeimages"] = (Func<object[], Table>)Function.DecodeImages;
            rainy["encodeimages"] = (Func<object[], Table>)Function.EncodeImages;
            rainy["convertbitmaps"] = (Func<object[], Table>)Function.ConvertBitmaps;
            rainy["pam"] = (Action<object[]>)Function.Pam;
            rainy["parsereanim"] = (Action<object[]>)Function.ParseReanim;
            rainy["parsetrail"] = (Action<object[]>)Function.ParseTrail;
            rainy["parseparticles"] = (Action<object[]>)Function.ParseParticles;
            rainy["reanim"] = (Action<object[]>)Function.Reanim;
            rainy["trail"] = (Action<object[]>)Function.Trail;
            rainy["particles"] = (Action<object[]>)Function.Particles;
            rainy["decoderton"] = (Action<object[]>)Function.DecodeRTON;
            rainy["encoderton"] = (Action<object[]>)Function.EncodeRTON;
            rainy["decompress"] = (Action<object[]>)Function.Decompress;
            rainy["compress"] = (Action<object[]>)Function.Compress;
            rainy["newdir"] = (Action<object[]>)Function.NewDir;
            rainy["getfiles"] = (Func<object[], string[]>)Function.GetFiles;
            rainy["getfileextension"] = (Func<object[], string>)Function.GetFileExtension;
            rainy["getfilename"] = (Func<object[], string>)Function.GetFileName;
            rainy["getfilepath"] = (Func<object[], string>)Function.GetFilePath;
            rainy["getfilenamewithoutextension"] = (Func<object[], string>)Function.GetFileNameWithoutExtension;
            rainy["getversion"] = (Func<object[], int>)Function.GetVersion;
            rainy["getsystem"] = (Func<object[], int>)Function.GetSystem;
            rainy["getlanguage"] = (Func<object[], int>)Function.GetLanguage;
            rainy["formatpath"] = (Func<object[], string>)Function.FormatPath;
            rainy["dofile"] = (Action<object[]>)Function.DoFile;
            rainy["choosefolder"] = (Func<object[], string>)Function.ChooseFolder;
            rainy["chooseopenfile"] = (Func<object[], string>)Function.ChooseOpenFile;
            rainy["choosesavefile"] = (Func<object[], string>)Function.ChooseSaveFile;
            rainy["openurl"] = (Action<object[]>)Function.OpenUrl;
            rainy["deletefile"] = (Func<object[], bool?>)Function.DeleteFile;
            rainy["fileexists"] = (Func<object[], int?>)Function.FileExists;
            rainy["sleep"] = (Action<object[]>)Function.Sleep;
            rainy["httpget"] = (Func<object[], string>)Function.HttpGet;
            rainy["getansiname"] = (Func<object[], string>)Function.GetAnsiName;
            Globals["rainy"] = rainy;
        }

        //静态部分

        public static Script luavm;
        
        public static void Load()
        {
            luavm = new Script();
        }

        public static void Unload()
        {
            luavm = null;
            YFAPI.DisposeAll();
        }
        public static void Do(string str)
        {
            Load();
            try
            {
                luavm?.DoString(str);
            }
            finally
            {
                Unload();
            }
        }
    }
}