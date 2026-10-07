#nullable enable
using System;

namespace XMemCompressionDotNet;

internal static partial class XCompress
{
    private static ReadOnlySpan<byte> kEncExtraBits =>
    [
        0, 0, 0, 0,
        1, 1, 2, 2,
        3, 3, 4, 4,
        5, 5, 6, 6,
        7, 7, 8, 8,
        9, 9, 10, 10,
        11, 11, 12, 12,
        13, 13, 14, 14,
        15, 15, 16, 16,
        17, 17, 17, 17,
        17, 17, 17, 17,
        17, 17, 17, 17,
        17, 17, 17, 17,
    ];

    internal static uint DetermineNumPositionSlots(uint window_size)
    {
        uint total = 4;
        uint slot = 4;

        do
        {
            uint extra_bits = kEncExtraBits[(int)slot];
            ++slot;
            total += 1u << (int)extra_bits;
        }
        while (total < window_size);

        return slot;
    }

    internal static void init_compression_memory(t_encoder_context context)
    {
        context.enc_tree_root.AsSpan().Clear();

        uint window_size = context.enc_window_size;
        uint num_position_slots = context.enc_num_position_slots;
        context.enc_bitbuf = 0;
        context.enc_MemWindowOffset = -(int)window_size;
        context.enc_BufPos = window_size;
        context.enc_bufpos_last_output_block = window_size;
        context.enc_last_matchpos_offset[0] = 1;
        context.enc_Left ??= new uint[context.enc_RealMemWindow.Length];
        context.enc_last_matchpos_offset[1] = 1;
        context.enc_Right ??= new uint[context.enc_RealMemWindow.Length];
        context.enc_last_matchpos_offset[2] = 1;
        context.enc_repeated_offset_at_literal_zero[0] = 1;
        context.enc_repeated_offset_at_literal_zero[1] = 1;
        context.enc_repeated_offset_at_literal_zero[2] = 1;
        context.enc_first_block = 1;
        context.enc_need_to_recalc_stats = true;
        context.enc_bitcount = 32;

        context.enc_main_tree_prev_len.AsSpan(0, (int)(8 * num_position_slots + 0x100)).Clear();
        context.enc_secondary_tree_prev_len.AsSpan(0, 0xF9).Clear();
        context.enc_main_tree_len.AsSpan(0, 0x100).Fill(8);
        context.enc_main_tree_len.AsSpan(0x100, (int)(8 * num_position_slots)).Fill(9);
        context.enc_secondary_tree_len.AsSpan(0, 0xF9).Fill(6);
        context.enc_aligned_tree_len.AsSpan().Fill(3);

        prevent_far_matches(context);

        context.enc_input_running_total = 0;
        context.enc_bufpos_at_last_block = context.enc_BufPos;
        context.enc_earliest_window_data_remaining = context.enc_BufPos;
        context.enc_first_time_this_group = true;

        context.enc_ItemType.AsSpan().Clear();
        context.enc_literals = 0;
        context.enc_distances = 0;
        context.enc_num_block_splits = 0;
        context.enc_repeated_offset_at_literal_zero[0] = 1;
        context.enc_repeated_offset_at_literal_zero[1] = 1;
        context.enc_repeated_offset_at_literal_zero[2] = 1;

        reset_translation(context);

        context.enc_num_cfdata_frames = 0;
        context.enc_inserted_dict_size = 0;

        context.enc_main_tree_freq.AsSpan().Clear();
        context.enc_secondary_tree_freq.AsSpan().Clear();
        context.enc_aligned_tree_freq.AsSpan().Clear();
        context.enc_RealMemWindow.AsSpan().Clear();
    }

    internal static void comp_free_compress_memory(t_encoder_context context)
    {
        context.enc_allocated_compression_memory = false;
    }

    internal static void comp_init_compress_memory_context(
        t_encoder_context? context,
        out ulong contextSize,
        int windowSize,
        int secondPartitionSize,
        uint headerSize,
        uint flags)
    {
        bool enable_streaming = (flags & 1) != 0;
        bool preserve_snapshot = (flags & 0x80000000u) != 0;

        uint base_window_bytes = (uint)(secondPartitionSize + windowSize + 0x1101);
        uint rounded_window_bytes = (base_window_bytes + 3) & ~3u;
        uint tree_bytes = base_window_bytes * 4;
        uint base_context_size = 2 * tree_bytes + rounded_window_bytes + 0x97D58u;
        uint streaming_extra = enable_streaming ? 0x26028u + 0x8000u : 0u;
        uint snapshot_extra = preserve_snapshot ? 0x9D89Cu + base_context_size : 0u;

        contextSize = headerSize + base_context_size + streaming_extra + snapshot_extra;

        if (context is null)
        {
            return;
        }

        context.enc_window_size = (uint)windowSize;
        context.enc_encoder_second_partition_size = (uint)secondPartitionSize;
        context.enc_num_position_slots = DetermineNumPositionSlots((uint)windowSize);

        int memSize = checked((int)(context.enc_encoder_second_partition_size + context.enc_window_size + 0x1101u));
        context.enc_RealMemWindow = new byte[memSize];
        context.enc_Left = new uint[memSize];
        context.enc_Right = new uint[memSize];
        context.enc_MemWindowOffset = 0;
        context.enc_LitData = new byte[0x10000];
        context.enc_DistData = new uint[0x20000 / 4];
        context.enc_ItemType = new byte[0x2000];
        context.enc_output_buffer_start = new byte[0x9800];
        context.enc_output_buffer_curpos = 0;
        context.enc_output_buffer_end = 0x97C0;
        context.enc_decision_node = new decision_node[0x18150 / 24];
        context.enc_tree_root = new uint[0x10000];

        create_slot_lookup_table(context);
        create_ones_table(context);

        if (enable_streaming)
        {
            context.enc_dest_staging_buffer = new byte[0x26028];
            context.enc_dest_staging_buffer_size = 0x26028u;
            context.enc_source_staging_buffer = new byte[0x8000];
            context.enc_dest_staging_size = 0;
            context.enc_source_staging_size = 0;
            context.enc_chunks_submitted = 0;
            context.enc_chunks_encoded = 0;
            context.enc_stream_flushed = true;
        }
        else if (preserve_snapshot)
        {
            context.enc_source_staging_buffer = new byte[0x9D89C];
            context.enc_context_data_snapshot = new t_encoder_context();
        }

        context.enc_context_data_size = base_context_size;
        context.enc_allocated_compression_memory = true;
    }

    internal static void comp_clear_compress_memory_context(t_encoder_context context)
    {
        context.enc_num_position_slots = DetermineNumPositionSlots(context.enc_window_size);
        context.enc_Left.AsSpan().Clear();
        context.enc_Right.AsSpan().Clear();
        context.enc_RealMemWindow.AsSpan().Clear();
        context.enc_LitData.AsSpan().Clear();
        context.enc_DistData.AsSpan().Clear();
        context.enc_ItemType.AsSpan().Clear();
        context.enc_output_buffer_start.AsSpan().Clear();
        context.enc_decision_node.AsSpan().Clear();
        context.enc_tree_root.AsSpan().Clear();
        context.enc_output_buffer_curpos = 0;
        context.enc_output_buffer_end = 0x97C0;
        context.enc_MemWindowOffset = 0;
    }

    internal static bool comp_alloc_compress_memory(t_encoder_context context)
    {
        comp_init_compress_memory_context(
            context,
            out _,
            (int)context.enc_window_size,
            (int)context.enc_encoder_second_partition_size,
            0,
            0);
        return true;
    }
}
