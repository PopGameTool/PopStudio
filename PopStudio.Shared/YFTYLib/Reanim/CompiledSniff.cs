namespace PopStudio.Reanim
{
    /// <summary>
    /// Lightweight platform sniff for .reanim.compiled (no full decode).
    /// </summary>
    internal static class CompiledSniff
    {
        const int ZlibMagic = -559022380;
        const int MagicPC = -1282165568;
        const int MagicPhone32 = -14326347;
        const int MagicPhone64 = -1069095568;

        /// <summary>
        /// Returns ReanimFormat index 0-5 when recognizable; otherwise null.
        /// </summary>
        public static int? TryDetect(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return null;
            }

            if (!path.EndsWith(".reanim.compiled", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            try
            {
                using (BinaryStream payload = OpenPayload(path))
                {
                    if (payload == null || payload.Length < 24)
                    {
                        return null;
                    }

                    payload.Position = 0;
                    int version = payload.ReadInt32(Endian.Small);
                    if (version == MagicPC)
                    {
                        return (int)ReanimFormat.PCCompiled;
                    }

                    if (version == MagicPhone32)
                    {
                        return (int)ReanimFormat.Phone32Compiled;
                    }

                    if (version == MagicPhone64)
                    {
                        return (int)ReanimFormat.Phone64Compiled;
                    }

                    if (version != 0)
                    {
                        return null;
                    }

                    payload.Position = 20;
                    int strideLe = payload.ReadInt32(Endian.Small);
                    if (strideLe == 0x14)
                    {
                        return (int)ReanimFormat.TVCompiled;
                    }

                    payload.Position = 20;
                    int strideBe = payload.ReadInt32(Endian.Big);
                    if (strideBe == 0x0C)
                    {
                        return (int)ReanimFormat.GameConsoleCompiled;
                    }

                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        static BinaryStream OpenPayload(string path)
        {
            using (BinaryStream source = new BinaryStream(path, FileMode.Open))
            {
                BinaryStream payload = new BinaryStream();
                if (source.PeekInt32(Endian.Small) == ZlibMagic)
                {
                    source.Position = 4;
                    int fieldA = source.ReadInt32(Endian.Small);
                    // Phone64: magic + 0 + size + 0; others: magic + size
                    if (fieldA == 0 && source.Length >= 16)
                    {
                        source.Position = 8;
                        source.ReadInt32(Endian.Small); // size
                        source.Position = 16;
                    }
                    else
                    {
                        source.Position = 8;
                    }

                    using (ZLibStream zlib = new ZLibStream(source, CompressionMode.Decompress))
                    {
                        zlib.CopyTo(payload);
                    }
                }
                else
                {
                    source.Position = 0;
                    source.CopyTo(payload);
                }

                payload.Position = 0;
                return payload;
            }
        }
    }
}
