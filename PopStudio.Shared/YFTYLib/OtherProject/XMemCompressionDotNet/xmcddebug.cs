#nullable enable
using System.Diagnostics;

namespace XMemCompressionDotNet;

internal static partial class XCompress
{
    internal static void XMCDPRINT(string format)
    {
        Debug.Write(format);
    }
}

