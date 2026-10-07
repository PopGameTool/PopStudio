#nullable enable
namespace XMemCompressionDotNet;

internal static partial class XCompress
{
    internal static void flush_all_pending_blocks(t_encoder_context context)
    {
        while (context.enc_literals != 0)
        {
            output_block(context);
        }

        perform_flush_output_callback(context);
    }

    internal static void update_tree_estimates(t_encoder_context context)
    {
        if (context.enc_literals == 0)
        {
            return;
        }

        if (context.enc_need_to_recalc_stats)
        {
            get_block_stats(context, 0, 0, context.enc_literals);
            context.enc_need_to_recalc_stats = false;
        }
        else
        {
            update_cumulative_block_stats(
                context,
                context.enc_last_literals,
                context.enc_last_distances,
                context.enc_literals);
        }

        create_trees(context, false);
        fix_tree_cost_estimates(context);
        context.enc_last_literals = context.enc_literals;
        context.enc_last_distances = context.enc_distances;
    }

    internal static void block_end(t_encoder_context context, uint bufPos)
    {
        context.enc_first_block = 0;
        context.enc_need_to_recalc_stats = true;
        output_block(context);

        context.enc_next_tree_create = context.enc_literals < 0x1000
            ? 0x1000
            : context.enc_literals + 0x1000;

        context.enc_bufpos_last_output_block = bufPos;
    }

    internal static bool redo_first_block(t_encoder_context context, ref uint bufpos)
    {
        uint old_bufpos = context.enc_bufpos_last_output_block;
        context.enc_first_block = 0;

        uint available = bufpos - old_bufpos;
        uint back_distance = old_bufpos - context.enc_window_size;
        if (back_distance < context.enc_window_size)
        {
            available += back_distance;
        }
        else
        {
            available += context.enc_window_size;
        }

        uint max_available = (uint)context.enc_MemWindowOffset + bufpos;
        if (available > max_available)
        {
            return false;
        }

        uint split_at_literal = 0;
        split_block(context, 0, context.enc_literals, context.enc_distances, out split_at_literal, out _);
        get_block_stats(context, 0, 0, split_at_literal);
        create_trees(context, false);
        fix_tree_cost_estimates(context);

        context.enc_tree_root.AsSpan().Clear();
        context.enc_ItemType.AsSpan().Clear();

        context.enc_input_running_total = 0;
        context.enc_literals = 0;
        context.enc_distances = 0;
        context.enc_last_matchpos_offset[0] = 1;
        context.enc_last_matchpos_offset[1] = 1;
        context.enc_last_matchpos_offset[2] = 1;
        context.enc_next_tree_create = split_at_literal;
        context.enc_repeated_offset_at_literal_zero[0] = 1;
        context.enc_repeated_offset_at_literal_zero[1] = 1;
        context.enc_repeated_offset_at_literal_zero[2] = 1;
        context.enc_need_to_recalc_stats = true;

        bufpos = old_bufpos;
        return true;
    }

    internal static void encoder_start(t_encoder_context context)
    {
        uint input_position = (uint)context.enc_BufPos;
        int bytes_read = comp_read_input(context, input_position, 0x8000);
        if (bytes_read > 0)
        {
            opt_encode_top(context, bytes_read);
        }
    }

    internal static void opt_encode_top(t_encoder_context context, int bytesRead)
    {
        byte[] window = context.enc_RealMemWindow;
        int windowOffset = context.enc_MemWindowOffset;
        byte[] mainTreeLen = context.enc_main_tree_len;
        byte[] secondaryTreeLen = context.enc_secondary_tree_len;
        byte[] litData = context.enc_LitData;
        byte[] itemType = context.enc_ItemType;
        uint[] distData = context.enc_DistData;
        byte[] slotTable = context.enc_slot_table;
        ReadOnlySpan<byte> extraBits = enc_extra_bits;
        uint bufPos = context.enc_BufPos;
        uint endPos = bufPos + (uint)bytesRead;

        if (context.enc_first_time_this_group)
        {
            context.enc_first_time_this_group = false;
            context.enc_next_tree_create = 0x2710;

            if (context.enc_file_size_for_translation != 0)
            {
                output_bits(context, 1, 1);
                output_bits(context, 16, context.enc_file_size_for_translation >> 16);
                output_bits(context, 16, context.enc_file_size_for_translation & 0xFFFFu);
            }
            else
            {
                output_bits(context, 1, 0);
            }
        }
        else
        {
            for (uint i = 0x32; i != 0; --i)
            {
                quick_insert_bsearch_findmatch(
                    context,
                    bufPos - i,
                    bufPos - context.enc_window_size + 4);
            }
        }

        for (;;)
        {
            while (bufPos < endPos)
            {
                uint chunk_end = (bufPos + 0x8000u) & 0xFFFF8000u;
                if (chunk_end > endPos)
                {
                    chunk_end = endPos;
                }

                int best_match_length = binary_search_findmatch(context, bufPos);
                if (best_match_length >= 2)
                {
                    uint clamped_end = bufPos + (uint)best_match_length;
                    if (clamped_end > chunk_end)
                    {
                        best_match_length = (int)(chunk_end - bufPos);
                    }
                }

                if (best_match_length < 2)
                {
                    litData[context.enc_literals] = window[windowOffset + (int)bufPos];
                    ++context.enc_literals;
                    ++bufPos;

                    if (context.enc_literals >= 0xFFF8)
                    {
                        block_end(context, bufPos);
                    }

                    continue;
                }

                if (best_match_length >= 0x32)
                {
                    uint length = (uint)best_match_length;
                    uint distance_code = context.enc_matchpos_table[length];

                    if (distance_code == 3 && length > 0x10)
                    {
                        quick_insert_bsearch_findmatch(
                            context,
                            bufPos + 1,
                            bufPos - context.enc_window_size + 5);
                    }
                    else
                    {
                        for (uint i = 1; i < length; ++i)
                        {
                            quick_insert_bsearch_findmatch(
                                context,
                                bufPos + i,
                                bufPos + i - context.enc_window_size + 4);
                        }
                    }

                    itemType[context.enc_literals >> 3] |= (byte)(1u << (int)(context.enc_literals & 7));
                    litData[context.enc_literals] = (byte)(length - 2);
                    distData[context.enc_distances] = distance_code;
                    ++context.enc_literals;
                    ++context.enc_distances;
                    bufPos += length;

                    if (distance_code >= 3)
                    {
                        context.enc_last_matchpos_offset[2] = context.enc_last_matchpos_offset[1];
                        context.enc_last_matchpos_offset[1] = context.enc_last_matchpos_offset[0];
                        context.enc_last_matchpos_offset[0] = distance_code - 2;
                    }
                    else if (distance_code != 0)
                    {
                        uint value = context.enc_last_matchpos_offset[distance_code];
                        context.enc_last_matchpos_offset[distance_code] = context.enc_last_matchpos_offset[0];
                        context.enc_last_matchpos_offset[0] = value;
                    }

                    if (context.enc_literals >= 0xFFF8 || context.enc_distances >= 0x7FF8)
                    {
                        block_end(context, bufPos);
                    }

                    continue;
                }

                uint start_pos = bufPos;
                uint decision_limit = start_pos + 0xEFD;
                uint reachable_end = start_pos + (uint)best_match_length;
                uint remaining_in_chunk = chunk_end - start_pos;
                decision_node[] decisions = context.enc_decision_node;

                decisions[1].numbits = mainTreeLen[window[windowOffset + (int)start_pos]];
                decisions[1].path = start_pos;

                for (uint length = 2; length <= (uint)best_match_length; ++length)
                {
                    uint distance_code = context.enc_matchpos_table[length];
                    uint slot;
                    if (distance_code < 0x400)
                    {
                        slot = slotTable[distance_code];
                    }
                    else if (distance_code < 0x80000)
                    {
                        slot = (uint)(slotTable[distance_code >> 9] + 0x12);
                    }
                    else
                    {
                        slot = (byte)(distance_code >> 17) + 0x22u;
                    }

                    uint cost;
                    if (length < 9)
                    {
                        cost = (uint)(mainTreeLen[0xFE + 8 * slot + length] + extraBits[(int)slot]);
                    }
                    else
                    {
                        cost = (uint)(
                            mainTreeLen[0x107 + 8 * slot] +
                            extraBits[(int)slot] +
                            secondaryTreeLen[length - 9]);
                    }

                    decisions[length].link = distance_code;
                    decisions[length].path = start_pos;
                    decisions[length].numbits = cost;
                }

                decisions[0].numbits = 0;
                decisions[0].repeated_offset_0 = context.enc_last_matchpos_offset[0];
                decisions[0].repeated_offset_1 = context.enc_last_matchpos_offset[1];
                decisions[0].repeated_offset_2 = context.enc_last_matchpos_offset[2];

                uint current_pos = start_pos;
                uint output_end_pos = start_pos;
                for (;;)
                {
                    ++current_pos;
                    --remaining_in_chunk;

                    ref decision_node current_decision = ref decisions[current_pos - start_pos];
                    if (current_decision.path != (current_pos - 1))
                    {
                        ref readonly decision_node previous = ref decisions[current_decision.path - start_pos];
                        if (current_decision.link >= 3)
                        {
                            context.enc_last_matchpos_offset[0] = current_decision.link - 2;
                            context.enc_last_matchpos_offset[1] = previous.repeated_offset_0;
                            context.enc_last_matchpos_offset[2] = previous.repeated_offset_1;
                        }
                        else if (current_decision.link == 0)
                        {
                            context.enc_last_matchpos_offset[0] = previous.repeated_offset_0;
                            context.enc_last_matchpos_offset[1] = previous.repeated_offset_1;
                            context.enc_last_matchpos_offset[2] = previous.repeated_offset_2;
                        }
                        else if (current_decision.link == 1)
                        {
                            context.enc_last_matchpos_offset[0] = previous.repeated_offset_1;
                            context.enc_last_matchpos_offset[1] = previous.repeated_offset_0;
                            context.enc_last_matchpos_offset[2] = previous.repeated_offset_2;
                        }
                        else
                        {
                            context.enc_last_matchpos_offset[0] = previous.repeated_offset_2;
                            context.enc_last_matchpos_offset[1] = previous.repeated_offset_1;
                            context.enc_last_matchpos_offset[2] = previous.repeated_offset_0;
                        }
                    }

                    current_decision.repeated_offset_0 = context.enc_last_matchpos_offset[0];
                    current_decision.repeated_offset_1 = context.enc_last_matchpos_offset[1];
                    current_decision.repeated_offset_2 = context.enc_last_matchpos_offset[2];

                    if (current_pos == reachable_end)
                    {
                        output_end_pos = current_pos;
                        break;
                    }

                    int current_match_length = binary_search_findmatch(context, current_pos);
                    if ((current_pos + (uint)current_match_length) > chunk_end)
                    {
                        current_match_length = (int)remaining_in_chunk;
                        if (remaining_in_chunk < 2)
                        {
                            current_match_length = 0;
                        }
                    }

                    if (current_match_length <= 0x32 &&
                        (current_pos + (uint)current_match_length) < decision_limit)
                    {
                        if (current_match_length > 2 ||
                            (current_match_length == 2 && context.enc_matchpos_table[2] < 0x800))
                        {
                            uint candidate_end = current_pos + (uint)current_match_length;
                            if (candidate_end > reachable_end)
                            {
                                uint new_limit = candidate_end - start_pos;
                                if (new_limit > 0xEFC)
                                {
                                    new_limit = 0xEFC;
                                }

                                uint invalid_index = reachable_end - start_pos + 1;
                                while (invalid_index <= new_limit)
                                {
                                    decisions[invalid_index].numbits = 0xFFFFFFFFu;
                                    ++invalid_index;
                                }

                                reachable_end = candidate_end;
                            }
                        }

                        uint literal_cost = current_decision.numbits + mainTreeLen[window[windowOffset + (int)current_pos]];
                        ref decision_node next_literal = ref decisions[current_pos + 1 - start_pos];
                        if (literal_cost < next_literal.numbits)
                        {
                            next_literal.numbits = literal_cost;
                            next_literal.path = current_pos;
                        }

                        for (uint length = 2; length <= (uint)current_match_length; ++length)
                        {
                            uint distance_code = context.enc_matchpos_table[length];
                            uint slot;
                            if (distance_code < 0x400)
                            {
                                slot = slotTable[distance_code];
                            }
                            else if (distance_code < 0x80000)
                            {
                                slot = (uint)(slotTable[distance_code >> 9] + 0x12);
                            }
                            else
                            {
                                slot = (byte)(distance_code >> 17) + 0x22u;
                            }

                            uint cost;
                            if (length < 9)
                            {
                                cost = (uint)(mainTreeLen[0xFE + 8 * slot + length] + extraBits[(int)slot]);
                            }
                            else
                            {
                                cost = (uint)(
                                    mainTreeLen[0x107 + 8 * slot] +
                                    extraBits[(int)slot] +
                                    secondaryTreeLen[length - 9]);
                            }

                            cost += current_decision.numbits;

                            ref decision_node target = ref decisions[current_pos + length - start_pos];
                            if (cost < target.numbits)
                            {
                                target.numbits = cost;
                                target.path = current_pos;
                                target.link = distance_code;
                            }
                        }

                        continue;
                    }

                    uint length2 = (uint)current_match_length;
                    uint next_pos = current_pos + length2;
                    uint distance_code2 = context.enc_matchpos_table[length2];
                    ref decision_node direct_target = ref decisions[next_pos - start_pos];
                    direct_target.link = distance_code2;
                    direct_target.path = current_pos;

                    if (distance_code2 == 3 && length2 > 0x10)
                    {
                        quick_insert_bsearch_findmatch(
                            context,
                            current_pos + 1,
                            current_pos - context.enc_window_size + 5);
                    }
                    else
                    {
                        for (uint i = 1; i < length2; ++i)
                        {
                            quick_insert_bsearch_findmatch(
                                context,
                                current_pos + i,
                                current_pos + i - context.enc_window_size + 4);
                        }
                    }

                    bufPos = next_pos;
                    if (distance_code2 >= 3)
                    {
                        context.enc_last_matchpos_offset[2] = context.enc_last_matchpos_offset[1];
                        context.enc_last_matchpos_offset[1] = context.enc_last_matchpos_offset[0];
                        context.enc_last_matchpos_offset[0] = distance_code2 - 2;
                    }
                    else if (distance_code2 != 0)
                    {
                        uint value = context.enc_last_matchpos_offset[distance_code2];
                        context.enc_last_matchpos_offset[distance_code2] = context.enc_last_matchpos_offset[0];
                        context.enc_last_matchpos_offset[0] = value;
                    }

                    output_end_pos = next_pos;
                    break;
                }

                uint output_pos = output_end_pos;
                uint output_count = 0;
                uint predecessor = decisions[output_pos - start_pos].path;
                do
                {
                    uint previous_pos = predecessor;
                    ++output_count;
                    predecessor = decisions[previous_pos - start_pos].path;
                    decisions[previous_pos - start_pos].path = output_pos;
                    output_pos = previous_pos;
                }
                while (output_pos != start_pos);

                while (context.enc_literals + output_count >= 0xFFF8 ||
                       context.enc_distances + output_count >= 0x7FF8)
                {
                    block_end(context, start_pos);
                }

                output_pos = start_pos;
                while (output_count-- != 0)
                {
                    uint next_pos = decisions[output_pos - start_pos].path;
                    if (next_pos > output_pos + 1)
                    {
                        itemType[context.enc_literals >> 3] |= (byte)(1u << (int)(context.enc_literals & 7));
                        litData[context.enc_literals] = (byte)(next_pos - output_pos - 2);
                        distData[context.enc_distances] = decisions[next_pos - start_pos].link;
                        ++context.enc_distances;
                    }
                    else
                    {
                        litData[context.enc_literals] = window[windowOffset + (int)output_pos];
                    }

                    ++context.enc_literals;
                    output_pos = next_pos;
                }

                bufPos = output_pos;

                if (context.enc_literals >= context.enc_next_tree_create)
                {
                    if (context.enc_literals != 0)
                    {
                        if (context.enc_need_to_recalc_stats)
                        {
                            get_block_stats(context, 0, 0, context.enc_literals);
                            context.enc_need_to_recalc_stats = false;
                        }
                        else
                        {
                            update_cumulative_block_stats(
                                context,
                                context.enc_last_literals,
                                context.enc_last_distances,
                                context.enc_literals);
                        }

                        create_trees(context, false);
                        fix_tree_cost_estimates(context);
                        context.enc_last_literals = context.enc_literals;
                        context.enc_last_distances = context.enc_distances;
                    }

                    context.enc_next_tree_create += 0x1000;
                }

                if (context.enc_first_block != 0)
                {
                    if ((context.enc_literals >= 0xFE00) || (context.enc_distances >= 0x7E00))
                    {
                        if (!redo_first_block(context, ref bufPos))
                        {
                            block_end(context, bufPos);
                        }
                    }
                }
            }

            context.enc_earliest_window_data_remaining = bufPos - context.enc_window_size;
            if (bytesRead < 0x8000)
            {
                if ((context.enc_first_block != 0) &&
                    redo_first_block(context, ref bufPos))
                {
                    continue;
                }

                break;
            }

            uint remove_path = context.enc_earliest_window_data_remaining + 0x36;
            for (uint i = 1; i <= 0x32; ++i)
            {
                binary_search_remove_node(context, bufPos - i, remove_path);
            }

            uint window_usage = (uint)context.enc_MemWindowOffset + bufPos;
            if (window_usage >= context.enc_encoder_second_partition_size + context.enc_window_size)
            {
                if ((context.enc_first_block != 0) &&
                    redo_first_block(context, ref bufPos))
                {
                    continue;
                }

                int moveBytes = checked((int)context.enc_window_size);
                Array.Copy(
                    context.enc_RealMemWindow,
                    (int)context.enc_encoder_second_partition_size,
                    context.enc_RealMemWindow,
                    0,
                    moveBytes);
                Array.Copy(
                    context.enc_Left,
                    (int)context.enc_encoder_second_partition_size,
                    context.enc_Left,
                    0,
                    moveBytes);
                Array.Copy(
                    context.enc_Right,
                    (int)context.enc_encoder_second_partition_size,
                    context.enc_Right,
                    0,
                    moveBytes);

                context.enc_MemWindowOffset -= (int)context.enc_encoder_second_partition_size;
                context.enc_earliest_window_data_remaining = bufPos - context.enc_window_size;
            }

            break;
        }

        context.enc_BufPos = bufPos;
    }
}
