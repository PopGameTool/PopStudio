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
