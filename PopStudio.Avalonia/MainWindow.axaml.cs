using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using PopStudio.Language.Languages;
using PopStudio.Platform;

namespace PopStudio.Avalonia
{
    public partial class MainWindow : Window
    {
        public static MainWindow Singleten;
        public Point downPos;
        // public bool isPressed = false;

        public MainWindow()
        {
            Singleten = this;
            InitializeComponent();
            LoadFont();
            LoadHomePage();
            MAUIStr.OnLanguageChanged += LoadFont;
            if (Setting.OpenProgramAD) ShowAD(new Random().Next(1, 4));
            Prepare();
            //Binding size changed event
            SizeChanged += Window_SizeChanged;
        }

        //private void Label_Head_PointerMoved(object sender, global::Avalonia.Input.PointerEventArgs e)
        //{
        //    if (isPressed)
        //    {
        //        PixelPoint po = Position;
        //        Point po2 = e.GetPosition(this);
        //        Position = new PixelPoint((int)(po.X + po2.X - downPos.X), (int)(po.Y + po2.Y - downPos.Y));
        //    }
        //}

        //private void Label_Head_PointerPressed(object sender, global::Avalonia.Input.PointerPressedEventArgs e)
        //{
        //    downPos = e.GetPosition(this);
        //    isPressed = true;
        //}

        //private void Label_Head_PointerReleased(object sender, global::Avalonia.Input.PointerReleasedEventArgs e)
        //{
        //    isPressed = false;
        //}

        public async void Prepare()
        {
            loadingbar.Value = 0;
            LoadingGrid.Opacity = 1;
            LoadingGrid.IsVisible = true;
            for (int i = 1; i <= 50; i++)
            {
                await Task.Delay(10);
                loadingbar.Value = i << 1;
            }
            await Task.Delay(200);
            for (int i = 25; i > 0; i--)
            {
                await Task.Delay(10);
                LoadingGrid.Opacity = (i << 2) / 100d;
            }
            LoadingGrid.IsVisible = false;
        }

        public async void ShowAD(int index)
        {
            byte[] img;
            string url;
            switch (index)
            {
                case 1:
                    img = Resource_AD.ImageAD1;
                    url = Resource_AD.AD1;
                    break;
                case 2:
                    img = Resource_AD.ImageAD2;
                    url = Resource_AD.AD2;
                    break;
                case 3:
                    img = Resource_AD.ImageAD3;
                    url = Resource_AD.AD3;
                    break;
                default:
                    return;
            }
            await PopupDialog.DisplayPicture(MAUIStr.Obj.AD_Title, img, MAUIStr.Obj.AD_Cancel, () => Permission.OpenUrl(url), true);
        }

        ~MainWindow()
        {
            MAUIStr.OnLanguageChanged -= LoadFont;
        }

        public void ShowDialog(UserControl page)
        {
            DialogControl.Content = page;
            DialogGrid.IsVisible = true;
        }

        public void CloseDialog()
        {
            DialogControl.Content = null;
            DialogGrid.IsVisible = false;
        }

        bool ShellState = false;

        private async void OpenShellPage()
        {
            if (ShellState) return;
            MyShellContent_Small.Opacity = 0;
            MyShellContent_Small.IsVisible = true;
            for (int i = 1; i <= 7; i++)
            {
                await Task.Delay(10);
                MyShellContent_Small.Opacity = i / 7d;
            }
            ShellState = true;
        }

        private async void CloseShellPage()
        {
            if (!ShellState) return;
            MyShellContent_Small.Opacity = 1;
            for (int i = 7; i > 0; i--)
            {
                await Task.Delay(10);
                MyShellContent_Small.Opacity = i / 7d;
            }
            MyShellContent_Small.IsVisible = false;
            ShellState = false;
        }

        private void CloseShellPage_Suddenly()
        {
            MyShellContent_Small.IsVisible = false;
            ShellState = false;
        }

        private void ButtonCloseShellPage_Click(object sender, RoutedEventArgs e) => CloseShellPage();

        void HideShell()
        {
            MyShellContent.IsVisible = false;
            Image_Shell.IsVisible = true;
        }

        void ShowShell()
        {
            MyShellContent.IsVisible = true;
            Image_Shell.IsVisible = false;
            CloseShellPage_Suddenly();
        }

        private void Image_TopMost_Tapped(object sender, RoutedEventArgs e) => Topmost = !Topmost;

        private void Image_Shell_Tapped(object sender, RoutedEventArgs e) => OpenShellPage();

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width < 500)
            {
                HideShell();
            }
            else
            {
                ShowShell();
            }
        }

        int CurrentPageIndex = -1;

        public void LoadFont()
        {
            button1.Content = MAUIStr.Obj.HomePage_Title;
            button2.Content = MAUIStr.Obj.Package_Title;
            button3.Content = MAUIStr.Obj.Atlas_Title;
            button4.Content = MAUIStr.Obj.Texture_Title;
            button5.Content = MAUIStr.Obj.Reanim_Title;
            button6.Content = MAUIStr.Obj.Particles_Title;
            button7.Content = MAUIStr.Obj.Trail_Title;
            button8.Content = MAUIStr.Obj.Pam_Title;
            button9.Content = MAUIStr.Obj.RTON_Title;
            button10.Content = MAUIStr.Obj.Compress_Title;
            button11.Content = MAUIStr.Obj.LuaScript_Title;
            button12.Content = MAUIStr.Obj.Setting_Title;
            button13.Content = MAUIStr.Obj.Image_Title;
            button1_Small.Content = MAUIStr.Obj.HomePage_Title;
            button2_Small.Content = MAUIStr.Obj.Package_Title;
            button3_Small.Content = MAUIStr.Obj.Atlas_Title;
            button4_Small.Content = MAUIStr.Obj.Texture_Title;
            button5_Small.Content = MAUIStr.Obj.Reanim_Title;
            button6_Small.Content = MAUIStr.Obj.Particles_Title;
            button7_Small.Content = MAUIStr.Obj.Trail_Title;
            button8_Small.Content = MAUIStr.Obj.Pam_Title;
            button9_Small.Content = MAUIStr.Obj.RTON_Title;
            button10_Small.Content = MAUIStr.Obj.Compress_Title;
            button11_Small.Content = MAUIStr.Obj.LuaScript_Title;
            button12_Small.Content = MAUIStr.Obj.Setting_Title;
            button13_Small.Content = MAUIStr.Obj.Image_Title;
            switch (CurrentPageIndex)
            {
                case 0:
                    SetTitle(MAUIStr.Obj.HomePage_Title);
                    break;
                case 1:
                    SetTitle(MAUIStr.Obj.Package_Title);
                    break;
                case 2:
                    SetTitle(MAUIStr.Obj.Atlas_Title);
                    break;
                case 3:
                    SetTitle(MAUIStr.Obj.Texture_Title);
                    break;
                case 4:
                    SetTitle(MAUIStr.Obj.Reanim_Title);
                    break;
                case 5:
                    SetTitle(MAUIStr.Obj.Particles_Title);
                    break;
                case 6:
                    SetTitle(MAUIStr.Obj.Trail_Title);
                    break;
                case 7:
                    SetTitle(MAUIStr.Obj.Pam_Title);
                    break;
                case 8:
                    SetTitle(MAUIStr.Obj.RTON_Title);
                    break;
                case 9:
                    SetTitle(MAUIStr.Obj.Compress_Title);
                    break;
                case 10:
                    SetTitle(MAUIStr.Obj.LuaScript_Title);
                    break;
                case 11:
                    SetTitle(MAUIStr.Obj.Setting_Title);
                    break;
                case 12:
                    SetTitle(MAUIStr.Obj.Image_Title);
                    break;
            }
        }

        void ResetShellButton()
        {
            button1.Foreground = Brushes.Black;
            button2.Foreground = Brushes.Black;
            button3.Foreground = Brushes.Black;
            button4.Foreground = Brushes.Black;
            button5.Foreground = Brushes.Black;
            button6.Foreground = Brushes.Black;
            button7.Foreground = Brushes.Black;
            button8.Foreground = Brushes.Black;
            button9.Foreground = Brushes.Black;
            button10.Foreground = Brushes.Black;
            button11.Foreground = Brushes.Black;
            button12.Foreground = Brushes.Black;
            button13.Foreground = Brushes.Black;
            button1.Background = Brushes.White;
            button2.Background = Brushes.White;
            button3.Background = Brushes.White;
            button4.Background = Brushes.White;
            button5.Background = Brushes.White;
            button6.Background = Brushes.White;
            button7.Background = Brushes.White;
            button8.Background = Brushes.White;
            button9.Background = Brushes.White;
            button10.Background = Brushes.White;
            button11.Background = Brushes.White;
            button12.Background = Brushes.White;
            button13.Background = Brushes.White;
            button1_Small.Background = Brushes.Transparent;
            button2_Small.Background = Brushes.Transparent;
            button3_Small.Background = Brushes.Transparent;
            button4_Small.Background = Brushes.Transparent;
            button5_Small.Background = Brushes.Transparent;
            button6_Small.Background = Brushes.Transparent;
            button7_Small.Background = Brushes.Transparent;
            button8_Small.Background = Brushes.Transparent;
            button9_Small.Background = Brushes.Transparent;
            button10_Small.Background = Brushes.Transparent;
            button11_Small.Background = Brushes.Transparent;
            button12_Small.Background = Brushes.Transparent;
            button13_Small.Background = Brushes.Transparent;
        }

        void UpButton(Button b)
        {
            b.Background = Brushes.CornflowerBlue;
            b.Foreground = Brushes.White;
        }

        void UpShellButton(Button b)
        {
            b.Background = Brushes.RoyalBlue;
        }

        void LoadPage(UserControl u) => PageControl.Content = u;

        void SetTitle(string s) => Label_Head.Content = s;

        Pages.Control_HomePage homePage = new Pages.Control_HomePage();
        Pages.Control_Package package = new Pages.Control_Package();
        Pages.Control_Atlas atlas = new Pages.Control_Atlas();
        Pages.Control_Texture texture = new Pages.Control_Texture();
        Pages.Control_Reanim reanim = new Pages.Control_Reanim();
        Pages.Control_Particles particles = new Pages.Control_Particles();
        Pages.Control_Trail trail = new Pages.Control_Trail();
        Pages.Control_Pam pam = new Pages.Control_Pam();
        Pages.Control_RTON rton = new Pages.Control_RTON();
        Pages.Control_Compress compress = new Pages.Control_Compress();
        Pages.Control_LuaScript luaScript = new Pages.Control_LuaScript();
        Pages.Control_Setting setting = new Pages.Control_Setting();
        Pages.Control_Image image = new Pages.Control_Image();

        public void LoadHomePage()
        {
            ResetShellButton();
            UpButton(button1);
            UpShellButton(button1_Small);
            LoadPage(homePage);
            SetTitle(MAUIStr.Obj.HomePage_Title);
            CurrentPageIndex = 0;
            CloseShellPage();
        }

        public void LoadPackage()
        {
            ResetShellButton();
            UpButton(button2);
            UpShellButton(button2_Small);
            LoadPage(package);
            SetTitle(MAUIStr.Obj.Package_Title);
            CurrentPageIndex = 1;
            CloseShellPage();
        }

        public void LoadAtlas()
        {
            ResetShellButton();
            UpButton(button3);
            UpShellButton(button3_Small);
            LoadPage(atlas);
            SetTitle(MAUIStr.Obj.Atlas_Title);
            CurrentPageIndex = 2;
            CloseShellPage();
        }

        public void LoadTexture()
        {
            ResetShellButton();
            UpButton(button4);
            UpShellButton(button4_Small);
            LoadPage(texture);
            SetTitle(MAUIStr.Obj.Texture_Title);
            CurrentPageIndex = 3;
            CloseShellPage();
        }

        public void LoadReanim()
        {
            ResetShellButton();
            UpButton(button5);
            UpShellButton(button5_Small);
            LoadPage(reanim);
            SetTitle(MAUIStr.Obj.Reanim_Title);
            CurrentPageIndex = 4;
            CloseShellPage();
        }

        public void LoadParticles()
        {
            ResetShellButton();
            UpButton(button6);
            UpShellButton(button6_Small);
            LoadPage(particles);
            SetTitle(MAUIStr.Obj.Particles_Title);
            CurrentPageIndex = 5;
            CloseShellPage();
        }

        public void LoadTrail()
        {
            ResetShellButton();
            UpButton(button7);
            UpShellButton(button7_Small);
            LoadPage(trail);
            SetTitle(MAUIStr.Obj.Trail_Title);
            CurrentPageIndex = 6;
            CloseShellPage();
        }

        public void LoadPam()
        {
            ResetShellButton();
            UpButton(button8);
            UpShellButton(button8_Small);
            LoadPage(pam);
            SetTitle(MAUIStr.Obj.Pam_Title);
            CurrentPageIndex = 7;
            CloseShellPage();
        }

        public void LoadRTON()
        {
            ResetShellButton();
            UpButton(button9);
            UpShellButton(button9_Small);
            LoadPage(rton);
            SetTitle(MAUIStr.Obj.RTON_Title);
            CurrentPageIndex = 8;
            CloseShellPage();
        }

        public void LoadCompress()
        {
            ResetShellButton();
            UpButton(button10);
            UpShellButton(button10_Small);
            LoadPage(compress);
            SetTitle(MAUIStr.Obj.Compress_Title);
            CurrentPageIndex = 9;
            CloseShellPage();
        }

        public void LoadLuaScript()
        {
            ResetShellButton();
            UpButton(button11);
            UpShellButton(button11_Small);
            LoadPage(luaScript);
            SetTitle(MAUIStr.Obj.LuaScript_Title);
            CurrentPageIndex = 10;
            CloseShellPage();
        }

        public void LoadSetting()
        {
            ResetShellButton();
            UpButton(button12);
            UpShellButton(button12_Small);
            LoadPage(setting);
            SetTitle(MAUIStr.Obj.Setting_Title);
            CurrentPageIndex = 11;
            CloseShellPage();
        }

        private void Button1_Click(object sender, RoutedEventArgs e) => LoadHomePage();

        private void Button2_Click(object sender, RoutedEventArgs e) => LoadPackage();

        private void Button3_Click(object sender, RoutedEventArgs e) => LoadAtlas();

        private void Button4_Click(object sender, RoutedEventArgs e) => LoadTexture();

        private void Button5_Click(object sender, RoutedEventArgs e) => LoadReanim();

        private void Button6_Click(object sender, RoutedEventArgs e) => LoadParticles();

        private void Button7_Click(object sender, RoutedEventArgs e) => LoadTrail();

        private void Button8_Click(object sender, RoutedEventArgs e) => LoadPam();

        private void Button9_Click(object sender, RoutedEventArgs e) => LoadRTON();

        private void Button10_Click(object sender, RoutedEventArgs e) => LoadCompress();

        private void Button11_Click(object sender, RoutedEventArgs e) => LoadLuaScript();

        public void LoadImage()
        {
            ResetShellButton();
            UpButton(button13);
            UpShellButton(button13_Small);
            LoadPage(image);
            SetTitle(MAUIStr.Obj.Image_Title);
            CurrentPageIndex = 12;
            CloseShellPage();
        }

        private void Button12_Click(object sender, RoutedEventArgs e) => LoadSetting();

        private void Button13_Click(object sender, RoutedEventArgs e) => LoadImage();
    }
}