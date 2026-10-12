namespace PopStudio.Image.PtxPS3
{
    internal class PtxHead
    {
        public static readonly string magic = "DDS ";
        public static readonly string nvt = "NVTT";
        public string tex = "DXT5";

        public int height;
        public int width;
        public int texturesize;

        public int GetTextureSize()
        {
            int blockSize = tex switch
            {
                "ATC " => 8,
                "DXT5" or "ATCA" or "ATCI" => 16,
                _ => throw new NotSupportedException($"Unsupported DDS FourCC: '{tex}'.")
            };
            if (width <= 0 || height <= 0 || (long)width * height > int.MaxValue / 4)
                throw new InvalidDataException("Invalid DDS dimensions.");
            return checked((int)(((width + 3L) / 4) * ((height + 3L) / 4) * blockSize));
        }

        public PtxHead Read(BinaryStream bs)
        {
            if (bs.Length - bs.Position < 128)
                throw new InvalidDataException("Truncated DDS header.");
            bs.IdString(magic);
            bs.IdInt32(0x7C);
            bs.ReadInt32(); // DDS flags vary between writers.
            height = bs.ReadInt32();
            width = bs.ReadInt32();
            texturesize = bs.ReadInt32();
            int depth = bs.ReadInt32();
            bs.ReadInt32(); // Decode only the largest mip level.
            bs.Position += 44; // Reserved fields, including optional writer signatures.
            bs.IdInt32(32);
            if ((bs.ReadInt32() & 4) == 0)
                throw new NotSupportedException("DDS pixel format must contain a FourCC.");
            tex = bs.ReadString(4);
            bs.Position += 20; // RGB bit count and masks do not apply to block compression.
            bs.ReadInt32(); // Caps may include mipmaps/complex even for a single level.
            int caps2 = bs.ReadInt32();
            bs.Position += 12;
            if (depth > 1 || (caps2 & 0x20FE00) != 0)
                throw new NotSupportedException("DDS cubemaps and volume textures are not supported.");
            int requiredSize = GetTextureSize();
            if (bs.Length - bs.Position < requiredSize)
                throw new InvalidDataException("Truncated DDS texture data.");
            return this;
        }

        public void Write(BinaryStream bs)
        {
            texturesize = GetTextureSize();
            bs.WriteString(magic);
            bs.WriteInt32(0x7C);
            bs.WriteInt32(528391);
            bs.WriteInt32(height);
            bs.WriteInt32(width);
            bs.WriteInt32(texturesize);
            for (int i = 0; i < 11; i++)
            {
                bs.WriteInt32(0);
            }
            bs.WriteString(nvt);
            bs.WriteInt32(131080);
            bs.WriteInt32(32);
            bs.WriteInt32(4);
            bs.WriteString(tex);
            for (int i = 0; i < 5; i++)
            {
                bs.WriteInt32(0);
            }
            bs.WriteInt32(4096);
            for (int i = 0; i < 4; i++)
            {
                bs.WriteInt32(0);
            }
        }
    }
}
