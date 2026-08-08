using System;
using System.IO;
using System.Linq;
using PopStudio.Language.Languages;

namespace PopStudio.Avalonia.Drop
{
    public sealed class PathRule
    {
        public bool AllowFile { get; init; } = true;

        public bool AllowDirectory { get; init; }

        /// <summary>
        /// When true, path must already exist as file/folder.
        /// Save targets usually set this false.
        /// </summary>
        public bool MustExist { get; init; } = true;

        /// <summary>
        /// Allowed suffixes including compound ones (e.g. ".pam.json").
        /// null = any extension.
        /// </summary>
        public string[] Extensions { get; init; }

        public static PathRule ExistingFolder() => new PathRule
        {
            AllowFile = false,
            AllowDirectory = true,
            MustExist = true
        };

        public static PathRule ExistingFile(params string[] extensions) => new PathRule
        {
            AllowFile = true,
            AllowDirectory = false,
            MustExist = true,
            Extensions = Normalize(extensions)
        };

        public static PathRule ExistingFileOrFolder(params string[] extensions) => new PathRule
        {
            AllowFile = true,
            AllowDirectory = true,
            MustExist = true,
            Extensions = Normalize(extensions)
        };

        public static PathRule SaveFile(params string[] extensions) => new PathRule
        {
            AllowFile = true,
            AllowDirectory = true,
            MustExist = false,
            Extensions = Normalize(extensions)
        };

        public static PathRule SaveFolder() => new PathRule
        {
            AllowFile = false,
            AllowDirectory = true,
            MustExist = false
        };

        public static string[] Normalize(string[] extensions)
        {
            if (extensions == null || extensions.Length == 0)
            {
                return null;
            }

            return extensions
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Select(e =>
                {
                    string v = e.Trim();
                    return v.StartsWith('.') ? v.ToLowerInvariant() : "." + v.ToLowerInvariant();
                })
                .Distinct()
                .ToArray();
        }

        /// <summary>
        /// Strip whitespace/quotes (Explorer "Copy as path") and unify / and \ separators.
        /// </summary>
        public static string CleanPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return path;
            }

            path = path.Trim();
            while (path.Length >= 2)
            {
                char start = path[0];
                char end = path[path.Length - 1];
                if ((start == '"' && end == '"') || (start == '\'' && end == '\''))
                {
                    path = path.Substring(1, path.Length - 2).Trim();
                    continue;
                }

                break;
            }

            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            char sep = Path.DirectorySeparatorChar;
            char alt = Path.AltDirectorySeparatorChar;
            if (sep != alt)
            {
                path = path.Replace(alt, sep);
            }

            bool unc = path.Length >= 2 && path[0] == sep && path[1] == sep;
            bool rooted = path.Length >= 1 && path[0] == sep;
            string[] parts = path.Split(new[] { sep }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                return unc || rooted ? (unc ? new string(sep, 2) : sep.ToString()) : path;
            }

            // Windows drive: keep "C:\..." form.
            if (parts[0].Length == 2 && parts[0][1] == ':')
            {
                if (parts.Length == 1)
                {
                    return parts[0] + (path.EndsWith(sep) ? sep.ToString() : string.Empty);
                }

                return parts[0] + sep + string.Join(sep.ToString(), parts, 1, parts.Length - 1);
            }

            string joined = string.Join(sep.ToString(), parts);
            if (unc)
            {
                return new string(sep, 2) + joined;
            }

            if (rooted)
            {
                return sep + joined;
            }

            return joined;
        }

        public static bool MatchesExtension(string path, string[] extensions)
        {
            if (extensions == null || extensions.Length == 0)
            {
                return true;
            }

            path = CleanPath(path);
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            foreach (string ext in extensions)
            {
                if (path.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool HasAnyExtension(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            string file = Path.GetFileName(CleanPath(path));
            int dot = file.LastIndexOf('.');
            return dot > 0 && dot < file.Length - 1;
        }

        public bool AcceptsPath(string path)
        {
            return GetRejectReason(path) == null;
        }

        public string GetRejectReason(string path)
        {
            path = CleanPath(path);
            if (string.IsNullOrWhiteSpace(path))
            {
                return MAUIStr.Obj.Share_DropRejected;
            }

            if (Directory.Exists(path))
            {
                return AllowDirectory ? null : MAUIStr.Obj.Share_DropNeedFile;
            }

            if (File.Exists(path))
            {
                if (!AllowFile)
                {
                    return MAUIStr.Obj.Share_DropNeedFolder;
                }

                if (!MatchesExtension(path, Extensions))
                {
                    return string.Format(MAUIStr.Obj.Share_ExtensionMismatch, string.Join(", ", Extensions));
                }

                return null;
            }

            // Drag items should exist; typed/save paths may not.
            if (!MustExist && AllowFile)
            {
                if (Extensions == null || !HasAnyExtension(path))
                {
                    return null;
                }

                return MatchesExtension(path, Extensions)
                    ? null
                    : string.Format(MAUIStr.Obj.Share_ExtensionMismatch, string.Join(", ", Extensions));
            }

            return MAUIStr.Obj.Share_DropRejected;
        }
    }
}
