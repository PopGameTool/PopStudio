#if ENABLE_LUA_EDITOR
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace PopStudio.Avalonia.Themes
{
    public partial class AvaloniaEditTheme : Styles
    {
        public AvaloniaEditTheme()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
#endif
