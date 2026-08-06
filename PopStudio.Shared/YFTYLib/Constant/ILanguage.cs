namespace PopStudio.Constant
{
    internal interface ILanguage
    {
        public string EndOfFile { get; }
        public string VarIntTooBig { get; }
        public string DataMisMatch { get; }
        public string AppVersion => "6.2";
        public string AppAuthor { get; }
        public string FileNotFound { get; }
        public string FolderNotFound { get; }
        public string XmemCompressInvalid { get; }
        public string UnknownFormat { get; }

        public string TypeMisMatch { get; }

        public string EncryptPSVPak { get; }
    }
}
