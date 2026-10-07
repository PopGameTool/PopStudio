#nullable enable
using System;
namespace XMemCompressionDotNet;

internal static partial class XCompress
{
    internal static int decode_uncompressed_block(t_decoder_context context, int bufpos, int amount_to_decode)
    {
        int input = context.dec_input_curpos;
        int available = context.dec_end_input_pos - input;
        if (available > context.dec_input.Length - input)
        {
            available = Math.Max(0, context.dec_input.Length - input);
        }

        int end_bufpos = bufpos + amount_to_decode;
        byte[] window = context.dec_mem_window;
        int bytes_to_copy = amount_to_decode < available ? amount_to_decode : available;

        int current_bufpos = bufpos;
        if (bytes_to_copy > 0)
        {
            context.dec_input.AsSpan(input, bytes_to_copy).CopyTo(window.AsSpan(bufpos, bytes_to_copy));
            input += bytes_to_copy;
            current_bufpos += bytes_to_copy;
        }

        if (bufpos + bytes_to_copy != end_bufpos)
        {
            return -1;
        }

        context.dec_input_curpos = input;

        int wrap_copy = ((end_bufpos > 0x101) ? 0x101 : end_bufpos) - bufpos;
        int wrap_destination = (int)context.dec_window_size + bufpos;
        if (wrap_copy > 0)
        {
            window.AsSpan(wrap_destination - (int)context.dec_window_size, wrap_copy)
                .CopyTo(window.AsSpan(wrap_destination, wrap_copy));
        }

        context.dec_bufpos = current_bufpos & (int)context.dec_window_mask;
        return 0;
    }

    internal static bool handle_beginning_of_uncompressed_block(t_decoder_context context)
    {
        context.dec_input_curpos -= 2;

        byte[] input_buffer = context.dec_input;
        int input = context.dec_input_curpos;
        if (input + 4 >= context.dec_end_input_pos)
        {
            return false;
        }

        for (int i = 0; i < 3; ++i)
        {
            uint value =
                ((uint)((uint)input < (uint)input_buffer.Length ? input_buffer[input] : 0) << 0) |
                ((uint)((uint)(input + 1) < (uint)input_buffer.Length ? input_buffer[input + 1] : 0) << 8) |
                ((uint)((uint)(input + 2) < (uint)input_buffer.Length ? input_buffer[input + 2] : 0) << 16) |
                ((uint)((uint)(input + 3) < (uint)input_buffer.Length ? input_buffer[input + 3] : 0) << 24);
            context.dec_last_matchpos_offset[i] = value;
            input += 4;
        }

        context.dec_input_curpos = input;
        return true;
    }
}
