#nullable enable
using System;

namespace XMemCompressionDotNet;

internal static partial class XCompress
{
    internal static uint get_dec_mem_window_alloc_size(uint dec_window_size)
    {
        return dec_window_size + 0x105;
    }

    internal static bool allocate_decompression_memory(t_decoder_context context)
    {
        uint total = 4;
        context.dec_num_position_slots = 4;

        do
        {
            byte slot = context.dec_num_position_slots;
            byte extra_bits = context.dec_extra_bits_table[slot];
            context.dec_num_position_slots = (byte)(slot + 1);
            total += 1u << extra_bits;
        }
        while (total < context.dec_window_size);

        context.dec_mem_window = dec_malloc(context, get_dec_mem_window_alloc_size(context.dec_window_size));
        return context.dec_mem_window.Length != 0;
    }

    internal static void free_decompression_memory(t_decoder_context context)
    {
        if (context.dec_mem_window.Length != 0)
        {
            dec_free(context, context.dec_mem_window);
            context.dec_mem_window = [];
        }
    }

    internal static void init_decompression_memory_context(
        t_decoder_context? context,
        out ulong contextSize,
        int windowSize,
        uint headerSize,
        uint flags)
    {
        bool use_streaming_buffers = (flags & 1) != 0;
        bool use_td_staging = (flags & 0x80000000u) != 0;

        uint context_size = get_dec_mem_window_alloc_size((uint)windowSize);
        context_size += headerSize + 0x3040;
        if (use_streaming_buffers)
        {
            context_size += 0x980A;
        }
        else if (use_td_staging)
        {
            context_size += 0x8000;
        }

        context_size += 0x8000;
        contextSize = context_size;
        if (context is null)
        {
            return;
        }

        build_global_tables(context);

        context.dec_malloc = null;
        context.dec_free = null;
        context.dec_memory = null;
        context.dec_memory_offset = 0;
        context.dec_window_size = (uint)windowSize;
        context.dec_num_position_slots = 4;
        context.dec_window_mask = (uint)(windowSize - 1);

        uint total = 4;
        do
        {
            byte slot = context.dec_num_position_slots;
            byte extra_bits = context.dec_extra_bits_table[slot];
            context.dec_num_position_slots = (byte)(slot + 1);
            total += 1u << extra_bits;
        }
        while (total < context.dec_window_size);

        context.dec_td_last_source = default;
        context.dec_td_last_source_array = null;
        context.dec_td_last_source_owner = null;
        context.dec_td_last_source_identity_offset = 0;
        context.dec_td_last_source_size = 0;
        context.dec_td_last_segment_size = 0;
        context.dec_td_last_segment_offset = 0;
        context.dec_td_last_source_offset = 0;
        context.dec_td_last_decoded_size = 0;
        context.dec_td_last_stage_offset = 0;
        context.dec_td_last_stage_size = 0;

        context.dec_mem_window = GC.AllocateUninitializedArray<byte>((int)get_dec_mem_window_alloc_size((uint)windowSize));
        context.dec_dest_staging_buffer = new byte[0x8000];
        if (use_streaming_buffers)
        {
            context.dec_source_staging_buffer = new byte[0x980A];
        }
        else
        {
            context.dec_source_staging_buffer = [];
        }

        if (use_td_staging)
        {
            context.dec_td_staging_buffer = new byte[0x8000];
        }
        else
        {
            context.dec_td_staging_buffer = [];
        }
    }

    internal static void reset_decoder_trees(t_decoder_context context)
    {
        int main_tree_size = 8 * context.dec_num_position_slots + 0x100;
        context.dec_main_tree_len.AsSpan(0, main_tree_size).Clear();
        context.dec_main_tree_prev_len.AsSpan(0, main_tree_size).Clear();
        context.dec_secondary_length_tree_len.AsSpan(0, 0xF9).Clear();
        context.dec_secondary_length_tree_prev_len.AsSpan(0, 0xF9).Clear();
    }

    internal static void decoder_misc_init(t_decoder_context context)
    {
        context.dec_last_matchpos_offset[0] = 1;
        context.dec_last_matchpos_offset[1] = 1;
        context.dec_last_matchpos_offset[2] = 1;
        context.dec_position_at_start = 0;
        context.dec_decoder_state = decoder_state.DEC_STATE_START_NEW_BLOCK;
        context.dec_block_size = 0;
        context.dec_block_type = lzx_block_type.BLOCKTYPE_INVALID;
        context.dec_bufpos = 0;
        context.dec_current_file_size = 0;
        context.dec_first_time_this_group = true;
        context.dec_error_condition = false;
    }
}
