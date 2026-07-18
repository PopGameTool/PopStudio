using System.Xml;
using System.Globalization;

namespace PopStudio.Particles
{
    internal class RawXml
    {
        public static Particles Decode(string inFile)
        {
            Particles particles = new Particles();
            string xmldata;
            using (StreamReader sr = new StreamReader(inFile))
            {
                xmldata = ("<?xml version=\"1.0\" encoding=\"utf-8\"?><root>" + sr.ReadToEnd().Replace("&", "&amp;") + "</root>");
            }
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(xmldata);
            XmlNode root = xml.SelectSingleNode("/root");
            XmlNodeList childlist = root.ChildNodes;
            int EmitterNumber = childlist.Count;
            particles.Emitters = new ParticlesEmitter[EmitterNumber];
            List<ParticlesField> EmitterField = new List<ParticlesField>();
            List<ParticlesField> EmitterSystemField = new List<ParticlesField>();
            for (int i = 0; i < EmitterNumber; i++)
            {
                EmitterField.Clear();
                EmitterSystemField.Clear();
                XmlNode node_out = childlist[i];
                if (node_out.Name != "Emitter") throw new Exception();
                ParticlesEmitter emitter = new ParticlesEmitter();
                XmlNodeList childchildlist = node_out.ChildNodes;
                foreach (XmlNode node in childchildlist)
                {
                    switch (node.Name)
                    {
                        case "Name":
                            emitter.Name = node.InnerText;
                            break;
                        case "Image":
                            emitter.Image = node.InnerText;
                            break;
                        case "ImageResource":
                            emitter.ImagePath = node.InnerText;
                            break;
                        case "ImageCol":
                            emitter.ImageCol = Convert.ToInt32(node.InnerText);
                            break;
                        case "ImageRow":
                            emitter.ImageRow = Convert.ToInt32(node.InnerText);
                            break;
                        case "ImageFrames":
                            emitter.ImageFrames = Convert.ToInt32(node.InnerText);
                            break;
                        case "Animated":
                            emitter.Animated = Convert.ToInt32(node.InnerText);
                            break;
                        case "RandomLaunchSpin":
                            emitter.ParticleFlags |= node.InnerText == "1" ? 0b1 : 0;
                            break;
                        case "AlignLaunchSpin":
                            emitter.ParticleFlags |= node.InnerText == "1" ? 0b10 : 0;
                            break;
                        case "AlignToPixel":
                            emitter.ParticleFlags |= node.InnerText == "1" ? 0b100 : 0;
                            break;
                        case "SystemLoops":
                            emitter.ParticleFlags |= node.InnerText == "1" ? 0b1000 : 0;
                            break;
                        case "ParticleLoops":
                            emitter.ParticleFlags |= node.InnerText == "1" ? 0b10000 : 0;
                            break;
                        case "ParticlesDontFollow":
                            emitter.ParticleFlags |= node.InnerText == "1" ? 0b100000 : 0;
                            break;
                        case "RandomStartTime":
                            emitter.ParticleFlags |= node.InnerText == "1" ? 0b1000000 : 0;
                            break;
                        case "DieIfOverloaded":
                            emitter.ParticleFlags |= node.InnerText == "1" ? 0b10000000 : 0;
                            break;
                        case "Additive":
                            emitter.ParticleFlags |= node.InnerText == "1" ? 0b100000000 : 0;
                            break;
                        case "FullScreen":
                            emitter.ParticleFlags |= node.InnerText == "1" ? 0b1000000000 : 0;
                            break;
                        case "SoftwareOnly":
                            emitter.ParticleFlags |= node.InnerText == "1" ? 0b10000000000 : 0;
                            break;
                        case "HardwareOnly":
                            emitter.ParticleFlags |= node.InnerText == "1" ? 0b100000000000 : 0;
                            break;
                        case "EmitterType": //eg: Emitter(15)
                            if (EmitterEDic.ContainsKey(node.InnerText))
                            {
                                emitter.EmitterType = EmitterEDic[node.InnerText];
                            }
                            else
                            {
                                emitter.EmitterType = Convert.ToInt32(node.InnerText[8..^1]);
                            }
                            break;
                        case "OnDuration":
                            emitter.OnDuration = node.InnerText;
                            break;
                        case "SystemDuration":
                            emitter.SystemDuration = ReadTrackNode(node.InnerText);
                            break;
                        case "CrossFadeDuration":
                            emitter.CrossFadeDuration = ReadTrackNode(node.InnerText);
                            break;
                        case "SpawnRate":
                            emitter.SpawnRate = ReadTrackNode(node.InnerText);
                            break;
                        case "SpawnMinActive":
                            emitter.SpawnMinActive = ReadTrackNode(node.InnerText);
                            break;
                        case "SpawnMaxActive":
                            emitter.SpawnMaxActive = ReadTrackNode(node.InnerText);
                            break;
                        case "SpawnMaxLaunched":
                            emitter.SpawnMaxLaunched = ReadTrackNode(node.InnerText);
                            break;
                        case "EmitterRadius":
                            emitter.EmitterRadius = ReadTrackNode(node.InnerText);
                            break;
                        case "EmitterOffsetX":
                            emitter.EmitterOffsetX = ReadTrackNode(node.InnerText);
                            break;
                        case "EmitterOffsetY":
                            emitter.EmitterOffsetY = ReadTrackNode(node.InnerText);
                            break;
                        case "EmitterBoxX":
                            emitter.EmitterBoxX = ReadTrackNode(node.InnerText);
                            break;
                        case "EmitterBoxY":
                            emitter.EmitterBoxY = ReadTrackNode(node.InnerText);
                            break;
                        case "EmitterPath":
                            emitter.EmitterPath = ReadTrackNode(node.InnerText);
                            break;
                        case "EmitterSkewX":
                            emitter.EmitterSkewX = ReadTrackNode(node.InnerText);
                            break;
                        case "EmitterSkewY":
                            emitter.EmitterSkewY = ReadTrackNode(node.InnerText);
                            break;
                        case "ParticleDuration":
                            emitter.ParticleDuration = ReadTrackNode(node.InnerText);
                            break;
                        case "SystemRed":
                            emitter.SystemRed = ReadTrackNode(node.InnerText);
                            break;
                        case "SystemGreen":
                            emitter.SystemGreen = ReadTrackNode(node.InnerText);
                            break;
                        case "SystemBlue":
                            emitter.SystemBlue = ReadTrackNode(node.InnerText);
                            break;
                        case "SystemAlpha":
                            emitter.SystemAlpha = ReadTrackNode(node.InnerText);
                            break;
                        case "SystemBrightness":
                            emitter.SystemBrightness = ReadTrackNode(node.InnerText);
                            break;
                        case "LaunchSpeed":
                            emitter.LaunchSpeed = ReadTrackNode(node.InnerText);
                            break;
                        case "LaunchAngle":
                            emitter.LaunchAngle = ReadTrackNode(node.InnerText);
                            break;
                        case "Field":
                            EmitterField.Add(ReadField(node));
                            break;
                        case "SystemField":
                            EmitterSystemField.Add(ReadField(node));
                            break;
                        case "ParticleRed":
                            emitter.ParticleRed = ReadTrackNode(node.InnerText);
                            break;
                        case "ParticleGreen":
                            emitter.ParticleGreen = ReadTrackNode(node.InnerText);
                            break;
                        case "ParticleBlue":
                            emitter.ParticleBlue = ReadTrackNode(node.InnerText);
                            break;
                        case "ParticleAlpha":
                            emitter.ParticleAlpha = ReadTrackNode(node.InnerText);
                            break;
                        case "ParticleBrightness":
                            emitter.ParticleBrightness = ReadTrackNode(node.InnerText);
                            break;
                        case "ParticleSpinAngle":
                            emitter.ParticleSpinAngle = ReadTrackNode(node.InnerText);
                            break;
                        case "ParticleSpinSpeed":
                            emitter.ParticleSpinSpeed = ReadTrackNode(node.InnerText);
                            break;
                        case "ParticleScale":
                            emitter.ParticleScale = ReadTrackNode(node.InnerText);
                            break;
                        case "ParticleStretch":
                            emitter.ParticleStretch = ReadTrackNode(node.InnerText);
                            break;
                        case "CollisionReflect":
                            emitter.CollisionReflect = ReadTrackNode(node.InnerText);
                            break;
                        case "CollisionSpin":
                            emitter.CollisionSpin = ReadTrackNode(node.InnerText);
                            break;
                        case "ClipTop":
                            emitter.ClipTop = ReadTrackNode(node.InnerText);
                            break;
                        case "ClipBottom":
                            emitter.ClipBottom = ReadTrackNode(node.InnerText);
                            break;
                        case "ClipLeft":
                            emitter.ClipLeft = ReadTrackNode(node.InnerText);
                            break;
                        case "ClipRight":
                            emitter.ClipRight = ReadTrackNode(node.InnerText);
                            break;
                        case "AnimationRate":
                            emitter.AnimationRate = ReadTrackNode(node.InnerText);
                            break;
                    }
                }
                emitter.Field = EmitterField.ToArray();
                emitter.SystemField = EmitterSystemField.ToArray();
                particles.Emitters[i] = emitter;
            }
            return particles;
        }

        static ParticlesField ReadField(XmlNode root)
        {
            ParticlesField field = new ParticlesField();
            XmlNodeList childnodes = root.ChildNodes;
            foreach (XmlNode node in childnodes)
            {
                switch (node.Name)
                {
                    case "FieldType":
                        if (FieldEDic.ContainsKey(node.InnerText))
                        {
                            field.FieldType = FieldEDic[node.InnerText];
                        }
                        else
                        {
                            field.FieldType = Convert.ToInt32(node.InnerText[6..^1]); //eg:Field(15)
                        }
                        break;
                    case "X":
                        field.X = ReadTrackNode(node.InnerText);
                        break;
                    case "Y":
                        field.Y = ReadTrackNode(node.InnerText);
                        break;
                }
            }
            return field;
        }

        static ParticlesTrackNode[] ReadTrackNode(string inText)
        {
            List<ParticlesTrackNode> ans = new List<ParticlesTrackNode>();
            int length = inText.Length;
            int i = 0;
            while (i < length)
            {
                ParticlesTrackNode node = new ParticlesTrackNode();
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
            ParticlesTrackNode[] realans = ans.ToArray();
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

        public static void Encode(Particles particles, string outFile)
        {
            using (StreamWriter sw = new StreamWriter(outFile, false))
            {
                ParticlesEmitter[] emitters = particles.Emitters;
                if (emitters != null && emitters.Length > 0)
                {
                    int count = emitters.Length;
                    for (int i = 0; i < count; i++)
                    {
                        sw.Write("<Emitter>\n");
                        ParticlesEmitter emitter = emitters[i];
                        if (emitter != null)
                        {
                            if (emitter.Name != null)
                            {
                                sw.Write("  <Name>");
                                sw.Write(emitter.Name);
                                sw.Write("</Name>\n");
                            }
                            if (emitter.SpawnMinActive != null)
                            {
                                sw.Write("  <SpawnMinActive>");
                                WriteTrackNode(emitter.SpawnMinActive, sw);
                                sw.Write("</SpawnMinActive>\n");
                            }
                            if (emitter.SpawnMaxLaunched != null)
                            {
                                sw.Write("  <SpawnMaxLaunched>");
                                WriteTrackNode(emitter.SpawnMaxLaunched, sw);
                                sw.Write("</SpawnMaxLaunched>\n");
                            }
                            if (emitter.ParticleDuration != null)
                            {
                                sw.Write("  <ParticleDuration>");
                                WriteTrackNode(emitter.ParticleDuration, sw);
                                sw.Write("</ParticleDuration>\n");
                            }
                            if (emitter.ParticleRed != null)
                            {
                                sw.Write("  <ParticleRed>");
                                WriteTrackNode(emitter.ParticleRed, sw);
                                sw.Write("</ParticleRed>\n");
                            }
                            if (emitter.ParticleGreen != null)
                            {
                                sw.Write("  <ParticleGreen>");
                                WriteTrackNode(emitter.ParticleGreen, sw);
                                sw.Write("</ParticleGreen>\n");
                            }
                            if (emitter.ParticleBlue != null)
                            {
                                sw.Write("  <ParticleBlue>");
                                WriteTrackNode(emitter.ParticleBlue, sw);
                                sw.Write("</ParticleBlue>\n");
                            }
                            if (emitter.ParticleAlpha != null)
                            {
                                sw.Write("  <ParticleAlpha>");
                                WriteTrackNode(emitter.ParticleAlpha, sw);
                                sw.Write("</ParticleAlpha>\n");
                            }
                            if (emitter.ParticleBrightness != null)
                            {
                                sw.Write("  <ParticleBrightness>");
                                WriteTrackNode(emitter.ParticleBrightness, sw);
                                sw.Write("</ParticleBrightness>\n");
                            }
                            if (emitter.ParticleSpinAngle != null)
                            {
                                sw.Write("  <ParticleSpinAngle>");
                                WriteTrackNode(emitter.ParticleSpinAngle, sw);
                                sw.Write("</ParticleSpinAngle>\n");
                            }
                            if (emitter.ParticleScale != null)
                            {
                                sw.Write("  <ParticleScale>");
                                WriteTrackNode(emitter.ParticleScale, sw);
                                sw.Write("</ParticleScale>\n");
                            }
                            if (emitter.ParticleStretch != null)
                            {
                                sw.Write("  <ParticleStretch>");
                                WriteTrackNode(emitter.ParticleStretch, sw);
                                sw.Write("</ParticleStretch>\n");
                            }
                            if (emitter.EmitterRadius != null)
                            {
                                sw.Write("  <EmitterRadius>");
                                WriteTrackNode(emitter.EmitterRadius, sw);
                                sw.Write("</EmitterRadius>\n");
                            }
                            int flags = emitter.EmitterType ?? 1;
                            if (flags != 1)
                            {
                                sw.Write("  <EmitterType>");
                                if (flags < 0 || flags > 4)
                                {
                                    sw.Write("Emitter(");
                                    sw.Write(flags);
                                    sw.Write(')');
                                }
                                else
                                {
                                    sw.Write(EmitterType[flags]);
                                }
                                sw.Write("</EmitterType>\n");
                            }
                            if (emitter.SystemDuration != null)
                            {
                                sw.Write("  <SystemDuration>");
                                WriteTrackNode(emitter.SystemDuration, sw);
                                sw.Write("</SystemDuration>\n");
                            }
                            if (emitter.AnimationRate != null)
                            {
                                sw.Write("  <AnimationRate>");
                                WriteTrackNode(emitter.AnimationRate, sw);
                                sw.Write("</AnimationRate>\n");
                            }
                            if (emitter.ImageFrames != null)
                            {
                                sw.Write("  <ImageFrames>");
                                sw.Write(emitter.ImageFrames);
                                sw.Write("</ImageFrames>\n");
                            }
                            if (emitter.EmitterOffsetX != null)
                            {
                                sw.Write("  <EmitterOffsetX>");
                                WriteTrackNode(emitter.EmitterOffsetX, sw);
                                sw.Write("</EmitterOffsetX>\n");
                            }
                            if (emitter.EmitterOffsetY != null)
                            {
                                sw.Write("  <EmitterOffsetY>");
                                WriteTrackNode(emitter.EmitterOffsetY, sw);
                                sw.Write("</EmitterOffsetY>\n");
                            }
                            if (emitter.EmitterBoxX != null)
                            {
                                sw.Write("  <EmitterBoxX>");
                                WriteTrackNode(emitter.EmitterBoxX, sw);
                                sw.Write("</EmitterBoxX>\n");
                            }
                            if (emitter.EmitterBoxY != null)
                            {
                                sw.Write("  <EmitterBoxY>");
                                WriteTrackNode(emitter.EmitterBoxY, sw);
                                sw.Write("</EmitterBoxY>\n");
                            }
                            if (emitter.EmitterPath != null)
                            {
                                sw.Write("  <EmitterPath>");
                                WriteTrackNode(emitter.EmitterPath, sw);
                                sw.Write("</EmitterPath>\n");
                            }
                            if (emitter.EmitterSkewX != null)
                            {
                                sw.Write("  <EmitterSkewX>");
                                WriteTrackNode(emitter.EmitterSkewX, sw);
                                sw.Write("</EmitterSkewX>\n");
                            }
                            if (emitter.EmitterSkewY != null)
                            {
                                sw.Write("  <EmitterSkewY>");
                                WriteTrackNode(emitter.EmitterSkewY, sw);
                                sw.Write("</EmitterSkewY>\n");
                            }
                            if (emitter.Image != null)
                            {
                                sw.Write("  <Image>");
                                sw.Write(emitter.Image);
                                sw.Write("</Image>\n");
                            }
                            if (emitter.ImagePath != null)
                            {
                                sw.Write("  <ImageResource>");
                                sw.Write(emitter.ImagePath);
                                sw.Write("</ImageResource>\n");
                            }
                            if (emitter.Field != null)
                            {
                                WriteFields(emitter.Field, sw, "Field");
                            }
                            if (emitter.SystemField != null)
                            {
                                WriteFields(emitter.SystemField, sw, "SystemField");
                            }
                            if (emitter.ParticleSpinSpeed != null)
                            {
                                sw.Write("  <ParticleSpinSpeed>");
                                WriteTrackNode(emitter.ParticleSpinSpeed, sw);
                                sw.Write("</ParticleSpinSpeed>\n");
                            }
                            if (emitter.ImageCol != null)
                            {
                                sw.Write("  <ImageCol>");
                                sw.Write(emitter.ImageCol);
                                sw.Write("</ImageCol>\n");
                            }
                            if (emitter.ImageRow != null)
                            {
                                sw.Write("  <ImageRow>");
                                sw.Write(emitter.ImageRow);
                                sw.Write("</ImageRow>\n");
                            }
                            if (emitter.Animated != null)
                            {
                                sw.Write("  <Animated>");
                                sw.Write(emitter.Animated);
                                sw.Write("</Animated>\n");
                            }
                            flags = emitter.ParticleFlags;
                            if ((flags & 0b1) != 0)
                            {
                                sw.Write("  <RandomLaunchSpin>1</RandomLaunchSpin>\n");
                            }
                            if ((flags & 0b10) != 0)
                            {
                                sw.Write("  <AlignLaunchSpin>1</AlignLaunchSpin>\n");
                            }
                            if ((flags & 0b100) != 0)
                            {
                                sw.Write("  <AlignToPixel>1</AlignToPixel>\n");
                            }
                            if ((flags & 0b1000) != 0)
                            {
                                sw.Write("  <SystemLoops>1</SystemLoops>\n");
                            }
                            if ((flags & 0b10000) != 0)
                            {
                                sw.Write("  <ParticleLoops>1</ParticleLoops>\n");
                            }
                            if ((flags & 0b100000) != 0)
                            {
                                sw.Write("  <ParticlesDontFollow>1</ParticlesDontFollow>\n");
                            }
                            if ((flags & 0b1000000) != 0)
                            {
                                sw.Write("  <RandomStartTime>1</RandomStartTime>\n");
                            }
                            if ((flags & 0b10000000) != 0)
                            {
                                sw.Write("  <DieIfOverloaded>1</DieIfOverloaded>\n");
                            }
                            if ((flags & 0b100000000) != 0)
                            {
                                sw.Write("  <Additive>1</Additive>\n");
                            }
                            if ((flags & 0b1000000000) != 0)
                            {
                                sw.Write("  <FullScreen>1</FullScreen>\n");
                            }
                            if ((flags & 0b10000000000) != 0)
                            {
                                sw.Write("  <SoftwareOnly>1</SoftwareOnly>\n");
                            }
                            if ((flags & 0b100000000000) != 0)
                            {
                                sw.Write("  <HardwareOnly>1</HardwareOnly>\n");
                            }
                            if (emitter.OnDuration != null)
                            {
                                sw.Write("  <OnDuration>");
                                sw.Write(emitter.OnDuration);
                                sw.Write("</OnDuration>\n");
                            }
                            if (emitter.CrossFadeDuration != null)
                            {
                                sw.Write("  <CrossFadeDuration>");
                                WriteTrackNode(emitter.CrossFadeDuration, sw);
                                sw.Write("</CrossFadeDuration>\n");
                            }
                            if (emitter.SpawnRate != null)
                            {
                                sw.Write("  <SpawnRate>");
                                WriteTrackNode(emitter.SpawnRate, sw);
                                sw.Write("</SpawnRate>\n");
                            }
                            if (emitter.SpawnMaxActive != null)
                            {
                                sw.Write("  <SpawnMaxActive>");
                                WriteTrackNode(emitter.SpawnMaxActive, sw);
                                sw.Write("</SpawnMaxActive>\n");
                            }
                            if (emitter.SystemRed != null)
                            {
                                sw.Write("  <SystemRed>");
                                WriteTrackNode(emitter.SystemRed, sw);
                                sw.Write("</SystemRed>\n");
                            }
                            if (emitter.SystemGreen != null)
                            {
                                sw.Write("  <SystemGreen>");
                                WriteTrackNode(emitter.SystemGreen, sw);
                                sw.Write("</SystemGreen>\n");
                            }
                            if (emitter.SystemBlue != null)
                            {
                                sw.Write("  <SystemBlue>");
                                WriteTrackNode(emitter.SystemBlue, sw);
                                sw.Write("</SystemBlue>\n");
                            }
                            if (emitter.SystemAlpha != null)
                            {
                                sw.Write("  <SystemAlpha>");
                                WriteTrackNode(emitter.SystemAlpha, sw);
                                sw.Write("</SystemAlpha>\n");
                            }
                            if (emitter.SystemBrightness != null)
                            {
                                sw.Write("  <SystemBrightness>");
                                WriteTrackNode(emitter.SystemBrightness, sw);
                                sw.Write("</SystemBrightness>\n");
                            }
                            if (emitter.LaunchSpeed != null)
                            {
                                sw.Write("  <LaunchSpeed>");
                                WriteTrackNode(emitter.LaunchSpeed, sw);
                                sw.Write("</LaunchSpeed>\n");
                            }
                            if (emitter.LaunchAngle != null)
                            {
                                sw.Write("  <LaunchAngle>");
                                WriteTrackNode(emitter.LaunchAngle, sw);
                                sw.Write("</LaunchAngle>\n");
                            }
                            if (emitter.CollisionReflect != null)
                            {
                                sw.Write("  <CollisionReflect>");
                                WriteTrackNode(emitter.CollisionReflect, sw);
                                sw.Write("</CollisionReflect>\n");
                            }
                            if (emitter.CollisionSpin != null)
                            {
                                sw.Write("  <CollisionSpin>");
                                WriteTrackNode(emitter.CollisionSpin, sw);
                                sw.Write("</CollisionSpin>\n");
                            }
                            if (emitter.ClipTop != null)
                            {
                                sw.Write("  <ClipTop>");
                                WriteTrackNode(emitter.ClipTop, sw);
                                sw.Write("</ClipTop>\n");
                            }
                            if (emitter.ClipBottom != null)
                            {
                                sw.Write("  <ClipBottom>");
                                WriteTrackNode(emitter.ClipBottom, sw);
                                sw.Write("</ClipBottom>\n");
                            }
                            if (emitter.ClipLeft != null)
                            {
                                sw.Write("  <ClipLeft>");
                                WriteTrackNode(emitter.ClipLeft, sw);
                                sw.Write("</ClipLeft>\n");
                            }
                            if (emitter.ClipRight != null)
                            {
                                sw.Write("  <ClipRight>");
                                WriteTrackNode(emitter.ClipRight, sw);
                                sw.Write("</ClipRight>\n");
                            }
                        }
                        sw.Write("</Emitter>\n");
                    }
                }
            }
        }
        
        static void WriteFields(ParticlesField[] fields, StreamWriter sw, string FieldsName)
        {
            int length = fields.Length;
            if (length == 0) return;
            for (int i = 0; i < length; i++)
            {
                ParticlesField field = fields[i];
                sw.Write("  <");
                sw.Write(FieldsName);
                sw.Write(">\n");
                int type = field.FieldType ?? 0;
                if (type != 0)
                {
                    sw.Write("    <FieldType>");
                    if (type < 0 || type > 11)
                    {
                        sw.Write("Field(");
                        sw.Write(type);
                        sw.Write(')');
                    }
                    else
                    {
                        sw.Write(FieldType[type]);
                    }
                    sw.Write("</FieldType>\n");
                }
                if (field.X != null)
                {
                    sw.Write("    <X>");
                    WriteTrackNode(field.X, sw);
                    sw.Write("</X>\n");
                }
                if (field.Y != null)
                {
                    sw.Write("    <Y>");
                    WriteTrackNode(field.Y, sw);
                    sw.Write("</Y>\n");
                }
                sw.Write("  </");
                sw.Write(FieldsName);
                sw.Write(">\n");
            }
        }

        static string FloatToString(float? f)
        {
            string ans = f.GetValueOrDefault().ToString(CultureInfo.InvariantCulture);
            return ans.StartsWith("0.") ? ans[1..] : ans;
        }

        static void WriteTrackNode(ParticlesTrackNode[] track, StreamWriter sw)
        {
            int length = track.Length;
            for (int i = 0; i < length; i++)
            {
                if (i > 0) sw.Write(' ');
                ParticlesTrackNode node = track[i];
                int Distribution = node.Distribution ?? 1;
                int CurveType = node.CurveType ?? 1;
                node.LowValue ??= 0;
                node.HighValue ??= 0;
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
                if (CurveType != 1)
                {
                    sw.Write(' ');
                    if (CurveType < 0 || CurveType > 13)
                    {
                        sw.Write("TodCurves(");
                        sw.Write(CurveType);
                        sw.Write(')');
                    }
                    else
                    {
                        sw.Write(TrailEnum[CurveType]);
                    }
                }
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
