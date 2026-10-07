#nullable enable
using System;

namespace XMemCompressionDotNet;

internal static partial class XCompress
{
    internal static int GetHResultFromDResult(int dResult)
    {
        if (dResult == 0)
        {
            return 0;
        }

        if (dResult == 1)
        {
            return XMCD_E_OUTOFMEMORY;
        }

        if (dResult == 6)
        {
            return XMCD_E_BADPARAM;
        }

        return XMCD_E_FAIL;
    }

    internal static LzxDecompressionContext XMemInitializeDecompressionContextLzx(
        in XMEMCODEC_PARAMETERS_LZX pLzxParams,
        uint flags)
    {
        LzxDecompressionContext context = new()
        {
            Identifier = 0x76C3F251u,
            CodecType = XMEMCODEC_TYPE.XMEMCODEC_LZX,
            Flags = flags,
        };

        InitializeDecompressionContextLzx(pLzxParams, flags, context);
        return context;
    }

    internal static void XMemDestroyDecompressionContextLzx(LzxDecompressionContext context)
    {
        DestroyDecompressionContextLzx(context);
    }

    internal static int XMemGetDecompressionContextSizeLzx(
        in XMEMCODEC_PARAMETERS_LZX pLzxParams,
        uint flags)
    {
        return GetDecompressionContextSizeLzx(pLzxParams, flags);
    }

    internal static int XMemResetDecompressionContextLzx(LzxDecompressionContext context)
    {
        return GetHResultFromDResult(ResetDecompressionContextLzx(context));
    }

    internal static int XMemDecompressLzx(
        LzxDecompressionContext context,
        Span<byte> destination,
        ref nuint destSize,
        ReadOnlySpan<byte> source)
    {
        if (source.Length == 0)
        {
            destSize = 0;
            return 0;
        }

        return GetHResultFromDResult(DecompressLzx(context, destination, ref destSize, source));
    }

    internal static int XMemDecompressStreamLzx(
        LzxDecompressionContext context,
        Span<byte> destination,
        ref nuint destSize,
        ReadOnlySpan<byte> source,
        ref nuint srcSize)
    {
        return GetHResultFromDResult(DecompressStreamLzx(context, destination, ref destSize, source, ref srcSize));
    }

    internal static int XMemDecompressSegmentTDLzx(
        LzxDecompressionContext context,
        Span<byte> destination,
        ref nuint destSize,
        ReadOnlySpan<byte> source,
        nuint segmentSize,
        nuint segmentOffset)
    {
        DecompressSegmentTDLzx(context, destination, ref destSize, source, segmentSize, segmentOffset);
        return 0;
    }
}
