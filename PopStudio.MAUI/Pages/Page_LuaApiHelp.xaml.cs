using System.Text;
using PopStudio.Language.Languages;
using PopStudio.LuaScript;

namespace PopStudio.MAUI
{
    public partial class Page_LuaApiHelp : ContentPage
    {
        readonly Action<string> _insertSnippet;
        readonly Dictionary<LuaApiCategory, View> _sectionAnchors = new Dictionary<LuaApiCategory, View>();
        string _searchKeyword = string.Empty;
        bool _suppressSearchRebuild;
        IDispatcherTimer _copiedHideTimer;

        public Page_LuaApiHelp(Action<string> insertSnippet)
        {
            _insertSnippet = insertSnippet;
            InitializeComponent();
            BuildDocument();
            MAUIStr.OnLanguageChanged += OnLanguageChanged;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MAUIStr.OnLanguageChanged -= OnLanguageChanged;
            if (_copiedHideTimer != null)
            {
                _copiedHideTimer.Stop();
                _copiedHideTimer.Tick -= CopiedHideTimer_Tick;
            }
        }

        void OnLanguageChanged()
        {
            string keep = entry_search.Text ?? string.Empty;
            _suppressSearchRebuild = true;
            BuildDocument();
            entry_search.Text = keep;
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
            label_jump.Text = MAUIStr.Obj.LuaScript_ApiJump;
            entry_search.Placeholder = MAUIStr.Obj.LuaScript_ApiSearch;
            button_copy_all.Text = MAUIStr.Obj.LuaScript_ApiCopyAll;
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
                    Text = categoryName,
                    Tag = category,
                    Padding = new Thickness(12, 4),
                    HeightRequest = 36
                };
                jump.Clicked += JumpButton_Clicked;
                jump_bar.Children.Add(jump);

                VerticalStackLayout section = new VerticalStackLayout { Spacing = 10 };
                Label header = new Label
                {
                    Text = categoryName,
                    FontSize = 20,
                    FontAttributes = FontAttributes.Bold,
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

        View CreateEntryBlock(LuaApiEntry entry, bool chinese)
        {
            Grid grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                },
                Padding = new Thickness(12, 10)
            };

            Label name = new Label
            {
                Text = entry.Name,
                FontSize = 17,
                FontAttributes = FontAttributes.Bold
            };
            Grid.SetColumn(name, 0);
            Grid.SetRow(name, 0);
            grid.Children.Add(name);

            Button insert = new Button
            {
                Text = MAUIStr.Obj.LuaScript_Insert,
                Tag = entry.InsertSnippet,
                Padding = new Thickness(10, 2),
                HeightRequest = 36,
                VerticalOptions = LayoutOptions.Start
            };
            insert.Clicked += InsertButton_Clicked;
            Grid.SetColumn(insert, 1);
            Grid.SetRow(insert, 0);
            grid.Children.Add(insert);

            Label signature = new Label
            {
                Text = entry.Signature,
                FontSize = 14,
                Opacity = 0.9,
                LineBreakMode = LineBreakMode.WordWrap,
                Margin = new Thickness(0, 4, 8, 0)
            };
            Grid.SetColumn(signature, 0);
            Grid.SetRow(signature, 1);
            grid.Children.Add(signature);

            Label summary = new Label
            {
                Text = entry.GetSummary(chinese),
                FontSize = 13,
                Opacity = 0.75,
                LineBreakMode = LineBreakMode.WordWrap,
                Margin = new Thickness(0, 4, 8, 0)
            };
            Grid.SetColumn(summary, 0);
            Grid.SetRow(summary, 2);
            grid.Children.Add(summary);

            Border border = new Border
            {
                StrokeThickness = 1,
                Padding = 0,
                Content = grid
            };
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

        void entry_search_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_suppressSearchRebuild) return;
            _searchKeyword = entry_search.Text ?? string.Empty;
            BuildDocument();
        }

        async void button_copy_all_Clicked(object sender, EventArgs e)
        {
            string text = BuildPlainText();
            if (string.IsNullOrEmpty(text)) return;
            try
            {
                await Clipboard.Default.SetTextAsync(text);
                ShowCopiedHint();
            }
            catch
            {
                // Clipboard may be unavailable or locked by another app; keep the page usable.
            }
        }

        void ShowCopiedHint()
        {
            label_copied.IsVisible = true;
            if (_copiedHideTimer == null)
            {
                _copiedHideTimer = Dispatcher.CreateTimer();
                _copiedHideTimer.Interval = TimeSpan.FromSeconds(1.5);
                _copiedHideTimer.Tick += CopiedHideTimer_Tick;
            }
            _copiedHideTimer.Stop();
            _copiedHideTimer.Start();
        }

        void CopiedHideTimer_Tick(object sender, EventArgs e)
        {
            _copiedHideTimer.Stop();
            label_copied.IsVisible = false;
        }

        async void JumpButton_Clicked(object sender, EventArgs e)
        {
            if (sender is not Button button || button.Tag is not LuaApiCategory category) return;
            if (!_sectionAnchors.TryGetValue(category, out View anchor)) return;
            await doc_scroll.ScrollToAsync(anchor, ScrollToPosition.Start, true);
        }

        void InsertButton_Clicked(object sender, EventArgs e)
        {
            if (sender is not Button button || button.Tag is not string snippet) return;
            _insertSnippet?.Invoke(snippet);
        }
    }
}
