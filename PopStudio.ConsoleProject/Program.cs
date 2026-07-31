using PopStudio.Language.Languages;
using PopStudio.Platform;

namespace PopStudio.ConsoleProject
{
    internal class Program
    {
        static void Main(params string[] args)
        {
            YFAPI.RegistPlatform<ConsoleAPI>();
#if LINUXCONSOLE
            YFBitmap.RegistPlatform<ImageBitmap>();
#elif MACOSCONSOLE
            YFBitmap.RegistPlatform<ImageBitmap>();
#else
            // YFBitmap.RegistPlatform<GDIBitmap>();
            YFBitmap.RegistPlatform<ImageBitmap>();
#endif
            try
            {
                string settingxml = Permission.GetSettingPath();
                if (File.Exists(settingxml))
                {
                    Setting.LoadFromXml(settingxml);
                }
            }
            catch (Exception)
            {
            }
            ConsoleReader.RegistArguments(args);
            //Begin
            App app = new App();
            while (true)
            {
                try
                {
                    app.Start();
                }
                catch (Exception ex)
                {
                    ConsoleWriter.WriteErrorLine(MAUIStr.Obj.Share_Wrong, ex.Message);
                }
            }
        }
    }
}