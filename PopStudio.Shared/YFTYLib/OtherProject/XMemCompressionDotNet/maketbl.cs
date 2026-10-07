#nullable enable
using System;

namespace XMemCompressionDotNet;

internal static partial class XCompress
{
    internal static bool make_table(
        t_decoder_context context,
        int num_elements,
        ReadOnlySpan<byte> len,
        byte nbits,
        Span<short> table,
        Span<short> left_right)
    {
        _ = context;
        Span<uint> count = stackalloc uint[17];
        Span<uint> start = stackalloc uint[18];

        for (int i = 0; i < num_elements; ++i)
        {
            ++count[len[i]];
        }

        start[1] = 0;
        for (int i = 1; i <= 16; ++i)
        {
            start[i + 1] = start[i] + (count[i] << (16 - i));
        }

        if (start[17] != 0x10000)
        {
            if (start[17] != 0)
            {
                return false;
            }

            table.Slice(0, 1 << nbits).Clear();
            return true;
        }

        uint tablebits = nbits;
        int shift = (int)(16 - tablebits);

        for (int i = 1; i <= tablebits; ++i)
        {
            start[i] >>= shift;
            count[i] = 1u << ((int)tablebits - i);
        }

        for (int i = (int)tablebits + 1; i <= 16; ++i)
        {
            count[i] = 1u << (16 - i);
        }

        uint start_index = start[(int)tablebits + 1] >> shift;
        if (start_index != 0x10000)
        {
            table.Slice((int)start_index, (1 << (int)tablebits) - (int)start_index).Clear();
        }

        uint next_node = (uint)num_elements;
        for (int symbol = 0; symbol < num_elements; ++symbol)
        {
            uint bitlen = len[symbol];
            if (bitlen == 0)
            {
                continue;
            }

            uint position = start[(int)bitlen];
            uint next = position + count[(int)bitlen];
            if (bitlen <= tablebits)
            {
                if (next > (1u << (int)tablebits))
                {
                    return false;
                }

                for (uint index = position; index < next; ++index)
                {
                    table[(int)index] = (short)symbol;
                }

                start[(int)bitlen] = next;
                continue;
            }

            start[(int)bitlen] = next;

            int nodeTableIndex = (int)(position >> shift);
            bool nodeInTable = true;
            uint code = position << (int)tablebits;
            uint remaining = bitlen - tablebits;

            while (remaining != 0)
            {
                short nodeValue = nodeInTable ? table[nodeTableIndex] : left_right[nodeTableIndex];
                if (nodeValue == 0)
                {
                    left_right[(int)(2 * next_node)] = 0;
                    left_right[(int)(2 * next_node + 1)] = 0;
                    nodeValue = (short)-(short)next_node;
                    if (nodeInTable)
                    {
                        table[nodeTableIndex] = nodeValue;
                    }
                    else
                    {
                        left_right[nodeTableIndex] = nodeValue;
                    }

                    ++next_node;
                }

                if ((code & 0x8000u) == 0)
                {
                    nodeTableIndex = -2 * nodeValue;
                }
                else
                {
                    nodeTableIndex = 1 - 2 * nodeValue;
                }

                nodeInTable = false;
                code <<= 1;
                --remaining;
            }

            if (nodeInTable)
            {
                table[nodeTableIndex] = (short)symbol;
            }
            else
            {
                left_right[nodeTableIndex] = (short)symbol;
            }
        }

        return true;
    }

    internal static bool make_table_8bit(t_decoder_context context, ReadOnlySpan<byte> len, Span<byte> table)
    {
        _ = context;
        Span<ushort> count = stackalloc ushort[17];
        Span<ushort> start = stackalloc ushort[18];

        for (int i = 0; i < 8; ++i)
        {
            ++count[len[i]];
        }

        start[1] = 0;
        for (ushort i = 1; i <= 16; ++i)
        {
            start[i + 1] = (ushort)(start[i] + (ushort)(count[i] << (16 - i)));
        }

        if (start[17] != 0)
        {
            return false;
        }

        for (ushort i = 1; i <= 7; ++i)
        {
            start[i] = (ushort)(start[i] >> 9);
            count[i] = (ushort)(1u << (7 - i));
        }

        for (ushort i = 8; i <= 16; ++i)
        {
            count[i] = (ushort)(1u << (16 - i));
        }

        table.Slice(0, 0x80).Clear();

        for (uint symbol = 0; symbol < 8; ++symbol)
        {
            uint bitlen = len[(int)symbol];
            if (bitlen == 0)
            {
                continue;
            }

            ushort position = start[(int)bitlen];
            ushort next = (ushort)(position + count[(int)bitlen]);
            if (next > 0x80u)
            {
                return false;
            }

            if (position < next)
            {
                table.Slice(position, count[(int)bitlen]).Fill((byte)symbol);
            }

            start[(int)bitlen] = next;
        }

        return true;
    }
}

