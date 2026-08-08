using System;
using System.IO;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using PopStudio.Language.Languages;

namespace PopStudio.Avalonia.Drop
{
    public static class PathHint
    {
        static readonly TimeSpan FlashDuration = TimeSpan.FromSeconds(2.5);
        static readonly ConditionalWeakTable<TextBox, Entry> Map = new ConditionalWeakTable<TextBox, Entry>();

        public static void Attach(TextBox box, Func<PathRule> getRule)
        {
            if (box == null || getRule == null)
            {
                return;
            }

            Entry entry = EnsureEntry(box);
            entry.GetRule = getRule;
            entry.Update();
        }

        public static void Refresh(TextBox box)
        {
            if (box != null && Map.TryGetValue(box, out Entry entry))
            {
                entry.Update();
            }
        }

        /// <summary>
        /// Show reject reason while dragging over; cleared on leave/accept.
        /// </summary>
        public static void ShowDropReject(TextBox box, string message)
        {
            if (box == null || string.IsNullOrEmpty(message))
            {
                return;
            }

            Entry entry = EnsureEntry(box);
            entry.StopFlashTimer();
            entry.TemporaryMessage = message;
            entry.Update();
        }

        /// <summary>
        /// Flash reject reason briefly after a rejected drop, then restore path validation.
        /// </summary>
        public static void FlashDropReject(TextBox box, string message)
        {
            if (box == null || string.IsNullOrEmpty(message))
            {
                return;
            }

            Entry entry = EnsureEntry(box);
            entry.TemporaryMessage = message;
            entry.Update();
            entry.StartFlashTimer();
        }

        public static void ClearDropReject(TextBox box)
        {
            if (box == null || !Map.TryGetValue(box, out Entry entry))
            {
                return;
            }

            if (string.IsNullOrEmpty(entry.TemporaryMessage))
            {
                return;
            }

            entry.StopFlashTimer();
            entry.TemporaryMessage = null;
            entry.Update();
        }

        /// <summary>
        /// If a drop-reject message is showing, keep it for FlashDuration then restore validation.
        /// </summary>
        public static void KeepDropRejectBriefly(TextBox box)
        {
            if (box == null || !Map.TryGetValue(box, out Entry entry))
            {
                return;
            }

            if (string.IsNullOrEmpty(entry.TemporaryMessage))
            {
                return;
            }

            entry.StartFlashTimer();
        }

        static Entry EnsureEntry(TextBox box)
        {
            if (Map.TryGetValue(box, out Entry existing))
            {
                return existing;
            }

            TextBlock hint = new TextBlock
            {
                IsVisible = false,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 13,
                Foreground = Brushes.IndianRed,
                Margin = new Thickness(0, 0, 0, 5)
            };

            InsertHint(box, hint);

            Entry entry = new Entry
            {
                Box = box,
                Hint = hint
            };
            Map.Add(box, entry);
            box.TextChanged += (_, __) => entry.Update();
            MAUIStr.OnLanguageChanged += entry.Update;
            return entry;
        }

        static void InsertHint(TextBox box, TextBlock hint)
        {
            if (box.Parent is not Control row || row.Parent is not Panel panel)
            {
                return;
            }

            int index = panel.Children.IndexOf(row);
            if (index < 0)
            {
                return;
            }

            panel.Children.Insert(index + 1, hint);
        }

        sealed class Entry
        {
            public TextBox Box;
            public TextBlock Hint;
            public Func<PathRule> GetRule;
            public string TemporaryMessage;
            public bool Cleaning;
            DispatcherTimer FlashTimer;

            public void Update()
            {
                if (Cleaning)
                {
                    return;
                }

                string raw = Box.Text;
                string cleaned = PathRule.CleanPath(raw);
                if (!string.IsNullOrEmpty(cleaned) && !string.Equals(raw, cleaned, StringComparison.Ordinal))
                {
                    Cleaning = true;
                    try
                    {
                        Box.Text = cleaned;
                    }
                    finally
                    {
                        Cleaning = false;
                    }
                }

                string message = TemporaryMessage;
                if (string.IsNullOrEmpty(message) && GetRule != null)
                {
                    message = Validate(cleaned ?? raw, GetRule());
                }

                if (string.IsNullOrEmpty(message))
                {
                    Hint.IsVisible = false;
                    Hint.Text = string.Empty;
                }
                else
                {
                    Hint.Text = message;
                    Hint.IsVisible = true;
                }
            }

            public void StartFlashTimer()
            {
                StopFlashTimer();
                FlashTimer = new DispatcherTimer { Interval = FlashDuration };
                FlashTimer.Tick += OnFlashTick;
                FlashTimer.Start();
            }

            public void StopFlashTimer()
            {
                if (FlashTimer == null)
                {
                    return;
                }

                FlashTimer.Stop();
                FlashTimer.Tick -= OnFlashTick;
                FlashTimer = null;
            }

            void OnFlashTick(object sender, EventArgs e)
            {
                StopFlashTimer();
                TemporaryMessage = null;
                Update();
            }
        }

        static string Validate(string path, PathRule rule)
        {
            path = PathRule.CleanPath(path);
            if (rule == null || string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            if (path.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                return MAUIStr.Obj.Share_InvalidPath;
            }

            bool isDir = Directory.Exists(path);
            bool isFile = File.Exists(path);

            if (rule.MustExist)
            {
                if (isDir)
                {
                    return rule.AllowDirectory ? null : string.Format(MAUIStr.Obj.Share_FileNotFound, path);
                }

                if (isFile)
                {
                    if (!rule.AllowFile)
                    {
                        return string.Format(MAUIStr.Obj.Share_FolderNotFound, path);
                    }

                    if (!PathRule.MatchesExtension(path, rule.Extensions))
                    {
                        return string.Format(MAUIStr.Obj.Share_ExtensionMismatch, string.Join(", ", rule.Extensions));
                    }

                    return null;
                }

                if (rule.AllowDirectory && !rule.AllowFile)
                {
                    return string.Format(MAUIStr.Obj.Share_FolderNotFound, path);
                }

                return string.Format(MAUIStr.Obj.Share_FileNotFound, path);
            }

            // Save / output path: warn only when an extension is present but wrong.
            if (rule.AllowFile && rule.Extensions != null && PathRule.HasAnyExtension(path) && !isDir
                && !PathRule.MatchesExtension(path, rule.Extensions))
            {
                return string.Format(MAUIStr.Obj.Share_ExtensionMismatch, string.Join(", ", rule.Extensions));
            }

            return null;
        }
    }
}
