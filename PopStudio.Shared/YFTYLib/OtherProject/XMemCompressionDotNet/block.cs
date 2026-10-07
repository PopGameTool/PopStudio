#nullable enable
using System;

namespace XMemCompressionDotNet;

internal static partial class XCompress
{
    internal static uint get_distances_from_literals(t_encoder_context context, uint literals)
    {
        uint distances = 0;

        uint whole_bytes = literals >> 3;
        for (uint i = 0; i < whole_bytes; ++i)
        {
            distances += context.enc_ones[context.enc_ItemType[i]];
        }

        for (uint literal_index = literals & 0xFFFFFFF8u; literal_index < literals; ++literal_index)
        {
            byte bit = (byte)(1u << (int)(literal_index & 7));
            if ((context.enc_ItemType[literal_index >> 3] & bit) != 0)
            {
                ++distances;
            }
        }

        return distances;
    }

    internal static void do_block_output(t_encoder_context context, int literal_to_end_at, int distance_to_end_at)
    {
        uint block_size = get_block_stats(context, 0, 0, (uint)literal_to_end_at);
        int block_type = (int)get_aligned_stats(context, (uint)distance_to_end_at);

        create_trees(context, true);

        if (estimate_compressed_block_size(context) >= block_size &&
            context.enc_bufpos_at_last_block >= context.enc_earliest_window_data_remaining)
        {
            block_type = 3;
        }

        output_bits(context, 3, (uint)block_type);
        output_bits(context, 8, (byte)(block_size >> 16));
        output_bits(context, 8, (byte)(block_size >> 8));
        output_bits(context, 8, (byte)block_size);

        if (block_type == 1)
        {
            encode_trees(context);
            encode_verbatim_block(context, (uint)literal_to_end_at);
            get_final_repeated_offset_states(context, (uint)distance_to_end_at);
        }
        else if (block_type == 2)
        {
            encode_aligned_tree(context);
            encode_trees(context);
            encode_aligned_block(context, (uint)literal_to_end_at);
            get_final_repeated_offset_states(context, (uint)distance_to_end_at);
        }
        else if (block_type == 3)
        {
            get_final_repeated_offset_states(context, (uint)distance_to_end_at);
            encode_uncompressed_block(context, context.enc_bufpos_at_last_block, block_size);
        }

        context.enc_bufpos_at_last_block += block_size;
    }

    internal static void output_block(t_encoder_context context)
    {
        uint where_to_split = context.enc_literals;
        uint distances = context.enc_distances;

        context.enc_first_block = 0;
        split_block(context, 0, context.enc_literals, context.enc_distances, out where_to_split, out distances);

        uint block_size = get_block_stats(context, 0, 0, where_to_split);
        int block_type = (int)get_aligned_stats(context, distances);

        create_trees(context, true);
        if (estimate_compressed_block_size(context) >= block_size &&
            context.enc_bufpos_at_last_block >= context.enc_earliest_window_data_remaining)
        {
            block_type = 3;
        }

        output_bits(context, 3, (uint)block_type);
        output_bits(context, 8, (byte)(block_size >> 16));
        output_bits(context, 8, (byte)(block_size >> 8));
        output_bits(context, 8, (byte)block_size);

        if (block_type == 1)
        {
            encode_trees(context);
            encode_verbatim_block(context, where_to_split);
            get_final_repeated_offset_states(context, distances);
        }
        else if (block_type == 2)
        {
            encode_aligned_tree(context);
            encode_trees(context);
            encode_aligned_block(context, where_to_split);
            get_final_repeated_offset_states(context, distances);
        }
        else if (block_type == 3)
        {
            get_final_repeated_offset_states(context, distances);
            encode_uncompressed_block(context, context.enc_bufpos_at_last_block, block_size);
        }

        uint original_literals = context.enc_literals;
        context.enc_bufpos_at_last_block += block_size;

        if (where_to_split == original_literals)
        {
            context.enc_ItemType.AsSpan().Clear();
            context.enc_literals = 0;
            context.enc_distances = 0;
        }
        else
        {
            context.enc_ItemType.AsSpan((int)(where_to_split >> 3))
                .CopyTo(context.enc_ItemType);
            uint remaining_literals = original_literals - where_to_split;
            context.enc_ItemType.AsSpan((int)(remaining_literals >> 3) + 1).Clear();

            context.enc_LitData.AsSpan((int)where_to_split, (int)(original_literals - where_to_split))
                .CopyTo(context.enc_LitData);
            context.enc_DistData.AsSpan((int)distances, (int)(context.enc_distances - distances))
                .CopyTo(context.enc_DistData);

            context.enc_literals -= where_to_split;
            context.enc_distances -= distances;
        }

        fix_tree_cost_estimates(context);
    }

    internal static void flush_output_bit_buffer(t_encoder_context context)
    {
        int bitcount = context.enc_bitcount;
        if (bitcount < 32)
        {
            output_bits(context, bitcount - 16, 0);
        }
    }

    internal static int estimate_buffer_contents(t_encoder_context context)
    {
        create_trees(context, false);
        uint estimate = estimate_compressed_block_size(context);
        fix_tree_cost_estimates(context);
        return (int)estimate;
    }
}

