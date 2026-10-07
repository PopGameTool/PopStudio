#nullable enable
using System;

namespace XMemCompressionDotNet;

public static partial class XMem
{
    public static int XMemGetDecompressionContextSize(
        XMEMCODEC_TYPE codecType,
        in XMEMCODEC_PARAMETERS_LZX codecParams,
        uint flags)
    {
        return XCompress.IsSupportedCodec(codecType)
            ? XCompress.XMemGetDecompressionContextSizeLzx(codecParams, flags)
            : 0;
    }

    public static int XMemCreateDecompressionContext(
        XMEMCODEC_TYPE codecType,
        in XMEMCODEC_PARAMETERS_LZX codecParams,
        uint flags,
        out XMemDecompressionContext? context)
    {
        context = null;
        if (!XCompress.IsSupportedCodec(codecType))
        {
            return XCompress.XMCD_E_FAIL;
        }

        context = new XMemDecompressionContext(codecType, codecParams, flags);
        return XCompress.S_OK;
    }

    public static int XMemResetDecompressionContext(XMemDecompressionContext context)
    {
        return XCompress.IsLzxCodec(context.CodecType)
            ? XCompress.XMemResetDecompressionContextLzx(context.Lzx)
            : XCompress.XMCD_E_FAIL;
    }

    public static int XMemDecompress(
        XMemDecompressionContext context,
        Span<byte> destination,
        ref nuint destSize,
        ReadOnlySpan<byte> source)
    {
        return XCompress.IsLzxCodec(context.CodecType)
            ? XCompress.XMemDecompressLzx(context.Lzx, destination, ref destSize, source)
            : XCompress.XMCD_E_FAIL;
    }

    public static int XMemDecompressStream(
        XMemDecompressionContext context,
        Span<byte> destination,
        ref nuint destSize,
        ReadOnlySpan<byte> source,
        ref nuint srcSize)
    {
        return XCompress.IsLzxCodec(context.CodecType)
            ? XCompress.XMemDecompressStreamLzx(context.Lzx, destination, ref destSize, source, ref srcSize)
            : XCompress.XMCD_E_FAIL;
    }

    public static int XMemDecompressSegmentTD(
        XMemDecompressionContext context,
        Span<byte> destination,
        ref nuint destSize,
        ReadOnlySpan<byte> source,
        nuint segmentSize,
        nuint segmentOffset)
    {
        return XCompress.XMemDecompressSegmentTDLzx(
            context.Lzx,
            destination,
            ref destSize,
            source,
            segmentSize,
            segmentOffset);
    }

    public static void XMemDestroyDecompressionContext(XMemDecompressionContext context)
    {
        if (XCompress.IsLzxCodec(context.CodecType))
        {
            XCompress.XMemDestroyDecompressionContextLzx(context.Lzx);
        }
    }
}

