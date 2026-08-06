using System.Text.Json;
using System.Globalization;

namespace PopStudio.Trail
{
    internal class TrailJson
    {
        public static Trail Decode(string inFile)
        {
            string jsondata;
            using (StreamReader sr = new StreamReader(inFile))
            {
                jsondata = sr.ReadToEnd();
            }
            using JsonDocument json = JsonDocument.Parse(jsondata);
            JsonElement root = json.RootElement;
            Trail trail = new Trail();
            JsonElement value;
            if (root.TryGetProperty("MaxPoints", out value))
            {
                trail.MaxPoints = value.GetInt32();
            }
            if (root.TryGetProperty("MinPointDistance", out value))
            {
                trail.MinPointDistance = value.GetSingle();
            }
            if (root.TryGetProperty("Loops", out value))
            {
                trail.TrailFlags |= value.GetBoolean() ? 0b1 : 0b0;
            }
            if (root.TryGetProperty("Image", out value))
            {
                if (value.ValueKind == JsonValueKind.String)
                {
                    trail.Image = value.GetString();
                }
                else
                {
                    trail.Image = value.GetInt32();
                }
            }
            if (root.TryGetProperty("ImageResource", out value))
            {
                trail.ImageResource = value.GetString();
            }
            if (root.TryGetProperty("WidthOverLength", out value))
            {
                trail.WidthOverLength = ReadTrackNode(value);
            }
            if (root.TryGetProperty("WidthOverTime", out value))
            {
                trail.WidthOverTime = ReadTrackNode(value);
            }
            if (root.TryGetProperty("AlphaOverLength", out value))
            {
                trail.AlphaOverLength = ReadTrackNode(value);
            }
            if (root.TryGetProperty("AlphaOverTime", out value))
            {
                trail.AlphaOverTime = ReadTrackNode(value);
            }
            if (root.TryGetProperty("TrailDuration", out value))
            {
                trail.TrailDuration = ReadTrackNode(value);
            }
            return trail;
        }

        static TrailTrackNode[] ReadTrackNode(JsonElement root)
        {
            int count = root.GetArrayLength();
            TrailTrackNode[] track = new TrailTrackNode[count];
            for (int i = 0; i < count; i++)
            {
                TrailTrackNode node = new TrailTrackNode();
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

        public static void Encode(Trail trail, string outFile)
        {
            using (StreamWriter sw = new StreamWriter(outFile, false))
            {
                sw.Write('{');
                sw.Write('\n');
                bool first = true;
                if (trail.MaxPoints != null)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        sw.Write(',');
                        sw.Write('\n');
                    }
                    sw.Write("    \"MaxPoints\":");
                    sw.Write(trail.MaxPoints);
                }
                if (trail.MinPointDistance != null)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        sw.Write(',');
                        sw.Write('\n');
                    }
                    sw.Write("    \"MinPointDistance\":");
                    sw.Write(trail.MinPointDistance.Value.ToString(CultureInfo.InvariantCulture));
                }
                if ((trail.TrailFlags & 0b1) != 0)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        sw.Write(',');
                        sw.Write('\n');
                    }
                    sw.Write("    \"Loops\":true");
                }
                if (trail.Image != null)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        sw.Write(',');
                        sw.Write('\n');
                    }
                    sw.Write("    \"Image\":");
                    if (trail.Image is int)
                    {
                        sw.Write((int)trail.Image);
                    }
                    else
                    {
                        sw.Write('\"');
                        sw.Write(((string)trail.Image).Replace("\\", "\\\\").Replace("\"", "\\\""));
                        sw.Write('\"');
                    }
                }
                if (trail.ImageResource != null)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        sw.Write(',');
                        sw.Write('\n');
                    }
                    sw.Write("    \"ImageResource\":\"");
                    sw.Write(trail.ImageResource.Replace("\\", "\\\\").Replace("\"", "\\\""));
                    sw.Write('\"');
                }
                if (trail.WidthOverLength != null && trail.WidthOverLength.Length > 0)
                {
                    WriteTrackNode(trail.WidthOverLength, "WidthOverLength", sw, ref first);
                }
                if (trail.WidthOverTime != null && trail.WidthOverTime.Length > 0)
                {
                    WriteTrackNode(trail.WidthOverTime, "WidthOverTime", sw, ref first);
                }
                if (trail.AlphaOverLength != null && trail.AlphaOverLength.Length > 0)
                {
                    WriteTrackNode(trail.AlphaOverLength, "AlphaOverLength", sw, ref first);
                }
                if (trail.AlphaOverTime != null && trail.AlphaOverTime.Length > 0)
                {
                    WriteTrackNode(trail.AlphaOverTime, "AlphaOverTime", sw, ref first);
                }
                if (trail.TrailDuration != null && trail.TrailDuration.Length > 0)
                {
                    WriteTrackNode(trail.TrailDuration, "TrailDuration", sw, ref first);
                }
                sw.Write('\n');
                sw.WriteLine('}');
            }
        }

        static void WriteTrackNode(TrailTrackNode[] track, string name, StreamWriter sw, ref bool first)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                sw.Write(',');
                sw.Write('\n');
            }
            sw.Write("    \"" + name + "\":[");
            int length = track.Length;
            for (int i = 0; i < length; i++)
            {
                if (i != 0) sw.Write(',');
                sw.Write("\n        {");
                TrailTrackNode node = track[i];
                sw.Write('\n');
                sw.Write("            \"Time\":");
                sw.Write(node.Time.ToString(CultureInfo.InvariantCulture));
                if (node.LowValue != null)
                {
                    sw.Write(',');
                    sw.Write('\n');
                    sw.Write("            \"LowValue\":");
                    sw.Write(node.LowValue.Value.ToString(CultureInfo.InvariantCulture));
                }
                if (node.HighValue != null)
                {
                    sw.Write(',');
                    sw.Write('\n');
                    sw.Write("            \"HighValue\":");
                    sw.Write(node.HighValue.Value.ToString(CultureInfo.InvariantCulture));
                }
                if (node.CurveType != null)
                {
                    sw.Write(',');
                    sw.Write('\n');
                    sw.Write("            \"CurveType\":");
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
                    sw.Write("            \"Distribution\":");
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
                sw.Write("\n        }");
            }
            if (length == 0)
            {
                sw.Write(']');
            }
            else
            {
                sw.Write("\n    ]");
            }
        }

        static Dictionary<string, int> TrailEDic = new Dictionary<string, int> { { "Constant", 0 }, { "Linear", 1 }, { "EaseIn", 2 }, { "EaseOut", 3 }, { "EaseInOut", 4 }, { "EaseInOutWeak", 5 }, { "FastInOut", 6 }, { "FastInOutWeak", 7 }, { "WeakFastInOut", 8 }, { "Bounce", 9 }, { "BounceFastMiddle", 10 }, { "BounceSlowMiddle", 11 }, { "SinWave", 12 }, { "EaseSinWave", 13 } };

        static string[] TrailEnum = new string[14] { "Constant", "Linear", "EaseIn", "EaseOut", "EaseInOut", "EaseInOutWeak", "FastInOut", "FastInOutWeak", "WeakFastInOut", "Bounce", "BounceFastMiddle", "BounceSlowMiddle", "SinWave", "EaseSinWave" };
    }
}
