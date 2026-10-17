using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using PopStudio.Language.Languages;
using PopStudio.LuaScript;

namespace PopStudio.Avalonia.Pages
{
    public partial class Window_LuaApiHelp : Window
    {
        readonly Action<string> _insertSnippet;
        readonly Dictionary<LuaApiCategory, Control> _sectionAnchors = new Dictionary<LuaApiCategory, Control>();
        string _searchKeyword = string.Empty;
        bool _suppressSearchRebuild;
        DispatcherTimer _copiedHideTimer;

        public Window_LuaApiHelp(Action<string> insertSnippet)
        {
            _insertSnippet = insertSnippet;
            InitializeComponent();
            LoadControl();
            BuildDocument();
            MAUIStr.OnLanguageChanged += OnLanguageChanged;
            Closed += (_, __) =>
            {
                MAUIStr.OnLanguageChanged -= OnLanguageChanged;
                _copiedHideTimer?.Stop();
            };
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        void LoadControl()
        {
            label_title = this.Get<TextBlock>("label_title");
            label_jump = this.Get<TextBlock>("label_jump");
            label_copied = this.Get<TextBlock>("label_copied");
            button_close = this.Get<Button>("button_close");
            button_copy_all = this.Get<Button>("button_copy_all");
            textbox_search = this.Get<TextBox>("textbox_search");
            jump_bar = this.Get<StackPanel>("jump_bar");
            doc_panel = this.Get<StackPanel>("doc_panel");
            doc_scroll = this.Get<ScrollViewer>("doc_scroll");
        }

        void OnLanguageChanged()
        {
            string keep = textbox_search.Text ?? string.Empty;
            _suppressSearchRebuild = true;
            BuildDocument();
            if (textbox_search.Text != keep) textbox_search.Text = keep;
            _suppressSearchRebuild = false;
        }

        static string GetCategoryName(LuaApiCategory category)
        {
            switch (category)
            {
                case LuaApiCategory.UI: return MAUIStr.Obj.LuaScript_ApiCategory_UI;
                case LuaApiCategory.File: return MAUIStr.Obj.LuaScript_ApiCategory_File;
                case LuaApiCategory.Image: return MAUIStr.Obj.LuaScript_ApiCategory_Image;
                case LuaApiCategory.RTON: return MAUIStr.Obj.LuaScript_ApiCategory_RTON;
                case LuaApiCategory.Animation: return MAUIStr.Obj.LuaScript_ApiCategory_Animation;
                case LuaApiCategory.Compress: return MAUIStr.Obj.LuaScript_ApiCategory_Compress;
                case LuaApiCategory.System: return MAUIStr.Obj.LuaScript_ApiCategory_System;
                case LuaApiCategory.Global: return MAUIStr.Obj.LuaScript_ApiCategory_Global;
                case LuaApiCategory.Stream: return MAUIStr.Obj.LuaScript_ApiCategory_Stream;
                default: return category.ToString();
            }
        }

        static bool EntryMatches(LuaApiEntry entry, string keyword, bool chinese)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return true;
            return ContainsIgnoreCase(entry.Name, keyword)
                || ContainsIgnoreCase(entry.Signature, keyword)
                || ContainsIgnoreCase(entry.GetSummary(chinese), keyword)
                || ContainsIgnoreCase(entry.InsertSnippet, keyword);
        }

        static bool ContainsIgnoreCase(string source, string keyword)
        {
            return !string.IsNullOrEmpty(source)
                && source.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        void BuildDocument()
        {
            Title = MAUIStr.Obj.LuaScript_ApiHelp;
            label_title.Text = MAUIStr.Obj.LuaScript_ApiHelp;
            label_jump.Text = MAUIStr.Obj.LuaScript_ApiJump;
            textbox_search.Watermark = MAUIStr.Obj.LuaScript_ApiSearch;
            button_copy_all.Content = MAUIStr.Obj.LuaScript_ApiCopyAll;
            button_close.Content = MAUIStr.Obj.Setting_OK.Replace("\0", string.Empty);
            label_copied.Text = MAUIStr.Obj.LuaScript_ApiCopied;

            jump_bar.Children.Clear();
            doc_panel.Children.Clear();
            _sectionAnchors.Clear();

            bool chinese = Setting.AppLanguage == Constant.Language.ZHCN;
            string keyword = _searchKeyword?.Trim() ?? string.Empty;

            foreach (LuaApiCategory category in LuaApiCatalog.Categories)
            {
                List<LuaApiEntry> matched = new List<LuaApiEntry>();
                foreach (LuaApiEntry entry in LuaApiCatalog.GetByCategory(category))
                {
                    if (EntryMatches(entry, keyword, chinese)) matched.Add(entry);
                }
                if (matched.Count == 0) continue;

                string categoryName = GetCategoryName(category);

                Button jump = new Button
                {
                    Content = categoryName,
                    Tag = category,
                    MinHeight = 30,
                    Padding = new Thickness(10, 4),
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                jump.Click += JumpButton_Click;
                jump_bar.Children.Add(jump);

                StackPanel section = new StackPanel { Spacing = 10 };
                TextBlock header = new TextBlock
                {
                    Text = categoryName,
                    FontSize = 20,
                    FontWeight = FontWeight.Bold,
                    Margin = new Thickness(0, 4, 0, 2)
                };
                section.Children.Add(header);
                _sectionAnchors[category] = header;

                foreach (LuaApiEntry entry in matched)
                {
                    section.Children.Add(CreateEntryBlock(entry, chinese));
                }

                doc_panel.Children.Add(section);
            }
        }

        Control CreateEntryBlock(LuaApiEntry entry, bool chinese)
        {
            Border border = new Border
            {
                Padding = new Thickness(12, 10),
                CornerRadius = new CornerRadius(4),
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Color.FromArgb(40, 128, 128, 128))
            };

            Grid grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("*,Auto"),
                RowDefinitions = new RowDefinitions("Auto,Auto,Auto")
            };

            TextBlock name = new TextBlock
            {
                Text = entry.Name,
                FontSize = 17,
                FontWeight = FontWeight.SemiBold
            };
            Grid.SetColumn(name, 0);
            Grid.SetRow(name, 0);
            grid.Children.Add(name);

            Button insert = new Button
            {
                Content = MAUIStr.Obj.LuaScript_Insert,
                Tag = entry.InsertSnippet,
                MinWidth = 72,
                MinHeight = 30,
                Padding = new Thickness(10, 2),
                VerticalAlignment = VerticalAlignment.Top,
                Focusable = false
            };
            insert.Click += InsertButton_Click;
            Grid.SetColumn(insert, 1);
            Grid.SetRow(insert, 0);
            grid.Children.Add(insert);

            TextBlock signature = new TextBlock
            {
                Text = entry.Signature,
                FontSize = 14,
                Opacity = 0.9,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 4, 8, 0)
            };
            Grid.SetColumn(signature, 0);
            Grid.SetRow(signature, 1);
            grid.Children.Add(signature);

            TextBlock summary = new TextBlock
            {
                Text = entry.GetSummary(chinese),
                FontSize = 13,
                Opacity = 0.75,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 4, 8, 0)
            };
            Grid.SetColumn(summary, 0);
            Grid.SetRow(summary, 2);
            grid.Children.Add(summary);

            border.Child = grid;
            return border;
        }

        string BuildPlainText()
        {
            bool chinese = Setting.AppLanguage == Constant.Language.ZHCN;
            string keyword = _searchKeyword?.Trim() ?? string.Empty;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(MAUIStr.Obj.LuaScript_ApiHelp);
            sb.AppendLine();

            foreach (LuaApiCategory category in LuaApiCatalog.Categories)
            {
                List<LuaApiEntry> matched = new List<LuaApiEntry>();
                foreach (LuaApiEntry entry in LuaApiCatalog.GetByCategory(category))
                {
                    if (EntryMatches(entry, keyword, chinese)) matched.Add(entry);
                }
                if (matched.Count == 0) continue;

                sb.AppendLine("## " + GetCategoryName(category));
                sb.AppendLine();
                foreach (LuaApiEntry entry in matched)
                {
                    sb.AppendLine(entry.Name);
                    sb.AppendLine(entry.Signature);
                    sb.AppendLine(entry.GetSummary(chinese));
                    sb.AppendLine(entry.InsertSnippet);
                    sb.AppendLine();
                }
            }
            return sb.ToString().TrimEnd();
        }

        void TextBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_suppressSearchRebuild) return;
            _searchKeyword = textbox_search.Text ?? string.Empty;
            BuildDocument();
        }

        async void ButtonCopyAll_Click(object sender, RoutedEventArgs e)
        {
            string text = BuildPlainText();
            if (string.IsNullOrEmpty(text)) return;
            if (Clipboard != null)
            {
                await Clipboard.SetTextAsync(text);
                ShowCopiedHint();
            }
        }

        void ShowCopiedHint()
        {
            label_copied.IsVisible = true;
            if (_copiedHideTimer == null)
            {
                _copiedHideTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1.5) };
                _copiedHideTimer.Tick += (_, __) =>
                {
                    _copiedHideTimer.Stop();
                    label_copied.IsVisible = false;
                };
            }
            _copiedHideTimer.Stop();
            _copiedHideTimer.Start();
        }

        void JumpButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not LuaApiCategory category) return;
            if (!_sectionAnchors.TryGetValue(category, out Control anchor)) return;
            anchor.BringIntoView();
        }

        void InsertButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not string snippet) return;
            _insertSnippet?.Invoke(snippet);
        }

        void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
