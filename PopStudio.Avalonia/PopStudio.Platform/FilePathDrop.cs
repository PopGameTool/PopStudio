using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using System.Linq;

namespace PopStudio.Platform
{
    internal static class FilePathDrop
    {
        public static void Enable(params TextBox[] textBoxes)
        {
            foreach (TextBox textBox in textBoxes)
            {
                DragDrop.SetAllowDrop(textBox, true);
                textBox.AddHandler(DragDrop.DragEnterEvent, DragOver);
                textBox.AddHandler(DragDrop.DragOverEvent, DragOver);
                textBox.AddHandler(DragDrop.DropEvent, Drop);
            }
        }

        /// <summary>
        /// 允许多个文件与文件夹一起拖入，本地路径列表回调给调用方（供多选输入框使用）。
        /// 单路径的 <see cref="Enable"/> 保持不动，已接入的页面行为不变。
        /// </summary>
        public static void EnableMulti(TextBox textBox, Action<IReadOnlyList<string>> onPaths)
        {
            DragDrop.SetAllowDrop(textBox, true);
            textBox.AddHandler(DragDrop.DragEnterEvent, (sender, e) => DragOverMulti(textBox, e));
            textBox.AddHandler(DragDrop.DragOverEvent, (sender, e) => DragOverMulti(textBox, e));
            textBox.AddHandler(DragDrop.DropEvent, (sender, e) => DropMulti(textBox, e, onPaths));
        }

        /// <summary>
        /// 让一大块区域接收拖入（用于「把图拖进来」这类快捷操作）。
        /// 与文本框版本共用路径提取，但不移动焦点与光标。
        /// </summary>
        public static void EnableMulti(Control control, Action<IReadOnlyList<string>> onPaths)
        {
            DragDrop.SetAllowDrop(control, true);
            control.AddHandler(DragDrop.DragEnterEvent, (sender, e) => DragOverMulti(control, e));
            control.AddHandler(DragDrop.DragOverEvent, (sender, e) => DragOverMulti(control, e));
            control.AddHandler(DragDrop.DropEvent, (sender, e) => DropMulti(control, e, onPaths));
        }

        private static List<string> GetPaths(Control control, DragEventArgs e)
        {
            if (!control.IsEffectivelyEnabled)
            {
                return null;
            }
            if (control is TextBox textBox && textBox.IsReadOnly)
            {
                return null;
            }

            IEnumerable<IStorageItem> files = e.Data.GetFiles();
            if (files == null)
            {
                return null;
            }

            List<string> ans = new List<string>();
            foreach (IStorageItem item in files)
            {
                string path = item.TryGetLocalPath();
                if (!string.IsNullOrEmpty(path))
                {
                    ans.Add(path);
                }
            }
            return ans.Count == 0 ? null : ans;
        }

        private static void DragOverMulti(Control control, DragEventArgs e)
        {
            e.DragEffects = GetPaths(control, e) == null
                ? DragDropEffects.None
                : e.DragEffects & DragDropEffects.Copy;
            e.Handled = true;
        }

        private static void DropMulti(Control control, DragEventArgs e, Action<IReadOnlyList<string>> onPaths)
        {
            List<string> paths = GetPaths(control, e);
            e.DragEffects = paths == null
                ? DragDropEffects.None
                : e.DragEffects & DragDropEffects.Copy;
            e.Handled = true;
            if (paths == null)
            {
                return;
            }

            if (onPaths != null)
            {
                onPaths(paths);
            }
            if (control is TextBox textBox)
            {
                textBox.Focus();
                textBox.CaretIndex = textBox.Text == null ? 0 : textBox.Text.Length;
            }
        }

        private static string GetPath(TextBox textBox, DragEventArgs e)
        {
            if (textBox.IsReadOnly || !textBox.IsEffectivelyEnabled)
            {
                return null;
            }

            // A path field holds one item, even if multiple files are dragged in.
            // TryGetLocalPath also decodes file URIs without losing spaces or Unicode.
            return e.Data.GetFiles()?.FirstOrDefault()?.TryGetLocalPath();
        }

        private static void DragOver(object sender, DragEventArgs e)
        {
            string path = GetPath((TextBox)sender, e);
            e.DragEffects = string.IsNullOrEmpty(path)
                ? DragDropEffects.None
                : e.DragEffects & DragDropEffects.Copy;
            e.Handled = true;
        }

        private static void Drop(object sender, DragEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string path = GetPath(textBox, e);
            e.DragEffects = string.IsNullOrEmpty(path)
                ? DragDropEffects.None
                : e.DragEffects & DragDropEffects.Copy;
            e.Handled = true;
            if (e.DragEffects == DragDropEffects.None)
            {
                return;
            }

            textBox.Text = path;
            textBox.Focus();
            textBox.CaretIndex = path.Length;
        }
    }
}
