using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace PopStudio.RTON
{
    internal static class RTON
    {
        public static readonly string magic = "RTON";
        public static readonly int version = 0x1;
        public static readonly string EOF = "DONE";

        static readonly StringPool R0x90 = new StringPool();
        static readonly StringPool R0x92 = new StringPool();
        static readonly List<byte[]> R0x90List = new List<byte[]>();
        static readonly List<byte[]> R0x92List = new List<byte[]>();
        static readonly byte[] NULL = new byte[] { 0x2A };
        static readonly byte[] RTID0 = new byte[] { 0x52, 0x54, 0x49, 0x44, 0x28, 0x30, 0x29 };
        static readonly string Str_Null = "*";
        static readonly string Str_RTID_Begin = "RTID(";
        static readonly string Str_RTID_End = ")";
        static readonly string Str_RTID_0 = "RTID(0)";
        static readonly string Str_RTID_2 = "RTID({0}.{1}.{2:x8}@{3})";
        static readonly string Str_RTID_3 = "RTID({0}@{1})";
        static readonly string Str_Binary = "$BINARY(\"{0}\", {1})";
        static readonly string Str_Binary_Begin = "$BINARY(\"";
        static readonly string Str_Binary_End = ")";
        static readonly string Str_Binary_Middle = "\", ";

        /// <summary>
        /// Now it is very fast
        /// </summary>
        /// <param name="inFile"></param>
        /// <param name="outFile"></param>
        public static void Decode(string inFile, string outFile)
        {
            R0x90List.Clear();
            R0x92List.Clear();
            using (FileStream stream = new FileStream(outFile, FileMode.Create))
            {
                using (Utf8JsonWriter sw = new Utf8JsonWriter(stream, new JsonWriterOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, Indented = true }))
                {
                    using (BinaryStream bs = BinaryStream.Open(inFile))
                    {
                        bs.IdString(magic);
                        bs.IdInt32(version);
                        ReadJObject(bs, sw);
                        bs.IdString(EOF);
                    }
                }
            }
            R0x90List.Clear();
            R0x92List.Clear();
        }

        public static void DecodeAndDecrypt(string inFile, string outFile)
        {
            R0x90List.Clear();
            R0x92List.Clear();
            //The key for Rijndael is the MD5 of the enterred key
            byte[] keybytes = Encoding.UTF8.GetBytes(BitConverter.ToString(System.Security.Cryptography.MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(Setting.RTONKey))).ToLower().Replace("-", ""));
            byte[] ivbytes = new byte[24];
            //The iv for Rijndael is part of the key for Rijndael
            Array.Copy(keybytes, 4, ivbytes, 0, 24);
            byte[] source;
            using (FileStream stream = new FileStream(outFile, FileMode.Create))
            {
                using (Utf8JsonWriter sw = new Utf8JsonWriter(stream, new JsonWriterOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, Indented = true }))
                {
                    using (BinaryStream bs = BinaryStream.Open(inFile))
                    {
                        bs.IdInt16(0x10);
                        source = bs.ReadBytes((int)(bs.Length - 2));
                    }
                    using (BinaryStream bs = new BinaryStream(RijndaelHelper.Decrypt(source, keybytes, ivbytes, new Org.BouncyCastle.Crypto.Paddings.ZeroBytePadding())))
                    {
                        bs.Position = 0;
                        bs.IdString(magic);
                        bs.IdInt32(version);
                        ReadJObject(bs, sw);
                        bs.IdString(EOF);
                    }
                }
            }
            R0x90List.Clear();
            R0x92List.Clear();
        }

        static string ReadBinary(BinaryStream bs)
        {
            bs.ReadByte();
            string s = bs.ReadStringByVarInt32Head();
            int i = bs.ReadVarInt32();
            return string.Format(Str_Binary, s, i);
        }

        static string ReadRTID(BinaryStream bs)
        {
            byte temp = bs.ReadByte();
            switch (temp)
            {
                case 0x00:
                    return Str_RTID_0;
                case 0x01: //Not sure
                    int value_1_2 = bs.ReadVarInt32();
                    int value_1_1 = bs.ReadVarInt32();
                    uint x16_1 = bs.ReadUInt32();
                    return string.Format(Str_RTID_2, value_1_1, value_1_2, x16_1, string.Empty);
                case 0x02:
                    bs.ReadVarInt32();
                    string str = bs.ReadStringByVarInt32Head();
                    int value_2_2 = bs.ReadVarInt32();
                    int value_2_1 = bs.ReadVarInt32();
                    uint x16_2 = bs.ReadUInt32();
                    return string.Format(Str_RTID_2, value_2_1, value_2_2, x16_2, str);
                case 0x03:
                    bs.ReadVarInt32();
                    string str2 = bs.ReadStringByVarInt32Head();
                    bs.ReadVarInt32();
                    string str1 = bs.ReadStringByVarInt32Head();
                    return string.Format(Str_RTID_3, str1, str2);
                default:
                    throw new Exception($"No such type in 0x83: {temp}");
            }
        }

        static void ReadJArray(BinaryStream bs, Utf8JsonWriter sw)
        {
            sw.WriteStartArray();
            byte[] tempstring;
            bs.IdByte(0xFD);
            int number = bs.ReadVarInt32();
            for (int i = 0; i < number; i++)
            {
                byte type = bs.ReadByte();
                switch (type)
                {
                    case 0x0:
                        sw.WriteBooleanValue(false);
                        break;
                    case 0x1:
                        sw.WriteBooleanValue(true);
                        break;
                    case 0x2:
                        sw.WriteStringValue(NULL);
                        break;
                    case 0x8:
                        sw.WriteNumberValue(bs.ReadSByte());
                        break;
                    case 0x9:
                        sw.WriteNumberValue(0);
                        break;
                    case 0xA:
                        sw.WriteNumberValue(bs.ReadByte());
                        break;
                    case 0xB:
                        sw.WriteNumberValue(0);
                        break;
                    case 0x10:
                        sw.WriteNumberValue(bs.ReadInt16());
                        break;
                    case 0x11:
                        sw.WriteNumberValue(0);
                        break;
                    case 0x12:
                        sw.WriteNumberValue(bs.ReadUInt16());
                        break;
                    case 0x13:
                        sw.WriteNumberValue(0);
                        break;
                    case 0x20:
                        sw.WriteNumberValue(bs.ReadInt32());
                        break;
                    case 0x21:
                        sw.WriteNumberValue(0);
                        break;
                    case 0x22:
                        sw.WriteNumberValue(bs.ReadFloat32());
                        break;
                    case 0x23:
                        sw.WriteNumberValue(0F);
                        break;
                    case 0x24:
                        sw.WriteNumberValue(bs.ReadVarInt32());
                        break;
                    case 0x25:
                        sw.WriteNumberValue(bs.ReadZigZag32());
                        break;
                    case 0x26:
                        sw.WriteNumberValue(bs.ReadUInt32());
                        break;
                    case 0x27:
                        sw.WriteNumberValue(0U);
                        break;
                    case 0x28:
                        sw.WriteNumberValue(bs.ReadUVarInt32());
                        break;
                    case 0x40:
                        sw.WriteNumberValue(bs.ReadInt64());
                        break;
                    case 0x41:
                        sw.WriteNumberValue(0L);
                        break;
                    case 0x42:
                        sw.WriteNumberValue(bs.ReadFloat64());
                        break;
                    case 0x43:
                        sw.WriteNumberValue(0D);
                        break;
                    case 0x44:
                        sw.WriteNumberValue(bs.ReadVarInt64());
                        break;
                    case 0x45:
                        sw.WriteNumberValue(bs.ReadZigZag64());
                        break;
                    case 0x46:
                        sw.WriteNumberValue(bs.ReadUInt64());
                        break;
                    case 0x47:
                        sw.WriteNumberValue(0UL);
                        break;
                    case 0x48:
                        sw.WriteNumberValue(bs.ReadUVarInt64());
                        break;
                    case 0x81:
                        sw.WriteStringValue(bs.ReadBytes(bs.ReadVarInt32()));
                        break;
                    case 0x82:
                        bs.ReadVarInt32();
                        sw.WriteStringValue(bs.ReadBytes(bs.ReadVarInt32()));
                        break;
                    case 0x83:
                        sw.WriteStringValue(ReadRTID(bs));
                        break;
                    case 0x84:
                        sw.WriteStringValue(RTID0);
                        break;
                    case 0x85:
                        ReadJObject(bs, sw);
                        break;
                    case 0x86:
                        ReadJArray(bs, sw);
                        break;
                    case 0x87:
                        sw.WriteStringValue(ReadBinary(bs));
                        break;
                    case 0x90:
                        tempstring = bs.ReadBytes(bs.ReadVarInt32());
                        R0x90List.Add(tempstring);
                        sw.WriteStringValue(tempstring);
                        break;
                    case 0x91:
                        sw.WriteStringValue(R0x90List[bs.ReadVarInt32()]);
                        break;
                    case 0x92:
                        bs.ReadVarInt32();
                        tempstring = bs.ReadBytes(bs.ReadVarInt32());
                        R0x92List.Add(tempstring);
                        sw.WriteStringValue(tempstring);
                        break;
                    case 0x93:
                        sw.WriteStringValue(R0x92List[bs.ReadVarInt32()]);
                        break;
                    case 0xB0:
                    case 0xB1:
                    case 0xB2:
                    case 0xB3:
                    case 0xB4:
                    case 0xB5:
                    case 0xB6:
                    case 0xB7:
                    case 0xB8:
                    //about object
                    case 0xB9:
                    //about array
                    case 0xBA:
                    //about string
                    case 0xBB:
                        //about binary
                        throw new Exception("0xb0-0xbb is not supported!");
                    case 0xBC:
                        sw.WriteBooleanValue(bs.ReadByte() != 0);
                        break;
                    default:
                        throw new Exception(Str.Obj.TypeMisMatch);
                }
            }
            bs.IdByte(0xFE);
            sw.WriteEndArray();
        }

        static void ReadJObject(BinaryStream bs, Utf8JsonWriter sw)
        {
            sw.WriteStartObject();
            byte[] tempstring;
            HashSet<string> seenKeys = new HashSet<string>();

            while (true)
            {
                //key
                byte type = bs.ReadByte();
                if (type == 0xFF)
                {
                    break;
                }

                string currentKey = null;
                bool isDuplicate = false;

                // 读取key
                switch (type)
                {
                    case 0x2:
                        currentKey = Encoding.UTF8.GetString(NULL);
                        break;
                    case 0x81:
                        currentKey = Encoding.UTF8.GetString(bs.ReadBytes(bs.ReadVarInt32()));
                        break;
                    case 0x82:
                        bs.ReadVarInt32();
                        currentKey = Encoding.UTF8.GetString(bs.ReadBytes(bs.ReadVarInt32()));
                        break;
                    case 0x83:
                        currentKey = ReadRTID(bs);
                        break;
                    case 0x84:
                        currentKey = Encoding.UTF8.GetString(RTID0);
                        break;
                    case 0x87:
                        currentKey = ReadBinary(bs);
                        break;
                    case 0x90:
                        tempstring = bs.ReadBytes(bs.ReadVarInt32());
                        R0x90List.Add(tempstring);
                        currentKey = Encoding.UTF8.GetString(tempstring);
                        break;
                    case 0x91:
                        currentKey = Encoding.UTF8.GetString(R0x90List[bs.ReadVarInt32()]);
                        break;
                    case 0x92:
                        bs.ReadVarInt32();
                        tempstring = bs.ReadBytes(bs.ReadVarInt32());
                        R0x92List.Add(tempstring);
                        currentKey = Encoding.UTF8.GetString(tempstring);
                        break;
                    case 0x93:
                        currentKey = Encoding.UTF8.GetString(R0x92List[bs.ReadVarInt32()]);
                        break;
                    default:
                        throw new Exception(Str.Obj.TypeMisMatch);
                }

                if (seenKeys.Contains(currentKey))
                {
                    isDuplicate = true;
                }
                else
                {
                    seenKeys.Add(currentKey);
                    sw.WritePropertyName(currentKey);
                }

                //value
                type = bs.ReadByte();

                switch (type)
                {
                    case 0x0:
                        if (!isDuplicate) sw.WriteBooleanValue(false);
                        break;
                    case 0x1:
                        if (!isDuplicate) sw.WriteBooleanValue(true);
                        break;
                    case 0x2:
                        if (!isDuplicate) sw.WriteStringValue(NULL);
                        break;
                    case 0x8:
                        sbyte v8 = bs.ReadSByte();
                        if (!isDuplicate) sw.WriteNumberValue(v8);
                        break;
                    case 0x9:
                        if (!isDuplicate) sw.WriteNumberValue(0);
                        break;
                    case 0xA:
                        byte vA = bs.ReadByte();
                        if (!isDuplicate) sw.WriteNumberValue(vA);
                        break;
                    case 0xB:
                        if (!isDuplicate) sw.WriteNumberValue(0);
                        break;
                    case 0x10:
                        short v10 = bs.ReadInt16();
                        if (!isDuplicate) sw.WriteNumberValue(v10);
                        break;
                    case 0x11:
                        if (!isDuplicate) sw.WriteNumberValue(0);
                        break;
                    case 0x12:
                        ushort v12 = bs.ReadUInt16();
                        if (!isDuplicate) sw.WriteNumberValue(v12);
                        break;
                    case 0x13:
                        if (!isDuplicate) sw.WriteNumberValue(0);
                        break;
                    case 0x20:
                        int v20 = bs.ReadInt32();
                        if (!isDuplicate) sw.WriteNumberValue(v20);
                        break;
                    case 0x21:
                        if (!isDuplicate) sw.WriteNumberValue(0);
                        break;
                    case 0x22:
                        float v22 = bs.ReadFloat32();
                        if (!isDuplicate) sw.WriteNumberValue(v22);
                        break;
                    case 0x23:
                        if (!isDuplicate) sw.WriteNumberValue(0F);
                        break;
                    case 0x24:
                        int v24 = bs.ReadVarInt32();
                        if (!isDuplicate) sw.WriteNumberValue(v24);
                        break;
                    case 0x25:
                        int v25 = bs.ReadZigZag32();
                        if (!isDuplicate) sw.WriteNumberValue(v25);
                        break;
                    case 0x26:
                        uint v26 = bs.ReadUInt32();
                        if (!isDuplicate) sw.WriteNumberValue(v26);
                        break;
                    case 0x27:
                        if (!isDuplicate) sw.WriteNumberValue(0U);
                        break;
                    case 0x28:
                        uint v28 = bs.ReadUVarInt32();
                        if (!isDuplicate) sw.WriteNumberValue(v28);
                        break;
                    case 0x40:
                        long v40 = bs.ReadInt64();
                        if (!isDuplicate) sw.WriteNumberValue(v40);
                        break;
                    case 0x41:
                        if (!isDuplicate) sw.WriteNumberValue(0L);
                        break;
                    case 0x42:
                        double v42 = bs.ReadFloat64();
                        if (!isDuplicate) sw.WriteNumberValue(v42);
                        break;
                    case 0x43:
                        if (!isDuplicate) sw.WriteNumberValue(0D);
                        break;
                    case 0x44:
                        long v44 = bs.ReadVarInt64();
                        if (!isDuplicate) sw.WriteNumberValue(v44);
                        break;
                    case 0x45:
                        long v45 = bs.ReadZigZag64();
                        if (!isDuplicate) sw.WriteNumberValue(v45);
                        break;
                    case 0x46:
                        ulong v46 = bs.ReadUInt64();
                        if (!isDuplicate) sw.WriteNumberValue(v46);
                        break;
                    case 0x47:
                        if (!isDuplicate) sw.WriteNumberValue(0UL);
                        break;
                    case 0x48:
                        ulong v48 = bs.ReadUVarInt64();
                        if (!isDuplicate) sw.WriteNumberValue(v48);
                        break;
                    case 0x81:
                        byte[] v81 = bs.ReadBytes(bs.ReadVarInt32());
                        if (!isDuplicate) sw.WriteStringValue(v81);
                        break;
                    case 0x82:
                        bs.ReadVarInt32();
                        byte[] v82 = bs.ReadBytes(bs.ReadVarInt32());
                        if (!isDuplicate) sw.WriteStringValue(v82);
                        break;
                    case 0x83:
                        string v83 = ReadRTID(bs);
                        if (!isDuplicate) sw.WriteStringValue(v83);
                        break;
                    case 0x84:
                        if (!isDuplicate) sw.WriteStringValue(RTID0);
                        break;
                    case 0x85:
                        if (isDuplicate)
                        {
                            using (var tempStream = new System.IO.MemoryStream())
                            using (var tempWriter = new Utf8JsonWriter(tempStream))
                            {
                                ReadJObject(bs, tempWriter);
                            }
                        }
                        else
                        {
                            ReadJObject(bs, sw);
                        }
                        break;
                    case 0x86:
                        if (isDuplicate)
                        {
                            using (var tempStream = new System.IO.MemoryStream())
                            using (var tempWriter = new Utf8JsonWriter(tempStream))
                            {
                                ReadJArray(bs, tempWriter);
                            }
                        }
                        else
                        {
                            ReadJArray(bs, sw);
                        }
                        break;
                    case 0x87:
                        string v87 = ReadBinary(bs);
                        if (!isDuplicate) sw.WriteStringValue(v87);
                        break;
                    case 0x90:
                        tempstring = bs.ReadBytes(bs.ReadVarInt32());
                        R0x90List.Add(tempstring);
                        if (!isDuplicate) sw.WriteStringValue(tempstring);
                        break;
                    case 0x91:
                        byte[] v91 = R0x90List[bs.ReadVarInt32()];
                        if (!isDuplicate) sw.WriteStringValue(v91);
                        break;
                    case 0x92:
                        bs.ReadVarInt32();
                        tempstring = bs.ReadBytes(bs.ReadVarInt32());
                        R0x92List.Add(tempstring);
                        if (!isDuplicate) sw.WriteStringValue(tempstring);
                        break;
                    case 0x93:
                        byte[] v93 = R0x92List[bs.ReadVarInt32()];
                        if (!isDuplicate) sw.WriteStringValue(v93);
                        break;
                    case 0xB0:
                    case 0xB1:
                    case 0xB2:
                    case 0xB3:
                    case 0xB4:
                    case 0xB5:
                    case 0xB6:
                    case 0xB7:
                    case 0xB8:
                    //about object
                    case 0xB9:
                    //about array
                    case 0xBA:
                    //about string
                    case 0xBB:
                        //about binary
                        throw new Exception("0xb0-0xbb is not supported!");
                    case 0xBC:
                        byte vBC = bs.ReadByte();
                        if (!isDuplicate) sw.WriteBooleanValue(vBC != 0);
                        break;
                    default:
                        throw new Exception(Str.Obj.TypeMisMatch);
                }
            }
            sw.WriteEndObject();
        }

        public static void EncodeAndEncrypt(string inFile, string outFile)
        {
            R0x90.Clear();
            R0x92.Clear();
            using BinaryStream sr = new BinaryStream(inFile, FileMode.Open);
            using JsonDocument json = JsonDocument.Parse(sr, new JsonDocumentOptions { AllowTrailingCommas = true });
            JsonElement root = json.RootElement;
            byte[] source;
            using (BinaryStream bs = new BinaryStream())
            {
                bs.WriteString(magic);
                bs.WriteInt32(version);
                WriteJObject(bs, root);
                bs.WriteString(EOF);
                bs.Position = 0;
                source = ((MemoryStream)bs.BaseStream).ToArray();
            }
            //The key for Rijndael is the MD5 of the enterred key
            byte[] keybytes = Encoding.UTF8.GetBytes(BitConverter.ToString(System.Security.Cryptography.MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(Setting.RTONKey))).ToLower().Replace("-", ""));
            byte[] ivbytes = new byte[24];
            //The iv for Rijndael is part of the key for Rijndael
            Array.Copy(keybytes, 4, ivbytes, 0, 24);
            using (BinaryStream bs = new BinaryStream(outFile, FileMode.Create))
            {
                bs.WriteInt16(0x10);
                bs.WriteBytes(RijndaelHelper.Encrypt(source, keybytes, ivbytes, new Org.BouncyCastle.Crypto.Paddings.ZeroBytePadding()));
            }
            R0x90.Clear();
            R0x92.Clear();
        }

        public static void Encode(string inFile, string outFile)
        {
            R0x90.Clear();
            R0x92.Clear();
            using BinaryStream sr = new BinaryStream(inFile, FileMode.Open);
            using JsonDocument json = JsonDocument.Parse(sr, new JsonDocumentOptions { AllowTrailingCommas = true });
            JsonElement root = json.RootElement;
            using (BinaryStream bs = BinaryStream.Create(outFile))
            {
                bs.WriteString(magic);
                bs.WriteInt32(version);
                WriteJObject(bs, root);
                bs.WriteString(EOF);
            }
            R0x90.Clear();
            R0x92.Clear();
        }

        static bool IsASCII(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] > 127) return false;
            }
            return true;
        }

        static bool WriteBinary(BinaryStream bs, string str)
        {
            if (str.StartsWith(Str_Binary_Begin) && str.EndsWith(Str_Binary_End))
            {
                int index = str.LastIndexOf(Str_Binary_Middle);
                if (index == -1) return false;
                int v;
                try
                {
                    v = Convert.ToInt32(str[(index + 3)..^1]);
                }
                catch (Exception)
                {
                    return false;
                }
                string mString = str[9..index];
                bs.WriteByte(0x87);
                bs.WriteByte(0);
                bs.WriteStringByVarInt32Head(mString);
                bs.WriteVarInt32(v);
            }
            return false;
        }

        static bool WriteRTID(BinaryStream bs, string str)
        {
            if (str.StartsWith(Str_RTID_Begin) && str.EndsWith(Str_RTID_End))
            {
                if (str == Str_RTID_0)
                {
                    bs.WriteByte(0x84);
                    return true;
                }
                string newstr = str[5..^1];
                int index;
                if ((index = newstr.IndexOf('@')) > -1)
                {
                    bs.WriteByte(0x83);
                    string str1 = newstr[..index];
                    string str2 = newstr[(index + 1)..];
                    //test if str1 is for 0x83 0x02
                    bool isr8302 = true;
                    int dot1index = 0, dot2index = 0, dindex = 0;
                    for (int i = 0; i < str1.Length; i++)
                    {
                        if (str1[i] == '.')
                        {
                            switch (dindex)
                            {
                                case 0:
                                    dot1index = i;
                                    break;
                                case 1:
                                    dot2index = i;
                                    break;
                                default:
                                    isr8302 = false;
                                    break;
                            }
                            dindex++;
                        }
                        else if (str1[i] > '9' && str1[i] < '0' && (!(dindex == 2 && ((str1[i] >= 'a' && str1[i] <= 'f') || (str1[i] >= 'A' && str1[i] <= 'F')))))
                        {
                            isr8302 = false;
                        }
                        if (!isr8302) break;
                    }
                    if (dindex != 2)
                    {
                        isr8302 = false;
                    }
                    if (isr8302)
                    {
                        bs.WriteByte(0x02);
                        bs.WriteVarInt32(str2.Length);
                        bs.WriteStringByVarInt32Head(str2);
                        bs.WriteVarInt32(Convert.ToInt32(str1[(dot1index + 1)..dot2index]));
                        bs.WriteVarInt32(Convert.ToInt32(str1[..dot1index]));
                        bs.WriteUInt32(Convert.ToUInt32(str1[(dot2index + 1)..]));
                    }
                    else
                    {
                        bs.WriteByte(0x03);
                        bs.WriteVarInt32(str2.Length);
                        bs.WriteStringByVarInt32Head(str2);
                        bs.WriteVarInt32(str1.Length);
                        bs.WriteStringByVarInt32Head(str1);
                    }
                    return true;
                }
            }
            return false;
        }

        static void WriteJArray(BinaryStream bs, JsonElement json)
        {
            bs.WriteByte(0xFD);
            int n = json.GetArrayLength();
            bs.WriteVarInt32(n);
            for (int i = 0; i < n; i++)
            {
                JsonElement value = json[i];
                switch (value.ValueKind)
                {
                    case JsonValueKind.Object:
                        bs.WriteByte(0x85);
                        WriteJObject(bs, value);
                        break;
                    case JsonValueKind.Array:
                        bs.WriteByte(0x86);
                        WriteJArray(bs, value);
                        break;
                    case JsonValueKind.Undefined:
                    case JsonValueKind.Null:
                        bs.WriteByte(0x84);
                        break;
                    case JsonValueKind.True:
                        bs.WriteByte(0x01);
                        break;
                    case JsonValueKind.False:
                        bs.WriteByte(0x00);
                        break;
                    case JsonValueKind.String:
                        string str = value.GetString();
                        if (str == Str_Null)
                        {
                            bs.WriteByte(0x02);
                        }
                        else if (WriteRTID(bs, str))
                        {
                            //83rton or 84(has already been written by WriteRTID function)
                        }
                        else if (WriteBinary(bs, str))
                        {
                            //87
                        }
                        else if (IsASCII(str))
                        {
                            //9091rton
                            if (R0x90.Exist(str))
                            {
                                //91
                                bs.WriteByte(0x91);
                                bs.WriteVarInt32(R0x90[str].Index);
                            }
                            else
                            {
                                //90
                                bs.WriteByte(0x90);
                                bs.WriteStringByVarInt32Head(str);
                                R0x90.ThrowInPool(str);
                            }
                        }
                        else
                        {
                            //9293rton
                            if (R0x92.Exist(str))
                            {
                                //93
                                bs.WriteByte(0x93);
                                bs.WriteVarInt32(R0x92[str].Index);
                            }
                            else
                            {
                                //92
                                bs.WriteByte(0x92);
                                bs.WriteVarInt32(str.Length);
                                bs.WriteStringByVarInt32Head(str);
                                R0x92.ThrowInPool(str);
                            }
                        }
                        break;
                    case JsonValueKind.Number:
                        if (value.GetRawText().IndexOf('.') > -1)
                        {
                            //is float number
                            double F64Val = value.GetDouble();
                            if (F64Val == 0.0)
                            {
                                bs.WriteByte(0x23);
                            }
                            else
                            {
                                float F32Val = value.GetSingle();
                                if (((double)F32Val) == F64Val)
                                {
                                    bs.WriteByte(0x22);
                                    bs.WriteFloat32(F32Val);
                                }
                                else
                                {
                                    bs.WriteByte(0x42);
                                    bs.WriteFloat64(F64Val);
                                }
                            }
                        }
                        else
                        {
                            //is varint number
                            //but don't know if it is ulong
                            if (value.TryGetInt64(out long I64Val))
                            {
                                //long I64Val = value.GetInt64();
                                if (I64Val == 0)
                                {
                                    bs.WriteByte(0x21);
                                }
                                else if (I64Val > 0)
                                {
                                    if (I64Val <= int.MaxValue)
                                    {
                                        //rton24
                                        bs.WriteByte(0x24);
                                        bs.WriteVarInt32((int)I64Val);
                                    }
                                    else
                                    {
                                        //rton44
                                        bs.WriteByte(0x44);
                                        bs.WriteVarInt64(I64Val);
                                    }
                                }
                                else
                                {
                                    if (I64Val + 0x40000000 >= 0)
                                    {
                                        bs.WriteByte(0x25);
                                        bs.WriteZigZag32((int)I64Val);
                                    }
                                    else
                                    {
                                        bs.WriteByte(0x45);
                                        bs.WriteZigZag64(I64Val);
                                    }
                                }
                            }
                            else
                            {
                                ulong v = value.GetUInt64();
                                bs.WriteByte(0x46);
                                bs.WriteUInt64(v);
                            }
                        }
                        break;
                    default:
                        throw new Exception(Str.Obj.UnknownFormat);
                }
            }
            bs.WriteByte(0xFE);
        }

        static void WriteJObject(BinaryStream bs, JsonElement json)
        {
            foreach (JsonProperty property in json.EnumerateObject())
            {
                //key
                string key = property.Name;
                if (key == Str_Null)
                {
                    bs.WriteByte(0x02);
                }
                else if (WriteRTID(bs, key))
                {
                    //83rton
                }
                else if (WriteBinary(bs, key))
                {
                    //87
                }
                else if (IsASCII(key))
                {
                    //9091rton
                    if (R0x90.Exist(key))
                    {
                        //91
                        bs.WriteByte(0x91);
                        bs.WriteVarInt32(R0x90[key].Index);
                    }
                    else
                    {
                        //90
                        bs.WriteByte(0x90);
                        bs.WriteStringByVarInt32Head(key);
                        R0x90.ThrowInPool(key);
                    }
                }
                else
                {
                    //9293rton
                    if (R0x92.Exist(key))
                    {
                        //93
                        bs.WriteByte(0x93);
                        bs.WriteVarInt32(R0x92[key].Index);
                    }
                    else
                    {
                        //92
                        bs.WriteByte(0x92);
                        bs.WriteVarInt32(key.Length);
                        bs.WriteStringByVarInt32Head(key);
                        R0x92.ThrowInPool(key);
                    }
                }
                JsonElement value = property.Value;
                switch (value.ValueKind)
                {
                    case JsonValueKind.Object:
                        bs.WriteByte(0x85);
                        WriteJObject(bs, value);
                        break;
                    case JsonValueKind.Array:
                        bs.WriteByte(0x86);
                        WriteJArray(bs, value);
                        break;
                    case JsonValueKind.Undefined:
                    case JsonValueKind.Null:
                        bs.WriteByte(0x84);
                        break;
                    case JsonValueKind.True:
                        bs.WriteByte(0x01);
                        break;
                    case JsonValueKind.False:
                        bs.WriteByte(0x00);
                        break;
                    case JsonValueKind.String:
                        string str = value.GetString();
                        if (str == Str_Null)
                        {
                            bs.WriteByte(0x02);
                        }
                        else if (WriteRTID(bs, str))
                        {
                            //83rton
                        }
                        else if (WriteBinary(bs, str))
                        {
                            //87
                        }
                        else if (IsASCII(str))
                        {
                            //9091rton
                            if (R0x90.Exist(str))
                            {
                                //91
                                bs.WriteByte(0x91);
                                bs.WriteVarInt32(R0x90[str].Index);
                            }
                            else
                            {
                                //90
                                bs.WriteByte(0x90);
                                bs.WriteStringByVarInt32Head(str);
                                R0x90.ThrowInPool(str);
                            }
                        }
                        else
                        {
                            //9293rton
                            if (R0x92.Exist(str))
                            {
                                //93
                                bs.WriteByte(0x93);
                                bs.WriteVarInt32(R0x92[str].Index);
                            }
                            else
                            {
                                //92
                                bs.WriteByte(0x92);
                                bs.WriteVarInt32(str.Length);
                                bs.WriteStringByVarInt32Head(str);
                                R0x92.ThrowInPool(str);
                            }
                        }
                        break;
                    case JsonValueKind.Number:
                        if (value.GetRawText().IndexOf('.') > -1)
                        {
                            //is float number
                            double F64Val = value.GetDouble();
                            if (F64Val == 0.0)
                            {
                                bs.WriteByte(0x23);
                            }
                            else
                            {
                                float F32Val = value.GetSingle();
                                if (((double)F32Val) == F64Val)
                                {
                                    bs.WriteByte(0x22);
                                    bs.WriteFloat32(F32Val);
                                }
                                else
                                {
                                    bs.WriteByte(0x42);
                                    bs.WriteFloat64(F64Val);
                                }
                            }
                        }
                        else
                        {
                            //is varint number
                            //but don't know if it is ulong
                            if (value.TryGetInt64(out long I64Val))
                            {
                                //long I64Val = value.GetInt64();
                                if (I64Val == 0)
                                {
                                    bs.WriteByte(0x21);
                                }
                                else if (I64Val > 0)
                                {
                                    if (I64Val <= int.MaxValue)
                                    {
                                        //rton24
                                        bs.WriteByte(0x24);
                                        bs.WriteVarInt32((int)I64Val);
                                    }
                                    else
                                    {
                                        //rton44
                                        bs.WriteByte(0x44);
                                        bs.WriteVarInt64(I64Val);
                                    }
                                }
                                else
                                {
                                    if (I64Val + 0x40000000 >= 0)
                                    {
                                        bs.WriteByte(0x25);
                                        bs.WriteZigZag32((int)I64Val);
                                    }
                                    else
                                    {
                                        bs.WriteByte(0x45);
                                        bs.WriteZigZag64(I64Val);
                                    }
                                }
                            }
                            else
                            {
                                ulong v = value.GetUInt64();
                                bs.WriteByte(0x46);
                                bs.WriteUInt64(v);
                            }
                        }
                        break;
                    default:
                        throw new Exception(Str.Obj.UnknownFormat);
                }
            }
            bs.WriteByte(0xFF);
        }
    }
}
