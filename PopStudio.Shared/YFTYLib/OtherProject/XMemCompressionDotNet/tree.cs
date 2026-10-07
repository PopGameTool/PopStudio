#nullable enable
namespace XMemCompressionDotNet;

internal static partial class XCompress
{
    internal static void count_len(t_encoder_context context, short i)
    {
        if (i >= context.enc_tree_n)
        {
            ++context.enc_depth;
            int index = unchecked((ushort)i) * 2;
            count_len(context, (short)context.enc_tree_leftright[index]);
            count_len(context, (short)context.enc_tree_leftright[index + 1]);
            --context.enc_depth;
            return;
        }

        int depth = context.enc_depth;
        if (depth > 16)
        {
            depth = 16;
        }

        ++context.enc_tree_len_cnt[depth];
    }

    internal static void make_len(t_encoder_context context, short root)
    {
        context.enc_tree_len_cnt.AsSpan().Clear();
        count_len(context, root);

        ushort total = 0;
        for (int i = 16; i != 0; --i)
        {
            total = (ushort)(total + (context.enc_tree_len_cnt[i] << (16 - i)));
        }

        while (total != 0)
        {
            --context.enc_tree_len_cnt[16];

            int i = 15;
            while (i != 0 && context.enc_tree_len_cnt[i] == 0)
            {
                --i;
            }

            if (i != 0)
            {
                context.enc_tree_len_cnt[i + 1] += 2;
                --context.enc_tree_len_cnt[i];
            }

            total = (ushort)(total - 1);
        }

        for (byte len = 16; len != 0; --len)
        {
            short count = (short)context.enc_tree_len_cnt[len];
            while (count-- > 0)
            {
                ushort symbol = context.enc_tree_sortptr[context.enc_tree_sortptr_index++];
                context.enc_len[symbol] = len;
            }
        }
    }

    internal static void downheap(t_encoder_context context, short i)
    {
        short k = i;
        ushort value = unchecked((ushort)context.enc_tree_heap[unchecked((ushort)i)]);
        short j = (short)(unchecked((ushort)i) * 2);

        while (j <= context.enc_tree_heapsize)
        {
            if ((j < context.enc_tree_heapsize) &&
                (context.enc_tree_freq[unchecked((ushort)context.enc_tree_heap[unchecked((ushort)j)])] >
                 context.enc_tree_freq[unchecked((ushort)context.enc_tree_heap[unchecked((ushort)(j + 1))])]))
            {
                ++j;
            }

            ushort child = unchecked((ushort)context.enc_tree_heap[unchecked((ushort)j)]);
            if (context.enc_tree_freq[value] <= context.enc_tree_freq[child])
            {
                break;
            }

            context.enc_tree_heap[unchecked((ushort)k)] = unchecked((short)child);
            k = j;
            j = (short)(j * 2);
        }

        context.enc_tree_heap[unchecked((ushort)k)] = unchecked((short)value);
    }

    internal static void make_code(t_encoder_context context, int n, ReadOnlySpan<byte> len, Span<ushort> code)
    {
        Span<ushort> start = stackalloc ushort[18];

        for (int i = 1; i <= 16; ++i)
        {
            start[i + 1] = (ushort)(2 * (start[i] + context.enc_tree_len_cnt[i]));
        }

        for (int i = 0; i < n; ++i)
        {
            byte symbol_len = len[i];
            code[i] = start[symbol_len];
            ++start[symbol_len];
        }
    }

    internal static void make_tree2(t_encoder_context context, short avail, Span<ushort> freqparm, Span<ushort> codeparm)
    {
        for (short i = (short)(context.enc_tree_heapsize >> 1); i >= 1; --i)
        {
            downheap(context, i);
        }

        EnsureTreeArrayLength(ref context.enc_tree_sortptr, codeparm.Length);
        context.enc_tree_sortptr_index = 0;
        short next = avail;

        do
        {
            short first = unchecked((short)context.enc_tree_heap[1]);
            if (first < context.enc_tree_n)
            {
                context.enc_tree_sortptr[context.enc_tree_sortptr_index++] = unchecked((ushort)first);
            }

            short last = context.enc_tree_heapsize;
            context.enc_tree_heap[1] = context.enc_tree_heap[unchecked((ushort)last)];
            context.enc_tree_heapsize = (short)(last - 1);
            downheap(context, 1);

            short second = unchecked((short)context.enc_tree_heap[1]);
            if (second < context.enc_tree_n)
            {
                context.enc_tree_sortptr[context.enc_tree_sortptr_index++] = unchecked((ushort)second);
            }

            short parent = next++;
            freqparm[unchecked((ushort)parent)] = (ushort)(
                freqparm[unchecked((ushort)first)] +
                freqparm[unchecked((ushort)second)]);

            context.enc_tree_heap[1] = parent;
            downheap(context, 1);

            int child_index = unchecked((ushort)parent) * 2;
            context.enc_tree_leftright[child_index + 1] = unchecked((ushort)second);
            context.enc_tree_leftright[child_index] = unchecked((ushort)first);
        }
        while (context.enc_tree_heapsize > 1);

        context.enc_tree_sortptr_index = 0;
        context.enc_tree_len_cnt.AsSpan().Clear();
        count_len(context, (short)(next - 1));

        ushort total = 0;
        for (int i = 16; i != 0; --i)
        {
            total = (ushort)(total + (context.enc_tree_len_cnt[i] << (16 - i)));
        }

        while (total != 0)
        {
            --context.enc_tree_len_cnt[16];

            int i = 15;
            while ((i != 0) && (context.enc_tree_len_cnt[i] == 0))
            {
                --i;
            }

            if (i != 0)
            {
                context.enc_tree_len_cnt[i + 1] += 2;
                --context.enc_tree_len_cnt[i];
            }

            total = (ushort)(total - 1);
        }

        for (byte len = 16; len != 0; --len)
        {
            short count = (short)context.enc_tree_len_cnt[len];
            while (count-- > 0)
            {
                ushort symbol = context.enc_tree_sortptr[context.enc_tree_sortptr_index++];
                context.enc_len[symbol] = len;
            }
        }
    }

    internal static void make_tree(
        t_encoder_context context,
        int n,
        Span<ushort> freqparm,
        Span<byte> lenparm,
        Span<ushort> codeparm,
        bool make_codes)
    {
    redo_tree:
        context.enc_tree_n = n;
        lenparm.Clear();
        EnsureTreeArrayLength(ref context.enc_tree_freq, Math.Max(n * 2, freqparm.Length));
        context.enc_tree_freq.AsSpan().Clear();
        freqparm.CopyTo(context.enc_tree_freq);
        EnsureTreeArrayLength(ref context.enc_len, lenparm.Length);
        context.enc_len.AsSpan().Clear();
        context.enc_depth = 0;
        context.enc_tree_heapsize = 0;
        context.enc_tree_heap[1] = 0;

        for (short i = 0; i < n; ++i)
        {
            context.enc_len[unchecked((ushort)i)] = 0;
            if (context.enc_tree_freq[unchecked((ushort)i)] != 0)
            {
                context.enc_tree_heap[++context.enc_tree_heapsize] = i;
            }
        }

        if (context.enc_tree_heapsize < 2)
        {
            if (context.enc_tree_heapsize == 0)
            {
                codeparm[unchecked((ushort)context.enc_tree_heap[1])] = 0;
                context.enc_len.AsSpan(0, n).CopyTo(lenparm);
                return;
            }

            if (context.enc_tree_heap[1] != 0)
            {
                freqparm[0] = 1;
            }
            else
            {
                freqparm[1] = 1;
            }

            goto redo_tree;
        }

        make_tree2(context, (short)n, context.enc_tree_freq.AsSpan(), codeparm);
        context.enc_len.AsSpan(0, n).CopyTo(lenparm);

        if (!make_codes)
        {
            return;
        }

        Span<ushort> start = stackalloc ushort[18];
        for (int i = 1; i <= 16; ++i)
        {
            start[i + 1] = (ushort)(2 * (start[i] + context.enc_tree_len_cnt[i]));
        }

        for (int i = 0; i < n; ++i)
        {
            byte len = lenparm[i];
            codeparm[i] = start[len];
            ++start[len];
        }
    }

    private static void EnsureTreeArrayLength<T>(ref T[] array, int length)
    {
        if (array.Length != length)
        {
            array = GC.AllocateUninitializedArray<T>(length);
        }
    }
}
