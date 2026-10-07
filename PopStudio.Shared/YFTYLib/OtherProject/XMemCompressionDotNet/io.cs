#nullable enable
using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace XMemCompressionDotNet;

internal static partial class XCompress
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void output_bits(t_encoder_context context, int n, uint x)
    {
        int bitcount = context.enc_bitcount;
        int shift = bitcount - n;
        bitcount -= n;
        uint bitbuf = context.enc_bitbuf | (x << shift);

        while (bitcount <= 16)
        {
            int curpos = context.enc_output_buffer_curpos;
            if (curpos >= context.enc_output_buffer_end)
            {
                context.enc_output_overflow = true;
                curpos = 0;
            }

            byte[] output = context.enc_output_buffer_start;
            output[curpos++] = (byte)(bitbuf >> 16);
            output[curpos++] = (byte)(bitbuf >> 24);
            context.enc_output_buffer_curpos = curpos;
            bitbuf <<= 16;
            bitcount += 16;
        }

        context.enc_bitbuf = bitbuf;
        context.enc_bitcount = bitcount;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void output_bits(t_encoder_context context, int n, ulong x)
    {
        output_bits(context, n, (uint)x);
    }

    internal static bool init_compressed_output_buffer(t_encoder_context context)
    {
        context.enc_output_buffer_start = new byte[0x9800];
        context.enc_output_buffer_curpos = 0;
        context.enc_output_buffer_end = 0x97C0;
        return true;
    }

    internal static void free_compressed_output_buffer(t_encoder_context context)
    {
        context.enc_output_buffer_start = [];
        context.enc_output_buffer_curpos = 0;
        context.enc_output_buffer_end = 0;
    }

    internal static void reset_translation(t_encoder_context context)
    {
        context.enc_instr_pos = 0;
    }

    internal static unsafe int read_input_data(t_encoder_context context, Span<byte> mem, int amount)
    {
        int available = context.enc_input_left;
        if (amount <= available)
        {
            if (context.enc_direct_input is not null)
            {
                new ReadOnlySpan<byte>(context.enc_direct_input + context.enc_input_ptr, amount).CopyTo(mem);
            }
            else
            {
                context.enc_input.AsSpan(context.enc_input_ptr, amount).CopyTo(mem);
            }

            context.enc_input_left -= amount;
            context.enc_input_ptr += amount;
            return amount;
        }

        if (available <= 0)
        {
            return 0;
        }

        if (context.enc_direct_input is not null)
        {
            new ReadOnlySpan<byte>(context.enc_direct_input + context.enc_input_ptr, available).CopyTo(mem);
        }
        else
        {
            context.enc_input.AsSpan(context.enc_input_ptr, available).CopyTo(mem);
        }

        context.enc_input_ptr += available;
        context.enc_input_left = 0;
        return available;
    }

    internal static void encoder_translate_e8(t_encoder_context context, Span<byte> mem)
    {
        int bytes = mem.Length;
        if (bytes <= 6)
        {
            context.enc_instr_pos += (uint)bytes;
            return;
        }

        Span<byte> tail = stackalloc byte[6];
        mem.Slice(bytes - 6, 6).CopyTo(tail);
        mem.Slice(bytes - 6, 6).Fill(0xE8);

        uint end_instr_pos = context.enc_instr_pos + (uint)bytes - 10;
        int cursor = 0;
        while (true)
        {
            while (mem[cursor] != 0xE8)
            {
                ++context.enc_instr_pos;
                ++cursor;
            }

            uint instr_pos = context.enc_instr_pos;
            if (instr_pos >= end_instr_pos)
            {
                break;
            }

            int relative = BinaryPrimitives.ReadInt32LittleEndian(mem.Slice(cursor + 1, 4));
            int translated = (int)instr_pos + relative;

            if (translated >= 0)
            {
                int file_size = (int)context.enc_file_size_for_translation;
                if (translated < file_size + (int)instr_pos)
                {
                    if (translated >= file_size)
                    {
                        translated = relative - file_size;
                    }

                    BinaryPrimitives.WriteInt32LittleEndian(mem.Slice(cursor + 1, 4), translated);
                }
            }

            cursor += 5;
            context.enc_instr_pos += 5;
        }

        tail.CopyTo(mem.Slice(bytes - 6, 6));
        context.enc_instr_pos = end_instr_pos + 10;
    }

    internal static int comp_read_input(t_encoder_context context, uint bufPos, int size)
    {
        if (size <= 0)
        {
            return 0;
        }

        int offset = checked(context.enc_MemWindowOffset + (int)bufPos);
        int bytes = read_input_data(context, context.enc_RealMemWindow.AsSpan(offset, size), size);
        if (bytes < 0)
        {
            return 0;
        }

        if (context.enc_file_size_for_translation != 0 && context.enc_num_cfdata_frames < 0x8000)
        {
            encoder_translate_e8(context, context.enc_RealMemWindow.AsSpan(offset, bytes));
        }

        ++context.enc_num_cfdata_frames;
        return bytes;
    }
}
