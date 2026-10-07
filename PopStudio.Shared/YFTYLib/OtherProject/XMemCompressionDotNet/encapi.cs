#nullable enable
using System;

namespace XMemCompressionDotNet;

internal static partial class XCompress
{
    internal static void LZX_EncodeFree(t_encoder_context context)
    {
        comp_free_compress_memory(context);
    }

    internal static void LZX_EncodeNewGroup(t_encoder_context context)
    {
        init_compression_memory(context);
    }

    internal static unsafe int LZX_Encode(
        t_encoder_context context,
        ReadOnlySpan<byte> input_data,
        out int estimated_bytes_compressed,
        int file_size_for_translation)
    {
        fixed (byte* inputPointer = input_data)
        {
            context.enc_direct_input = inputPointer;
            context.enc_input_ptr = 0;
            context.enc_input_left = input_data.Length;
            context.enc_file_size_for_translation = (uint)file_size_for_translation;

            try
            {
                encoder_start(context);
            }
            finally
            {
                context.enc_direct_input = null;
            }
        }

        if (context.enc_output_overflow)
        {
            estimated_bytes_compressed = 0;
            return 2;
        }

        estimated_bytes_compressed = estimate_buffer_contents(context);
        return 0;
    }

    internal static bool LZX_EncodeFlush(t_encoder_context context)
    {
        flush_all_pending_blocks(context);
        return !context.enc_output_overflow;
    }

    internal static void LZX_EncodeResetState(t_encoder_context context)
    {
        context.enc_ItemType.AsSpan().Clear();

        uint num_position_slots = context.enc_num_position_slots;

        context.enc_literals = 0;
        context.enc_distances = 0;
        context.enc_input_running_total = 0;

        context.enc_last_matchpos_offset[0] = 1;
        context.enc_last_matchpos_offset[1] = 1;
        context.enc_last_matchpos_offset[2] = 1;

        context.enc_repeated_offset_at_literal_zero[0] = 1;
        context.enc_repeated_offset_at_literal_zero[1] = 1;
        context.enc_repeated_offset_at_literal_zero[2] = 1;

        context.enc_main_tree_prev_len.AsSpan(0, (int)(8 * num_position_slots + 0x100)).Clear();
        context.enc_secondary_tree_prev_len.AsSpan(0, 0xF9).Clear();

        context.enc_bitbuf = 0;
        context.enc_bitcount = 32;
        context.enc_output_overflow = false;
        context.enc_bufpos_last_output_block = context.enc_BufPos;
        context.enc_next_tree_create = 0x1000;
        context.enc_first_block = 0;
        context.enc_need_to_recalc_stats = true;

        reset_translation(context);

        context.enc_num_cfdata_frames = 0;
        context.enc_first_time_this_group = true;
        context.enc_num_block_splits = 0;
    }

    internal static Span<byte> LZX_GetInputData(
        t_encoder_context context,
        out uint input_position,
        out uint bytes_available)
    {
        uint position = context.enc_BufPos - context.enc_window_size;
        uint window_offset = context.enc_window_size;

        if (position >= context.enc_window_size)
        {
            position -= context.enc_window_size;
            input_position = position;
            bytes_available = context.enc_window_size;
            window_offset = context.enc_BufPos - context.enc_window_size;
        }
        else
        {
            input_position = 0;
            bytes_available = position;
        }

        int offset = context.enc_MemWindowOffset + (int)window_offset;
        return context.enc_RealMemWindow.AsSpan(offset, (int)bytes_available);
    }

    internal static unsafe void LZX_EncodeInsertDictionary(
        t_encoder_context context,
        ReadOnlySpan<byte> input_data)
    {
        context.enc_file_size_for_translation = 0;
        context.enc_first_time_this_group = false;

        uint input_position = (uint)(context.enc_MemWindowOffset + (int)context.enc_BufPos);
        uint bytes_read;
        fixed (byte* inputPointer = input_data)
        {
            context.enc_direct_input = inputPointer;
            context.enc_input_ptr = 0;
            context.enc_input_left = input_data.Length;

            try
            {
                bytes_read = (uint)comp_read_input(context, input_position, input_data.Length);
            }
            finally
            {
                context.enc_direct_input = null;
            }
        }

        uint bufpos = context.enc_BufPos;
        context.enc_inserted_dict_size += bytes_read;

        uint end_bufpos = bufpos + bytes_read;
        while (bufpos < end_bufpos)
        {
            quick_insert_bsearch_findmatch(context, bufpos, bufpos - context.enc_window_size + 4);
            ++bufpos;
        }

        context.enc_earliest_window_data_remaining = bufpos - context.enc_window_size;

        uint path = context.enc_earliest_window_data_remaining + 0x36;
        for (uint i = 1; i <= 0x32; ++i)
        {
            binary_search_remove_node(context, bufpos - i, path);
        }

        context.enc_BufPos = bufpos;
        context.enc_bufpos_at_last_block = bufpos;
    }

    internal static bool LZX_EncodeInit(
        t_encoder_context context,
        int compression_window_size,
        int second_partition_size)
    {
        context.enc_window_size = (uint)compression_window_size;

        if ((second_partition_size & 0x7FFF) != 0)
        {
            second_partition_size &= unchecked((int)0xFFFF8000);
        }

        if (second_partition_size < 0x8000)
        {
            second_partition_size = 0x8000;
        }

        if (compression_window_size < 0x8000)
        {
            return false;
        }

        context.enc_encoder_second_partition_size = (uint)second_partition_size;

        if (!comp_alloc_compress_memory(context))
        {
            return false;
        }

        init_compression_memory(context);
        return true;
    }
}
