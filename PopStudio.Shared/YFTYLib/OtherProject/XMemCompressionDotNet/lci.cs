#nullable enable
using System;
using System.Buffers.Binary;
using System.Security.Cryptography;

namespace XMemCompressionDotNet;

internal static partial class XCompress
{
    private const uint LCI_CONTEXT_SIGNATURE = 0x4349434Cu;

    internal sealed class LciContext
    {
        public uint signature = LCI_CONTEXT_SIGNATURE;
        public uint pad0;
        public DecoderMallocDelegate? pfnAlloc;
        public DecoderFreeDelegate? pfnFree;
        public uint cbDataBlockMax;
        public uint file_translation_size;
        public t_encoder_context encoder_context = new();
    }

    internal sealed class StreamEncoderBlockInfo
    {
        public byte[] pDestination = [];
        public int DestinationOffset;
        public ulong DestSize;
        public ulong CompressedSize;
        public uint ChunksEncoded;
        public uint ChunksFinal;
    }

    internal sealed class EncoderBlockInfo
    {
        public byte[] pDestination = [];
        public int DestinationOffset;
        public ulong DestSize;
        public ulong CompressedSize;
    }

    internal static t_encoder_context GetEncoderContext(LzxCompressionContext context)
    {
        return context.Encoder;
    }

    internal static void LzxReplaceDefaultParametersLocal(
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

        if (dstParams.CompressionPartitionSize == 0)
        {
            dstParams = new XMEMCODEC_PARAMETERS_LZX(
                dstParams.Flags,
                dstParams.WindowSize,
                0x80000);
        }
    }

    internal static uint XCTDGetSegmentCount(uint headerDWord)
    {
        return (headerDWord >> 6) & 0xFFFFu;
    }

    internal static uint XCTDGetTranslationSize(uint headerDWord)
    {
        return (headerDWord & 0x00C00000u) != 0 ? 0x20u : 0x14u;
    }

    internal static uint XCTDGetAccessTranslationSizeInDWords(uint headerDWord)
    {
        return (XCTDGetTranslationSize(headerDWord) * XCTDGetSegmentCount(headerDWord) + 0x1Fu) >> 5;
    }

    internal static uint XCTDGetAccessTranslationSize(uint headerDWord)
    {
        return XCTDGetAccessTranslationSizeInDWords(headerDWord) << 2;
    }

    internal static uint XCTDGetHeaderSize(uint headerDWord)
    {
        return XCTDGetAccessTranslationSize(headerDWord) + 16;
    }

    internal static uint LocalCountLeadingZeros(int value)
    {
        uint count = 0;
        while (value >= 0 && count < 0x20)
        {
            ++count;
            value += value;
        }

        return count;
    }

    internal static uint Log2(uint value)
    {
        return 31u - LocalCountLeadingZeros((int)value);
    }

    internal static uint LEndianSwap8In32(uint value)
    {
        return ((value & 0x000000FFu) << 24) |
               ((value & 0x0000FF00u) << 8) |
               ((value & 0x00FF0000u) >> 8) |
               ((value & 0xFF000000u) >> 24);
    }

    internal static ulong LEndianSwap8In64(ulong value)
    {
        return ((value & 0x00000000000000FFul) << 56) |
               ((value & 0x000000000000FF00ul) << 40) |
               ((value & 0x0000000000FF0000ul) << 24) |
               ((value & 0x00000000FF000000ul) << 8) |
               ((value & 0x000000FF00000000ul) >> 8) |
               ((value & 0x0000FF0000000000ul) >> 24) |
               ((value & 0x00FF000000000000ul) >> 40) |
               ((value & 0xFF00000000000000ul) >> 56);
    }

    internal static uint LCIFlushCompressorOutput(LciContext context)
    {
        if (context.signature != LCI_CONTEXT_SIGNATURE)
        {
            return 2;
        }

        LZX_EncodeFlush(context.encoder_context);
        return 0;
    }

    internal static uint LCIResetCompression(LciContext context)
    {
        if (context.signature != LCI_CONTEXT_SIGNATURE)
        {
            return 2;
        }

        LZX_EncodeNewGroup(context.encoder_context);
        return 0;
    }

    internal static Span<byte> LCIGetInputData(
        LciContext context,
        out uint input_position,
        out uint bytes_available)
    {
        if (context.signature != LCI_CONTEXT_SIGNATURE)
        {
            input_position = 0;
            bytes_available = 0;
            return [];
        }

        return LZX_GetInputData(context.encoder_context, out input_position, out bytes_available);
    }

    internal static uint LCIResetState(LciContext context)
    {
        if (context.signature != LCI_CONTEXT_SIGNATURE)
        {
            return 2;
        }

        LZX_EncodeResetState(context.encoder_context);
        return 0;
    }

    internal static uint LCISetTranslationSize(LciContext context, uint size)
    {
        if (context.signature != LCI_CONTEXT_SIGNATURE)
        {
            return 2;
        }

        context.file_translation_size = size;
        return 0;
    }

    internal static uint LCISetWindowData(LciContext context, ReadOnlySpan<byte> source)
    {
        if (context.signature != LCI_CONTEXT_SIGNATURE)
        {
            return 2;
        }

        LZX_EncodeInsertDictionary(context.encoder_context, source);
        return 0;
    }

    internal static uint LCICreateCompression(
        ref uint cbDataBlockMax,
        in XMEMCODEC_PARAMETERS_LZX configuration,
        out uint cbDstBufferMin,
        out LciContext? handle)
    {
        cbDstBufferMin = 0;
        handle = null;
        LciContext context = new();

        if (!LZX_EncodeInit(
            context.encoder_context,
            (int)configuration.WindowSize,
            (int)configuration.CompressionPartitionSize))
        {
            return 1;
        }

        context.cbDataBlockMax = cbDataBlockMax;
        context.pad0 = 0;
        context.file_translation_size = 0;
        context.pfnAlloc = null;
        context.pfnFree = null;

        cbDstBufferMin = cbDataBlockMax + 0x1800;
        handle = context;
        return 0;
    }

    internal static uint LCICompress(
        LciContext context,
        ReadOnlySpan<byte> source,
        Span<byte> destination,
        out uint cbResult)
    {
        cbResult = 0;
        if (context.signature != LCI_CONTEXT_SIGNATURE)
        {
            return 2;
        }

        if (((uint)source.Length > context.cbDataBlockMax) ||
            ((uint)destination.Length < (context.cbDataBlockMax + 0x1800)))
        {
            return 2;
        }

        if (LZX_Encode(
            context.encoder_context,
            source,
            out int estimated_leftover_bytes,
            (int)context.file_translation_size) == 0)
        {
            cbResult = (uint)estimated_leftover_bytes;
            return 0;
        }

        return 4;
    }

    internal static uint LCIDestroyCompression(LciContext context)
    {
        if (context.signature != LCI_CONTEXT_SIGNATURE)
        {
            return 2;
        }

        LZX_EncodeFree(context.encoder_context);
        context.signature = 0;
        return 0;
    }

    internal static void DestroyCompressionContextLzx(LzxCompressionContext context)
    {
        context.Encoder.enc_td_hash?.Dispose();
        context.Encoder.enc_td_hash = null;
    }

    internal static int GetCompressionContextSizeLzx(in XMEMCODEC_PARAMETERS_LZX pLzxParams, uint flags)
    {
        LzxReplaceDefaultParametersLocal(out XMEMCODEC_PARAMETERS_LZX parameters, pLzxParams);
        comp_init_compress_memory_context(
            null,
            out ulong contextSize,
            (int)parameters.WindowSize,
            (int)parameters.CompressionPartitionSize,
            0x10,
            flags);
        return checked((int)contextSize);
    }

    internal static void InitializeCompressionContextLzx(
        in XMEMCODEC_PARAMETERS_LZX pLzxParams,
        uint flags,
        LzxCompressionContext context)
    {
        LzxReplaceDefaultParametersLocal(out XMEMCODEC_PARAMETERS_LZX parameters, pLzxParams);
        context.LzxFlags = parameters.Flags;

        t_encoder_context encoder = new();
        encoder.enc_tdat_uncompressed_size_list = new uint[0x10000];
        comp_init_compress_memory_context(
            encoder,
            out ulong contextSize,
            (int)parameters.WindowSize,
            (int)parameters.CompressionPartitionSize,
            0x10,
            flags);
        encoder.enc_context_data_size = checked((uint)(contextSize - 0x10));
        context.Encoder = encoder;

        if (!LZX_EncodeInit(
            encoder,
            (int)parameters.WindowSize,
            (int)parameters.CompressionPartitionSize))
        {
            throw new InvalidOperationException("Failed to initialize LZX encoder context.");
        }

        if ((flags & 1) != 0)
        {
            ResetCompressionContextLzx(context);
        }
    }

    internal static int ResetCompressionContextLzx(LzxCompressionContext context)
    {
        t_encoder_context encoder = GetEncoderContext(context);
        LZX_EncodeNewGroup(encoder);

        encoder.enc_td_hash?.Dispose();
        encoder.enc_td_hash = null;

        if ((context.Flags & 1) != 0)
        {
            encoder.enc_dest_staging_offset = 0;
            encoder.enc_dest_staging_size = 0;
        }
        else if ((context.Flags & 0x80000000u) != 0)
        {
            encoder.enc_td_segment_pitch = 0;
            encoder.enc_td_uncompressed_size = 0;
            encoder.enc_td_compressed_size = 0;
            encoder.enc_td_segment_count = 0;
            encoder.enc_td_segment_count_expected = 0;
            encoder.enc_td_translation_padding = 0;
            encoder.enc_td_translation_bits = 0;
            encoder.enc_td_translation_bits_expected = 0;
            encoder.enc_td_encode_data_uncompressed = false;
        }

        return 0;
    }

    internal static int BeginCompressionTDLzx(LzxCompressionContext context, nuint segmentPitch)
    {
        t_encoder_context encoder = GetEncoderContext(context);
        if (encoder.enc_td_segment_pitch == 0)
        {
            encoder.enc_td_segment_pitch = segmentPitch;
        }

        comp_clear_compress_memory_context(encoder);

        uint savedSegmentCount = encoder.enc_td_segment_count;
        uint savedTranslationBits = encoder.enc_td_translation_bits;
        encoder.enc_td_uncompressed_size = 0;
        encoder.enc_td_compressed_size = 0;
        encoder.enc_td_segment_count = 0;
        encoder.enc_td_segment_count_expected = savedSegmentCount;
        encoder.enc_td_translation_bits = 0;
        encoder.enc_td_translation_bits_expected = savedTranslationBits;

        return XMCDShaInit(encoder) != 0 ? 0 : 4;
    }

    internal static int CompressSegmentTDLzx(
        LzxCompressionContext context,
        Span<byte> destination,
        ref nuint destSize,
        ReadOnlySpan<byte> source,
        ref nuint srcSize,
        float threshold)
    {
        t_encoder_context encoder = GetEncoderContext(context);
        ReadOnlySpan<byte> source_begin = source;
        uint requested_source_size = (uint)Math.Min((nuint)source.Length, srcSize);
        uint segment_index = encoder.enc_td_segment_count;

        uint header_size = 0;
        uint translation_padding = 0;
        if (segment_index == 0)
        {
            header_size = 0x10;
            if (!encoder.enc_td_encode_data_uncompressed)
            {
                uint expected_translation_bits = 0x20;
                if (encoder.enc_td_translation_bits_expected <= 0x14)
                {
                    expected_translation_bits = 0x14;
                }

                header_size = 0x10 + ((((expected_translation_bits * encoder.enc_td_segment_count_expected) + 0x1Fu) >> 5) << 2);
            }

            translation_padding = encoder.enc_td_translation_padding;
        }

        uint segment_pitch = (uint)encoder.enc_td_segment_pitch;
        if (header_size >= (segment_pitch - 8))
        {
            XMCDPRINT("The file header is too large to fit within the first compressed data segment.\n");
            return 3;
        }

        uint payload_budget = segment_pitch - translation_padding - header_size;
        int result = 0;
        uint source_consumed = 0;
        ulong output_size = 0;
        ulong available_destination = destSize;

        if (encoder.enc_td_encode_data_uncompressed)
        {
            uint copied_source_size = requested_source_size;
            uint max_copy = segment_pitch - header_size;
            if (copied_source_size > max_copy)
            {
                copied_source_size = max_copy;
            }

            if (available_destination >= copied_source_size + header_size)
            {
                source_begin[..(int)copied_source_size].CopyTo(destination[(int)header_size..]);
            }

            output_size = copied_source_size + header_size;
            source_consumed = copied_source_size;

            destSize = (nuint)output_size;
            srcSize = copied_source_size;
            if (available_destination < output_size)
            {
                result = 6;
            }

            encoder.enc_td_uncompressed_size += copied_source_size;
            encoder.enc_td_compressed_size += output_size;
            encoder.enc_tdat_uncompressed_size_list[segment_index] = copied_source_size;
        }
        else
        {
            EnsureArrayCapacity(ref encoder.enc_td_work_buffer, checked((int)Math.Max(available_destination, 1ul)));
            EncoderBlockInfo block_info = new()
            {
                pDestination = encoder.enc_td_work_buffer,
            };

            uint consumed_prefix = translation_padding + header_size;
            if (available_destination > consumed_prefix)
            {
                block_info.DestSize = available_destination - consumed_prefix;
            }

            encoder.enc_output_callback_function = LzxEncoderTDCallback;
            encoder.enc_fci_data = block_info;

            LZX_EncodeNewGroup(encoder);

            ReadOnlySpan<byte> source_ptr = source_begin;
            uint source_remaining = requested_source_size;
            uint lower_bound = 1;
            uint compressed_payload_size = 0;
            uint original_chunk_limit = 0x8000;
            if (source_remaining < original_chunk_limit)
            {
                original_chunk_limit = source_remaining;
            }

            uint current_candidate = original_chunk_limit;
            uint upper_bound = original_chunk_limit;
            ulong accepted_compressed_size = block_info.CompressedSize;
            int snapshot_destination_offset = block_info.DestinationOffset;
            ulong snapshot_dest_size = block_info.DestSize;
            ulong snapshot_compressed_size = block_info.CompressedSize;

            if (payload_budget != 0)
            {
                while ((current_candidate != 0) && (upper_bound >= lower_bound) && (source_remaining != 0))
                {
                    uint available_payload = payload_budget - compressed_payload_size;
                    if (available_payload < (current_candidate + 0x1800))
                    {
                        snapshot_destination_offset = block_info.DestinationOffset;
                        snapshot_dest_size = block_info.DestSize;
                        snapshot_compressed_size = block_info.CompressedSize;
                        SnapshotEncoderContext(encoder);
                    }

                    if (LZX_Encode(
                        encoder,
                        source_ptr[..(int)current_candidate],
                        out int estimated_leftover_bytes,
                        0) != 0)
                    {
                        _ = estimated_leftover_bytes;
                        result = 4;
                        break;
                    }

                    LZX_EncodeFlush(encoder);

                    uint compressed_delta = (uint)(block_info.CompressedSize - accepted_compressed_size);
                    if ((compressed_payload_size + compressed_delta) > payload_budget)
                    {
                        upper_bound = current_candidate - 1;
                        if (upper_bound < lower_bound)
                        {
                            lower_bound = upper_bound;
                            current_candidate = upper_bound;
                        }
                        else
                        {
                            uint projected_candidate = compressed_delta == 0
                                ? upper_bound
                                : (uint)(((ulong)available_payload * current_candidate) / compressed_delta);
                            uint relaxed_candidate = lower_bound + ((upper_bound - lower_bound) / 10);
                            uint next_candidate = projected_candidate > relaxed_candidate ? projected_candidate : relaxed_candidate;
                            if (next_candidate >= upper_bound)
                            {
                                next_candidate = upper_bound;
                            }

                            current_candidate = next_candidate;
                        }

                        block_info.DestinationOffset = snapshot_destination_offset;
                        block_info.DestSize = snapshot_dest_size;
                        block_info.CompressedSize = snapshot_compressed_size;
                        RestoreEncoderContext(encoder);
                        continue;
                    }

                    bool finalize_segment = false;
                    if ((compressed_delta < available_payload) && (current_candidate != original_chunk_limit))
                    {
                        lower_bound = current_candidate + 1;
                        if (lower_bound <= upper_bound)
                        {
                            uint projected_candidate = compressed_delta == 0
                                ? upper_bound
                                : (uint)(((ulong)available_payload * current_candidate) / compressed_delta);
                            uint relaxed_candidate = upper_bound - ((upper_bound - lower_bound) / 10);
                            uint next_candidate = projected_candidate < relaxed_candidate ? projected_candidate : relaxed_candidate;
                            if (next_candidate <= lower_bound)
                            {
                                next_candidate = lower_bound;
                            }

                            current_candidate = next_candidate;
                            block_info.DestinationOffset = snapshot_destination_offset;
                            block_info.DestSize = snapshot_dest_size;
                            block_info.CompressedSize = snapshot_compressed_size;
                            RestoreEncoderContext(encoder);
                            continue;
                        }

                        finalize_segment = true;
                    }

                    source_ptr = source_ptr[(int)current_candidate..];
                    source_remaining -= current_candidate;
                    compressed_payload_size += compressed_delta;

                    if (finalize_segment)
                    {
                        break;
                    }

                    accepted_compressed_size = block_info.CompressedSize;

                    current_candidate = 0x8000;
                    if (source_remaining < current_candidate)
                    {
                        current_candidate = source_remaining;
                    }

                    original_chunk_limit = current_candidate;
                    upper_bound = current_candidate;
                    if (compressed_payload_size >= payload_budget)
                    {
                        break;
                    }
                }
            }

            uint segment_body_size = (uint)block_info.CompressedSize;
            source_consumed = (uint)(requested_source_size - source_remaining);

            if (result == 0)
            {
                uint uncompressed_tail = payload_budget - compressed_payload_size;
                if (source_remaining < uncompressed_tail)
                {
                    uncompressed_tail = source_remaining;
                }

                float threshold_output = block_info.CompressedSize + uncompressed_tail;
                float threshold_input = (source_consumed + 2) * threshold;
                if (!(threshold_output < threshold_input))
                {
                    uint raw_source_size = requested_source_size;
                    uint max_raw_source_size = payload_budget - 2;
                    if (raw_source_size > max_raw_source_size)
                    {
                        raw_source_size = max_raw_source_size;
                    }

                    uint raw_segment_size = raw_source_size + 2;
                    if (block_info.DestSize >= 2)
                    {
                        Span<byte> payload = destination[(int)header_size..];
                        BinaryPrimitives.WriteUInt16LittleEndian(payload, 0);

                        uint raw_copy_size = raw_source_size;
                        ulong max_copy_size = block_info.DestSize - 2;
                        if (raw_copy_size > max_copy_size)
                        {
                            raw_copy_size = (uint)max_copy_size;
                        }

                        source_begin[..(int)raw_copy_size].CopyTo(payload[2..]);
                    }

                    source_consumed = raw_source_size;
                    source_remaining = requested_source_size - raw_source_size;
                    segment_body_size = raw_segment_size;
                }
                else
                {
                    block_info.pDestination.AsSpan(0, block_info.DestinationOffset)
                        .CopyTo(destination[(int)header_size..]);

                    uint zero_fill = uncompressed_tail;
                    if (zero_fill > 2)
                    {
                        zero_fill = 2;
                    }

                    if (zero_fill != 0)
                    {
                        destination.Slice((int)(header_size + segment_body_size), (int)zero_fill).Clear();
                        segment_body_size += zero_fill;
                    }
                }

                uint alignment = source_remaining != 0 ? segment_pitch : 1;
                ulong total_size_before_align = header_size + translation_padding + segment_body_size;
                output_size = (total_size_before_align + alignment - 1) & ~(ulong)(alignment - 1);

                destination.Slice((int)total_size_before_align, (int)(output_size - total_size_before_align)).Clear();
                destSize = (nuint)output_size;

                if (output_size > (ulong)(header_size + translation_padding) + block_info.DestSize)
                {
                    result = 6;
                }
            }
            else
            {
                destSize = 0;
            }

            srcSize = source_consumed;
            encoder.enc_td_uncompressed_size += source_consumed;
            encoder.enc_td_compressed_size += destSize;
            encoder.enc_tdat_uncompressed_size_list[segment_index] = source_consumed;
            encoder.enc_output_callback_function = null;
            encoder.enc_fci_data = null;
        }

        uint translation_bits = 0;
        if (source_consumed != 0)
        {
            translation_bits = 32u - LocalCountLeadingZeros((int)source_consumed);
        }

        if (translation_bits > encoder.enc_td_translation_bits)
        {
            encoder.enc_td_translation_bits = translation_bits;
        }

            if ((segment_index == 0) && (result == 0) && (encoder.enc_td_hash is not null))
            {
                if (encoder.enc_td_encode_data_uncompressed)
                {
                    if (XMCDShaUpdate(encoder, source_begin[..(int)source_consumed]) == 0)
                    {
                        result = 4;
                    }
                }
                else
                {
                    if (XMCDShaUpdate(encoder, destination.Slice((int)header_size, (int)(output_size - header_size))) == 0)
                    {
                        result = 4;
                    }
                }
            }

        ++encoder.enc_td_segment_count;
        return result;
    }

    internal static int EndCompressionTDLzx(
        LzxCompressionContext context,
        Span<byte> headerData,
        ref nuint headerSize,
        float threshold)
    {
        t_encoder_context encoder = GetEncoderContext(context);

        uint translation_bits = encoder.enc_td_translation_bits;
        uint translation_entry_bits = 0x20;
        if (translation_bits <= 0x14)
        {
            translation_entry_bits = 0x14;
        }

        uint expected_translation_entry_bits = 0x20;
        if (encoder.enc_td_translation_bits_expected <= 0x14)
        {
            expected_translation_entry_bits = 0x14;
        }

        uint segment_count = 0;
        uint access_translation_dwords = 0;
        uint translation_mode = 0;
        uint required_header_size = 0x10;

        if (!encoder.enc_td_encode_data_uncompressed && encoder.enc_td_uncompressed_size != 0)
        {
            uint expected_segment_count = encoder.enc_td_segment_count_expected;
            segment_count = encoder.enc_td_segment_count;

            if ((expected_segment_count < segment_count) ||
                (translation_entry_bits > expected_translation_entry_bits))
            {
                headerSize = 0;
                return 7;
            }

            if ((expected_segment_count >= segment_count) &&
                (translation_entry_bits <= expected_translation_entry_bits))
            {
                float segment_threshold = segment_count;
                float payload_threshold = (encoder.enc_td_uncompressed_size + 0x10) * threshold;
                if (!(segment_threshold < payload_threshold))
                {
                    encoder.enc_td_encode_data_uncompressed = true;
                    headerSize = 0;
                    return 7;
                }
            }

            if ((expected_segment_count > segment_count) ||
                (translation_entry_bits < expected_translation_entry_bits))
            {
                encoder.enc_td_translation_padding =
                    ((((expected_segment_count * expected_translation_entry_bits) + 0x1Fu) >> 5) -
                     (((segment_count * translation_entry_bits) + 0x1Fu) >> 5))
                    << 2;
                if (encoder.enc_td_translation_padding != 0)
                {
                    headerSize = 0;
                    return 7;
                }

                encoder.enc_td_segment_count_expected = segment_count;
                encoder.enc_td_translation_bits_expected = translation_bits;
            }

            access_translation_dwords = ((segment_count * translation_entry_bits) + 0x1Fu) >> 5;
            translation_mode = translation_entry_bits != 0x14 ? 1u : 0u;
            required_header_size = 0x10 + (access_translation_dwords << 2);
        }

        if (required_header_size >= ((uint)encoder.enc_td_segment_pitch - 8))
        {
            XMCDPRINT(
                "The file header is too large to fit within the first compressed data segment.\n" +
                "The compressed segment size must be increased or the uncompressed size of the file reduced.\n");
            return 5;
        }

        nuint available_header_size = headerSize;
        headerSize = required_header_size;
        if (available_header_size < required_header_size)
        {
            return 6;
        }

        headerData.Slice(0, (int)required_header_size).Clear();
        BinaryPrimitives.WriteUInt32LittleEndian(headerData[0..4], 0xED12F50Fu);
        BinaryPrimitives.WriteUInt16LittleEndian(headerData[4..6], 1);
        BinaryPrimitives.WriteUInt16LittleEndian(headerData[6..8], 0);

        uint window_code = (Log2(encoder.enc_window_size) - Log2(0x8000u)) & 0xFu;
        uint segment_pitch_code = (Log2((uint)encoder.enc_td_segment_pitch) - 15u) << 4;
        uint preserved_header_bits = BinaryPrimitives.ReadUInt32LittleEndian(headerData[12..16]) & 0xFF000000u;
        uint packed_header =
            preserved_header_bits |
            window_code |
            segment_pitch_code |
            (segment_count << 6) |
            (translation_mode << 22);
        BinaryPrimitives.WriteUInt32LittleEndian(headerData[12..16], LEndianSwap8In32(packed_header));

        if (segment_count != 0)
        {
            if (translation_mode == 0)
            {
                for (uint group_base = 0; group_base < segment_count; group_base += 8)
                {
                    UInt8 valuesInline = default;
                    Span<uint> values = valuesInline;
                    for (uint i = 0; i < 8; ++i)
                    {
                        uint index = group_base + i;
                        if (index < segment_count)
                        {
                            values[(int)i] = encoder.enc_tdat_uncompressed_size_list[index];
                        }
                    }

                    UInt5 packedValuesInline = default;
                    Span<uint> packed_values = packedValuesInline;
                    packed_values[0] = (values[0] << 12) | (values[1] >> 8);
                    packed_values[1] = (values[1] << 24) | (values[2] << 4) | (values[3] >> 16);
                    packed_values[2] = (values[3] << 16) | (values[4] >> 4);
                    packed_values[3] = (values[4] << 28) | (values[5] << 8) | (values[6] >> 12);
                    packed_values[4] = (values[6] << 20) | values[7];

                    uint output_base = (group_base >> 3) * 5;
                    for (uint i = 0; i < 5; ++i)
                    {
                        if ((output_base + i) >= access_translation_dwords)
                        {
                            break;
                        }

                        BinaryPrimitives.WriteUInt32LittleEndian(
                            headerData.Slice((int)(0x10 + ((output_base + i) * 4)), 4),
                            LEndianSwap8In32(packed_values[(int)i]));
                    }
                }
            }
            else
            {
                for (uint i = 0; i < segment_count; ++i)
                {
                    BinaryPrimitives.WriteUInt32LittleEndian(
                        headerData.Slice((int)(0x10 + (i * 4)), 4),
                        LEndianSwap8In32(encoder.enc_tdat_uncompressed_size_list[i]));
                }
            }
        }

        ulong uncompressed_size = LEndianSwap8In64(encoder.enc_td_uncompressed_size);
        ulong compressed_size = LEndianSwap8In64(encoder.enc_td_compressed_size);

        int hash_failed = 0;
        if (encoder.enc_td_hash is null)
        {
            hash_failed = 1;
        }
        else
        {
            hash_failed |= XMCDShaUpdate(encoder, headerData[0..8]) == 0 ? 1 : 0;
            hash_failed |= XMCDShaUpdate(encoder, headerData[12..16]) == 0 ? 1 : 0;
            if (access_translation_dwords != 0)
            {
                hash_failed |= XMCDShaUpdate(encoder, headerData.Slice(16, (int)(access_translation_dwords << 2))) == 0 ? 1 : 0;
            }

            Span<byte> sizeBytes = stackalloc byte[16];
            BinaryPrimitives.WriteUInt64LittleEndian(sizeBytes[0..8], uncompressed_size);
            BinaryPrimitives.WriteUInt64LittleEndian(sizeBytes[8..16], compressed_size);
            hash_failed |= XMCDShaUpdate(encoder, sizeBytes) == 0 ? 1 : 0;
        }

        if ((XMCDShaFinal(encoder, out uint digest) != 0) && (hash_failed == 0))
        {
            BinaryPrimitives.WriteUInt32LittleEndian(headerData[8..12], digest);
            return 0;
        }

        return 4;
    }

    internal static int CompressLzx(
        LzxCompressionContext context,
        Span<byte> destination,
        ref nuint destSize,
        ReadOnlySpan<byte> source)
    {
        t_encoder_context encoder = GetEncoderContext(context);
        if ((context.Flags & 1) == 0)
        {
            ResetCompressionContextLzx(context);
        }

        StreamEncoderBlockInfo block_info = new()
        {
            pDestination = destination.ToArray(),
            DestSize = destSize,
        };

        encoder.enc_output_callback_function = LzxEncoderCallback;
        encoder.enc_fci_data = block_info;

        ReadOnlySpan<byte> remaining = source;
        uint chunk_index = 0;
        uint final_chunk_index = 0;

        while (!remaining.IsEmpty)
        {
            int chunk_size = 0x8000;
            if (remaining.Length < chunk_size)
            {
                chunk_size = remaining.Length;
            }

            ++chunk_index;
            if (remaining.Length <= 0x8000)
            {
                final_chunk_index = chunk_index;
            }

            block_info.ChunksFinal = final_chunk_index;

            _ = LZX_Encode(
                encoder,
                remaining[..chunk_size],
                out int estimated_leftover_bytes,
                0);
            _ = estimated_leftover_bytes;

            remaining = remaining[chunk_size..];
        }

        LZX_EncodeFlush(encoder);
        block_info.pDestination.AsSpan(0, block_info.DestinationOffset).CopyTo(destination);
        destSize = (nuint)block_info.CompressedSize;
        encoder.enc_output_callback_function = null;
        encoder.enc_fci_data = null;
        return block_info.CompressedSize > block_info.DestSize ? 6 : 0;
    }

    internal static int CompressStreamLzx(
        LzxCompressionContext context,
        Span<byte> destination,
        ref nuint destSize,
        ReadOnlySpan<byte> source,
        ref nuint srcSize)
    {
        t_encoder_context encoder = GetEncoderContext(context);
        encoder.enc_output_callback_function = LzxEncoderCallback;

        StreamEncoderBlockInfo block_info = new();
        encoder.enc_fci_data = block_info;

        uint source_available = (uint)Math.Min((nuint)source.Length, srcSize);
        ulong dest_capacity = (ulong)Math.Min((nuint)destination.Length, destSize);
        ulong dest_written = 0;
        uint source_consumed = 0;
        ReadOnlySpan<byte> source_bytes = source[..(int)source_available];

        if (encoder.enc_dest_staging_size != 0)
        {
            uint copy_size = encoder.enc_dest_staging_size;
            if (dest_capacity < copy_size)
            {
                copy_size = (uint)dest_capacity;
            }

            encoder.enc_dest_staging_buffer
                .AsSpan((int)encoder.enc_dest_staging_offset, (int)copy_size)
                .CopyTo(destination);

            dest_written = copy_size;
            encoder.enc_dest_staging_size -= copy_size;
            encoder.enc_dest_staging_offset += copy_size;
        }

        uint target_chunk_size;
        if (source_available != 0)
        {
            target_chunk_size = 0x8000;
            encoder.enc_stream_flushed = false;
        }
        else
        {
            target_chunk_size = encoder.enc_source_staging_size;
        }

        ReadOnlySpan<byte> encode_source = source_bytes;
        bool encode_source_is_staging = false;
        uint available_to_encode;
        if (encoder.enc_source_staging_size != 0)
        {
            uint staged_bytes = encoder.enc_source_staging_size;
            uint copy_size = target_chunk_size - staged_bytes;
            if (source_available < copy_size)
            {
                copy_size = source_available;
            }

            source_bytes[..(int)copy_size]
                .CopyTo(encoder.enc_source_staging_buffer.AsSpan((int)staged_bytes));

            encoder.enc_source_staging_size += copy_size;
            source_consumed = copy_size;
            encode_source = encoder.enc_source_staging_buffer;
            encode_source_is_staging = true;
            available_to_encode = staged_bytes + source_available;
        }
        else
        {
            available_to_encode = source_available;
        }

        while (encoder.enc_dest_staging_size == 0)
        {
            if (available_to_encode < target_chunk_size)
            {
                break;
            }

            if ((available_to_encode == target_chunk_size) &&
                ((source_available != 0) || encoder.enc_stream_flushed))
            {
                break;
            }

            ulong remaining_dest = dest_capacity - dest_written;
            if (remaining_dest < encoder.enc_dest_staging_buffer_size)
            {
                block_info.pDestination = encoder.enc_dest_staging_buffer;
                block_info.DestinationOffset = 0;
                block_info.DestSize = encoder.enc_dest_staging_buffer_size;
            }
            else
            {
                block_info.pDestination = GC.AllocateUninitializedArray<byte>((int)remaining_dest);
                block_info.DestinationOffset = 0;
                block_info.DestSize = remaining_dest;
            }

            block_info.CompressedSize = 0;
            block_info.ChunksEncoded = encoder.enc_chunks_encoded;

            if (target_chunk_size != 0)
            {
                ++encoder.enc_chunks_submitted;

                block_info.ChunksFinal = 0;
                if (source_available == 0)
                {
                    block_info.ChunksFinal = encoder.enc_chunks_submitted;
                }

                _ = LZX_Encode(
                    encoder,
                    encode_source[..(int)target_chunk_size],
                    out int estimated_leftover_bytes,
                    0);
                _ = estimated_leftover_bytes;
            }
            else
            {
                block_info.ChunksFinal = encoder.enc_chunks_submitted;
                LZX_EncodeFlush(encoder);
                encoder.enc_stream_flushed = true;
            }

            encoder.enc_chunks_encoded = block_info.ChunksEncoded;

            if (ReferenceEquals(block_info.pDestination, encoder.enc_dest_staging_buffer))
            {
                uint copy_size = (uint)block_info.CompressedSize;
                if (remaining_dest < copy_size)
                {
                    copy_size = (uint)remaining_dest;
                }

                block_info.pDestination.AsSpan(0, (int)copy_size)
                    .CopyTo(destination.Slice((int)dest_written));

                encoder.enc_dest_staging_offset = copy_size;
                encoder.enc_dest_staging_size = (uint)block_info.CompressedSize - copy_size;
                dest_written += copy_size;
            }
            else
            {
                block_info.pDestination.AsSpan(0, block_info.DestinationOffset)
                    .CopyTo(destination.Slice((int)dest_written));
                dest_written += block_info.CompressedSize;
            }

            if (encode_source_is_staging)
            {
                encode_source = source_bytes[(int)source_consumed..];
                encode_source_is_staging = false;
                encoder.enc_source_staging_size = 0;
            }
            else
            {
                encode_source = encode_source[(int)target_chunk_size..];
                source_consumed += target_chunk_size;
            }

            if (source_available != 0)
            {
                available_to_encode = source_available - source_consumed;
            }
            else
            {
                target_chunk_size = 0;
                available_to_encode = 0;
            }
        }

        uint bytes_to_stage = target_chunk_size - encoder.enc_source_staging_size;
        uint source_remaining = source_available - source_consumed;
        if (source_remaining < bytes_to_stage)
        {
            bytes_to_stage = source_remaining;
        }

        source_bytes
            .Slice((int)source_consumed, (int)bytes_to_stage)
            .CopyTo(encoder.enc_source_staging_buffer.AsSpan((int)encoder.enc_source_staging_size));

        encoder.enc_source_staging_size += bytes_to_stage;
        destSize = (nuint)dest_written;
        srcSize = source_consumed + bytes_to_stage;
        encoder.enc_fci_data = null;
        encoder.enc_output_callback_function = null;
        return 0;
    }

    internal static int XMCDShaInit(t_encoder_context encoder)
    {
        encoder.enc_td_hash?.Dispose();
        encoder.enc_td_hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA1);
        return encoder.enc_td_hash is not null ? 1 : 0;
    }

    internal static int XMCDShaUpdate(t_encoder_context encoder, ReadOnlySpan<byte> data)
    {
        if (encoder.enc_td_hash is null)
        {
            return 0;
        }

        encoder.enc_td_hash.AppendData(data);
        return 1;
    }

    internal static int XMCDShaFinal(t_encoder_context encoder, out uint hash)
    {
        hash = 0;
        if (encoder.enc_td_hash is null)
        {
            return 0;
        }

        Span<byte> digest = stackalloc byte[20];
        encoder.enc_td_hash.GetHashAndReset(digest);
        hash = BinaryPrimitives.ReadUInt32LittleEndian(digest);
        encoder.enc_td_hash.Dispose();
        encoder.enc_td_hash = null;
        return 1;
    }

    private static void SnapshotEncoderContext(t_encoder_context encoder)
    {
        t_encoder_context snapshot = encoder.enc_context_data_snapshot ??= new t_encoder_context();

        CopyToSnapshot(encoder.enc_RealMemWindow, ref snapshot.enc_RealMemWindow);
        snapshot.enc_MemWindowOffset = encoder.enc_MemWindowOffset;
        snapshot.enc_window_size = encoder.enc_window_size;
        snapshot.enc_encoder_second_partition_size = encoder.enc_encoder_second_partition_size;
        CopyToSnapshot(encoder.enc_tree_root, ref snapshot.enc_tree_root);
        CopyToSnapshot(encoder.enc_Left, ref snapshot.enc_Left);
        CopyToSnapshot(encoder.enc_Right, ref snapshot.enc_Right);
        snapshot.enc_bitbuf = encoder.enc_bitbuf;
        snapshot.enc_bitcount = encoder.enc_bitcount;
        snapshot.enc_output_overflow = encoder.enc_output_overflow;
        snapshot.enc_literals = encoder.enc_literals;
        snapshot.enc_distances = encoder.enc_distances;
        CopyToSnapshot(encoder.enc_DistData, ref snapshot.enc_DistData);
        CopyToSnapshot(encoder.enc_LitData, ref snapshot.enc_LitData);
        CopyToSnapshot(encoder.enc_ItemType, ref snapshot.enc_ItemType);
        encoder.enc_repeated_offset_at_literal_zero.CopyTo(snapshot.enc_repeated_offset_at_literal_zero, 0);
        encoder.enc_last_matchpos_offset.CopyTo(snapshot.enc_last_matchpos_offset, 0);
        encoder.enc_matchpos_table.CopyTo(snapshot.enc_matchpos_table, 0);
        snapshot.enc_BufPos = encoder.enc_BufPos;
        CopyToSnapshot(encoder.enc_output_buffer_start, ref snapshot.enc_output_buffer_start);
        snapshot.enc_output_buffer_curpos = encoder.enc_output_buffer_curpos;
        snapshot.enc_output_buffer_end = encoder.enc_output_buffer_end;
        snapshot.enc_input_running_total = encoder.enc_input_running_total;
        snapshot.enc_bufpos_at_last_block = encoder.enc_bufpos_at_last_block;
        snapshot.enc_bufpos_last_output_block = encoder.enc_bufpos_last_output_block;
        snapshot.enc_num_position_slots = encoder.enc_num_position_slots;
        snapshot.enc_file_size_for_translation = encoder.enc_file_size_for_translation;
        snapshot.enc_allocated_compression_memory = encoder.enc_allocated_compression_memory;
        snapshot.enc_num_block_splits = encoder.enc_num_block_splits;
        snapshot.enc_first_block = encoder.enc_first_block;
        snapshot.enc_need_to_recalc_stats = encoder.enc_need_to_recalc_stats;
        snapshot.enc_first_time_this_group = encoder.enc_first_time_this_group;
        snapshot.enc_input_ptr = encoder.enc_input_ptr;
        snapshot.enc_input_left = encoder.enc_input_left;
        snapshot.enc_instr_pos = encoder.enc_instr_pos;
        snapshot.enc_tree_sortptr_index = encoder.enc_tree_sortptr_index;
        encoder.enc_tree_heap.CopyTo(snapshot.enc_tree_heap, 0);
        encoder.enc_tree_leftright.CopyTo(snapshot.enc_tree_leftright, 0);
        encoder.enc_tree_len_cnt.CopyTo(snapshot.enc_tree_len_cnt, 0);
        snapshot.enc_tree_n = encoder.enc_tree_n;
        snapshot.enc_tree_heapsize = encoder.enc_tree_heapsize;
        snapshot.enc_depth = encoder.enc_depth;
        snapshot.enc_next_tree_create = encoder.enc_next_tree_create;
        snapshot.enc_last_literals = encoder.enc_last_literals;
        snapshot.enc_last_distances = encoder.enc_last_distances;
        snapshot.enc_earliest_window_data_remaining = encoder.enc_earliest_window_data_remaining;
        CopyToSnapshot(encoder.enc_main_tree_len, ref snapshot.enc_main_tree_len);
        CopyToSnapshot(encoder.enc_secondary_tree_len, ref snapshot.enc_secondary_tree_len);
        CopyToSnapshot(encoder.enc_main_tree_freq, ref snapshot.enc_main_tree_freq);
        CopyToSnapshot(encoder.enc_main_tree_code, ref snapshot.enc_main_tree_code);
        CopyToSnapshot(encoder.enc_main_tree_prev_len, ref snapshot.enc_main_tree_prev_len);
        CopyToSnapshot(encoder.enc_secondary_tree_freq, ref snapshot.enc_secondary_tree_freq);
        CopyToSnapshot(encoder.enc_secondary_tree_code, ref snapshot.enc_secondary_tree_code);
        CopyToSnapshot(encoder.enc_secondary_tree_prev_len, ref snapshot.enc_secondary_tree_prev_len);
        CopyToSnapshot(encoder.enc_aligned_tree_freq, ref snapshot.enc_aligned_tree_freq);
        CopyToSnapshot(encoder.enc_aligned_tree_code, ref snapshot.enc_aligned_tree_code);
        CopyToSnapshot(encoder.enc_aligned_tree_len, ref snapshot.enc_aligned_tree_len);
        CopyToSnapshot(encoder.enc_aligned_tree_prev_len, ref snapshot.enc_aligned_tree_prev_len);
        snapshot.enc_num_cfdata_frames = encoder.enc_num_cfdata_frames;
        snapshot.enc_inserted_dict_size = encoder.enc_inserted_dict_size;
        snapshot.enc_td_segment_pitch = encoder.enc_td_segment_pitch;
        snapshot.enc_td_uncompressed_size = encoder.enc_td_uncompressed_size;
        snapshot.enc_td_compressed_size = encoder.enc_td_compressed_size;
        snapshot.enc_td_segment_count = encoder.enc_td_segment_count;
        snapshot.enc_td_segment_count_expected = encoder.enc_td_segment_count_expected;
        snapshot.enc_td_translation_padding = encoder.enc_td_translation_padding;
        snapshot.enc_td_translation_bits = encoder.enc_td_translation_bits;
        snapshot.enc_td_translation_bits_expected = encoder.enc_td_translation_bits_expected;
        snapshot.enc_td_encode_data_uncompressed = encoder.enc_td_encode_data_uncompressed;
        snapshot.enc_fci_data = encoder.enc_fci_data;
        snapshot.enc_output_callback_function = encoder.enc_output_callback_function;
    }

    private static void RestoreEncoderContext(t_encoder_context encoder)
    {
        t_encoder_context snapshot = encoder.enc_context_data_snapshot!;

        RestoreFromSnapshot(snapshot.enc_RealMemWindow, ref encoder.enc_RealMemWindow);
        encoder.enc_MemWindowOffset = snapshot.enc_MemWindowOffset;
        encoder.enc_window_size = snapshot.enc_window_size;
        encoder.enc_encoder_second_partition_size = snapshot.enc_encoder_second_partition_size;
        RestoreFromSnapshot(snapshot.enc_tree_root, ref encoder.enc_tree_root);
        RestoreFromSnapshot(snapshot.enc_Left, ref encoder.enc_Left);
        RestoreFromSnapshot(snapshot.enc_Right, ref encoder.enc_Right);
        encoder.enc_bitbuf = snapshot.enc_bitbuf;
        encoder.enc_bitcount = snapshot.enc_bitcount;
        encoder.enc_output_overflow = snapshot.enc_output_overflow;
        encoder.enc_literals = snapshot.enc_literals;
        encoder.enc_distances = snapshot.enc_distances;
        RestoreFromSnapshot(snapshot.enc_DistData, ref encoder.enc_DistData);
        RestoreFromSnapshot(snapshot.enc_LitData, ref encoder.enc_LitData);
        RestoreFromSnapshot(snapshot.enc_ItemType, ref encoder.enc_ItemType);
        snapshot.enc_repeated_offset_at_literal_zero.CopyTo(encoder.enc_repeated_offset_at_literal_zero, 0);
        snapshot.enc_last_matchpos_offset.CopyTo(encoder.enc_last_matchpos_offset, 0);
        snapshot.enc_matchpos_table.CopyTo(encoder.enc_matchpos_table, 0);
        encoder.enc_BufPos = snapshot.enc_BufPos;
        RestoreFromSnapshot(snapshot.enc_output_buffer_start, ref encoder.enc_output_buffer_start);
        encoder.enc_output_buffer_curpos = snapshot.enc_output_buffer_curpos;
        encoder.enc_output_buffer_end = snapshot.enc_output_buffer_end;
        encoder.enc_input_running_total = snapshot.enc_input_running_total;
        encoder.enc_bufpos_at_last_block = snapshot.enc_bufpos_at_last_block;
        encoder.enc_bufpos_last_output_block = snapshot.enc_bufpos_last_output_block;
        encoder.enc_num_position_slots = snapshot.enc_num_position_slots;
        encoder.enc_file_size_for_translation = snapshot.enc_file_size_for_translation;
        encoder.enc_allocated_compression_memory = snapshot.enc_allocated_compression_memory;
        encoder.enc_num_block_splits = snapshot.enc_num_block_splits;
        encoder.enc_first_block = snapshot.enc_first_block;
        encoder.enc_need_to_recalc_stats = snapshot.enc_need_to_recalc_stats;
        encoder.enc_first_time_this_group = snapshot.enc_first_time_this_group;
        encoder.enc_input_ptr = snapshot.enc_input_ptr;
        encoder.enc_input_left = snapshot.enc_input_left;
        encoder.enc_instr_pos = snapshot.enc_instr_pos;
        encoder.enc_tree_sortptr_index = snapshot.enc_tree_sortptr_index;
        snapshot.enc_tree_heap.CopyTo(encoder.enc_tree_heap, 0);
        snapshot.enc_tree_leftright.CopyTo(encoder.enc_tree_leftright, 0);
        snapshot.enc_tree_len_cnt.CopyTo(encoder.enc_tree_len_cnt, 0);
        encoder.enc_tree_n = snapshot.enc_tree_n;
        encoder.enc_tree_heapsize = snapshot.enc_tree_heapsize;
        encoder.enc_depth = snapshot.enc_depth;
        encoder.enc_next_tree_create = snapshot.enc_next_tree_create;
        encoder.enc_last_literals = snapshot.enc_last_literals;
        encoder.enc_last_distances = snapshot.enc_last_distances;
        encoder.enc_earliest_window_data_remaining = snapshot.enc_earliest_window_data_remaining;
        RestoreFromSnapshot(snapshot.enc_main_tree_len, ref encoder.enc_main_tree_len);
        RestoreFromSnapshot(snapshot.enc_secondary_tree_len, ref encoder.enc_secondary_tree_len);
        RestoreFromSnapshot(snapshot.enc_main_tree_freq, ref encoder.enc_main_tree_freq);
        RestoreFromSnapshot(snapshot.enc_main_tree_code, ref encoder.enc_main_tree_code);
        RestoreFromSnapshot(snapshot.enc_main_tree_prev_len, ref encoder.enc_main_tree_prev_len);
        RestoreFromSnapshot(snapshot.enc_secondary_tree_freq, ref encoder.enc_secondary_tree_freq);
        RestoreFromSnapshot(snapshot.enc_secondary_tree_code, ref encoder.enc_secondary_tree_code);
        RestoreFromSnapshot(snapshot.enc_secondary_tree_prev_len, ref encoder.enc_secondary_tree_prev_len);
        RestoreFromSnapshot(snapshot.enc_aligned_tree_freq, ref encoder.enc_aligned_tree_freq);
        RestoreFromSnapshot(snapshot.enc_aligned_tree_code, ref encoder.enc_aligned_tree_code);
        RestoreFromSnapshot(snapshot.enc_aligned_tree_len, ref encoder.enc_aligned_tree_len);
        RestoreFromSnapshot(snapshot.enc_aligned_tree_prev_len, ref encoder.enc_aligned_tree_prev_len);
        encoder.enc_num_cfdata_frames = snapshot.enc_num_cfdata_frames;
        encoder.enc_inserted_dict_size = snapshot.enc_inserted_dict_size;
        encoder.enc_td_segment_pitch = snapshot.enc_td_segment_pitch;
        encoder.enc_td_uncompressed_size = snapshot.enc_td_uncompressed_size;
        encoder.enc_td_compressed_size = snapshot.enc_td_compressed_size;
        encoder.enc_td_segment_count = snapshot.enc_td_segment_count;
        encoder.enc_td_segment_count_expected = snapshot.enc_td_segment_count_expected;
        encoder.enc_td_translation_padding = snapshot.enc_td_translation_padding;
        encoder.enc_td_translation_bits = snapshot.enc_td_translation_bits;
        encoder.enc_td_translation_bits_expected = snapshot.enc_td_translation_bits_expected;
        encoder.enc_td_encode_data_uncompressed = snapshot.enc_td_encode_data_uncompressed;
        encoder.enc_fci_data = snapshot.enc_fci_data;
        encoder.enc_output_callback_function = snapshot.enc_output_callback_function;
    }

    private static void CopyToSnapshot<T>(T[] source, ref T[] destination)
    {
        EnsureArrayLength(ref destination, source.Length);
        source.AsSpan().CopyTo(destination);
    }

    private static void RestoreFromSnapshot<T>(T[] source, ref T[] destination)
    {
        EnsureArrayLength(ref destination, source.Length);
        source.AsSpan(0, source.Length).CopyTo(destination);
    }

    private static void EnsureArrayLength<T>(ref T[] array, int required)
    {
        if (array.Length != required)
        {
            array = GC.AllocateUninitializedArray<T>(required);
        }
    }

    private static void EnsureArrayCapacity<T>(ref T[] array, int required)
    {
        if (array.Length < required)
        {
            array = GC.AllocateUninitializedArray<T>(required);
        }
    }

    private static int LzxEncoderTDCallback(
        object? parameter,
        ReadOnlySpan<byte> data,
        int compressedSize,
        int uncompressedSize)
    {
        _ = uncompressedSize;
        EncoderBlockInfo block = (EncoderBlockInfo)parameter!;

        if ((block.CompressedSize + 2) <= block.DestSize)
        {
            block.pDestination[block.DestinationOffset + 0] = (byte)(compressedSize >> 8);
            block.pDestination[block.DestinationOffset + 1] = (byte)compressedSize;
            block.DestinationOffset += 2;
        }

        block.CompressedSize += 2;

        ulong bytes_available = 0;
        if (block.DestSize >= block.CompressedSize)
        {
            bytes_available = block.DestSize - block.CompressedSize;
        }

        ulong bytes_to_copy = (uint)compressedSize;
        if (bytes_to_copy > bytes_available)
        {
            bytes_to_copy = bytes_available;
        }

        data[..(int)bytes_to_copy].CopyTo(block.pDestination.AsSpan(block.DestinationOffset));
        block.CompressedSize += (uint)compressedSize;
        block.DestinationOffset += (int)bytes_to_copy;
        return 0;
    }

    private static int LzxEncoderCallback(
        object? parameter,
        ReadOnlySpan<byte> data,
        int compressedSize,
        int uncompressedSize)
    {
        StreamEncoderBlockInfo block = (StreamEncoderBlockInfo)parameter!;
        ++block.ChunksEncoded;

        uint trailer_size = 0;
        if (block.ChunksEncoded != block.ChunksFinal)
        {
            if ((block.CompressedSize + 2) <= block.DestSize)
            {
                block.pDestination[block.DestinationOffset + 0] = (byte)(compressedSize >> 8);
                block.pDestination[block.DestinationOffset + 1] = (byte)compressedSize;
                block.DestinationOffset += 2;
            }

            block.CompressedSize += 2;
        }
        else
        {
            trailer_size = 5;
            if ((block.CompressedSize + trailer_size) <= block.DestSize)
            {
                block.pDestination[block.DestinationOffset + 0] = 0xFF;
                block.pDestination[block.DestinationOffset + 1] = (byte)(uncompressedSize >> 8);
                block.pDestination[block.DestinationOffset + 2] = (byte)uncompressedSize;
                block.pDestination[block.DestinationOffset + 3] = (byte)(compressedSize >> 8);
                block.pDestination[block.DestinationOffset + 4] = (byte)compressedSize;
                block.DestinationOffset += (int)trailer_size;
            }

            block.CompressedSize += trailer_size;
        }

        ulong bytes_available = 0;
        if (block.DestSize >= block.CompressedSize)
        {
            bytes_available = block.DestSize - block.CompressedSize;
        }

        ulong bytes_to_copy = (uint)compressedSize;
        if (bytes_to_copy > bytes_available)
        {
            bytes_to_copy = bytes_available;
        }

        data[..(int)bytes_to_copy].CopyTo(block.pDestination.AsSpan(block.DestinationOffset));
        block.DestinationOffset += (int)bytes_to_copy;
        block.CompressedSize += (uint)compressedSize;

        if (trailer_size != 0)
        {
            bytes_available = 0;
            if (block.DestSize >= block.CompressedSize)
            {
                bytes_available = block.DestSize - block.CompressedSize;
            }

            ulong bytes_to_zero = trailer_size;
            if (bytes_to_zero > bytes_available)
            {
                bytes_to_zero = bytes_available;
            }

            block.pDestination.AsSpan(block.DestinationOffset, (int)bytes_to_zero).Clear();
            block.DestinationOffset += (int)bytes_to_zero;
            block.CompressedSize += trailer_size;
        }

        return 0;
    }
}
