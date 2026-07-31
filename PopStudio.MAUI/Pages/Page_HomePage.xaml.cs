using PopStudio.Language.Languages;
using PopStudio.Platform;

namespace PopStudio.MAUI
{
	public partial class Page_HomePage : ContentPage
	{
        void LoadFont()
        {
            Title = MAUIStr.Obj.HomePage_Title;
            label_begin.Text = MAUIStr.Obj.HomePage_Begin;
            label_function.Text = MAUIStr.Obj.HomePage_Function;
            label_agreement.Text = MAUIStr.Obj.HomePage_Agreement;
            label_permission.Text = MAUIStr.Obj.HomePage_Permission;
            button_permission.Text = MAUIStr.Obj.HomePage_PermissionAsk;
            button_close.Text = MAUIStr.Obj.Permission_Cancel;
            label_ver.Text = string.Format(MAUIStr.Obj.HomePage_Version, Str.Obj.AppVersion);
            label_author_string.Text = MAUIStr.Obj.HomePage_Author_String;
            label_author.Text = MAUIStr.Obj.HomePage_Author;
            label_thanks_string.Text = MAUIStr.Obj.HomePage_Thanks_String;
            label_thanks.Text = MAUIStr.Obj.HomePage_Thanks;
            label_qqgroup_string.Text = MAUIStr.Obj.HomePage_QQGroup_String;
            label_qqgroup.Text = MAUIStr.Obj.HomePage_QQGroup;
            label_course_string.Text = MAUIStr.Obj.HomePage_Course_String;
            label_course.Text = MAUIStr.Obj.HomePage_Course;
            label_appnewnotice_string.Text = MAUIStr.Obj.HomePage_AppNewNotice_String;
            label_appnewnotice.Text = MAUIStr.Obj.HomePage_AppNewNotice;
        }

        public Page_HomePage()
		{
			InitializeComponent();
            LoadFont();
            MAUIStr.OnLanguageChanged += LoadFont;
        }

        ~Page_HomePage()
        {
            MAUIStr.OnLanguageChanged -= LoadFont;
        }

        // 【关键修改 1】重写 OnAppearing：页面每次显示时（包括从系统设置返回）都检查权限
        protected override void OnAppearing()
        {
            base.OnAppearing();
            CheckPermissionAndUpdateUi();
        }

        // 【关键修改 2】用 OnDisappearing 替代析构函数（更可靠）
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MAUIStr.OnLanguageChanged -= LoadFont;
        }

        // 【关键修改 3】封装统一的权限检查+UI更新逻辑
        private async void CheckPermissionAndUpdateUi()
        {
            bool hasPermission = await Permission.CheckPermissionAsync();
            // 有权限则隐藏弹窗，无权限则显示
            AndroidPermission.IsVisible = !hasPermission;
        }

        int GetRandomNumber(int m, int n)
        {
            return 2;
        }

        private async void button_permission_Clicked(object sender, EventArgs e)
        {
            await this.CheckAndRequestPermissionAsync();
            // 【关键修改 4】用户在当前页授权后，立即重新检查并更新 UI
            CheckPermissionAndUpdateUi();
        }

        private void button_close_Clicked(object sender, EventArgs e)
        {
            AndroidPermission.IsVisible = false;
        }
    }
}