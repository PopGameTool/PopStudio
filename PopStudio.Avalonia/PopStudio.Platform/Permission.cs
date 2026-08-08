using System.Diagnostics;
using System.IO;

namespace PopStudio.Platform
{
    internal static class Permission
    {
        public static void OpenUrl(string url)
        {
#if LINUX
            Process.Start("xdg-open", url);
#elif MACOS
            Process.Start("open", url);
#else
            Process.Start(new ProcessStartInfo(url.Replace("&", "^&")) { UseShellExecute = true });
#endif
        }

        public static void OpenFolder(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            try
            {
                if (File.Exists(path))
                {
#if LINUX
                    string folder = Path.GetDirectoryName(path);
                    if (!string.IsNullOrEmpty(folder))
                    {
                        Process.Start("xdg-open", folder);
                    }
#elif MACOS
                    Process.Start("open", "-R \"" + path + "\"");
#else
                    Process.Start(new ProcessStartInfo("explorer.exe", "/select,\"" + path + "\"")
                    {
                        UseShellExecute = true
                    });
#endif
                    return;
                }

                string target = path;
                if (!Directory.Exists(target))
                {
                    target = Path.GetDirectoryName(path);
                }

                if (string.IsNullOrEmpty(target) || !Directory.Exists(target))
                {
                    return;
                }

#if LINUX
                Process.Start("xdg-open", target);
#elif MACOS
                Process.Start("open", target);
#else
                Process.Start(new ProcessStartInfo("explorer.exe", "\"" + target + "\"")
                {
                    UseShellExecute = true
                });
#endif
            }
            catch
            {
            }
        }

        public static string GetSettingPath() => "setting.xml";
    }
}