#nullable enable
using System;

namespace XMemCompressionDotNet;

internal static partial class XCompress
{
    internal static bool ReadRepTree(t_decoder_context context, int num_elements, Span<byte> lastlen, Span<byte> len)
    {
        Byte32 small_bitlenInline = default;
        Short96 leftrightInline = default;
        Short256 small_tableInline = default;
        byte[] input_buffer = context.dec_input;
        Span<byte> small_bitlen = small_bitlenInline;
        Span<short> leftright_s = leftrightInline;
        Span<short> small_table = small_tableInline;

        for (int i = 0; i < 20; ++i)
        {
            small_bitlen[i] = (byte)getbits(context, 4);
        }

        if (context.dec_error_condition)
        {
            return false;
        }

        make_table(context, 20, small_bitlen, 8, small_table, leftright_s);

        int input_curpos = context.dec_input_curpos;
        int end_input_pos = context.dec_end_input_pos;
        uint bitbuf = context.dec_bitbuf;
        int bitcount = context.dec_bitcount;
        bool error_condition = context.dec_error_condition;

        int element = 0;
        while (element < num_elements && !error_condition)
        {
            int symbol = small_table[(int)(bitbuf >> 24)];
            if (symbol < 0)
            {
                uint mask = 0x800000;
                do
                {
                    int index = -symbol;
                    symbol = (bitbuf & mask) != 0
                        ? leftright_s[2 * index + 1]
                        : leftright_s[2 * index];
                    mask >>= 1;
                }
                while (symbol < 0);
            }

            uint symbol_len = small_bitlen[symbol];
            bitcount -= (int)symbol_len;
            bitbuf <<= (int)symbol_len;
            if (bitcount <= 0)
            {
                if (input_curpos >= end_input_pos)
                {
                    error_condition = true;
                }
                else
                {
                    uint next =
                        (uint)((uint)input_curpos < (uint)input_buffer.Length ? input_buffer[input_curpos] : 0) |
                        ((uint)((uint)(input_curpos + 1) < (uint)input_buffer.Length ? input_buffer[input_curpos + 1] : 0) << 8);
                    input_curpos += 2;
                    bitbuf |= next << -bitcount;
                    bitcount += 16;
                    if ((bitcount <= 0) && (input_curpos < end_input_pos))
                    {
                        uint more =
                            (uint)((uint)input_curpos < (uint)input_buffer.Length ? input_buffer[input_curpos] : 0) |
                            ((uint)((uint)(input_curpos + 1) < (uint)input_buffer.Length ? input_buffer[input_curpos + 1] : 0) << 8);
                        input_curpos += 2;
                        bitbuf |= more << -bitcount;
                        bitcount += 16;
                    }
                    else if (bitcount <= 0)
                    {
                        error_condition = true;
                    }
                }
            }

            if (error_condition)
            {
                break;
            }

            if (symbol == 17)
            {
                uint run_length = (bitbuf >> 28) + 4;
                bitcount -= 4;
                bitbuf <<= 4;
                if (bitcount <= 0)
                {
                    if (input_curpos >= end_input_pos)
                    {
                        error_condition = true;
                    }
                    else
                    {
                        uint next =
                            (uint)((uint)input_curpos < (uint)input_buffer.Length ? input_buffer[input_curpos] : 0) |
                            ((uint)((uint)(input_curpos + 1) < (uint)input_buffer.Length ? input_buffer[input_curpos + 1] : 0) << 8);
                        input_curpos += 2;
                        bitbuf |= next << -bitcount;
                        bitcount += 16;
                        if ((bitcount <= 0) && (input_curpos < end_input_pos))
                        {
                            uint more =
                                (uint)((uint)input_curpos < (uint)input_buffer.Length ? input_buffer[input_curpos] : 0) |
                                ((uint)((uint)(input_curpos + 1) < (uint)input_buffer.Length ? input_buffer[input_curpos + 1] : 0) << 8);
                            input_curpos += 2;
                            bitbuf |= more << -bitcount;
                            bitcount += 16;
                        }
                        else if (bitcount <= 0)
                        {
                            error_condition = true;
                        }
                    }
                }
                if (element + (int)run_length > num_elements)
                {
                    run_length = (uint)(num_elements - element);
                }

                if (run_length != 0)
                {
                    len.Slice(element, (int)run_length).Clear();
                    element += (int)run_length;
                }

                --element;
            }
            else if (symbol == 18)
            {
                uint run_length = (bitbuf >> 27) + 20;
                bitcount -= 5;
                bitbuf <<= 5;
                if (bitcount <= 0)
                {
                    if (input_curpos >= end_input_pos)
                    {
                        error_condition = true;
                    }
                    else
                    {
                        uint next =
                            (uint)((uint)input_curpos < (uint)input_buffer.Length ? input_buffer[input_curpos] : 0) |
                            ((uint)((uint)(input_curpos + 1) < (uint)input_buffer.Length ? input_buffer[input_curpos + 1] : 0) << 8);
                        input_curpos += 2;
                        bitbuf |= next << -bitcount;
                        bitcount += 16;
                        if ((bitcount <= 0) && (input_curpos < end_input_pos))
                        {
                            uint more =
                                (uint)((uint)input_curpos < (uint)input_buffer.Length ? input_buffer[input_curpos] : 0) |
                                ((uint)((uint)(input_curpos + 1) < (uint)input_buffer.Length ? input_buffer[input_curpos + 1] : 0) << 8);
                            input_curpos += 2;
                            bitbuf |= more << -bitcount;
                            bitcount += 16;
                        }
                        else if (bitcount <= 0)
                        {
                            error_condition = true;
                        }
                    }
                }
                if (element + (int)run_length > num_elements)
                {
                    run_length = (uint)(num_elements - element);
                }

                if (run_length != 0)
                {
                    len.Slice(element, (int)run_length).Clear();
                    element += (int)run_length;
                }

                --element;
            }
            else if (symbol == 19)
            {
                uint run_length = (bitbuf >> 31) + 4;
                bitcount -= 1;
                bitbuf <<= 1;
                if (bitcount <= 0)
                {
                    if (input_curpos >= end_input_pos)
                    {
                        error_condition = true;
                    }
                    else
                    {
                        uint next =
                            (uint)((uint)input_curpos < (uint)input_buffer.Length ? input_buffer[input_curpos] : 0) |
                            ((uint)((uint)(input_curpos + 1) < (uint)input_buffer.Length ? input_buffer[input_curpos + 1] : 0) << 8);
                        input_curpos += 2;
                        bitbuf |= next << -bitcount;
                        bitcount += 16;
                        if ((bitcount <= 0) && (input_curpos < end_input_pos))
                        {
                            uint more =
                                (uint)((uint)input_curpos < (uint)input_buffer.Length ? input_buffer[input_curpos] : 0) |
                                ((uint)((uint)(input_curpos + 1) < (uint)input_buffer.Length ? input_buffer[input_curpos + 1] : 0) << 8);
                            input_curpos += 2;
                            bitbuf |= more << -bitcount;
                            bitcount += 16;
                        }
                        else if (bitcount <= 0)
                        {
                            error_condition = true;
                        }
                    }
                }

                int rep_symbol = small_table[(int)(bitbuf >> 24)];
                if (rep_symbol < 0)
                {
                    uint mask = 0x800000;
                    do
                    {
                        int index = -rep_symbol;
                        rep_symbol = (bitbuf & mask) != 0
                            ? leftright_s[2 * index + 1]
                            : leftright_s[2 * index];
                        mask >>= 1;
                    }
                    while (rep_symbol < 0);
                }

                uint rep_len = small_bitlen[rep_symbol];
                bitcount -= (int)rep_len;
                bitbuf <<= (int)rep_len;
                if (bitcount <= 0)
                {
                    if (input_curpos >= end_input_pos)
                    {
                        error_condition = true;
                    }
                    else
                    {
                        uint next =
                            (uint)((uint)input_curpos < (uint)input_buffer.Length ? input_buffer[input_curpos] : 0) |
                            ((uint)((uint)(input_curpos + 1) < (uint)input_buffer.Length ? input_buffer[input_curpos + 1] : 0) << 8);
                        input_curpos += 2;
                        bitbuf |= next << -bitcount;
                        bitcount += 16;
                        if ((bitcount <= 0) && (input_curpos < end_input_pos))
                        {
                            uint more =
                                (uint)((uint)input_curpos < (uint)input_buffer.Length ? input_buffer[input_curpos] : 0) |
                                ((uint)((uint)(input_curpos + 1) < (uint)input_buffer.Length ? input_buffer[input_curpos + 1] : 0) << 8);
                            input_curpos += 2;
                            bitbuf |= more << -bitcount;
                            bitcount += 16;
                        }
                        else if (bitcount <= 0)
                        {
                            error_condition = true;
                        }
                    }
                }

                if (element + (int)run_length > num_elements)
                {
                    run_length = (uint)(num_elements - element);
                }

                int value = lastlen[element] - rep_symbol + 17;
                if (value >= 17)
                {
                    value -= 17;
                }

                if (run_length != 0)
                {
                    len.Slice(element, (int)run_length).Fill((byte)value);
                    element += (int)run_length;
                }

                --element;
            }
            else
            {
                int value = lastlen[element] - symbol + 17;
                if (value >= 17)
                {
                    value -= 17;
                }

                len[element] = (byte)value;
            }

            ++element;
        }

        context.dec_input_curpos = input_curpos;
        context.dec_bitbuf = bitbuf;
        context.dec_bitcount = bitcount;
        context.dec_error_condition = error_condition;
        return !error_condition;
    }

    internal static bool read_main_and_secondary_trees(t_decoder_context context)
    {
        if (!ReadRepTree(context, 0x100, context.dec_main_tree_prev_len, context.dec_main_tree_len))
        {
            return false;
        }

        if (!ReadRepTree(
                context,
                8 * context.dec_num_position_slots,
                context.dec_main_tree_prev_len.AsSpan(0x100),
                context.dec_main_tree_len.AsSpan(0x100)))
        {
            return false;
        }

        if (!make_table(
                context,
                8 * context.dec_num_position_slots + 0x100,
                context.dec_main_tree_len,
                10,
                context.dec_main_tree_table,
                context.dec_main_tree_left_right))
        {
            return false;
        }

        if (!ReadRepTree(
                context,
                0xF9,
                context.dec_secondary_length_tree_prev_len,
                context.dec_secondary_length_tree_len))
        {
            return false;
        }

        return make_table(
            context,
            0xF9,
            context.dec_secondary_length_tree_len,
            8,
            context.dec_secondary_length_tree_table,
            context.dec_secondary_length_tree_left_right);
    }

    internal static bool read_aligned_offset_tree(t_decoder_context context)
    {
        for (int i = 0; i < 8; ++i)
        {
            context.dec_aligned_len[i] = (byte)getbits(context, 3);
        }

        if (context.dec_error_condition)
        {
            return false;
        }

        return make_table_8bit(context, context.dec_aligned_len, context.dec_aligned_table);
    }
}
