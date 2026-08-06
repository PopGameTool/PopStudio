using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace PopStudio.Reanim
{
    internal class FlashXfl
    {
        public static Reanim Decode(string inFile) => FlashXflDecoder.Decode(inFile);

        public static void Encode(Reanim reanim, string outFile)
        {
            UseLabelName = Setting.ReanimXflLabelName;
            ScaleX = Setting.ReanimXflScaleX;
            ScaleY = Setting.ReanimXflScaleY;
            outFile = Dir.FormatPath(outFile) + Const.PATHSEPARATOR;
            string outFile_Library = outFile + "LIBRARY";
            Dir.NewDir(outFile_Library);
            outFile_Library += Const.PATHSEPARATOR;
            //main.xfl
            using (BinaryStream bs = new BinaryStream(outFile + "main.xfl", FileMode.Create))
            {
                bs.WriteString("PROXY-CS5");
            }
            //Png List
            List<string> media = new List<string>();
            List<string> symbols = new List<string>();
            Dictionary<string, bool> Exist = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, string> RegisteredImageNames = new Dictionary<string, string>(StringComparer.Ordinal);
            const string locatorName = "__reanim_locator__";
            symbols.Add(locatorName);
            Exist.Add(locatorName, true);
            WriteLocatorSymbol(outFile_Library, locatorName);
            using (StreamWriter sw = new StreamWriter(outFile + "DOMDocument.xml", false))
            {
                sw.Write("<DOMDocument frameRate=\"");
                sw.Write(reanim.fps);
                sw.Write("\" width=\"");
                sw.Write(Setting.ReanimXflWidth);
                sw.Write("\" height=\"");
                sw.Write(Setting.ReanimXflHeight);
                sw.Write("\" xflVersion=\"2.97\" xmlns=\"http://ns.adobe.com/xfl/2008/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">\n");
                //timelines
                sw.Write("    <timelines>\n");
                sw.Write("        <DOMTimeline name=\"Scene 1\">\n");
                sw.Write("            <layers>\n");
                for (int i = reanim.tracks.Length - 1; i >= 0; i--)
                {
                    ReanimTransform defaultTransform = new ReanimTransform
                    {
                        x = 0,
                        y = 0,
                        sx = 1,
                        sy = 1,
                        kx = 0,
                        ky = 0,
                        f = 0,
                        a = 1
                    };
                    ReanimTrack track = reanim.tracks[i];
                    sw.Write("                <DOMLayer name=\"");
                    sw.Write(track.name);
                    sw.Write("\">\n");
                    sw.Write("                    <frames>\n");
                    int transformNumber = track.transforms.Length;
                    List<string> ImgList = new List<string>();
                    Dictionary<string, string> ImageNames = new Dictionary<string, string>(StringComparer.Ordinal);
                    int index = 0;
                    for (int j = 0; j < transformNumber; j++)
                    {
                        ReanimTransform thisTransform = track.transforms[j];
                        #region update default transform
                        defaultTransform.x = thisTransform.x ?? defaultTransform.x;
                        defaultTransform.y = thisTransform.y ?? defaultTransform.y;
                        defaultTransform.kx = thisTransform.kx ?? defaultTransform.kx;
                        defaultTransform.ky = thisTransform.ky ?? defaultTransform.ky;
                        defaultTransform.sx = thisTransform.sx ?? defaultTransform.sx;
                        defaultTransform.sy = thisTransform.sy ?? defaultTransform.sy;
                        defaultTransform.f = thisTransform.f ?? defaultTransform.f;
                        defaultTransform.a = thisTransform.a ?? defaultTransform.a;
                        if (thisTransform.i != null)
                        {
                            string imageID = Convert.ToString(thisTransform.i, CultureInfo.InvariantCulture);
                            if (string.IsNullOrEmpty(imageID))
                            {
                                defaultTransform.i = null;
                            }
                            else
                            {
                                if (!ImageNames.TryGetValue(imageID, out string nid))
                                {
                                    string imageKey = UseLabelName > 0 ? i + "\0" + imageID : imageID;
                                    if (!RegisteredImageNames.TryGetValue(imageKey, out nid))
                                    {
                                        nid = GetUniqueLibraryName(GetNameByID(imageID, track.name, index), Exist);
                                        RegisteredImageNames.Add(imageKey, nid);
                                        Exist.Add(nid, true);
                                        ImgList.Add(nid);
                                        media.Add(nid);
                                        symbols.Add(nid);
                                    }
                                    index++;
                                    ImageNames.Add(imageID, nid);
                                }
                                defaultTransform.i = nid;
                            }
                        }
                        #endregion
                        sw.Write("                        <DOMFrame index=\"");
                        sw.Write(j);
                        sw.Write("\">\n");
                        if (defaultTransform.f != -1)
                        {
                            sw.Write("                            <elements>\n");
                            sw.Write("                                <DOMSymbolInstance");
                            sw.Write(" libraryItemName=\"");
                            sw.Write(defaultTransform.i ?? locatorName);
                            sw.Write("\"");
                            sw.Write(">\n");
                            sw.Write("                                    <matrix>\n");
                            sw.Write("                                        <Matrix ");
                            #region matrix
                            //compute the matrix
                            double dx = 180 / Math.PI;
                            double skewx = (defaultTransform.kx ?? 0) / dx;
                            double skewy = -(defaultTransform.ky ?? 0) / dx;
                            float sx = defaultTransform.sx ?? 1;
                            float sy = defaultTransform.sy ?? 1;
                            sw.Write("a=\"");
                            sw.Write(Math.Cos(skewx) * sx);
                            sw.Write("\" b=\"");
                            sw.Write(Math.Sin(skewx) * sx);
                            sw.Write("\" c=\"");
                            sw.Write(Math.Sin(skewy) * sy);
                            sw.Write("\" d=\"");
                            sw.Write(Math.Cos(skewy) * sy);
                            sw.Write("\" ");
                            #endregion
                            #region coordinate
                            if (defaultTransform.x != null)
                            {
                                sw.Write("tx=\"");
                                sw.Write(defaultTransform.x * ScaleX);
                                sw.Write("\" ");
                            }
                            if (defaultTransform.y != null)
                            {
                                sw.Write("ty=\"");
                                sw.Write(defaultTransform.y * ScaleY);
                                sw.Write("\" ");
                            }
                            #endregion
                            sw.Write("/>\n");
                            sw.Write("                                    </matrix>\n");
                            #region alpha
                            if (defaultTransform.a != 1)
                            {
                                sw.Write("                                             <color>\n                                                  <Color alphaMultiplier=\"");
                                sw.Write(defaultTransform.a);
                                sw.Write("\"/>\n                                             </color>\n");
                            }
                            #endregion
                            sw.Write("                                </DOMSymbolInstance>\n");
                            sw.Write("                            </elements>\n");
                        }
                        else
                        {
                            sw.Write("                            <elements />\n");
                        }
                        sw.Write("                        </DOMFrame>\n");
                       
                    }
                    sw.Write("                    </frames>\n");
                    sw.Write("                </DOMLayer>\n");
                    //Libxml
                    int num = ImgList.Count;
                    for (int j = 0; j < num; j++)
                    {
                        string namenow = ImgList[j];
                        using (StreamWriter sw2 = new StreamWriter(outFile_Library + namenow + ".xml", false))
                        {
                            sw2.Write("<DOMSymbolItem xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://ns.adobe.com/xfl/2008/\" name=\"");
                            sw2.Write(namenow);
                            sw2.Write("\">\n");
                            sw2.Write("    <timeline>\n");
                            sw2.Write("        <DOMTimeline name=\"");
                            sw2.Write(namenow);
                            sw2.Write("\">\n");
                            sw2.Write("            <layers>\n");
                            sw2.Write("                <DOMLayer name=\"Layer 1\" color=\"#4FFF4F\" current=\"true\" isSelected=\"true\">\n");
                            sw2.Write("                    <frames>\n");
                            sw2.Write($"                        <DOMFrame index=\"0\">\n");
                            sw2.Write("                            <elements>\n");
                            sw2.Write($"                                <DOMBitmapInstance selected=\"true\" libraryItemName=\"{namenow}.png\" />\n");
                            sw2.Write("                            </elements>\n");
                            sw2.Write("                        </DOMFrame>\n");
                            sw2.Write("                    </frames>\n");
                            sw2.Write("                </DOMLayer>\n");
                            sw2.Write("            </layers>\n");
                            sw2.Write("        </DOMTimeline>\n");
                            sw2.Write("    </timeline>\n");
                            sw2.Write("</DOMSymbolItem>");
                        }
                    }
                }
                sw.Write("            </layers>\n");
                sw.Write("        </DOMTimeline>\n");
                sw.Write("    </timelines>\n");
                //media
                sw.Write("    <media>\n");
                int mediaNumber = media.Count;
                for (int i = 0; i < mediaNumber; i++)
                {
                    sw.Write("        <DOMBitmapItem name=\"");
                    sw.Write(media[i]);
                    sw.Write(".png\" href=\"");
                    sw.Write(media[i]);
                    sw.Write(".png\" />\n");
                    string pngFilePath = outFile_Library + media[i] + ".png";
                    if (!File.Exists(pngFilePath))
                    {
                        using (BinaryStream bs = new BinaryStream(pngFilePath, FileMode.Create))
                        {
                            bs.WriteBytes(defaultPicture);
                        }
                    }
                }
                sw.Write("    </media>\n");
                //symbols
                sw.Write("    <symbols>\n");
                int symbolsNumber = symbols.Count;
                for (int i = 0; i < symbolsNumber; i++)
                {
                    sw.Write("        <Include href=\"");
                    sw.Write(symbols[i]);
                    sw.Write(".xml\" />\n");
                }
                sw.Write("    </symbols>\n");
                sw.Write("</DOMDocument>");
            }
        }

        static string GetUniqueLibraryName(string requestedName, Dictionary<string, bool> existingNames)
        {
            string baseName = string.IsNullOrEmpty(requestedName) ? "image" : requestedName;
            if (!existingNames.ContainsKey(baseName))
            {
                return baseName;
            }

            int suffix = 1;
            string uniqueName;
            do
            {
                uniqueName = baseName + "_" + suffix++;
            }
            while (existingNames.ContainsKey(uniqueName));
            return uniqueName;
        }

        static void WriteLocatorSymbol(string libraryFolder, string locatorName)
        {
            using (StreamWriter sw = new StreamWriter(libraryFolder + locatorName + ".xml", false))
            {
                sw.Write("<DOMSymbolItem xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://ns.adobe.com/xfl/2008/\" name=\"");
                sw.Write(locatorName);
                sw.Write("\">\n");
                sw.Write("    <timeline>\n");
                sw.Write("        <DOMTimeline name=\"");
                sw.Write(locatorName);
                sw.Write("\">\n");
                sw.Write("            <layers>\n");
                sw.Write("                <DOMLayer name=\"Layer 1\" color=\"#4FFF4F\" current=\"true\" isSelected=\"true\">\n");
                sw.Write("                    <frames>\n");
                sw.Write("                        <DOMFrame index=\"0\">\n");
                sw.Write("                            <elements />\n");
                sw.Write("                        </DOMFrame>\n");
                sw.Write("                    </frames>\n");
                sw.Write("                </DOMLayer>\n");
                sw.Write("            </layers>\n");
                sw.Write("        </DOMTimeline>\n");
                sw.Write("    </timeline>\n");
                sw.Write("</DOMSymbolItem>");
            }
        }

        static string GetNameByID(string ID, string labelname, int labelindex)
        {
            if (UseLabelName > 0)
            {
                if (labelindex != 0)
                {
                    return labelname + "_" + labelindex;
                }
                return labelname;
            }
            else if (UseLabelName < 0)
            {
                return ID.ToLowerInvariant();
            }
            else
            {
                string name = ID;
                if (name.StartsWith("IMAGE_REANIM_", StringComparison.OrdinalIgnoreCase))
                {
                    name = name[13..];
                }
                return name.ToLowerInvariant();
            }
        }

        static int UseLabelName = 0;
        static double ScaleX = 1;
        static double ScaleY = 1;

        #region PopStudioPicture
        public static readonly byte[] defaultPicture = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52, 0x00, 0x00, 0x00, 0x6A, 0x00, 0x00, 0x00, 0x2B, 0x08, 0x02, 0x00, 0x00, 0x00, 0x1F, 0x47, 0x8A, 0x09, 0x00, 0x00, 0x00, 0x01, 0x73, 0x52, 0x47, 0x42, 0x00, 0xAE, 0xCE, 0x1C, 0xE9, 0x00, 0x00, 0x00, 0x04, 0x67, 0x41, 0x4D, 0x41, 0x00, 0x00, 0xB1, 0x8F, 0x0B, 0xFC, 0x61, 0x05, 0x00, 0x00, 0x00, 0x09, 0x70, 0x48, 0x59, 0x73, 0x00, 0x00, 0x12, 0x74, 0x00, 0x00, 0x12, 0x74, 0x01, 0xDE, 0x66, 0x1F, 0x78, 0x00, 0x00, 0x02, 0x00, 0x49, 0x44, 0x41, 0x54, 0x68, 0x43, 0xED, 0x96, 0xEB, 0xAD, 0xC2, 0x30, 0x0C, 0x46, 0x99, 0xAB, 0x03, 0x31, 0x0F, 0xD3, 0xB0, 0x0C, 0xC3, 0x70, 0xDB, 0x38, 0x89, 0xED, 0xE4, 0x4B, 0x5F, 0x5F, 0xAF, 0x90, 0x2A, 0x9F, 0x7F, 0x71, 0x1C, 0x3F, 0x0E, 0x02,
            0xF1, 0xF8, 0x06, 0x04, 0xA1, 0x8F, 0x22, 0xF4, 0x51, 0x84, 0x3E, 0x8A, 0xD0, 0x47, 0x11, 0xFA, 0x28, 0x42, 0x1F, 0x45, 0xE8, 0xA3, 0x08, 0x7D, 0x14, 0xA1, 0x8F, 0x22, 0xF4, 0x51, 0x84, 0x3E, 0x8A, 0xD0, 0x47, 0x11, 0xFA, 0x28, 0x56, 0xF5, 0xBD, 0x9F, 0x8F, 0x96, 0xE9, 0xF5, 0xC9, 0x97, 0x97, 0xF2, 0x79, 0x4D, 0xB9, 0xC1, 0xC2, 0xF3, 0x9D, 0xC3, 0xD7, 0x92, 0xD6, 0xA9, 0xB5, 0xA5, 0x25, 0xB9, 0xCF, 0xB6, 0x3E, 0xDB, 0x40, 0x7C, 0x5E, 0xAC, 0x50, 0xF6, 0x30, 0xCA, 0xE6, 0x40, 0xD7, 0xF4, 0x0A, 0xA3, 0x3F, 0xD7, 0x07, 0x76, 0x65, 0x49, 0x05, 0xD7, 0x96, 0xB8, 0xAE, 0xE3, 0x65, 0x9F, 0x83, 0x72, 0x54, 0xDF, 0xF6, 0xBA, 0x07, 0x41, 0x3D, 0x1C, 0xF7, 0xD2, 0xD7, 0xC7, 0x52, 0xA4, 0xE2, 0xC7, 0xAB, 0xD9, 0x2E, 0xC9, 0xE6, 0x88, 0x1D, 0xEC, 0x4F, 0xEE, 0x2C, 0x92, 0x07, 0xE6, 0x42, 0x9F, 0xAA, 0xED, 0x59,
            0x67, 0xD0, 0xDE, 0xCD, 0x31, 0xE1, 0xC6, 0xDC, 0xA1, 0xFA, 0xD4, 0x97, 0x57, 0x43, 0xD2, 0xAE, 0xB6, 0xE9, 0x5C, 0xD4, 0x71, 0xDA, 0x27, 0x7A, 0x2E, 0x8E, 0x7C, 0x1F, 0x45, 0xEE, 0xDD, 0x26, 0xDB, 0x73, 0x75, 0x93, 0xCC, 0x4F, 0xA6, 0xC9, 0x17, 0x4A, 0x55, 0x4C, 0x5D, 0x19, 0x6C, 0xBC, 0x0B, 0xE2, 0x90, 0x3E, 0x30, 0x53, 0xD7, 0xC0, 0xC7, 0x64, 0xA4, 0x26, 0xA5, 0x13, 0x22, 0x81, 0x85, 0x26, 0x73, 0xA1, 0xCB, 0x86, 0x7D, 0x53, 0x56, 0x0D, 0x81, 0x37, 0x65, 0x16, 0x0D, 0xA6, 0x73, 0x3D, 0x82, 0x9A, 0x30, 0xE6, 0xD9, 0xD6, 0xE7, 0x70, 0xB5, 0x7C, 0xFF, 0x82, 0xEB, 0x89, 0x07, 0xF0, 0xBB, 0x66, 0xB4, 0x99, 0xBF, 0x39, 0xA1, 0x0F, 0xD6, 0xEF, 0xE6, 0x75, 0xC7, 0xE6, 0x2E, 0x03, 0x1A, 0x79, 0x0E, 0xFF, 0xF6, 0x29, 0x83, 0x21, 0x5D, 0x18, 0x4F, 0x35, 0x0A, 0xE7, 0x8B, 0x19, 0x53, 0xF6, 0x84, 0xBE, 0x7D,
            0x5D, 0xED, 0x71, 0xCF, 0x2E, 0x88, 0x9F, 0xE8, 0x4B, 0x29, 0x60, 0xBF, 0x84, 0x5C, 0xEA, 0x2D, 0x48, 0x06, 0x73, 0xED, 0xE8, 0xDA, 0x86, 0xED, 0xD1, 0x3D, 0x57, 0x06, 0x61, 0x85, 0xD0, 0x37, 0x98, 0xD2, 0x45, 0x71, 0x85, 0x43, 0x75, 0xCF, 0xEA, 0xEB, 0xEA, 0xB7, 0x85, 0x5C, 0x1B, 0xDF, 0xB3, 0x80, 0xA3, 0x06, 0x46, 0x1F, 0xBC, 0xF7, 0x1D, 0xD3, 0x09, 0xA6, 0x8C, 0xCB, 0x36, 0x6B, 0x02, 0x7D, 0x7D, 0x48, 0x22, 0xB5, 0x26, 0xEA, 0x20, 0x29, 0xE6, 0x15, 0x98, 0x74, 0x75, 0x17, 0x04, 0xA5, 0x2F, 0x67, 0x68, 0x87, 0x6E, 0x2F, 0xB9, 0xB7, 0xB1, 0x66, 0xD1, 0x94, 0x61, 0x27, 0x1C, 0xA9, 0xB1, 0x91, 0xB6, 0x88, 0x1C, 0xDD, 0xA8, 0x60, 0xB0, 0x53, 0x7F, 0x5C, 0x5C, 0xDB, 0x1E, 0x52, 0xDF, 0x4C, 0x99, 0x5D, 0x68, 0xB2, 0x4B, 0x05, 0x19, 0x2D, 0xD3, 0x8E, 0xE4, 0x2B, 0x80, 0x86, 0x9A, 0xA0, 0x77, 0xF6, 0xD1,
            0x1C, 0x4D, 0x47, 0xFF, 0xB2, 0xEB, 0x99, 0x02, 0xDA, 0xBC, 0x39, 0x2E, 0xAC, 0xEE, 0x82, 0x58, 0xD5, 0xC7, 0x53, 0xF4, 0xE5, 0xE3, 0xED, 0x08, 0x7D, 0x14, 0xA1, 0x8F, 0x22, 0xF4, 0x51, 0xFC, 0xB3, 0xBE, 0xBB, 0x13, 0xFA, 0x28, 0x42, 0x1F, 0x45, 0xE8, 0xA3, 0x08, 0x7D, 0x14, 0xA1, 0x8F, 0x22, 0xF4, 0x51, 0x84, 0x3E, 0x8A, 0xD0, 0x47, 0x11, 0xFA, 0x28, 0x42, 0x1F, 0xC1, 0xF7, 0xFB, 0x07, 0x82, 0x99, 0x4A, 0x2F, 0x2A, 0x15, 0xF4, 0xD5, 0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE, 0x42, 0x60, 0x82 };
        #endregion
    }
}
