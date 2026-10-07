namespace PopStudio.Constant
{
    internal class ENUS : ILanguage
    {
        public string EndOfFile => "Cannot read out of stream";

        public string VarIntTooBig => "The varint is too big to read";

        public string DataMisMatch => "The data is mismatched";

        public string AppAuthor => "yingfengtingyu";

        public string FileNotFound => "File {0} is not exist";

        public string FolderNotFound => "Folder {0} is not exist";

        public string XmemCompressInvalid => "The current PAK implementation does not support combining Xmem compression with PCVersion or ZlibCompress";

        public string UnknownFormat => "Unknown format";

        public string TypeMisMatch => "The type is mismatched";

        public string EncryptPSVPak => "The data is mismatched. If you unpack encrypt pak in PSV, you should use other tools to decrypt it.";
    }
}
