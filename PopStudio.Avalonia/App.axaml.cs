using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
#if ENABLE_LUA_EDITOR
using PopStudio.Avalonia.Themes;
#endif

namespace PopStudio.Avalonia
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
#if ENABLE_LUA_EDITOR
            // Must load via compiled Styles (x:Class), not runtime StyleInclude of AvaloniaEdit URI.
            Styles.Add(new AvaloniaEditTheme());
#endif
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}