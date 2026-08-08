using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Media;
using PopStudio.Language.Languages;
using PopStudio.Platform;

namespace PopStudio.Avalonia.Drop
{
    /// <summary>
    /// Show finish/error status with an optional clickable output path that opens the folder.
    /// </summary>
    internal static class FinishStatus
    {
        static readonly ConditionalWeakTable<TextBlock, object> Hooked = new ConditionalWeakTable<TextBlock, object>();
        static readonly IBrush PathBrush = SolidColorBrush.Parse("#1565C0");

        public static void Set(TextBlock target, string message)
        {
            if (target == null)
            {
                return;
            }

            EnsureHook(target);
            ClearPath(target);
            target.Text = message ?? string.Empty;
        }

        public static void ShowFinish(TextBlock target, object time, string path)
        {
            Show(target, string.Format(MAUIStr.Obj.Share_Finish, time), path);
        }

        public static void ShowWrong(TextBlock target, string err)
        {
            Set(target, string.Format(MAUIStr.Obj.Share_Wrong, err));
        }

        public static void Show(TextBlock target, string message, string path)
        {
            if (target == null)
            {
                return;
            }

            EnsureHook(target);
            if (string.IsNullOrWhiteSpace(path))
            {
                Set(target, message);
                return;
            }

            target.Inlines.Clear();
            target.Inlines.Add(new Run(message ?? string.Empty));
            target.Inlines.Add(new LineBreak());
            target.Inlines.Add(new Run(path)
            {
                TextDecorations = TextDecorations.Underline,
                Foreground = PathBrush
            });
            target.Tag = path;
            target.Cursor = new Cursor(StandardCursorType.Hand);
            ToolTip.SetTip(target, MAUIStr.Obj.Share_ClickOpenFolder);
        }

        static void ClearPath(TextBlock target)
        {
            target.Tag = null;
            target.Cursor = Cursor.Default;
            ToolTip.SetTip(target, null);
        }

        static void EnsureHook(TextBlock target)
        {
            if (Hooked.TryGetValue(target, out _))
            {
                return;
            }

            Hooked.Add(target, null);
            target.PointerPressed += OnPointerPressed;
        }

        static void OnPointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (sender is not TextBlock tb || tb.Tag is not string path || string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            Permission.OpenFolder(path);
            e.Handled = true;
        }
    }
}
