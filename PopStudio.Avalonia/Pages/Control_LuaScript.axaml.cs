using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using PopStudio.Language.Languages;
using PopStudio.LuaScript;
using PopStudio.Platform;
#if ENABLE_LUA_EDITOR
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using AvaloniaEdit.CodeCompletion;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using System.Xml;
#endif

namespace PopStudio.Avalonia.Pages
{
    public partial class Control_LuaScript : UserControl
    {
#if ENABLE_LUA_EDITOR
        TextEditor _luaEditor;
        CompletionWindow _completionWindow;
        string _scriptFilePath;
        MenuItem _miUndo;
        MenuItem _miRedo;
        MenuItem _miCut;
        MenuItem _miCopy;
        MenuItem _miPaste;
        MenuItem _miDelete;
        MenuItem _miSelectAll;
        MenuItem _miOpen;
        MenuItem _miSave;
        MenuItem _miSaveAs;
        MenuItem _miClear;
        MenuItem _miInsertFilePath;
        MenuItem _miInsertFolderPath;
        MenuItem _miWordWrap;
        MenuItem _miRun;
        MenuItem _miApiHelp;
#endif
        Window_LuaApiHelp _helpWindow;

        public Control_LuaScript()
        {
            InitializeComponent();
            LoadControl();
            LoadFont();
#if ENABLE_LUA_EDITOR
            InitLuaEditor();
#endif
            MAUIStr.OnLanguageChanged += LoadFont;
            YFAPI.LoadTextBox(richtextbox2);
        }

        ~Control_LuaScript()
        {
            MAUIStr.OnLanguageChanged -= LoadFont;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        void LoadControl()
        {
            label_introduction = this.Get<TextBlock>("label_introduction");
            label_print = this.Get<TextBlock>("label_print");
            button_run = this.Get<Button>("button_run");
            button_api_help = this.Get<Button>("button_api_help");
            richtextbox1 = this.Get<TextBox>("richtextbox1");
            richtextbox2 = this.Get<TextBox>("richtextbox2");
            editor_host = this.Get<Panel>("editor_host");
        }

        void LoadFont()
        {
            label_introduction.Text = MAUIStr.Obj.LuaScript_Introduction;
            label_print.Text = MAUIStr.Obj.LuaScript_TracePrint;
            button_run.Content = MAUIStr.Obj.Share_Run;
            button_api_help.Content = MAUIStr.Obj.LuaScript_ApiHelp;
#if ENABLE_LUA_EDITOR
            ApplyContextMenuLanguage();
#endif
        }

        void ButtonApiHelp_Click(object sender, RoutedEventArgs e)
        {
            if (_helpWindow != null)
            {
                _helpWindow.Activate();
                return;
            }
            Window owner = TopLevel.GetTopLevel(this) as Window;
            _helpWindow = new Window_LuaApiHelp(InsertIntoEditor);
            _helpWindow.Closed += (_, __) => _helpWindow = null;
            if (owner != null) _helpWindow.Show(owner);
            else _helpWindow.Show();
        }

        void InsertIntoEditor(string snippet)
        {
            if (string.IsNullOrEmpty(snippet)) return;
#if ENABLE_LUA_EDITOR
            if (_luaEditor != null)
            {
                TextDocument doc = _luaEditor.Document;
                int offset = _luaEditor.CaretOffset;
                if (offset < 0 || offset > doc.TextLength) offset = doc.TextLength;
                string insert = snippet;
                if (offset > 0)
                {
                    char prev = doc.GetCharAt(offset - 1);
                    if (prev != '\n' && prev != '\r') insert = "\n" + insert;
                }
                doc.Insert(offset, insert);
                _luaEditor.CaretOffset = offset + insert.Length;
                _luaEditor.Focus();
                return;
            }
#endif
            string text = richtextbox1.Text ?? string.Empty;
            int pos = richtextbox1.CaretIndex;
            if (pos < 0 || pos > text.Length) pos = text.Length;
            string toInsert = snippet;
            if (pos > 0 && text[pos - 1] != '\n' && text[pos - 1] != '\r')
            {
                toInsert = "\n" + toInsert;
            }
            richtextbox1.Text = text.Insert(pos, toInsert);
            richtextbox1.CaretIndex = pos + toInsert.Length;
            richtextbox1.Focus();
        }

        string GetEditorText()
        {
#if ENABLE_LUA_EDITOR
            if (_luaEditor != null) return _luaEditor.Text ?? string.Empty;
#endif
            return richtextbox1.Text ?? string.Empty;
        }

        private void ButtonRun_Click(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            b.IsEnabled = false;
            richtextbox2.Text = string.Empty;
            string script = GetEditorText();
            new Thread(new ThreadStart(() =>
            {
                bool cg = true;
                try
                {
                    YFAPI.DoScript(script);
                }
                catch (Exception ex)
                {
                    cg = false;
                    YFAPI.Print(string.Format(MAUIStr.Obj.Share_Wrong, ex.Message));
                }
                if (cg)
                {
                    YFAPI.Print(MAUIStr.Obj.Share_Finish_NoTime);
                }
                Dispatcher.UIThread.InvokeAsync(() => b.IsEnabled = true);
            }))
            { IsBackground = true }.Start();
        }

#if ENABLE_LUA_EDITOR
        void InitLuaEditor()
        {
            EnsureLuaHighlighting();
            _luaEditor = new TextEditor
            {
                FontSize = 16,
                Height = 200,
                ShowLineNumbers = true,
                WordWrap = true,
                SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("Lua")
            };
            _luaEditor.Options.EnableHyperlinks = false;
            _luaEditor.Options.EnableEmailHyperlinks = false;
            _luaEditor.TextArea.TextEntering += LuaEditor_TextEntering;
            _luaEditor.TextArea.TextEntered += LuaEditor_TextEntered;
            DragDrop.SetAllowDrop(_luaEditor, true);
            _luaEditor.AddHandler(DragDrop.DropEvent, LuaEditor_Drop);
            _luaEditor.AddHandler(DragDrop.DragOverEvent, LuaEditor_DragOver);
            _luaEditor.TextArea.ContextMenu = BuildEditorContextMenu();

            richtextbox1.IsVisible = false;
            editor_host.Children.Add(_luaEditor);
        }

        ContextMenu BuildEditorContextMenu()
        {
            _miUndo = CreateMenuItem(MAUIStr.Obj.LuaScript_CtxUndo, (_, __) =>
            {
                if (_luaEditor.CanUndo) _luaEditor.Undo();
            });
            _miRedo = CreateMenuItem(MAUIStr.Obj.LuaScript_CtxRedo, (_, __) =>
            {
                if (_luaEditor.CanRedo) _luaEditor.Redo();
            });
            _miCut = CreateMenuItem(MAUIStr.Obj.LuaScript_CtxCut, (_, __) => _luaEditor.Cut());
            _miCopy = CreateMenuItem(MAUIStr.Obj.LuaScript_CtxCopy, (_, __) => _luaEditor.Copy());
            _miPaste = CreateMenuItem(MAUIStr.Obj.LuaScript_CtxPaste, (_, __) => _luaEditor.Paste());
            _miDelete = CreateMenuItem(MAUIStr.Obj.LuaScript_CtxDelete, (_, __) =>
            {
                if (_luaEditor.SelectionLength > 0) _luaEditor.Delete();
            });
            _miSelectAll = CreateMenuItem(MAUIStr.Obj.LuaScript_CtxSelectAll, (_, __) => _luaEditor.SelectAll());
            _miOpen = CreateMenuItem(MAUIStr.Obj.LuaScript_CtxOpen, async (_, __) => await OpenScriptAsync());
            _miSave = CreateMenuItem(MAUIStr.Obj.LuaScript_CtxSave, async (_, __) => await SaveScriptAsync(false));
            _miSaveAs = CreateMenuItem(MAUIStr.Obj.LuaScript_CtxSaveAs, async (_, __) => await SaveScriptAsync(true));
            _miClear = CreateMenuItem(MAUIStr.Obj.LuaScript_CtxClear, (_, __) =>
            {
                _luaEditor.Clear();
                _scriptFilePath = null;
            });
            _miInsertFilePath = CreateMenuItem(MAUIStr.Obj.LuaScript_CtxInsertFilePath, async (_, __) => await InsertFilePathAsync());
            _miInsertFolderPath = CreateMenuItem(MAUIStr.Obj.LuaScript_CtxInsertFolderPath, async (_, __) => await InsertFolderPathAsync());
            _miWordWrap = new MenuItem
            {
                Header = MAUIStr.Obj.LuaScript_CtxWordWrap,
                ToggleType = MenuItemToggleType.CheckBox,
                IsChecked = _luaEditor.WordWrap
            };
            _miWordWrap.Click += (_, __) =>
            {
                _luaEditor.WordWrap = _miWordWrap.IsChecked == true;
            };
            _miRun = CreateMenuItem(MAUIStr.Obj.Share_Run, (s, e) => ButtonRun_Click(button_run, e));
            _miApiHelp = CreateMenuItem(MAUIStr.Obj.LuaScript_ApiHelp, ButtonApiHelp_Click);

            ContextMenu menu = new ContextMenu();
            menu.Items.Add(_miUndo);
            menu.Items.Add(_miRedo);
            menu.Items.Add(new Separator());
            menu.Items.Add(_miCut);
            menu.Items.Add(_miCopy);
            menu.Items.Add(_miPaste);
            menu.Items.Add(_miDelete);
            menu.Items.Add(new Separator());
            menu.Items.Add(_miSelectAll);
            menu.Items.Add(new Separator());
            menu.Items.Add(_miOpen);
            menu.Items.Add(_miSave);
            menu.Items.Add(_miSaveAs);
            menu.Items.Add(_miClear);
            menu.Items.Add(new Separator());
            menu.Items.Add(_miInsertFilePath);
            menu.Items.Add(_miInsertFolderPath);
            menu.Items.Add(new Separator());
            menu.Items.Add(_miWordWrap);
            menu.Items.Add(new Separator());
            menu.Items.Add(_miRun);
            menu.Items.Add(_miApiHelp);
            menu.Opening += EditorContextMenu_Opening;
            return menu;
        }

        static MenuItem CreateMenuItem(string header, EventHandler<RoutedEventArgs> click)
        {
            MenuItem item = new MenuItem { Header = header };
            item.Click += click;
            return item;
        }

        void ApplyContextMenuLanguage()
        {
            if (_miUndo == null) return;
            _miUndo.Header = MAUIStr.Obj.LuaScript_CtxUndo;
            _miRedo.Header = MAUIStr.Obj.LuaScript_CtxRedo;
            _miCut.Header = MAUIStr.Obj.LuaScript_CtxCut;
            _miCopy.Header = MAUIStr.Obj.LuaScript_CtxCopy;
            _miPaste.Header = MAUIStr.Obj.LuaScript_CtxPaste;
            _miDelete.Header = MAUIStr.Obj.LuaScript_CtxDelete;
            _miSelectAll.Header = MAUIStr.Obj.LuaScript_CtxSelectAll;
            _miOpen.Header = MAUIStr.Obj.LuaScript_CtxOpen;
            _miSave.Header = MAUIStr.Obj.LuaScript_CtxSave;
            _miSaveAs.Header = MAUIStr.Obj.LuaScript_CtxSaveAs;
            _miClear.Header = MAUIStr.Obj.LuaScript_CtxClear;
            _miInsertFilePath.Header = MAUIStr.Obj.LuaScript_CtxInsertFilePath;
            _miInsertFolderPath.Header = MAUIStr.Obj.LuaScript_CtxInsertFolderPath;
            _miWordWrap.Header = MAUIStr.Obj.LuaScript_CtxWordWrap;
            _miRun.Header = MAUIStr.Obj.Share_Run;
            _miApiHelp.Header = MAUIStr.Obj.LuaScript_ApiHelp;
        }

        void EditorContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool hasSelection = _luaEditor.SelectionLength > 0;
            _miUndo.IsEnabled = _luaEditor.CanUndo;
            _miRedo.IsEnabled = _luaEditor.CanRedo;
            _miCut.IsEnabled = hasSelection;
            _miCopy.IsEnabled = hasSelection;
            _miDelete.IsEnabled = hasSelection;
            _miPaste.IsEnabled = true;
            _miWordWrap.IsChecked = _luaEditor.WordWrap;
            _miRun.IsEnabled = button_run.IsEnabled;
        }

        IStorageProvider GetStorageProvider()
        {
            return TopLevel.GetTopLevel(this)?.StorageProvider;
        }

        static FilePickerFileType LuaScriptFileType => new FilePickerFileType("Lua / Text")
        {
            Patterns = new[] { "*.lua", "*.txt" }
        };

        async Task OpenScriptAsync()
        {
            try
            {
                IStorageProvider storage = GetStorageProvider();
                if (storage == null) return;
                IReadOnlyList<IStorageFile> files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = MAUIStr.Obj.LuaScript_CtxOpen,
                    AllowMultiple = false,
                    FileTypeFilter = new[] { LuaScriptFileType }
                });
                if (files == null || files.Count == 0) return;
                IStorageFile file = files[0];
                await using Stream stream = await file.OpenReadAsync();
                using StreamReader reader = new StreamReader(stream);
                string content = await reader.ReadToEndAsync();
                _luaEditor.Text = content;
                _scriptFilePath = file.TryGetLocalPath();
                _luaEditor.Focus();
            }
            catch (Exception ex)
            {
                YFAPI.Print(string.Format(MAUIStr.Obj.Share_Wrong, ex.Message));
            }
        }

        async Task SaveScriptAsync(bool saveAs)
        {
            try
            {
                if (!saveAs && !string.IsNullOrEmpty(_scriptFilePath))
                {
                    await File.WriteAllTextAsync(_scriptFilePath, _luaEditor.Text ?? string.Empty);
                    return;
                }

                IStorageProvider storage = GetStorageProvider();
                if (storage == null) return;
                string suggested = string.IsNullOrEmpty(_scriptFilePath)
                    ? "script.lua"
                    : Path.GetFileName(_scriptFilePath);
                IStorageFile file = await storage.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = MAUIStr.Obj.LuaScript_CtxSaveAs,
                    SuggestedFileName = suggested,
                    DefaultExtension = "lua",
                    FileTypeChoices = new[] { LuaScriptFileType }
                });
                if (file == null) return;
                string text = _luaEditor.Text ?? string.Empty;
                string local = file.TryGetLocalPath();
                if (!string.IsNullOrEmpty(local))
                {
                    await File.WriteAllTextAsync(local, text);
                    _scriptFilePath = local;
                    return;
                }
                await using Stream stream = await file.OpenWriteAsync();
                using StreamWriter writer = new StreamWriter(stream);
                await writer.WriteAsync(text);
            }
            catch (Exception ex)
            {
                YFAPI.Print(string.Format(MAUIStr.Obj.Share_Wrong, ex.Message));
            }
        }

        async Task InsertFilePathAsync()
        {
            IStorageProvider storage = GetStorageProvider();
            if (storage == null) return;
            IReadOnlyList<IStorageFile> files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = MAUIStr.Obj.LuaScript_CtxInsertFilePath,
                AllowMultiple = false
            });
            if (files == null || files.Count == 0) return;
            string path = files[0].TryGetLocalPath();
            if (string.IsNullOrEmpty(path)) return;
            InsertTextAtCaret(ToLuaPathLiteral(path));
        }

        async Task InsertFolderPathAsync()
        {
            IStorageProvider storage = GetStorageProvider();
            if (storage == null) return;
            IReadOnlyList<IStorageFolder> folders = await storage.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = MAUIStr.Obj.LuaScript_CtxInsertFolderPath,
                AllowMultiple = false
            });
            if (folders == null || folders.Count == 0) return;
            string path = folders[0].TryGetLocalPath();
            if (string.IsNullOrEmpty(path)) return;
            InsertTextAtCaret(ToLuaPathLiteral(path));
        }

        void InsertTextAtCaret(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            if (_luaEditor.SelectionLength > 0)
            {
                _luaEditor.Document.Replace(_luaEditor.SelectionStart, _luaEditor.SelectionLength, text);
            }
            else
            {
                int offset = _luaEditor.CaretOffset;
                if (offset < 0 || offset > _luaEditor.Document.TextLength) offset = _luaEditor.Document.TextLength;
                _luaEditor.Document.Insert(offset, text);
                _luaEditor.CaretOffset = offset + text.Length;
            }
            _luaEditor.Focus();
        }

        static string ToLuaPathLiteral(string path)
        {
            string normalized = path.Replace('\\', '/');
            return "\"" + normalized.Replace("\"", "\\\"") + "\"";
        }

        static bool _luaHighlightingLoaded;

        static void EnsureLuaHighlighting()
        {
            if (_luaHighlightingLoaded) return;
            _luaHighlightingLoaded = true;
            using Stream s = AssetLoader.Open(new Uri("avares://PopStudio/Assets/Lua.xshd"));
            using XmlTextReader reader = new XmlTextReader(s);
            IHighlightingDefinition def = HighlightingLoader.Load(reader, HighlightingManager.Instance);
            HighlightingManager.Instance.RegisterHighlighting("Lua", new[] { ".lua" }, def);
        }

        void LuaEditor_DragOver(object sender, DragEventArgs e)
        {
            e.DragEffects = e.Data.Contains(DataFormats.Files) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        async void LuaEditor_Drop(object sender, DragEventArgs e)
        {
            try
            {
                if (!e.Data.Contains(DataFormats.Files)) return;
                IEnumerable<IStorageItem> files = e.Data.GetFiles();
                if (files == null) return;
                foreach (IStorageItem item in files)
                {
                    if (item is not IStorageFile file) continue;
                    string name = file.Name ?? string.Empty;
                    string ext = Path.GetExtension(name).ToLowerInvariant();
                    if (ext != ".lua" && ext != ".txt") continue;
                    await using Stream stream = await file.OpenReadAsync();
                    using StreamReader reader = new StreamReader(stream);
                    string content = await reader.ReadToEndAsync();
                    _luaEditor.Text = content;
                    _scriptFilePath = file.TryGetLocalPath();
                    break;
                }
            }
            catch (Exception ex)
            {
                YFAPI.Print(string.Format(MAUIStr.Obj.Share_Wrong, ex.Message));
            }
        }

        void LuaEditor_TextEntering(object sender, TextInputEventArgs e)
        {
            if (e.Text != null && e.Text.Length > 0 && _completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]) && e.Text[0] != '_')
                {
                    _completionWindow.CompletionList.RequestInsertion(e);
                }
            }
        }

        void LuaEditor_TextEntered(object sender, TextInputEventArgs e)
        {
            if (e.Text == ".")
            {
                TryShowRainyCompletion();
                return;
            }
            if (e.Text != null && e.Text.Length == 1 && (char.IsLetter(e.Text[0]) || e.Text[0] == '_'))
            {
                TryShowIdentifierCompletion();
            }
        }

        string GetIdentifierBeforeCaret()
        {
            int offset = _luaEditor.CaretOffset;
            DocumentLine line = _luaEditor.Document.GetLineByOffset(offset);
            string text = _luaEditor.Document.GetText(line.Offset, offset - line.Offset);
            int i = text.Length - 1;
            while (i >= 0 && (char.IsLetterOrDigit(text[i]) || text[i] == '_')) i--;
            return text.Substring(i + 1);
        }

        bool IsRainyDotContext()
        {
            int offset = _luaEditor.CaretOffset;
            if (offset < 6) return false;
            string before = _luaEditor.Document.GetText(Math.Max(0, offset - 16), Math.Min(16, offset));
            return before.EndsWith("rainy.", StringComparison.Ordinal);
        }

        void TryShowRainyCompletion()
        {
            if (!IsRainyDotContext()) return;
            ShowCompletion(LuaApiCatalog.GetRainyApis(), 0);
        }

        void TryShowIdentifierCompletion()
        {
            string prefix = GetIdentifierBeforeCaret();
            if (string.IsNullOrEmpty(prefix)) return;
            // Already open: AvaloniaEdit filters from StartOffset as the user keeps typing.
            if (_completionWindow != null) return;
            IEnumerable<LuaApiEntry> source = IsTypingRainyMember(prefix)
                ? LuaApiCatalog.GetRainyApis()
                : LuaApiCatalog.All;
            List<LuaApiEntry> matched = new List<LuaApiEntry>();
            foreach (LuaApiEntry entry in source)
            {
                string name = entry.Name;
                int dot = name.LastIndexOf('.');
                string shortName = dot >= 0 ? name.Substring(dot + 1) : name;
                if (shortName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                    || name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    matched.Add(entry);
                }
            }
            if (matched.Count == 0) return;
            ShowCompletion(matched, prefix.Length);
        }

        bool IsTypingRainyMember(string prefix)
        {
            int offset = _luaEditor.CaretOffset - prefix.Length;
            if (offset < 6) return false;
            string before = _luaEditor.Document.GetText(Math.Max(0, offset - 16), Math.Min(16, offset));
            return before.EndsWith("rainy.", StringComparison.Ordinal);
        }

        void ShowCompletion(IEnumerable<LuaApiEntry> entries, int replaceLength)
        {
            _completionWindow?.Close();
            _completionWindow = new CompletionWindow(_luaEditor.TextArea);
            // Default StartOffset is CaretOffset; without backing up over the typed
            // prefix, selecting "print" after "p" becomes "pprint".
            if (replaceLength > 0)
            {
                int start = _luaEditor.CaretOffset - replaceLength;
                if (start < 0) start = 0;
                _completionWindow.StartOffset = start;
            }
            IList<ICompletionData> data = _completionWindow.CompletionList.CompletionData;
            bool chinese = Setting.AppLanguage == Constant.Language.ZHCN;
            foreach (LuaApiEntry entry in entries)
            {
                string name = entry.Name;
                int dot = name.LastIndexOf('.');
                string insertName = dot >= 0 ? name.Substring(dot + 1) : name;
                data.Add(new LuaCompletionData(insertName, entry.GetSummary(chinese)));
            }
            if (data.Count == 0)
            {
                _completionWindow = null;
                return;
            }
            _completionWindow.Closed += (_, __) => _completionWindow = null;
            _completionWindow.Show();
        }

        sealed class LuaCompletionData : ICompletionData
        {
            public LuaCompletionData(string text, string description)
            {
                Text = text;
                Description = description;
            }

            public string Text { get; }
            public object Content => Text;
            public object Description { get; }
            public double Priority => 0;
            public IImage Image => null;

            public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
            {
                textArea.Document.Replace(completionSegment, Text);
            }
        }
#endif
    }
}
