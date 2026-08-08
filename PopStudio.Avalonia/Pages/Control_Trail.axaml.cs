using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using PopStudio.Avalonia.Drop;
using PopStudio.Language.Languages;
using System.Diagnostics;
using PopStudio.Platform;

namespace PopStudio.Avalonia.Pages
{
    public partial class Control_Trail : UserControl
    {
        public Control_Trail()
        {
            InitializeComponent();
            LoadControl();
            LoadFont();
            CB_InMode.Items.Add("PC_Compiled");
            CB_InMode.Items.Add("Phone32_Compiled");
            CB_InMode.Items.Add("Phone64_Compiled");
            CB_InMode.Items.Add("WP_Xnb");
            CB_InMode.Items.Add("GameConsole_Compiled");
            CB_InMode.Items.Add("TV_Compiled");
            CB_InMode.Items.Add("Studio_Json");
            CB_InMode.Items.Add("Raw_Xml");
            CB_InMode.SelectedIndex = 0;
            CB_OutMode.Items.Add("PC_Compiled");
            CB_OutMode.Items.Add("Phone32_Compiled");
            CB_OutMode.Items.Add("Phone64_Compiled");
            CB_OutMode.Items.Add("WP_Xnb");
            CB_OutMode.Items.Add("GameConsole_Compiled");
            CB_OutMode.Items.Add("TV_Compiled");
            CB_OutMode.Items.Add("Studio_Json");
            CB_OutMode.Items.Add("Raw_Xml");
            CB_OutMode.SelectedIndex = 7;
            MAUIStr.OnLanguageChanged += LoadFont;
        }

        ~Control_Trail()
        {
            MAUIStr.OnLanguageChanged -= LoadFont;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        void LoadControl()
        {
            label_batch1 = this.Get<TextBlock>("label_batch1");
            label_batch2 = this.Get<TextBlock>("label_batch2");
            batch_mode = this.Get<ToggleSwitch>("batch_mode");
            label_introduction = this.Get<TextBlock>("label_introduction");
            text1 = this.Get<TextBlock>("text1");
            text2 = this.Get<TextBlock>("text2");
            text_in = this.Get<TextBlock>("text_in");
            text_out = this.Get<TextBlock>("text_out");
            button1 = this.Get<Button>("button1");
            button2 = this.Get<Button>("button2");
            button_run = this.Get<Button>("button_run");
            label_statue = this.Get<TextBlock>("label_statue");
            text4 = this.Get<TextBlock>("text4");
            textbox1 = this.Get<TextBox>("textbox1");
            textbox2 = this.Get<TextBox>("textbox2");
            CB_InMode = this.Get<ComboBox>("CB_InMode");
            CB_OutMode = this.Get<ComboBox>("CB_OutMode");
            DropAccept.AttachPathDrop(textbox1, SetInputFromDrop, () => PathRule.ExistingFileOrFolder(PathExt.TrailByIndex(CB_InMode.SelectedIndex)));
            PathHint.Attach(textbox1, GetInputRule);
            DropAccept.AttachPathBox(textbox2, path => textbox2.Text = path, GetOutputRule);
            CB_InMode.SelectionChanged += (_, __) => PathHint.Refresh(textbox1);
            CB_OutMode.SelectionChanged += (_, __) => PathHint.Refresh(textbox2);
        }

        PathRule GetInputRule()
        {
            return batch_mode.IsChecked == true
                ? PathRule.ExistingFolder()
                : PathRule.ExistingFile(PathExt.TrailByIndex(CB_InMode.SelectedIndex));
        }

        PathRule GetOutputRule()
        {
            return batch_mode.IsChecked == true
                ? PathRule.SaveFolder()
                : PathRule.SaveFile(PathExt.TrailByIndex(CB_OutMode.SelectedIndex));
        }

        void SetInputFromDrop(string path)
        {
            batch_mode.IsChecked = Directory.Exists(path);
            textbox1.Text = path;
            PathHint.Refresh(textbox1);
            PathHint.Refresh(textbox2);
        }

        void LoadFont()
        {
            label_batch1.Text = MAUIStr.Obj.Share_SingleMode;
            label_batch2.Text = MAUIStr.Obj.Share_BatchMode;
            label_introduction.Text = MAUIStr.Obj.Trail_Introduction;
            if (batch_mode.IsChecked == true)
            {
                text1.Text = MAUIStr.Obj.Trail_Choose1_Batch;
                text2.Text = MAUIStr.Obj.Trail_Choose2_Batch;
            }
            else
            {
                text1.Text = MAUIStr.Obj.Trail_Choose1;
                text2.Text = MAUIStr.Obj.Trail_Choose2;
            }
            text_in.Text = MAUIStr.Obj.Trail_InFormat;
            text_out.Text = MAUIStr.Obj.Trail_OutFormat;
            button1.Content = MAUIStr.Obj.Share_Choose;
            button2.Content = MAUIStr.Obj.Share_Choose;
            button_run.Content = MAUIStr.Obj.Share_Run;
            label_statue.Text = MAUIStr.Obj.Share_RunStatue;
            FinishStatus.Set(text4, MAUIStr.Obj.Share_Waiting);
        }

        private void Switch_Batch_Checked(object sender, RoutedEventArgs e)
        {
            LoadFont();
            PathHint.Refresh(textbox1);
            PathHint.Refresh(textbox2);
        }

        private async void Button1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string val = batch_mode.IsChecked == false
                    ? await StorageDialog.OpenFileAsync(PathExt.TrailByIndex(CB_InMode.SelectedIndex))
                    : await StorageDialog.OpenFolderAsync();
                if (!string.IsNullOrEmpty(val)) textbox1.Text = val;
            }
            catch (Exception)
            {
            }
        }

        private async void Button2_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string val = batch_mode.IsChecked == false
                    ? await StorageDialog.SaveFileAsync(PathExt.TrailByIndex(CB_OutMode.SelectedIndex))
                    : await StorageDialog.OpenFolderAsync();
                if (!string.IsNullOrEmpty(val)) textbox2.Text = val;
            }
            catch (Exception)
            {
            }
        }

        private void ButtonRun_Click(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            b.IsEnabled = false;
            FinishStatus.Set(text4, MAUIStr.Obj.Share_Running);
            string inFile = textbox1.Text;
            string outFile = textbox2.Text;
            int inmode = CB_InMode.SelectedIndex;
            int outmode = CB_OutMode.SelectedIndex;
            bool batchmode = batch_mode.IsChecked == true;
            new Thread(new ThreadStart(() =>
            {
                string err = null;
                Stopwatch sw = new Stopwatch();
                sw.Start();
                try
                {
                    string outFormat = outmode switch
                    {
                        0 => ".trail.compiled",
                        1 => ".trail.compiled",
                        2 => ".trail.compiled",
                        3 => ".xnb",
                        4 => ".trail.compiled",
                        5 => ".trail.compiled",
                        6 => ".trail.json",
                        7 => ".trail",
                        _ => null
                    };
                    if (batchmode)
                    {
                        if (!Directory.Exists(inFile))
                        {
                            throw new Exception(string.Format(MAUIStr.Obj.Share_FolderNotFound, inFile));
                        }
                        inFile = YFAPI.FormatPath(inFile);
                        int length = inFile.Length;
                        string[] files = YFAPI.GetFiles(inFile);
                        YFAPI.NewDir(outFile);
                        string rightFormat = inmode switch
                        {
                            0 => ".trail.compiled",
                            1 => ".trail.compiled",
                            2 => ".trail.compiled",
                            3 => ".xnb",
                            4 => ".trail.compiled",
                            5 => ".trail.compiled",
                            6 => ".trail.json",
                            7 => ".trail",
                            _ => null
                        };
                        int rightFormatLength = rightFormat.Length;
                        foreach (string mfile in files)
                        {
                            if (mfile.Length < rightFormatLength || mfile[^rightFormatLength..].ToLower() != rightFormat)
                            {
                                continue;
                            }
                            string newPath = YFAPI.FormatPath(outFile + mfile[length..] + outFormat);
                            YFAPI.NewDir(newPath, false);
                            try
                            {
                                YFAPI.Trail(mfile, newPath, inmode, outmode);
                            }
                            catch (Exception)
                            {
                                File.Delete(newPath);
                            }
                        }
                    }
                    else
                    {
                        if (!File.Exists(inFile))
                        {
                            throw new Exception(string.Format(MAUIStr.Obj.Share_FileNotFound, inFile));
                        }
                        if (outmode != 8 && Directory.Exists(outFile))
                        {
                            outFile += "/" + Path.GetFileName(inFile) + outFormat;
                            outFile = YFAPI.FormatPath(outFile);
                        }
                        YFAPI.NewDir(outFile, false);
                        YFAPI.Trail(inFile, outFile, inmode, outmode);
                    }
                }
                catch (Exception ex)
                {
                    err = ex.Message;
                }
                sw.Stop();
                decimal time = sw.ElapsedMilliseconds / 1000m;
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (err == null)
                    {
                        FinishStatus.ShowFinish(text4, time.ToString("F3"), outFile);
                    }
                    else
                    {
                        FinishStatus.ShowWrong(text4, err);
                    }
                    b.IsEnabled = true;
                });
            }))
            { IsBackground = true }.Start();
        }
    }
}
