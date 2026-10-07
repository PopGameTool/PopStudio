#nullable enable
using System;

namespace XMemCompressionDotNet;

internal static partial class XCompress
{
    internal static void copy_data_to_output(t_decoder_context context, int amount, int source)
    {
        if (context.dec_output_buffer is null)
        {
            return;
        }

        bool translate_e8 =
            context.dec_current_file_size != 0 &&
            context.dec_num_cfdata_frames < 0x8000;

        context.dec_mem_window.AsSpan(source, amount)
            .CopyTo(context.dec_output_buffer.AsSpan(0, amount));

        if (translate_e8)
        {
            decoder_translate_e8(context, context.dec_output_buffer.AsSpan(0, amount));
        }
    }
}
