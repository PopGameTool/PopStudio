#nullable enable
using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace XMemCompressionDotNet;

internal enum lzx_block_type
{
    BLOCKTYPE_INVALID = 0,
    BLOCKTYPE_VERBATIM = 1,
    BLOCKTYPE_ALIGNED = 2,
    BLOCKTYPE_UNCOMPRESSED = 3,
}

internal enum decoder_state
{
    DEC_STATE_UNKNOWN = 0,
    DEC_STATE_START_NEW_BLOCK = 1,
    DEC_STATE_DECODING_DATA = 2,
}

internal struct decision_node
{
    public uint link;
    public uint path;
    public uint repeated_offset_0;
    public uint repeated_offset_1;
    public uint repeated_offset_2;
    public uint numbits;

    public uint GetRepeatedOffset(int index)
    {
        return index switch
        {
            0 => repeated_offset_0,
            1 => repeated_offset_1,
            _ => repeated_offset_2,
        };
    }

    public void SetRepeatedOffset(int index, uint value)
    {
        switch (index)
        {
            case 0:
                repeated_offset_0 = value;
                break;
            case 1:
                repeated_offset_1 = value;
                break;
            default:
                repeated_offset_2 = value;
                break;
        }
    }
}

internal delegate byte[] DecoderMallocDelegate(uint cb);
internal delegate void DecoderFreeDelegate(byte[]? pv);
internal delegate int EncoderOutputCallback(object? parameter, ReadOnlySpan<byte> data, int compressedSize, int uncompressedSize);

[InlineArray(8)]
internal struct Byte8
{
    private byte _element0;
}

[InlineArray(20)]
internal struct Byte20
{
    private byte _element0;
}

[InlineArray(32)]
internal struct Byte32
{
    private byte _element0;
}

[InlineArray(52)]
internal struct Byte52
{
    private byte _element0;
}

[InlineArray(5)]
internal struct UInt5
{
    private uint _element0;
}

[InlineArray(8)]
internal struct UInt8
{
    private uint _element0;
}

[InlineArray(96)]
internal struct Short96
{
    private short _element0;
}

[InlineArray(256)]
internal struct Short256
{
    private short _element0;
}

internal sealed class t_decoder_context
{
    public byte[] dec_mem_window = [];
    public uint dec_window_size;
    public uint dec_window_mask;
    public readonly uint[] dec_last_matchpos_offset = new uint[3];
    public readonly short[] dec_main_tree_table = new short[1024];
    public readonly short[] dec_secondary_length_tree_table = new short[256];
    public readonly byte[] dec_main_tree_len = new byte[672];
    public readonly byte[] dec_secondary_length_tree_len = new byte[249];
    public readonly byte[] dec_aligned_table = new byte[128];
    public readonly byte[] dec_aligned_len = new byte[8];
    public readonly short[] dec_main_tree_left_right = new short[2688];
    public readonly short[] dec_secondary_length_tree_left_right = new short[996];
    public byte[] dec_input = [];
    public int dec_input_curpos;
    public int dec_end_input_pos;
    public byte[]? dec_output_buffer;
    public int dec_position_at_start;
    public readonly byte[] dec_main_tree_prev_len = new byte[672];
    public readonly byte[] dec_secondary_length_tree_prev_len = new byte[249];
    public uint dec_bitbuf;
    public int dec_bitcount;
    public byte dec_num_position_slots;
    public bool dec_first_time_this_group;
    public bool dec_error_condition;
    public int dec_bufpos;
    public uint dec_current_file_size;
    public uint dec_instr_pos;
    public uint dec_num_cfdata_frames;
    public int dec_original_block_size;
    public int dec_block_size;
    public lzx_block_type dec_block_type;
    public decoder_state dec_decoder_state;
    public DecoderMallocDelegate? dec_malloc;
    public DecoderFreeDelegate? dec_free;
    public byte[]? dec_memory;
    public int dec_memory_offset;
    public readonly byte[] dec_extra_bits_table = new byte[52];
    public readonly int[] MP_POS_minus2_table = new int[51];
    public byte[] dec_dest_staging_buffer = new byte[0x8000];
    public byte[] dec_source_staging_buffer = new byte[0x980A];
    public uint dec_dest_staging_offset;
    public uint dec_dest_staging_size;
    public uint dec_source_staging_size;
    public bool dec_end_of_stream;
    public ReadOnlyMemory<byte> dec_td_last_source;
    public byte[]? dec_td_last_source_array;
    public object? dec_td_last_source_owner;
    public int dec_td_last_source_identity_offset;
    public uint dec_td_last_source_size;
    public uint dec_td_last_segment_size;
    public uint dec_td_last_segment_offset;
    public uint dec_td_last_source_offset;
    public uint dec_td_last_decoded_size;
    public uint dec_td_last_stage_offset;
    public uint dec_td_last_stage_size;
    public byte[] dec_td_staging_buffer = new byte[0x8000];
}

internal sealed class t_encoder_context
{
    public byte[] enc_RealMemWindow = [];
    public int enc_MemWindowOffset;
    public uint enc_window_size;
    public uint enc_encoder_second_partition_size;
    public uint[] enc_tree_root = new uint[0x10000];
    public uint[] enc_Left = [];
    public uint[] enc_Right = [];
    public uint enc_bitbuf;
    public int enc_bitcount = 32;
    public bool enc_output_overflow;
    public uint enc_literals;
    public uint enc_distances;
    public uint[] enc_DistData = new uint[0x8000];
    public byte[] enc_LitData = new byte[0x10000];
    public byte[] enc_ItemType = new byte[0x2000];
    public readonly uint[] enc_repeated_offset_at_literal_zero = new uint[3];
    public readonly uint[] enc_last_matchpos_offset = new uint[3];
    public readonly uint[] enc_matchpos_table = new uint[258];
    public uint enc_BufPos;
    public byte[] enc_slot_table = new byte[1024];
    public byte[] enc_output_buffer_start = new byte[0x9800];
    public int enc_output_buffer_curpos;
    public int enc_output_buffer_end = 0x97C0;
    public uint enc_input_running_total;
    public uint enc_bufpos_at_last_block;
    public uint enc_bufpos_last_output_block;
    public uint enc_num_position_slots;
    public uint enc_file_size_for_translation;
    public bool enc_allocated_compression_memory;
    public byte enc_num_block_splits;
    public byte[] enc_ones = new byte[256];
    public byte enc_first_block;
    public bool enc_need_to_recalc_stats;
    public bool enc_first_time_this_group;
    public byte[] enc_input = [];
    public unsafe byte* enc_direct_input;
    public int enc_input_ptr;
    public int enc_input_left;
    public uint enc_instr_pos;
    public ushort[] enc_tree_freq = [];
    public ushort[] enc_tree_sortptr = [];
    public int enc_tree_sortptr_index;
    public byte[] enc_len = [];
    public short[] enc_tree_heap = new short[702];
    public ushort[] enc_tree_leftright = new ushort[2798];
    public ushort[] enc_tree_len_cnt = new ushort[17];
    public int enc_tree_n;
    public short enc_tree_heapsize;
    public int enc_depth;
    public uint enc_next_tree_create;
    public uint enc_last_literals;
    public uint enc_last_distances;
    public uint enc_earliest_window_data_remaining;
    public decision_node[] enc_decision_node = new decision_node[0x6054];
    public byte[] enc_main_tree_len = new byte[701];
    public byte[] enc_secondary_tree_len = new byte[250];
    public ushort[] enc_main_tree_freq = new ushort[1400];
    public ushort[] enc_main_tree_code = new ushort[700];
    public byte[] enc_main_tree_prev_len = new byte[701];
    public ushort[] enc_secondary_tree_freq = new ushort[498];
    public ushort[] enc_secondary_tree_code = new ushort[249];
    public byte[] enc_secondary_tree_prev_len = new byte[250];
    public ushort[] enc_aligned_tree_freq = new ushort[16];
    public ushort[] enc_aligned_tree_code = new ushort[8];
    public byte[] enc_aligned_tree_len = new byte[8];
    public byte[] enc_aligned_tree_prev_len = new byte[8];
    public uint enc_num_cfdata_frames;
    public object? enc_fci_data;
    public uint enc_inserted_dict_size;
    public EncoderOutputCallback? enc_output_callback_function;
    public byte[] enc_dest_staging_buffer = new byte[0x26028];
    public uint enc_dest_staging_buffer_size;
    public uint enc_dest_staging_offset;
    public uint enc_dest_staging_size;
    public byte[] enc_source_staging_buffer = new byte[0x8000];
    public uint enc_source_staging_size;
    public uint enc_chunks_submitted;
    public uint enc_chunks_encoded;
    public bool enc_stream_flushed = true;
    public ulong enc_td_segment_pitch;
    public ulong enc_td_uncompressed_size;
    public ulong enc_td_compressed_size;
    public uint[] enc_tdat_uncompressed_size_list = Array.Empty<uint>();
    public uint enc_td_segment_count;
    public uint enc_td_segment_count_expected;
    public uint enc_td_translation_padding;
    public uint enc_td_translation_bits;
    public uint enc_td_translation_bits_expected;
    public bool enc_td_encode_data_uncompressed;
    public IncrementalHash? enc_td_hash;
    public uint enc_context_data_size;
    public t_encoder_context? enc_context_data_snapshot;
    public byte[] enc_td_work_buffer = [];
    public byte[] enc_td_snapshot_buffer = [];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte Window(uint index)
    {
        return enc_RealMemWindow[enc_MemWindowOffset + (int)index];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<byte> WindowSpan(uint index, int length)
    {
        return enc_RealMemWindow.AsSpan(enc_MemWindowOffset + (int)index, length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetWindow(uint index, byte value)
    {
        enc_RealMemWindow[enc_MemWindowOffset + (int)index] = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint Left(uint index)
    {
        return enc_Left[enc_MemWindowOffset + (int)index];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref uint LeftRef(uint index)
    {
        return ref enc_Left[enc_MemWindowOffset + (int)index];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetLeft(uint index, uint value)
    {
        enc_Left[enc_MemWindowOffset + (int)index] = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint Right(uint index)
    {
        return enc_Right[enc_MemWindowOffset + (int)index];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref uint RightRef(uint index)
    {
        return ref enc_Right[enc_MemWindowOffset + (int)index];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetRight(uint index, uint value)
    {
        enc_Right[enc_MemWindowOffset + (int)index] = value;
    }
}

internal sealed class LzxDecompressionContext
{
    public uint Identifier { get; set; } = 0x76C3F251u;

    public XMEMCODEC_TYPE CodecType { get; set; } = XMEMCODEC_TYPE.XMEMCODEC_LZX;

    public uint Flags { get; set; }

    public uint LzxFlags { get; set; }

    public ulong WindowSize { get; set; }

    public t_decoder_context Decoder { get; } = new();
}

internal sealed class LzxCompressionContext
{
    public uint Identifier { get; set; } = 0x76C3F250u;

    public XMEMCODEC_TYPE CodecType { get; set; } = XMEMCODEC_TYPE.XMEMCODEC_LZX;

    public uint Flags { get; set; }

    public uint LzxFlags { get; set; }

    public t_encoder_context Encoder { get; set; } = new();
}

internal ref struct TdPayloadWriter
{
    private Span<byte> _destination;
    private int _position;

    public TdPayloadWriter(Span<byte> destination)
    {
        _destination = destination;
        _position = 0;
    }

    public readonly int Position => _position;

    public void Advance(int count)
    {
        _position += count;
    }

    public readonly Span<byte> Remaining => _destination[_position..];
}
