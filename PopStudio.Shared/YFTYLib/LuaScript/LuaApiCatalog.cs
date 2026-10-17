namespace PopStudio.LuaScript
{
    internal enum LuaApiCategory
    {
        UI,
        File,
        Image,
        RTON,
        Animation,
        Compress,
        System,
        Global,
        Stream
    }

    internal sealed class LuaApiEntry
    {
        public string Name { get; }
        public string Signature { get; }
        public LuaApiCategory Category { get; }
        public string SummaryZh { get; }
        public string SummaryEn { get; }
        public string InsertSnippet { get; }

        public LuaApiEntry(string name, string signature, LuaApiCategory category, string summaryZh, string summaryEn, string insertSnippet)
        {
            Name = name;
            Signature = signature;
            Category = category;
            SummaryZh = summaryZh;
            SummaryEn = summaryEn;
            InsertSnippet = insertSnippet;
        }

        public string GetSummary(bool chinese) => chinese ? SummaryZh : SummaryEn;
    }

    internal static class LuaApiCatalog
    {
        public static IReadOnlyList<LuaApiEntry> All { get; } = Build();

        public static IReadOnlyList<LuaApiCategory> Categories { get; } = new[]
        {
            LuaApiCategory.UI,
            LuaApiCategory.File,
            LuaApiCategory.Image,
            LuaApiCategory.RTON,
            LuaApiCategory.Animation,
            LuaApiCategory.Compress,
            LuaApiCategory.System,
            LuaApiCategory.Global,
            LuaApiCategory.Stream
        };

        public static IEnumerable<LuaApiEntry> GetByCategory(LuaApiCategory category)
        {
            for (int i = 0; i < All.Count; i++)
            {
                if (All[i].Category == category) yield return All[i];
            }
        }

        public static IEnumerable<LuaApiEntry> GetRainyApis()
        {
            for (int i = 0; i < All.Count; i++)
            {
                LuaApiCategory c = All[i].Category;
                if (c != LuaApiCategory.Global && c != LuaApiCategory.Stream) yield return All[i];
            }
        }

        static List<LuaApiEntry> Build()
        {
            return new List<LuaApiEntry>
            {
                // UI
                E("alert", "rainy.alert(msg [, title [, ask]])", LuaApiCategory.UI,
                    "弹出提示框；ask 为真时显示确定/取消并返回是否确定。",
                    "Show an alert dialog; when ask is true, returns whether OK was chosen.",
                    "rainy.alert(\"message\")"),
                E("prompt", "rainy.prompt(msg [, title [, initial]])", LuaApiCategory.UI,
                    "弹出输入框，返回用户输入的字符串。",
                    "Show an input dialog and return the entered string.",
                    "rainy.prompt(\"message\", \"PopStudio\", \"\")"),
                E("sheet", "rainy.sheet(title [, option1, option2, ...])", LuaApiCategory.UI,
                    "弹出选项列表（ActionSheet），返回所选选项文本。",
                    "Show an action sheet and return the selected option text.",
                    "rainy.sheet(\"title\", \"A\", \"B\", \"C\")"),

                // File
                E("getfilestream", "rainy.getfilestream(path [, mode])", LuaApiCategory.File,
                    "打开文件流，返回 BinaryStream。mode 默认 -1。",
                    "Open a file stream and return BinaryStream. mode defaults to -1.",
                    "local s = rainy.getfilestream(\"path\")"),
                E("getmemorystream", "rainy.getmemorystream([bytes])", LuaApiCategory.File,
                    "创建内存流；可选传入初始字节数组。",
                    "Create a memory stream; optional initial byte array.",
                    "local s = rainy.getmemorystream()"),
                E("gethttpstream", "rainy.gethttpstream(url)", LuaApiCategory.File,
                    "从 URL 获取 HTTP 流，返回 BinaryStream。",
                    "Open an HTTP stream from url and return BinaryStream.",
                    "local s = rainy.gethttpstream(\"https://github.com/PopGameTool/PopStudio\")"),
                E("gettempfilepool", "rainy.gettempfilepool()", LuaApiCategory.File,
                    "获取临时文件池 TempFilePool，脚本结束时会清理。",
                    "Get a TempFilePool that cleans up when the script ends.",
                    "local pool = rainy.gettempfilepool()"),
                E("newdir", "rainy.newdir(path [, clear])", LuaApiCategory.File,
                    "创建目录；clear 默认真，已存在时可清空。",
                    "Create a directory; clear defaults to true.",
                    "rainy.newdir(\"path\")"),
                E("getfiles", "rainy.getfiles(dir)", LuaApiCategory.File,
                    "列出目录下文件路径数组。",
                    "List file paths under a directory.",
                    "local files = rainy.getfiles(\"dir\")"),
                E("getfileextension", "rainy.getfileextension(path)", LuaApiCategory.File,
                    "获取文件扩展名。",
                    "Get the file extension.",
                    "rainy.getfileextension(path)"),
                E("getfilename", "rainy.getfilename(path)", LuaApiCategory.File,
                    "获取带扩展名的文件名。",
                    "Get the file name with extension.",
                    "rainy.getfilename(path)"),
                E("getfilepath", "rainy.getfilepath(path)", LuaApiCategory.File,
                    "获取路径所在目录。",
                    "Get the directory of a path.",
                    "rainy.getfilepath(path)"),
                E("getfilenamewithoutextension", "rainy.getfilenamewithoutextension(path)", LuaApiCategory.File,
                    "获取不带扩展名的文件名。",
                    "Get the file name without extension.",
                    "rainy.getfilenamewithoutextension(path)"),
                E("formatpath", "rainy.formatpath(path)", LuaApiCategory.File,
                    "规范化路径分隔符。",
                    "Normalize path separators.",
                    "rainy.formatpath(path)"),
                E("fileexists", "rainy.fileexists(path)", LuaApiCategory.File,
                    "判断文件是否存在（返回整数状态）。",
                    "Check whether a file exists (integer status).",
                    "rainy.fileexists(path)"),
                E("deletefile", "rainy.deletefile(path)", LuaApiCategory.File,
                    "删除文件，返回是否成功。",
                    "Delete a file and return success.",
                    "rainy.deletefile(path)"),
                E("choosefolder", "rainy.choosefolder()", LuaApiCategory.File,
                    "弹出文件夹选择对话框，返回路径。",
                    "Show a folder picker and return the path.",
                    "local dir = rainy.choosefolder()"),
                E("chooseopenfile", "rainy.chooseopenfile()", LuaApiCategory.File,
                    "弹出打开文件对话框，返回路径。",
                    "Show an open-file dialog and return the path.",
                    "local path = rainy.chooseopenfile()"),
                E("choosesavefile", "rainy.choosesavefile()", LuaApiCategory.File,
                    "弹出保存文件对话框，返回路径。",
                    "Show a save-file dialog and return the path.",
                    "local path = rainy.choosesavefile()"),
                E("dofile", "rainy.dofile(path [, ...])", LuaApiCategory.File,
                    "执行另一个 Lua 文件；额外参数可通过 args 访问。",
                    "Run another Lua file; extra args are available as args.",
                    "rainy.dofile(\"script.lua\")"),
                E("unpack", "rainy.unpack(inpath, outdir [, mode [, decodeimage [, deleteimage]]])", LuaApiCategory.File,
                    "解包数据包（dz/rsb/pak/arcv 等）；mode 默认 -1。",
                    "Unpack a package (dz/rsb/pak/arcv, etc.); mode defaults to -1.",
                    "rainy.unpack(\"in\", \"outdir\")"),
                E("pack", "rainy.pack(indir, outpath [, mode])", LuaApiCategory.File,
                    "打包目录为数据包；mode 默认 -1。",
                    "Pack a folder into a package; mode defaults to -1.",
                    "rainy.pack(\"indir\", \"out\")"),

                // Image
                E("cutimage", "rainy.cutimage(inpath, outdir, info, info2 [, mode])", LuaApiCategory.Image,
                    "按切割信息切割图集。",
                    "Cut an atlas by cutting information.",
                    "rainy.cutimage(\"in\", \"outdir\", \"info\", \"info2\")"),
                E("spliceimage", "rainy.spliceimage(indir, outpath, info, info2 [, mode [, maxw [, maxh]]])", LuaApiCategory.Image,
                    "将散图拼接为图集；默认最大宽高 2048。",
                    "Splice images into an atlas; default max size 2048.",
                    "rainy.spliceimage(\"indir\", \"out\", \"info\", \"info2\")"),
                E("decodeimage", "rainy.decodeimage(inpath, outpath [, mode])", LuaApiCategory.Image,
                    "解码纹理/图像格式到普通图像。",
                    "Decode a texture/image format to a normal image.",
                    "rainy.decodeimage(\"in\", \"out.png\")"),
                E("encodeimage", "rainy.encodeimage(inpath, outpath [, mode [, format]])", LuaApiCategory.Image,
                    "将图像编码为游戏纹理格式。",
                    "Encode an image into a game texture format.",
                    "rainy.encodeimage(\"in.png\", \"out\")"),

                // RTON
                E("decoderton", "rainy.decoderton(inpath, outpath [, mode])", LuaApiCategory.RTON,
                    "解码 RTON 为 JSON/文本；加密 RTON 需在设置中配置密钥。",
                    "Decode RTON to JSON/text; encrypted RTON needs a key in settings.",
                    "rainy.decoderton(\"in.rton\", \"out.json\")"),
                E("encoderton", "rainy.encoderton(inpath, outpath [, mode])", LuaApiCategory.RTON,
                    "将 JSON/文本编码为 RTON。",
                    "Encode JSON/text to RTON.",
                    "rainy.encoderton(\"in.json\", \"out.rton\")"),

                // Animation
                E("pam", "rainy.pam(inpath, outpath [, inmode [, outmode]])", LuaApiCategory.Animation,
                    "转换 PAM 动画格式。",
                    "Convert PAM animation formats.",
                    "rainy.pam(\"in\", \"out\")"),
                E("parsereanim", "rainy.parsereanim(inpath, outpath [, mode])", LuaApiCategory.Animation,
                    "解析 reanim.compiled 为可读格式。",
                    "Parse reanim.compiled into a readable format.",
                    "rainy.parsereanim(\"in\", \"out\")"),
                E("parsetrail", "rainy.parsetrail(inpath, outpath [, mode])", LuaApiCategory.Animation,
                    "解析 trail.compiled 为可读格式。",
                    "Parse trail.compiled into a readable format.",
                    "rainy.parsetrail(\"in\", \"out\")"),
                E("parseparticles", "rainy.parseparticles(inpath, outpath [, mode])", LuaApiCategory.Animation,
                    "解析 particles（xml.compiled）为可读格式。",
                    "Parse particles (xml.compiled) into a readable format.",
                    "rainy.parseparticles(\"in\", \"out\")"),
                E("reanim", "rainy.reanim(inpath, outpath [, inmode [, outmode]])", LuaApiCategory.Animation,
                    "在不同 reanim 格式之间转换（含 XFL/FLA）。",
                    "Convert between reanim formats (including XFL/FLA).",
                    "rainy.reanim(\"in\", \"out\")"),
                E("trail", "rainy.trail(inpath, outpath [, inmode [, outmode]])", LuaApiCategory.Animation,
                    "在不同 trail 格式之间转换。",
                    "Convert between trail formats.",
                    "rainy.trail(\"in\", \"out\")"),
                E("particles", "rainy.particles(inpath, outpath [, inmode [, outmode]])", LuaApiCategory.Animation,
                    "在不同 particles 格式之间转换。",
                    "Convert between particles formats.",
                    "rainy.particles(\"in\", \"out\")"),

                // Compress
                E("decompress", "rainy.decompress(inpath, outpath [, mode])", LuaApiCategory.Compress,
                    "解压缩文件（gzip/zlib/lzma/lz4/brotli/bzip2 等）。",
                    "Decompress a file (gzip/zlib/lzma/lz4/brotli/bzip2, etc.).",
                    "rainy.decompress(\"in\", \"out\")"),
                E("compress", "rainy.compress(inpath, outpath [, mode])", LuaApiCategory.Compress,
                    "压缩文件。",
                    "Compress a file.",
                    "rainy.compress(\"in\", \"out\")"),

                // System
                E("closeexception", "rainy.closeexception()", LuaApiCategory.System,
                    "关闭异常抛出：出错时打印而不中断脚本。",
                    "Disable throwing: print errors instead of aborting.",
                    "rainy.closeexception()"),
                E("openexception", "rainy.openexception()", LuaApiCategory.System,
                    "开启异常抛出（默认行为）。",
                    "Enable throwing exceptions (default).",
                    "rainy.openexception()"),
                E("throwexception", "rainy.throwexception([msg])", LuaApiCategory.System,
                    "主动抛出异常并中断脚本。",
                    "Throw an exception and abort the script.",
                    "rainy.throwexception(\"error\")"),
                E("getversion", "rainy.getversion()", LuaApiCategory.System,
                    "获取 PopStudio 版本号。",
                    "Get the PopStudio version number.",
                    "print(rainy.getversion())"),
                E("getsystem", "rainy.getsystem()", LuaApiCategory.System,
                    "获取当前运行系统类型编号。",
                    "Get the current system type id.",
                    "print(rainy.getsystem())"),
                E("getlanguage", "rainy.getlanguage()", LuaApiCategory.System,
                    "获取当前界面语言编号。",
                    "Get the current UI language id.",
                    "print(rainy.getlanguage())"),
                E("openurl", "rainy.openurl(url)", LuaApiCategory.System,
                    "用系统默认方式打开 URL。",
                    "Open a URL with the system default handler.",
                    "rainy.openurl(\"https://github.com/PopGameTool/PopStudio\")"),
                E("sleep", "rainy.sleep(ms)", LuaApiCategory.System,
                    "休眠指定毫秒数。",
                    "Sleep for the given milliseconds.",
                    "rainy.sleep(1000)"),
                E("httpget", "rainy.httpget(url)", LuaApiCategory.System,
                    "HTTP GET 并返回响应文本。",
                    "HTTP GET and return response text.",
                    "local t = rainy.httpget(\"https://github.com/PopGameTool/PopStudio\")"),
                E("getansiname", "rainy.getansiname()", LuaApiCategory.System,
                    "获取当前 ANSI 编码名称。",
                    "Get the current ANSI encoding name.",
                    "print(rainy.getansiname())"),

                // Global
                E("print", "print(...)", LuaApiCategory.Global,
                    "向输出区打印内容（可多个参数）。",
                    "Print values to the output panel.",
                    "print(\"hello\")"),
                E("args", "args", LuaApiCategory.Global,
                    "通过 rainy.dofile 传入的参数表（页面直接运行时通常不存在）。",
                    "Args table from rainy.dofile (usually absent when running in the page).",
                    "local a = args and args[1]"),

                // Stream helpers
                E("BinaryStream.Position", "stream.Position", LuaApiCategory.Stream,
                    "当前读写位置（可读写）。",
                    "Current read/write position (get/set).",
                    "s.Position = 0"),
                E("BinaryStream.Length", "stream.Length", LuaApiCategory.Stream,
                    "流长度（可读写）。",
                    "Stream length (get/set).",
                    "print(s.Length)"),
                E("BinaryStream.ReadByte", "stream:ReadByte()", LuaApiCategory.Stream,
                    "读取一个字节。",
                    "Read one byte.",
                    "local b = s:ReadByte()"),
                E("BinaryStream.ReadInt32", "stream:ReadInt32([endian])", LuaApiCategory.Stream,
                    "读取 32 位整数。",
                    "Read a 32-bit integer.",
                    "local n = s:ReadInt32()"),
                E("BinaryStream.ReadString", "stream:ReadString(count [, endian])", LuaApiCategory.Stream,
                    "按长度读取字符串。",
                    "Read a string of the given length.",
                    "local str = s:ReadString(4)"),
                E("BinaryStream.WriteByte", "stream:WriteByte(value)", LuaApiCategory.Stream,
                    "写入一个字节。",
                    "Write one byte.",
                    "s:WriteByte(0)"),
                E("BinaryStream.WriteInt32", "stream:WriteInt32(value [, endian])", LuaApiCategory.Stream,
                    "写入 32 位整数。",
                    "Write a 32-bit integer.",
                    "s:WriteInt32(0)"),
                E("BinaryStream.WriteString", "stream:WriteString(value [, endian])", LuaApiCategory.Stream,
                    "写入字符串。",
                    "Write a string.",
                    "s:WriteString(\"text\")"),
                E("BinaryStream.Dispose", "stream:Dispose()", LuaApiCategory.Stream,
                    "关闭并释放流。",
                    "Close and dispose the stream.",
                    "s:Dispose()"),
                E("TempFilePool.Add", "pool:Add()", LuaApiCategory.Stream,
                    "创建临时文件并加入池，返回路径。",
                    "Create a temp file in the pool and return its path.",
                    "local p = pool:Add()"),
                E("TempFilePool.Remove", "pool:Remove(path)", LuaApiCategory.Stream,
                    "从池中移除并删除指定临时文件。",
                    "Remove and delete a temp file from the pool.",
                    "pool:Remove(p)"),
                E("TempFilePool.Clear", "pool:Clear()", LuaApiCategory.Stream,
                    "清空并删除池中全部临时文件。",
                    "Clear and delete all temp files in the pool.",
                    "pool:Clear()"),
            };
        }

        static LuaApiEntry E(string name, string signature, LuaApiCategory category, string zh, string en, string snippet)
            => new LuaApiEntry(name, signature, category, zh, en, snippet);
    }
}
