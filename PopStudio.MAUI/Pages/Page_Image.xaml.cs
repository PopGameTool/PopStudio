using PopStudio.Image.Convert;
using PopStudio.Language.Languages;
using PopStudio.Platform;
using System.Text;

namespace PopStudio.MAUI
{
	public partial class Page_Image : ContentPage
	{
		public Page_Image()
		{
			InitializeComponent();
			// 状态文本只在初始化时设一次，免得切语言把正在跑的进度或结果冲掉
			label_statue.Text = MAUIStr.Obj.Share_RunStatue;
			LoadFont();
			CB_Layout.SelectedIndex = 0;
			CB_Bitmap.SelectedIndex = 0;
			textbox_quality.Text = "90";
			textbox_maxedge.Text = "0";
			RefreshVisible();
			MAUIStr.OnLanguageChanged += LoadFont;
		}

		~Page_Image()
		{
			MAUIStr.OnLanguageChanged -= LoadFont;
		}

		void LoadFont()
		{
			Title = MAUIStr.Obj.Image_Title;
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
			button_inputfile.Text = MAUIStr.Obj.Image_InputFile;
			button_inputfolder.Text = MAUIStr.Obj.Image_InputFolder;
			button_outdir.Text = MAUIStr.Obj.Share_Choose;
			button_run.Text = MAUIStr.Obj.Share_Run;
			// 下拉里的文字随语言变，按下标原地替换以免重置已选项
			SetItem(CB_Layout.Items, 0, MAUIStr.Obj.Image_Layout0);
			SetItem(CB_Layout.Items, 1, MAUIStr.Obj.Image_Layout1);
			for (int i = 0; i < ImageConvertCatalog.BitmapFormats.Count; i++)
			{
				SetItem(CB_Bitmap.Items, i, ImageConvertCatalog.BitmapFormats[i].DisplayName);
			}
		}

		static void SetItem(IList<string> items, int index, string text)
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

		private void CB_Bitmap_Selected(object sender, EventArgs e) => RefreshVisible();

		private async void ButtonInputFile_Clicked(object sender, EventArgs e)
		{
			try
			{
				string val = await this.ChooseOpenFile(); //Can't default this
				if (!string.IsNullOrEmpty(val)) textbox_input.Text = val;
			}
			catch (Exception)
			{
			}
		}

		private async void ButtonInputFolder_Clicked(object sender, EventArgs e)
		{
			try
			{
				string val = await this.ChooseFolder(); //Can't default this
				if (!string.IsNullOrEmpty(val)) textbox_input.Text = val;
			}
			catch (Exception)
			{
			}
		}

		private async void ButtonOutDir_Clicked(object sender, EventArgs e)
		{
			try
			{
				string val = await this.ChooseFolder(); //Can't default this
				if (!string.IsNullOrEmpty(val)) textbox_outdir.Text = val;
			}
			catch (Exception)
			{
			}
		}

		private async void ButtonRun_Clicked(object sender, EventArgs e)
		{
			List<string> inputs = SplitInputs(textbox_input.Text);
			if (inputs.Count == 0)
			{
				await DisplayAlert(MAUIStr.Obj.Image_Title, MAUIStr.Obj.Image_NeedInput, MAUIStr.Obj.Shell_OK);
				return;
			}
			ImageConvertOptions options = new ImageConvertOptions();
			options.Mode = ImageConvertMode.BitmapToBitmap;
			options.BitmapFormat = GetSelectedBitmapFormat();
			options.JpegQuality = ParseInt(textbox_quality.Text, 90);
			options.MaxEdge = ParseInt(textbox_maxedge.Text, 0);
			options.IncludeSubdirectories = TB_Recursive.IsToggled;
			options.Conflict = TB_Overwrite.IsToggled ? ImageConflictPolicy.Overwrite : ImageConflictPolicy.Skip;
			options.Layout = CB_Layout.SelectedIndex == 1 ? ImageOutputLayout.Flat : ImageOutputLayout.PreserveRelative;
			string outdir = textbox_outdir.Text == null ? null : textbox_outdir.Text.Trim();
			options.OutputDirectory = string.IsNullOrWhiteSpace(outdir) ? null : outdir;

			Button b = (Button)sender;
			b.IsEnabled = false;
			textbox_failures.Text = string.Empty;
			label_statue.Text = MAUIStr.Obj.Share_Running;
			new Thread(new ThreadStart(() =>
			{
				string err = null;
				ImageConvertResult result = null;
				try
				{
					result = ImageBatchConverter.Convert(inputs, options);
				}
				catch (Exception ex)
				{
					err = ex.Message;
				}
				Dispatcher.Dispatch(() =>
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
					if (err == null && result == null) err = MAUIStr.Obj.Image_InvalidParams;
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
						label_statue.Text = string.Format(MAUIStr.Obj.Image_Result, result.Elapsed.TotalSeconds.ToString("F3"), result.Succeeded, result.Skipped, result.Failed);
					}
					b.IsEnabled = true;
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
		/// 输入框里可以是一段路径，也可以是分号（或换行）分隔的多个路径。
		/// 路径自带分号时用双引号包住即可（放在引号里的分隔符不作数）。
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
