namespace PopStudio.Particles
{
    internal class PC
    {
        public static void Encode(Particles particles, string outFile)
        {
            using (BinaryStream bs = new BinaryStream())
            {
                bs.WriteInt32(1092589901);
                bs.WriteInt32(0);
                int count = particles.Emitters?.Length ?? 0;
                bs.WriteInt32(count);
                bs.WriteInt32(0x164);
                for (int i = 0; i < count; i++)
                {
                    ParticlesEmitter emitter = particles.Emitters[i];
                    bs.WriteInt32(0);
                    bs.WriteInt32(emitter.ImageCol ?? 0);
                    bs.WriteInt32(emitter.ImageRow ?? 0);
                    bs.WriteInt32(emitter.ImageFrames ?? 1);
                    bs.WriteInt32(emitter.Animated ?? 0);
                    bs.WriteInt32(emitter.ParticleFlags);
                    bs.WriteInt32(emitter.EmitterType ?? 1);
                    bs.WriteInt32(0);
                    bs.WriteInt32(0);
                    for (int j = 0; j < 22; j++)
                    {
                        bs.WriteInt32(0);
                        bs.WriteInt32(0);
                    }
                    bs.WriteInt32(0);
                    bs.WriteInt32(emitter.Field?.Length ?? 0);
                    bs.WriteInt32(0);
                    bs.WriteInt32(emitter.SystemField?.Length ?? 0);
                    for (int j = 24; j < 40; j++)
                    {
                        bs.WriteInt32(0);
                        bs.WriteInt32(0);
                    }
                }
                for (int i = 0; i < count; i++)
                {
                    ParticlesEmitter emitter = particles.Emitters[i];
                    bs.WriteStringByInt32Head((string)emitter.Image);
                    bs.WriteStringByInt32Head(emitter.Name);
                    WriteTrackNodes(bs, emitter.SystemDuration);
                    bs.WriteStringByInt32Head(emitter.OnDuration);
                    WriteTrackNodes(bs, emitter.CrossFadeDuration);
                    WriteTrackNodes(bs, emitter.SpawnRate);
                    WriteTrackNodes(bs, emitter.SpawnMinActive);
                    WriteTrackNodes(bs, emitter.SpawnMaxActive);
                    WriteTrackNodes(bs, emitter.SpawnMaxLaunched);
                    WriteTrackNodes(bs, emitter.EmitterRadius);
                    WriteTrackNodes(bs, emitter.EmitterOffsetX);
                    WriteTrackNodes(bs, emitter.EmitterOffsetY);
                    WriteTrackNodes(bs, emitter.EmitterBoxX);
                    WriteTrackNodes(bs, emitter.EmitterBoxY);
                    WriteTrackNodes(bs, emitter.EmitterPath);
                    WriteTrackNodes(bs, emitter.EmitterSkewX);
                    WriteTrackNodes(bs, emitter.EmitterSkewY);
                    WriteTrackNodes(bs, emitter.ParticleDuration);
                    WriteTrackNodes(bs, emitter.SystemRed);
                    WriteTrackNodes(bs, emitter.SystemGreen);
                    WriteTrackNodes(bs, emitter.SystemBlue);
                    WriteTrackNodes(bs, emitter.SystemAlpha);
                    WriteTrackNodes(bs, emitter.SystemBrightness);
                    WriteTrackNodes(bs, emitter.LaunchSpeed);
                    WriteTrackNodes(bs, emitter.LaunchAngle);
                    WriteFields(bs, emitter.Field);
                    WriteFields(bs, emitter.SystemField);
                    WriteTrackNodes(bs, emitter.ParticleRed);
                    WriteTrackNodes(bs, emitter.ParticleGreen);
                    WriteTrackNodes(bs, emitter.ParticleBlue);
                    WriteTrackNodes(bs, emitter.ParticleAlpha);
                    WriteTrackNodes(bs, emitter.ParticleBrightness);
                    WriteTrackNodes(bs, emitter.ParticleSpinAngle);
                    WriteTrackNodes(bs, emitter.ParticleSpinSpeed);
                    WriteTrackNodes(bs, emitter.ParticleScale);
                    WriteTrackNodes(bs, emitter.ParticleStretch);
                    WriteTrackNodes(bs, emitter.CollisionReflect);
                    WriteTrackNodes(bs, emitter.CollisionSpin);
                    WriteTrackNodes(bs, emitter.ClipTop);
                    WriteTrackNodes(bs, emitter.ClipBottom);
                    WriteTrackNodes(bs, emitter.ClipLeft);
                    WriteTrackNodes(bs, emitter.ClipRight);
                    WriteTrackNodes(bs, emitter.AnimationRate);
                }
                bs.Position = 0;
                using (BinaryStream bs_source = new BinaryStream(outFile, FileMode.Create))
                {
                    bs_source.WriteInt32(-559022380);
                    bs_source.WriteInt32((int)bs.Length);
                    using (ZLibStream compressionStream = new ZLibStream(bs_source, CompressionMode.Compress))
                    {
                        bs.CopyTo(compressionStream);
                    }
                }
            }
        }

        static void WriteFields(BinaryStream bs, ParticlesField[] fields)
        {
            bs.WriteInt32(0x14);
            if (fields == null)
            {
                return;
            }
            int count = fields.Length;
            for (int i = 0; i < count; i++)
            {
                bs.WriteInt32(fields[i].FieldType ?? 0);
                bs.WriteInt32(0);
                bs.WriteInt32(0);
                bs.WriteInt32(0);
                bs.WriteInt32(0);
            }
            for (int i = 0; i < count; i++)
            {
                WriteTrackNodes(bs, fields[i].X);
                WriteTrackNodes(bs, fields[i].Y);
            }
        }

        static void WriteTrackNodes(BinaryStream bs, ParticlesTrackNode[] nodes)
        {
            if (nodes == null)
            {
                bs.WriteInt32(0);
                return;
            }
            int count = nodes.Length;
            bs.WriteInt32(count);
            for (int i = 0; i < count; i++)
            {
                ParticlesTrackNode node = nodes[i];
                bs.WriteFloat32(node.Time);
                bs.WriteFloat32(node.LowValue ?? 0F);
                bs.WriteFloat32(node.HighValue ?? 0F);
                bs.WriteInt32(node.CurveType ?? 1);
                bs.WriteInt32(node.Distribution ?? 1);
            }
        }

        public static Particles Decode(string inFile)
        {
            using (BinaryStream bs = new BinaryStream())
            {
                using (BinaryStream bs_source = new BinaryStream(inFile, FileMode.Open))
                {
                    if (bs_source.PeekInt32() == -559022380)
                    {
                        bs_source.Position += 4;
                        int size = bs_source.ReadInt32();
                        //zlib
                        using (ZLibStream zLibStream = new ZLibStream(bs_source, CompressionMode.Decompress))
                        {
                            zLibStream.CopyTo(bs);
                        }
                    }
                    else
                    {
                        bs_source.CopyTo(bs);
                    }
                }
                Particles particles = new Particles();
                bs.Position = 8;
                int count = bs.ReadInt32();
                particles.Emitters = new ParticlesEmitter[count];
                bs.IdInt32(0x164);
                for (int i = 0; i < count; i++)
                {
                    ParticlesEmitter emitter = new ParticlesEmitter();
                    bs.Position += 4;
                    int tempint = bs.ReadInt32();
                    if (tempint != 0) emitter.ImageCol = tempint;
                    tempint = bs.ReadInt32();
                    if (tempint != 0) emitter.ImageRow = tempint;
                    tempint = bs.ReadInt32();
                    if (tempint != 1) emitter.ImageFrames = tempint;
                    tempint = bs.ReadInt32();
                    if (tempint != 0) emitter.Animated = tempint;
                    emitter.ParticleFlags = bs.ReadInt32();
                    tempint = bs.ReadInt32();
                    if (tempint != 1) emitter.EmitterType = tempint;
                    bs.Position += 8;
                    bs.Position += 22 * 8;
                    bs.Position += 4;
                    tempint = bs.ReadInt32();
                    if (tempint != 0) emitter.Field = new ParticlesField[tempint];
                    bs.Position += 4;
                    tempint = bs.ReadInt32();
                    if (tempint != 0) emitter.SystemField = new ParticlesField[tempint];
                    bs.Position += 16 * 8;
                    particles.Emitters[i] = emitter;
                }
                for (int i = 0; i < count; i++)
                {
                    ParticlesEmitter emitter = particles.Emitters[i];
                    string tempstr = ReadNullableString(bs);
                    if (!string.IsNullOrEmpty(tempstr)) emitter.Image = tempstr;
                    tempstr = ReadNullableString(bs);
                    if (!string.IsNullOrEmpty(tempstr)) emitter.Name = tempstr;
                    emitter.SystemDuration = ReadTrackNodes(bs);
                    tempstr = ReadNullableString(bs);
                    if (!string.IsNullOrEmpty(tempstr)) emitter.OnDuration = tempstr;
                    emitter.CrossFadeDuration = ReadTrackNodes(bs);
                    emitter.SpawnRate = ReadTrackNodes(bs);
                    emitter.SpawnMinActive = ReadTrackNodes(bs);
                    emitter.SpawnMaxActive = ReadTrackNodes(bs);
                    emitter.SpawnMaxLaunched = ReadTrackNodes(bs);
                    emitter.EmitterRadius = ReadTrackNodes(bs);
                    emitter.EmitterOffsetX = ReadTrackNodes(bs);
                    emitter.EmitterOffsetY = ReadTrackNodes(bs);
                    emitter.EmitterBoxX = ReadTrackNodes(bs);
                    emitter.EmitterBoxY = ReadTrackNodes(bs);
                    emitter.EmitterPath = ReadTrackNodes(bs);
                    emitter.EmitterSkewX = ReadTrackNodes(bs);
                    emitter.EmitterSkewY = ReadTrackNodes(bs);
                    emitter.ParticleDuration = ReadTrackNodes(bs);
                    emitter.SystemRed = ReadTrackNodes(bs);
                    emitter.SystemGreen = ReadTrackNodes(bs);
                    emitter.SystemBlue = ReadTrackNodes(bs);
                    emitter.SystemAlpha = ReadTrackNodes(bs);
                    emitter.SystemBrightness = ReadTrackNodes(bs);
                    emitter.LaunchSpeed = ReadTrackNodes(bs);
                    emitter.LaunchAngle = ReadTrackNodes(bs);
                    bs.IdInt32(0x14);
                    ParticlesField[] fields = emitter.Field;
                    if (fields != null)
                    {
                        int fcount = fields.Length;
                        for (int j = 0; j < fcount; j++)
                        {
                            ParticlesField field = new ParticlesField();
                            int type = bs.ReadInt32();
                            if (type != 0) field.FieldType = type;
                            bs.Position += 16;
                            fields[j] = field;
                        }
                        for (int j = 0; j < fcount; j++)
                        {
                            ParticlesField field = fields[j];
                            field.X = ReadTrackNodes(bs);
                            field.Y = ReadTrackNodes(bs);
                        }
                    }
                    bs.IdInt32(0x14);
                    fields = emitter.SystemField;
                    if (fields != null)
                    {
                        int fcount = fields.Length;
                        for (int j = 0; j < fcount; j++)
                        {
                            ParticlesField field = new ParticlesField();
                            int type = bs.ReadInt32();
                            if (type != 0) field.FieldType = type;
                            bs.Position += 16;
                            fields[j] = field;
                        }
                        for (int j = 0; j < fcount; j++)
                        {
                            ParticlesField field = fields[j];
                            field.X = ReadTrackNodes(bs);
                            field.Y = ReadTrackNodes(bs);
                        }
                    }
                    emitter.ParticleRed = ReadTrackNodes(bs);
                    emitter.ParticleGreen = ReadTrackNodes(bs);
                    emitter.ParticleBlue = ReadTrackNodes(bs);
                    emitter.ParticleAlpha = ReadTrackNodes(bs);
                    emitter.ParticleBrightness = ReadTrackNodes(bs);
                    emitter.ParticleSpinAngle = ReadTrackNodes(bs);
                    emitter.ParticleSpinSpeed = ReadTrackNodes(bs);
                    emitter.ParticleScale = ReadTrackNodes(bs);
                    emitter.ParticleStretch = ReadTrackNodes(bs);
                    emitter.CollisionReflect = ReadTrackNodes(bs);
                    emitter.CollisionSpin = ReadTrackNodes(bs);
                    emitter.ClipTop = ReadTrackNodes(bs);
                    emitter.ClipBottom = ReadTrackNodes(bs);
                    emitter.ClipLeft = ReadTrackNodes(bs);
                    emitter.ClipRight = ReadTrackNodes(bs);
                    emitter.AnimationRate = ReadTrackNodes(bs);
                }
                return particles;
            }
        }

        static string ReadNullableString(BinaryStream bs)
        {
            int length = bs.ReadInt32();
            return length > 0 ? bs.ReadString(length) : null;
        }

        static ParticlesTrackNode[] ReadTrackNodes(BinaryStream bs)
        {
            int count = bs.ReadInt32();
            if (count == 0) return null;
            ParticlesTrackNode[] ans = new ParticlesTrackNode[count];
            for (int i = 0; i < count; i++)
            {
                ParticlesTrackNode node = new ParticlesTrackNode();
                node.Time = bs.ReadFloat32();
                float tempfloat = bs.ReadFloat32();
                if (tempfloat != 0) node.LowValue = tempfloat;
                tempfloat = bs.ReadFloat32();
                if (tempfloat != 0) node.HighValue = tempfloat;
                int tempint = bs.ReadInt32();
                if (tempint != 1) node.CurveType = tempint;
                tempint = bs.ReadInt32();
                if (tempint != 1) node.Distribution = tempint;
                ans[i] = node;
            }
            return ans;
        }
    }
}
