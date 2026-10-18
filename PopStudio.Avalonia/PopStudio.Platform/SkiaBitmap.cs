using SkiaSharp;

namespace PopStudio.Platform
{
    /// <summary>
    /// It is slower than GDIBitmap
    /// </summary>
    public class SkiaBitmap : YFBitmap
    {
        public override int Width => m_width;
        public override int Height => m_height;

        readonly SKBitmap m_bitmap;
        readonly int m_width;
        readonly int m_height;

        public SkiaBitmap()
        {
        }

        public SkiaBitmap(int width, int height)
        {
            m_bitmap = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
            m_width = m_bitmap.Width;
            m_height = m_bitmap.Height;
        }

        public SkiaBitmap(Stream stream)
        {
            using (SKCodec sKCodec = SKCodec.Create(stream))
            {
                m_bitmap = SKBitmap.Decode(sKCodec, new SKImageInfo
                {
                    ColorType = SKColorType.Bgra8888,
                    AlphaType = SKAlphaType.Unpremul,
                    ColorSpace = null,
                    Width = sKCodec.Info.Width,
                    Height = sKCodec.Info.Height
                });
            }
            m_width = m_bitmap.Width;
            m_height = m_bitmap.Height;
        }

        public SkiaBitmap(string filePath)
        {
            using (SKCodec sKCodec = SKCodec.Create(filePath))
            {
                m_bitmap = SKBitmap.Decode(sKCodec, new SKImageInfo
                {
                    ColorType = SKColorType.Bgra8888,
                    AlphaType = SKAlphaType.Unpremul,
                    ColorSpace = null,
                    Width = sKCodec.Info.Width,
                    Height = sKCodec.Info.Height
                });
            }
            m_width = m_bitmap.Width;
            m_height = m_bitmap.Height;
        }

        protected override YFBitmap InternalCreate(int width, int height) => new SkiaBitmap(width, height);
        protected override YFBitmap InternalCreate(Stream stream) => new SkiaBitmap(stream);
        protected override YFBitmap InternalCreate(string filePath) => new SkiaBitmap(filePath);

        /// <summary>
        /// BB GG RR AA
        /// </summary>
        /// <returns></returns>
        public override nint GetPixels() => m_bitmap.GetPixels();

        public override void Save(string filePath)
        {
            using (SKPixmap sKPixmap = m_bitmap.PeekPixels())
            {
                using (SKData p = sKPixmap?.Encode(new SKPngEncoderOptions(SKPngEncoderFilterFlags.NoFilters, 1)))
                {
                    using (FileStream fs = new FileStream(filePath, FileMode.Create))
                    {
                        byte[] t = p.ToArray();
                        fs.Write(t, 0, t.Length);
                        t = null;
                    }
                }
            }
        }

        public override void Save(Stream stream)
        {
            using (SKPixmap sKPixmap = m_bitmap.PeekPixels())
            {
                using (SKData p = sKPixmap?.Encode(new SKPngEncoderOptions(SKPngEncoderFilterFlags.NoFilters, 1)))
                {
                    byte[] t = p.ToArray();
                    stream.Write(t, 0, t.Length);
                    t = null;
                }
            }
        }

        public override void Save(Stream stream, YFBitmapSaveOptions options)
        {
            if (options.Format == YFBitmapFormat.Jpeg)
            {
                using (SKPixmap sKPixmap = m_bitmap.PeekPixels())
                {
                    if (sKPixmap == null) throw new Exception(Str.Obj.UnknownFormat);
                    sKPixmap.Encode(stream, SKEncodedImageFormat.Jpeg, options.Quality);
                }
                return;
            }
            base.Save(stream, options);
        }

        public override void Dispose() => m_bitmap?.Dispose();
    }
}