namespace PopStudio.Constant
{
    internal class ZHCN : ILanguage
    {
        public string EndOfFile => "无法在流的结尾之外读取";

        public string VarIntTooBig => "变长整数过大无法读取";

        public string DataMisMatch => "数据不匹配";

        public string AppAuthor => "萌新迎风听雨";

        public string FileNotFound => "文件{0}不存在";

        public string FolderNotFound => "文件夹{0}不存在";

        public string XmemCompressInvalid => "当前PAK实现尚不支持Xmem压缩与PCVersion或ZlibCompress同时启用";

        public string UnknownFormat => "不支持的格式";

        public string TypeMisMatch => "类型不匹配";

        public string EncryptPSVPak => "数据不匹配，如果你解包加密的PSV版pak，请先使用其他工具进行解密";
    }
}
