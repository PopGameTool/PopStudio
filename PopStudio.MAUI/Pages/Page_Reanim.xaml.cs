using PopStudio.Language.Languages;
using PopStudio.Platform;

namespace PopStudio.MAUI
{
	public partial class Page_Reanim : ContentPage
	{
        void LoadFont()
        {
            Title = MAUIStr.Obj.Reanim_Title;
            label_introduction.Text = MAUIStr.Obj.Reanim_Introduction;
            text1.Text = MAUIStr.Obj.Reanim_Choose1;
            text2.Text = MAUIStr.Obj.Reanim_Choose2;
            text_in.Text = MAUIStr.Obj.Reanim_InFormat;
            text_out.Text = MAUIStr.Obj.Reanim_OutFormat;
            button1.Text = MAUIStr.Obj.Share_Choose;
            button2.Text = MAUIStr.Obj.Share_Choose;
            button_run.Text = MAUIStr.Obj.Share_Run;
            label_statue.Text = MAUIStr.Obj.Share_RunStatue;
            text4.Text = MAUIStr.Obj.Share_Waiting;
        }

        public Page_Reanim()
		{
			InitializeComponent();
            LoadFont();
            CB_InMode.Items.Clear();
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
            CB_OutMode.Items.Clear();
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

        ~Page_Reanim()
        {
            MAUIStr.OnLanguageChanged -= LoadFont;
        }

        private void Button_Click(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            b.IsEnabled = false;
            text4.Text = MAUIStr.Obj.Share_Running;
            string inFile = textbox1.Text;
            string outFile = textbox2.Text;
            int inmode = CB_InMode.SelectedIndex;
            int outmode = CB_OutMode.SelectedIndex;
            new Thread(new ThreadStart(() =>
            {
                string err = null;
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                try
                {
                    if (!File.Exists(inFile) && !(inmode == 8 && Directory.Exists(inFile)))
                    {
                        throw new Exception(string.Format(MAUIStr.Obj.Share_FileNotFound, inFile));
                    }
                    YFAPI.Reanim(inFile, outFile, inmode, outmode);
                }
                catch (Exception ex)
                {
                    err = ex.Message;
                }
                sw.Stop();
                decimal time = sw.ElapsedMilliseconds / 1000m;
                Dispatcher.Dispatch(() =>
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

		private async void Button_Clicked(object sender, EventArgs e)
		{
			try
			{
				string val = CB_InMode.SelectedIndex == 8
                    ? await this.ChooseFolder()
                    : await this.ChooseOpenFile(); //Can't default this
                if (!string.IsNullOrEmpty(val)) textbox1.Text = val;
			}
			catch (Exception)
			{
			}
		}

		private async void Button2_Clicked(object sender, EventArgs e)
		{
			try
			{
                string val;
                if (CB_OutMode.SelectedIndex == 8)
                {
                    val = await this.ChooseFolder(); //Can't default this
                }
                else
                {
                    val = await this.ChooseSaveFile(); //Can't default this
                }
                if (!string.IsNullOrEmpty(val)) textbox2.Text = val;
			}
			catch (Exception)
			{
			}
		}
	}
}
