namespace PopStudio.Platform
{
    /// <summary>
    /// 位图输出格式
    /// </summary>
    public enum YFBitmapFormat
    {
        Png = 0,
        Jpeg = 1,
        Bmp = 2
    }

    /// <summary>
    /// 位图保存参数。Quality 只有 JPEG 使用，构造时钳制到 1..100。
    /// </summary>
    public readonly struct YFBitmapSaveOptions
    {
        public YFBitmapFormat Format { get; }
        public int Quality { get; }

        public YFBitmapSaveOptions(YFBitmapFormat format, int quality = 90)
        {
            Format = format;
            if (quality < 1) quality = 1;
            else if (quality > 100) quality = 100;
            Quality = quality;
        }

        public static YFBitmapSaveOptions Png => new YFBitmapSaveOptions(YFBitmapFormat.Png);
    }

    public abstract unsafe class YFBitmap : IDisposable
    {
        internal static YFBitmap InternalCreateBitmap;

        public static void RegistPlatform<T>() where T : YFBitmap, new() => InternalCreateBitmap = new T();

        public static void RegistPlatform(object o) => InternalCreateBitmap = (o is YFBitmap bitmap) ? bitmap : InternalCreateBitmap;

        public static YFBitmap Create(int width, int height) => InternalCreateBitmap?.InternalCreate(width, height);

        public static YFBitmap Create(Stream stream) => InternalCreateBitmap?.InternalCreate(stream);

        public static YFBitmap Create(string filePath) => InternalCreateBitmap?.InternalCreate(filePath);


        public abstract int Width { get; }
        public abstract int Height { get; }
        /// <summary>
        /// must be BB GG RR AA
        /// </summary>
        /// <returns></returns>
        public abstract IntPtr GetPixels();
        public abstract void Save(string filePath);
        public abstract void Save(Stream stream);
        /// <summary>
        /// 每行字节数。GDI 自下而上时可能为负
        /// </summary>
        public virtual int Stride => Width * 4;
        /// <summary>
        /// 像素是否自下而上排列
        /// </summary>
        public virtual bool IsBottomUp => false;
        /// <summary>
        /// 按指定格式保存。PNG 复用现有实现，其余格式统一走流式重载
        /// （BMP 走共享写入器，JPEG 由各平台覆写）。
        /// </summary>
        public virtual void Save(string filePath, YFBitmapSaveOptions options)
        {
            if (options.Format == YFBitmapFormat.Png)
            {
                Save(filePath);
                return;
            }
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                Save(fs, options);
            }
        }
        public virtual void Save(Stream stream, YFBitmapSaveOptions options)
        {
            switch (options.Format)
            {
                case YFBitmapFormat.Png: Save(stream); return;
                case YFBitmapFormat.Bmp: YFBitmapWriter.WriteBmp(stream, this); return;
                default: throw new NotSupportedException(Str.Obj.UnknownFormat);
            }
        }
        /// <summary>
        /// 缩放到指定尺寸。基类用面积平均下采样实现（按 alpha 加权，避免透明像素渗色），
        /// 四个平台实现共用，不做预乘 alpha 的改写。
        /// </summary>
        public virtual YFBitmap Resize(int width, int height)
        {
            if (width < 1) width = 1;
            if (height < 1) height = 1;
            YFBitmap ans = Create(width, height);
            uint* res = (uint*)GetPixels().ToPointer();
            uint* dec = (uint*)ans.GetPixels().ToPointer();
            // 源行距（以 uint 计）。Stride 理论上可能是负的（自下而上的位图），
            // 这里按绝对值当行距以免往回越界；行序对面积平均没有影响。
            long srcStride = Math.Abs(Stride) / 4;
            if (srcStride < Width) srcStride = Width;
            if (width == Width && height == Height)
            {
                for (int y = 0; y < height; y++)
                {
                    uint* srow = res + (long)y * srcStride;
                    uint* drow = dec + (long)y * width;
                    for (int x = 0; x < width; x++)
                    {
                        drow[x] = srow[x];
                    }
                }
                return ans;
            }
            for (int y = 0; y < height; y++)
            {
                int sy0 = (int)((long)y * Height / height);
                int sy1 = (int)(((long)(y + 1) * Height + height - 1) / height);
                if (sy1 <= sy0) sy1 = sy0 + 1;
                if (sy1 > Height) sy1 = Height;
                for (int x = 0; x < width; x++)
                {
                    int sx0 = (int)((long)x * Width / width);
                    int sx1 = (int)(((long)(x + 1) * Width + width - 1) / width);
                    if (sx1 <= sx0) sx1 = sx0 + 1;
                    if (sx1 > Width) sx1 = Width;
                    long sa = 0, sr = 0, sg = 0, sb = 0;
                    for (int sy = sy0; sy < sy1; sy++)
                    {
                        uint* row = res + (long)sy * srcStride;
                        for (int sx = sx0; sx < sx1; sx++)
                        {
                            uint c = row[sx];
                            long a = c >> 24 & 0xFF;
                            sa += a;
                            sr += (c >> 16 & 0xFF) * a;
                            sg += (c >> 8 & 0xFF) * a;
                            sb += (c & 0xFF) * a;
                        }
                    }
                    uint outa = (uint)(sa / ((long)(sy1 - sy0) * (sx1 - sx0)));
                    uint outr, outg, outb;
                    if (sa == 0)
                    {
                        outr = outg = outb = 0;
                    }
                    else
                    {
                        outr = (uint)(sr / sa);
                        outg = (uint)(sg / sa);
                        outb = (uint)(sb / sa);
                    }
                    dec[(long)y * width + x] = outa << 24 | outr << 16 | outg << 8 | outb;
                }
            }
            return ans;
        }
        protected abstract YFBitmap InternalCreate(int width, int height);
        protected abstract YFBitmap InternalCreate(Stream stream);
        protected abstract YFBitmap InternalCreate(string filePath);
        public abstract void Dispose();

        public virtual int Square => Width * Height;
        public virtual void MoveTo(YFBitmap decimg, int mX, int mY)
        {
            uint* res = (uint*)GetPixels().ToPointer();
            uint* dec = (uint*)decimg.GetPixels().ToPointer();
            for (int y = 0; y < Height; y++)
            {
                uint* resraw = res + y * Width;
                uint* decraw = dec + (y + mY) * decimg.Width + mX;
                for (int x = 0; x < Width; x++)
                {
                    if (((x + mX) >= decimg.Width) || ((y + mY) >= decimg.Height) || ((x + mX) < 0) || ((y + mY) < 0))
                    {
                        decraw++;
                        resraw++;
                    }
                    else
                    {
                        *decraw++ = *resraw++;
                    }
                }
            }
        }
        public virtual YFBitmap Cut(int mX, int mY, int mWidth, int mHeight)
        {
            YFBitmap ans = Create(mWidth, mHeight);
            uint* res = (uint*)GetPixels().ToPointer();
            uint* dec = (uint*)ans.GetPixels().ToPointer();
            for (int y = 0; y < mHeight; y++)
            {
                uint* decraw = dec + y * mWidth;
                uint* resraw = res + (y + mY) * Width + mX;
                for (int x = 0; x < mWidth; x++)
                {
                    if (((x + mX) >= Width) || ((y + mY) >= Height) || ((x + mX) < 0) || ((y + mY) < 0))
                    {
                        *decraw++ = 0;
                        resraw++;
                    }
                    else
                    {
                        *decraw++ = *resraw++;
                    }
                }
            }
            return ans;
        }
        public virtual YFBitmap Rotate0()
        {
            int resH = Height;
            int resW = Width;
            YFBitmap N = Create(resW, resH);
            uint* res = (uint*)GetPixels().ToPointer();
            uint* dec = (uint*)N.GetPixels().ToPointer();
            for (int y = 0; y < resH; y++)
            {
                for (int x = 0; x < resW; x++)
                {
                    *dec++ = *res++;
                }
            }
            return N;
        }
        public virtual YFBitmap Rotate90()
        {
            int resH = Height;
            int resW = Width;
            YFBitmap N = Create(resH, resW);
            uint* res = (uint*)GetPixels().ToPointer();
            uint* dec = (uint*)N.GetPixels().ToPointer();
            for (int y = 0; y < resH; y++)
            {
                for (int x = 0; x < resW; x++)
                {
                    dec[x * resH + (resH - y - 1)] = *res++;
                }
            }
            return N;
        }
        public virtual YFBitmap Rotate180()
        {
            int resH = Height;
            int resW = Width;
            YFBitmap N = Create(resW, resH);
            uint* res = (uint*)GetPixels().ToPointer();
            uint* dec = (uint*)N.GetPixels().ToPointer() + resH * resW;
            for (int y = 0; y < resH; y++)
            {
                for (int x = 0; x < resW; x++)
                {
                    *--dec = *res++;
                }
            }
            return N;
        }
        public virtual YFBitmap Rotate270()
        {
            int resH = Height;
            int resW = Width;
            YFBitmap N = Create(resH, resW);
            uint* res = (uint*)GetPixels().ToPointer();
            uint* dec = (uint*)N.GetPixels().ToPointer();
            for (int y = 0; y < resH; y++)
            {
                for (int x = 0; x < resW; x++)
                {
                    dec[(resW - x - 1) * resH + y] = *res++;
                }
            }
            return N;
        }
    }
}