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
    public partial class Control_Atlas : UserControl
    {
        public Control_Atlas()
        {
            InitializeComponent();
            LoadControl();
            LoadFont();
            CB_Mode.Items.Add("RESOURCES.XML(Rsb)");
            CB_Mode.Items.Add("resources.xml(Old)");
            CB_Mode.Items.Add("resources.xml(Ancient)");
            CB_Mode.Items.Add("plist(Free)");
            CB_Mode.Items.Add("atlasimagemap.dat");
            CB_Mode.Items.Add("xml(TV)");
            CB_Mode.Items.Add("RESOURCES.RTON(Rsb)");
            CB_Mode.SelectedIndex = 0;
            CB_MaxWidth.Items.Add("256");
            CB_MaxWidth.Items.Add("512");
            CB_MaxWidth.Items.Add("1024");
            CB_MaxWidth.Items.Add("2048");
            CB_MaxWidth.Items.Add("4096");
            CB_MaxWidth.Items.Add("8192");
            CB_MaxWidth.SelectedIndex = 3;
            CB_MaxHeight.Items.Add("256");
            CB_MaxHeight.Items.Add("512");
            CB_MaxHeight.Items.Add("1024");
            CB_MaxHeight.Items.Add("2048");
            CB_MaxHeight.Items.Add("4096");
            CB_MaxHeight.Items.Add("8192");
            CB_MaxHeight.SelectedIndex = 3;
            MAUIStr.OnLanguageChanged += LoadFont;
        }

        ~Control_Atlas()
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
            label_choosemode = this.Get<TextBlock>("label_choosemode");
            label_mode1 = this.Get<TextBlock>("label_mode1");
            label_mode2 = this.Get<TextBlock>("label_mode2");
            text1 = this.Get<TextBlock>("text1");
            text2 = this.Get<TextBlock>("text2");
            text3 = this.Get<TextBlock>("text3");
            text4 = this.Get<TextBlock>("text4");
            text_mode = this.Get<TextBlock>("text_mode");
            text_maxwidth = this.Get<TextBlock>("text_maxwidth");
            text_maxheight = this.Get<TextBlock>("text_maxheight");
            button1 = this.Get<Button>("button1");
            button2 = this.Get<Button>("button2");
            button3 = this.Get<Button>("button3");
            button_run = this.Get<Button>("button_run");
            label_statue = this.Get<TextBlock>("label_statue");
            text5 = this.Get<TextBlock>("text5");
            TB_Mode = this.Get<ToggleSwitch>("TB_Mode");
            textbox1 = this.Get<TextBox>("textbox1");
            textbox2 = this.Get<TextBox>("textbox2");
            textbox3 = this.Get<TextBox>("textbox3");
            textbox4 = this.Get<TextBox>("textbox4");
            CB_Mode = this.Get<ComboBox>("CB_Mode");
            CB_MaxHeight = this.Get<ComboBox>("CB_MaxHeight");
            CB_MaxWidth = this.Get<ComboBox>("CB_MaxWidth");
            splice_size = this.Get<StackPanel>("splice_size");
            DropAccept.AttachPathBox(textbox1, path => textbox1.Text = path, GetInputRule);
            DropAccept.AttachPathBox(textbox2, path => textbox2.Text = path, GetOutputRule);
            DropAccept.AttachPathBox(textbox3, path => textbox3.Text = path, () => PathRule.ExistingFile(".xml", ".json", ".txt"));
        }

        PathRule GetInputRule()
        {
            return TB_Mode.IsChecked == true
                ? PathRule.ExistingFolder()
                : PathRule.ExistingFile(".png");
        }

        PathRule GetOutputRule()
        {
            return TB_Mode.IsChecked == true
                ? PathRule.SaveFile(".png")
                : PathRule.SaveFolder();
        }

        void LoadFont()
        {
            label_introduction.Text = MAUIStr.Obj.Atlas_Introduction;
            label_choosemode.Text = MAUIStr.Obj.Share_ChooseMode;
            label_mode1.Text = MAUIStr.Obj.Atlas_Mode1;
            label_mode2.Text = MAUIStr.Obj.Atlas_Mode2;
            LoadFont_Checked(TB_Mode.IsChecked == true);
            text3.Text = MAUIStr.Obj.Atlas_Choose3;
            text_mode.Text = MAUIStr.Obj.Atlas_Format;
            text_maxwidth.Text = MAUIStr.Obj.Atlas_MaxWidth;
            text_maxheight.Text = MAUIStr.Obj.Atlas_MaxHeight;
            button1.Content = MAUIStr.Obj.Share_Choose;
            button2.Content = MAUIStr.Obj.Share_Choose;
            button3.Content = MAUIStr.Obj.Share_Choose;
            button_run.Content = MAUIStr.Obj.Share_Run;
            label_statue.Text = MAUIStr.Obj.Share_RunStatue;
            FinishStatus.Set(text5, MAUIStr.Obj.Share_Waiting);
        }

        private async void Button1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string val;
                if (TB_Mode.IsChecked == true)
                {
                    val = await StorageDialog.OpenFolderAsync();
                }
                else
                {
                    val = await StorageDialog.OpenFileAsync(".png");
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
                if (TB_Mode.IsChecked == true)
                {
                    val = await StorageDialog.SaveFileAsync(".png");
                }
                else
                {
                    val = await StorageDialog.OpenFolderAsync();
                }
                if (!string.IsNullOrEmpty(val)) textbox2.Text = val;
            }
            catch (Exception)
            {
            }
        }

        private async void Button3_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string val = await StorageDialog.OpenFileAsync(".xml", ".json", ".txt");
                if (!string.IsNullOrEmpty(val)) textbox3.Text = val;
            }
            catch (Exception)
            {
            }
        }

        private void ButtonRun_Click(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            b.IsEnabled = false;
            FinishStatus.Set(text5, MAUIStr.Obj.Share_Running);
            bool mode = TB_Mode.IsChecked == true;
            string inFile = textbox1.Text;
            string outFile = textbox2.Text;
            string infoFile = textbox3.Text;
            string ID = textbox4.Text;
            int cmode = CB_Mode.SelectedIndex;
            int MaxWidth = 256 << CB_MaxWidth.SelectedIndex;
            int MaxHeight = 256 << CB_MaxHeight.SelectedIndex;
            new Thread(new ThreadStart(() =>
            {
                string err = null;
                Stopwatch sw = new Stopwatch();
                sw.Start();
                try
                {
                    if (mode)
                    {
                        if (!Directory.Exists(inFile))
                        {
                            throw new Exception(string.Format(MAUIStr.Obj.Share_FolderNotFound, inFile));
                        }
                        if (!YFAPI.SpliceImage(inFile, outFile, infoFile, ID, cmode, MaxWidth, MaxHeight))
                        {
                            err = MAUIStr.Obj.Atlas_NotFound2;
                        }
                    }
                    else
                    {
                        if (!File.Exists(inFile))
                        {
                            throw new Exception(string.Format(MAUIStr.Obj.Share_FileNotFound, inFile));
                        }
                        if (!YFAPI.CutImage(inFile, outFile, infoFile, ID, cmode))
                        {
                            err = MAUIStr.Obj.Atlas_NotFound1;
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
                        FinishStatus.ShowFinish(text5, time.ToString("F3"), outFile);
                    }
                    else
                    {
                        FinishStatus.ShowWrong(text5, err);
                    }
                    b.IsEnabled = true;
                });
            }))
            { IsBackground = true }.Start();
        }

        void LoadFont_Checked(bool v)
        {
            if (v)
            {
                text1.Text = MAUIStr.Obj.Atlas_Choose5;
                text2.Text = MAUIStr.Obj.Atlas_Choose6;
                text4.Text = MAUIStr.Obj.Atlas_Choose7;
            }
            else
            {
                text1.Text = MAUIStr.Obj.Atlas_Choose1;
                text2.Text = MAUIStr.Obj.Atlas_Choose2;
                text4.Text = MAUIStr.Obj.Atlas_Choose4;
            }
        }

        private void Switch_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleSwitch s)
            {
                LoadFont_Checked(s.IsChecked == true);
                if (s.IsChecked == true)
                {
                    splice_size.IsVisible = true;
                }
                else
                {
                    splice_size.IsVisible = false;
                }
                (textbox1.Text, textbox2.Text) = (textbox2.Text, textbox1.Text);
                PathHint.Refresh(textbox1);
                PathHint.Refresh(textbox2);
            }
        }
    }
}
