using PopStudio.Language.Languages;
using PopStudio.Platform;

namespace PopStudio.MAUI
{
	public partial class Page_LuaScript : ContentPage
	{
        void LoadFont()
        {
            Title = MAUIStr.Obj.LuaScript_Title;
            label_introduction.Text = MAUIStr.Obj.LuaScript_Introduction;
            label_print.Text = MAUIStr.Obj.LuaScript_TracePrint;
            button_run.Text = MAUIStr.Obj.Share_Run;
            button_api_help.Text = MAUIStr.Obj.LuaScript_ApiHelp;
        }

        public Page_LuaScript()
		{
			InitializeComponent();
            LoadFont();
            YFAPI.LoadTextBox(this);
            MAUIStr.OnLanguageChanged += LoadFont;
        }

        ~Page_LuaScript()
        {
            MAUIStr.OnLanguageChanged -= LoadFont;
        }

        async void button_api_help_Clicked(object sender, EventArgs e)
        {
            try
            {
                await Shell.Current.Navigation.PushAsync(new Page_LuaApiHelp(InsertIntoEditor));
            }
            catch (Exception ex)
            {
                Print(string.Format(MAUIStr.Obj.Share_Wrong, ex.Message));
            }
        }

        void InsertIntoEditor(string snippet)
        {
            if (string.IsNullOrEmpty(snippet)) return;
            string text = richtextbox1.Text ?? string.Empty;
            int pos = richtextbox1.CursorPosition;
            if (pos < 0 || pos > text.Length) pos = text.Length;
            string toInsert = snippet;
            if (pos > 0 && text[pos - 1] != '\n' && text[pos - 1] != '\r')
            {
                toInsert = "\n" + toInsert;
            }
            richtextbox1.Text = text.Insert(pos, toInsert);
            richtextbox1.CursorPosition = pos + toInsert.Length;
            richtextbox1.Focus();
        }

        private void button_run_Click(object sender, EventArgs e)
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
                Dispatcher.Dispatch(() => { b.IsEnabled = true; });
            }))
            { IsBackground = true }.Start();
        }

        public void Print(string str)
        {
            richtextbox2.Text += str + '\n';
        }
	}
}