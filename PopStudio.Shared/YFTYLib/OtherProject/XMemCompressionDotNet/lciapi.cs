#nullable enable
using System;

namespace XMemCompressionDotNet;

internal static partial class XCompress
{
    internal static int GetHResultFromCResult(int cResult)
    {
        if (cResult == 0)
        {
            return 0;
        }

        if (cResult == 1)
        {
            return XMCD_E_OUTOFMEMORY;
        }

        if (cResult is 6 or 7)
        {
            return XMCD_E_BADPARAM;
        }

        return XMCD_E_FAIL;
    }

    internal static void XMemDestroyCompressionContextLzx(LzxCompressionContext context)
    {
        DestroyCompressionContextLzx(context);
    }

    internal static int XMemGetCompressionContextSizeLzx(in XMEMCODEC_PARAMETERS_LZX pLzxParams, uint flags)
    {
        return GetCompressionContextSizeLzx(pLzxParams, flags);
    }

    internal static LzxCompressionContext XMemInitializeCompressionContextLzx(
        in XMEMCODEC_PARAMETERS_LZX pLzxParams,
        uint flags)
    {
        LzxCompressionContext context = new()
        {
            Identifier = 0x76C3F250u,
            CodecType = XMEMCODEC_TYPE.XMEMCODEC_LZX,
            Flags = flags,
        };

        InitializeCompressionContextLzx(pLzxParams, flags, context);
        return context;
    }

    internal static int XMemResetCompressionContextLzx(LzxCompressionContext context)
    {
        return GetHResultFromCResult(ResetCompressionContextLzx(context));
    }

    internal static int XMemBeginCompressionTDLzx(LzxCompressionContext context, nuint segmentPitch)
    {
        return GetHResultFromCResult(BeginCompressionTDLzx(context, segmentPitch));
    }

    internal static int XMemEndCompressionTDLzx(
        LzxCompressionContext context,
        Span<byte> headerData,
        ref nuint headerSize,
        float threshold)
    {
        return GetHResultFromCResult(EndCompressionTDLzx(context, headerData, ref headerSize, threshold));
    }

    internal static int XMemCompressSegmentTDLzx(
        LzxCompressionContext context,
        Span<byte> destination,
        ref nuint destSize,
        ReadOnlySpan<byte> source,
        ref nuint srcSize,
        float threshold)
    {
        if (srcSize == 0)
        {
            destSize = 0;
            return 0;
        }

        return GetHResultFromCResult(
            CompressSegmentTDLzx(context, destination, ref destSize, source, ref srcSize, threshold));
    }

    internal static int XMemCompressLzx(
        LzxCompressionContext context,
        Span<byte> destination,
        ref nuint destSize,
        ReadOnlySpan<byte> source)
    {
        if (source.Length == 0)
        {
            destSize = 0;
            return 0;
        }

        return GetHResultFromCResult(CompressLzx(context, destination, ref destSize, source));
    }

    internal static int XMemCompressStreamLzx(
        LzxCompressionContext context,
        Span<byte> destination,
        ref nuint destSize,
        ReadOnlySpan<byte> source,
        ref nuint srcSize)
    {
        return GetHResultFromCResult(CompressStreamLzx(context, destination, ref destSize, source, ref srcSize));
    }
}

