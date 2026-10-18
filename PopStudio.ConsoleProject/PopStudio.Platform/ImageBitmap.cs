#if !WINDOWSCONSOLE
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System.Buffers;
using SixLabors.ImageSharp.Formats;

namespace PopStudio.Platform
{
    /// <summary>
    /// It is slower than GDIBitmap
    /// </summary>
    public class ImageBitmap : YFBitmap
    {
        public override int Width => m_width;
        public override int Height => m_height;

        readonly Image<Bgra32> m_image;
        readonly MemoryHandle m_handle;
        readonly int m_width;
        readonly int m_height;
        readonly bool nullhandle = false;

        public ImageBitmap()
        {
            nullhandle = true;
        }

        public ImageBitmap(int width, int height)
        {
            Configuration customConfig = Configuration.Default.Clone();
            customConfig.PreferContiguousImageBuffers = true;
            m_image = new Image<Bgra32>(customConfig, width, height);
            if (!m_image.DangerousTryGetSinglePixelMemory(out Memory<Bgra32> memory))
            {
                throw new Exception(
                    "This can only happen with multi-GB images or when PreferContiguousImageBuffers is not set to true.");
            }
            m_width = m_image.Width;
            m_height = m_image.Height;
            m_handle = memory.Pin();
        }

        public unsafe ImageBitmap(Stream stream)
        {
            Configuration customConfig = Configuration.Default.Clone();
            customConfig.PreferContiguousImageBuffers = true;
            m_image = SixLabors.ImageSharp.Image.Load<Bgra32>(new DecoderOptions { Configuration = customConfig }, stream);
            if (!m_image.DangerousTryGetSinglePixelMemory(out Memory<Bgra32> memory))
            {
                throw new Exception(
                    "This can only happen with multi-GB images or when PreferContiguousImageBuffers is not set to true.");
            }
            m_width = m_image.Width;
            m_height = m_image.Height;
            m_handle = memory.Pin();
        }

        public unsafe ImageBitmap(string filePath)
        {
            Configuration customConfig = Configuration.Default.Clone();
            customConfig.PreferContiguousImageBuffers = true;
            m_image = SixLabors.ImageSharp.Image.Load<Bgra32>(new DecoderOptions { Configuration = customConfig }, filePath);
            if (!m_image.DangerousTryGetSinglePixelMemory(out Memory<Bgra32> memory))
            {
                throw new Exception(
                    "This can only happen with multi-GB images or when PreferContiguousImageBuffers is not set to true.");
            }
            m_width = m_image.Width;
            m_height = m_image.Height;
            m_handle = memory.Pin();
        }

        protected override YFBitmap InternalCreate(int width, int height) => new ImageBitmap(width, height);
        protected override YFBitmap InternalCreate(Stream stream) => new ImageBitmap(stream);
        protected override YFBitmap InternalCreate(string filePath) => new ImageBitmap(filePath);

        /// <summary>
        /// BB GG RR AA
        /// </summary>
        /// <returns></returns>
        public override unsafe nint GetPixels() => new IntPtr(m_handle.Pointer);

        public override unsafe void Save(string filePath)
        {
            m_image.SaveAsPng(filePath, new PngEncoder
            {
                FilterMethod = PngFilterMethod.None,
                CompressionLevel = PngCompressionLevel.Level1
            });
        }

        public override unsafe void Save(Stream stream)
        {
            m_image.SaveAsPng(stream, new PngEncoder
            {
                FilterMethod = PngFilterMethod.None,
                CompressionLevel = PngCompressionLevel.Level1
            });
        }

        public override void Save(Stream stream, YFBitmapSaveOptions options)
        {
            if (options.Format == YFBitmapFormat.Jpeg)
            {
                m_image.SaveAsJpeg(stream, new JpegEncoder { Quality = options.Quality });
                return;
            }
            base.Save(stream, options);
        }

        public override void Dispose()
        {
            if (!nullhandle) m_handle.Dispose();
            m_image?.Dispose();
        }
    }
}
#endif