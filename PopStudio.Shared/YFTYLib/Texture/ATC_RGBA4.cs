using PopStudio.Platform;

namespace PopStudio.Texture
{
    internal static class ATC_RGBA4
    {
        public static YFBitmap Read(BinaryStream bs, int width, int height) =>
            ATC.Read(bs, width, height, ATC.AlphaMode.Explicit);

        public static int Write(BinaryStream bs, YFBitmap image) =>
            ATC.Write(bs, image, ATC.AlphaMode.Explicit);
    }
}
