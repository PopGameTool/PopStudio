using PopStudio.Language.Languages;
using PopStudio.Platform;

namespace PopStudio.MAUI
{
    public partial class AppShell : Shell
    {
        void LoadFont() => LoadFont(false);

        void LoadFont(bool selectzero)
        {
            int mIndex = selectzero ? 1 : ((collectionView.SelectedItem as FlyoutPageItem)?.Index ?? 1);
            mIndex--;
            List<FlyoutPageItem> mItem = new List<FlyoutPageItem>();
            mItem.Add(new FlyoutPageItem
            {
                Index = 1,
                Title = fitem_homepage.Title = MAUIStr.Obj.HomePage_Title
            });
            mItem.Add(new FlyoutPageItem
            {
                Index = 2,
                Title = fitem_package.Title = MAUIStr.Obj.Package_Title
            });
            mItem.Add(new FlyoutPageItem
            {
                Index = 3,
                Title = fitem_atlas.Title = MAUIStr.Obj.Atlas_Title
            });
            mItem.Add(new FlyoutPageItem
            {
                Index = 4,
                Title = fitem_texture.Title = MAUIStr.Obj.Texture_Title
            });
            mItem.Add(new FlyoutPageItem
            {
                Index = 5,
                Title = fitem_reanim.Title = MAUIStr.Obj.Reanim_Title
            });
            mItem.Add(new FlyoutPageItem
            {
                Index = 6,
                Title = fitem_particles.Title = MAUIStr.Obj.Particles_Title
            });
            mItem.Add(new FlyoutPageItem
            {
                Index = 7,
                Title = fitem_trail.Title = MAUIStr.Obj.Trail_Title
            });
            mItem.Add(new FlyoutPageItem
            {
                Index = 8,
                Title = fitem_pam.Title = MAUIStr.Obj.Pam_Title
            });
            mItem.Add(new FlyoutPageItem
            {
                Index = 9,
                Title = fitem_rton.Title = MAUIStr.Obj.RTON_Title
            });
            mItem.Add(new FlyoutPageItem
            {
                Index = 10,
                Title = fitem_compress.Title = MAUIStr.Obj.Compress_Title
            });
            mItem.Add(new FlyoutPageItem
            {
                Index = 11,
                Title = fitem_luascript.Title = MAUIStr.Obj.LuaScript_Title
            });
            mItem.Add(new FlyoutPageItem
            {
                Index = 12,
                Title = fitem_setting.Title = MAUIStr.Obj.Setting_Title
            });
            mItem.Add(new FlyoutPageItem
            {
                Index = 13,
                Title = fitem_image.Title = MAUIStr.Obj.Image_Title
            });
            collectionView.ItemsSource = mItem;
            collectionView.SelectedItem = mItem[mIndex];
        }

        public AppShell()
        {
            InitializeComponent();
            LoadFont(true);
            FlyoutBehavior = FlyoutBehavior.Flyout;
            Permission.PlatformInit();
            MAUIStr.OnLanguageChanged += LoadFont;
        }

        ~AppShell()
        {
            MAUIStr.OnLanguageChanged -= LoadFont;
        }

        protected override bool OnBackButtonPressed()
        {
            if (FlyoutIsPresented)
            {
                FlyoutIsPresented = false;
                return true;
            }
            Dispatcher.Dispatch(async () =>
            {
                if (await DisplayAlert(MAUIStr.Obj.Shell_ExitTitle, MAUIStr.Obj.Shell_ExitText, MAUIStr.Obj.Shell_OK, MAUIStr.Obj.Shell_Cancel)) Environment.Exit(0);
            });
            return true;
        }

        private void collectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is FlyoutPageItem item)
            {
                CurrentItem = item.Index switch
                {
                    2 => fitem_package,
                    3 => fitem_atlas,
                    4 => fitem_texture,
                    5 => fitem_reanim,
                    6 => fitem_particles,
                    7 => fitem_trail,
                    8 => fitem_pam,
                    9 => fitem_rton,
                    10 => fitem_compress,
                    11 => fitem_luascript,
                    12 => fitem_setting,
                    13 => fitem_image,
                    _ => fitem_homepage
                };
                FlyoutIsPresented = false;
            }
        }
    }

    public class FlyoutPageItem
    {
        public string Title { get; set; }
        public int Index { get; set; }
    }
}