using Avalonia.Controls;
using Avalonia.Threading;
using PopStudio.Avalonia;
using System.Text;

namespace PopStudio.Platform
{
    internal class AvaloniaAPI : YFAPI
    {
        TextBox box;

        public override void InternalLoadTextBox(object o)
        {
            if (o is TextBox b)
            {
                box = b;
            }
        }

        public override void InternalPrint(params object[] os)
        {
            StringBuilder str = new StringBuilder();
            if (os.Length != 0)
            {
                string nil = "nil";
                for (int i = 0; i < os.Length; i++)
                {
                    str.Append((os[i]?.ToString()) ?? nil);
                    str.Append(' ');
                }
                str.Remove(str.Length - 1, 1);
            }
            str.Append('\n');
            Dispatcher.UIThread.InvokeAsync(() => box.Text += str);
        }

        public override bool? InternalAlert(string text, string title, bool ask)
        {
            bool? ans = null;
            bool a = false;
            if (ask)
            {
                Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    ans = await PopupDialog.DisplayAlert(title, text, "OK\0", "Cancel\0");
                    a = true;
                });
            }
            else
            {
                Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await PopupDialog.DisplayAlert(title, text, "Cancel\0");
                    a = true;
                });
            }
            while (!a) Thread.Sleep(200);
            return ans;
        }

        public override string InternalPrompt(string text, string title, string defaulttext)
        {
            string ans = string.Empty;
            bool a = false;
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                ans = await PopupDialog.DisplayPromptAsync(title, text, initialValue: defaulttext);
                a = true;
            });
            while (!a) Thread.Sleep(200);
            return ans;
        }

        public override string InternalSheet(string title, params string[] items)
        {
            string ans = null;
            bool a = false;
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                ans = await PopupDialog.DisplayActionSheet(title, "Cancel\0", "OK\0", items);
                a = true;
            });
            while (!a) Thread.Sleep(200);
            if (ans == "OK\0" && ans == "Cancel\0")
            {
                return null;
            }
            return ans;
        }

        public override string InternalChooseFolder()
        {
            string ans = null;
            bool a = false;
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                try
                {
                    ans = await StorageDialog.OpenFolderAsync();
                }
                catch (Exception)
                {
                    ans = null;
                }
                a = true;
            });
            while (!a) Thread.Sleep(200);
            return ans;
        }

        public override string InternalChooseOpenFile()
        {
            string ans = null;
            bool a = false;
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                try
                {
                    ans = await StorageDialog.OpenFileAsync();
                }
                catch (Exception)
                {
                    ans = null;
                }
                a = true;
            });
            while (!a) Thread.Sleep(200);
            return ans;
        }

        public override string InternalChooseSaveFile()
        {
            string ans = null;
            bool a = false;
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                try
                {
                    ans = await StorageDialog.SaveFileAsync();
                }
                catch (Exception)
                {
                    ans = null;
                }
                a = true;
            });
            while (!a) Thread.Sleep(200);
            return ans;
        }

        public override void InternalOpenUrl(string url) => Permission.OpenUrl(url);
    }
}

