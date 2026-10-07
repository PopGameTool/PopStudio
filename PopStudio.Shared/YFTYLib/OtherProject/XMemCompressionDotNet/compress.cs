#nullable enable
using System;

namespace XMemCompressionDotNet;

public static partial class XMem
{
    public static int XMemGetCompressionContextSize(
        XMEMCODEC_TYPE codecType,
        in XMEMCODEC_PARAMETERS_LZX codecParams,
        uint flags)
    {
        return XCompress.IsSupportedCodec(codecType)
            ? XCompress.XMemGetCompressionContextSizeLzx(codecParams, flags)
            : 0;
    }

    public static int XMemCreateCompressionContext(
        XMEMCODEC_TYPE codecType,
        in XMEMCODEC_PARAMETERS_LZX codecParams,
        uint flags,
        out XMemCompressionContext? context)
    {
        context = null;
        if (!XCompress.IsSupportedCodec(codecType))
        {
            return XCompress.XMCD_E_FAIL;
        }

        context = new XMemCompressionContext(codecType, codecParams, flags);
        return XCompress.S_OK;
    }

    public static int XMemResetCompressionContext(XMemCompressionContext context)
    {
        return XCompress.IsLzxCodec(context.CodecType)
            ? XCompress.XMemResetCompressionContextLzx(context.Lzx)
            : XCompress.XMCD_E_FAIL;
    }

    public static int XMemBeginCompressionTD(XMemCompressionContext context, nuint segmentPitch)
    {
        return XCompress.IsLzxCodec(context.CodecType)
            ? XCompress.XMemBeginCompressionTDLzx(context.Lzx, segmentPitch)
            : XCompress.XMCD_E_FAIL;
    }

    public static int XMemEndCompressionTD(
        XMemCompressionContext context,
        Span<byte> headerData,
        ref nuint headerSize,
        float threshold)
    {
        return XCompress.IsLzxCodec(context.CodecType)
            ? XCompress.XMemEndCompressionTDLzx(context.Lzx, headerData, ref headerSize, threshold)
            : XCompress.XMCD_E_FAIL;
    }

    public static int XMemCompressSegmentTD(
        XMemCompressionContext context,
        Span<byte> destination,
        ref nuint destSize,
        ReadOnlySpan<byte> source,
        ref nuint srcSize,
        float threshold)
    {
        return XCompress.IsLzxCodec(context.CodecType)
            ? XCompress.XMemCompressSegmentTDLzx(context.Lzx, destination, ref destSize, source, ref srcSize, threshold)
            : XCompress.XMCD_E_FAIL;
    }

    public static int XMemCompress(
        XMemCompressionContext context,
        Span<byte> destination,
        ref nuint destSize,
        ReadOnlySpan<byte> source)
    {
        return XCompress.IsLzxCodec(context.CodecType)
            ? XCompress.XMemCompressLzx(context.Lzx, destination, ref destSize, source)
            : XCompress.XMCD_E_FAIL;
    }

    public static int XMemCompressStream(
        XMemCompressionContext context,
        Span<byte> destination,
        ref nuint destSize,
        ReadOnlySpan<byte> source,
        ref nuint srcSize)
    {
        return XCompress.IsLzxCodec(context.CodecType)
            ? XCompress.XMemCompressStreamLzx(context.Lzx, destination, ref destSize, source, ref srcSize)
            : XCompress.XMCD_E_FAIL;
    }

    public static void XMemDestroyCompressionContext(XMemCompressionContext context)
    {
        if (XCompress.IsLzxCodec(context.CodecType))
        {
            XCompress.XMemDestroyCompressionContextLzx(context.Lzx);
        }
    }
}

