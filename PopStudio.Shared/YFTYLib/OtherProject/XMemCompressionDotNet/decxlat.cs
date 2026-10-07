#nullable enable
using System;
using System.Buffers.Binary;

namespace XMemCompressionDotNet;

internal static partial class XCompress
{
    internal static void init_decoder_translation(t_decoder_context context)
    {
        context.dec_instr_pos = 0;
    }

    internal static void decoder_translate_e8(t_decoder_context context, Span<byte> mem)
    {
        int bytes = mem.Length;
        if (bytes <= 6)
        {
            context.dec_instr_pos += (uint)bytes;
            return;
        }

        Span<byte> tail = stackalloc byte[6];
        mem.Slice(bytes - 6, 6).CopyTo(tail);
        mem.Slice(bytes - 6, 6).Fill(0xE8);

        uint limit = context.dec_instr_pos + (uint)bytes - 10;

        uint skipped = 0;
        int cursor = 0;
        while (true)
        {
            while (mem[cursor] != 0xE8)
            {
                ++cursor;
                ++skipped;
            }

            context.dec_instr_pos += skipped;

            uint instr_pos = context.dec_instr_pos;
            if (instr_pos >= limit)
            {
                break;
            }

            int relative = BinaryPrimitives.ReadInt32LittleEndian(mem.Slice(cursor + 1, 4));
            int translation_size = (int)context.dec_current_file_size;

            if (relative < translation_size)
            {
                relative -= (int)instr_pos;
                BinaryPrimitives.WriteInt32LittleEndian(mem.Slice(cursor + 1, 4), relative);
            }
            else
            {
                int neg_relative = -relative;
                if ((uint)neg_relative <= instr_pos)
                {
                    BinaryPrimitives.WriteInt32LittleEndian(mem.Slice(cursor + 1, 4), translation_size + relative);
                }
            }

            cursor += 5;
            context.dec_instr_pos = instr_pos + 5;
            skipped = 0;
        }

        context.dec_instr_pos = limit + 10;
        tail.CopyTo(mem.Slice(bytes - 6, 6));
    }
}

