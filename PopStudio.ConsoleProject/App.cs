using PopStudio.Language.Languages;
using PopStudio.Platform;
using System.Text;

namespace PopStudio.ConsoleProject
{
    internal class App
    {
        public void Start()
        {
            string filePath = ConsoleReader.ReadPath(MAUIStr.Obj.Console_App_FilePath);
            CommandCode index = (CommandCode)ConsoleReader.ReadInt32(GenIndexHelp(MAUIStr.Obj.Console_App_CommandIndex, filePath));
            switch (index)
            {
                case CommandCode.Unpack_Dz:
                    bool decode_dz = ConsoleReader.ReadBoolean(MAUIStr.Obj.Console_App_Unpack_DecodeImage_Ask);
                    bool delete_dz = decode_dz && ConsoleReader.ReadBoolean(MAUIStr.Obj.Console_App_Unpack_DeleteImage_Ask);
                    YFAPI.Unpack(filePath, Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath)) + "_unpack", 0, decode_dz, delete_dz);
                    break;
                case CommandCode.Unpack_Rsb:
                    bool decode_rsb = ConsoleReader.ReadBoolean(MAUIStr.Obj.Console_App_Unpack_DecodeImage_Ask);
                    bool delete_rsb = decode_rsb && ConsoleReader.ReadBoolean(MAUIStr.Obj.Console_App_Unpack_DeleteImage_Ask);
                    YFAPI.Unpack(filePath, Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath)) + "_unpack", 1, decode_rsb, delete_rsb);
                    break;
                case CommandCode.Unpack_Pak:
                    bool decode_pak = ConsoleReader.ReadBoolean(MAUIStr.Obj.Console_App_Unpack_DecodeImage_Ask);
                    bool delete_pak = decode_pak && ConsoleReader.ReadBoolean(MAUIStr.Obj.Console_App_Unpack_DeleteImage_Ask);
                    YFAPI.Unpack(filePath, Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath)) + "_unpack", 2, decode_pak, delete_pak);
                    break;
                case CommandCode.Unpack_Arcv:
                    YFAPI.Unpack(filePath, Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath)) + "_unpack", 3);
                    break;
                case CommandCode.Pack_Dz:
                    YFAPI.Pack(filePath, (filePath.EndsWith("_unpack") ? filePath[..^"_unpack".Length] : filePath) + ".dz", 0);
                    break;
                case CommandCode.Pack_Rsb:
                    YFAPI.Pack(filePath, (filePath.EndsWith("_unpack") ? filePath[..^"_unpack".Length] : filePath) + ".rsb", 1);
                    break;
                case CommandCode.Pack_Pak:
                    YFAPI.Pack(filePath, (filePath.EndsWith("_unpack") ? filePath[..^"_unpack".Length] : filePath) + ".pak", 2);
                    break;
                case CommandCode.Pack_Arcv:
                    YFAPI.Pack(filePath, (filePath.EndsWith("_unpack") ? filePath[..^"_unpack".Length] : filePath) + ".bin", 3);
                    break;
                case CommandCode.CutImage_NewXml:
                    YFAPI.CutImage(filePath, Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath)) + "_cut", ConsoleReader.ReadPath(MAUIStr.Obj.Console_App_Atlas_InfoFilePath_Ask), ConsoleReader.ReadString(MAUIStr.Obj.Console_App_Atlas_InfoID_Ask), 0);
                    break;
                case CommandCode.CutImage_OldXml:
                    YFAPI.CutImage(filePath, Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath)) + "_cut", ConsoleReader.ReadPath(MAUIStr.Obj.Console_App_Atlas_InfoFilePath_Ask), ConsoleReader.ReadString(MAUIStr.Obj.Console_App_Atlas_InfoID_Ask), 1);
                    break;
                case CommandCode.CutImage_AncientXml:
                    YFAPI.CutImage(filePath, Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath)) + "_cut", ConsoleReader.ReadPath(MAUIStr.Obj.Console_App_Atlas_InfoFilePath_Ask), ConsoleReader.ReadString(MAUIStr.Obj.Console_App_Atlas_InfoID_Ask), 2);
                    break;
                case CommandCode.CutImage_Plist:
                    YFAPI.CutImage(filePath, Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath)) + "_cut", ConsoleReader.ReadPath(MAUIStr.Obj.Console_App_Atlas_InfoFilePath_Ask), ConsoleReader.ReadString(MAUIStr.Obj.Console_App_Atlas_InfoID_Ask), 3);
                    break;
                case CommandCode.CutImage_AtlasImageDat:
                    YFAPI.CutImage(filePath, Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath)) + "_cut", ConsoleReader.ReadPath(MAUIStr.Obj.Console_App_Atlas_InfoFilePath_Ask), ConsoleReader.ReadString(MAUIStr.Obj.Console_App_Atlas_InfoID_Ask), 4);
                    break;
                case CommandCode.CutImage_TVAtlasXml:
                    YFAPI.CutImage(filePath, Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath)) + "_cut", ConsoleReader.ReadPath(MAUIStr.Obj.Console_App_Atlas_InfoFilePath_Ask), ConsoleReader.ReadString(MAUIStr.Obj.Console_App_Atlas_InfoID_Ask), 5);
                    break;
                case CommandCode.CutImage_ResRTON:
                    YFAPI.CutImage(filePath, Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath)) + "_cut", ConsoleReader.ReadPath(MAUIStr.Obj.Console_App_Atlas_InfoFilePath_Ask), ConsoleReader.ReadString(MAUIStr.Obj.Console_App_Atlas_InfoID_Ask), 6);
                    break;
                case CommandCode.SpliceImage_NewXml:
                    YFAPI.SpliceImage(filePath, (filePath.EndsWith("_cut") ? filePath[..^"_cut".Length] : filePath) + ".png", ConsoleReader.ReadPath(MAUIStr.Obj.Console_App_Atlas_InfoFilePath_Ask), ConsoleReader.ReadString(MAUIStr.Obj.Console_App_Atlas_InfoID_Ask), 0, ConsoleReader.ReadInt32(MAUIStr.Obj.Console_App_Atlas_Width_Ask), ConsoleReader.ReadInt32(MAUIStr.Obj.Console_App_Atlas_Height_Ask));
                    break;
                case CommandCode.SpliceImage_OldXml:
                    YFAPI.SpliceImage(filePath, (filePath.EndsWith("_cut") ? filePath[..^"_cut".Length] : filePath) + ".png", ConsoleReader.ReadPath(MAUIStr.Obj.Console_App_Atlas_InfoFilePath_Ask), ConsoleReader.ReadString(MAUIStr.Obj.Console_App_Atlas_InfoID_Ask), 1, ConsoleReader.ReadInt32(MAUIStr.Obj.Console_App_Atlas_Width_Ask), ConsoleReader.ReadInt32(MAUIStr.Obj.Console_App_Atlas_Height_Ask));
                    break;
                case CommandCode.SpliceImage_AncientXml:
                    YFAPI.SpliceImage(filePath, (filePath.EndsWith("_cut") ? filePath[..^"_cut".Length] : filePath) + ".png", ConsoleReader.ReadPath(MAUIStr.Obj.Console_App_Atlas_InfoFilePath_Ask), ConsoleReader.ReadString(MAUIStr.Obj.Console_App_Atlas_InfoID_Ask), 2, ConsoleReader.ReadInt32(MAUIStr.Obj.Console_App_Atlas_Width_Ask), ConsoleReader.ReadInt32(MAUIStr.Obj.Console_App_Atlas_Height_Ask));
                    break;
                case CommandCode.SpliceImage_Plist:
                    YFAPI.SpliceImage(filePath, (filePath.EndsWith("_cut") ? filePath[..^"_cut".Length] : filePath) + ".png", ConsoleReader.ReadPath(MAUIStr.Obj.Console_App_Atlas_InfoFilePath_Ask), ConsoleReader.ReadString(MAUIStr.Obj.Console_App_Atlas_InfoID_Ask), 3, ConsoleReader.ReadInt32(MAUIStr.Obj.Console_App_Atlas_Width_Ask), ConsoleReader.ReadInt32(MAUIStr.Obj.Console_App_Atlas_Height_Ask));
                    break;
                case CommandCode.SpliceImage_AtlasImageDat:
                    YFAPI.SpliceImage(filePath, (filePath.EndsWith("_cut") ? filePath[..^"_cut".Length] : filePath) + ".png", ConsoleReader.ReadPath(MAUIStr.Obj.Console_App_Atlas_InfoFilePath_Ask), ConsoleReader.ReadString(MAUIStr.Obj.Console_App_Atlas_InfoID_Ask), 4, ConsoleReader.ReadInt32(MAUIStr.Obj.Console_App_Atlas_Width_Ask), ConsoleReader.ReadInt32(MAUIStr.Obj.Console_App_Atlas_Height_Ask));
                    break;
                case CommandCode.SpliceImage_TVAtlasXml:
                    YFAPI.SpliceImage(filePath, (filePath.EndsWith("_cut") ? filePath[..^"_cut".Length] : filePath) + ".png", ConsoleReader.ReadPath(MAUIStr.Obj.Console_App_Atlas_InfoFilePath_Ask), ConsoleReader.ReadString(MAUIStr.Obj.Console_App_Atlas_InfoID_Ask), 5, ConsoleReader.ReadInt32(MAUIStr.Obj.Console_App_Atlas_Width_Ask), ConsoleReader.ReadInt32(MAUIStr.Obj.Console_App_Atlas_Height_Ask));
                    break;
                case CommandCode.SpliceImage_ResRTON:
                    YFAPI.SpliceImage(filePath, (filePath.EndsWith("_cut") ? filePath[..^"_cut".Length] : filePath) + ".png", ConsoleReader.ReadPath(MAUIStr.Obj.Console_App_Atlas_InfoFilePath_Ask), ConsoleReader.ReadString(MAUIStr.Obj.Console_App_Atlas_InfoID_Ask), 6, ConsoleReader.ReadInt32(MAUIStr.Obj.Console_App_Atlas_Width_Ask), ConsoleReader.ReadInt32(MAUIStr.Obj.Console_App_Atlas_Height_Ask));
                    break;
                case CommandCode.DecodeImage_PtxRsb:
                    YFAPI.DecodeImage(filePath, Path.ChangeExtension(filePath, ".png"), 0);
                    break;
                case CommandCode.DecodeImage_Cdat:
                    YFAPI.DecodeImage(filePath, Path.ChangeExtension(filePath, ".png"), 1);
                    break;
                case CommandCode.DecodeImage_TexiOS:
                    YFAPI.DecodeImage(filePath, Path.ChangeExtension(filePath, ".png"), 2);
                    break;
                case CommandCode.DecodeImage_Txz:
                    YFAPI.DecodeImage(filePath, Path.ChangeExtension(filePath, ".png"), 3);
                    break;
                case CommandCode.DecodeImage_TexTV:
                    YFAPI.DecodeImage(filePath, Path.ChangeExtension(filePath, ".png"), 4);
                    break;
                case CommandCode.DecodeImage_PtxXbox360:
                    YFAPI.DecodeImage(filePath, Path.ChangeExtension(filePath, ".png"), 5);
                    break;
                case CommandCode.DecodeImage_PtxPS3:
                    YFAPI.DecodeImage(filePath, Path.ChangeExtension(filePath, ".png"), 6);
                    break;
                case CommandCode.DecodeImage_PtxPSV:
                    YFAPI.DecodeImage(filePath, Path.ChangeExtension(filePath, ".png"), 7);
                    break;
                case CommandCode.DecodeImage_Xnb:
                    YFAPI.DecodeImage(filePath, Path.ChangeExtension(filePath, ".png"), 8);
                    break;
                case CommandCode.EncodeImage_PtxRsb:
                    YFAPI.EncodeImage(filePath, Path.ChangeExtension(filePath, ".PTX"), 0, ConsoleReader.ReadInt32(GenEncodeImageHelp(MAUIStr.Obj.Console_App_Texture_Format_Ask, 0)));
                    break;
                case CommandCode.EncodeImage_Cdat:
                    YFAPI.EncodeImage(filePath, Path.ChangeExtension(filePath, ".cdat"), 1, ConsoleReader.ReadInt32(GenEncodeImageHelp(MAUIStr.Obj.Console_App_Texture_Format_Ask, 1)));
                    break;
                case CommandCode.EncodeImage_TexiOS:
                    YFAPI.EncodeImage(filePath, Path.ChangeExtension(filePath, ".tex"), 2, ConsoleReader.ReadInt32(GenEncodeImageHelp(MAUIStr.Obj.Console_App_Texture_Format_Ask, 2)));
                    break;
                case CommandCode.EncodeImage_Txz:
                    YFAPI.EncodeImage(filePath, Path.ChangeExtension(filePath, ".txz"), 3, ConsoleReader.ReadInt32(GenEncodeImageHelp(MAUIStr.Obj.Console_App_Texture_Format_Ask, 3)));
                    break;
                case CommandCode.EncodeImage_TexTV:
                    YFAPI.EncodeImage(filePath, Path.ChangeExtension(filePath, ".tex"), 4, ConsoleReader.ReadInt32(GenEncodeImageHelp(MAUIStr.Obj.Console_App_Texture_Format_Ask, 4)));
                    break;
                case CommandCode.EncodeImage_PtxXbox360:
                    YFAPI.EncodeImage(filePath, Path.ChangeExtension(filePath, ".ptx"), 5, ConsoleReader.ReadInt32(GenEncodeImageHelp(MAUIStr.Obj.Console_App_Texture_Format_Ask, 5)));
                    break;
                case CommandCode.EncodeImage_PtxPS3:
                    YFAPI.EncodeImage(filePath, Path.ChangeExtension(filePath, ".ptx"), 6, ConsoleReader.ReadInt32(GenEncodeImageHelp(MAUIStr.Obj.Console_App_Texture_Format_Ask, 6)));
                    break;
                case CommandCode.EncodeImage_PtxPSV:
                    YFAPI.EncodeImage(filePath, Path.ChangeExtension(filePath, ".ptx"), 7, ConsoleReader.ReadInt32(GenEncodeImageHelp(MAUIStr.Obj.Console_App_Texture_Format_Ask, 7)));
                    break;
                case CommandCode.EncodeImage_Xnb:
                    YFAPI.EncodeImage(filePath, Path.ChangeExtension(filePath, ".xnb"), 8, ConsoleReader.ReadInt32(GenEncodeImageHelp(MAUIStr.Obj.Console_App_Texture_Format_Ask, 8)));
                    break;
                case CommandCode.Reanim_ToPCCompiled:
                    YFAPI.ParseReanim(filePath, HandleRPTExtension(filePath, ".reanim.compiled"), 0);
                    break;
                case CommandCode.Reanim_ToPhone32Compiled:
                    YFAPI.ParseReanim(filePath, HandleRPTExtension(filePath, ".reanim.compiled"), 1);
                    break;
                case CommandCode.Reanim_ToPhone64Compiled:
                    YFAPI.ParseReanim(filePath, HandleRPTExtension(filePath, ".reanim.compiled"), 2);
                    break;
                case CommandCode.Reanim_ToWPXnb:
                    YFAPI.ParseReanim(filePath, HandleRPTExtension(filePath, ".xnb"), 3);
                    break;
                case CommandCode.Reanim_ToGameConsoleCompiled:
                    YFAPI.ParseReanim(filePath, HandleRPTExtension(filePath, ".reanim.compiled"), 4);
                    break;
                case CommandCode.Reanim_ToTVCompiled:
                    YFAPI.ParseReanim(filePath, HandleRPTExtension(filePath, ".reanim.compiled"), 5);
                    break;
                case CommandCode.Reanim_ToStudioJson:
                    YFAPI.ParseReanim(filePath, HandleRPTExtension(filePath, ".reanim.json"), 6);
                    break;
                case CommandCode.Reanim_ToRawXml:
                    YFAPI.ParseReanim(filePath, HandleRPTExtension(filePath, ".reanim"), 7);
                    break;
                case CommandCode.Reanim_ToFlashXfl:
                    YFAPI.ParseReanim(filePath, HandleRPTExtension(filePath, null), 8);
                    break;
                case CommandCode.Reanim_ToFlashFla:
                    YFAPI.ParseReanim(filePath, HandleRPTExtension(filePath, ".fla"), 10);
                    break;
                case CommandCode.Particles_ToPCCompiled:
                    YFAPI.ParseParticles(filePath, HandleRPTExtension(filePath, ".xml.compiled"), 0);
                    break;
                case CommandCode.Particles_ToPhone32Compiled:
                    YFAPI.ParseParticles(filePath, HandleRPTExtension(filePath, ".xml.compiled"), 1);
                    break;
                case CommandCode.Particles_ToPhone64Compiled:
                    YFAPI.ParseParticles(filePath, HandleRPTExtension(filePath, ".xml.compiled"), 2);
                    break;
                case CommandCode.Particles_ToWPXnb:
                    YFAPI.ParseParticles(filePath, HandleRPTExtension(filePath, ".xnb"), 3);
                    break;
                case CommandCode.Particles_ToGameConsoleCompiled:
                    YFAPI.ParseParticles(filePath, HandleRPTExtension(filePath, ".xml.compiled"), 4);
                    break;
                case CommandCode.Particles_ToTVCompiled:
                    YFAPI.ParseParticles(filePath, HandleRPTExtension(filePath, ".xml.compiled"), 5);
                    break;
                case CommandCode.Particles_ToStudioJson:
                    YFAPI.ParseParticles(filePath, HandleRPTExtension(filePath, ".xml.json"), 6);
                    break;
                case CommandCode.Particles_ToRawXml:
                    YFAPI.ParseParticles(filePath, HandleRPTExtension(filePath, ".xml"), 7);
                    break;
                case CommandCode.Trail_ToPCCompiled:
                    YFAPI.ParseTrail(filePath, HandleRPTExtension(filePath, ".trail.compiled"), 0);
                    break;
                case CommandCode.Trail_ToPhone32Compiled:
                    YFAPI.ParseTrail(filePath, HandleRPTExtension(filePath, ".trail.compiled"), 1);
                    break;
                case CommandCode.Trail_ToPhone64Compiled:
                    YFAPI.ParseTrail(filePath, HandleRPTExtension(filePath, ".trail.compiled"), 2);
                    break;
                case CommandCode.Trail_ToWPXnb:
                    YFAPI.ParseTrail(filePath, HandleRPTExtension(filePath, ".xnb"), 3);
                    break;
                case CommandCode.Trail_ToGameConsoleCompiled:
                    YFAPI.ParseTrail(filePath, HandleRPTExtension(filePath, ".trail.compiled"), 4);
                    break;
                case CommandCode.Trail_ToTVCompiled:
                    YFAPI.ParseTrail(filePath, HandleRPTExtension(filePath, ".trail.compiled"), 5);
                    break;
                case CommandCode.Trail_ToStudioJson:
                    YFAPI.ParseTrail(filePath, HandleRPTExtension(filePath, ".trail.json"), 6);
                    break;
                case CommandCode.Trail_ToRawXml:
                    YFAPI.ParseTrail(filePath, HandleRPTExtension(filePath, ".trail"), 7);
                    break;
                case CommandCode.DecodePam:
                    YFAPI.Pam(filePath, Path.ChangeExtension(filePath, ".pam.json"), 0, 1);
                    break;
                case CommandCode.EncodePam:
                    YFAPI.Pam(filePath, Path.GetExtension(Path.GetFileNameWithoutExtension(filePath)).ToLower() == ".pam" ? Path.ChangeExtension(Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath)), ".pam") : Path.ChangeExtension(filePath, ".pam"), 1, 0);
                    break;
                case CommandCode.DecodeRTON_Simple:
                    YFAPI.DecodeRTON(filePath, Path.ChangeExtension(filePath, ".json"), 0);
                    break;
                case CommandCode.DecodeRTON_Encrypted:
                    YFAPI.DecodeRTON(filePath, Path.ChangeExtension(filePath, ".json"), 1);
                    break;
                case CommandCode.EncodeRTON_Simple:
                    YFAPI.EncodeRTON(filePath, Path.ChangeExtension(filePath, ".RTON"), 0);
                    break;
                case CommandCode.EncodeRTON_Encrypted:
                    YFAPI.EncodeRTON(filePath, Path.ChangeExtension(filePath, ".RTON"), 1);
                    break;
                case CommandCode.Decompress_Zlib:
                    YFAPI.Decompress(filePath, Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath)) + "_decompress" + Path.GetExtension(filePath), 0);
                    break;
                case CommandCode.Decompress_Gzip:
                    YFAPI.Decompress(filePath, Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath)) + "_decompress" + Path.GetExtension(filePath), 1);
                    break;
                case CommandCode.Decompress_Deflate:
                    YFAPI.Decompress(filePath, Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath)) + "_decompress" + Path.GetExtension(filePath), 2);
                    break;
                case CommandCode.Decompress_Brotli:
                    YFAPI.Decompress(filePath, Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath)) + "_decompress" + Path.GetExtension(filePath), 3);
                    break;
                case CommandCode.Decompress_Lzma:
                    YFAPI.Decompress(filePath, Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath)) + "_decompress" + Path.GetExtension(filePath), 4);
                    break;
                case CommandCode.Decompress_Lz4:
                    YFAPI.Decompress(filePath, Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath)) + "_decompress" + Path.GetExtension(filePath), 5);
                    break;
                case CommandCode.Decompress_Bzip2:
                    YFAPI.Decompress(filePath, Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath)) + "_decompress" + Path.GetExtension(filePath), 6);
                    break;
                case CommandCode.Compress_Zlib:
                    YFAPI.Compress(filePath, filePath + ".zlib", 0);
                    break;
                case CommandCode.Compress_Gzip:
                    YFAPI.Compress(filePath, filePath + ".gz", 1);
                    break;
                case CommandCode.Compress_Deflate:
                    YFAPI.Compress(filePath, filePath + ".deflate", 2);
                    break;
                case CommandCode.Compress_Brotli:
                    YFAPI.Compress(filePath, filePath + ".brotli", 3);
                    break;
                case CommandCode.Compress_Lzma:
                    YFAPI.Compress(filePath, filePath + ".lzma", 4);
                    break;
                case CommandCode.Compress_Lz4:
                    YFAPI.Compress(filePath, filePath + ".lz4", 5);
                    break;
                case CommandCode.Compress_Bzip2:
                    YFAPI.Compress(filePath, filePath + ".bz", 6);
                    break;
                case CommandCode.LoadSetting:
                    Setting.LoadFromXml(filePath);
                    Setting.SaveAsXml(Permission.GetSettingPath());
                    break;
                case CommandCode.LoadImageConvertXml:
                    Setting.LoadImageConvertXml(filePath);
                    Setting.SaveAsXml(Permission.GetSettingPath());
                    break;
                case CommandCode.RunScript:
                    YFAPI.DoScript($"rainy.dofile(\"{filePath.Replace("\\", "\\\\")}\");");
                    break;
                default:
                    throw new Exception(string.Format(MAUIStr.Obj.Console_NoCommand, (int)index));
            }
        }

        string HandleRPTExtension(string filePath, string ex)
        {
            string ex_2 = Path.GetExtension(Path.GetFileNameWithoutExtension(filePath)).ToLower();
            if (ex_2 == ".reanim" || ex_2 == ".xml" || ex_2 == ".trail")
            {
                if (ex == null)
                {
                    return Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(filePath))) + "_xfl";
                }
                return Path.ChangeExtension(Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath)), ex);
            }
            return ex == null ? (Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath)) + "_xfl") : Path.ChangeExtension(filePath, ex);
        }

        string GenEncodeImageHelp(string simpleprintinfo, int format)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(simpleprintinfo);
            void Add(int index, string str)
            {
                sb.AppendLine(string.Format(MAUIStr.Obj.Console_App_Texture_Format_Ask_Formater, index, str));
            }
            switch (format)
            {
                case 0:
                    Add(0, "ARGB8888(0)");
                    Add(1, "ABGR8888(0)");
                    Add(2, "RGBA4444(1)");
                    Add(3, "RGB565(2)");
                    Add(4, "RGBA5551(3)");
                    Add(5, "RGBA4444_Block(21)");
                    Add(6, "RGB565_Block(22)");
                    Add(7, "RGBA5551_Block(23)");
                    Add(8, "XRGB8888_A8(149)");
                    Add(9, "ARGB8888(BE)(0)");
                    Add(10, "ARGB8888_Padding(BE)(0)");
                    Add(11, "DXT1_RGB(35)");
                    Add(12, "DXT3_RGBA(36)");
                    Add(13, "DXT5_RGBA(37)");
                    Add(14, "DXT5_RGBA_MortonBlock(5)");
                    Add(15, "DXT5_RGBA(BE)(5)");
                    Add(16, "ETC1_RGB(32)");
                    Add(17, "ETC1_RGB_A8(147)");
                    Add(18, "ETC1_RGB_A_Palette(147)");
                    Add(19, "ETC1_RGB_A_Palette(150)");
                    Add(20, "PVRTC_4BPP_RGBA(30)");
                    Add(21, "PVRTC_4BPP_RGB_A8(148)");
                    break;
                case 1:
                    Add(0, "Encrypt");
                    break;
                case 2:
                case 3:
                    Add(0, "ABGR8888(1)");
                    Add(1, "RGBA4444(2)");
                    Add(2, "RGBA5551(3)");
                    Add(3, "RGB565(4)");
                    break;
                case 4:
                    Add(0, "LUT8(1)(Invalid)");
                    Add(1, "ARGB8888(2)");
                    Add(2, "ARGB4444(3)");
                    Add(3, "ARGB1555(4)");
                    Add(4, "RGB565(5)");
                    Add(5, "ABGR8888(6)");
                    Add(6, "RGBA4444(7)");
                    Add(7, "RGBA5551(8)");
                    Add(8, "XRGB8888(9)");
                    Add(9, "LA88(10)");
                    Add(10, "LUT8(NoZlib)(1)(Invalid)");
                    Add(11, "ARGB8888(NoZlib)(2)");
                    Add(12, "ARGB4444(NoZlib)(3)");
                    Add(13, "ARGB1555(NoZlib)(4)");
                    Add(14, "RGB565(NoZlib)(5)");
                    Add(15, "ABGR8888(NoZlib)(6)");
                    Add(16, "RGBA4444(NoZlib)(7)");
                    Add(17, "RGBA5551(NoZlib)(8)");
                    Add(18, "XRGB8888(NoZlib)(9)");
                    Add(19, "LA88(NoZlib)(10)");
                    break;
                case 5:
                    Add(0, "DXT5_RGBA_Padding(BE)");
                    break;
                case 6:
                    Add(0, "DXT5_RGBA");
                    break;
                case 7:
                    Add(0, "DXT5_RGBA_Morton");
                    break;
                case 8:
                    Add(0, "ABGR8888");
                    break;
            }
            return sb.ToString();
        }

        string GenIndexHelp(string simpleprintinfo, string filePath)
        {
            //Get extension
            List<CommandCode> codeList = new List<CommandCode>();
            if (Directory.Exists(filePath))
            {
                codeList.Add(CommandCode.Pack_Dz);
                codeList.Add(CommandCode.Pack_Rsb);
                codeList.Add(CommandCode.Pack_Pak);
                codeList.Add(CommandCode.Pack_Arcv);
                codeList.Add(CommandCode.SpliceImage_NewXml);
                codeList.Add(CommandCode.SpliceImage_OldXml);
                codeList.Add(CommandCode.SpliceImage_AncientXml);
                codeList.Add(CommandCode.SpliceImage_Plist);
                codeList.Add(CommandCode.SpliceImage_AtlasImageDat);
                codeList.Add(CommandCode.SpliceImage_TVAtlasXml);
                codeList.Add(CommandCode.SpliceImage_ResRTON);
                if (File.Exists(Path.Combine(filePath, "DOMDocument.xml")))
                {
                    AddReanimConversions(codeList);
                }
            }
            else
            {
                bool b = false;
                string Ex1 = Path.GetExtension(filePath).ToLower();
                string Ex2 = Path.GetExtension(Path.GetFileNameWithoutExtension(filePath)).ToLower();
                switch (Ex2 + Ex1)
                {
                    case ".pam.json":
                        codeList.Add(CommandCode.EncodePam);
                        break;
                    case ".reanim.json":
                        codeList.Add(CommandCode.Reanim_ToPCCompiled);
                        codeList.Add(CommandCode.Reanim_ToPhone32Compiled);
                        codeList.Add(CommandCode.Reanim_ToPhone64Compiled);
                        codeList.Add(CommandCode.Reanim_ToWPXnb);
                        codeList.Add(CommandCode.Reanim_ToGameConsoleCompiled);
                        codeList.Add(CommandCode.Reanim_ToTVCompiled);
                        codeList.Add(CommandCode.Reanim_ToRawXml);
                        codeList.Add(CommandCode.Reanim_ToFlashXfl);
                        codeList.Add(CommandCode.Reanim_ToFlashFla);
                        break;
                    case ".xml.json":
                        codeList.Add(CommandCode.Particles_ToPCCompiled);
                        codeList.Add(CommandCode.Particles_ToPhone32Compiled);
                        codeList.Add(CommandCode.Particles_ToPhone64Compiled);
                        codeList.Add(CommandCode.Particles_ToWPXnb);
                        codeList.Add(CommandCode.Particles_ToGameConsoleCompiled);
                        codeList.Add(CommandCode.Particles_ToTVCompiled);
                        codeList.Add(CommandCode.Particles_ToRawXml);
                        break;
                    case ".trail.json":
                        codeList.Add(CommandCode.Trail_ToPCCompiled);
                        codeList.Add(CommandCode.Trail_ToPhone32Compiled);
                        codeList.Add(CommandCode.Trail_ToPhone64Compiled);
                        codeList.Add(CommandCode.Trail_ToWPXnb);
                        codeList.Add(CommandCode.Trail_ToGameConsoleCompiled);
                        codeList.Add(CommandCode.Trail_ToTVCompiled);
                        codeList.Add(CommandCode.Trail_ToRawXml);
                        break;
                    case ".reanim.compiled":
                        codeList.Add(CommandCode.Reanim_ToPCCompiled);
                        codeList.Add(CommandCode.Reanim_ToPhone32Compiled);
                        codeList.Add(CommandCode.Reanim_ToPhone64Compiled);
                        codeList.Add(CommandCode.Reanim_ToWPXnb);
                        codeList.Add(CommandCode.Reanim_ToGameConsoleCompiled);
                        codeList.Add(CommandCode.Reanim_ToTVCompiled);
                        codeList.Add(CommandCode.Reanim_ToStudioJson);
                        codeList.Add(CommandCode.Reanim_ToRawXml);
                        codeList.Add(CommandCode.Reanim_ToFlashXfl);
                        codeList.Add(CommandCode.Reanim_ToFlashFla);
                        break;
                    case ".xml.compiled":
                        codeList.Add(CommandCode.Particles_ToPCCompiled);
                        codeList.Add(CommandCode.Particles_ToPhone32Compiled);
                        codeList.Add(CommandCode.Particles_ToPhone64Compiled);
                        codeList.Add(CommandCode.Particles_ToWPXnb);
                        codeList.Add(CommandCode.Particles_ToGameConsoleCompiled);
                        codeList.Add(CommandCode.Particles_ToTVCompiled);
                        codeList.Add(CommandCode.Particles_ToStudioJson);
                        codeList.Add(CommandCode.Particles_ToRawXml);
                        break;
                    case ".trail.compiled":
                        codeList.Add(CommandCode.Trail_ToPCCompiled);
                        codeList.Add(CommandCode.Trail_ToPhone32Compiled);
                        codeList.Add(CommandCode.Trail_ToPhone64Compiled);
                        codeList.Add(CommandCode.Trail_ToWPXnb);
                        codeList.Add(CommandCode.Trail_ToGameConsoleCompiled);
                        codeList.Add(CommandCode.Trail_ToTVCompiled);
                        codeList.Add(CommandCode.Trail_ToStudioJson);
                        codeList.Add(CommandCode.Trail_ToRawXml);
                        break;
                    case ".rsb.smf":
                        codeList.Add(CommandCode.Unpack_Rsb);
                        break;
                    default:
                        switch (Ex1)
                        {
                            case ".xfl":
                                AddReanimConversions(codeList);
                                break;
                            case ".dz":
                                codeList.Add(CommandCode.Unpack_Dz);
                                break;
                            case ".rsb":
                            case ".obb":
                                codeList.Add(CommandCode.Unpack_Rsb);
                                break;
                            case ".pak":
                                codeList.Add(CommandCode.Unpack_Pak);
                                break;
                            case ".arcv":
                            case ".bin":
                                codeList.Add(CommandCode.Unpack_Arcv);
                                break;
                            case ".png":
                            case ".jpg":
                            case ".gif":
                                codeList.Add(CommandCode.CutImage_NewXml);
                                codeList.Add(CommandCode.CutImage_OldXml);
                                codeList.Add(CommandCode.CutImage_AncientXml);
                                codeList.Add(CommandCode.CutImage_Plist);
                                codeList.Add(CommandCode.CutImage_AtlasImageDat);
                                codeList.Add(CommandCode.CutImage_TVAtlasXml);
                                codeList.Add(CommandCode.CutImage_ResRTON);
                                codeList.Add(CommandCode.EncodeImage_PtxRsb);
                                codeList.Add(CommandCode.EncodeImage_Cdat);
                                codeList.Add(CommandCode.EncodeImage_TexiOS);
                                codeList.Add(CommandCode.EncodeImage_Txz);
                                codeList.Add(CommandCode.EncodeImage_TexTV);
                                codeList.Add(CommandCode.EncodeImage_PtxXbox360);
                                codeList.Add(CommandCode.EncodeImage_PtxPS3);
                                codeList.Add(CommandCode.EncodeImage_PtxPSV);
                                codeList.Add(CommandCode.EncodeImage_Xnb);
                                break;
                            case ".ptx":
                                codeList.Add(CommandCode.DecodeImage_PtxRsb);
                                codeList.Add(CommandCode.DecodeImage_PtxXbox360);
                                codeList.Add(CommandCode.DecodeImage_PtxPS3);
                                codeList.Add(CommandCode.DecodeImage_PtxPSV);
                                break;
                            case ".cdat":
                                codeList.Add(CommandCode.DecodeImage_Cdat);
                                break;
                            case ".tex":
                                codeList.Add(CommandCode.DecodeImage_TexiOS);
                                codeList.Add(CommandCode.DecodeImage_TexTV);
                                break;
                            case ".txz":
                                codeList.Add(CommandCode.DecodeImage_Txz);
                                break;
                            case ".xnb":
                                codeList.Add(CommandCode.DecodeImage_Xnb);
                                codeList.Add(CommandCode.Reanim_ToPCCompiled);
                                codeList.Add(CommandCode.Reanim_ToPhone32Compiled);
                                codeList.Add(CommandCode.Reanim_ToPhone64Compiled);
                                codeList.Add(CommandCode.Reanim_ToGameConsoleCompiled);
                                codeList.Add(CommandCode.Reanim_ToTVCompiled);
                                codeList.Add(CommandCode.Reanim_ToStudioJson);
                                codeList.Add(CommandCode.Reanim_ToRawXml);
                                codeList.Add(CommandCode.Reanim_ToFlashXfl);
                                codeList.Add(CommandCode.Reanim_ToFlashFla);
                                codeList.Add(CommandCode.Particles_ToPCCompiled);
                                codeList.Add(CommandCode.Particles_ToPhone32Compiled);
                                codeList.Add(CommandCode.Particles_ToPhone64Compiled);
                                codeList.Add(CommandCode.Particles_ToGameConsoleCompiled);
                                codeList.Add(CommandCode.Particles_ToTVCompiled);
                                codeList.Add(CommandCode.Particles_ToStudioJson);
                                codeList.Add(CommandCode.Particles_ToRawXml);
                                codeList.Add(CommandCode.Trail_ToPCCompiled);
                                codeList.Add(CommandCode.Trail_ToPhone32Compiled);
                                codeList.Add(CommandCode.Trail_ToPhone64Compiled);
                                codeList.Add(CommandCode.Trail_ToGameConsoleCompiled);
                                codeList.Add(CommandCode.Trail_ToTVCompiled);
                                codeList.Add(CommandCode.Trail_ToStudioJson);
                                codeList.Add(CommandCode.Trail_ToRawXml);
                                break;
                            case ".reanim":
                                codeList.Add(CommandCode.Reanim_ToPCCompiled);
                                codeList.Add(CommandCode.Reanim_ToPhone32Compiled);
                                codeList.Add(CommandCode.Reanim_ToPhone64Compiled);
                                codeList.Add(CommandCode.Reanim_ToWPXnb);
                                codeList.Add(CommandCode.Reanim_ToGameConsoleCompiled);
                                codeList.Add(CommandCode.Reanim_ToTVCompiled);
                                codeList.Add(CommandCode.Reanim_ToStudioJson);
                                codeList.Add(CommandCode.Reanim_ToFlashXfl);
                                codeList.Add(CommandCode.Reanim_ToFlashFla);
                                break;
                            case ".xml":
                                codeList.Add(CommandCode.Particles_ToPCCompiled);
                                codeList.Add(CommandCode.Particles_ToPhone32Compiled);
                                codeList.Add(CommandCode.Particles_ToPhone64Compiled);
                                codeList.Add(CommandCode.Particles_ToWPXnb);
                                codeList.Add(CommandCode.Particles_ToGameConsoleCompiled);
                                codeList.Add(CommandCode.Particles_ToTVCompiled);
                                codeList.Add(CommandCode.Particles_ToStudioJson);
                                b = true;
                                break;
                            case ".trail":
                                codeList.Add(CommandCode.Trail_ToPCCompiled);
                                codeList.Add(CommandCode.Trail_ToPhone32Compiled);
                                codeList.Add(CommandCode.Trail_ToPhone64Compiled);
                                codeList.Add(CommandCode.Trail_ToWPXnb);
                                codeList.Add(CommandCode.Trail_ToGameConsoleCompiled);
                                codeList.Add(CommandCode.Trail_ToTVCompiled);
                                codeList.Add(CommandCode.Trail_ToStudioJson);
                                break;
                            case ".pam":
                                codeList.Add(CommandCode.DecodePam);
                                break;
                            case ".rton":
                                codeList.Add(CommandCode.DecodeRTON_Simple);
                                codeList.Add(CommandCode.DecodeRTON_Encrypted);
                                break;
                            case ".json":
                                codeList.Add(CommandCode.EncodeRTON_Simple);
                                codeList.Add(CommandCode.EncodeRTON_Encrypted);
                                break;
                        }
                        break;
                }
                if (!string.Equals(Ex1, ".xfl", StringComparison.OrdinalIgnoreCase)
                    && PopStudio.Reanim.FlashFla.IsZipXfl(filePath))
                {
                    AddReanimConversions(codeList);
                }
                codeList.Add(CommandCode.Decompress_Zlib);
                codeList.Add(CommandCode.Decompress_Gzip);
                codeList.Add(CommandCode.Decompress_Deflate);
                codeList.Add(CommandCode.Decompress_Brotli);
                codeList.Add(CommandCode.Decompress_Lzma);
                codeList.Add(CommandCode.Decompress_Lz4);
                codeList.Add(CommandCode.Decompress_Bzip2);
                codeList.Add(CommandCode.Compress_Zlib);
                codeList.Add(CommandCode.Compress_Gzip);
                codeList.Add(CommandCode.Compress_Deflate);
                codeList.Add(CommandCode.Compress_Brotli);
                codeList.Add(CommandCode.Compress_Lzma);
                codeList.Add(CommandCode.Compress_Lz4);
                codeList.Add(CommandCode.Compress_Bzip2);
                if (b)
                {
                    codeList.Add(CommandCode.LoadSetting);
                    codeList.Add(CommandCode.LoadImageConvertXml);
                }
                codeList.Add(CommandCode.RunScript);
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(simpleprintinfo);
            sb.AppendLine(string.Format(MAUIStr.Obj.Console_App_CommandIndex_Ask_Count, codeList.Count));
            foreach (CommandCode code in codeList)
            {
                sb.AppendLine(string.Format(MAUIStr.Obj.Console_App_CommandIndex_Ask_Formater, (int)code, code switch
                {
                    CommandCode.Unpack_Dz => MAUIStr.Obj.Console_App_CommandIndex_Unpack_Dz,
                    CommandCode.Unpack_Rsb => MAUIStr.Obj.Console_App_CommandIndex_Unpack_Rsb,
                    CommandCode.Unpack_Pak => MAUIStr.Obj.Console_App_CommandIndex_Unpack_Pak,
                    CommandCode.Unpack_Arcv => MAUIStr.Obj.Console_App_CommandIndex_Unpack_Arcv,
                    CommandCode.Pack_Dz => MAUIStr.Obj.Console_App_CommandIndex_Pack_Dz,
                    CommandCode.Pack_Rsb => MAUIStr.Obj.Console_App_CommandIndex_Pack_Rsb,
                    CommandCode.Pack_Pak => MAUIStr.Obj.Console_App_CommandIndex_Pack_Pak,
                    CommandCode.Pack_Arcv => MAUIStr.Obj.Console_App_CommandIndex_Pack_Arcv,
                    CommandCode.CutImage_NewXml => MAUIStr.Obj.Console_App_CommandIndex_CutImage_NewXml,
                    CommandCode.CutImage_OldXml => MAUIStr.Obj.Console_App_CommandIndex_CutImage_OldXml,
                    CommandCode.CutImage_AncientXml => MAUIStr.Obj.Console_App_CommandIndex_CutImage_AncientXml,
                    CommandCode.CutImage_Plist => MAUIStr.Obj.Console_App_CommandIndex_CutImage_Plist,
                    CommandCode.CutImage_AtlasImageDat => MAUIStr.Obj.Console_App_CommandIndex_CutImage_AtlasImageDat,
                    CommandCode.CutImage_TVAtlasXml => MAUIStr.Obj.Console_App_CommandIndex_CutImage_TVAtlasXml,
                    CommandCode.CutImage_ResRTON => MAUIStr.Obj.Console_App_CommandIndex_CutImage_ResRTON,
                    CommandCode.SpliceImage_NewXml => MAUIStr.Obj.Console_App_CommandIndex_SpliceImage_NewXml,
                    CommandCode.SpliceImage_OldXml => MAUIStr.Obj.Console_App_CommandIndex_SpliceImage_OldXml,
                    CommandCode.SpliceImage_AncientXml => MAUIStr.Obj.Console_App_CommandIndex_SpliceImage_AncientXml,
                    CommandCode.SpliceImage_Plist => MAUIStr.Obj.Console_App_CommandIndex_SpliceImage_Plist,
                    CommandCode.SpliceImage_AtlasImageDat => MAUIStr.Obj.Console_App_CommandIndex_SpliceImage_AtlasImageDat,
                    CommandCode.SpliceImage_TVAtlasXml => MAUIStr.Obj.Console_App_CommandIndex_SpliceImage_TVAtlasXml,
                    CommandCode.SpliceImage_ResRTON => MAUIStr.Obj.Console_App_CommandIndex_SpliceImage_ResRTON,
                    CommandCode.DecodeImage_PtxRsb => MAUIStr.Obj.Console_App_CommandIndex_DecodeImage_PtxRsb,
                    CommandCode.DecodeImage_Cdat => MAUIStr.Obj.Console_App_CommandIndex_DecodeImage_Cdat,
                    CommandCode.DecodeImage_TexiOS => MAUIStr.Obj.Console_App_CommandIndex_DecodeImage_TexiOS,
                    CommandCode.DecodeImage_Txz => MAUIStr.Obj.Console_App_CommandIndex_DecodeImage_Txz,
                    CommandCode.DecodeImage_TexTV => MAUIStr.Obj.Console_App_CommandIndex_DecodeImage_TexTV,
                    CommandCode.DecodeImage_PtxXbox360 => MAUIStr.Obj.Console_App_CommandIndex_DecodeImage_PtxXbox360,
                    CommandCode.DecodeImage_PtxPS3 => MAUIStr.Obj.Console_App_CommandIndex_DecodeImage_PtxPS3,
                    CommandCode.DecodeImage_PtxPSV => MAUIStr.Obj.Console_App_CommandIndex_DecodeImage_PtxPSV,
                    CommandCode.DecodeImage_Xnb => MAUIStr.Obj.Console_App_CommandIndex_DecodeImage_Xnb,
                    CommandCode.EncodeImage_PtxRsb => MAUIStr.Obj.Console_App_CommandIndex_EncodeImage_PtxRsb,
                    CommandCode.EncodeImage_Cdat => MAUIStr.Obj.Console_App_CommandIndex_EncodeImage_Cdat,
                    CommandCode.EncodeImage_TexiOS => MAUIStr.Obj.Console_App_CommandIndex_EncodeImage_TexiOS,
                    CommandCode.EncodeImage_Txz => MAUIStr.Obj.Console_App_CommandIndex_EncodeImage_Txz,
                    CommandCode.EncodeImage_TexTV => MAUIStr.Obj.Console_App_CommandIndex_EncodeImage_TexTV,
                    CommandCode.EncodeImage_PtxXbox360 => MAUIStr.Obj.Console_App_CommandIndex_EncodeImage_PtxXbox360,
                    CommandCode.EncodeImage_PtxPS3 => MAUIStr.Obj.Console_App_CommandIndex_EncodeImage_PtxPS3,
                    CommandCode.EncodeImage_PtxPSV => MAUIStr.Obj.Console_App_CommandIndex_EncodeImage_PtxPSV,
                    CommandCode.EncodeImage_Xnb => MAUIStr.Obj.Console_App_CommandIndex_EncodeImage_Xnb,
                    CommandCode.Reanim_ToPCCompiled => MAUIStr.Obj.Console_App_CommandIndex_Reanim_ToPCCompiled,
                    CommandCode.Reanim_ToPhone32Compiled => MAUIStr.Obj.Console_App_CommandIndex_Reanim_ToPhone32Compiled,
                    CommandCode.Reanim_ToPhone64Compiled => MAUIStr.Obj.Console_App_CommandIndex_Reanim_ToPhone64Compiled,
                    CommandCode.Reanim_ToWPXnb => MAUIStr.Obj.Console_App_CommandIndex_Reanim_ToWPXnb,
                    CommandCode.Reanim_ToGameConsoleCompiled => MAUIStr.Obj.Console_App_CommandIndex_Reanim_ToGameConsoleCompiled,
                    CommandCode.Reanim_ToTVCompiled => MAUIStr.Obj.Console_App_CommandIndex_Reanim_ToTVCompiled,
                    CommandCode.Reanim_ToStudioJson => MAUIStr.Obj.Console_App_CommandIndex_Reanim_ToStudioJson,
                    CommandCode.Reanim_ToRawXml => MAUIStr.Obj.Console_App_CommandIndex_Reanim_ToRawXml,
                    CommandCode.Reanim_ToFlashXfl => MAUIStr.Obj.Console_App_CommandIndex_Reanim_ToFlashXfl,
                    CommandCode.Reanim_ToFlashFla => MAUIStr.Obj.Console_App_CommandIndex_Reanim_ToFlashFla,
                    CommandCode.Particles_ToPCCompiled => MAUIStr.Obj.Console_App_CommandIndex_Particles_ToPCCompiled,
                    CommandCode.Particles_ToPhone32Compiled => MAUIStr.Obj.Console_App_CommandIndex_Particles_ToPhone32Compiled,
                    CommandCode.Particles_ToPhone64Compiled => MAUIStr.Obj.Console_App_CommandIndex_Particles_ToPhone64Compiled,
                    CommandCode.Particles_ToWPXnb => MAUIStr.Obj.Console_App_CommandIndex_Particles_ToWPXnb,
                    CommandCode.Particles_ToGameConsoleCompiled => MAUIStr.Obj.Console_App_CommandIndex_Particles_ToGameConsoleCompiled,
                    CommandCode.Particles_ToTVCompiled => MAUIStr.Obj.Console_App_CommandIndex_Particles_ToTVCompiled,
                    CommandCode.Particles_ToStudioJson => MAUIStr.Obj.Console_App_CommandIndex_Particles_ToStudioJson,
                    CommandCode.Particles_ToRawXml => MAUIStr.Obj.Console_App_CommandIndex_Particles_ToRawXml,
                    CommandCode.Trail_ToPCCompiled => MAUIStr.Obj.Console_App_CommandIndex_Trail_ToPCCompiled,
                    CommandCode.Trail_ToPhone32Compiled => MAUIStr.Obj.Console_App_CommandIndex_Trail_ToPhone32Compiled,
                    CommandCode.Trail_ToPhone64Compiled => MAUIStr.Obj.Console_App_CommandIndex_Trail_ToPhone64Compiled,
                    CommandCode.Trail_ToWPXnb => MAUIStr.Obj.Console_App_CommandIndex_Trail_ToWPXnb,
                    CommandCode.Trail_ToGameConsoleCompiled => MAUIStr.Obj.Console_App_CommandIndex_Trail_ToGameConsoleCompiled,
                    CommandCode.Trail_ToTVCompiled => MAUIStr.Obj.Console_App_CommandIndex_Trail_ToTVCompiled,
                    CommandCode.Trail_ToStudioJson => MAUIStr.Obj.Console_App_CommandIndex_Trail_ToStudioJson,
                    CommandCode.Trail_ToRawXml => MAUIStr.Obj.Console_App_CommandIndex_Trail_ToRawXml,
                    CommandCode.DecodePam => MAUIStr.Obj.Console_App_CommandIndex_DecodePam,
                    CommandCode.EncodePam => MAUIStr.Obj.Console_App_CommandIndex_EncodePam,
                    CommandCode.DecodeRTON_Simple => MAUIStr.Obj.Console_App_CommandIndex_DecodeRTON_Simple,
                    CommandCode.DecodeRTON_Encrypted => MAUIStr.Obj.Console_App_CommandIndex_DecodeRTON_Encrypted,
                    CommandCode.EncodeRTON_Simple => MAUIStr.Obj.Console_App_CommandIndex_EncodeRTON_Simple,
                    CommandCode.EncodeRTON_Encrypted => MAUIStr.Obj.Console_App_CommandIndex_EncodeRTON_Encrypted,
                    CommandCode.Decompress_Zlib => MAUIStr.Obj.Console_App_CommandIndex_Decompress_Zlib,
                    CommandCode.Decompress_Gzip => MAUIStr.Obj.Console_App_CommandIndex_Decompress_Gzip,
                    CommandCode.Decompress_Deflate => MAUIStr.Obj.Console_App_CommandIndex_Decompress_Deflate,
                    CommandCode.Decompress_Brotli => MAUIStr.Obj.Console_App_CommandIndex_Decompress_Brotli,
                    CommandCode.Decompress_Lzma => MAUIStr.Obj.Console_App_CommandIndex_Decompress_Lzma,
                    CommandCode.Decompress_Lz4 => MAUIStr.Obj.Console_App_CommandIndex_Decompress_Lz4,
                    CommandCode.Decompress_Bzip2 => MAUIStr.Obj.Console_App_CommandIndex_Decompress_Bzip2,
                    CommandCode.Compress_Zlib => MAUIStr.Obj.Console_App_CommandIndex_Compress_Zlib,
                    CommandCode.Compress_Gzip => MAUIStr.Obj.Console_App_CommandIndex_Compress_Gzip,
                    CommandCode.Compress_Deflate => MAUIStr.Obj.Console_App_CommandIndex_Compress_Deflate,
                    CommandCode.Compress_Brotli => MAUIStr.Obj.Console_App_CommandIndex_Compress_Brotli,
                    CommandCode.Compress_Lzma => MAUIStr.Obj.Console_App_CommandIndex_Compress_Lzma,
                    CommandCode.Compress_Lz4 => MAUIStr.Obj.Console_App_CommandIndex_Compress_Lz4,
                    CommandCode.Compress_Bzip2 => MAUIStr.Obj.Console_App_CommandIndex_Compress_Bzip2,
                    CommandCode.LoadSetting => MAUIStr.Obj.Console_App_CommandIndex_LoadSetting,
                    CommandCode.LoadImageConvertXml => MAUIStr.Obj.Console_App_CommandIndex_LoadImageConvertXml,
                    CommandCode.RunScript => MAUIStr.Obj.Console_App_CommandIndex_RunScript,
                    _ => throw new Exception()
                }));
            }
            return sb.ToString();
        }

        static void AddReanimConversions(List<CommandCode> codeList)
        {
            codeList.Add(CommandCode.Reanim_ToPCCompiled);
            codeList.Add(CommandCode.Reanim_ToPhone32Compiled);
            codeList.Add(CommandCode.Reanim_ToPhone64Compiled);
            codeList.Add(CommandCode.Reanim_ToWPXnb);
            codeList.Add(CommandCode.Reanim_ToGameConsoleCompiled);
            codeList.Add(CommandCode.Reanim_ToTVCompiled);
            codeList.Add(CommandCode.Reanim_ToStudioJson);
            codeList.Add(CommandCode.Reanim_ToRawXml);
            codeList.Add(CommandCode.Reanim_ToFlashXfl);
            codeList.Add(CommandCode.Reanim_ToFlashFla);
        }

        enum CommandCode
        {
            Unpack_Dz = 0, // 解包dz数据包
            Unpack_Rsb, // 解包rsb数据包
            Unpack_Pak, // 解包pak数据包
            Unpack_Arcv, // 解包arcv数据包
            Pack_Dz = 20, // 打包dz数据包
            Pack_Rsb, // 打包rsb数据包
            Pack_Pak, // 打包pak数据包
            Pack_Arcv, // 打包arcv数据包
            CutImage_NewXml = 40, // 以RESOURCES.XML(Rsb)格式切割图像
            CutImage_OldXml, // 以resources.xml(Old)格式切割图像
            CutImage_AncientXml, // 以resources.xml(Ancient)格式切割图像
            CutImage_Plist, // 以plist(Free)格式切割图像
            CutImage_AtlasImageDat, // 以atlasimagemap.dat格式切割图像
            CutImage_TVAtlasXml, // 以xml(TV)格式切割图像
            CutImage_ResRTON, // 以RESOURCES.RTON(Rsb)格式切割图像
            SpliceImage_NewXml = 60, // 以RESOURCES.XML(Rsb)格式拼接图像
            SpliceImage_OldXml, // 以resources.xml(Old)格式拼接图像
            SpliceImage_AncientXml, // 以resources.xml(Ancient)格式拼接图像
            SpliceImage_Plist, // 以plist(Free)格式拼接图像
            SpliceImage_AtlasImageDat, // 以atlasimagemap.dat格式拼接图像
            SpliceImage_TVAtlasXml, // 以xml(TV)格式拼接图像
            SpliceImage_ResRTON, // 以RESOURCES.RTON(Rsb)格式拼接图像
            DecodeImage_PtxRsb = 80, // 解码PTX(rsb)图像
            DecodeImage_Cdat, // 解码cdat(Android,iOS)图像
            DecodeImage_TexiOS, // 解码tex(iOS)图像
            DecodeImage_Txz, // 解码txz(Android,iOS)图像
            DecodeImage_TexTV, // 解码tex(TV)图像
            DecodeImage_PtxXbox360, // 解码ptx(Xbox360)图像
            DecodeImage_PtxPS3, // 解码ptx(PS3)图像
            DecodeImage_PtxPSV, // 解码ptx(PSV)图像
            DecodeImage_Xnb, // 解码xnb(Windows Phone)图像
            EncodeImage_PtxRsb = 100, // 编码PTX(rsb)图像
            EncodeImage_Cdat, // 编码cdat(Android,iOS)图像
            EncodeImage_TexiOS, // 编码tex(iOS)图像
            EncodeImage_Txz, // 编码txz(Android,iOS)图像
            EncodeImage_TexTV, // 编码tex(TV)图像
            EncodeImage_PtxXbox360, // 编码ptx(Xbox360)图像
            EncodeImage_PtxPS3, // 编码ptx(PS3)图像
            EncodeImage_PtxPSV, // 编码ptx(PSV)图像
            EncodeImage_Xnb, // 编码xnb(Windows Phone)图像
            Reanim_ToPCCompiled = 120, // 将reanim动画转为PC_Compiled格式
            Reanim_ToPhone32Compiled, // 将reanim动画转为Phone32_Compiled格式
            Reanim_ToPhone64Compiled, // 将reanim动画转为Phone64_Compiled格式
            Reanim_ToWPXnb, // 将reanim动画转为WP_Xnb格式
            Reanim_ToGameConsoleCompiled, // 将reanim动画转为GameConsole_Compiled格式
            Reanim_ToTVCompiled, // 将reanim动画转为TV_Compiled格式
            Reanim_ToStudioJson, // 将reanim动画转为Studio_Json格式
            Reanim_ToRawXml, // 将reanim动画转为Raw_Xml格式
            Reanim_ToFlashXfl, // 将reanim动画转为Flash_Xfl_Folder格式
            Reanim_ToFlashFla, // 将reanim动画转为Flash_Fla格式
            Particles_ToPCCompiled = 140, // 将xml粒子特效转为PC_Compiled格式
            Particles_ToPhone32Compiled, // 将xml粒子特效转为Phone32_Compiled格式
            Particles_ToPhone64Compiled, // 将xml粒子特效转为Phone64_Compiled格式
            Particles_ToWPXnb, // 将xml粒子特效转为WP_Xnb格式
            Particles_ToGameConsoleCompiled, // 将xml粒子特效转为GameConsole_Compiled格式
            Particles_ToTVCompiled, // 将xml粒子特效转为TV_Compiled格式
            Particles_ToStudioJson, // 将xml粒子特效转为Studio_Json格式
            Particles_ToRawXml, // 将xml粒子特效转为Raw_Xml格式
            Trail_ToPCCompiled = 160, // 将trail拖尾特效转为PC_Compiled格式
            Trail_ToPhone32Compiled, // 将trail拖尾特效转为Phone32_Compiled格式
            Trail_ToPhone64Compiled, // 将trail拖尾特效转为Phone64_Compiled格式
            Trail_ToWPXnb, // 将trail拖尾特效转为WP_Xnb格式
            Trail_ToGameConsoleCompiled, // 将trail拖尾特效转为GameConsole_Compiled格式
            Trail_ToTVCompiled, // 将trail拖尾特效转为TV_Compiled格式
            Trail_ToStudioJson, // 将trail拖尾特效转为Studio_Json格式
            Trail_ToRawXml, // 将trail拖尾特效转为Raw_Xml格式
            DecodePam = 180, // 将pam动画转为表示pam动画的json格式
            EncodePam = 200, // 将表示pam动画的json格式转为pam格式
            DecodeRTON_Simple = 220, // 将Simple RTON格式的RTON解码为json格式
            DecodeRTON_Encrypted, // 将Encrypted RTON格式的RTON解码为json格式
            EncodeRTON_Simple = 240, // 将json编码为Simple RTON格式的RTON
            EncodeRTON_Encrypted, // 将json编码为Encrypted RTON格式的RTON
            Decompress_Zlib = 260, // 对文件进行Zlib解压
            Decompress_Gzip, // 对文件进行Gzip解压
            Decompress_Deflate, // 对文件进行Deflate解压
            Decompress_Brotli, // 对文件进行Brotli解压
            Decompress_Lzma, // 对文件进行Lzma解压
            Decompress_Lz4, // 对文件进行Lz4解压
            Decompress_Bzip2, // 对文件进行Bzip2解压
            Compress_Zlib = 280, // 对文件进行Zlib压缩
            Compress_Gzip, // 对文件进行Gzip压缩
            Compress_Deflate, // 对文件进行Deflate压缩
            Compress_Brotli, // 对文件进行Brotli压缩
            Compress_Lzma, // 对文件进行Lzma压缩
            Compress_Lz4, // 对文件进行Lz4压缩
            Compress_Bzip2, // 对文件进行Bzip2压缩
            LoadSetting = 300, // 加载设置
            LoadImageConvertXml,
            RunScript = 320 // 执行lua脚本
        }
    }
}
