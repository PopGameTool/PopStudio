#nullable enable
using System;

namespace XMemCompressionDotNet;

public enum XMEMCODEC_TYPE
{
    XMEMCODEC_DEFAULT = 0,
    XMEMCODEC_LZX = 1,
}

public readonly struct XMEMCODEC_PARAMETERS_LZX
{
    public XMEMCODEC_PARAMETERS_LZX(uint flags, uint windowSize, uint compressionPartitionSize)
    {
        Flags = flags;
        WindowSize = windowSize;
        CompressionPartitionSize = compressionPartitionSize;
    }

    public uint Flags { get; }

    public uint WindowSize { get; }

    public uint CompressionPartitionSize { get; }
}

internal static partial class XCompress
{
    internal const int S_OK = 0;
    internal const int XMCD_E_FAIL = unchecked((int)0x80004005u);
    internal const int XMCD_E_OUTOFMEMORY = unchecked((int)0x8007000Eu);
    internal const int XMCD_E_BADPARAM = unchecked((int)0x81DE2001u);
    internal const uint XMEM_TD_FLAG = 0x80000000u;
    internal const uint XMCD_CONTEXT_FLAG_OWNS_HEAP = 0x40000000u;

    internal static bool IsLzxCodec(XMEMCODEC_TYPE codecType)
    {
        return codecType == XMEMCODEC_TYPE.XMEMCODEC_LZX;
    }

    internal static bool IsSupportedCodec(XMEMCODEC_TYPE codecType)
    {
        return codecType is XMEMCODEC_TYPE.XMEMCODEC_DEFAULT or XMEMCODEC_TYPE.XMEMCODEC_LZX;
    }

    internal static XMEMCODEC_PARAMETERS_LZX ReplaceDefaultParameters(in XMEMCODEC_PARAMETERS_LZX parameters)
    {
        uint windowSize = parameters.WindowSize == 0 ? 0x20000u : parameters.WindowSize;
        uint partitionSize = parameters.CompressionPartitionSize == 0 ? 0x80000u : parameters.CompressionPartitionSize;
        return new XMEMCODEC_PARAMETERS_LZX(parameters.Flags, windowSize, partitionSize);
    }

    internal static XMEMCODEC_PARAMETERS_LZX ReplaceDefaultDecompressionParameters(in XMEMCODEC_PARAMETERS_LZX parameters)
    {
        uint windowSize = parameters.WindowSize == 0 ? 0x20000u : parameters.WindowSize;
        return new XMEMCODEC_PARAMETERS_LZX(parameters.Flags, windowSize, parameters.CompressionPartitionSize);
    }
}

public sealed class XMemCompressionContext
{
    internal XMemCompressionContext(XMEMCODEC_TYPE codecType, XMEMCODEC_PARAMETERS_LZX parameters, uint flags)
    {
        CodecType = codecType == XMEMCODEC_TYPE.XMEMCODEC_DEFAULT ? XMEMCODEC_TYPE.XMEMCODEC_LZX : codecType;
        Parameters = XCompress.ReplaceDefaultParameters(parameters);
        Flags = flags;
        Lzx = XCompress.XMemInitializeCompressionContextLzx(Parameters, flags);
    }

    internal XMEMCODEC_TYPE CodecType { get; }

    internal XMEMCODEC_PARAMETERS_LZX Parameters { get; }

    internal uint Flags { get; }

    internal LzxCompressionContext Lzx { get; }
}

public sealed class XMemDecompressionContext
{
    internal XMemDecompressionContext(XMEMCODEC_TYPE codecType, XMEMCODEC_PARAMETERS_LZX parameters, uint flags)
    {
        CodecType = codecType == XMEMCODEC_TYPE.XMEMCODEC_DEFAULT ? XMEMCODEC_TYPE.XMEMCODEC_LZX : codecType;
        Parameters = XCompress.ReplaceDefaultDecompressionParameters(parameters);
        Flags = flags;
        Lzx = XCompress.XMemInitializeDecompressionContextLzx(Parameters, flags);
    }

    internal XMEMCODEC_TYPE CodecType { get; }

    internal XMEMCODEC_PARAMETERS_LZX Parameters { get; }

    internal uint Flags { get; }

    internal LzxDecompressionContext Lzx { get; }
}

