using System.Text.Json;
using System.Globalization;

namespace PopStudio.Particles
{
    internal class ParticlesJson
    {
        public static Particles Decode(string inFile)
        {
            string jsondata;
            using (StreamReader sr = new StreamReader(inFile))
            {
                jsondata = sr.ReadToEnd();
            }
            using JsonDocument json = JsonDocument.Parse(jsondata);
            JsonElement root = json.RootElement;
            Particles particles = new Particles();
            JsonElement value;
            if (!root.TryGetProperty("Emitters", out value)) return particles;
            root = value;
            int emitterscount = root.GetArrayLength();
            particles.Emitters = new ParticlesEmitter[emitterscount];
            for (int i = 0; i < emitterscount; i++)
            {
                ParticlesEmitter emitter = new ParticlesEmitter();
                JsonElement node = root[i];
                if (node.TryGetProperty("Name", out value))
                {
                    emitter.Name = value.GetString();
                }
                if (node.TryGetProperty("Image", out value))
                {
                    if (value.ValueKind == JsonValueKind.String)
                    {
                        emitter.Image = value.GetString();
                    }
                    else
                    {
                        emitter.Image = value.GetInt32();
                    }
                }
                if (node.TryGetProperty("ImageResource", out value))
                {
                    emitter.ImagePath = value.GetString();
                }
                if (node.TryGetProperty("ImageCol", out value))
                {
                    emitter.ImageCol = value.GetInt32();
                }
                if (node.TryGetProperty("ImageRow", out value))
                {
                    emitter.ImageRow = value.GetInt32();
                }
                if (node.TryGetProperty("ImageFrames", out value))
                {
                    emitter.ImageFrames = value.GetInt32();
                }
                if (node.TryGetProperty("Animated", out value))
                {
                    emitter.Animated = value.GetInt32();
                }
                if (node.TryGetProperty("RandomLaunchSpin", out value))
                {
                    emitter.ParticleFlags |= value.GetBoolean() ? 0b1 : 0;
                }
                if (node.TryGetProperty("AlignLaunchSpin", out value))
                {
                    emitter.ParticleFlags |= value.GetBoolean() ? 0b10 : 0;
                }
                if (node.TryGetProperty("AlignToPixel", out value))
                {
                    emitter.ParticleFlags |= value.GetBoolean() ? 0b100 : 0;
                }
                if (node.TryGetProperty("SystemLoops", out value))
                {
                    emitter.ParticleFlags |= value.GetBoolean() ? 0b1000 : 0;
                }
                if (node.TryGetProperty("ParticleLoops", out value))
                {
                    emitter.ParticleFlags |= value.GetBoolean() ? 0b10000 : 0;
                }
                if (node.TryGetProperty("ParticlesDontFollow", out value))
                {
                    emitter.ParticleFlags |= value.GetBoolean() ? 0b100000 : 0;
                }
                if (node.TryGetProperty("RandomStartTime", out value))
                {
                    emitter.ParticleFlags |= value.GetBoolean() ? 0b1000000 : 0;
                }
                if (node.TryGetProperty("DieIfOverloaded", out value))
                {
                    emitter.ParticleFlags |= value.GetBoolean() ? 0b10000000 : 0;
                }
                if (node.TryGetProperty("Additive", out value))
                {
                    emitter.ParticleFlags |= value.GetBoolean() ? 0b100000000 : 0;
                }
                if (node.TryGetProperty("FullScreen", out value))
                {
                    emitter.ParticleFlags |= value.GetBoolean() ? 0b1000000000 : 0;
                }
                if (node.TryGetProperty("SoftwareOnly", out value))
                {
                    emitter.ParticleFlags |= value.GetBoolean() ? 0b10000000000 : 0;
                }
                if (node.TryGetProperty("HardwareOnly", out value))
                {
                    emitter.ParticleFlags |= value.GetBoolean() ? 0b100000000000 : 0;
                }
                if (node.TryGetProperty("EmitterType", out value))
                {
                    if (value.ValueKind == JsonValueKind.String)
                    {
                        emitter.EmitterType = EmitterEDic[value.GetString()];
                    }
                    else
                    {
                        emitter.EmitterType = value.GetInt32();
                    }
                }
                if (node.TryGetProperty("OnDuration", out value))
                {
                    emitter.OnDuration = value.GetString();
                }
                if (node.TryGetProperty("SystemDuration", out value))
                {
                    emitter.SystemDuration = ReadTrackNode(value);
                }
                if (node.TryGetProperty("CrossFadeDuration", out value))
                {
                    emitter.CrossFadeDuration = ReadTrackNode(value);
                }
                if (node.TryGetProperty("SpawnRate", out value))
                {
                    emitter.SpawnRate = ReadTrackNode(value);
                }
                if (node.TryGetProperty("SpawnMinActive", out value))
                {
                    emitter.SpawnMinActive = ReadTrackNode(value);
                }
                if (node.TryGetProperty("SpawnMaxActive", out value))
                {
                    emitter.SpawnMaxActive = ReadTrackNode(value);
                }
                if (node.TryGetProperty("SpawnMaxLaunched", out value))
                {
                    emitter.SpawnMaxLaunched = ReadTrackNode(value);
                }
                if (node.TryGetProperty("EmitterRadius", out value))
                {
                    emitter.EmitterRadius = ReadTrackNode(value);
                }
                if (node.TryGetProperty("EmitterOffsetX", out value))
                {
                    emitter.EmitterOffsetX = ReadTrackNode(value);
                }
                if (node.TryGetProperty("EmitterOffsetY", out value))
                {
                    emitter.EmitterOffsetY = ReadTrackNode(value);
                }
                if (node.TryGetProperty("EmitterBoxX", out value))
                {
                    emitter.EmitterBoxX = ReadTrackNode(value);
                }
                if (node.TryGetProperty("EmitterBoxY", out value))
                {
                    emitter.EmitterBoxY = ReadTrackNode(value);
                }
                if (node.TryGetProperty("EmitterPath", out value))
                {
                    emitter.EmitterPath = ReadTrackNode(value);
                }
                if (node.TryGetProperty("EmitterSkewX", out value))
                {
                    emitter.EmitterSkewX = ReadTrackNode(value);
                }
                if (node.TryGetProperty("EmitterSkewY", out value))
                {
                    emitter.EmitterSkewY = ReadTrackNode(value);
                }
                if (node.TryGetProperty("ParticleDuration", out value))
                {
                    emitter.ParticleDuration = ReadTrackNode(value);
                }
                if (node.TryGetProperty("SystemRed", out value))
                {
                    emitter.SystemRed = ReadTrackNode(value);
                }
                if (node.TryGetProperty("SystemGreen", out value))
                {
                    emitter.SystemGreen = ReadTrackNode(value);
                }
                if (node.TryGetProperty("SystemBlue", out value))
                {
                    emitter.SystemBlue = ReadTrackNode(value);
                }
                if (node.TryGetProperty("SystemAlpha", out value))
                {
                    emitter.SystemAlpha = ReadTrackNode(value);
                }
                if (node.TryGetProperty("SystemBrightness", out value))
                {
                    emitter.SystemBrightness = ReadTrackNode(value);
                }
                if (node.TryGetProperty("LaunchSpeed", out value))
                {
                    emitter.LaunchSpeed = ReadTrackNode(value);
                }
                if (node.TryGetProperty("LaunchAngle", out value))
                {
                    emitter.LaunchAngle = ReadTrackNode(value);
                }
                if (node.TryGetProperty("Field", out value))
                {
                    emitter.Field = ReadField(value);
                }
                if (node.TryGetProperty("SystemField", out value))
                {
                    emitter.SystemField = ReadField(value);
                }
                if (node.TryGetProperty("ParticleRed", out value))
                {
                    emitter.ParticleRed = ReadTrackNode(value);
                }
                if (node.TryGetProperty("ParticleGreen", out value))
                {
                    emitter.ParticleGreen = ReadTrackNode(value);
                }
                if (node.TryGetProperty("ParticleBlue", out value))
                {
                    emitter.ParticleBlue = ReadTrackNode(value);
                }
                if (node.TryGetProperty("ParticleAlpha", out value))
                {
                    emitter.ParticleAlpha = ReadTrackNode(value);
                }
                if (node.TryGetProperty("ParticleBrightness", out value))
                {
                    emitter.ParticleBrightness = ReadTrackNode(value);
                }
                if (node.TryGetProperty("ParticleSpinAngle", out value))
                {
                    emitter.ParticleSpinAngle = ReadTrackNode(value);
                }
                if (node.TryGetProperty("ParticleSpinSpeed", out value))
                {
                    emitter.ParticleSpinSpeed = ReadTrackNode(value);
                }
                if (node.TryGetProperty("ParticleScale", out value))
                {
                    emitter.ParticleScale = ReadTrackNode(value);
                }
                if (node.TryGetProperty("ParticleStretch", out value))
                {
                    emitter.ParticleStretch = ReadTrackNode(value);
                }
                if (node.TryGetProperty("CollisionReflect", out value))
                {
                    emitter.CollisionReflect = ReadTrackNode(value);
                }
                if (node.TryGetProperty("CollisionSpin", out value))
                {
                    emitter.CollisionSpin = ReadTrackNode(value);
                }
                if (node.TryGetProperty("ClipTop", out value))
                {
                    emitter.ClipTop = ReadTrackNode(value);
                }
                if (node.TryGetProperty("ClipBottom", out value))
                {
                    emitter.ClipBottom = ReadTrackNode(value);
                }
                if (node.TryGetProperty("ClipLeft", out value))
                {
                    emitter.ClipLeft = ReadTrackNode(value);
                }
                if (node.TryGetProperty("ClipRight", out value))
                {
                    emitter.ClipRight = ReadTrackNode(value);
                }
                if (node.TryGetProperty("AnimationRate", out value))
                {
                    emitter.AnimationRate = ReadTrackNode(value);
                }
                particles.Emitters[i] = emitter;
            }
            return particles;
        }

        static ParticlesField[] ReadField(JsonElement root)
        {
            int count = root.GetArrayLength();
            ParticlesField[] ans = new ParticlesField[count];
            for (int i = 0; i < count; i++)
            {
                ParticlesField field = new ParticlesField();
                JsonElement jsonnode = root[i];
                JsonElement value;
                if (jsonnode.TryGetProperty("FieldType", out value))
                {
                    if (value.ValueKind == JsonValueKind.String)
                    {
                        field.FieldType = FieldEDic[value.GetString()];
                    }
                    else
                    {
                        field.FieldType = value.GetInt32();
                    }
                }
                if (jsonnode.TryGetProperty("X", out value))
                {
                    field.X = ReadTrackNode(value);
                }
                if (jsonnode.TryGetProperty("Y", out value))
                {
                    field.Y = ReadTrackNode(value);
                }
                ans[i] = field;
            }
            return ans;
        }

        static ParticlesTrackNode[] ReadTrackNode(JsonElement root)
        {
            int count = root.GetArrayLength();
            ParticlesTrackNode[] track = new ParticlesTrackNode[count];
            for (int i = 0; i < count; i++)
            {
                ParticlesTrackNode node = new ParticlesTrackNode();
                JsonElement jsonnode = root[i];
                JsonElement value;
                if (jsonnode.TryGetProperty("Time", out value))
                {
                    node.Time = value.GetSingle();
                }
                if (jsonnode.TryGetProperty("LowValue", out value))
                {
                    node.LowValue = value.GetSingle();
                }
                if (jsonnode.TryGetProperty("HighValue", out value))
                {
                    node.HighValue = value.GetSingle();
                }
                if (jsonnode.TryGetProperty("CurveType", out value))
                {
                    if (value.ValueKind == JsonValueKind.String)
                    {
                        node.CurveType = TrailEDic[value.GetString()];
                    }
                    else
                    {
                        node.CurveType = value.GetInt32();
                    }
                }
                if (jsonnode.TryGetProperty("Distribution", out value))
                {
                    if (value.ValueKind == JsonValueKind.String)
                    {
                        node.Distribution = TrailEDic[value.GetString()];
                    }
                    else
                    {
                        node.Distribution = value.GetInt32();
                    }
                }
                track[i] = node;
            }
            return track;
        }

        public static void Encode(Particles particles, string outFile)
        {
            using (StreamWriter sw = new StreamWriter(outFile, false))
            {
                ParticlesEmitter[] emitters = particles.Emitters;
                if (emitters == null)
                {
                    sw.Write("{}");
                }
                else if (emitters.Length == 0)
                {
                    sw.Write("{\n    \"Emitters\":[]\n}");
                }
                else
                {
                    sw.Write("{\n    \"Emitters\":[");
                    int count = emitters.Length;
                    for (int i = 0; i < count; i++)
                    {
                        if (i != 0) sw.Write(",\n        ");
                        sw.Write('{');
                        WriteEmitter(emitters[i], sw);
                        sw.Write("\n        }");
                    }
                    sw.Write("\n    ]\n}");
                }
            }
        }

        static void WriteEmitter(ParticlesEmitter emitter, StreamWriter sw)
        {
            bool first = true;
            if (emitter.Name != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"Name\":\"" + emitter.Name.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"");
            }
            if (emitter.Image != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                if (emitter.Image is int)
                {
                    sw.Write("\n            \"Image\":" + ((int)emitter.Image));
                }
                else
                {
                    sw.Write("\n            \"Image\":\"" + ((string)emitter.Image).Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"");
                }
            }
            if (emitter.ImagePath != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"ImageResource\":\"" + emitter.ImagePath.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"");
            }
            if (emitter.ImageCol != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"ImageCol\":" + emitter.ImageCol);
            }
            if (emitter.ImageRow != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"ImageRow\":" + emitter.ImageRow);
            }
            if (emitter.ImageFrames != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"ImageFrames\":" + emitter.ImageFrames);
            }
            if (emitter.Animated != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"Animated\":" + emitter.Animated);
            }
            int flags = emitter.ParticleFlags;
            if ((flags & 0b1) != 0)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"RandomLaunchSpin\":true");
            }
            if ((flags & 0b10) != 0)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"AlignLaunchSpin\":true");
            }
            if ((flags & 0b100) != 0)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"AlignToPixel\":true");
            }
            if ((flags & 0b1000) != 0)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"SystemLoops\":true");
            }
            if ((flags & 0b10000) != 0)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"ParticleLoops\":true");
            }
            if ((flags & 0b100000) != 0)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"ParticlesDontFollow\":true");
            }
            if ((flags & 0b1000000) != 0)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"RandomStartTime\":true");
            }
            if ((flags & 0b10000000) != 0)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"DieIfOverloaded\":true");
            }
            if ((flags & 0b100000000) != 0)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"Additive\":true");
            }
            if ((flags & 0b1000000000) != 0)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"FullScreen\":true");
            }
            if ((flags & 0b10000000000) != 0)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"SoftwareOnly\":true");
            }
            if ((flags & 0b100000000000) != 0)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"HardwareOnly\":true");
            }
            if (emitter.EmitterType != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                int type = emitter.EmitterType ?? -1;
                if (type < 0 || type >= 5)
                {
                    sw.Write("\n            \"EmitterType\":" + type);
                }
                else
                {
                    sw.Write("\n            \"EmitterType\":\"" + EmitterType[type] + "\"");
                }
            }
            if (emitter.OnDuration != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"OnDuration\":\"" + emitter.OnDuration.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"");
            }
            if (emitter.SystemDuration != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"SystemDuration\":[");
                if (emitter.SystemDuration.Length == 0)
                {
                    sw.Write(']');
                }
                else
                {
                    WriteTrackNode(emitter.SystemDuration, sw, "                ");
                    sw.Write("\n            ]");
                }
            }
            if (emitter.CrossFadeDuration != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"CrossFadeDuration\":[");
                if (emitter.CrossFadeDuration.Length == 0)
                {
                    sw.Write(']');
                }
                else
                {
                    WriteTrackNode(emitter.CrossFadeDuration, sw, "                ");
                    sw.Write("\n            ]");
                }
            }
            if (emitter.SpawnRate != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"SpawnRate\":[");
                if (emitter.SpawnRate.Length == 0)
                {
                    sw.Write(']');
                }
                else
                {
                    WriteTrackNode(emitter.SpawnRate, sw, "                ");
                    sw.Write("\n            ]");
                }
            }
            if (emitter.SpawnMinActive != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"SpawnMinActive\":[");
                if (emitter.SpawnMinActive.Length == 0)
                {
                    sw.Write(']');
                }
                else
                {
                    WriteTrackNode(emitter.SpawnMinActive, sw, "                ");
                    sw.Write("\n            ]");
                }
            }
            if (emitter.SpawnMaxActive != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"SpawnMaxActive\":[");
                if (emitter.SpawnMaxActive.Length == 0)
                {
                    sw.Write(']');
                }
                else
                {
                    WriteTrackNode(emitter.SpawnMaxActive, sw, "                ");
                    sw.Write("\n            ]");
                }
            }
            if (emitter.SpawnMaxLaunched != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"SpawnMaxLaunched\":[");
                if (emitter.SpawnMaxLaunched.Length == 0)
                {
                    sw.Write(']');
                }
                else
                {
                    WriteTrackNode(emitter.SpawnMaxLaunched, sw, "                ");
                    sw.Write("\n            ]");
                }
            }
            if (emitter.EmitterRadius != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"EmitterRadius\":[");
                if (emitter.EmitterRadius.Length == 0)
                {
                    sw.Write(']');
                }
                else
                {
                    WriteTrackNode(emitter.EmitterRadius, sw, "                ");
                    sw.Write("\n            ]");
                }
            }
            if (emitter.EmitterOffsetX != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"EmitterOffsetX\":[");
                if (emitter.EmitterOffsetX.Length == 0)
                {
                    sw.Write(']');
                }
                else
                {
                    WriteTrackNode(emitter.EmitterOffsetX, sw, "                ");
                    sw.Write("\n            ]");
                }
            }
            if (emitter.EmitterOffsetY != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"EmitterOffsetY\":[");
                if (emitter.EmitterOffsetY.Length == 0)
                {
                    sw.Write(']');
                }
                else
                {
                    WriteTrackNode(emitter.EmitterOffsetY, sw, "                ");
                    sw.Write("\n            ]");
                }
            }
            if (emitter.EmitterBoxX != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"EmitterBoxX\":[");
                if (emitter.EmitterBoxX.Length == 0)
                {
                    sw.Write(']');
                }
                else
                {
                    WriteTrackNode(emitter.EmitterBoxX, sw, "                ");
                    sw.Write("\n            ]");
                }
            }
            if (emitter.EmitterBoxY != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"EmitterBoxY\":[");
                if (emitter.EmitterBoxY.Length == 0)
                {
                    sw.Write(']');
                }
                else
                {
                    WriteTrackNode(emitter.EmitterBoxY, sw, "                ");
                    sw.Write("\n            ]");
                }
            }
            if (emitter.EmitterPath != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"EmitterPath\":[");
                if (emitter.EmitterPath.Length == 0)
                {
                    sw.Write(']');
                }
                else
                {
                    WriteTrackNode(emitter.EmitterPath, sw, "                ");
                    sw.Write("\n            ]");
                }
            }
            if (emitter.EmitterSkewX != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"EmitterSkewX\":[");
                if (emitter.EmitterSkewX.Length == 0)
                {
                    sw.Write(']');
                }
                else
                {
                    WriteTrackNode(emitter.EmitterSkewX, sw, "                ");
                    sw.Write("\n            ]");
                }
            }
            if (emitter.EmitterSkewY != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"EmitterSkewY\":[");
                if (emitter.EmitterSkewY.Length == 0)
                {
                    sw.Write(']');
                }
                else
                {
                    WriteTrackNode(emitter.EmitterSkewY, sw, "                ");
                    sw.Write("\n            ]");
                }
            }
            if (emitter.ParticleDuration != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"ParticleDuration\":[");
                if (emitter.ParticleDuration.Length == 0)
                {
                    sw.Write(']');
                }
                else
                {
                    WriteTrackNode(emitter.ParticleDuration, sw, "                ");
                    sw.Write("\n            ]");
                }
            }
            if (emitter.SystemRed != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"SystemRed\":[");
                if (emitter.SystemRed.Length == 0)
                {
                    sw.Write(']');
                }
                else
                {
                    WriteTrackNode(emitter.SystemRed, sw, "                ");
                    sw.Write("\n            ]");
                }
            }
            if (emitter.SystemGreen != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"SystemGreen\":[");
                if (emitter.SystemGreen.Length == 0)
                {
                    sw.Write(']');
                }
                else
                {
                    WriteTrackNode(emitter.SystemGreen, sw, "                ");
                    sw.Write("\n            ]");
                }
            }
            if (emitter.SystemBlue != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"SystemBlue\":[");
                if (emitter.SystemBlue.Length == 0)
                {
                    sw.Write(']');
                }
                else
                {
                    WriteTrackNode(emitter.SystemBlue, sw, "                ");
                    sw.Write("\n            ]");
                }
            }
            if (emitter.SystemAlpha != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"SystemAlpha\":[");
                if (emitter.SystemAlpha.Length == 0)
                {
                    sw.Write(']');
                }
                else
                {
                    WriteTrackNode(emitter.SystemAlpha, sw, "                ");
                    sw.Write("\n            ]");
                }
            }
            if (emitter.SystemBrightness != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"SystemBrightness\":[");
                if (emitter.SystemBrightness.Length == 0)
                {
                    sw.Write(']');
                }
                else
                {
                    WriteTrackNode(emitter.SystemBrightness, sw, "                ");
                    sw.Write("\n            ]");
                }
            }
            if (emitter.LaunchSpeed != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"LaunchSpeed\":[");
                if (emitter.LaunchSpeed.Length == 0)
                {
                    sw.Write(']');
                }
                else
                {
                    WriteTrackNode(emitter.LaunchSpeed, sw, "                ");
                    sw.Write("\n            ]");
                }
            }
            if (emitter.LaunchAngle != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"LaunchAngle\":[");
                if (emitter.LaunchAngle.Length == 0)
                {
                    sw.Write(']');
                }
                else
                {
                    WriteTrackNode(emitter.LaunchAngle, sw, "                ");
                    sw.Write("\n            ]");
                }
            }
            if (emitter.Field != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"Field\":[");
                if (emitter.Field.Length == 0)
                {
                    sw.Write(']');
                }
                else
                {
                    WriteFields(emitter.Field, sw);
                    sw.Write("\n            ]");
                }
            }
            if (emitter.SystemField != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"SystemField\":[");
                if (emitter.SystemField.Length == 0)
                {
                    sw.Write(']');
                }
                else
                {
                    WriteFields(emitter.SystemField, sw);
                    sw.Write("\n            ]");
                }
            }
            if (emitter.ParticleRed != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"ParticleRed\":[");
                if (emitter.ParticleRed.Length == 0)
                {
                    sw.Write(']');
                }
                else
                {
                    WriteTrackNode(emitter.ParticleRed, sw, "                ");
                    sw.Write("\n            ]");
                }
            }
            if (emitter.ParticleGreen != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"ParticleGreen\":[");
                if (emitter.ParticleGreen.Length == 0)
                {
                    sw.Write(']');
                }
                else
                {
                    WriteTrackNode(emitter.ParticleGreen, sw, "                ");
                    sw.Write("\n            ]");
                }
            }
            if (emitter.ParticleBlue != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"ParticleBlue\":[");
                if (emitter.ParticleBlue.Length == 0)
                {
                    sw.Write(']');
                }
                else
                {
                    WriteTrackNode(emitter.ParticleBlue, sw, "                ");
                    sw.Write("\n            ]");
                }
            }
            if (emitter.ParticleAlpha != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"ParticleAlpha\":[");
                if (emitter.ParticleAlpha.Length == 0)
                {
                    sw.Write(']');
                }
                else
                {
                    WriteTrackNode(emitter.ParticleAlpha, sw, "                ");
                    sw.Write("\n            ]");
                }
            }
            if (emitter.ParticleBrightness != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"ParticleBrightness\":[");
                if (emitter.ParticleBrightness.Length == 0)
                {
                    sw.Write(']');
                }
                else
                {
                    WriteTrackNode(emitter.ParticleBrightness, sw, "                ");
                    sw.Write("\n            ]");
                }
            }
            if (emitter.ParticleSpinAngle != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"ParticleSpinAngle\":[");
                if (emitter.ParticleSpinAngle.Length == 0)
                {
                    sw.Write(']');
                }
                else
                {
                    WriteTrackNode(emitter.ParticleSpinAngle, sw, "                ");
                    sw.Write("\n            ]");
                }
            }
            if (emitter.ParticleSpinSpeed != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"ParticleSpinSpeed\":[");
                if (emitter.ParticleSpinSpeed.Length == 0)
                {
                    sw.Write(']');
                }
                else
                {
                    WriteTrackNode(emitter.ParticleSpinSpeed, sw, "                ");
                    sw.Write("\n            ]");
                }
            }
            if (emitter.ParticleScale != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"ParticleScale\":[");
                if (emitter.ParticleScale.Length == 0)
                {
                    sw.Write(']');
                }
                else
                {
                    WriteTrackNode(emitter.ParticleScale, sw, "                ");
                    sw.Write("\n            ]");
                }
            }
            if (emitter.ParticleStretch != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"ParticleStretch\":[");
                if (emitter.ParticleStretch.Length == 0)
                {
                    sw.Write(']');
                }
                else
                {
                    WriteTrackNode(emitter.ParticleStretch, sw, "                ");
                    sw.Write("\n            ]");
                }
            }
            if (emitter.CollisionReflect != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"CollisionReflect\":[");
                if (emitter.CollisionReflect.Length == 0)
                {
                    sw.Write(']');
                }
                else
                {
                    WriteTrackNode(emitter.CollisionReflect, sw, "                ");
                    sw.Write("\n            ]");
                }
            }
            if (emitter.CollisionSpin != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"CollisionSpin\":[");
                if (emitter.CollisionSpin.Length == 0)
                {
                    sw.Write(']');
                }
                else
                {
                    WriteTrackNode(emitter.CollisionSpin, sw, "                ");
                    sw.Write("\n            ]");
                }
            }
            if (emitter.ClipTop != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"ClipTop\":[");
                if (emitter.ClipTop.Length == 0)
                {
                    sw.Write(']');
                }
                else
                {
                    WriteTrackNode(emitter.ClipTop, sw, "                ");
                    sw.Write("\n            ]");
                }
            }
            if (emitter.ClipBottom != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"ClipBottom\":[");
                if (emitter.ClipBottom.Length == 0)
                {
                    sw.Write(']');
                }
                else
                {
                    WriteTrackNode(emitter.ClipBottom, sw, "                ");
                    sw.Write("\n            ]");
                }
            }
            if (emitter.ClipLeft != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"ClipLeft\":[");
                if (emitter.ClipLeft.Length == 0)
                {
                    sw.Write(']');
                }
                else
                {
                    WriteTrackNode(emitter.ClipLeft, sw, "                ");
                    sw.Write("\n            ]");
                }
            }
            if (emitter.ClipRight != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"ClipRight\":[");
                if (emitter.ClipRight.Length == 0)
                {
                    sw.Write(']');
                }
                else
                {
                    WriteTrackNode(emitter.ClipRight, sw, "                ");
                    sw.Write("\n            ]");
                }
            }
            if (emitter.AnimationRate != null)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sw.Write(',');
                }
                sw.Write("\n            \"AnimationRate\":[");
                if (emitter.AnimationRate.Length == 0)
                {
                    sw.Write(']');
                }
                else
                {
                    WriteTrackNode(emitter.AnimationRate, sw, "                ");
                    sw.Write("\n            ]");
                }
            }
        }

        static void WriteFields(ParticlesField[] fields, StreamWriter sw)
        {
            int length = fields.Length;
            string space = "                ";
            string space2 = "                    ";
            for (int i = 0; i < length; i++)
            {
                if (i != 0)
                {
                    sw.Write(",\n");
                    sw.Write(space);
                }
                sw.Write('{');
                ParticlesField node = fields[i];
                bool first = true;
                if (node.FieldType != null)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        sw.Write(',');
                    }
                    sw.Write('\n');
                    sw.Write(space2);
                    sw.Write("\"FieldType\":");
                    int t = node.FieldType ?? -1;
                    if (t >= 12 || t < 0)
                    {
                        sw.Write(t);
                    }
                    else
                    {
                        sw.Write('"');
                        sw.Write(FieldType[t]);
                        sw.Write('"');
                    }
                }
                if (node.X != null)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        sw.Write(',');
                    }
                    sw.Write('\n');
                    sw.Write(space2);
                    sw.Write("\"X\":[");
                    if (node.X.Length == 0)
                    {
                        sw.Write(']');
                    }
                    else
                    {
                        WriteTrackNode(node.X, sw, space2 + "    ");
                        sw.Write('\n');
                        sw.Write(space2);
                        sw.Write(']');
                    }
                }
                if (node.Y != null)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        sw.Write(',');
                    }
                    sw.Write('\n');
                    sw.Write(space2);
                    sw.Write("\"Y\":[");
                    if (node.Y.Length == 0)
                    {
                        sw.Write(']');
                    }
                    else
                    {
                        WriteTrackNode(node.Y, sw, space2 + "    ");
                        sw.Write('\n');
                        sw.Write(space2);
                        sw.Write(']');
                    }
                }
                if (!first)
                {
                    sw.Write('\n');
                    sw.Write(space);
                }
                sw.Write('}');
            }
        }

        static void WriteTrackNode(ParticlesTrackNode[] track, StreamWriter sw, string space)
        {
            int length = track.Length;
            string space2 = space + "    ";
            for (int i = 0; i < length; i++)
            {
                if (i != 0)
                {
                    sw.Write(",\n");
                    sw.Write(space);
                }
                sw.Write('{');
                ParticlesTrackNode node = track[i];
                sw.Write('\n');
                sw.Write(space2);
                sw.Write("\"Time\":");
                sw.Write(node.Time.ToString(CultureInfo.InvariantCulture));
                if (node.LowValue != null)
                {
                    sw.Write(',');
                    sw.Write('\n');
                    sw.Write(space2);
                    sw.Write("\"LowValue\":");
                    sw.Write(node.LowValue.Value.ToString(CultureInfo.InvariantCulture));
                }
                if (node.HighValue != null)
                {
                    sw.Write(',');
                    sw.Write('\n');
                    sw.Write(space2);
                    sw.Write("\"HighValue\":");
                    sw.Write(node.HighValue.Value.ToString(CultureInfo.InvariantCulture));
                }
                if (node.CurveType != null)
                {
                    sw.Write(',');
                    sw.Write('\n');
                    sw.Write(space2);
                    sw.Write("\"CurveType\":");
                    int index = node.CurveType ?? 1;
                    if (index < 0 || index >= 14)
                    {
                        sw.Write(index);
                    }
                    else
                    {
                        sw.Write('\"');
                        sw.Write(TrailEnum[index]);
                        sw.Write('\"');
                    }
                }
                if (node.Distribution != null)
                {
                    sw.Write(',');
                    sw.Write('\n');
                    sw.Write(space2);
                    sw.Write("\"Distribution\":");
                    int index = node.Distribution ?? 1;
                    if (index < 0 || index >= 14)
                    {
                        sw.Write(index);
                    }
                    else
                    {
                        sw.Write('\"');
                        sw.Write(TrailEnum[index]);
                        sw.Write('\"');
                    }
                }
                sw.Write('\n');
                sw.Write(space);
                sw.Write('}');
            }
        }

        static Dictionary<string, int> EmitterEDic = new Dictionary<string, int> { { "Circle", 0 }, { "Box", 1 }, { "BoxPath", 2 }, { "CirclePath", 3 }, { "CircleEvenSpacing", 4 } };

        static string[] EmitterType = new string[5] { "Circle", "Box", "BoxPath", "CirclePath", "CircleEvenSpacing" };

        static Dictionary<string, int> TrailEDic = new Dictionary<string, int> { { "Constant", 0 }, { "Linear", 1 }, { "EaseIn", 2 }, { "EaseOut", 3 }, { "EaseInOut", 4 }, { "EaseInOutWeak", 5 }, { "FastInOut", 6 }, { "FastInOutWeak", 7 }, { "WeakFastInOut", 8 }, { "Bounce", 9 }, { "BounceFastMiddle", 10 }, { "BounceSlowMiddle", 11 }, { "SinWave", 12 }, { "EaseSinWave", 13 } };

        static string[] TrailEnum = new string[14] { "Constant", "Linear", "EaseIn", "EaseOut", "EaseInOut", "EaseInOutWeak", "FastInOut", "FastInOutWeak", "WeakFastInOut", "Bounce", "BounceFastMiddle", "BounceSlowMiddle", "SinWave", "EaseSinWave" };

        static Dictionary<string, int> FieldEDic = new Dictionary<string, int> { { "Invalid", 0 }, { "Friction", 1 }, { "Acceleration", 2 }, { "Attractor", 3 }, { "MaxVelocity", 4 }, { "Velocity", 5 }, { "Position", 6 }, { "SystemPosition", 7 }, { "GroundConstraint", 8 }, { "Shake", 9 }, { "Circle", 10 }, { "Away", 11 } };

        static string[] FieldType = new string[12] { "Invalid", "Friction", "Acceleration", "Attractor", "MaxVelocity", "Velocity", "Position", "SystemPosition", "GroundConstraint", "Shake", "Circle", "Away" };
    }
}
