#nullable enable
using System;
using System.Buffers.Binary;

namespace XMemCompressionDotNet;

internal static partial class XCompress
{
    private const uint LDI_CONTEXT_SIGNATURE = 0x4349444Cu;

    internal sealed class LdiContext
    {
        public uint signature = LDI_CONTEXT_SIGNATURE;
        public uint cbDataBlockMax;
        public uint configuration_value;
        public uint pad0;
        public t_decoder_context decoder_context = new();
    }

    internal static t_decoder_context GetDecoderContext(LzxDecompressionContext context)
    {
        return context.Decoder;
    }

    internal static ushort LEndianSwap8In16Local(ushort value)
    {
        return (ushort)((value << 8) | (value >> 8));
    }

    internal static void LzxReplaceDefaultParametersLocalDecompression(
        out XMEMCODEC_PARAMETERS_LZX dstParams,
        in XMEMCODEC_PARAMETERS_LZX srcParams)
    {
        dstParams = srcParams;
        if (dstParams.WindowSize == 0)
        {
            dstParams = new XMEMCODEC_PARAMETERS_LZX(
                dstParams.Flags,
                0x20000,
                dstParams.CompressionPartitionSize);
        }
    }

    internal static uint LDICreateDecompression(
        ref uint cbDataBlockMax,
        in XMEMCODEC_PARAMETERS_LZX configuration,
        out uint cbSrcBufferMin,
        out LdiContext? handle)
    {
        cbSrcBufferMin = cbDataBlockMax + 0x1800;
        handle = null;

        LdiContext context = new();
        context.cbDataBlockMax = cbDataBlockMax;
        context.configuration_value = configuration.CompressionPartitionSize;
        context.pad0 = 0;

        if (!LZX_DecodeInit(context.decoder_context, (int)configuration.WindowSize))
        {
            dec_free(context.decoder_context, null);
            return 1;
        }

        handle = context;
        return 0;
    }

    internal static uint LDIDecompress(
        LdiContext context,
        ReadOnlySpan<byte> source,
        Span<byte> destination,
        ref uint cbResult)
    {
        if (context.signature != LDI_CONTEXT_SIGNATURE)
        {
            return 2;
        }

        if (cbResult > context.cbDataBlockMax)
        {
            return 3;
        }

        int result = LZX_Decode(
            context.decoder_context,
            (int)cbResult,
            source,
            destination,
            out int total_bytes_written);

        cbResult = (uint)total_bytes_written;
        return result == 0 ? 0u : 4u;
    }

    internal static uint LDIResetDecompression(LdiContext context)
    {
        if (context.signature != LDI_CONTEXT_SIGNATURE)
        {
            return 2;
        }

        LZX_DecodeNewGroup(context.decoder_context);
        return 0;
    }

    internal static uint LDIDestroyDecompression(LdiContext context)
    {
        if (context.signature != LDI_CONTEXT_SIGNATURE)
        {
            return 2;
        }

        LZX_DecodeFree(context.decoder_context);
        context.signature = 0;
        dec_free(context.decoder_context, null);
        return 0;
    }

    internal static uint LDIGetWindow(
        LdiContext context,
        out byte[]? window,
        out int fileOffset,
        out int windowOffset,
        out int bytesAvail)
    {
        window = context.decoder_context.dec_mem_window;
        if ((uint)context.decoder_context.dec_position_at_start < context.decoder_context.dec_window_size)
        {
            fileOffset = 0;
            windowOffset = 0;
            bytesAvail = context.decoder_context.dec_position_at_start;
        }
        else
        {
            fileOffset = (int)((context.decoder_context.dec_window_size - 1) & (uint)context.decoder_context.dec_position_at_start);
            bytesAvail = (int)context.decoder_context.dec_window_size;
            windowOffset = context.decoder_context.dec_position_at_start - (int)context.decoder_context.dec_window_size;
        }

        return 0;
    }

    internal static uint LDISetWindowData(LdiContext context, ReadOnlySpan<byte> source)
    {
        if (context.signature != LDI_CONTEXT_SIGNATURE)
        {
            return 2;
        }

        return LZX_DecodeInsertDictionary(context.decoder_context, source) ? 0u : 4u;
    }

    internal static void DestroyDecompressionContextLzx(LzxDecompressionContext context)
    {
        _ = context;
    }

    internal static int GetDecompressionContextSizeLzx(in XMEMCODEC_PARAMETERS_LZX pLzxParams, uint flags)
    {
        LzxReplaceDefaultParametersLocalDecompression(out XMEMCODEC_PARAMETERS_LZX parameters, pLzxParams);
        init_decompression_memory_context(null, out ulong context_size, (int)parameters.WindowSize, 0x18, flags);
        return checked((int)context_size);
    }

    internal static int ResetDecompressionContextLzx(LzxDecompressionContext context)
    {
        t_decoder_context decoder = GetDecoderContext(context);
        LZX_DecodeNewGroup(decoder);

        if ((context.Flags & 1) != 0)
        {
            decoder.dec_source_staging_size = 0;
            decoder.dec_end_of_stream = false;
            decoder.dec_dest_staging_offset = 0;
            decoder.dec_dest_staging_size = 0;
        }
        else if ((context.Flags & 0x80000000u) != 0)
        {
            decoder.dec_td_last_source = default;
            decoder.dec_td_last_source_array = null;
            decoder.dec_td_last_source_owner = null;
            decoder.dec_td_last_source_identity_offset = 0;
            decoder.dec_td_last_source_size = 0;
            decoder.dec_td_last_segment_size = 0;
            decoder.dec_td_last_segment_offset = 0;
            decoder.dec_td_last_source_offset = 0;
            decoder.dec_td_last_decoded_size = 0;
            decoder.dec_td_last_stage_offset = 0;
            decoder.dec_td_last_stage_size = 0;
        }

        return 0;
    }

    internal static int DecompressLzx(
        LzxDecompressionContext context,
        Span<byte> destination,
        ref nuint destSize,
        ReadOnlySpan<byte> source)
    {
        t_decoder_context decoder = GetDecoderContext(context);
        uint remaining_source = 0;
        if (source.Length > 5)
        {
            remaining_source = (uint)(source.Length - 5);
        }

        if ((context.Flags & 1) == 0)
        {
            ResetDecompressionContextLzx(context);
        }

        int sourceOffset = 0;
        int destinationOffset = 0;
        uint total_bytes_written = 0;

        while (remaining_source != 0)
        {
            uint compressed_size = 0x8000;
            if (source[sourceOffset] == 0xFF)
            {
                compressed_size = (uint)((source[sourceOffset + 3] << 8) | source[sourceOffset + 4]);
                sourceOffset += 5;
                remaining_source -= 5;
            }
            else
            {
                compressed_size = (uint)((source[sourceOffset] << 8) | source[sourceOffset + 1]);
                sourceOffset += 2;
                remaining_source -= 2;
            }

            LZX_Decode(
                decoder,
                0x8000,
                source.Slice(sourceOffset, (int)compressed_size),
                destination[destinationOffset..],
                out int block_bytes_written);

            total_bytes_written += (uint)block_bytes_written;
            destinationOffset += block_bytes_written;
            sourceOffset += (int)compressed_size;
            remaining_source -= compressed_size;
        }

        destSize = total_bytes_written;
        return 0;
    }

    internal static int DecompressStreamLzx(
        LzxDecompressionContext context,
        Span<byte> destination,
        ref nuint destSize,
        ReadOnlySpan<byte> source,
        ref nuint srcSize)
    {
        t_decoder_context decoder = GetDecoderContext(context);
        uint dest_capacity = (uint)Math.Min((nuint)destination.Length, destSize);
        uint source_available = 0;
        if (!decoder.dec_end_of_stream)
        {
            source_available = (uint)Math.Min((nuint)source.Length, srcSize);
        }

        uint dest_written = 0;
        uint source_consumed = 0;

        if (decoder.dec_dest_staging_size != 0)
        {
            uint copy_size = decoder.dec_dest_staging_size;
            if (dest_capacity < copy_size)
            {
                copy_size = dest_capacity;
            }

            decoder.dec_dest_staging_buffer.AsSpan((int)decoder.dec_dest_staging_offset, (int)copy_size)
                .CopyTo(destination);

            decoder.dec_dest_staging_size -= copy_size;
            decoder.dec_dest_staging_offset += copy_size;
            dest_written = copy_size;
        }

        if ((source_available == 0) && (decoder.dec_source_staging_size <= 1))
        {
            destSize = dest_written;
            srcSize = source_consumed;
            return 0;
        }

        uint remaining_source = source_available;
        ReadOnlySpan<byte> block_source = source;
        bool block_source_is_staging = false;

        if (decoder.dec_source_staging_size != 0)
        {
            block_source = decoder.dec_source_staging_buffer;
            block_source_is_staging = true;

            bool final_chunk = block_source[0] == 0xFF;
            uint header_size = final_chunk ? 5u : 2u;
            uint trailer_size = final_chunk ? 5u : 0u;
            int size_ptr = final_chunk ? 3 : 0;
            uint total_staged_block_size = 0;

            if (decoder.dec_source_staging_size < header_size)
            {
                uint copy_size = header_size - decoder.dec_source_staging_size;
                if (copy_size > remaining_source)
                {
                    copy_size = remaining_source;
                }

                source.Slice(0, (int)copy_size)
                    .CopyTo(decoder.dec_source_staging_buffer.AsSpan((int)decoder.dec_source_staging_size, (int)copy_size));

                decoder.dec_source_staging_size += copy_size;
                source_consumed += copy_size;
                remaining_source -= copy_size;
            }

            if (decoder.dec_source_staging_size >= header_size)
            {
                total_staged_block_size =
                    (uint)((block_source[size_ptr] << 8) | block_source[size_ptr + 1]);
                total_staged_block_size += trailer_size + header_size;
            }

            if ((remaining_source != 0) && (total_staged_block_size != 0))
            {
                uint copy_size = total_staged_block_size - decoder.dec_source_staging_size;
                if (copy_size > remaining_source)
                {
                    copy_size = remaining_source;
                }

                source.Slice((int)source_consumed, (int)copy_size)
                    .CopyTo(decoder.dec_source_staging_buffer.AsSpan((int)decoder.dec_source_staging_size, (int)copy_size));

                source_consumed += copy_size;
                remaining_source -= copy_size;
                decoder.dec_source_staging_size += copy_size;
            }

            if (total_staged_block_size != 0)
            {
                if (decoder.dec_source_staging_size >= total_staged_block_size)
                {
                    remaining_source = decoder.dec_source_staging_size;
                }
                else
                {
                    remaining_source = 0;
                }
            }

            block_source = decoder.dec_source_staging_buffer;
        }

        while (decoder.dec_dest_staging_size == 0)
        {
            if (remaining_source == 0)
            {
                break;
            }

            uint first_byte = block_source[0];
            uint header_size = 0;
            uint trailer_size = 0;
            uint chunk_output_size = 0;
            uint total_block_size = 0;

            if (first_byte == 0xFF)
            {
                header_size = 5;
                trailer_size = 5;
                if (remaining_source >= 5)
                {
                    chunk_output_size = (uint)((block_source[1] << 8) | block_source[2]);
                    total_block_size = (uint)(((block_source[3] << 8) | block_source[4]) + 10);
                }
                else
                {
                    total_block_size = 5;
                }
            }
            else
            {
                header_size = 2;
                if (remaining_source >= 2)
                {
                    chunk_output_size = 0x8000;
                    total_block_size = (uint)(((block_source[0] << 8) | block_source[1]) + 2);
                }
                else
                {
                    total_block_size = 2;
                }
            }

            if (remaining_source < total_block_size)
            {
                block_source.Slice(0, (int)remaining_source).CopyTo(decoder.dec_source_staging_buffer);
                source_consumed += remaining_source;
                decoder.dec_source_staging_size = remaining_source;
                remaining_source = 0;
                break;
            }

            if ((remaining_source < (total_block_size + 5)) &&
                !block_source_is_staging &&
                (trailer_size == 0))
            {
                block_source.Slice(0, (int)total_block_size).CopyTo(decoder.dec_source_staging_buffer);
                source_consumed += total_block_size;
                block_source = decoder.dec_source_staging_buffer;
                block_source_is_staging = true;
                decoder.dec_source_staging_size = total_block_size;
            }

            uint remaining_dest = dest_capacity - dest_written;
            Span<byte> decode_destination = destination[(int)dest_written..];
            if (remaining_dest < chunk_output_size)
            {
                decode_destination = decoder.dec_dest_staging_buffer;
            }

            LZX_Decode(
                decoder,
                (int)chunk_output_size,
                block_source.Slice((int)header_size, (int)(total_block_size - trailer_size - header_size)),
                decode_destination,
                out int dest_block_size);

            if (decode_destination == decoder.dec_dest_staging_buffer)
            {
                uint copy_size = (uint)dest_block_size;
                if (remaining_dest < copy_size)
                {
                    copy_size = remaining_dest;
                }

                decoder.dec_dest_staging_buffer.AsSpan(0, (int)copy_size)
                    .CopyTo(destination[(int)dest_written..]);

                decoder.dec_dest_staging_offset = copy_size;
                decoder.dec_dest_staging_size = (uint)dest_block_size - copy_size;
                dest_written += copy_size;
            }
            else
            {
                dest_written += (uint)dest_block_size;
            }

            if (block_source_is_staging)
            {
                block_source = source[(int)source_consumed..];
                block_source_is_staging = false;
                decoder.dec_source_staging_size = 0;
            }
            else
            {
                source_consumed += total_block_size;
                block_source = block_source[(int)total_block_size..];
            }

            if (first_byte == 0xFF)
            {
                decoder.dec_end_of_stream = true;
                remaining_source = 0;
            }
            else
            {
                remaining_source = source_available - source_consumed;
            }
        }

        destSize = dest_written;
        srcSize = source_consumed;
        return 0;
    }

    internal static void DecompressSegmentTDLzx(
        LzxDecompressionContext context,
        Span<byte> destination,
        ref nuint destSize,
        ReadOnlySpan<byte> source,
        nuint segmentSize,
        nuint segmentOffset)
    {
        byte[] stableSource = source.ToArray();
        DecompressSegmentTDLzx(context, destination, ref destSize, stableSource, 0, stableSource.Length, segmentSize, segmentOffset);
    }

    internal static void DecompressSegmentTDLzx(
        LzxDecompressionContext context,
        Span<byte> destination,
        ref nuint destSize,
        ReadOnlySpan<byte> source,
        object sourceOwner,
        int sourceIdentityOffset,
        nuint segmentSize,
        nuint segmentOffset)
    {
        DecompressSegmentTDLzxCore(
            context,
            destination,
            ref destSize,
            source,
            sourceBuffer: null,
            sourceOwner,
            sourceIdentityOffset,
            segmentSize,
            segmentOffset);
    }

    internal static void DecompressSegmentTDLzx(
        LzxDecompressionContext context,
        Span<byte> destination,
        ref nuint destSize,
        byte[] sourceBuffer,
        int sourceOffset,
        int sourceLength,
        nuint segmentSize,
        nuint segmentOffset)
    {
        DecompressSegmentTDLzxCore(
            context,
            destination,
            ref destSize,
            sourceBuffer.AsSpan(sourceOffset, sourceLength),
            sourceBuffer,
            sourceOwner: null,
            sourceOffset,
            segmentSize,
            segmentOffset);
    }

    private static void DecompressSegmentTDLzxCore(
        LzxDecompressionContext context,
        Span<byte> destination,
        ref nuint destSize,
        ReadOnlySpan<byte> source,
        byte[]? sourceBuffer,
        object? sourceOwner,
        int sourceIdentityOffset,
        nuint segmentSize,
        nuint segmentOffset)
    {
        t_decoder_context decoder = GetDecoderContext(context);
        uint source_size = (uint)source.Length;
        uint segment_size = (uint)segmentSize;
        uint segment_offset = (uint)segmentOffset;
        uint destination_capacity = (uint)Math.Min((nuint)destination.Length, destSize);

        uint bytes_written = 0;
        uint decoded_offset = 0;
        uint source_offset = 0;
        uint stage_offset = 0;
        uint stage_size = 0;
        bool keep_stage_data = false;

        if (source.Length < 2 || segmentOffset > segmentSize)
        {
            throw new InvalidDataException("Invalid or truncated TD segment.");
        }
        ushort firstWord = BinaryPrimitives.ReadUInt16LittleEndian(source);
        if (firstWord == 0)
        {
            uint copy_size = destination_capacity;
            uint available_bytes = segment_size - segment_offset;
            if (copy_size > available_bytes)
            {
                copy_size = available_bytes;
            }

            if ((ulong)segment_offset + 2 + copy_size > (ulong)source.Length)
            {
                throw new InvalidDataException("Stored TD segment is truncated.");
            }
            source.Slice((int)segment_offset + 2, (int)copy_size).CopyTo(destination);
            decoder.dec_td_last_segment_offset = decoder.dec_td_last_segment_size;
            destSize = copy_size;
            return;
        }

        bool cache_hit =
            ReferenceEquals(decoder.dec_td_last_source_array, sourceBuffer) &&
            ReferenceEquals(decoder.dec_td_last_source_owner, sourceOwner) &&
            decoder.dec_td_last_source_identity_offset == sourceIdentityOffset &&
            decoder.dec_td_last_source_size == source_size &&
            decoder.dec_td_last_segment_size == segment_size &&
            segment_offset >= decoder.dec_td_last_segment_offset;

        int block_source = 0;
        uint remaining_source = source_size;
        if (cache_hit)
        {
            source_offset = decoder.dec_td_last_source_offset;
            block_source = (int)source_offset;
            remaining_source = source_size - source_offset;
            decoded_offset = decoder.dec_td_last_decoded_size;

            if (decoder.dec_td_last_stage_size != 0)
            {
                uint delta = segment_offset - decoder.dec_td_last_segment_offset;
                if (delta < decoder.dec_td_last_stage_size)
                {
                    uint copy_size = decoder.dec_td_last_stage_size - delta;
                    if (copy_size > destination_capacity)
                    {
                        copy_size = destination_capacity;
                    }

                    decoder.dec_td_staging_buffer.AsSpan((int)(decoder.dec_td_last_stage_offset + delta), (int)copy_size)
                        .CopyTo(destination);

                    bytes_written = copy_size;
                    stage_offset = decoder.dec_td_last_stage_offset + delta + copy_size;
                    stage_size = decoder.dec_td_last_stage_size - delta - copy_size;
                    keep_stage_data = true;
                }
            }
        }
        else
        {
            LZX_DecodeNewGroup(decoder);
        }

        while (remaining_source > 1 && bytes_written < destination_capacity)
        {
            uint compressed_size = LEndianSwap8In16Local(BinaryPrimitives.ReadUInt16LittleEndian(source.Slice(block_source, 2)));
            if (compressed_size == 0)
            {
                break;
            }

            block_source += 2;
            remaining_source -= 2;
            if (compressed_size > remaining_source)
            {
                throw new InvalidDataException("Compressed TD frame is truncated.");
            }

            uint chunk_output_size = 0x8000;
            uint remaining_segment_bytes = segment_size - decoded_offset;
            if (remaining_segment_bytes < chunk_output_size)
            {
                chunk_output_size = remaining_segment_bytes;
            }

            uint prefix_skip = 0;
            if (segment_offset > decoded_offset)
            {
                prefix_skip = segment_offset - decoded_offset;
                if (prefix_skip > chunk_output_size)
                {
                    prefix_skip = chunk_output_size;
                }
            }

            uint bytes_from_chunk = chunk_output_size - prefix_skip;
            uint destination_remaining = destination_capacity - bytes_written;
            if (bytes_from_chunk > destination_remaining)
            {
                bytes_from_chunk = destination_remaining;
            }

            bool stage_this_chunk = bytes_from_chunk < chunk_output_size;
            Span<byte> decode_destination = stage_this_chunk
                ? decoder.dec_td_staging_buffer
                : destination.Slice((int)bytes_written);

            int decodeResult = LZX_Decode(
                decoder,
                (int)chunk_output_size,
                source.Slice(block_source, (int)compressed_size),
                decode_destination,
                out int total_bytes_written);
            if (decodeResult != 0 || total_bytes_written != chunk_output_size)
            {
                throw new InvalidDataException("Invalid LZX data in TD frame.");
            }

            if (stage_this_chunk && bytes_from_chunk != 0)
            {
                decoder.dec_td_staging_buffer.AsSpan((int)prefix_skip, (int)bytes_from_chunk)
                    .CopyTo(destination.Slice((int)bytes_written));
            }

            bytes_written += bytes_from_chunk;
            block_source += (int)compressed_size;
            remaining_source -= compressed_size;
            source_offset = (uint)block_source;
            decoded_offset += chunk_output_size;

            keep_stage_data = stage_this_chunk;
            if (stage_this_chunk)
            {
                stage_offset = prefix_skip + bytes_from_chunk;
                stage_size = chunk_output_size - prefix_skip - bytes_from_chunk;
            }
            else
            {
                stage_offset = 0;
                stage_size = 0;
            }
        }

        decoder.dec_td_last_source = sourceBuffer is null
            ? default
            : new ReadOnlyMemory<byte>(sourceBuffer, sourceIdentityOffset, source.Length);
        decoder.dec_td_last_source_array = sourceBuffer;
        decoder.dec_td_last_source_owner = sourceOwner;
        decoder.dec_td_last_source_identity_offset = sourceIdentityOffset;
        decoder.dec_td_last_source_size = source_size;
        decoder.dec_td_last_source_offset = source_offset;
        decoder.dec_td_last_segment_size = segment_size;
        decoder.dec_td_last_segment_offset = segment_offset + bytes_written;
        decoder.dec_td_last_decoded_size = decoded_offset;

        if (keep_stage_data)
        {
            decoder.dec_td_last_stage_offset = stage_offset;
            decoder.dec_td_last_stage_size = stage_size;
        }
        else
        {
            decoder.dec_td_last_stage_offset = 0;
            decoder.dec_td_last_stage_size = 0;
        }

        destSize = bytes_written;
    }

    internal static void InitializeDecompressionContextLzx(
        in XMEMCODEC_PARAMETERS_LZX pLzxParams,
        uint flags,
        LzxDecompressionContext context)
    {
        LzxReplaceDefaultParametersLocalDecompression(out XMEMCODEC_PARAMETERS_LZX parameters, pLzxParams);
        init_decompression_memory_context(GetDecoderContext(context), out ulong requiredContextSize, (int)parameters.WindowSize, 0x18, flags);
        context.LzxFlags = parameters.Flags;
        context.WindowSize = parameters.WindowSize;

        if ((flags & 1) != 0)
        {
            ResetDecompressionContextLzx(context);
        }
    }
}
