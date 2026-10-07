#nullable enable
namespace XMemCompressionDotNet;

internal static partial class XCompress
{
    internal static void create_slot_lookup_table(t_encoder_context context)
    {
        context.enc_slot_table[0] = 0;
        context.enc_slot_table[1] = 1;
        context.enc_slot_table[2] = 2;
        context.enc_slot_table[3] = 3;

        int run_length = 2;
        byte slot = 4;
        int offset = 4;

        while (offset < 0x400)
        {
            if (run_length > 0)
            {
                context.enc_slot_table.AsSpan(offset, run_length).Fill(slot);
                offset += run_length;
            }

            ++slot;

            if (run_length > 0)
            {
                context.enc_slot_table.AsSpan(offset, run_length).Fill(slot);
                offset += run_length;
            }

            ++slot;
            run_length *= 2;
        }
    }

    internal static void create_ones_table(t_encoder_context context)
    {
        for (int value = 0; value < 0x100; ++value)
        {
            uint v = (uint)value;
            byte ones = 0;

            while (v != 0)
            {
                if ((v & 1) != 0)
                {
                    ++ones;
                }

                v >>= 1;
            }

            context.enc_ones[value] = ones;
        }
    }
}

