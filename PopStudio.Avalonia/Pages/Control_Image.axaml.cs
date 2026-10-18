using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using PopStudio.Image.Convert;
using PopStudio.Language.Languages;
using PopStudio.Platform;
using System.Diagnostics;
using System.Text;

namespace PopStudio.Avalonia.Pages
{
    public partial class Control_Image : UserControl
    {
        /// <summary>当前批量任务的取消源，空闲时为 null</summary>
        CancellationTokenSource m_cancel;

        public Control_Image()
        {
            InitializeComponent();
            LoadControl();
            FilePathDrop.EnableMulti(textbox_input, OnFilesDropped);
            // 整个页面也能接收拖入：拖一张图进来就直接问要不要转换
            FilePathDrop.EnableMulti(this, OnFilesDropped);
            // 输出框保持单路径拖入；失败清单只是用来吞掉拖放事件，
            // 否则拖到这两个框上会冒泡到页面，误触发转换提示
            FilePathDrop.Enable(textbox_outdir, textbox_failures);
            // 状态文本只在初始化时设一次，免得切语言把正在跑的进度或结果冲掉
            label_statue.Text = MAUIStr.Obj.Share_RunStatue;
            LoadFont();
            CB_Layout.SelectedIndex = 0;
            CB_Bitmap.SelectedIndex = 0;
            textbox_quality.Text = "90";
            textbox_maxedge.Text = "0";
            textbox_input.TextChanged += Input_Changed;
            CB_Bitmap.SelectionChanged += Bitmap_Selected;
            RefreshVisible();
            MAUIStr.OnLanguageChanged += LoadFont;
        }

        ~Control_Image()
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
            label_input = this.Get<TextBlock>("label_input");
            label_inputcount = this.Get<TextBlock>("label_inputcount");
            label_bitmap = this.Get<TextBlock>("label_bitmap");
            label_quality = this.Get<TextBlock>("label_quality");
            label_maxedge = this.Get<TextBlock>("label_maxedge");
            label_recursive = this.Get<TextBlock>("label_recursive");
            label_outdir = this.Get<TextBlock>("label_outdir");
            label_layout = this.Get<TextBlock>("label_layout");
            label_overwrite = this.Get<TextBlock>("label_overwrite");
            label_statue = this.Get<TextBlock>("label_statue");
            label_failures = this.Get<TextBlock>("label_failures");
            textbox_input = this.Get<TextBox>("textbox_input");
            textbox_outdir = this.Get<TextBox>("textbox_outdir");
            textbox_quality = this.Get<TextBox>("textbox_quality");
            textbox_maxedge = this.Get<TextBox>("textbox_maxedge");
            textbox_failures = this.Get<TextBox>("textbox_failures");
            button_inputfile = this.Get<Button>("button_inputfile");
            button_inputfolder = this.Get<Button>("button_inputfolder");
            button_outdir = this.Get<Button>("button_outdir");
            button_run = this.Get<Button>("button_run");
            button_cancel = this.Get<Button>("button_cancel");
            CB_Bitmap = this.Get<ComboBox>("CB_Bitmap");
            CB_Layout = this.Get<ComboBox>("CB_Layout");
            TB_Recursive = this.Get<ToggleSwitch>("TB_Recursive");
            TB_Overwrite = this.Get<ToggleSwitch>("TB_Overwrite");
            progress = this.Get<ProgressBar>("progress");
            SP_Bitmap = this.Get<StackPanel>("SP_Bitmap");
            SP_Quality = this.Get<StackPanel>("SP_Quality");
            SP_MaxEdge = this.Get<StackPanel>("SP_MaxEdge");
            SP_Layout = this.Get<StackPanel>("SP_Layout");
        }

        void LoadFont()
        {
            textbox_input.Watermark = MAUIStr.Obj.Share_DropPathHint;
            textbox_outdir.Watermark = MAUIStr.Obj.Share_DropPathHint;
            label_introduction.Text = MAUIStr.Obj.Image_Introduction;
            label_input.Text = MAUIStr.Obj.Image_Input;
            label_bitmap.Text = MAUIStr.Obj.Image_BitmapFormat;
            label_quality.Text = MAUIStr.Obj.Image_JpegQuality;
            label_maxedge.Text = MAUIStr.Obj.Image_MaxEdge;
            label_recursive.Text = MAUIStr.Obj.Image_Recursive;
            label_outdir.Text = MAUIStr.Obj.Image_OutputDir;
            label_layout.Text = MAUIStr.Obj.Image_Layout;
            label_overwrite.Text = MAUIStr.Obj.Image_Overwrite;
            label_failures.Text = MAUIStr.Obj.Image_Failures;
            button_inputfile.Content = MAUIStr.Obj.Image_InputFile;
            button_inputfolder.Content = MAUIStr.Obj.Image_InputFolder;
            button_outdir.Content = MAUIStr.Obj.Share_Choose;
            button_run.Content = MAUIStr.Obj.Share_Run;
            button_cancel.Content = MAUIStr.Obj.Image_Cancel;
            // 下拉里的文字随语言变，用下标原地替换以免重置已选项
            SetItem(CB_Layout.Items, 0, MAUIStr.Obj.Image_Layout0);
            SetItem(CB_Layout.Items, 1, MAUIStr.Obj.Image_Layout1);
            for (int i = 0; i < ImageConvertCatalog.BitmapFormats.Count; i++)
            {
                SetItem(CB_Bitmap.Items, i, ImageConvertCatalog.BitmapFormats[i].DisplayName);
            }
            UpdateInputCount();
        }

        static void SetItem(ItemCollection items, int index, string text)
        {
            if (index < items.Count)
            {
                items[index] = text;
            }
            else
            {
                items.Add(text);
            }
        }

        void RefreshVisible()
        {
            SP_Quality.IsVisible = IsJpegSelected();
        }

        bool IsJpegSelected()
        {
            int index = CB_Bitmap.SelectedIndex;
            if (index < 0 || index >= ImageConvertCatalog.BitmapFormats.Count) return false;
            return ImageConvertCatalog.BitmapFormats[index].Format == YFBitmapFormat.Jpeg;
        }

        void Bitmap_Selected(object sender, SelectionChangedEventArgs e) => RefreshVisible();

        void Input_Changed(object sender, TextChangedEventArgs e) => UpdateInputCount();

        void UpdateInputCount()
        {
            if (label_inputcount == null) return;
            List<string> inputs = SplitInputs(textbox_input.Text);
            label_inputcount.Text = inputs.Count > 1 ? string.Format(MAUIStr.Obj.Image_InputCount, inputs.Count) : string.Empty;
        }

        void OnFilesDropped(IReadOnlyList<string> paths)
        {
            if (paths == null || paths.Count == 0) return;
            textbox_input.Text = paths.Count == 1 ? paths[0] : JoinInputs(paths);
            UpdateInputCount();
            // 拖放事件处理完再弹确认框，避免在拖放回调里切换对话框层
            Dispatcher.UIThread.Post(() => PromptQuickConvert(paths));
        }

        /// <summary>
        /// 拖入的图片全是 PNG 或全是 JPG 时，问一句要不要就地转成另一种；
        /// 方向不唯一（混合格式、文件夹、其它文件）就只填输入框，不打扰。
        /// </summary>
        async void PromptQuickConvert(IReadOnlyList<string> paths)
        {
            try
            {
                if (m_cancel != null) return;
                YFBitmapFormat target;
                string targetName;
                if (!TryGetQuickTarget(paths, out target, out targetName)) return;
                // 「就地转换」要求所有文件在同一个目录；跨目录拖入不猜，只填输入框
                string outputDir = GetCommonDirectory(paths);
                if (outputDir == null) return;
                // 让页面上的输出格式跟着提示走，确认后看到的就是实际会用的格式
                SelectBitmapFormat(target);
                bool ok = await PopupDialog.DisplayAlert(
                    MAUIStr.Obj.Image_Title,
                    string.Format(MAUIStr.Obj.Image_DropConvert_Ask, paths.Count, targetName),
                    MAUIStr.Obj.Image_DropConvert_OK,
                    MAUIStr.Obj.Image_Cancel);
                if (!ok || m_cancel != null) return;
                // 快捷转换固定写回源目录、平铺、跳过已存在，不读页面上的输出设置，
                // 免得「拖进来问一句」变成一次摸不着方向的批量任务
                ImageConvertOptions options = new ImageConvertOptions();
                options.Mode = ImageConvertMode.BitmapToBitmap;
                options.BitmapFormat = target;
                options.JpegQuality = ParseInt(textbox_quality.Text, 90);
                options.Layout = ImageOutputLayout.Flat;
                options.Conflict = ImageConflictPolicy.Skip;
                options.IncludeSubdirectories = false;
                options.OutputDirectory = outputDir;
                StartConvert(new List<string>(paths), options);
            }
            catch (Exception)
            {
            }
        }

        static bool TryGetQuickTarget(IReadOnlyList<string> paths, out YFBitmapFormat target, out string targetName)
        {
            bool allPng = true;
            bool allJpeg = true;
            for (int i = 0; i < paths.Count; i++)
            {
                string ext = Path.GetExtension(paths[i]);
                if (!string.Equals(ext, ".png", StringComparison.OrdinalIgnoreCase)) allPng = false;
                if (!string.Equals(ext, ".jpg", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(ext, ".jpeg", StringComparison.OrdinalIgnoreCase)) allJpeg = false;
            }
            if (allPng)
            {
                target = YFBitmapFormat.Jpeg;
                targetName = "JPG";
                return true;
            }
            if (allJpeg)
            {
                target = YFBitmapFormat.Png;
                targetName = "PNG";
                return true;
            }
            target = YFBitmapFormat.Png;
            targetName = null;
            return false;
        }

        /// <summary>所有输入文件是否在同一个目录里；跨目录时返回 null。</summary>
        static string GetCommonDirectory(IReadOnlyList<string> paths)
        {
            string ans = null;
            for (int i = 0; i < paths.Count; i++)
            {
                string directory = Path.GetDirectoryName(paths[i]);
                if (string.IsNullOrEmpty(directory)) return null;
                if (ans == null) ans = directory;
                else if (!string.Equals(ans, directory, StringComparison.OrdinalIgnoreCase)) return null;
            }
            return ans;
        }

        void SelectBitmapFormat(YFBitmapFormat format)
        {
            for (int i = 0; i < ImageConvertCatalog.BitmapFormats.Count; i++)
            {
                if (ImageConvertCatalog.BitmapFormats[i].Format == format)
                {
                    CB_Bitmap.SelectedIndex = i;
                    return;
                }
            }
        }

        private async void ButtonInputFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string[] val = await new OpenFileDialog { AllowMultiple = true }.ShowAsync(MainWindow.Singleten);
                if (val != null && val.Length > 0)
                {
                    textbox_input.Text = val.Length == 1 ? val[0] : JoinInputs(val);
                    UpdateInputCount();
                }
            }
            catch (Exception)
            {
            }
        }

        private async void ButtonInputFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string val = await new OpenFolderDialog().ShowAsync(MainWindow.Singleten);
                if (!string.IsNullOrEmpty(val))
                {
                    textbox_input.Text = val;
                    UpdateInputCount();
                }
            }
            catch (Exception)
            {
            }
        }

        private async void ButtonOutDir_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string val = await new OpenFolderDialog().ShowAsync(MainWindow.Singleten);
                if (!string.IsNullOrEmpty(val)) textbox_outdir.Text = val;
            }
            catch (Exception)
            {
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            CancellationTokenSource cts = m_cancel;
            if (cts != null) cts.Cancel();
        }

        private void ButtonRun_Click(object sender, RoutedEventArgs e)
        {
            List<string> inputs = SplitInputs(textbox_input.Text);
            if (inputs.Count == 0)
            {
                label_statue.Text = MAUIStr.Obj.Image_NeedInput;
                return;
            }
            ImageConvertOptions options = new ImageConvertOptions();
            options.Mode = ImageConvertMode.BitmapToBitmap;
            options.BitmapFormat = GetSelectedBitmapFormat();
            options.JpegQuality = ParseInt(textbox_quality.Text, 90);
            options.MaxEdge = ParseInt(textbox_maxedge.Text, 0);
            options.IncludeSubdirectories = TB_Recursive.IsChecked == true;
            options.Conflict = TB_Overwrite.IsChecked == true ? ImageConflictPolicy.Overwrite : ImageConflictPolicy.Skip;
            options.Layout = CB_Layout.SelectedIndex == 1 ? ImageOutputLayout.Flat : ImageOutputLayout.PreserveRelative;
            string outdir = textbox_outdir.Text == null ? null : textbox_outdir.Text.Trim().Trim('"');
            options.OutputDirectory = string.IsNullOrWhiteSpace(outdir) ? null : outdir;
            StartConvert(inputs, options);
        }

        void StartConvert(List<string> inputs, ImageConvertOptions options)
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            m_cancel = cts;
            button_run.IsEnabled = false;
            button_cancel.IsEnabled = true;
            progress.Value = 0;
            textbox_failures.Text = string.Empty;
            label_statue.Text = MAUIStr.Obj.Share_Running;

            new Thread(new ThreadStart(() =>
            {
                string err = null;
                ImageConvertResult result = null;
                Stopwatch throttle = new Stopwatch();
                throttle.Start();
                long lastPost = 0;
                try
                {
                    result = ImageBatchConverter.Convert(inputs, options, null, cts.Token, p =>
                    {
                        long now = throttle.ElapsedMilliseconds;
                        // 每 100ms 回填一次进度，避免几千个文件把 UI 刷爆
                        if (p.Completed == p.Total || now - lastPost >= 100)
                        {
                            lastPost = now;
                            Dispatcher.UIThread.Post(() =>
                            {
                                progress.Value = p.Total > 0 ? p.Completed * 100d / p.Total : 0;
                                label_statue.Text = string.Format(MAUIStr.Obj.Image_Running, p.Completed, p.Total);
                            });
                        }
                    });
                }
                catch (Exception ex)
                {
                    err = ex.Message;
                }
                throttle.Stop();
                Dispatcher.UIThread.Post(() =>
                {
                    if (result != null)
                    {
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < result.Failures.Count; i++)
                        {
                            ImageConvertFailure failure = result.Failures[i];
                            sb.AppendLine(failure.InputPath + " -> " + (failure.OutputPath ?? "-") + " : " + failure.Message);
                        }
                        textbox_failures.Text = sb.ToString();
                    }
                    if (err == null && result == null) err = MAUIStr.Obj.Share_Wrong;
                    if (err != null)
                    {
                        label_statue.Text = string.Format(MAUIStr.Obj.Share_Wrong, err);
                    }
                    else if (result.Cancelled)
                    {
                        label_statue.Text = MAUIStr.Obj.Image_Cancelled;
                    }
                    else
                    {
                        progress.Value = 100;
                        label_statue.Text = string.Format(MAUIStr.Obj.Image_Result, result.Elapsed.TotalSeconds.ToString("F3"), result.Succeeded, result.Skipped, result.Failed);
                    }
                    m_cancel = null;
                    button_run.IsEnabled = true;
                    button_cancel.IsEnabled = false;
                });
            }))
            { IsBackground = true }.Start();
        }

        YFBitmapFormat GetSelectedBitmapFormat()
        {
            int index = CB_Bitmap.SelectedIndex;
            if (index >= 0 && index < ImageConvertCatalog.BitmapFormats.Count)
            {
                return ImageConvertCatalog.BitmapFormats[index].Format;
            }
            return YFBitmapFormat.Png;
        }

        static int ParseInt(string text, int defaultValue)
        {
            if (string.IsNullOrWhiteSpace(text)) return defaultValue;
            int ans;
            return int.TryParse(text.Trim(), out ans) ? ans : defaultValue;
        }

        /// <summary>
        /// 把多个路径拼成输入框里的一段文本。路径自身带分号时用双引号包起来，
        /// 否则 <see cref="SplitInputs"/> 会把它当成两个路径。
        /// </summary>
        static string JoinInputs(IReadOnlyList<string> paths)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < paths.Count; i++)
            {
                if (i > 0) sb.Append(';');
                bool quote = paths[i].IndexOf(';') >= 0;
                if (quote) sb.Append('"');
                sb.Append(paths[i]);
                if (quote) sb.Append('"');
            }
            return sb.ToString();
        }

        /// <summary>
        /// 输入框里可以是一段路径，也可以是分号（或换行）分隔的多个路径（多选文件与拖入用）。
        /// 路径自带分号时由 <see cref="JoinInputs"/> 加引号包住，所以引号内部的分隔符不作数。
        /// </summary>
        static List<string> SplitInputs(string text)
        {
            List<string> ans = new List<string>();
            if (string.IsNullOrWhiteSpace(text)) return ans;
            StringBuilder sb = new StringBuilder();
            bool quoted = false;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '"')
                {
                    quoted = !quoted;
                    continue;
                }
                if (!quoted && (c == ';' || c == '\r' || c == '\n'))
                {
                    string part = sb.ToString().Trim();
                    if (part.Length > 0) ans.Add(part);
                    sb.Clear();
                    continue;
                }
                sb.Append(c);
            }
            string last = sb.ToString().Trim();
            if (last.Length > 0) ans.Add(last);
            return ans;
        }
    }
}
