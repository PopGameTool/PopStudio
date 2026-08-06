using System.Xml;
using System.Globalization;

namespace PopStudio.Trail
{
    internal class RawXml
    {
        public static Trail Decode(string inFile)
        {
            Trail trail = new Trail();
            string xmldata;
            using (StreamReader sr = new StreamReader(inFile))
            {
                xmldata = ("<?xml version=\"1.0\" encoding=\"utf-8\"?><root>" + sr.ReadToEnd().Replace("&", "&amp;") + "</root>");
            }
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(xmldata);
            XmlNode root = xml.SelectSingleNode("/root");
            XmlNodeList childlist = root.ChildNodes;
            foreach (XmlNode node in childlist)
            {
                switch (node.Name)
                {
                    case "MaxPoints":
                        trail.MaxPoints = Convert.ToInt32(node.InnerText);
                        break;
                    case "MinPointDistance":
                        trail.MinPointDistance = Convert.ToSingle(node.InnerText, CultureInfo.InvariantCulture);
                        break;
                    case "Loops":
                        trail.TrailFlags |= node.InnerText == "1" ? 0b1 : 0;
                        break;
                    case "Image":
                        trail.Image = node.InnerText;
                        break;
                    case "ImageResource":
                        trail.ImageResource = node.InnerText;
                        break;
                    case "WidthOverLength":
                        trail.WidthOverLength = ReadTrackNode(node.InnerText);
                        break;
                    case "WidthOverTime":
                        trail.WidthOverTime = ReadTrackNode(node.InnerText);
                        break;
                    case "AlphaOverLength":
                        trail.AlphaOverLength = ReadTrackNode(node.InnerText);
                        break;
                    case "AlphaOverTime":
                        trail.AlphaOverTime = ReadTrackNode(node.InnerText);
                        break;
                    case "TrailDuration":
                        trail.TrailDuration = ReadTrackNode(node.InnerText);
                        break;
                }
            }
            return trail;
        }

        static TrailTrackNode[] ReadTrackNode(string inText)
        {
            List<TrailTrackNode> ans = new List<TrailTrackNode>();
            int length = inText.Length;
            int i = 0;
            while (i < length)
            {
                TrailTrackNode node = new TrailTrackNode();
                //Min, Max, Distribution
                char next = inText[i];
                if (next == '[')
                {
                    //Read First Number
                    i++;
                    int j = i;
                    while (true)
                    {
                        j++;
                        char next2 = inText[j];
                        if (next2 == ' ' || next2 == ']') break;
                    }
                    float n = Convert.ToSingle(inText[i..j], CultureInfo.InvariantCulture);
                    node.LowValue = n;
                    //Check Next Token
                    if (inText[j] == ']')
                    {
                        node.HighValue = n;
                        node.Distribution = 0; //Only one number => Constant
                        i = j + 1;
                    }
                    else
                    {
                        //Is Distribution?
                        i = ++j;
                        char next2 = inText[i];
                        if (next2 >= 'A' && next2 <= 'Z')
                        {
                            while (true)
                            {
                                j++;
                                if (inText[j] == ' ') break;
                            }
                            string temp = inText[i..j];
                            if (TrailEDic.ContainsKey(temp))
                            {
                                node.Distribution = TrailEDic[temp];
                            }
                            else
                            {
                                node.Distribution = Convert.ToInt32(temp[10..^1]); //eg: TodCurves(15)
                            }
                            i = ++j;
                        }
                        else
                        {
                            node.Distribution = 1; //Two Number => Linear
                        }
                        //Last Number
                        while (true)
                        {
                            j++;
                            if (inText[j] == ']') break;
                        }
                        n = Convert.ToSingle(inText[i..j], CultureInfo.InvariantCulture);
                        node.HighValue = n;
                        i = ++j;
                    }
                }
                else if (next == '.' || next == '-' || (next >= '0' && next <= '9'))
                {
                    //Read a number
                    int j = i;
                    while (true)
                    {
                        j++;
                        if (j >= length) break;
                        char next2 = inText[j];
                        if (next2 == ' ' || next2 == ',') break;
                    }
                    float n = Convert.ToSingle(inText[i..j], CultureInfo.InvariantCulture);
                    node.LowValue = n;
                    node.HighValue = n;
                    node.Distribution = 1; //Only One Number Without [] => Linear
                    i = j;
                }
                else
                {
                    node.LowValue = 0;
                    node.HighValue = 0;
                    node.Distribution = 1;
                }
                //Time
                if (i >= length)
                {
                    node.Time = -10000;
                    node.CurveType = 1;
                    ans.Add(node);
                    break;
                }
                next = inText[i];
                if (next == ',')
                {
                    i++;
                    int j = i;
                    while (true)
                    {
                        j++;
                        if (j >= length || inText[j] == ' ') break;
                    }
                    node.Time = Convert.ToSingle(inText[i..j], CultureInfo.InvariantCulture);
                    i = j;
                }
                else
                {
                    node.Time = -10000;
                }
                //CurveType
                if ((++i) >= length)
                {
                    node.CurveType = 1;
                    ans.Add(node);
                    break;
                }
                next = inText[i];
                if (next < 'A' || next > 'Z')
                {
                    node.CurveType = 1;
                }
                else
                {
                    int j = i;
                    while (true)
                    {
                        j++;
                        if (j >= length || inText[j] == ' ') break;
                    }
                    string temp = inText[i..j];
                    if (TrailEDic.ContainsKey(temp))
                    {
                        node.CurveType = TrailEDic[temp];
                    }
                    else
                    {
                        node.CurveType = Convert.ToInt32(temp[10..^1]); //e.g.: TodCurves(15)
                    }
                    i = ++j;
                }
                ans.Add(node);
            }
            TrailTrackNode[] realans = ans.ToArray();
            //Default Times
            int tNum = realans.Length;
            if (tNum == 0) return realans;
            if (realans[0].Time < -1000) realans[0].Time = 0;
            if (tNum != 1 && realans[tNum - 1].Time < -1000) realans[tNum - 1].Time = 100;
            float delta = 0, last = 0;
            for (i = 0; i < tNum; i++)
            {
                if (realans[i].Time >= -1000)
                {
                    last = realans[i].Time;
                    //Find the delta
                    if (i < tNum - 1)
                    {
                        int j = i + 1;
                        while (realans[j].Time < -1000) j++;
                        delta = (realans[j].Time - realans[i].Time) / (j - i);
                    }
                }
                else
                {
                    last += delta;
                    realans[i].Time = last;
                }
                realans[i].Time /= 100;
            }
            return realans;
        }

        public static void Encode(Trail trail, string outFile)
        {
            using (StreamWriter sw = new StreamWriter(outFile, false))
            {
                if (trail.Image != null)
                {
                    sw.Write("<Image>");
                    sw.Write(trail.Image);
                    sw.Write("</Image>\n");
                }
                int MaxPoints = trail.MaxPoints ?? 2;
                if (MaxPoints != 2)
                {
                    sw.Write("<MaxPoints>");
                    sw.Write(trail.MaxPoints);
                    sw.Write("</MaxPoints>\n");
                }
                MaxPoints = trail.TrailFlags;
                if ((MaxPoints & 0b1) != 0)
                {
                    sw.Write("<Loops>1</Loops>\n");
                }
                float MinPointDistance = trail.MinPointDistance ?? 1;
                if (MinPointDistance != 1)
                {
                    sw.Write("<MinPointDistance>");
                    sw.Write(FloatToString(trail.MinPointDistance));
                    sw.Write("</MinPointDistance>\n");
                }
                if (trail.WidthOverLength != null && trail.WidthOverLength.Length != 0)
                {
                    sw.Write("<WidthOverLength>");
                    WriteTrackNode(trail.WidthOverLength, sw);
                    sw.Write("</WidthOverLength>\n");
                }
                if (trail.WidthOverTime != null && trail.WidthOverTime.Length != 0)
                {
                    sw.Write("<WidthOverTime>");
                    WriteTrackNode(trail.WidthOverTime, sw);
                    sw.Write("</WidthOverTime>\n");
                }
                if (trail.AlphaOverLength != null && trail.AlphaOverLength.Length != 0)
                {
                    sw.Write("<AlphaOverLength>");
                    WriteTrackNode(trail.AlphaOverLength, sw);
                    sw.Write("</AlphaOverLength>\n");
                }
                if (trail.AlphaOverTime != null && trail.AlphaOverTime.Length != 0)
                {
                    sw.Write("<AlphaOverTime>");
                    WriteTrackNode(trail.AlphaOverTime, sw);
                    sw.Write("</AlphaOverTime>\n");
                }
                if (trail.TrailDuration != null && trail.TrailDuration.Length != 0)
                {
                    sw.Write("<TrailDuration>");
                    WriteTrackNode(trail.TrailDuration, sw);
                    sw.Write("</TrailDuration>\n");
                }
            }
        }

        static string FloatToString(float? f)
        {
            string ans = f.GetValueOrDefault().ToString(CultureInfo.InvariantCulture);
            return ans.StartsWith("0.") ? ans[1..] : ans;
        }

        static void WriteTrackNode(TrailTrackNode[] track, StreamWriter sw)
        {
            int length = track.Length;
            for (int i = 0; i < length; i++)
            {
                if (i > 0) sw.Write(' ');
                TrailTrackNode node = track[i];
                int Distribution = node.Distribution ?? 1;
                node.LowValue ??= 0;
                node.HighValue ??= 0;
                node.CurveType ??= 1;
                //Min, Max, Distribution
                if (node.LowValue == node.HighValue)
                {
                    if (Distribution == 0)
                    {
                        sw.Write('[');
                        sw.Write(FloatToString(node.LowValue));
                        sw.Write(']');
                    }
                    else if (Distribution == 1)
                    {
                        sw.Write(FloatToString(node.LowValue));
                    }
                    else
                    {
                        sw.Write('[');
                        sw.Write(FloatToString(node.LowValue));
                        sw.Write(' ');
                        if (Distribution < 0 || Distribution > 13)
                        {
                            sw.Write("TodCurves(");
                            sw.Write(Distribution);
                            sw.Write(')');
                        }
                        else
                        {
                            sw.Write(TrailEnum[Distribution]);
                        }
                        sw.Write(' ');
                        sw.Write(FloatToString(node.HighValue));
                        sw.Write(']');
                    }
                }
                else
                {
                    sw.Write('[');
                    sw.Write(FloatToString(node.LowValue));
                    if (Distribution != 1)
                    {
                        sw.Write(' ');
                        if (Distribution < 0 || Distribution > 13)
                        {
                            sw.Write("TodCurves(");
                            sw.Write(Distribution);
                            sw.Write(')');
                        }
                        else
                        {
                            sw.Write(TrailEnum[Distribution]);
                        }
                    }
                    sw.Write(' ');
                    sw.Write(FloatToString(node.HighValue));
                    sw.Write(']');
                }
                //Time
                if (node.Time != 0 && node.Time != 1)
                {
                    sw.Write(',');
                    sw.Write((node.Time * 100).ToString(CultureInfo.InvariantCulture));
                }
                //Curves
                Distribution = node.CurveType ?? 1;
                if (Distribution != 1)
                {
                    sw.Write(' ');
                    if (Distribution < 0 || Distribution > 13)
                    {
                        sw.Write("TodCurves(");
                        sw.Write(Distribution);
                        sw.Write(')');
                    }
                    else
                    {
                        sw.Write(TrailEnum[Distribution]);
                    }
                }
            }
        }

        static Dictionary<string, int> TrailEDic = new Dictionary<string, int> { { "Constant", 0 }, { "Linear", 1 }, { "EaseIn", 2 }, { "EaseOut", 3 }, { "EaseInOut", 4 }, { "EaseInOutWeak", 5 }, { "FastInOut", 6 }, { "FastInOutWeak", 7 }, { "WeakFastInOut", 8 }, { "Bounce", 9 }, { "BounceFastMiddle", 10 }, { "BounceSlowMiddle", 11 }, { "SinWave", 12 }, { "EaseSinWave", 13 } };

        static string[] TrailEnum = new string[14] { "Constant", "Linear", "EaseIn", "EaseOut", "EaseInOut", "EaseInOutWeak", "FastInOut", "FastInOutWeak", "WeakFastInOut", "Bounce", "BounceFastMiddle", "BounceSlowMiddle", "SinWave", "EaseSinWave" };
    }
}
