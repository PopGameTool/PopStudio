using PopStudio.Platform;

namespace PopStudio.Texture
{
    internal static unsafe class ATC
    {
        internal enum AlphaMode { None, Explicit, Interpolated }

        public static YFBitmap Read(BinaryStream bs, int width, int height, AlphaMode alphaMode)
        {
            YFBitmap image = YFBitmap.Create(width, height);
            try
            {
                YFColor* pixels = (YFColor*)image.GetPixels().ToPointer();
                YFColor* palette = stackalloc YFColor[4];
                byte* alphaPalette = stackalloc byte[8];
                for (int y = 0; y < height; y += 4)
                {
                    for (int x = 0; x < width; x += 4)
                    {
                        ulong alpha = alphaMode == AlphaMode.None ? 0 : bs.ReadUInt64(Endian.Small);
                        if (alphaMode == AlphaMode.Interpolated)
                        {
                            BuildAlphaPalette((byte)alpha, (byte)(alpha >> 8), alphaPalette);
                            alpha >>= 16;
                        }
                        ushort color0 = bs.ReadUInt16(Endian.Small);
                        ushort color1 = bs.ReadUInt16(Endian.Small);
                        BuildColorPalette(color0, color1, palette);
                        uint indices = bs.ReadUInt32(Endian.Small);
                        for (int i = 0; i < 16; i++)
                        {
                            YFColor color = palette[indices & 3];
                            indices >>= 2;
                            if (alphaMode == AlphaMode.Explicit)
                            {
                                color.Alpha = (byte)((alpha & 15) * 17);
                                alpha >>= 4;
                            }
                            else if (alphaMode == AlphaMode.Interpolated)
                            {
                                color.Alpha = alphaPalette[alpha & 7];
                                alpha >>= 3;
                            }
                            int px = x + (i & 3), py = y + (i >> 2);
                            if (px < width && py < height) pixels[py * width + px] = color;
                        }
                    }
                }
                return image;
            }
            catch
            {
                image.Dispose();
                throw;
            }
        }

        // ATC uses RGB555/RGB565 endpoints and 5:3 / 3:5 interpolation, not DXT's 2:1 / 1:2.
        private static void BuildColorPalette(ushort color0, ushort color1, YFColor* palette)
        {
            int r = (color0 >> 10) & 31, g = (color0 >> 5) & 31, b = color0 & 31;
            YFColor first = new YFColor((byte)((r << 3) | (r >> 2)),
                (byte)((g << 3) | (g >> 2)), (byte)((b << 3) | (b >> 2)));
            r = color1 >> 11; g = (color1 >> 5) & 63; b = color1 & 31;
            palette[3] = new YFColor((byte)((r << 3) | (r >> 2)),
                (byte)((g << 2) | (g >> 4)), (byte)((b << 3) | (b >> 2)));
            if ((color0 & 0x8000) == 0)
            {
                palette[0] = first;
                palette[1] = Mix(first, palette[3], 5, 3);
                palette[2] = Mix(first, palette[3], 3, 5);
            }
            else
            {
                palette[0] = new YFColor(0, 0, 0);
                palette[1] = new YFColor(
                    (byte)Math.Max(0, first.Red - palette[3].Red / 4),
                    (byte)Math.Max(0, first.Green - palette[3].Green / 4),
                    (byte)Math.Max(0, first.Blue - palette[3].Blue / 4));
                palette[2] = first;
            }
        }

        private static YFColor Mix(YFColor a, YFColor b, int wa, int wb) => new YFColor(
            (byte)((a.Red * wa + b.Red * wb) / 8),
            (byte)((a.Green * wa + b.Green * wb) / 8),
            (byte)((a.Blue * wa + b.Blue * wb) / 8));

        private static void BuildAlphaPalette(byte a, byte b, byte* palette)
        {
            palette[0] = a;
            palette[1] = b;
            int divisor = a > b ? 7 : 5;
            for (int i = 1; i < divisor; i++)
                palette[i + 1] = (byte)(((divisor - i) * a + i * b + divisor / 2) / divisor);
            if (a <= b)
            {
                palette[6] = 0;
                palette[7] = 255;
            }
        }

        public static int Write(BinaryStream bs, YFBitmap image, AlphaMode alphaMode)
        {
            YFColor* pixels = (YFColor*)image.GetPixels().ToPointer();
            YFColor* block = stackalloc YFColor[16];
            YFColor* palette = stackalloc YFColor[4];
            byte* alphaPalette = stackalloc byte[8];
            for (int y = 0; y < image.Height; y += 4)
            {
                for (int x = 0; x < image.Width; x += 4)
                {
                    for (int i = 0; i < 16; i++)
                    {
                        // Extend edge pixels so partial blocks do not acquire a black border.
                        int px = Math.Min(x + (i & 3), image.Width - 1);
                        int py = Math.Min(y + (i >> 2), image.Height - 1);
                        block[i] = pixels[py * image.Width + px];
                    }
                    if (alphaMode == AlphaMode.Explicit)
                    {
                        ulong alpha = 0;
                        for (int i = 0; i < 16; i++)
                            alpha |= (ulong)((block[i].Alpha + 8) / 17) << (i * 4);
                        bs.WriteUInt64(alpha, Endian.Small);
                    }
                    else if (alphaMode == AlphaMode.Interpolated)
                    {
                        byte minAlpha = 255, maxAlpha = 0;
                        for (int i = 0; i < 16; i++)
                        {
                            minAlpha = Math.Min(minAlpha, block[i].Alpha);
                            maxAlpha = Math.Max(maxAlpha, block[i].Alpha);
                        }
                        BuildAlphaPalette(maxAlpha, minAlpha, alphaPalette);
                        ulong alpha = (ulong)maxAlpha | ((ulong)minAlpha << 8);
                        for (int i = 0; i < 16; i++)
                        {
                            int best = 0, error = int.MaxValue;
                            for (int j = 0; j < 8; j++)
                            {
                                int distance = Math.Abs(block[i].Alpha - alphaPalette[j]);
                                if (distance < error) { best = j; error = distance; }
                            }
                            alpha |= (ulong)best << (16 + i * 3);
                        }
                        bs.WriteUInt64(alpha, Endian.Small);
                    }
                    YFColor min, max;
                    DXTEncode.GetMinMaxColorsByEuclideanDistance(block, &min, &max);
                    ushort color0 = DXTEncode.ColorTo555(max), color1 = DXTEncode.ColorTo565(min);
                    BuildColorPalette(color0, color1, palette);
                    uint indices = 0;
                    for (int i = 0; i < 16; i++)
                    {
                        int best = 0, error = int.MaxValue;
                        for (int j = 0; j < 4; j++)
                        {
                            int dr = block[i].Red - palette[j].Red;
                            int dg = block[i].Green - palette[j].Green;
                            int db = block[i].Blue - palette[j].Blue;
                            int distance = dr * dr + dg * dg + db * db;
                            if (distance < error) { best = j; error = distance; }
                        }
                        indices |= (uint)best << (i * 2);
                    }
                    bs.WriteUInt16(color0, Endian.Small);
                    bs.WriteUInt16(color1, Endian.Small);
                    bs.WriteUInt32(indices, Endian.Small);
                }
            }
            // Preserve the PTX caller's existing pitch convention.
            return alphaMode == AlphaMode.None ? image.Width >> 1 : image.Width;
        }
    }
}
