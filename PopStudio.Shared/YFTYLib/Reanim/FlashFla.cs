namespace PopStudio.Reanim
{
    internal static class FlashFla
    {
        private const string TemporaryDirectoryPrefix = "PopStudio-Fla-";

        public static Reanim Decode(string inFile)
        {
            if (!File.Exists(inFile))
            {
                throw new FileNotFoundException("The ZIP-XFL archive was not found.", inFile);
            }
            return FlashXflDecoder.DecodeArchive(inFile);
        }

        public static bool IsZipXfl(string inFile) => FlashXflDecoder.IsZipXflFile(inFile);

        public static void Encode(Reanim reanim, string outFile)
        {
            string temporaryDirectory = Path.Combine(Path.GetTempPath(), TemporaryDirectoryPrefix + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(temporaryDirectory);
            try
            {
                FlashXfl.Encode(reanim, temporaryDirectory);
                using FileStream output = new FileStream(outFile, FileMode.Create, FileAccess.Write, FileShare.None);
                using ZipArchive archive = new ZipArchive(output, ZipArchiveMode.Create, false);
                foreach (string file in Directory.GetFiles(temporaryDirectory, "*", SearchOption.AllDirectories).OrderBy(path => path, StringComparer.Ordinal))
                {
                    string entryName = Path.GetRelativePath(temporaryDirectory, file).Replace('\\', '/');
                    ZipArchiveEntry entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
                    using Stream source = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read);
                    using Stream destination = entry.Open();
                    source.CopyTo(destination);
                }
            }
            finally
            {
                DeleteTemporaryDirectory(temporaryDirectory);
            }
        }

        private static void DeleteTemporaryDirectory(string path)
        {
            string fullPath = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string tempRoot = Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            StringComparison comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            if (fullPath.StartsWith(tempRoot, comparison)
                && Path.GetFileName(fullPath).StartsWith(TemporaryDirectoryPrefix, StringComparison.Ordinal)
                && Directory.Exists(fullPath))
            {
                Directory.Delete(fullPath, true);
            }
        }
    }
}
