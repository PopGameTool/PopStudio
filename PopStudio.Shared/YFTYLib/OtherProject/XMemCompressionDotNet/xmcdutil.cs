#nullable enable
using System;

namespace XMemCompressionDotNet;

internal static partial class XCompress
{
    internal static byte[] XMCDMemAlloc(int size)
    {
        return GC.AllocateUninitializedArray<byte>(size);
    }

    internal static void XMCDMemFree(byte[]? memory)
    {
        _ = memory;
    }
}

