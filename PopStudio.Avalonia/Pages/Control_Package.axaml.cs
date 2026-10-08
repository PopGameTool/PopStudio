using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using PopStudio.Language.Languages;
using System.Diagnostics;
using PopStudio.Platform;

namespace PopStudio.Avalonia.Pages
{
    public partial class Control_Package : UserControl
    {
        public Control_Package()
        {
            InitializeComponent();
            LoadControl();
            FilePathDrop.Enable(textbox1, textbox2);
            LoadFont();
            CB_CMode.Items.Add("dz");
            CB_CMode.Items.Add("rsb");
            CB_CMode.Items.Add("pak");
            CB_CMode.Items.Add("arcv");
            CB_CMode.SelectedIndex = 0;
            MAUIStr.OnLanguageChanged += LoadFont;
        }

        ~Control_Package()
        {
            MAUIStr.OnLanguageChanged -= LoadFont;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        void LoadControl()
        {
            label_mode1 = this.Get<TextBlock>("label_mode1");
            label_mode2 = this.Get<TextBlock>("label_mode2");
            label_introduction = this.Get<TextBlock>("label_introduction");
            label_choosemode = this.Get<TextBlock>("label_choosemode");
            label1 = this.Get<TextBlock>("label1");
            label2 = this.Get<TextBlock>("label2");
            label3 = this.Get<TextBlock>("label3");
            label_changeimage = this.Get<TextBlock>("label_changeimage");
            label_deleteimage = this.Get<TextBlock>("label_deleteimage");
            button1 = this.Get<Button>("button1");
            button2 = this.Get<Button>("button2");
            label_statue = this.Get<TextBlock>("label_statue");
            text4 = this.Get<TextBlock>("text4");
            button_run = this.Get<Button>("button_run");
            change = this.Get<Grid>("change");
            textbox1 = this.Get<TextBox>("textbox1");
            textbox2 = this.Get<TextBox>("textbox2");
            switchmode = this.Get<ToggleSwitch>("switchmode");
            switchchange1 = this.Get<ToggleSwitch>("switchchange1");
            switchchange2 = this.Get<ToggleSwitch>("switchchange2");
            CB_CMode = this.Get<ComboBox>("CB_CMode");
        }

        void LoadFont()
        {
            textbox1.Watermark = textbox2.Watermark = MAUIStr.Obj.Share_DropPathHint;
            label_mode1.Text = MAUIStr.Obj.Package_Mode1;
            label_mode2.Text = MAUIStr.Obj.Package_Mode2;
            label_introduction.Text = MAUIStr.Obj.Package_Introduction;
            label_choosemode.Text = MAUIStr.Obj.Share_ChooseMode;
            LoadFont_Checked(switchmode.IsChecked == true);
            label_changeimage.Text = MAUIStr.Obj.Package_ChangeImage;
            label_deleteimage.Text = MAUIStr.Obj.Package_DeleteImage;
            button1.Content = MAUIStr.Obj.Share_Choose;
            button2.Content = MAUIStr.Obj.Share_Choose;
            label_statue.Text = MAUIStr.Obj.Share_RunStatue;
            text4.Text = MAUIStr.Obj.Share_Waiting;
            button_run.Content = MAUIStr.Obj.Share_Run;
        }

        void LoadFont_Checked(bool v)
        {
            if (v)
            {
                label1.Text = MAUIStr.Obj.Package_Choose4;
                label2.Text = MAUIStr.Obj.Package_Choose5;
                label3.Text = MAUIStr.Obj.Package_Choose6;
            }
            else
            {
                label1.Text = MAUIStr.Obj.Package_Choose1;
                label2.Text = MAUIStr.Obj.Package_Choose2;
                label3.Text = MAUIStr.Obj.Package_Choose3;
            }
        }

        private void Switch_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleSwitch s)
            {
                LoadFont_Checked(s.IsChecked == true);
                if (s.IsChecked == true)
                {
                    change.IsVisible = false;
                }
                else
                {
                    change.IsVisible = true;
                }
                (textbox1.Text, textbox2.Text) = (textbox2.Text, textbox1.Text);
            }
        }

        private async void Button1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string val;
                if (switchmode.IsChecked == true)
                {
                    val = await new OpenFolderDialog().ShowAsync(MainWindow.Singleten);
                }
                else
                {
                    val = (await new OpenFileDialog { AllowMultiple = false }.ShowAsync(MainWindow.Singleten))?[0];

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
                if (switchmode.IsChecked == true)
                {
                    val = await new SaveFileDialog().ShowAsync(MainWindow.Singleten);
                }
                else
                {
                    val = await new OpenFolderDialog().ShowAsync(MainWindow.Singleten);
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
            text4.Text = MAUIStr.Obj.Share_Running;
            bool mode = switchmode.IsChecked == true;
            string inFile = textbox1.Text;
            string outFile = textbox2.Text;
            int pmode = CB_CMode.SelectedIndex;
            bool c1 = switchchange1.IsChecked == true;
            bool c2 = switchchange2.IsChecked == true;
            new Thread(new ThreadStart(() =>
            {
                string err = null;
                Stopwatch sw = new Stopwatch();
                sw.Start();
                try
                {
                    if (mode == true)
                    {
                        if (!Directory.Exists(inFile))
                        {
                            throw new Exception(string.Format(MAUIStr.Obj.Share_FolderNotFound, inFile));
                        }
                        YFAPI.Pack(inFile, outFile, pmode);
                    }
                    else
                    {
                        if (!File.Exists(inFile))
                        {
                            throw new Exception(string.Format(MAUIStr.Obj.Share_FileNotFound, inFile));
                        }
                        YFAPI.Unpack(inFile, outFile, pmode, c1, c2);
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
    }
}
