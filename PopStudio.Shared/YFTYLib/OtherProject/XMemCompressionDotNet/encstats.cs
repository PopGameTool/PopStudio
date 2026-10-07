#nullable enable
using System;
using System.Runtime.CompilerServices;

namespace XMemCompressionDotNet;

internal static partial class XCompress
{
    private static ReadOnlySpan<uint> square_table =>
    [
        0, 1, 4, 9, 16, 25, 36, 49, 64, 81, 100, 121, 144, 169, 196, 225, 256,
    ];

    private static ReadOnlySpan<byte> log2_table =>
    [
        0, 1, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 4, 4,
        5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
        6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
        6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
        7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
        7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
        7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
        7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
        8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
        8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
        8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
        8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
        8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
        8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
        8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
        8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
    ];

    internal static void tally_aligned_bits(t_encoder_context context, uint dist_to_end_at)
    {
        uint[] distData = context.enc_DistData;
        ushort[] alignedFreq = context.enc_aligned_tree_freq;
        uint distanceIndex = 0;
        while (dist_to_end_at != 0)
        {
            uint value = distData[distanceIndex++];
            if (value >= 0x10)
            {
                ++alignedFreq[value & 7];
            }

            --dist_to_end_at;
        }
    }

    internal static lzx_block_type get_aligned_stats(t_encoder_context context, uint dist_to_end_at)
    {
        ushort[] alignedFreq = context.enc_aligned_tree_freq;
        uint[] distData = context.enc_DistData;
        alignedFreq.AsSpan().Clear();

        uint total = 0;
        uint maximum = 0;
        uint distanceIndex = 0;

        for (uint i = 0; i < dist_to_end_at; ++i)
        {
            uint value = distData[distanceIndex++];
            if (value >= 0x10)
            {
                alignedFreq[value & 7] += 1;
            }
        }

        for (int i = 0; i < 8; ++i)
        {
            uint value = alignedFreq[i];
            if (value > maximum)
            {
                maximum = value;
            }

            total += value;
        }

        if (maximum > total / 5 && dist_to_end_at >= 0x64)
        {
            return lzx_block_type.BLOCKTYPE_ALIGNED;
        }

        return lzx_block_type.BLOCKTYPE_VERBATIM;
    }

    internal static uint tally_frequency(
        t_encoder_context context,
        uint literal_to_start_at,
        uint distance_to_start_at,
        uint literal_to_end_at)
    {
        uint total = 0;
        uint literal_index = literal_to_start_at;
        const byte increment = 1;
        byte[] item_type = context.enc_ItemType;
        byte[] lit_data = context.enc_LitData;
        uint[] dist_data = context.enc_DistData;
        ushort[] main_freq = context.enc_main_tree_freq;
        ushort[] secondary_freq = context.enc_secondary_tree_freq;
        byte[] slot_table = context.enc_slot_table;
        uint distance_index = distance_to_start_at;

        while (literal_index < literal_to_end_at)
        {
            byte bit = (byte)(increment << (int)(literal_index & 7));
            if ((item_type[literal_index >> 3] & bit) == 0)
            {
                main_freq[lit_data[literal_index]] += increment;
                total += increment;
            }
            else
            {
                uint distance = dist_data[distance_index];
                byte literal = lit_data[literal_index];
                if (literal < 7)
                {
                    uint slot = EncoderSlot(slot_table, distance);
                    main_freq[literal + 8 * slot + 0x100] += increment;
                }
                else
                {
                    uint slot = EncoderSlot(slot_table, distance);
                    main_freq[8 * slot + 0x107] += increment;
                    secondary_freq[literal - 7] += increment;
                }

                ++distance_index;
                total += (uint)(literal + 2);
            }

            ++literal_index;
        }

        return total;
    }

    internal static uint get_block_stats(
        t_encoder_context context,
        uint literal_to_start_at,
        uint distance_to_start_at,
        uint literal_to_end_at)
    {
        context.enc_main_tree_freq.AsSpan().Clear();
        context.enc_secondary_tree_freq.AsSpan().Clear();
        return tally_frequency(context, literal_to_start_at, distance_to_start_at, literal_to_end_at);
    }

    internal static uint update_cumulative_block_stats(
        t_encoder_context context,
        uint literal_to_start_at,
        uint distance_to_start_at,
        uint literal_to_end_at)
    {
        return tally_frequency(context, literal_to_start_at, distance_to_start_at, literal_to_end_at);
    }

    internal static uint return_difference(
        t_encoder_context context,
        uint item_start1,
        uint item_start2,
        uint dist_at_1,
        uint dist_at_2,
        uint size)
    {
        uint num_position_slots = context.enc_num_position_slots;
        uint num_symbols = 8 * num_position_slots + 0x100;
        if (num_symbols >= 0x320)
        {
            return 0;
        }

        Span<ushort> freq1 = stackalloc ushort[0x320];
        Span<ushort> freq2 = stackalloc ushort[0x320];
        freq1[..(int)num_symbols].Clear();
        freq2[..(int)num_symbols].Clear();

        uint difference = 0;
        if (size != 0)
        {
            byte[] item_type = context.enc_ItemType;
            byte[] lit_data = context.enc_LitData;
            uint[] dist_data = context.enc_DistData;
            byte[] slot_table = context.enc_slot_table;
            uint dist_index1 = dist_at_1;
            uint dist_index2 = dist_at_2;

            while (size != 0)
            {
                uint symbol1;
                byte bit1 = (byte)(1u << (int)(item_start1 & 7));
                if ((item_type[item_start1 >> 3] & bit1) == 0)
                {
                    symbol1 = lit_data[item_start1];
                }
                else
                {
                    uint distance = dist_data[dist_index1];
                    byte literal = lit_data[item_start1];
                    if (literal < 7)
                    {
                        uint slot = EncoderSlot(slot_table, distance);
                        symbol1 = literal + 8 * slot + 0x100;
                    }
                    else
                    {
                        uint slot = EncoderSlot(slot_table, distance);
                        symbol1 = 8 * slot + 0x107;
                    }

                    ++dist_index1;
                }

                ++freq1[(int)symbol1];
                ++item_start1;

                uint symbol2;
                byte bit2 = (byte)(1u << (int)(item_start2 & 7));
                if ((item_type[item_start2 >> 3] & bit2) == 0)
                {
                    symbol2 = lit_data[item_start2];
                }
                else
                {
                    uint distance = dist_data[dist_index2];
                    byte literal = lit_data[item_start2];
                    if (literal < 7)
                    {
                        uint slot = EncoderSlot(slot_table, distance);
                        symbol2 = literal + 8 * slot + 0x100;
                    }
                    else
                    {
                        uint slot = EncoderSlot(slot_table, distance);
                        symbol2 = 8 * slot + 0x107;
                    }

                    ++dist_index2;
                }

                ++item_start2;
                ++freq2[(int)symbol2];
                --size;
            }
        }

        for (uint i = 0; i < num_symbols; ++i)
        {
            ushort freqValue1 = freq1[(int)i];
            uint log1 = freqValue1 >= 0x100
                ? (uint)(log2_table[freqValue1 >> 8] + 8)
                : log2_table[(int)freqValue1];

            ushort freqValue2 = freq2[(int)i];
            uint log2 = freqValue2 >= 0x100
                ? (uint)(log2_table[freqValue2 >> 8] + 8)
                : log2_table[(int)freqValue2];

            int delta = (int)square_table[(int)log1] - (int)square_table[(int)log2];
            difference += (uint)(delta < 0 ? -delta : delta);
        }

        return difference;
    }

    internal static bool split_block(
        t_encoder_context context,
        uint literal_to_start_at,
        uint literal_to_end_at,
        uint distance_to_end_at,
        out uint split_at_literal,
        out uint split_at_distance)
    {
        Span<ushort> num_dist_at_item = stackalloc ushort[1032];
        num_dist_at_item.Clear();

        split_at_literal = literal_to_end_at;
        split_at_distance = distance_to_end_at;

        if (literal_to_end_at - literal_to_start_at < 0x1800)
        {
            return false;
        }

        byte num_block_splits = context.enc_num_block_splits;
        if (num_block_splits >= 4)
        {
            return false;
        }

        uint distance_count = 0;
        uint item_type_index = 0;
        if ((literal_to_end_at >> 3) != 0)
        {
            byte[] item_type = context.enc_ItemType;
            int distance_table_index = 0;
            do
            {
                if ((item_type_index & 7) == 0)
                {
                    num_dist_at_item[distance_table_index++] = (ushort)distance_count;
                }

                distance_count += context.enc_ones[item_type[item_type_index]];
                ++item_type_index;
            } while (item_type_index < (literal_to_end_at >> 3));
        }

        uint first_candidate = (literal_to_start_at + 0x3Fu) & 0xFFFFFFC0u;
        uint last_center = literal_to_end_at - 0x1000u;
        uint left = first_candidate + 0x800u;
        if (left >= last_center)
        {
            return false;
        }

        for (uint right = left + 0x800u; ; right += 0x400u)
        {
            if (return_difference(
                    context,
                    left,
                    right - 0x400u,
                    num_dist_at_item[(int)(left >> 6)],
                    num_dist_at_item[(int)((right - 0x400u) >> 6)],
                    0x400u) > 0x578u &&
                return_difference(
                    context,
                    left - 0x400u,
                    right,
                    num_dist_at_item[(int)((left - 0x400u) >> 6)],
                    num_dist_at_item[(int)(right >> 6)],
                    0x400u) > 0x578u &&
                return_difference(
                    context,
                    left - 0x800u,
                    right + 0x400u,
                    num_dist_at_item[(int)((left - 0x800u) >> 6)],
                    num_dist_at_item[(int)((right + 0x400u) >> 6)],
                    0x400u) > 0x578u)
            {
                uint best_literal = 0;
                uint best_score = 0;
                uint candidate = right - 0x600u;
                if (candidate < right + 0x200u)
                {
                    do
                    {
                        uint score = return_difference(
                            context,
                            candidate - 0x400u,
                            candidate,
                            num_dist_at_item[(int)((candidate - 0x400u) >> 6)],
                            num_dist_at_item[(int)(candidate >> 6)],
                            0x400u);

                        if (score > best_score)
                        {
                            best_score = score;
                            best_literal = candidate;
                        }

                        candidate += 0x40u;
                    } while (candidate < right + 0x200u);

                    if (best_score >= 0x6A4u && (best_literal - first_candidate) >= 0x1000u)
                    {
                        context.enc_num_block_splits = (byte)(num_block_splits + 1);
                        split_at_literal = best_literal;
                        split_at_distance = num_dist_at_item[(int)(best_literal >> 6)];
                        return true;
                    }
                }
            }

            left += 0x400u;
            if (left >= last_center)
            {
                return false;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint EncoderSlot(t_encoder_context context, uint distance)
    {
        return EncoderSlot(context.enc_slot_table, distance);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint EncoderSlot(byte[] slotTable, uint distance)
    {
        if (distance < 0x400)
        {
            return slotTable[distance];
        }

        if (distance < 0x80000)
        {
            return (uint)(slotTable[distance >> 9] + 0x12);
        }

        return (byte)(distance >> 17) + 0x22u;
    }
}
