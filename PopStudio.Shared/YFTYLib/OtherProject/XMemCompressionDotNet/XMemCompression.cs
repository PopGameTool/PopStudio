#nullable enable
using System;
using System.Buffers.Binary;
using System.IO.MemoryMappedFiles;
using System.Security.Cryptography;

namespace XMemCompressionDotNet;

public static partial class XMem
{
    private const uint XCOMPRESS_FILE_IDENTIFIER_LZXTDECODE = 0x0FF512EDu;
    private const uint XCOMPRESS_HEADER_SIZE = 16u;
    private const uint XMEM_TD_DEFAULT_WINDOW_SIZE = 0x20000u;
    private const uint XMEM_TD_DEFAULT_COMPRESSED_BLOCK_SIZE = 0x20000u;
    private const uint XMEM_TD_MAX_SEGMENTS = 0xFFFFu;
    private const uint XMEM_TD_MAX_PASSES = 8u;

    public static byte[] XMemDecompressLzxTdBuffer(ReadOnlySpan<byte> input)
    {
        TdHeader header = parse_td_header(input);
        uint[] segmentSizes = decode_segment_sizes(input, header, out nuint decompressedSize);
        return decompress_td_stream(input, header, segmentSizes, decompressedSize);
    }

    public static bool XMemDecompressLzxTdBuffer(
        ReadOnlySpan<byte> input,
        out byte[] outputData)
    {
        try
        {
            outputData = XMemDecompressLzxTdBuffer(input);
            return true;
        }
        catch
        {
            outputData = [];
            return false;
        }
    }

    public static byte[] XMemCompressLzxTdBufferEx(
        ReadOnlySpan<byte> input,
        uint windowSize,
        uint compressedBlockSize,
        float threshold)
    {
        validate_td_compression_args(input.Length, windowSize, compressedBlockSize, threshold);

        XMEMCODEC_PARAMETERS_LZX parameters = new(0, windowSize, 0);
        int hr = XMemCreateCompressionContext(
            XMEMCODEC_TYPE.XMEMCODEC_DEFAULT,
            parameters,
            XCompress.XMEM_TD_FLAG,
            out XMemCompressionContext? context);
        if (hr != XCompress.S_OK || context is null)
        {
            throw new InvalidOperationException($"XMemCreateCompressionContext failed: 0x{hr:X8}.");
        }

        try
        {
            hr = XMemResetCompressionContext(context);
            if (hr != XCompress.S_OK)
            {
                throw new InvalidOperationException($"XMemResetCompressionContext failed: 0x{hr:X8}.");
            }

            byte[]? pass = null;
            for (uint passIndex = 0; passIndex < XMEM_TD_MAX_PASSES; ++passIndex)
            {
                int passResult = compress_td_pass(context, input, compressedBlockSize, threshold, out byte[]? passData);
                if (passResult == 0)
                {
                    pass = passData;
                    break;
                }

                if (passResult != 2)
                {
                    throw new InvalidOperationException("TD compression failed before converging to a stable header size.");
                }
            }

            if (pass is null)
            {
                throw new InvalidOperationException("TD compression did not converge on a stable header size.");
            }

            return pass;
        }
        finally
        {
            XMemDestroyCompressionContext(context);
        }
    }

    public static bool XMemCompressLzxTdBufferEx(
        ReadOnlySpan<byte> input,
        out byte[] outputData,
        uint windowSize,
        uint compressedBlockSize,
        float threshold)
    {
        try
        {
            outputData = XMemCompressLzxTdBufferEx(input, windowSize, compressedBlockSize, threshold);
            return true;
        }
        catch
        {
            outputData = [];
            return false;
        }
    }

    public static byte[] XMemCompressLzxTdBuffer(ReadOnlySpan<byte> input)
    {
        return XMemCompressLzxTdBufferEx(
            input,
            XMEM_TD_DEFAULT_WINDOW_SIZE,
            XMEM_TD_DEFAULT_COMPRESSED_BLOCK_SIZE,
            1.0f);
    }

    public static bool XMemCompressLzxTdBuffer(ReadOnlySpan<byte> input, out byte[] outputData)
    {
        return XMemCompressLzxTdBufferEx(
            input,
            out outputData,
            XMEM_TD_DEFAULT_WINDOW_SIZE,
            XMEM_TD_DEFAULT_COMPRESSED_BLOCK_SIZE,
            1.0f);
    }

    public static long XMemDecompressLzxTdFile(
        string inputPath,
        string outputPath,
        bool overwrite = false)
    {
        FileMode mode = overwrite ? FileMode.Create : FileMode.CreateNew;
        using FileStream input = new(inputPath, FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 1024, FileOptions.SequentialScan);
        if (input.Length > int.MaxValue)
        {
            throw new InvalidDataException("Input is too large for this build.");
        }

        using FileStream output = new(outputPath, mode, FileAccess.Write, FileShare.None, 1024 * 1024);
        using MemoryMappedFile mappedInput = MemoryMappedFile.CreateFromFile(
            input,
            mapName: null,
            capacity: 0,
            MemoryMappedFileAccess.Read,
            HandleInheritability.None,
            leaveOpen: false);
        using MemoryMappedViewAccessor view = mappedInput.CreateViewAccessor(0, input.Length, MemoryMappedFileAccess.Read);

        unsafe
        {
            byte* inputPointer = null;
            view.SafeMemoryMappedViewHandle.AcquirePointer(ref inputPointer);
            try
            {
                ReadOnlySpan<byte> mappedInputSpan = new(inputPointer, (int)input.Length);
                TdHeader header = parse_td_header(mappedInputSpan);
                uint[] segmentSizes = decode_segment_sizes(mappedInputSpan, header, out _);
                return decompress_td_stream_to(mappedInputSpan, view, header, segmentSizes, output);
            }
            finally
            {
                if (inputPointer is not null)
                {
                    view.SafeMemoryMappedViewHandle.ReleasePointer();
                }
            }
        }
    }

    public static long XMemDecompressLzxTdBufferToStream(
        ReadOnlySpan<byte> input,
        Stream output)
    {
        byte[] stableInput = input.ToArray();
        TdHeader header = parse_td_header(stableInput);
        uint[] segmentSizes = decode_segment_sizes(stableInput, header, out _);
        return decompress_td_stream_to(stableInput, header, segmentSizes, output);
    }

    public static long XMemCompressLzxTdFile(
        string inputPath,
        string outputPath,
        bool overwrite = false)
    {
        return XMemCompressLzxTdFileEx(
            inputPath,
            outputPath,
            XMEM_TD_DEFAULT_WINDOW_SIZE,
            XMEM_TD_DEFAULT_COMPRESSED_BLOCK_SIZE,
            1.0f,
            overwrite);
    }

    public static long XMemStoreLzxTdFile(
        string inputPath,
        string outputPath,
        bool overwrite = false)
    {
        return XMemStoreLzxTdFileEx(
            inputPath,
            outputPath,
            XMEM_TD_DEFAULT_WINDOW_SIZE,
            XMEM_TD_DEFAULT_COMPRESSED_BLOCK_SIZE,
            overwrite);
    }

    public static long XMemStoreLzxTdFileEx(
        string inputPath,
        string outputPath,
        uint windowSize,
        uint compressedBlockSize,
        bool overwrite = false)
    {
        using FileStream input = new(inputPath, FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 1024, FileOptions.SequentialScan);
        if (input.Length > int.MaxValue)
        {
            throw new InvalidDataException("Input is too large for this build.");
        }

        validate_td_compression_args((int)input.Length, windowSize, compressedBlockSize, 1.0f);

        uint[] segmentSizes = build_stored_td_segment_sizes(
            (int)input.Length,
            compressedBlockSize,
            out nuint headerSize,
            out uint maxSegmentBits,
            out long outputSize);

        FileMode mode = overwrite ? FileMode.Create : FileMode.CreateNew;
        using FileStream output = new(outputPath, mode, FileAccess.Write, FileShare.None, 1024 * 1024, FileOptions.SequentialScan);
        write_stored_td_stream(input, output, segmentSizes, headerSize, maxSegmentBits, outputSize, windowSize, compressedBlockSize);
        return outputSize;
    }

    public static long XMemCompressLzxTdFileEx(
        string inputPath,
        string outputPath,
        uint windowSize,
        uint compressedBlockSize,
        float threshold,
        bool overwrite = false)
    {
        using FileStream input = new(inputPath, FileMode.Open, FileAccess.Read, FileShare.Read, 1024 * 1024, FileOptions.SequentialScan);
        if (input.Length > int.MaxValue)
        {
            throw new InvalidDataException("Input is too large for this build.");
        }

        validate_td_compression_args((int)input.Length, windowSize, compressedBlockSize, threshold);

        FileMode mode = overwrite ? FileMode.Create : FileMode.CreateNew;
        using FileStream output = new(outputPath, mode, FileAccess.ReadWrite, FileShare.None, 1024 * 1024);

        using MemoryMappedFile mappedInput = MemoryMappedFile.CreateFromFile(
            input,
            mapName: null,
            capacity: 0,
            MemoryMappedFileAccess.Read,
            HandleInheritability.None,
            leaveOpen: false);
        using MemoryMappedViewAccessor view = mappedInput.CreateViewAccessor(0, input.Length, MemoryMappedFileAccess.Read);

        unsafe
        {
            byte* inputPointer = null;
            view.SafeMemoryMappedViewHandle.AcquirePointer(ref inputPointer);
            try
            {
                ReadOnlySpan<byte> mappedInputSpan = new(inputPointer, (int)input.Length);
                return XMemCompressLzxTdBufferToStreamEx(mappedInputSpan, output, windowSize, compressedBlockSize, threshold);
            }
            finally
            {
                if (inputPointer is not null)
                {
                    view.SafeMemoryMappedViewHandle.ReleasePointer();
                }
            }
        }
    }

    public static long XMemCompressLzxTdBufferToStream(
        ReadOnlySpan<byte> input,
        Stream output)
    {
        return XMemCompressLzxTdBufferToStreamEx(
            input,
            output,
            XMEM_TD_DEFAULT_WINDOW_SIZE,
            XMEM_TD_DEFAULT_COMPRESSED_BLOCK_SIZE,
            1.0f);
    }

    public static long XMemCompressLzxTdBufferToStreamEx(
        ReadOnlySpan<byte> input,
        Stream output,
        uint windowSize,
        uint compressedBlockSize,
        float threshold)
    {
        validate_td_compression_args(input.Length, windowSize, compressedBlockSize, threshold);

        if (!output.CanSeek)
        {
            throw new ArgumentException("TD file compression requires a seekable output stream.", nameof(output));
        }

        XMEMCODEC_PARAMETERS_LZX parameters = new(0, windowSize, 0);
        int hr = XMemCreateCompressionContext(
            XMEMCODEC_TYPE.XMEMCODEC_DEFAULT,
            parameters,
            XCompress.XMEM_TD_FLAG,
            out XMemCompressionContext? context);
        if (hr != XCompress.S_OK || context is null)
        {
            throw new InvalidOperationException($"XMemCreateCompressionContext failed: 0x{hr:X8}.");
        }

        long outputStart = output.Position;
        if (output.Length < outputStart + compressedBlockSize)
        {
            output.SetLength(outputStart + compressedBlockSize);
        }

        try
        {
            hr = XMemResetCompressionContext(context);
            if (hr != XCompress.S_OK)
            {
                throw new InvalidOperationException($"XMemResetCompressionContext failed: 0x{hr:X8}.");
            }

            for (uint passIndex = 0; passIndex < XMEM_TD_MAX_PASSES; ++passIndex)
            {
                output.Position = outputStart;
                output.SetLength(outputStart + compressedBlockSize);

                int passResult = compress_td_pass_to_stream(
                    context,
                    input,
                    compressedBlockSize,
                    threshold,
                    output,
                    outputStart,
                    out long outputSize);
                if (passResult == 0)
                {
                    return outputSize;
                }

                if (passResult != 2)
                {
                    throw new InvalidOperationException("TD compression failed before converging to a stable header size.");
                }
            }

            throw new InvalidOperationException("TD compression did not converge on a stable header size.");
        }
        finally
        {
            XMemDestroyCompressionContext(context);
        }
    }

    private static uint read_u32_be(ReadOnlySpan<byte> data)
    {
        return BinaryPrimitives.ReadUInt32BigEndian(data);
    }

    private static uint read_u32_le(ReadOnlySpan<byte> data)
    {
        return BinaryPrimitives.ReadUInt32LittleEndian(data);
    }

    private static uint read_u32(ReadOnlySpan<byte> data, TdEndian endian)
    {
        return endian == TdEndian.Big ? read_u32_be(data) : read_u32_le(data);
    }

    private static uint bit_width_u32(uint value)
    {
        uint bits = 0;
        while (value != 0)
        {
            ++bits;
            value >>= 1;
        }

        return bits;
    }

    private static uint td_translation_entry_bits(uint maxSegmentBits)
    {
        return maxSegmentBits <= 20u ? 20u : 32u;
    }

    private static nuint td_header_size_for(uint segmentCount, uint maxSegmentBits)
    {
        uint entryBits = td_translation_entry_bits(maxSegmentBits);
        return (nuint)(XCOMPRESS_HEADER_SIZE + ((((ulong)entryBits * segmentCount) + 31u) >> 5) * sizeof(uint));
    }

    private static bool is_power_of_two_u32(uint value)
    {
        return value != 0u && (value & (value - 1u)) == 0u;
    }

    private static bool is_supported_td_pitch(uint compressedBlockSize)
    {
        return compressedBlockSize is 0x8000u or 0x10000u or 0x20000u or 0x40000u;
    }

    private static void validate_td_compression_args(
        int inputLength,
        uint windowSize,
        uint compressedBlockSize,
        float threshold)
    {
        if (inputLength == 0)
        {
            throw new ArgumentException("TD compression of an empty buffer is not supported by this wrapper.");
        }

        if (!is_power_of_two_u32(windowSize) || windowSize < 0x8000u)
        {
            throw new ArgumentOutOfRangeException(nameof(windowSize), $"Invalid LZX window size: 0x{windowSize:X8}.");
        }

        if (!is_supported_td_pitch(compressedBlockSize))
        {
            throw new ArgumentOutOfRangeException(nameof(compressedBlockSize), $"Invalid TD compressed block size: 0x{compressedBlockSize:X8}.");
        }

        if (threshold <= 0.0f)
        {
            throw new ArgumentOutOfRangeException(nameof(threshold), $"Invalid compression threshold: {threshold}.");
        }
    }

    private static TdHeader parse_td_header(ReadOnlySpan<byte> input)
    {
        ReadOnlySpan<uint> bitsPerSizeTable = [20u, 32u, 0u, 0u];

        if (input.Length < XCOMPRESS_HEADER_SIZE)
        {
            throw new InvalidDataException("Input is smaller than the XMem TD header.");
        }

        uint identifierBe = read_u32_be(input);
        uint identifierLe = read_u32_le(input);
        TdEndian endian;
        if (identifierBe == XCOMPRESS_FILE_IDENTIFIER_LZXTDECODE)
        {
            endian = TdEndian.Big;
        }
        else if (identifierLe == XCOMPRESS_FILE_IDENTIFIER_LZXTDECODE)
        {
            endian = TdEndian.Little;
        }
        else
        {
            throw new InvalidDataException($"Unsupported header: expected 0x{XCOMPRESS_FILE_IDENTIFIER_LZXTDECODE:X8}.");
        }

        uint flags = read_u32(input[12..16], endian);
        uint segmentCount = (flags >> 6) & 0xFFFFu;
        uint bitsPerSize = bitsPerSizeTable[(int)((flags >> 22) & 0x3u)];
        uint compressedBlockSize = 0x8000u << (int)((flags >> 4) & 0x3u);
        uint windowSize = 1u << (int)((flags & 0xFu) + 15u);

        if (segmentCount == 0)
        {
            throw new InvalidDataException("Invalid TD header: segment count is zero.");
        }

        if (bitsPerSize == 0)
        {
            throw new InvalidDataException("Unsupported TD header: unknown size encoding.");
        }

        uint bitsPerEntry = 20u + ((flags & 0x00C00000u) != 0 ? 12u : 0u);
        nuint payloadOffset = (nuint)(XCOMPRESS_HEADER_SIZE + ((((ulong)bitsPerEntry * segmentCount) + 31u) >> 5) * sizeof(uint));

        if (payloadOffset > (nuint)input.Length)
        {
            throw new InvalidDataException("Invalid TD header: size table exceeds input size.");
        }

        return new TdHeader(endian, flags, segmentCount, bitsPerSize, compressedBlockSize, windowSize, payloadOffset);
    }

    private static uint[] decode_segment_sizes(ReadOnlySpan<byte> input, TdHeader header, out nuint totalOutputSize)
    {
        uint[] sizes = new uint[header.SegmentCount];
        ulong total = 0;
        nuint cursor = XCOMPRESS_HEADER_SIZE;
        uint bits = 0;
        uint old = 0;
        uint num = 0;

        for (uint index = 0; index < header.SegmentCount; ++index)
        {
            uint size;

            if ((header.BitsPerSize & 31u) != 0)
            {
                bits = (bits + header.BitsPerSize) & 31u;
                if (bits > 0 && bits <= header.BitsPerSize)
                {
                    if (cursor + sizeof(uint) > (nuint)input.Length)
                    {
                        throw new InvalidDataException("Segment table is truncated.");
                    }

                    num = read_u32(input.Slice((int)cursor, sizeof(uint)), header.Endian);
                    cursor += sizeof(uint);
                }
                else
                {
                    num = old;
                    old = 0;
                }

                if (bits == 0)
                {
                    size = num;
                    old = 0;
                }
                else
                {
                    size = (num >> (int)(32u - bits)) | (old << (int)bits);
                    old = num & ((1u << (int)(32u - bits)) - 1u);
                }
            }
            else
            {
                if (cursor + sizeof(uint) > (nuint)input.Length)
                {
                    throw new InvalidDataException("Segment table is truncated.");
                }

                size = read_u32(input.Slice((int)cursor, sizeof(uint)), header.Endian);
                cursor += sizeof(uint);
            }

            sizes[index] = size;
            total += size;
        }

        if (cursor != header.PayloadOffset)
        {
            throw new InvalidDataException("Segment table size does not match header.");
        }

        if (total > int.MaxValue)
        {
            throw new InvalidDataException("Output is too large for this build.");
        }

        totalOutputSize = (nuint)total;
        return sizes;
    }

    private static byte[] decompress_td_stream(
        ReadOnlySpan<byte> input,
        TdHeader header,
        ReadOnlySpan<uint> segmentSizes,
        nuint outputSize)
    {
        XMEMCODEC_PARAMETERS_LZX parameters = new(0, header.WindowSize, 0);
        int hr = XMemCreateDecompressionContext(
            XMEMCODEC_TYPE.XMEMCODEC_DEFAULT,
            parameters,
            XCompress.XMEM_TD_FLAG,
            out XMemDecompressionContext? context);
        if (hr != XCompress.S_OK || context is null)
        {
            throw new InvalidOperationException($"XMemCreateDecompressionContext failed: 0x{hr:X8}.");
        }

        byte[] output = GC.AllocateUninitializedArray<byte>((int)outputSize);
        nuint written = 0;

        try
        {
            byte[] stableInput = input.ToArray();
            for (uint index = 0; index < header.SegmentCount; ++index)
            {
                nuint blockOutputSize = segmentSizes[(int)index];
                nuint segmentBaseOffset = (nuint)index * header.CompressedBlockSize;
                nuint segmentDataOffset = segmentBaseOffset;
                nuint offset = 0;

                if (segmentBaseOffset > (nuint)input.Length)
                {
                    throw new InvalidDataException($"Segment {index} base exceeds input size.");
                }

                if (index == 0)
                {
                    segmentDataOffset = header.PayloadOffset;
                }

                nuint segmentEndOffset = segmentBaseOffset + header.CompressedBlockSize;
                if (segmentEndOffset > (nuint)input.Length)
                {
                    segmentEndOffset = (nuint)input.Length;
                }

                if (segmentDataOffset > segmentEndOffset)
                {
                    throw new InvalidDataException($"Segment {index} payload exceeds segment bounds.");
                }

                int segmentArrayOffset = (int)segmentDataOffset;
                int segmentArrayLength = (int)(segmentEndOffset - segmentDataOffset);

                while (offset < blockOutputSize)
                {
                    nuint chunkSize = blockOutputSize - offset;
                    if (chunkSize > header.CompressedBlockSize)
                    {
                        chunkSize = header.CompressedBlockSize;
                    }

                    nuint destSize = chunkSize;
                    XCompress.DecompressSegmentTDLzx(
                        context.Lzx,
                        output.AsSpan((int)written),
                        ref destSize,
                        stableInput,
                        segmentArrayOffset,
                        segmentArrayLength,
                        blockOutputSize,
                        offset);
                    int chunkResult = XCompress.S_OK;

                    if (chunkResult != XCompress.S_OK)
                    {
                        throw new InvalidDataException($"XMemDecompressSegmentTD failed at segment {index} offset {offset}: 0x{chunkResult:X8}.");
                    }

                    if (destSize == 0)
                    {
                        throw new InvalidDataException($"XMemDecompressSegmentTD returned zero bytes at segment {index} offset {offset}.");
                    }

                    written += destSize;
                    offset += destSize;
                }
            }
        }
        finally
        {
            XMemDestroyDecompressionContext(context);
        }

        if (written != outputSize)
        {
            Array.Resize(ref output, (int)written);
        }

        return output;
    }

    private static long decompress_td_stream_to(
        byte[] input,
        TdHeader header,
        ReadOnlySpan<uint> segmentSizes,
        Stream output)
    {
        XMEMCODEC_PARAMETERS_LZX parameters = new(0, header.WindowSize, 0);
        int hr = XMemCreateDecompressionContext(
            XMEMCODEC_TYPE.XMEMCODEC_DEFAULT,
            parameters,
            XCompress.XMEM_TD_FLAG,
            out XMemDecompressionContext? context);
        if (hr != XCompress.S_OK || context is null)
        {
            throw new InvalidOperationException($"XMemCreateDecompressionContext failed: 0x{hr:X8}.");
        }

        byte[] chunk = GC.AllocateUninitializedArray<byte>((int)header.CompressedBlockSize);
        long written = 0;

        try
        {
            for (uint index = 0; index < header.SegmentCount; ++index)
            {
                nuint blockOutputSize = segmentSizes[(int)index];
                nuint segmentBaseOffset = (nuint)index * header.CompressedBlockSize;
                nuint segmentDataOffset = segmentBaseOffset;
                nuint offset = 0;

                if (segmentBaseOffset > (nuint)input.Length)
                {
                    throw new InvalidDataException($"Segment {index} base exceeds input size.");
                }

                if (index == 0)
                {
                    segmentDataOffset = header.PayloadOffset;
                }

                nuint segmentEndOffset = segmentBaseOffset + header.CompressedBlockSize;
                if (segmentEndOffset > (nuint)input.Length)
                {
                    segmentEndOffset = (nuint)input.Length;
                }

                if (segmentDataOffset > segmentEndOffset)
                {
                    throw new InvalidDataException($"Segment {index} payload exceeds segment bounds.");
                }

                int segmentArrayOffset = (int)segmentDataOffset;
                int segmentArrayLength = (int)(segmentEndOffset - segmentDataOffset);

                while (offset < blockOutputSize)
                {
                    nuint chunkSize = blockOutputSize - offset;
                    if (chunkSize > header.CompressedBlockSize)
                    {
                        chunkSize = header.CompressedBlockSize;
                    }

                    nuint destSize = chunkSize;
                    XCompress.DecompressSegmentTDLzx(
                        context.Lzx,
                        chunk.AsSpan(0, (int)chunkSize),
                        ref destSize,
                        input,
                        segmentArrayOffset,
                        segmentArrayLength,
                        blockOutputSize,
                        offset);

                    if (destSize == 0)
                    {
                        throw new InvalidDataException($"XMemDecompressSegmentTD returned zero bytes at segment {index} offset {offset}.");
                    }

                    output.Write(chunk.AsSpan(0, (int)destSize));
                    written += (long)destSize;
                    offset += destSize;
                }
            }
        }
        finally
        {
            XMemDestroyDecompressionContext(context);
        }

        return written;
    }

    private static long decompress_td_stream_to(
        ReadOnlySpan<byte> input,
        object inputOwner,
        TdHeader header,
        ReadOnlySpan<uint> segmentSizes,
        Stream output)
    {
        XMEMCODEC_PARAMETERS_LZX parameters = new(0, header.WindowSize, 0);
        int hr = XMemCreateDecompressionContext(
            XMEMCODEC_TYPE.XMEMCODEC_DEFAULT,
            parameters,
            XCompress.XMEM_TD_FLAG,
            out XMemDecompressionContext? context);
        if (hr != XCompress.S_OK || context is null)
        {
            throw new InvalidOperationException($"XMemCreateDecompressionContext failed: 0x{hr:X8}.");
        }

        byte[] chunk = GC.AllocateUninitializedArray<byte>((int)header.CompressedBlockSize);
        long written = 0;

        try
        {
            for (uint index = 0; index < header.SegmentCount; ++index)
            {
                nuint blockOutputSize = segmentSizes[(int)index];
                nuint segmentBaseOffset = (nuint)index * header.CompressedBlockSize;
                nuint segmentDataOffset = segmentBaseOffset;
                nuint offset = 0;

                if (segmentBaseOffset > (nuint)input.Length)
                {
                    throw new InvalidDataException($"Segment {index} base exceeds input size.");
                }

                if (index == 0)
                {
                    segmentDataOffset = header.PayloadOffset;
                }

                nuint segmentEndOffset = segmentBaseOffset + header.CompressedBlockSize;
                if (segmentEndOffset > (nuint)input.Length)
                {
                    segmentEndOffset = (nuint)input.Length;
                }

                if (segmentDataOffset > segmentEndOffset)
                {
                    throw new InvalidDataException($"Segment {index} payload exceeds segment bounds.");
                }

                int segmentSpanOffset = (int)segmentDataOffset;
                int segmentSpanLength = (int)(segmentEndOffset - segmentDataOffset);
                ReadOnlySpan<byte> segmentSource = input.Slice(segmentSpanOffset, segmentSpanLength);

                while (offset < blockOutputSize)
                {
                    nuint chunkSize = blockOutputSize - offset;
                    if (chunkSize > header.CompressedBlockSize)
                    {
                        chunkSize = header.CompressedBlockSize;
                    }

                    nuint destSize = chunkSize;
                    XCompress.DecompressSegmentTDLzx(
                        context.Lzx,
                        chunk.AsSpan(0, (int)chunkSize),
                        ref destSize,
                        segmentSource,
                        inputOwner,
                        segmentSpanOffset,
                        blockOutputSize,
                        offset);

                    if (destSize == 0)
                    {
                        throw new InvalidDataException($"XMemDecompressSegmentTD returned zero bytes at segment {index} offset {offset}.");
                    }

                    output.Write(chunk.AsSpan(0, (int)destSize));
                    written += (long)destSize;
                    offset += destSize;
                }
            }
        }
        finally
        {
            XMemDestroyDecompressionContext(context);
        }

        return written;
    }

    private static int compress_td_pass(
        XMemCompressionContext context,
        ReadOnlySpan<byte> input,
        uint compressedBlockSize,
        float threshold,
        out byte[]? output)
    {
        output = null;
        byte[] data = new byte[compressedBlockSize];
        int inputCursor = 0;
        uint segmentCount = 0;
        uint maxSegmentBits = 0;
        nuint size = 0;

        int hr = XMemBeginCompressionTD(context, compressedBlockSize);
        if (hr != XCompress.S_OK)
        {
            throw new InvalidOperationException($"XMemBeginCompressionTD failed: 0x{hr:X8}.");
        }

        while (inputCursor < input.Length)
        {
            if (segmentCount >= XMEM_TD_MAX_SEGMENTS)
            {
                throw new InvalidDataException($"Too many TD segments; maximum is {XMEM_TD_MAX_SEGMENTS}.");
            }

            nuint segmentOffset = (nuint)segmentCount * compressedBlockSize;
            EnsureCapacity(ref data, checked((int)(segmentOffset + compressedBlockSize)));

            nuint destSize = compressedBlockSize;
            nuint srcSize = (nuint)(input.Length - inputCursor);
            hr = XMemCompressSegmentTD(
                context,
                data.AsSpan((int)segmentOffset, (int)compressedBlockSize),
                ref destSize,
                input[inputCursor..],
                ref srcSize,
                threshold);
            if (hr != XCompress.S_OK)
            {
                throw new InvalidOperationException($"XMemCompressSegmentTD failed at segment {segmentCount}: 0x{hr:X8}.");
            }

            if (srcSize == 0)
            {
                throw new InvalidOperationException($"XMemCompressSegmentTD consumed zero bytes at segment {segmentCount}.");
            }

            if (destSize > compressedBlockSize)
            {
                throw new InvalidOperationException("XMemCompressSegmentTD returned an oversized segment.");
            }

            size = segmentOffset + destSize;
            uint segmentBits = bit_width_u32((uint)srcSize);
            if (segmentBits > maxSegmentBits)
            {
                maxSegmentBits = segmentBits;
            }

            inputCursor += (int)srcSize;
            ++segmentCount;
        }

        nuint headerSizeExpected = td_header_size_for(segmentCount, maxSegmentBits);
        if (headerSizeExpected > compressedBlockSize)
        {
            throw new InvalidDataException("TD header is larger than the compressed block size.");
        }

        if (size < headerSizeExpected)
        {
            EnsureCapacity(ref data, (int)headerSizeExpected);
            size = headerSizeExpected;
        }

        nuint headerSize = compressedBlockSize;
        hr = XMemEndCompressionTD(context, data, ref headerSize, threshold);
        if (hr != XCompress.S_OK)
        {
            if (hr == XCompress.XMCD_E_BADPARAM && headerSize == 0)
            {
                return 2;
            }

            throw new InvalidOperationException($"XMemEndCompressionTD failed: 0x{hr:X8}.");
        }

        Array.Resize(ref data, (int)size);
        output = data;
        return 0;
    }

    private static int compress_td_pass_to_stream(
        XMemCompressionContext context,
        ReadOnlySpan<byte> input,
        uint compressedBlockSize,
        float threshold,
        Stream output,
        long outputStart,
        out long outputSize)
    {
        outputSize = 0;
        byte[] segmentData = GC.AllocateUninitializedArray<byte>((int)compressedBlockSize);
        byte[] headerData = GC.AllocateUninitializedArray<byte>((int)compressedBlockSize);
        int inputCursor = 0;
        uint segmentCount = 0;
        uint maxSegmentBits = 0;
        nuint size = 0;

        int hr = XMemBeginCompressionTD(context, compressedBlockSize);
        if (hr != XCompress.S_OK)
        {
            throw new InvalidOperationException($"XMemBeginCompressionTD failed: 0x{hr:X8}.");
        }

        while (inputCursor < input.Length)
        {
            if (segmentCount >= XMEM_TD_MAX_SEGMENTS)
            {
                throw new InvalidDataException($"Too many TD segments; maximum is {XMEM_TD_MAX_SEGMENTS}.");
            }

            segmentData.AsSpan().Clear();

            nuint segmentOffset = (nuint)segmentCount * compressedBlockSize;
            nuint destSize = compressedBlockSize;
            nuint srcSize = (nuint)(input.Length - inputCursor);
            hr = XMemCompressSegmentTD(
                context,
                segmentData,
                ref destSize,
                input[inputCursor..],
                ref srcSize,
                threshold);
            if (hr != XCompress.S_OK)
            {
                throw new InvalidOperationException($"XMemCompressSegmentTD failed at segment {segmentCount}: 0x{hr:X8}.");
            }

            if (srcSize == 0)
            {
                throw new InvalidOperationException($"XMemCompressSegmentTD consumed zero bytes at segment {segmentCount}.");
            }

            if (destSize > compressedBlockSize)
            {
                throw new InvalidOperationException("XMemCompressSegmentTD returned an oversized segment.");
            }

            output.Position = outputStart + (long)segmentOffset;
            output.Write(segmentData.AsSpan(0, (int)destSize));
            size = segmentOffset + destSize;

            uint segmentBits = bit_width_u32((uint)srcSize);
            if (segmentBits > maxSegmentBits)
            {
                maxSegmentBits = segmentBits;
            }

            inputCursor += (int)srcSize;
            ++segmentCount;
        }

        nuint headerSizeExpected = td_header_size_for(segmentCount, maxSegmentBits);
        if (headerSizeExpected > compressedBlockSize)
        {
            throw new InvalidDataException("TD header is larger than the compressed block size.");
        }

        if (size < headerSizeExpected)
        {
            size = headerSizeExpected;
        }

        nuint headerSize = compressedBlockSize;
        headerData.AsSpan().Clear();
        hr = XMemEndCompressionTD(context, headerData, ref headerSize, threshold);
        if (hr != XCompress.S_OK)
        {
            if (hr == XCompress.XMCD_E_BADPARAM && headerSize == 0)
            {
                return 2;
            }

            throw new InvalidOperationException($"XMemEndCompressionTD failed: 0x{hr:X8}.");
        }

        output.Position = outputStart;
        output.Write(headerData.AsSpan(0, (int)headerSize));
        output.SetLength(outputStart + (long)size);
        output.Position = outputStart + (long)size;
        outputSize = (long)size;
        return 0;
    }

    private static uint[] build_stored_td_segment_sizes(
        int inputLength,
        uint compressedBlockSize,
        out nuint headerSize,
        out uint maxSegmentBits,
        out long outputSize)
    {
        uint payloadPerNonFirstSegment = compressedBlockSize - 2u;
        uint segmentCount = (uint)(((ulong)inputLength + payloadPerNonFirstSegment - 1u) / payloadPerNonFirstSegment);
        if (segmentCount == 0)
        {
            segmentCount = 1;
        }

        uint firstSegmentCapacity;
        while (true)
        {
            if (segmentCount > XMEM_TD_MAX_SEGMENTS)
            {
                throw new InvalidDataException($"Too many TD segments; maximum is {XMEM_TD_MAX_SEGMENTS}.");
            }

            nuint candidateHeaderSize = td_header_size_for(segmentCount, bit_width_u32(payloadPerNonFirstSegment));
            if (candidateHeaderSize + 2u > compressedBlockSize)
            {
                throw new InvalidDataException("TD header is larger than the compressed block size.");
            }

            firstSegmentCapacity = compressedBlockSize - (uint)candidateHeaderSize - 2u;
            if (firstSegmentCapacity == 0)
            {
                throw new InvalidDataException("TD header leaves no room for stored segment data.");
            }

            uint firstSegmentSize = (uint)Math.Min(inputLength, (int)firstSegmentCapacity);
            int remaining = inputLength - (int)firstSegmentSize;
            uint requiredSegmentCount = 1u;
            if (remaining > 0)
            {
                requiredSegmentCount += (uint)(((ulong)remaining + payloadPerNonFirstSegment - 1u) / payloadPerNonFirstSegment);
            }

            if (requiredSegmentCount == segmentCount)
            {
                headerSize = candidateHeaderSize;
                break;
            }

            segmentCount = requiredSegmentCount;
        }

        uint[] segmentSizes = new uint[segmentCount];
        int inputRemaining = inputLength;
        maxSegmentBits = 0;

        uint firstSize = (uint)Math.Min(inputRemaining, (int)firstSegmentCapacity);
        segmentSizes[0] = firstSize;
        inputRemaining -= (int)firstSize;
        maxSegmentBits = Math.Max(maxSegmentBits, bit_width_u32(firstSize));

        for (uint i = 1; i < segmentCount; ++i)
        {
            uint segmentSize = (uint)Math.Min(inputRemaining, (int)payloadPerNonFirstSegment);
            segmentSizes[(int)i] = segmentSize;
            inputRemaining -= (int)segmentSize;
            maxSegmentBits = Math.Max(maxSegmentBits, bit_width_u32(segmentSize));
        }

        headerSize = td_header_size_for(segmentCount, maxSegmentBits);
        outputSize = segmentCount == 1
            ? checked((long)headerSize + 2L + segmentSizes[0])
            : checked(((long)segmentCount - 1L) * compressedBlockSize + 2L + segmentSizes[^1]);

        return segmentSizes;
    }

    private static void write_stored_td_stream(
        Stream input,
        Stream output,
        ReadOnlySpan<uint> segmentSizes,
        nuint headerSize,
        uint maxSegmentBits,
        long outputSize,
        uint windowSize,
        uint compressedBlockSize)
    {
        int firstOutputSize = segmentSizes.Length == 1
            ? checked((int)headerSize + 2 + (int)segmentSizes[0])
            : checked((int)compressedBlockSize);

        byte[] firstBlock = GC.AllocateUninitializedArray<byte>(firstOutputSize);
        firstBlock.AsSpan().Clear();
        input.ReadExactly(firstBlock.AsSpan((int)headerSize + 2, (int)segmentSizes[0]));

        byte[] headerData = GC.AllocateUninitializedArray<byte>((int)headerSize);
        write_stored_td_header(headerData, segmentSizes, maxSegmentBits, windowSize, compressedBlockSize);

        uint accessTranslationDwords = (uint)((headerSize - XCOMPRESS_HEADER_SIZE) >> 2);
        uint digest = compute_stored_td_hash(
            firstBlock.AsSpan((int)headerSize, firstOutputSize - (int)headerSize),
            headerData,
            accessTranslationDwords,
            (ulong)input.Length,
            (ulong)outputSize);
        BinaryPrimitives.WriteUInt32LittleEndian(headerData.AsSpan(8, 4), digest);
        headerData.AsSpan().CopyTo(firstBlock);

        output.Write(firstBlock);

        if (segmentSizes.Length == 1)
        {
            return;
        }

        byte[] segmentBlock = GC.AllocateUninitializedArray<byte>((int)compressedBlockSize);
        for (int i = 1; i < segmentSizes.Length; ++i)
        {
            uint segmentSize = segmentSizes[i];
            int segmentOutputSize = i == segmentSizes.Length - 1
                ? checked(2 + (int)segmentSize)
                : checked((int)compressedBlockSize);

            Span<byte> outputSegment = segmentBlock.AsSpan(0, segmentOutputSize);
            outputSegment.Clear();
            input.ReadExactly(outputSegment.Slice(2, (int)segmentSize));
            output.Write(outputSegment);
        }
    }

    private static void write_stored_td_header(
        Span<byte> headerData,
        ReadOnlySpan<uint> segmentSizes,
        uint maxSegmentBits,
        uint windowSize,
        uint compressedBlockSize)
    {
        headerData.Clear();
        BinaryPrimitives.WriteUInt32LittleEndian(headerData[0..4], 0xED12F50Fu);
        BinaryPrimitives.WriteUInt16LittleEndian(headerData[4..6], 1);
        BinaryPrimitives.WriteUInt16LittleEndian(headerData[6..8], 0);

        uint translationEntryBits = td_translation_entry_bits(maxSegmentBits);
        uint translationMode = translationEntryBits != 20u ? 1u : 0u;
        uint windowCode = (bit_width_u32(windowSize) - bit_width_u32(0x8000u)) & 0xFu;
        uint segmentPitchCode = (bit_width_u32(compressedBlockSize) - 16u) << 4;
        uint packedHeader =
            windowCode |
            segmentPitchCode |
            ((uint)segmentSizes.Length << 6) |
            (translationMode << 22);

        BinaryPrimitives.WriteUInt32LittleEndian(headerData[12..16], XCompress.LEndianSwap8In32(packedHeader));

        if (translationMode == 0)
        {
            uint accessTranslationDwords = (uint)((((ulong)segmentSizes.Length * 20u) + 31u) >> 5);
            for (uint groupBase = 0; groupBase < segmentSizes.Length; groupBase += 8)
            {
                UInt8 valuesInline = default;
                Span<uint> values = valuesInline;
                for (uint i = 0; i < 8; ++i)
                {
                    uint index = groupBase + i;
                    if (index < segmentSizes.Length)
                    {
                        values[(int)i] = segmentSizes[(int)index];
                    }
                }

                UInt5 packedValuesInline = default;
                Span<uint> packedValues = packedValuesInline;
                packedValues[0] = (values[0] << 12) | (values[1] >> 8);
                packedValues[1] = (values[1] << 24) | (values[2] << 4) | (values[3] >> 16);
                packedValues[2] = (values[3] << 16) | (values[4] >> 4);
                packedValues[3] = (values[4] << 28) | (values[5] << 8) | (values[6] >> 12);
                packedValues[4] = (values[6] << 20) | values[7];

                uint outputBase = (groupBase >> 3) * 5;
                for (uint i = 0; i < 5; ++i)
                {
                    if (outputBase + i >= accessTranslationDwords)
                    {
                        break;
                    }

                    BinaryPrimitives.WriteUInt32LittleEndian(
                        headerData.Slice((int)(XCOMPRESS_HEADER_SIZE + ((outputBase + i) * 4)), 4),
                        XCompress.LEndianSwap8In32(packedValues[(int)i]));
                }
            }
        }
        else
        {
            for (int i = 0; i < segmentSizes.Length; ++i)
            {
                BinaryPrimitives.WriteUInt32LittleEndian(
                    headerData.Slice((int)(XCOMPRESS_HEADER_SIZE + (i * 4u)), 4),
                    XCompress.LEndianSwap8In32(segmentSizes[i]));
            }
        }
    }

    private static uint compute_stored_td_hash(
        ReadOnlySpan<byte> firstSegmentPayload,
        ReadOnlySpan<byte> headerData,
        uint accessTranslationDwords,
        ulong uncompressedSize,
        ulong compressedSize)
    {
        using IncrementalHash hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA1);
        hash.AppendData(firstSegmentPayload);
        hash.AppendData(headerData[0..8]);
        hash.AppendData(headerData[12..16]);
        if (accessTranslationDwords != 0)
        {
            hash.AppendData(headerData.Slice((int)XCOMPRESS_HEADER_SIZE, (int)(accessTranslationDwords << 2)));
        }

        Span<byte> sizeBytes = stackalloc byte[16];
        BinaryPrimitives.WriteUInt64BigEndian(sizeBytes[0..8], uncompressedSize);
        BinaryPrimitives.WriteUInt64BigEndian(sizeBytes[8..16], compressedSize);
        hash.AppendData(sizeBytes);

        Span<byte> digest = stackalloc byte[20];
        hash.GetHashAndReset(digest);
        return BinaryPrimitives.ReadUInt32LittleEndian(digest);
    }

    private static void EnsureCapacity(ref byte[] data, int required)
    {
        if (required <= data.Length)
        {
            return;
        }

        int newCapacity = data.Length == 0 ? required : data.Length;
        while (newCapacity < required)
        {
            newCapacity = newCapacity > int.MaxValue / 2 ? required : newCapacity * 2;
        }

        Array.Resize(ref data, newCapacity);
    }

    private enum TdEndian
    {
        Big = 0,
        Little = 1,
    }

    private readonly record struct TdHeader(
        TdEndian Endian,
        uint Flags,
        uint SegmentCount,
        uint BitsPerSize,
        uint CompressedBlockSize,
        uint WindowSize,
        nuint PayloadOffset);
}
