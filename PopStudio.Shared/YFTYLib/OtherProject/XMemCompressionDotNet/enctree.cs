#nullable enable
using System;

namespace XMemCompressionDotNet;

internal static partial class XCompress
{
    private static ReadOnlySpan<byte> Modulo17Lookup =>
    [
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16,
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16,
        0,
    ];

    internal static void WriteRepTree(t_encoder_context context, Span<byte> pLen, Span<byte> pLastLen, int Num)
    {
        Span<ushort> smallFreq = stackalloc ushort[48];
        Span<byte> miniLen = stackalloc byte[24];
        Span<ushort> miniCode = stackalloc ushort[24];

        int count = Num;
        byte saved_length = pLen[count];
        pLen[count] = 123;

        for (int i = 0; i < count; ++i)
        {
            int repeat_count = 0;
            byte current = pLen[i];
            if (pLen[i + 1] == current)
            {
                int run = i + 1;
                do
                {
                    ++run;
                    ++repeat_count;
                }
                while (pLen[run] == current);

                if (repeat_count >= 4)
                {
                    if (current == 0)
                    {
                        if (repeat_count > 0x33)
                        {
                            repeat_count = 0x33;
                        }

                        if (repeat_count > 0x13)
                        {
                            ++smallFreq[18];
                        }
                        else
                        {
                            ++smallFreq[17];
                        }
                    }
                    else
                    {
                        if (repeat_count > 5)
                        {
                            repeat_count = 5;
                        }

                        ++smallFreq[Modulo17Lookup[pLastLen[i] - current + 17]];
                        ++smallFreq[19];
                    }

                    i += repeat_count - 1;
                    continue;
                }
            }

            ++smallFreq[Modulo17Lookup[pLastLen[i] - current + 17]];
        }

        make_tree(context, 20, smallFreq, miniLen, miniCode, true);
        for (int i = 0; i < 20; ++i)
        {
            output_bits(context, 4, miniLen[i]);
        }

        for (int i = 0; i < count; ++i)
        {
            int repeat_count = 0;
            byte current = pLen[i];
            byte symbol;

            if (pLen[i + 1] == current)
            {
                int run = i + 1;
                do
                {
                    ++run;
                    ++repeat_count;
                }
                while (pLen[run] == current);

                if (repeat_count >= 4)
                {
                    if (current == 0)
                    {
                        if (repeat_count > 0x33)
                        {
                            repeat_count = 0x33;
                        }

                        symbol = (byte)((repeat_count > 0x13 ? 1 : 0) + 17);
                    }
                    else
                    {
                        if (repeat_count > 5)
                        {
                            repeat_count = 5;
                        }

                        symbol = 19;
                    }
                }
                else
                {
                    symbol = Modulo17Lookup[pLastLen[i] - current + 17];
                }
            }
            else
            {
                symbol = Modulo17Lookup[pLastLen[i] - current + 17];
            }

            output_bits(context, miniLen[symbol], miniCode[symbol]);

            if (symbol == 17)
            {
                output_bits(context, 4, (uint)(repeat_count - 4));
                i += repeat_count - 1;
            }
            else if (symbol == 18)
            {
                output_bits(context, 5, (uint)(repeat_count - 20));
                i += repeat_count - 1;
            }
            else if (symbol == 19)
            {
                output_bits(context, 1, (uint)(repeat_count - 4));

                byte delta_symbol = Modulo17Lookup[pLastLen[i] - current + 17];
                output_bits(context, miniLen[delta_symbol], miniCode[delta_symbol]);

                i += repeat_count - 1;
            }
        }

        pLen[count] = saved_length;
        pLen.Slice(0, count).CopyTo(pLastLen);
    }

    internal static void create_trees(t_encoder_context context, bool generate_codes)
    {
        make_tree(
            context,
            (int)(8 * context.enc_num_position_slots + 0x100),
            context.enc_main_tree_freq,
            context.enc_main_tree_len,
            context.enc_main_tree_code,
            generate_codes);

        make_tree(
            context,
            0xF9,
            context.enc_secondary_tree_freq,
            context.enc_secondary_tree_len,
            context.enc_secondary_tree_code,
            generate_codes);

        make_tree(
            context,
            8,
            context.enc_aligned_tree_freq,
            context.enc_aligned_tree_len,
            context.enc_aligned_tree_code,
            true);
    }

    internal static void prevent_far_matches(t_encoder_context context)
    {
        uint slot = (uint)(context.enc_slot_table[0x100] + 0x12);
        if (slot < context.enc_num_position_slots)
        {
            int treeIndex = 0x100 + 8 * (int)slot;
            do
            {
                context.enc_main_tree_len[treeIndex] = 100;
                ++slot;
                treeIndex += 8;
            }
            while (slot < context.enc_num_position_slots);
        }
    }

    internal static void encode_trees(t_encoder_context context)
    {
        WriteRepTree(context, context.enc_main_tree_len, context.enc_main_tree_prev_len, 0x100);
        WriteRepTree(
            context,
            context.enc_main_tree_len.AsSpan(0x100),
            context.enc_main_tree_prev_len.AsSpan(0x100),
            (int)(context.enc_num_position_slots << 3));
        WriteRepTree(context, context.enc_secondary_tree_len, context.enc_secondary_tree_prev_len, 0xF9);
    }

    internal static void encode_aligned_tree(t_encoder_context context)
    {
        make_tree(
            context,
            8,
            context.enc_aligned_tree_freq,
            context.enc_aligned_tree_len,
            context.enc_aligned_tree_code,
            true);

        for (uint i = 0; i < 8; ++i)
        {
            output_bits(context, 3, context.enc_aligned_tree_len[i]);
        }
    }

    internal static void fix_tree_cost_estimates(t_encoder_context context)
    {
        for (uint i = 0; i < 0x100; ++i)
        {
            if (context.enc_main_tree_len[i] == 0)
            {
                context.enc_main_tree_len[i] = 11;
            }
        }

        uint main_tree_count = 8 * context.enc_num_position_slots + 0x100;
        for (uint i = 0x100; i < main_tree_count; ++i)
        {
            if (context.enc_main_tree_len[i] == 0)
            {
                context.enc_main_tree_len[i] = 12;
            }
        }

        for (uint i = 0; i < 0xF9; ++i)
        {
            if (context.enc_secondary_tree_len[i] == 0)
            {
                context.enc_secondary_tree_len[i] = 8;
            }
        }

        prevent_far_matches(context);
    }
}

