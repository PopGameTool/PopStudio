#nullable enable
namespace XMemCompressionDotNet;

internal static partial class XCompress
{
    internal static void initialise_decoder_bitbuf(t_decoder_context context)
    {
        if (context.dec_block_type == lzx_block_type.BLOCKTYPE_UNCOMPRESSED)
        {
            return;
        }

        int input = context.dec_input_curpos;
        int next = input + 4;
        if (next > context.dec_end_input_pos)
        {
            return;
        }

        byte[] input_buffer = context.dec_input;
        byte b0 = (uint)input < (uint)input_buffer.Length ? input_buffer[input] : (byte)0;
        byte b1 = (uint)(input + 1) < (uint)input_buffer.Length ? input_buffer[input + 1] : (byte)0;
        byte b2 = (uint)(input + 2) < (uint)input_buffer.Length ? input_buffer[input + 2] : (byte)0;
        byte b3 = (uint)(input + 3) < (uint)input_buffer.Length ? input_buffer[input + 3] : (byte)0;
        context.dec_bitbuf =
            ((uint)b1 << 24) |
            ((uint)b0 << 16) |
            ((uint)b3 << 8) |
            b2;
        context.dec_bitcount = 16;
        context.dec_input_curpos = next;
    }

    internal static void init_decoder_input(t_decoder_context context)
    {
        initialise_decoder_bitbuf(context);
    }

    internal static void fillbuf(t_decoder_context context, int n)
    {
        int bitcount = context.dec_bitcount - n;
        context.dec_bitcount = bitcount;
        context.dec_bitbuf <<= n;

        if (bitcount > 0)
        {
            return;
        }

        int input = context.dec_input_curpos;
        int end = context.dec_end_input_pos;
        byte[] input_buffer = context.dec_input;

        if (input >= end)
        {
            context.dec_error_condition = true;
            return;
        }

        do
        {
            uint bits =
                (uint)((uint)input < (uint)input_buffer.Length ? input_buffer[input] : 0) |
                ((uint)((uint)(input + 1) < (uint)input_buffer.Length ? input_buffer[input + 1] : 0) << 8);
            int shift = -bitcount;

            bitcount += 16;
            context.dec_bitcount = bitcount;
            input += 2;

            bits <<= shift;
            context.dec_input_curpos = input;
            context.dec_bitbuf |= bits;

            if (bitcount > 0)
            {
                return;
            }

            if (input >= end)
            {
                context.dec_error_condition = true;
                return;
            }
        }
        while (true);
    }

    internal static uint getbits(t_decoder_context context, int n)
    {
        uint result = context.dec_bitbuf >> (32 - n);
        fillbuf(context, n);
        return result;
    }
}
