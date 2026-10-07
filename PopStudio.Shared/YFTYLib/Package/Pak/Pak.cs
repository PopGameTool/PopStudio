using System.Xml;

using XMemCompressionDotNet;

namespace PopStudio.Package.Pak
{
    /// <summary>
    /// SexyApp Framework Pak 
    /// </summary>
    internal static class Pak
    {
        public static Dictionary<string, CompressFlags> compressDictionary;
        public static CompressFlags defaultcompressMethod;

        public static void Pack(string inFolder, string outFile)
        {
            bool TVPak = false;
            Dir.FormatAndDeleteEndPathSeparator(ref inFolder);
            Dir.FormatAndDeleteEndPathSeparator(ref outFile);
            if (!Directory.Exists(inFolder))
            {
                throw new Exception(string.Format(Str.Obj.FolderNotFound, inFolder));
            }
            Dir.NewDir(outFile, false);
            PakInfo pak = new PakInfo
            {
                fileInfoLibrary = new List<FileInfo>()
            };
            //read some parameter
            string xmlpath = inFolder + Const.PATHSEPARATOR + "popstudioinfo" + Const.PATHSEPARATOR + "packinfo.xml";
            if (File.Exists(xmlpath))
            {
                string xmldata = File.ReadAllText(xmlpath);
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(xmldata);
                var root = xml.SelectSingleNode("/PackInfo");
                if (root == null) return;
                var childlist = root.ChildNodes;
                foreach (XmlNode child in childlist)
                {
                    if (child.Name == "PCVersion")
                    {
                        pak.pc = Convert.ToBoolean(child.InnerText);
                    }
                    else if (child.Name == "TVVersion")
                    {
                        TVPak = Convert.ToBoolean(child.InnerText);
                    }
                    else if (child.Name == "WindowsPathSeparate")
                    {
                        pak.win = Convert.ToBoolean(child.InnerText);
                    }
                    else if (child.Name == "Xbox360PtxAlign")
                    {
                        pak.x360 = Convert.ToBoolean(child.InnerText);
                    }
                    else if (child.Name == "XmemCompress")
                    {
                        pak.xmem = Convert.ToBoolean(child.InnerText);
                    }
                    else if (child.Name == "ZlibCompress")
                    {
                        pak.compress = Convert.ToBoolean(child.InnerText);
                    }
                }
            }
            else
            {
                pak.pc = true;
                pak.win = true;
                pak.x360 = false;
                pak.xmem = false;
                pak.compress = false;
            }
            if (TVPak)
            {
                ZipFile.CreateFromDirectory(inFolder, outFile); //The file in popstudio will enter the zip file, but it doesn't matter.
                return;
            }
            if (pak.xmem && (pak.pc || pak.compress == true))
            {
                throw new InvalidDataException(Str.Obj.XmemCompressInvalid);
            }
            string[] a = Dir.GetFiles(inFolder);
            int temp = inFolder.Length + 1;
            using TempFilePool tempFilePool = new TempFilePool();
            string tempFile = outFile;
            if (pak.pc || pak.xmem) tempFile = tempFilePool.Add();
            using (BinaryStream bs_files = new BinaryStream(tempFile, FileMode.Create))
            {
                bs_files.Encode = EncodeHelper.ANSI;
                //write head
                for (int i = 0; i < a.Length; i++)
                {
                    if (a[i] == xmlpath) continue;
                    FileInfo info = new FileInfo();
                    pak.fileInfoLibrary.Add(info);
                    if (pak.win)
                    {
                        info.fileName = Dir.FormatWindowsPath(a[i][temp..]);
                    }
                    else
                    {
                        info.fileName = Dir.FormatLinuxPath(a[i][temp..]);
                    }
                }
                pak.Write(bs_files);
                int jian = 0;
                compressDictionary = Setting.PakPS3CompressDictionary;
                defaultcompressMethod = Setting.PakPS3DefaultCompressMethod;
                for (int i = 0; i < a.Length; i++)
                {
                    if (a[i] == xmlpath)
                    {
                        jian++;
                        continue;
                    }
                    if (!pak.pc)
                    {
                        if (pak.x360 && Path.GetExtension(a[i]).ToLower() == ".ptx")
                        {
                            PakInfo.Fill0x1000(bs_files);
                        }
                        else
                        {
                            PakInfo.Fill(bs_files);
                        }
                    }
                    FileInfo info = pak.fileInfoLibrary[i - jian];
                    using (BinaryStream bs_thisfile = new BinaryStream(a[i], FileMode.Open))
                    {
                        if (pak.compress == true)
                        {
                            CompressFlags method = GetCompressMethod(a[i]);
                            switch (method)
                            {
                                case CompressFlags.STORE:
                                    bs_thisfile.CopyTo(bs_files);
                                    info.zsize = (int)bs_thisfile.Length;
                                    break;
                                case CompressFlags.ZLIB:
                                    using (BinaryStream bs3 = new BinaryStream())
                                    {
                                        using (ZLibStream zLibStream = new ZLibStream(bs3, CompressionMode.Compress, true))
                                        {
                                            bs_thisfile.CopyTo(zLibStream);
                                        }
                                        bs3.Position = 0;
                                        bs3.CopyTo(bs_files);
                                        info.size = (int)bs_thisfile.Length;
                                        info.zsize = (int)bs3.Length;
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            bs_thisfile.CopyTo(bs_files);
                            info.zsize = (int)bs_thisfile.Length;
                        }
                    }
                }
                bs_files.Position = 0;
                pak.Write(bs_files);
                if (pak.pc)
                {
                    using (BinaryStream bs = new BinaryStream(outFile, FileMode.Create))
                    {
                        int endlength = (int)bs_files.Length;
                        bs_files.Position = 0;
                        for (int i = 0; i < endlength; i++)
                        {
                            bs.WriteUInt8((byte)(bs_files.ReadUInt8() ^ 0xF7));
                        }
                    }
                }
                compressDictionary = null;
            }
            if (pak.xmem)
            {
                // The XMem TD wrapper compresses the complete, aligned console PAK.
                XMem.XMemCompressLzxTdFile(tempFile, outFile, overwrite: true);
            }
        }

        public static CompressFlags GetCompressMethod(string fileName)
        {
            string extension = Path.GetExtension(fileName).ToLower();
            if (compressDictionary.ContainsKey(extension))
            {
                return compressDictionary[extension];
            }
            return defaultcompressMethod;
        }

        public static void Unpack(string inFile, string outFolder, bool changeimage = false, bool delete = false)
        {
            Dir.FormatAndDeleteEndPathSeparator(ref inFile);
            Dir.FormatAndDeleteEndPathSeparator(ref outFolder);
            if (!File.Exists(inFile))
            {
                throw new Exception(string.Format(Str.Obj.FileNotFound, inFile));
            }
            Dir.NewDir(outFolder);
            outFolder += Const.PATHSEPARATOR;
            string tempName;
            using TempFilePool tempFilePool = new TempFilePool();
            using (BinaryStream bs = new BinaryStream())
            {
                bs.Encode = EncodeHelper.ANSI;
                bool TVMode = false;
                PakInfo pak = new PakInfo();
                using (BinaryStream bs_origin = BinaryStream.Open(inFile))
                {
                    int magic = bs_origin.PeekInt32();
                    if (magic == 1295498551) //37 BD 37 4D
                    {
                        //pak PC xor 0xf7 encrypt
                        pak.pc = true;
                        //pak.compress = false;
                        int l = (int)bs_origin.Length;
                        for (int i = 0; i < l; i++)
                        {
                            bs.WriteUInt8((byte)(bs_origin.ReadUInt8() ^ 0xF7));
                        }
                    }
                    else if (magic == -1161803072) //C0 4A C0 BA
                    {
                        //pak game console
                        bs_origin.CopyTo(bs);
                        //bs.WriteBytes(bs_origin.ReadBytes((int)bs_origin.Length));
                    }
                    else if (magic == -317524721 || magic == 0x0FF512ED) //XMem TD, big/little endian
                    {
                        pak.xmem = true;
                        pak.compress = false;
                    }
                    else if (magic == 67324752) //TV(zip)50 4B 03 04
                    {
                        TVMode = true;
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                if (pak.xmem)
                {
                    // Keep the expanded archive on disk, especially for mobile devices.
                    string expandedPak = tempFilePool.Add();
                    XMem.XMemDecompressLzxTdFile(inFile, expandedPak, overwrite: true);
                    bs.BaseStream.Dispose();
                    bs.BaseStream = File.OpenRead(expandedPak);
                }
                bs.Position = 0;
                if (TVMode)
                {
                    ZipFile.ExtractToDirectory(inFile, outFolder, true); //in .net standard it can't work well in Android when decompress a zip file from Windows
                }
                else
                {
                    pak.Read(bs);
                    if (pak.fileInfoLibrary == null) return;
                    for (int i = 0; i < pak.fileInfoLibrary.Count; i++)
                    {
                        if (!pak.pc) pak.x360 |= PakInfo.Jump(bs);
                        tempName = Dir.FormatPath(outFolder + pak.fileInfoLibrary[i].fileName);
                        Dir.NewDir(tempName, false);
                        byte firstbyte = 0;
                        if (pak.fileInfoLibrary[i].size != 0)
                        {
                            using (BinaryStream bs2 = new BinaryStream())
                            {
                                bs2.WriteBytes(bs.ReadBytes(pak.fileInfoLibrary[i].zsize));
                                bs2.Position = 0;
                                using (ZLibStream zLibStream = new ZLibStream(bs2, CompressionMode.Decompress))
                                {
                                    using (BinaryStream bs3 = new BinaryStream(tempName, FileMode.Create))
                                    {
                                        zLibStream.CopyTo(bs3);
                                        bs3.Position = 0;
                                        if (bs3.Length != 0) firstbyte = bs3.ReadByte();
                                    }
                                }
                            }
                        }
                        else
                        {
                            using (BinaryStream bs2 = new BinaryStream(tempName, FileMode.Create))
                            {
                                bs2.WriteBytes(bs.ReadBytes(pak.fileInfoLibrary[i].zsize));
                                bs2.Position = 0;
                                if (bs2.Length != 0) firstbyte = bs2.ReadByte();
                            }
                        }
                        if (changeimage && Path.GetExtension(tempName).ToLower() == ".ptx")
                        {
                            if (pak.x360)
                            {
                                Image.PtxXbox360.Ptx.Decode(tempName, Path.ChangeExtension(tempName, ".png"));
                                if (delete) File.Delete(tempName);
                            }
                            else
                            {
                                if (firstbyte == 0x44)
                                {
                                    Image.PtxPS3.Ptx.Decode(tempName, Path.ChangeExtension(tempName, ".png"));
                                    if (delete) File.Delete(tempName);
                                }
                                else if (firstbyte == 0x47)
                                {
                                    Image.PtxPSV.Ptx.Decode(tempName, Path.ChangeExtension(tempName, ".png"));
                                    if (delete) File.Delete(tempName);
                                }
                            }
                        }
                    }
                }
                string lst = outFolder + "popstudioinfo";
                Dir.NewDir(lst);
                using (StreamWriter sw = new StreamWriter(lst + Const.PATHSEPARATOR + "packinfo.xml", false))
                {
                    sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                    sw.WriteLine("<!-- DO NOT EDIT THIS FILE. This file is generated by PopStudio. (unless you want to change the way for pack) -->");
                    sw.WriteLine("<PackInfo version=\"1\">");
                    sw.WriteLine("    <PCVersion>" + pak.pc + "</PCVersion>");
                    sw.WriteLine("    <TVVersion>" + TVMode + "</TVVersion>");
                    sw.WriteLine("    <WindowsPathSeparate>" + pak.win + "</WindowsPathSeparate>");
                    sw.WriteLine("    <Xbox360PtxAlign>" + pak.x360 + "</Xbox360PtxAlign>");
                    sw.WriteLine("    <XmemCompress>" + pak.xmem + "</XmemCompress>");
                    sw.WriteLine("    <ZlibCompress>" + (pak.compress == true) + "</ZlibCompress>");
                    sw.WriteLine("</PackInfo>");
                }
            }
        }
    }
}
