#nullable enable
namespace XMemCompressionDotNet;

internal static partial class XCompress
{
    internal static int special_decode_aligned_block(t_decoder_context context, int bufpos, int amount_to_decode)
    {
        int bitcount = context.dec_bitcount;
        uint bitbuf = context.dec_bitbuf;
        int input_curpos = context.dec_input_curpos;
        byte[] mem_window = context.dec_mem_window;
        int write_pos = bufpos;
        int window_pos = bufpos;
        int end_pos = bufpos + amount_to_decode;

        while (window_pos < end_pos)
        {
            int symbol = context.dec_main_tree_table[bitbuf >> 22];
            if (symbol < 0)
            {
                uint mask = 0x200000;
                do
                {
                    int index = -symbol;
                    if ((bitbuf & mask) != 0)
                    {
                        symbol = context.dec_main_tree_left_right[2 * index + 1];
                    }
                    else
                    {
                        symbol = context.dec_main_tree_left_right[2 * index];
                    }

                    mask >>= 1;
                }
                while (symbol < 0);
            }

            if (input_curpos >= context.dec_end_input_pos)
            {
                return -1;
            }

            uint main_len = context.dec_main_tree_len[symbol];
            bitcount -= (int)main_len;
            bitbuf <<= (int)main_len;
            if (bitcount <= 0)
            {
                uint next = (uint)context.dec_input[input_curpos] |
                            ((uint)context.dec_input[input_curpos + 1] << 8);
                input_curpos += 2;
                bitbuf |= next << -bitcount;
                bitcount += 16;
            }

            symbol -= 0x100;
            if (symbol < 0)
            {
                byte value = unchecked((byte)symbol);
                mem_window[window_pos] = value;
                mem_window[write_pos + (int)context.dec_window_size] = value;
                ++write_pos;
                ++window_pos;
                continue;
            }

            int match_length = symbol & 7;
            if (match_length == 7)
            {
                int length_symbol = context.dec_secondary_length_tree_table[bitbuf >> 24];
                if (length_symbol < 0)
                {
                    uint mask = 0x800000;
                    do
                    {
                        int index = -length_symbol;
                        if ((bitbuf & mask) != 0)
                        {
                            length_symbol = context.dec_secondary_length_tree_left_right[2 * index + 1];
                        }
                        else
                        {
                            length_symbol = context.dec_secondary_length_tree_left_right[2 * index];
                        }

                        mask >>= 1;
                    }
                    while (length_symbol < 0);
                }

                uint secondary_len = context.dec_secondary_length_tree_len[length_symbol];
                bitcount -= (int)secondary_len;
                bitbuf <<= (int)secondary_len;
                if (bitcount <= 0)
                {
                    uint next = (uint)context.dec_input[input_curpos] |
                                ((uint)context.dec_input[input_curpos + 1] << 8);
                    input_curpos += 2;
                    bitbuf |= next << -bitcount;
                    bitcount += 16;
                }

                match_length = length_symbol + 7;
            }

            int offset_slot = symbol >> 3;
            uint match_offset;
            if (offset_slot <= 2)
            {
                match_offset = context.dec_last_matchpos_offset[offset_slot];
                context.dec_last_matchpos_offset[offset_slot] = context.dec_last_matchpos_offset[0];
            }
            else
            {
                uint extra_bits = context.dec_extra_bits_table[(byte)offset_slot];
                if (extra_bits < 3)
                {
                    if (extra_bits != 0)
                    {
                        bitcount -= (int)extra_bits;
                        uint extra = bitbuf >> (int)(32 - extra_bits);
                        bitbuf <<= (int)extra_bits;
                        if (bitcount <= 0)
                        {
                            uint next = (uint)context.dec_input[input_curpos] |
                                        ((uint)context.dec_input[input_curpos + 1] << 8);
                            input_curpos += 2;
                            bitbuf |= next << -bitcount;
                            bitcount += 16;
                        }

                        match_offset = (uint)context.MP_POS_minus2_table[(byte)offset_slot] + extra;
                    }
                    else
                    {
                        match_offset = 1;
                    }
                }
                else
                {
                    uint extra = 0;
                    if (extra_bits != 3)
                    {
                        extra = bitbuf >> (int)(35 - extra_bits);
                        bitcount += 3 - (int)extra_bits;
                        bitbuf <<= (int)(extra_bits - 3);
                        if (bitcount <= 0)
                        {
                            uint next = (uint)context.dec_input[input_curpos] |
                                        ((uint)context.dec_input[input_curpos + 1] << 8);
                            input_curpos += 2;
                            bitbuf |= next << -bitcount;
                            bitcount += 16;
                        }
                    }

                    uint match_base = (uint)context.MP_POS_minus2_table[(byte)offset_slot] + 8 * extra;
                    uint aligned_symbol = context.dec_aligned_table[bitbuf >> 25];
                    uint aligned_len = context.dec_aligned_len[(int)aligned_symbol];
                    bitcount -= (int)aligned_len;
                    bitbuf <<= (int)aligned_len;
                    if (bitcount <= 0)
                    {
                        uint next = (uint)context.dec_input[input_curpos] |
                                    ((uint)context.dec_input[input_curpos + 1] << 8);
                        input_curpos += 2;
                        bitbuf |= next << -bitcount;
                        bitcount += 16;
                    }

                    match_offset = match_base + aligned_symbol;
                }

                context.dec_last_matchpos_offset[2] = context.dec_last_matchpos_offset[1];
                context.dec_last_matchpos_offset[1] = context.dec_last_matchpos_offset[0];
            }

            uint window_mask = context.dec_window_mask;
            context.dec_last_matchpos_offset[0] = match_offset;

            int remaining = match_length + 2;
            do
            {
                byte value = mem_window[window_mask & (uint)(write_pos - (int)match_offset)];
                mem_window[window_pos] = value;
                if (window_pos < 0x101)
                {
                    mem_window[write_pos + (int)context.dec_window_size] = value;
                }

                --remaining;
                ++write_pos;
                ++window_pos;
            }
            while (remaining > 0);
        }

        context.dec_bitcount = bitcount;
        context.dec_bitbuf = bitbuf;
        context.dec_input_curpos = input_curpos;
        return write_pos;
    }

    internal static int fast_decode_aligned_offset_block(t_decoder_context context, int bufpos, int amount_to_decode)
    {
        int bitcount = context.dec_bitcount;
        uint bitbuf = context.dec_bitbuf;
        int input_curpos = context.dec_input_curpos;
        byte[] mem_window = context.dec_mem_window;
        int write_pos = bufpos;
        int end_pos = bufpos + amount_to_decode;
        int window_pos = bufpos;

        while (window_pos < end_pos)
        {
            int symbol = context.dec_main_tree_table[bitbuf >> 22];
            if (symbol < 0)
            {
                uint mask = 0x200000;
                do
                {
                    int index = -symbol;
                    if ((bitbuf & mask) != 0)
                    {
                        symbol = context.dec_main_tree_left_right[2 * index + 1];
                    }
                    else
                    {
                        symbol = context.dec_main_tree_left_right[2 * index];
                    }

                    mask >>= 1;
                }
                while (symbol < 0);
            }

            if (input_curpos >= context.dec_end_input_pos)
            {
                return -1;
            }

            uint main_len = context.dec_main_tree_len[symbol];
            bitcount -= (int)main_len;
            bitbuf <<= (int)main_len;
            if (bitcount <= 0)
            {
                uint next = (uint)context.dec_input[input_curpos] |
                            ((uint)context.dec_input[input_curpos + 1] << 8);
                input_curpos += 2;
                bitbuf |= next << -bitcount;
                bitcount += 16;
            }

            symbol -= 0x100;
            if (symbol < 0)
            {
                mem_window[window_pos] = unchecked((byte)symbol);
                ++write_pos;
                ++window_pos;
                continue;
            }

            int match_length = symbol & 7;
            if (match_length == 7)
            {
                int length_symbol = context.dec_secondary_length_tree_table[bitbuf >> 24];
                if (length_symbol < 0)
                {
                    uint mask = 0x800000;
                    do
                    {
                        int index = -length_symbol;
                        if ((bitbuf & mask) != 0)
                        {
                            length_symbol = context.dec_secondary_length_tree_left_right[2 * index + 1];
                        }
                        else
                        {
                            length_symbol = context.dec_secondary_length_tree_left_right[2 * index];
                        }

                        mask >>= 1;
                    }
                    while (length_symbol < 0);
                }

                uint secondary_len = context.dec_secondary_length_tree_len[length_symbol];
                bitcount -= (int)secondary_len;
                bitbuf <<= (int)secondary_len;
                if (bitcount <= 0)
                {
                    uint next = (uint)context.dec_input[input_curpos] |
                                ((uint)context.dec_input[input_curpos + 1] << 8);
                    input_curpos += 2;
                    bitbuf |= next << -bitcount;
                    bitcount += 16;
                }

                match_length = length_symbol + 7;
            }

            int offset_slot = symbol >> 3;
            uint match_offset;
            if (offset_slot <= 2)
            {
                match_offset = context.dec_last_matchpos_offset[offset_slot];
                context.dec_last_matchpos_offset[offset_slot] = context.dec_last_matchpos_offset[0];
            }
            else
            {
                uint extra_bits = context.dec_extra_bits_table[(byte)offset_slot];
                if (extra_bits < 3)
                {
                    if (extra_bits != 0)
                    {
                        bitcount -= (int)extra_bits;
                        uint extra = bitbuf >> (int)(32 - extra_bits);
                        bitbuf <<= (int)extra_bits;
                        if (bitcount <= 0)
                        {
                            uint next = (uint)context.dec_input[input_curpos] |
                                        ((uint)context.dec_input[input_curpos + 1] << 8);
                            input_curpos += 2;
                            bitbuf |= next << -bitcount;
                            bitcount += 16;
                        }

                        match_offset = (uint)context.MP_POS_minus2_table[(byte)offset_slot] + extra;
                    }
                    else
                    {
                        match_offset = (uint)context.MP_POS_minus2_table[(byte)offset_slot];
                    }
                }
                else
                {
                    uint extra = 0;
                    if (extra_bits != 3)
                    {
                        extra = bitbuf >> (int)(35 - extra_bits);
                        bitcount += 3 - (int)extra_bits;
                        bitbuf <<= (int)(extra_bits - 3);
                        if (bitcount <= 0)
                        {
                            uint next = (uint)context.dec_input[input_curpos] |
                                        ((uint)context.dec_input[input_curpos + 1] << 8);
                            input_curpos += 2;
                            bitbuf |= next << -bitcount;
                            bitcount += 16;
                        }
                    }

                    uint match_base = (uint)context.MP_POS_minus2_table[(byte)offset_slot] + 8 * extra;
                    uint aligned_symbol = context.dec_aligned_table[bitbuf >> 25];
                    uint aligned_len = context.dec_aligned_len[(int)aligned_symbol];
                    bitcount -= (int)aligned_len;
                    bitbuf <<= (int)aligned_len;
                    if (bitcount <= 0)
                    {
                        uint next = (uint)context.dec_input[input_curpos] |
                                    ((uint)context.dec_input[input_curpos + 1] << 8);
                        input_curpos += 2;
                        bitbuf |= next << -bitcount;
                        bitcount += 16;
                    }

                    match_offset = match_base + aligned_symbol;
                }

                context.dec_last_matchpos_offset[2] = context.dec_last_matchpos_offset[1];
                context.dec_last_matchpos_offset[1] = context.dec_last_matchpos_offset[0];
            }

            int remaining = match_length + 2;
            context.dec_last_matchpos_offset[0] = match_offset;
            int source = (int)(context.dec_window_mask & (uint)(write_pos - (int)match_offset));
            do
            {
                --remaining;
                ++write_pos;
                mem_window[window_pos++] = mem_window[source++];
            }
            while (remaining > 0);
        }

        context.dec_bitcount = bitcount;
        context.dec_bitbuf = bitbuf;
        context.dec_input_curpos = input_curpos;
        context.dec_bufpos = write_pos & (int)context.dec_window_mask;
        return write_pos - end_pos;
    }

    internal static int decode_aligned_offset_block(t_decoder_context context, int bufpos, int amount_to_decode)
    {
        int current_bufpos = bufpos;
        int remaining = amount_to_decode;

        if (current_bufpos < 0x101)
        {
            int special_amount = 0x101 - current_bufpos;
            if (special_amount > remaining)
            {
                special_amount = remaining;
            }

            int new_bufpos = special_decode_aligned_block(context, current_bufpos, special_amount);
            remaining += current_bufpos - new_bufpos;
            context.dec_bufpos = new_bufpos;
            current_bufpos = new_bufpos;

            if (remaining <= 0)
            {
                return remaining;
            }
        }

        return fast_decode_aligned_offset_block(context, current_bufpos, remaining);
    }
}
