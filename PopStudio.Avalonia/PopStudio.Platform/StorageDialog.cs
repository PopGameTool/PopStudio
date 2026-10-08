using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using PopStudio.Avalonia;
using PopStudio.Avalonia.Drop;

namespace PopStudio.Platform
{
    internal static class StorageDialog
    {
        public static async Task<string> OpenFileAsync(params string[] extensions)
        {
            IReadOnlyList<IStorageFile> files = await MainWindow.Singleten.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    AllowMultiple = false,
                    FileTypeFilter = BuildOpenFilters(extensions)
                });
            return files.Count > 0 ? files[0].TryGetLocalPath() : null;
        }

        public static async Task<string> OpenFolderAsync()
        {
            IReadOnlyList<IStorageFolder> folders = await MainWindow.Singleten.StorageProvider.OpenFolderPickerAsync(
                new FolderPickerOpenOptions
                {
                    AllowMultiple = false
                });
            return folders.Count > 0 ? folders[0].TryGetLocalPath() : null;
        }

        public static async Task<string> SaveFileAsync(params string[] extensions)
        {
            string[] normalized = PathRule.Normalize(extensions);
            FilePickerSaveOptions options = new FilePickerSaveOptions();
            if (normalized != null && normalized.Length > 0)
            {
                options.DefaultExtension = normalized[0].TrimStart('.');
                options.FileTypeChoices = BuildSaveFilters(normalized);
            }

            IStorageFile file = await MainWindow.Singleten.StorageProvider.SaveFilePickerAsync(options);
            return file?.TryGetLocalPath();
        }

        static List<FilePickerFileType> BuildOpenFilters(string[] extensions)
        {
            string[] normalized = PathRule.Normalize(extensions);
            var list = new List<FilePickerFileType>();
            if (normalized != null && normalized.Length > 0)
            {
                list.Add(new FilePickerFileType(FilterName(normalized))
                {
                    Patterns = normalized.Select(e => "*" + e).ToArray()
                });
            }

            list.Add(FilePickerFileTypes.All);
            return list;
        }

        static List<FilePickerFileType> BuildSaveFilters(string[] normalized)
        {
            return new List<FilePickerFileType>
            {
                new FilePickerFileType(FilterName(normalized))
                {
                    Patterns = normalized.Select(e => "*" + e).ToArray()
                },
                FilePickerFileTypes.All
            };
        }

        static string FilterName(string[] normalized)
        {
            return string.Join(" / ", normalized.Select(e => e.TrimStart('.').ToUpperInvariant()));
        }
    }
}
