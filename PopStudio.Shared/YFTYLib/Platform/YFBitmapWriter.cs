using System.Runtime.InteropServices;

namespace PopStudio.Platform
{
    /// <summary>
    /// 共享的位图写入器。只依赖 YFBitmap 的像素接口，这样 BMP 输出在三端字节级一致，
    /// 各平台实现只需要各自补一个 JPEG 编码器。
    /// </summary>
    public static unsafe class YFBitmapWriter
    {
        /// <summary>
        /// 写 32bpp BI_RGB 的 BMP。高度取正数即自下而上存储，与源位图的行序无关。
        /// </summary>
        public static void WriteBmp(Stream stream, YFBitmap bitmap)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            int rowBytes = width * 4;
            long imageBytes = (long)rowBytes * height;
            // BMP 头里的尺寸字段是 32 位，装不下就直说
            if (imageBytes + 54 > int.MaxValue) throw new Exception(Str.Obj.ImageTooLarge);
            byte[] head = new byte[54];
            head[0] = (byte)'B';
            head[1] = (byte)'M';
            WriteInt32(head, 2, (int)(54 + imageBytes));   // bfSize
            WriteInt32(head, 6, 0);                 // bfReserved
            WriteInt32(head, 10, 54);               // bfOffBits
            WriteInt32(head, 14, 40);               // biSize
            WriteInt32(head, 18, width);
            WriteInt32(head, 22, height);
            WriteInt16(head, 26, 1);                // biPlanes
            WriteInt16(head, 28, 32);               // biBitCount
            WriteInt32(head, 30, 0);                // biCompression = BI_RGB
            WriteInt32(head, 34, (int)imageBytes);       // biSizeImage
            WriteInt32(head, 38, 0);                // biXPelsPerMeter
            WriteInt32(head, 42, 0);                // biYPelsPerMeter
            WriteInt32(head, 46, 0);                // biClrUsed
            WriteInt32(head, 50, 0);                // biClrImportant
            stream.Write(head, 0, head.Length);
            IntPtr pixels = bitmap.GetPixels();
            int stride = bitmap.Stride;
            bool bottomUp = bitmap.IsBottomUp;
            byte[] row = new byte[rowBytes];
            // 行距取绝对值：Stride 理论上可能是负的，直接用它会往低地址跑
            long rowStride = Math.Abs(stride);
            for (int y = 0; y < height; y++)
            {
                int sy = bottomUp ? y : height - 1 - y;
                byte* src = (byte*)pixels + (long)sy * rowStride;
                Marshal.Copy((IntPtr)src, row, 0, rowBytes);
                stream.Write(row, 0, rowBytes);
            }
        }

        static void WriteInt16(byte[] buffer, int offset, int value)
        {
            buffer[offset] = (byte)value;
            buffer[offset + 1] = (byte)(value >> 8);
        }

        static void WriteInt32(byte[] buffer, int offset, int value)
        {
            buffer[offset] = (byte)value;
            buffer[offset + 1] = (byte)(value >> 8);
            buffer[offset + 2] = (byte)(value >> 16);
            buffer[offset + 3] = (byte)(value >> 24);
        }
    }
}
