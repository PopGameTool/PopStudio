#nullable enable
using System;

namespace XMemCompressionDotNet;

internal static partial class XCompress
{
    internal static ReadOnlySpan<byte> enc_extra_bits =>
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

    internal static ReadOnlySpan<uint> enc_slot_mask =>
    [
        0x00000, 0x00000, 0x00000, 0x00000,
        0x00001, 0x00001, 0x00003, 0x00003,
        0x00007, 0x00007, 0x0000F, 0x0000F,
        0x0001F, 0x0001F, 0x0003F, 0x0003F,
        0x0007F, 0x0007F, 0x000FF, 0x000FF,
        0x001FF, 0x001FF, 0x003FF, 0x003FF,
        0x007FF, 0x007FF, 0x00FFF, 0x00FFF,
        0x01FFF, 0x01FFF, 0x03FFF, 0x03FFF,
        0x07FFF, 0x07FFF, 0x0FFFF, 0x0FFFF,
        0x1FFFF, 0x1FFFF, 0x1FFFF, 0x1FFFF,
        0x1FFFF, 0x1FFFF, 0x1FFFF, 0x1FFFF,
        0x1FFFF, 0x1FFFF, 0x1FFFF, 0x1FFFF,
        0x1FFFF, 0x1FFFF, 0x1FFFF, 0x1FFFF,
    ];

    internal static void get_final_repeated_offset_states(t_encoder_context context, uint distances)
    {
        uint[] distData = context.enc_DistData;
        uint[] repeatedOffsets = context.enc_repeated_offset_at_literal_zero;
        int start_index = (int)distances - 1;
        byte consecutive_new_offsets = 0;

        for (int i = start_index; i >= 0; --i)
        {
            if (distData[i] <= 2)
            {
                consecutive_new_offsets = 0;
            }
            else
            {
                ++consecutive_new_offsets;
                if (consecutive_new_offsets >= 3)
                {
                    start_index = i;
                    break;
                }
            }

            start_index = i - 1;
        }

        if (consecutive_new_offsets < 3)
        {
            start_index = 0;
        }

        for (uint i = (uint)start_index; i < distances; ++i)
        {
            uint distance = distData[i];
            if (distance == 0)
            {
                continue;
            }

            if (distance <= 2)
            {
                uint value = repeatedOffsets[distance];
                repeatedOffsets[distance] = repeatedOffsets[0];
                repeatedOffsets[0] = value;
            }
            else
            {
                repeatedOffsets[2] = repeatedOffsets[1];
                repeatedOffsets[1] = repeatedOffsets[0];
                repeatedOffsets[0] = distance - 2;
            }
        }
    }

    internal static uint estimate_compressed_block_size(t_encoder_context context)
    {
        ushort[] mainFreq = context.enc_main_tree_freq;
        byte[] mainLen = context.enc_main_tree_len;
        ushort[] secondaryFreq = context.enc_secondary_tree_freq;
        byte[] secondaryLen = context.enc_secondary_tree_len;
        ReadOnlySpan<byte> extraBitsTable = enc_extra_bits;
        uint bits = 0x4B0;

        for (int i = 0; i < 0x100; ++i)
        {
            bits += (uint)(mainFreq[i] * mainLen[i]);
        }

        for (uint slot = 0; slot < context.enc_num_position_slots; ++slot)
        {
            uint extra_bits = extraBitsTable[(int)slot];
            int baseIndex = 0x100 + 8 * (int)slot;

            for (int i = 0; i < 8; ++i)
            {
                bits += (uint)(mainFreq[baseIndex + i] * (mainLen[baseIndex + i] + extra_bits));
            }
        }

        for (int i = 0; i < 0xF9; ++i)
        {
            bits += (uint)(secondaryFreq[i] * secondaryLen[i]);
        }

        return (bits + 7) >> 3;
    }

    internal static void perform_flush_output_callback(t_encoder_context context)
    {
        if (context.enc_input_running_total > 0)
        {
            flush_output_bit_buffer(context);

            int bytes = context.enc_output_buffer_curpos;
            if (bytes > 0)
            {
                ReadOnlySpan<byte> buffer = context.enc_output_buffer_start.AsSpan(0, bytes);
                context.enc_output_callback_function?.Invoke(
                    context.enc_fci_data,
                    buffer,
                    bytes,
                    (int)context.enc_input_running_total);
            }
        }

        context.enc_input_running_total = 0;
        context.enc_bitbuf = 0;
        context.enc_output_buffer_curpos = 0;
        context.enc_bitcount = 32;
    }

    internal static void encode_uncompressed_block(t_encoder_context context, uint bufpos, uint block_size)
    {
        output_bits(context, context.enc_bitcount - 16, 0);

        for (int i = 0; i < 3; ++i)
        {
            uint value = context.enc_repeated_offset_at_literal_zero[i];
            for (int byte_index = 0; byte_index < 4; ++byte_index)
            {
                if (context.enc_output_buffer_curpos >= context.enc_output_buffer_end)
                {
                    context.enc_output_overflow = true;
                    context.enc_output_buffer_curpos = 0;
                }

                context.enc_output_buffer_start[context.enc_output_buffer_curpos++] = (byte)value;
                value >>= 8;
            }
        }

        while (block_size != 0)
        {
            if (context.enc_output_buffer_curpos >= context.enc_output_buffer_end)
            {
                context.enc_output_overflow = true;
                context.enc_output_buffer_curpos = 0;
            }

            context.enc_output_buffer_start[context.enc_output_buffer_curpos++] = context.Window(bufpos++);
            --block_size;
            ++context.enc_input_running_total;

            if (context.enc_input_running_total == 0x8000)
            {
                perform_flush_output_callback(context);
                context.enc_num_block_splits = 0;
            }
        }

        context.enc_bitbuf = 0;
        context.enc_bitcount = 32;
    }

    internal static void encode_verbatim_block(t_encoder_context context, uint literal_to_end_at)
    {
        byte[] itemType = context.enc_ItemType;
        byte[] litData = context.enc_LitData;
        uint[] distData = context.enc_DistData;
        byte[] mainLen = context.enc_main_tree_len;
        ushort[] mainCode = context.enc_main_tree_code;
        byte[] secondaryLen = context.enc_secondary_tree_len;
        ushort[] secondaryCode = context.enc_secondary_tree_code;
        byte[] slotTable = context.enc_slot_table;
        ReadOnlySpan<byte> extraBitsTable = enc_extra_bits;
        ReadOnlySpan<uint> slotMaskTable = enc_slot_mask;
        uint literal_index = 0;
        uint distance_index = 0;

        while (literal_index < literal_to_end_at)
        {
            byte bit = (byte)(1u << (int)(literal_index & 7));
            if ((itemType[literal_index >> 3] & bit) == 0)
            {
                uint symbol = litData[literal_index];
                output_bits(context, mainLen[symbol], mainCode[symbol]);
                ++literal_index;
                ++context.enc_input_running_total;
            }
            else
            {
                uint length_symbol = litData[literal_index];
                uint distance = distData[distance_index++];
                uint slot;

                if (distance < 0x400)
                {
                    slot = slotTable[distance];
                }
                else if (distance < 0x80000)
                {
                    slot = (uint)(slotTable[distance >> 9] + 0x12);
                }
                else
                {
                    slot = (byte)(distance >> 17) + 0x22u;
                }

                if (length_symbol < 7)
                {
                    uint symbol = 0x100 + 8 * slot + length_symbol;
                    output_bits(context, mainLen[symbol], mainCode[symbol]);
                }
                else
                {
                    uint symbol = 0x107 + 8 * slot;
                    output_bits(context, mainLen[symbol], mainCode[symbol]);
                    output_bits(
                        context,
                        secondaryLen[length_symbol - 7],
                        secondaryCode[length_symbol - 7]);
                }

                byte extraBits = extraBitsTable[(int)slot];
                if (extraBits != 0)
                {
                    output_bits(context, extraBits, slotMaskTable[(int)slot] & distance);
                }

                ++literal_index;
                context.enc_input_running_total += length_symbol + 2;
            }

            if (context.enc_input_running_total == 0x8000)
            {
                perform_flush_output_callback(context);
                context.enc_num_block_splits = 0;
            }
        }
    }

    internal static void encode_aligned_block(t_encoder_context context, uint literal_to_end_at)
    {
        byte[] itemType = context.enc_ItemType;
        byte[] litData = context.enc_LitData;
        uint[] distData = context.enc_DistData;
        byte[] mainLen = context.enc_main_tree_len;
        ushort[] mainCode = context.enc_main_tree_code;
        byte[] secondaryLen = context.enc_secondary_tree_len;
        ushort[] secondaryCode = context.enc_secondary_tree_code;
        byte[] alignedLen = context.enc_aligned_tree_len;
        ushort[] alignedCode = context.enc_aligned_tree_code;
        byte[] slotTable = context.enc_slot_table;
        ReadOnlySpan<byte> extraBitsTable = enc_extra_bits;
        ReadOnlySpan<uint> slotMaskTable = enc_slot_mask;
        uint literal_index = 0;
        uint distance_index = 0;

        while (literal_index < literal_to_end_at)
        {
            byte bit = (byte)(1u << (int)(literal_index & 7));
            if ((itemType[literal_index >> 3] & bit) == 0)
            {
                uint symbol = litData[literal_index];
                output_bits(context, mainLen[symbol], mainCode[symbol]);
                ++literal_index;
                ++context.enc_input_running_total;
            }
            else
            {
                uint length_symbol = litData[literal_index];
                uint distance = distData[distance_index++];
                uint slot;

                if (distance < 0x400)
                {
                    slot = slotTable[distance];
                }
                else if (distance < 0x80000)
                {
                    slot = (uint)(slotTable[distance >> 9] + 0x12);
                }
                else
                {
                    slot = (byte)(distance >> 17) + 0x22u;
                }

                if (length_symbol < 7)
                {
                    uint symbol = 0x100 + 8 * slot + length_symbol;
                    output_bits(context, mainLen[symbol], mainCode[symbol]);
                }
                else
                {
                    uint symbol = 0x107 + 8 * slot;
                    output_bits(context, mainLen[symbol], mainCode[symbol]);
                    output_bits(
                        context,
                        secondaryLen[length_symbol - 7],
                        secondaryCode[length_symbol - 7]);
                }

                byte extraBits = extraBitsTable[(int)slot];
                if (extraBits > 3)
                {
                    uint bits = (uint)extraBits - 3;
                    output_bits(context, (int)bits, (distance >> 3) & ((1u << (int)bits) - 1));
                    output_bits(context, alignedLen[distance & 7], alignedCode[distance & 7]);
                }
                else if (extraBits == 3)
                {
                    output_bits(context, alignedLen[distance & 7], alignedCode[distance & 7]);
                }
                else if (extraBits != 0)
                {
                    output_bits(context, extraBits, slotMaskTable[(int)slot] & distance);
                }

                ++literal_index;
                context.enc_input_running_total += length_symbol + 2;
            }

            if (context.enc_input_running_total == 0x8000)
            {
                perform_flush_output_callback(context);
                context.enc_num_block_splits = 0;
            }
        }
    }
}
