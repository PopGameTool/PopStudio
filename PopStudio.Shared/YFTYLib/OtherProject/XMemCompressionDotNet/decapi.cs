#nullable enable
using System;

namespace XMemCompressionDotNet;

internal static partial class XCompress
{
    internal static void LZX_DecodeFree(t_decoder_context context)
    {
        free_decompression_memory(context);
    }

    internal static void LZX_DecodeNewGroup(t_decoder_context context)
    {
        reset_decoder_trees(context);
        decoder_misc_init(context);
        init_decoder_translation(context);
        context.dec_num_cfdata_frames = 0;
    }

    internal static int LZX_Decode(
        t_decoder_context context,
        int bytes_to_decode,
        ReadOnlySpan<byte> compressed_input_buffer,
        Span<byte> uncompressed_output_buffer,
        out int bytes_decoded)
    {
        int inputLengthWithSentinel = compressed_input_buffer.Length + 4;
        if (context.dec_input.Length < inputLengthWithSentinel)
        {
            context.dec_input = GC.AllocateUninitializedArray<byte>(inputLengthWithSentinel);
        }

        compressed_input_buffer.CopyTo(context.dec_input);
        context.dec_input.AsSpan(compressed_input_buffer.Length, 4).Clear();
        context.dec_input_curpos = 0;

        int outputLength = Math.Min(bytes_to_decode, uncompressed_output_buffer.Length);
        if (context.dec_output_buffer is null || context.dec_output_buffer.Length < outputLength)
        {
            context.dec_output_buffer = GC.AllocateUninitializedArray<byte>(outputLength);
        }

        context.dec_end_input_pos = inputLengthWithSentinel;

        init_decoder_input(context);

        int decoded = decode_data(context, bytes_to_decode);
        ++context.dec_num_cfdata_frames;

        if (decoded < 0)
        {
            bytes_decoded = 0;
            return 1;
        }

        bytes_decoded = decoded;
        context.dec_position_at_start += decoded;
        context.dec_output_buffer.AsSpan(0, Math.Min(decoded, uncompressed_output_buffer.Length))
            .CopyTo(uncompressed_output_buffer);
        return 0;
    }

    internal static bool LZX_DecodeInsertDictionary(
        t_decoder_context context,
        ReadOnlySpan<byte> data)
    {
        uint window_size = context.dec_window_size;
        if ((uint)data.Length > window_size)
        {
            return false;
        }

        data.CopyTo(context.dec_mem_window.AsSpan((int)(window_size - (uint)data.Length), data.Length));

        if ((uint)data.Length < window_size)
        {
            context.dec_mem_window.AsSpan(0, (int)(window_size - (uint)data.Length)).Clear();
        }

        return true;
    }

    internal static void build_global_tables(t_decoder_context context)
    {
        context.dec_extra_bits_table[0] = 0x00;
        context.dec_extra_bits_table[1] = 0x00;
        context.dec_extra_bits_table[2] = 0x00;
        context.dec_extra_bits_table[3] = 0x00;
        context.dec_extra_bits_table[4] = 0x01;
        context.dec_extra_bits_table[5] = 0x01;
        context.dec_extra_bits_table[6] = 0x02;
        context.dec_extra_bits_table[7] = 0x02;
        context.dec_extra_bits_table[8] = 0x03;
        context.dec_extra_bits_table[9] = 0x03;
        context.dec_extra_bits_table[10] = 0x04;
        context.dec_extra_bits_table[11] = 0x04;
        context.dec_extra_bits_table[12] = 0x05;
        context.dec_extra_bits_table[13] = 0x05;
        context.dec_extra_bits_table[14] = 0x06;
        context.dec_extra_bits_table[15] = 0x06;
        context.dec_extra_bits_table[16] = 0x07;
        context.dec_extra_bits_table[17] = 0x07;
        context.dec_extra_bits_table[18] = 0x08;
        context.dec_extra_bits_table[19] = 0x08;
        context.dec_extra_bits_table[20] = 0x09;
        context.dec_extra_bits_table[21] = 0x09;
        context.dec_extra_bits_table[22] = 0x0A;
        context.dec_extra_bits_table[23] = 0x0A;
        context.dec_extra_bits_table[24] = 0x0B;
        context.dec_extra_bits_table[25] = 0x0B;
        context.dec_extra_bits_table[26] = 0x0C;
        context.dec_extra_bits_table[27] = 0x0C;
        context.dec_extra_bits_table[28] = 0x0D;
        context.dec_extra_bits_table[29] = 0x0D;
        context.dec_extra_bits_table[30] = 0x0E;
        context.dec_extra_bits_table[31] = 0x0E;
        context.dec_extra_bits_table[32] = 0x0F;
        context.dec_extra_bits_table[33] = 0x0F;
        context.dec_extra_bits_table[34] = 0x10;
        context.dec_extra_bits_table[35] = 0x10;
        context.dec_extra_bits_table[36] = 0x11;
        context.dec_extra_bits_table[37] = 0x11;
        context.dec_extra_bits_table[38] = 0x11;
        context.dec_extra_bits_table[39] = 0x11;
        context.dec_extra_bits_table[40] = 0x11;
        context.dec_extra_bits_table[41] = 0x11;
        context.dec_extra_bits_table[42] = 0x11;
        context.dec_extra_bits_table[43] = 0x11;
        context.dec_extra_bits_table[44] = 0x11;
        context.dec_extra_bits_table[45] = 0x11;
        context.dec_extra_bits_table[46] = 0x11;
        context.dec_extra_bits_table[47] = 0x11;
        context.dec_extra_bits_table[48] = 0x11;
        context.dec_extra_bits_table[49] = 0x11;
        context.dec_extra_bits_table[50] = 0x11;
        context.dec_extra_bits_table[51] = 0x11;

        context.MP_POS_minus2_table[0] = -2;
        context.MP_POS_minus2_table[1] = -1;
        context.MP_POS_minus2_table[2] = 0;
        context.MP_POS_minus2_table[3] = 1;
        context.MP_POS_minus2_table[4] = 2;
        context.MP_POS_minus2_table[5] = 4;
        context.MP_POS_minus2_table[6] = 6;
        context.MP_POS_minus2_table[7] = 10;
        context.MP_POS_minus2_table[8] = 14;
        context.MP_POS_minus2_table[9] = 22;
        context.MP_POS_minus2_table[10] = 30;
        context.MP_POS_minus2_table[11] = 46;
        context.MP_POS_minus2_table[12] = 62;
        context.MP_POS_minus2_table[13] = 94;
        context.MP_POS_minus2_table[14] = 126;
        context.MP_POS_minus2_table[15] = 190;
        context.MP_POS_minus2_table[16] = 254;
        context.MP_POS_minus2_table[17] = 382;
        context.MP_POS_minus2_table[18] = 510;
        context.MP_POS_minus2_table[19] = 766;
        context.MP_POS_minus2_table[20] = 1022;
        context.MP_POS_minus2_table[21] = 1534;
        context.MP_POS_minus2_table[22] = 2046;
        context.MP_POS_minus2_table[23] = 3070;
        context.MP_POS_minus2_table[24] = 4094;
        context.MP_POS_minus2_table[25] = 6142;
        context.MP_POS_minus2_table[26] = 8190;
        context.MP_POS_minus2_table[27] = 12286;
        context.MP_POS_minus2_table[28] = 16382;
        context.MP_POS_minus2_table[29] = 24574;
        context.MP_POS_minus2_table[30] = 32766;
        context.MP_POS_minus2_table[31] = 49150;
        context.MP_POS_minus2_table[32] = 65534;
        context.MP_POS_minus2_table[33] = 98302;
        context.MP_POS_minus2_table[34] = 131070;
        context.MP_POS_minus2_table[35] = 196606;
        context.MP_POS_minus2_table[36] = 262142;
        context.MP_POS_minus2_table[37] = 393214;
        context.MP_POS_minus2_table[38] = 524286;
        context.MP_POS_minus2_table[39] = 655358;
        context.MP_POS_minus2_table[40] = 786430;
        context.MP_POS_minus2_table[41] = 917502;
        context.MP_POS_minus2_table[42] = 1048574;
        context.MP_POS_minus2_table[43] = 1179646;
        context.MP_POS_minus2_table[44] = 1310718;
        context.MP_POS_minus2_table[45] = 1441790;
        context.MP_POS_minus2_table[46] = 1572862;
        context.MP_POS_minus2_table[47] = 1703934;
        context.MP_POS_minus2_table[48] = 1835006;
        context.MP_POS_minus2_table[49] = 1966078;
        context.MP_POS_minus2_table[50] = 2097150;
    }

    internal static byte[] dec_malloc(t_decoder_context context, uint cb)
    {
        if (context.dec_malloc is not null)
        {
            return context.dec_malloc(cb);
        }

        if (context.dec_memory is not null)
        {
            byte[] memory = context.dec_memory;
            int offset = context.dec_memory_offset;
            int size = checked((int)cb);
            context.dec_memory_offset = checked(offset + size);

            if (offset == 0 && memory.Length == size)
            {
                return memory;
            }

            byte[] slice = GC.AllocateUninitializedArray<byte>(size);
            int available = Math.Max(0, Math.Min(size, memory.Length - offset));
            if (available > 0)
            {
                memory.AsSpan(offset, available).CopyTo(slice);
            }

            return slice;
        }

        return GC.AllocateUninitializedArray<byte>((int)cb);
    }

    internal static void dec_free(t_decoder_context context, byte[]? pv)
    {
        if (context.dec_free is not null)
        {
            context.dec_free(pv);
        }
    }

    internal static bool LZX_DecodeInit(t_decoder_context context, int compression_window_size)
    {
        build_global_tables(context);

        uint window_size = (uint)compression_window_size;
        context.dec_window_size = window_size;
        context.dec_window_mask = window_size - 1;

        if ((window_size & context.dec_window_mask) != 0)
        {
            return false;
        }

        if (!allocate_decompression_memory(context))
        {
            return false;
        }

        LZX_DecodeNewGroup(context);
        return true;
    }
}
