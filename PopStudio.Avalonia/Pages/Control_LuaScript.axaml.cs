using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using PopStudio.Avalonia.Drop;
using PopStudio.Language.Languages;
using PopStudio.Platform;

namespace PopStudio.Avalonia.Pages
{
    public partial class Control_LuaScript : UserControl
    {
        public Control_LuaScript()
        {
            InitializeComponent();
            LoadControl();
            LoadFont();
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
            richtextbox1 = this.Get<TextBox>("richtextbox1");
            richtextbox2 = this.Get<TextBox>("richtextbox2");
            DropAccept.AttachFileContentDrop(richtextbox1, text => richtextbox1.Text = text, ".lua", ".txt");
        }

        void LoadFont()
        {
            label_introduction.Text = MAUIStr.Obj.LuaScript_Introduction;
            label_print.Text = MAUIStr.Obj.LuaScript_TracePrint;
            button_run.Content = MAUIStr.Obj.Share_Run;
        }

        private void ButtonRun_Click(object sender, RoutedEventArgs e)
        {
            Button b = (Button)sender;
            b.IsEnabled = false;
            richtextbox2.Text = string.Empty;
            string script = richtextbox1.Text;
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
    }
}
