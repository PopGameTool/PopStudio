using PopStudio.Platform;

namespace PopStudio.Image.PtxPS3
{
    /// <summary>
    /// DDS textures (including PS3 PTX) with DXT5 or ATC compression.
    /// </summary>
    internal static class Ptx
    {
        public static void Encode(string inFile, string outFile, int format = 0)
        {
            string fourCC = format switch
            {
                0 => "DXT5",
                1 => "ATC ",
                2 => "ATCA",
                3 => "ATCI",
                _ => throw new ArgumentOutOfRangeException(nameof(format))
            };
            using (BinaryStream bs = new BinaryStream(outFile, FileMode.Create))
            {
                using (YFBitmap sKBitmap = YFBitmap.Create(inFile))
                {
                    PtxHead head = new PtxHead
                    {
                        width = sKBitmap.Width,
                        height = sKBitmap.Height,
                        tex = fourCC
                    };
                    head.Write(bs);
                    switch (format)
                    {
                        case 0: Texture.DXT5_RGBA.Write(bs, sKBitmap); break;
                        case 1: Texture.ATC_RGB.Write(bs, sKBitmap); break;
                        case 2: Texture.ATC_RGBA4.Write(bs, sKBitmap); break;
                        case 3: Texture.ATC.Write(bs, sKBitmap, Texture.ATC.AlphaMode.Interpolated); break;
                    }
                }
            }
        }

        public static void Decode(string inFile, string outFile)
        {
            using (BinaryStream bs = new BinaryStream(inFile, FileMode.Open))
            {
                PtxHead head = new PtxHead().Read(bs);
                using (YFBitmap sKBitmap = head.tex switch
                {
                    "DXT5" => Texture.DXT5_RGBA.Read(bs, head.width, head.height),
                    "ATC " => Texture.ATC_RGB.Read(bs, head.width, head.height),
                    "ATCA" => Texture.ATC_RGBA4.Read(bs, head.width, head.height),
                    "ATCI" => Texture.ATC.Read(bs, head.width, head.height, Texture.ATC.AlphaMode.Interpolated),
                    _ => throw new NotSupportedException($"Unsupported DDS FourCC: '{head.tex}'.")
                })
                {
                    sKBitmap.Save(outFile);
                }
            }
        }
    }
}
