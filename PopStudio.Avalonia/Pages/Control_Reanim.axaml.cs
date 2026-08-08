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
    public partial class Control_Reanim : UserControl
    {
        public Control_Reanim()
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
            CB_InMode.Items.Add("Flash_Xfl_Folder");
            CB_InMode.Items.Add("Flash_Fla");
            CB_InMode.SelectedIndex = 0;
            CB_OutMode.Items.Add("PC_Compiled");
            CB_OutMode.Items.Add("Phone32_Compiled");
            CB_OutMode.Items.Add("Phone64_Compiled");
            CB_OutMode.Items.Add("WP_Xnb");
            CB_OutMode.Items.Add("GameConsole_Compiled");
            CB_OutMode.Items.Add("TV_Compiled");
            CB_OutMode.Items.Add("Studio_Json");
            CB_OutMode.Items.Add("Raw_Xml");
            CB_OutMode.Items.Add("Flash_Xfl_Folder");
            CB_OutMode.Items.Add("Godot_Anim");
            CB_OutMode.Items.Add("Flash_Fla");
            CB_OutMode.SelectedIndex = 7;
            MAUIStr.OnLanguageChanged += LoadFont;
        }

        ~Control_Reanim()
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
            DropAccept.AttachPathDrop(textbox1, SetInputFromDrop, GetInputDropRule);
            PathHint.Attach(textbox1, GetInputRule);
            DropAccept.AttachPathBox(textbox2, path => textbox2.Text = path, GetOutputRule);
            CB_InMode.SelectionChanged += (_, __) => PathHint.Refresh(textbox1);
            CB_OutMode.SelectionChanged += (_, __) => PathHint.Refresh(textbox2);
        }

        PathRule GetInputDropRule()
        {
            if (CB_InMode.SelectedIndex == 8)
            {
                return PathRule.ExistingFolder();
            }

            // Match current input mode only; compiled sniff still works across
            // PC/Phone/… modes because they share ".reanim.compiled".
            return PathRule.ExistingFileOrFolder(PathExt.ReanimByIndex(CB_InMode.SelectedIndex));
        }

        PathRule GetInputRule()
        {
            if (batch_mode.IsChecked == true || CB_InMode.SelectedIndex == 8)
            {
                return PathRule.ExistingFolder();
            }

            return PathRule.ExistingFile(PathExt.ReanimByIndex(CB_InMode.SelectedIndex));
        }

        PathRule GetOutputRule()
        {
            if (batch_mode.IsChecked == true || CB_OutMode.SelectedIndex == 8)
            {
                return PathRule.SaveFolder();
            }

            return PathRule.SaveFile(PathExt.ReanimByIndex(CB_OutMode.SelectedIndex));
        }

        async void SetInputFromDrop(string path)
        {
            batch_mode.IsChecked = Directory.Exists(path);
            textbox1.Text = path;
            PathHint.Refresh(textbox1);
            PathHint.Refresh(textbox2);
            await MaybeSwitchCompiledFormatAsync(path);
        }

        async Task MaybeSwitchCompiledFormatAsync(string path)
        {
            if (string.IsNullOrEmpty(path) || Directory.Exists(path))
            {
                return;
            }

            int? detected = PopStudio.Reanim.CompiledSniff.TryDetect(path);
            if (detected == null)
            {
                return;
            }

            int current = CB_InMode.SelectedIndex;
            if (current == detected.Value)
            {
                return;
            }

            string detectedName = FormatModeName(detected.Value);
            string currentName = FormatModeName(current);
            bool switchMode = await PopupDialog.DisplayAlert(
                MAUIStr.Obj.Reanim_CompiledMismatch_Title,
                string.Format(MAUIStr.Obj.Reanim_CompiledMismatch_Text, detectedName, currentName),
                MAUIStr.Obj.Setting_OK,
                MAUIStr.Obj.Setting_Cancel);
            if (!switchMode)
            {
                return;
            }

            CB_InMode.SelectedIndex = detected.Value;
            PathHint.Refresh(textbox1);
            PathHint.Refresh(textbox2);
        }

        string FormatModeName(int index)
        {
            if (index >= 0 && index < CB_InMode.Items.Count)
            {
                return CB_InMode.Items[index]?.ToString() ?? index.ToString();
            }

            return index.ToString();
        }

        void LoadFont()
        {
            label_batch1.Text = MAUIStr.Obj.Share_SingleMode;
            label_batch2.Text = MAUIStr.Obj.Share_BatchMode;
            label_introduction.Text = MAUIStr.Obj.Reanim_Introduction;
            if (batch_mode.IsChecked == true)
            {
                text1.Text = MAUIStr.Obj.Reanim_Choose1_Batch;
                text2.Text = MAUIStr.Obj.Reanim_Choose2_Batch;
            }
            else
            {
                text1.Text = MAUIStr.Obj.Reanim_Choose1;
                text2.Text = MAUIStr.Obj.Reanim_Choose2;
            }
            text_in.Text = MAUIStr.Obj.Reanim_InFormat;
            text_out.Text = MAUIStr.Obj.Reanim_OutFormat;
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
                string val;
                if (CB_InMode.SelectedIndex == 8 || batch_mode.IsChecked == true)
                {
                    val = await StorageDialog.OpenFolderAsync();
                }
                else
                {
                    val = await StorageDialog.OpenFileAsync(PathExt.ReanimByIndex(CB_InMode.SelectedIndex));
                }
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
                string val;
                if (CB_OutMode.SelectedIndex == 8 || batch_mode.IsChecked == true)
                {
                    val = await StorageDialog.OpenFolderAsync();
                }
                else
                {
                    val = await StorageDialog.SaveFileAsync(PathExt.ReanimByIndex(CB_OutMode.SelectedIndex));
                }
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
                        0 => ".reanim.compiled",
                        1 => ".reanim.compiled",
                        2 => ".reanim.compiled",
                        3 => ".xnb",
                        4 => ".reanim.compiled",
                        5 => ".reanim.compiled",
                        6 => ".reanim.json",
                        7 => ".reanim",
                        8 => ".xfl",
                        9 => ".scn",
                        10 => ".fla",
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
                            0 => ".reanim.compiled",
                            1 => ".reanim.compiled",
                            2 => ".reanim.compiled",
                            3 => ".xnb",
                            4 => ".reanim.compiled",
                            5 => ".reanim.compiled",
                            6 => ".reanim.json",
                            7 => ".reanim",
                            8 => ".xfl",
                            9 => null,
                            _ => null
                        };
                        int rightFormatLength = rightFormat?.Length ?? 0;
                        foreach (string mfile in files)
                        {
                            bool matchesInputFormat = inmode == 9
                                ? PopStudio.Reanim.FlashFla.IsZipXfl(mfile)
                                : mfile.Length >= rightFormatLength && mfile[^rightFormatLength..].ToLower() == rightFormat;
                            if (!matchesInputFormat)
                            {
                                continue;
                            }
                            string newPath = YFAPI.FormatPath(outFile + mfile[length..] + outFormat);
                            YFAPI.NewDir(newPath, false);
                            try
                            {
                                YFAPI.Reanim(mfile, newPath, inmode, outmode);
                            }
                            catch (Exception)
                            {
                                if (outmode != 8)
                                {
                                    File.Delete(newPath);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (!File.Exists(inFile) && !(inmode == 8 && Directory.Exists(inFile)))
                        {
                            throw new Exception(string.Format(MAUIStr.Obj.Share_FileNotFound, inFile));
                        }
                        if (outmode != 8 && Directory.Exists(outFile))
                        {
                            outFile += "/" + Path.GetFileName(inFile) + outFormat;
                            outFile = YFAPI.FormatPath(outFile);
                        }
                        YFAPI.NewDir(outFile, false);
                        YFAPI.Reanim(inFile, outFile, inmode, outmode);
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
