using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using PopStudio.Language.Languages;
using System.Diagnostics;
using PopStudio.Platform;

namespace PopStudio.Avalonia.Pages
{
    public partial class Control_Texture : UserControl
    {
        public Control_Texture()
        {
            InitializeComponent();
            LoadControl();
            FilePathDrop.Enable(textbox1, textbox2);
            LoadFont();
            foreach (var item in new List<string>
                     {
                         "PTX(rsb)",
                         "cdat(Android,iOS)",
                         "tex(iOS)",
                         "txz(Android,iOS)",
                         "tex(TV)",
                         "ptx(Xbox360)",
                         "ptx(PS3)",
                         "ptx(PSV)",
                         "xnb(Windows Phone)"
                     })
            {
                CB_CMode.Items.Add(item);
            }
            CB_CMode.SelectedIndex = 0;
            foreach (var item in new List<string>
                     {
                         "ARGB8888(0)",
                         "ABGR8888(0)",
                         "RGBA4444(1)",
                         "RGB565(2)",
                         "RGBA5551(3)",
                         "RGBA4444_Block(21)",
                         "RGB565_Block(22)",
                         "RGBA5551_Block(23)",
                         "ARGB8888_A8(149)",
                         "ABGR8888_A8(149)",
                         "ARGB8888(BE)(0)",
                         "ARGB8888_Padding(BE)(0)",
                         "DXT1_RGB(35)",
                         "DXT3_RGBA(36)",
                         "DXT5_RGBA(37)",
                         "DXT5_RGBA_MortonBlock(5)",
                         "DXT5_RGBA(BE)(5)",
                         "ETC1_RGB(32)",
                         "ETC1_RGB_A8(147)",
                         "ETC1_RGB_A_Palette(147)",
                         "PVRTC_4BPP_RGBA(30)",
                         "PVRTC_4BPP_RGBA_A8(148)",
                         "PVRTC_2BPP_RGBA(31)",
                         "ATC_RGB(38)",
                         "ATC_RGBA4(39)",
                     })
            {
                CB_FMode.Items.Add(item);
            }
            CB_FMode.SelectedIndex = 0;
            CB_CMode.SelectionChanged += CB_CMode_Selected;
            MAUIStr.OnLanguageChanged += LoadFont;
        }

        ~Control_Texture()
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
            label_choosemode = this.Get<TextBlock>("label_choosemode");
            label_mode1 = this.Get<TextBlock>("label_mode1");
            label_mode2 = this.Get<TextBlock>("label_mode2");
            text1 = this.Get<TextBlock>("text1");
            text2 = this.Get<TextBlock>("text2");
            text3 = this.Get<TextBlock>("text3");
            textN = this.Get<TextBlock>("textN");
            button1 = this.Get<Button>("button1");
            button2 = this.Get<Button>("button2");
            button_run = this.Get<Button>("button_run");
            label_statue = this.Get<TextBlock>("label_statue");
            text4 = this.Get<TextBlock>("text4");
            textbox1 = this.Get<TextBox>("textbox1");
            textbox2 = this.Get<TextBox>("textbox2");
            TB_Mode = this.Get<ToggleSwitch>("TB_Mode");
            CB_CMode = this.Get<ComboBox>("CB_CMode");
            CB_FMode = this.Get<ComboBox>("CB_FMode");
            SP_FMode = this.Get<StackPanel>("SP_FMode");
        }

        void LoadFont()
        {
            textbox1.Watermark = textbox2.Watermark = MAUIStr.Obj.Share_DropPathHint;
            label_batch1.Text = MAUIStr.Obj.Share_SingleMode;
            label_batch2.Text = MAUIStr.Obj.Share_BatchMode;
            label_introduction.Text = MAUIStr.Obj.Texture_Introduction;
            label_choosemode.Text = MAUIStr.Obj.Share_ChooseMode;
            if (batch_mode.IsChecked == true)
            {
                label_mode1.Text = MAUIStr.Obj.Texture_Mode1_Batch;
                label_mode2.Text = MAUIStr.Obj.Texture_Mode2_Batch;
                textN.Text = MAUIStr.Obj.Texture_Choose7_Batch;
            }
            else
            {
                label_mode1.Text = MAUIStr.Obj.Texture_Mode1;
                label_mode2.Text = MAUIStr.Obj.Texture_Mode2;
                textN.Text = MAUIStr.Obj.Texture_Choose7;
            }
            LoadFont_Checked(TB_Mode.IsChecked == true);
            button1.Content = MAUIStr.Obj.Share_Choose;
            button2.Content = MAUIStr.Obj.Share_Choose;
            button_run.Content = MAUIStr.Obj.Share_Run;
            label_statue.Text = MAUIStr.Obj.Share_RunStatue;
            text4.Text = MAUIStr.Obj.Share_Waiting;
        }

        void LoadFont_Checked(bool v)
        {
            if (batch_mode.IsChecked == true)
            {
                if (v)
                {
                    text1.Text = MAUIStr.Obj.Texture_Choose4_Batch;
                    text2.Text = MAUIStr.Obj.Texture_Choose5_Batch;
                    text3.Text = MAUIStr.Obj.Texture_Choose6_Batch;
                }
                else
                {
                    text1.Text = MAUIStr.Obj.Texture_Choose1_Batch;
                    text2.Text = MAUIStr.Obj.Texture_Choose2_Batch;
                    text3.Text = MAUIStr.Obj.Texture_Choose3_Batch;
                }
            }
            else
            {
                if (v)
                {
                    text1.Text = MAUIStr.Obj.Texture_Choose4;
                    text2.Text = MAUIStr.Obj.Texture_Choose5;
                    text3.Text = MAUIStr.Obj.Texture_Choose6;
                }
                else
                {
                    text1.Text = MAUIStr.Obj.Texture_Choose1;
                    text2.Text = MAUIStr.Obj.Texture_Choose2;
                    text3.Text = MAUIStr.Obj.Texture_Choose3;
                }
            }
        }

        private void Switch_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleSwitch s)
            {
                LoadFont_Checked(s.IsChecked == true);
                if (s.IsChecked == true)
                {
                    SP_FMode.IsVisible = true;
                }
                else
                {
                    SP_FMode.IsVisible = false;
                }
                (textbox1.Text, textbox2.Text) = (textbox2.Text, textbox1.Text);
            }
        }

        private void Switch_Batch_Checked(object sender, RoutedEventArgs e)
        {
            LoadFont();
        }

        private async void Button1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string val = batch_mode.IsChecked == false ? (await new OpenFileDialog { AllowMultiple = false }.ShowAsync(MainWindow.Singleten))?[0] : (await new OpenFolderDialog().ShowAsync(MainWindow.Singleten));
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
                string val = batch_mode.IsChecked == false ? (await new SaveFileDialog().ShowAsync(MainWindow.Singleten)) : (await new OpenFolderDialog().ShowAsync(MainWindow.Singleten));
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
            text4.Text = MAUIStr.Obj.Share_Running;
            bool mode = TB_Mode.IsChecked == true;
            string inFile = textbox1.Text;
            string outFile = textbox2.Text;
            bool batchmode = batch_mode.IsChecked == true;
            int cmode = CB_CMode.SelectedIndex;
            int fmode = CB_FMode.SelectedIndex;
            new Thread(new ThreadStart(() =>
            {
                string err = null;
                Stopwatch sw = new Stopwatch();
                sw.Start();
                try
                {
                    string outFormat = mode ? (cmode switch
                    {
                        0 => ".ptx",
                        1 => ".cdat",
                        2 => ".tex",
                        3 => ".txz",
                        4 => ".tex",
                        5 => ".ptx",
                        6 => ".ptx",
                        7 => ".ptx",
                        8 => ".xnb",
                        _ => null
                    }) : ".png";
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
                        string rightFormat = (!mode) ? (cmode switch
                        {
                            0 => ".ptx",
                            1 => ".cdat",
                            2 => ".tex",
                            3 => ".txz",
                            4 => ".tex",
                            5 => ".ptx",
                            6 => ".ptx",
                            7 => ".ptx",
                            8 => ".xnb",
                            _ => null
                        }) : ".png";
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
                                if (mode)
                                {
                                    YFAPI.EncodeImage(mfile, newPath, cmode, fmode);
                                }
                                else
                                {
                                    YFAPI.DecodeImage(mfile, newPath, cmode);
                                }
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
                        if (Directory.Exists(outFile))
                        {
                            outFile += "/" + Path.GetFileName(inFile) + outFormat;
                            outFile = YFAPI.FormatPath(outFile);
                        }
                        YFAPI.NewDir(outFile, false);
                        if (mode)
                        {
                            YFAPI.EncodeImage(inFile, outFile, cmode, fmode);
                        }
                        else
                        {
                            YFAPI.DecodeImage(inFile, outFile, cmode);
                        }
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
                        text4.Text = string.Format(MAUIStr.Obj.Share_Finish, time.ToString("F3"));
                    }
                    else
                    {
                        text4.Text = string.Format(MAUIStr.Obj.Share_Wrong, err);
                    }
                    b.IsEnabled = true;
                });
            }))
            { IsBackground = true }.Start();
        }

        private void CB_CMode_Selected(object sender, SelectionChangedEventArgs e)
        {
            int index = CB_CMode.SelectedIndex;
            var FMode_Items = CB_FMode.Items;
            FMode_Items.Clear();
            switch (index)
            {
                case 0:
                    FMode_Items.Add("ARGB8888(0)");
                    FMode_Items.Add("ABGR8888(0)");
                    FMode_Items.Add("RGBA4444(1)");
                    FMode_Items.Add("RGB565(2)");
                    FMode_Items.Add("RGBA5551(3)");
                    FMode_Items.Add("RGBA4444_Block(21)");
                    FMode_Items.Add("RGB565_Block(22)");
                    FMode_Items.Add("RGBA5551_Block(23)");
                    FMode_Items.Add("ARGB8888_A8(149)");
                    FMode_Items.Add("ABGR8888_A8(149)");
                    FMode_Items.Add("ARGB8888(BE)(0)");
                    FMode_Items.Add("ARGB8888_Padding(BE)(0)");
                    FMode_Items.Add("DXT1_RGB(35)");
                    FMode_Items.Add("DXT3_RGBA(36)");
                    FMode_Items.Add("DXT5_RGBA(37)");
                    FMode_Items.Add("DXT5_RGBA_MortonBlock(5)");
                    FMode_Items.Add("DXT5_RGBA(BE)(5)");
                    FMode_Items.Add("ETC1_RGB(32)");
                    FMode_Items.Add("ETC1_RGB_A8(147)");
                    FMode_Items.Add("ETC1_RGB_A_Palette(147)");
                    FMode_Items.Add("PVRTC_4BPP_RGBA(30)");
                    FMode_Items.Add("PVRTC_4BPP_RGBA_A8(148)");
                    FMode_Items.Add("PVRTC_2BPP_RGBA(31)");
                    FMode_Items.Add("ATC_RGB(38)");
                    FMode_Items.Add("ATC_RGBA4(39)");
                    break;
                case 1:
                    FMode_Items.Add("Encrypt");
                    break;
                case 2:
                case 3:
                    FMode_Items.Add("ABGR8888(1)");
                    FMode_Items.Add("RGBA4444(2)");
                    FMode_Items.Add("RGBA5551(3)");
                    FMode_Items.Add("RGB565(4)");
                    break;
                case 4:
                    FMode_Items.Add("ARGB8888(2)");
                    FMode_Items.Add("ARGB4444(3)");
                    FMode_Items.Add("ARGB1555(4)");
                    FMode_Items.Add("RGB565(5)");
                    FMode_Items.Add("ABGR8888(6)");
                    FMode_Items.Add("RGBA4444(7)");
                    FMode_Items.Add("RGBA5551(8)");
                    FMode_Items.Add("ARGB8888(NoZlib)(2)");
                    FMode_Items.Add("ARGB4444(NoZlib)(3)");
                    FMode_Items.Add("ARGB1555(NoZlib)(4)");
                    FMode_Items.Add("RGB565(NoZlib)(5)");
                    FMode_Items.Add("ABGR8888(NoZlib)(6)");
                    FMode_Items.Add("RGBA4444(NoZlib)(7)");
                    FMode_Items.Add("RGBA5551(NoZlib)(8)");
                    break;
                case 5:
                    FMode_Items.Add("DXT5_RGBA_Padding(BE)");
                    break;
                case 6:
                    FMode_Items.Add("DXT5_RGBA");
                    break;
                case 7:
                    FMode_Items.Add("DXT5_RGBA_Morton");
                    break;
                case 8:
                    FMode_Items.Add("ABGR8888");
                    break;
            }
            CB_FMode.SelectedIndex = 0;
        }
    }
}
