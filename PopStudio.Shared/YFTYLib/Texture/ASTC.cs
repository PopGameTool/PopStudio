using PopStudio.Platform;

namespace PopStudio.Texture
{
    internal static unsafe class ASTC
    {
        public static YFBitmap Read(BinaryStream bs, int width, int height, int block)
        {
            int blockWidth = block, blockHeight = block;
            int blocksX = (width + blockWidth - 1) / blockWidth;
            int blocksY = (height + blockHeight - 1) / blockHeight;
            int compressedSize = blocksX * blocksY * 16;

            byte[] astcData = bs.ReadBytes(compressedSize);

            AstcDecoder decoder = new AstcDecoder();
            byte[] rgba = decoder.DecodeASTC(astcData, width, height, blockWidth, blockHeight);

            YFBitmap image = YFBitmap.Create(width, height);
            YFColor* pixels = (YFColor*)image.GetPixels().ToPointer();

            fixed (byte* src = rgba)
            {
                int stride = width * 4;
                for (int y = 0; y < height; y++)
                {
                    byte* srcRow = src + (height - 1 - y) * stride;
                    YFColor* destRow = pixels + y * width;
                    for (int x = 0; x < width; x++)
                    {
                        byte b = srcRow[x * 4];
                        byte g = srcRow[x * 4 + 1];
                        byte r = srcRow[x * 4 + 2];
                        byte a = srcRow[x * 4 + 3];
                        destRow[x] = new YFColor(r, g, b, a);
                    }
                }
            }

            return image;
        }
    }
}