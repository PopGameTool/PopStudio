namespace PopStudio.Language.Languages
{
    internal interface ILocalization
    {
        //Console
        public string Console_API_Alert_Ask { get; }
        public string Console_API_Alert_Continue { get; }
        public string Console_API_ActionSheet_Item { get; }
        public string Console_API_ChooseFolder_Ask { get; }
        public string Console_API_ChooseOpenFile_Ask { get; }
        public string Console_API_ChooseSaveFile_Ask { get; }
        public string Console_Reader_ReadInt16 { get; }
        public string Console_Reader_ReadInt32 { get; }
        public string Console_Reader_ReadInt64 { get; }
        public string Console_Reader_ReadUInt16 { get; }
        public string Console_Reader_ReadUInt32 { get; }
        public string Console_Reader_ReadUInt64 { get; }
        public string Console_Reader_ReadString { get; }
        public string Console_Reader_ReadBoolean { get; }
        public string Console_Reader_WrongInt16 { get; }
        public string Console_Reader_WrongInt32 { get; }
        public string Console_Reader_WrongInt64 { get; }
        public string Console_Reader_WrongUInt16 { get; }
        public string Console_Reader_WrongUInt32 { get; }
        public string Console_Reader_WrongUInt64 { get; }
        public string Console_App_FilePath { get; }
        public string Console_App_CommandIndex { get; }
        public string Console_App_CommandIndex_Ask_Count { get; }
        public string Console_App_CommandIndex_Ask_Formater { get; }
        public string Console_App_CommandIndex_Unpack_Dz { get; }
        public string Console_App_CommandIndex_Unpack_Rsb { get; }
        public string Console_App_CommandIndex_Unpack_Pak { get; }
        public string Console_App_CommandIndex_Unpack_Arcv { get; }
        public string Console_App_CommandIndex_Pack_Dz { get; }
        public string Console_App_CommandIndex_Pack_Rsb { get; }
        public string Console_App_CommandIndex_Pack_Pak { get; }
        public string Console_App_CommandIndex_Pack_Arcv { get; }
        public string Console_App_CommandIndex_CutImage_NewXml { get; }
        public string Console_App_CommandIndex_CutImage_OldXml { get; }
        public string Console_App_CommandIndex_CutImage_AncientXml { get; }
        public string Console_App_CommandIndex_CutImage_Plist { get; }
        public string Console_App_CommandIndex_CutImage_AtlasImageDat { get; }
        public string Console_App_CommandIndex_CutImage_TVAtlasXml { get; }
        public string Console_App_CommandIndex_CutImage_ResRTON { get; }
        public string Console_App_CommandIndex_SpliceImage_NewXml { get; }
        public string Console_App_CommandIndex_SpliceImage_OldXml { get; }
        public string Console_App_CommandIndex_SpliceImage_AncientXml { get; }
        public string Console_App_CommandIndex_SpliceImage_Plist { get; }
        public string Console_App_CommandIndex_SpliceImage_AtlasImageDat { get; }
        public string Console_App_CommandIndex_SpliceImage_TVAtlasXml { get; }
        public string Console_App_CommandIndex_SpliceImage_ResRTON { get; }
        public string Console_App_CommandIndex_DecodeImage_PtxRsb { get; }
        public string Console_App_CommandIndex_DecodeImage_Cdat { get; }
        public string Console_App_CommandIndex_DecodeImage_TexiOS { get; }
        public string Console_App_CommandIndex_DecodeImage_Txz { get; }
        public string Console_App_CommandIndex_DecodeImage_TexTV { get; }
        public string Console_App_CommandIndex_DecodeImage_PtxXbox360 { get; }
        public string Console_App_CommandIndex_DecodeImage_PtxPS3 { get; }
        public string Console_App_CommandIndex_DecodeImage_PtxPSV { get; }
        public string Console_App_CommandIndex_DecodeImage_Xnb { get; }
        public string Console_App_CommandIndex_EncodeImage_PtxRsb { get; }
        public string Console_App_CommandIndex_EncodeImage_Cdat { get; }
        public string Console_App_CommandIndex_EncodeImage_TexiOS { get; }
        public string Console_App_CommandIndex_EncodeImage_Txz { get; }
        public string Console_App_CommandIndex_EncodeImage_TexTV { get; }
        public string Console_App_CommandIndex_EncodeImage_PtxXbox360 { get; }
        public string Console_App_CommandIndex_EncodeImage_PtxPS3 { get; }
        public string Console_App_CommandIndex_EncodeImage_PtxPSV { get; }
        public string Console_App_CommandIndex_EncodeImage_Xnb { get; }
        public string Console_App_CommandIndex_Reanim_ToPCCompiled { get; }
        public string Console_App_CommandIndex_Reanim_ToPhone32Compiled { get; }
        public string Console_App_CommandIndex_Reanim_ToPhone64Compiled { get; }
        public string Console_App_CommandIndex_Reanim_ToWPXnb { get; }
        public string Console_App_CommandIndex_Reanim_ToGameConsoleCompiled { get; }
        public string Console_App_CommandIndex_Reanim_ToTVCompiled { get; }
        public string Console_App_CommandIndex_Reanim_ToStudioJson { get; }
        public string Console_App_CommandIndex_Reanim_ToRawXml { get; }
        public string Console_App_CommandIndex_Reanim_ToFlashXfl { get; }
        public string Console_App_CommandIndex_Reanim_ToFlashFla { get; }
        public string Console_App_CommandIndex_Particles_ToPCCompiled { get; }
        public string Console_App_CommandIndex_Particles_ToPhone32Compiled { get; }
        public string Console_App_CommandIndex_Particles_ToPhone64Compiled { get; }
        public string Console_App_CommandIndex_Particles_ToWPXnb { get; }
        public string Console_App_CommandIndex_Particles_ToGameConsoleCompiled { get; }
        public string Console_App_CommandIndex_Particles_ToTVCompiled { get; }
        public string Console_App_CommandIndex_Particles_ToStudioJson { get; }
        public string Console_App_CommandIndex_Particles_ToRawXml { get; }
        public string Console_App_CommandIndex_Trail_ToPCCompiled { get; }
        public string Console_App_CommandIndex_Trail_ToPhone32Compiled { get; }
        public string Console_App_CommandIndex_Trail_ToPhone64Compiled { get; }
        public string Console_App_CommandIndex_Trail_ToWPXnb { get; }
        public string Console_App_CommandIndex_Trail_ToGameConsoleCompiled { get; }
        public string Console_App_CommandIndex_Trail_ToTVCompiled { get; }
        public string Console_App_CommandIndex_Trail_ToStudioJson { get; }
        public string Console_App_CommandIndex_Trail_ToRawXml { get; }
        public string Console_App_CommandIndex_DecodePam { get; }
        public string Console_App_CommandIndex_EncodePam { get; }
        public string Console_App_CommandIndex_DecodeRTON_Simple { get; }
        public string Console_App_CommandIndex_DecodeRTON_Encrypted { get; }
        public string Console_App_CommandIndex_EncodeRTON_Simple { get; }
        public string Console_App_CommandIndex_EncodeRTON_Encrypted { get; }
        public string Console_App_CommandIndex_Decompress_Zlib { get; }
        public string Console_App_CommandIndex_Decompress_Gzip { get; }
        public string Console_App_CommandIndex_Decompress_Deflate { get; }
        public string Console_App_CommandIndex_Decompress_Brotli { get; }
        public string Console_App_CommandIndex_Decompress_Lzma { get; }
        public string Console_App_CommandIndex_Decompress_Lz4 { get; }
        public string Console_App_CommandIndex_Decompress_Bzip2 { get; }
        public string Console_App_CommandIndex_Compress_Zlib { get; }
        public string Console_App_CommandIndex_Compress_Gzip { get; }
        public string Console_App_CommandIndex_Compress_Deflate { get; }
        public string Console_App_CommandIndex_Compress_Brotli { get; }
        public string Console_App_CommandIndex_Compress_Lzma { get; }
        public string Console_App_CommandIndex_Compress_Lz4 { get; }
        public string Console_App_CommandIndex_Compress_Bzip2 { get; }
        public string Console_App_CommandIndex_LoadSetting { get; }
        public string Console_App_CommandIndex_LoadImageConvertXml { get; }
        public string Console_App_CommandIndex_RunScript { get; }
        public string Console_App_Unpack_DecodeImage_Ask { get; }
        public string Console_App_Unpack_DeleteImage_Ask { get; }
        public string Console_App_Atlas_InfoFilePath_Ask { get; }
        public string Console_App_Atlas_InfoID_Ask { get; }
        public string Console_App_Atlas_Width_Ask { get; }
        public string Console_App_Atlas_Height_Ask { get; }
        public string Console_App_Texture_Format_Ask { get; }
        public string Console_App_Texture_Format_Ask_Formater { get; }
        
        public string Console_NoCommand { get; }

        //Article
        public string Article_Title { get; }

        //Command
        public string Command_EnterInteger { get; }
        public string Command_EnterString { get; }
        public string Command_EnterFormat { get; }

        //Atlas
        public string Atlas_Title { get; }
        public string Atlas_Introduction { get; }
        public string Atlas_Mode1 { get; }
        public string Atlas_Mode2 { get; }
        public string Atlas_Choose1 { get; }
        public string Atlas_Choose2 { get; }
        public string Atlas_Choose3 { get; }
        public string Atlas_Choose4 { get; }
        public string Atlas_Choose5 { get; }
        public string Atlas_Choose6 { get; }
        public string Atlas_Choose7 { get; }
        public string Atlas_Format { get; }
        public string Atlas_MaxWidth { get; }
        public string Atlas_MaxHeight { get; }
        public string Atlas_NotFound1 { get; }
        public string Atlas_NotFound2 { get; }

        //LuaScript

        public string LuaScript_Title { get; }
        public string LuaScript_Introduction { get; }
        public string LuaScript_TracePrint { get; }


        //Shell
        public string Shell_OK { get; }
        public string Shell_Cancel { get; }
        public string Shell_ExitTitle { get; }
        public string Shell_ExitText { get; }
        public string Shell_ChoosePageTitle { get; }

        //Setting
        public string Setting_Title { get; }
        public string Setting_Introduction { get; }
        public string Setting_ItemLanguage { get; }
        public string Setting_SetLanguage { get; }
        public string Setting_Extension { get; }
        public string Setting_CompressionMethod { get; }
        public string Setting_ItemDz { get; }
        public string Setting_IntroDz { get; }
        public string Setting_Add { get; }
        public string Setting_Clear { get; }
        public string Setting_ItemPak { get; }
        public string Setting_IntroPak { get; }
        public string Setting_ItemRsb { get; }
        public string Setting_IntroRsb { get; }
        public string Setting_ItemPtx { get; }
        public string Setting_IntroPtx { get; }
        public string Setting_ItemCdat { get; }
        public string Setting_IntroCdat { get; }
        public string Setting_ItemRTON { get; }
        public string Setting_IntroRTON { get; }
        public string Setting_ItemCompiled { get; }
        public string Setting_IntroCompiled { get; }
        public string Setting_CompiledLoadMethod { get; }
        public string Setting_CompiledLoadFileName { get; }
        public string Setting_CompiledLoadFromProgram { get; }
        public string Setting_CompiledLoadFromFile { get; }
        public string Setting_CompiledLoadError_Title { get; }
        public string Setting_CompiledLoadError_Text_NoFile { get; }
        public string Setting_CompiledLoadError_Text { get; }
        public string Setting_ItemXfl { get; }
        public string Setting_IntroXfl { get; }
        public string Setting_XflWidth { get; }
        public string Setting_XflHeight { get; }
        public string Setting_XflLabelName { get; }
        public string Setting_XflScaleX { get; }
        public string Setting_XflScaleY { get; }
        public string Setting_ItemPamXfl { get; }
        public string Setting_IntroPamXfl { get; }
        public string Setting_PamXflResolution { get; }
        public string Setting_AD { get; }
        public string Setting_Load { get; }
        public string Setting_Unload { get; }
        public string Setting_Recover { get; }
        public string Setting_OK { get; }
        public string Setting_Cancel { get; }
        public string Setting_SureRecover { get; }
        public string Setting_SureRecoverText { get; }
        public string Setting_FinishRecover { get; }
        public string Setting_FinishRecoverText { get; }
        public string Setting_ChooseItem { get; }
        public string Setting_ChooseCompressMode { get; }
        public string Setting_EnterExtension { get; }
        public string Setting_EnterExtensionText { get; }
        public string Setting_CompressItem1 { get; }
        public string Setting_CompressItem2 { get; }
        public string Setting_ChooseLanguage { get; }
        public string Setting_FinishChooseLanguage { get; }
        public string Setting_FinishChooseLanguageText { get; }
        public string Setting_Submit { get; }
        public string Setting_InvalidData_Title { get; }
        public string Setting_InvalidData_Text { get; }

        //Texture
        public string Texture_Title { get; }
        public string Texture_Introduction { get; }
        public string Texture_Mode1 { get; }
        public string Texture_Mode2 { get; }
        public string Texture_Choose1 { get; }
        public string Texture_Choose2 { get; }
        public string Texture_Choose3 { get; }
        public string Texture_Choose4 { get; }
        public string Texture_Choose5 { get; }
        public string Texture_Choose6 { get; }
        public string Texture_Choose7 { get; }
        public string Texture_Mode1_Batch { get; }
        public string Texture_Mode2_Batch { get; }
        public string Texture_Choose1_Batch { get; }
        public string Texture_Choose2_Batch { get; }
        public string Texture_Choose3_Batch { get; }
        public string Texture_Choose4_Batch { get; }
        public string Texture_Choose5_Batch { get; }
        public string Texture_Choose6_Batch { get; }
        public string Texture_Choose7_Batch { get; }

        //Pam
        public string Pam_Title { get; }
        public string Pam_Introduction { get; }
        public string Pam_Mode1 { get; }
        public string Pam_Mode2 { get; }
        public string Pam_Choose1 { get; }
        public string Pam_Choose2 { get; }
        public string Pam_Choose3 { get; }
        public string Pam_Choose4 { get; }
        public string Pam_Choose5 { get; }
        public string Pam_Choose6 { get; }
        public string Pam_Mode1_Batch { get; }
        public string Pam_Mode2_Batch { get; }
        public string Pam_Choose1_Batch { get; }
        public string Pam_Choose2_Batch { get; }
        public string Pam_Choose3_Batch { get; }
        public string Pam_Choose4_Batch { get; }
        public string Pam_Choose5_Batch { get; }
        public string Pam_Choose6_Batch { get; }
        public string Pam_InFormat { get; }
        public string Pam_OutFormat { get; }

        //RTON
        public string RTON_Title { get; }
        public string RTON_Introduction { get; }
        public string RTON_Mode1 { get; }
        public string RTON_Mode2 { get; }
        public string RTON_Choose1 { get; }
        public string RTON_Choose2 { get; }
        public string RTON_Choose3 { get; }
        public string RTON_Choose4 { get; }
        public string RTON_Choose5 { get; }
        public string RTON_Choose6 { get; }
        public string RTON_Mode1_Batch { get; }
        public string RTON_Mode2_Batch { get; }
        public string RTON_Choose1_Batch { get; }
        public string RTON_Choose2_Batch { get; }
        public string RTON_Choose3_Batch { get; }
        public string RTON_Choose4_Batch { get; }
        public string RTON_Choose5_Batch { get; }
        public string RTON_Choose6_Batch { get; }

        //Trail
        public string Trail_Title { get; }
        public string Trail_Introduction { get; }
        public string Trail_Choose1 { get; }
        public string Trail_Choose2 { get; }
        public string Trail_Choose1_Batch { get; }
        public string Trail_Choose2_Batch { get; }
        public string Trail_InFormat { get; }
        public string Trail_OutFormat { get; }

        //Reanim
        public string Reanim_Title { get; }
        public string Reanim_Introduction { get; }
        public string Reanim_Choose1 { get; }
        public string Reanim_Choose2 { get; }
        public string Reanim_Choose1_Batch { get; }
        public string Reanim_Choose2_Batch { get; }
        public string Reanim_InFormat { get; }
        public string Reanim_OutFormat { get; }

        //Particles
        public string Particles_Title { get; }
        public string Particles_Introduction { get; }
        public string Particles_Choose1 { get; }
        public string Particles_Choose2 { get; }
        public string Particles_Choose1_Batch { get; }
        public string Particles_Choose2_Batch { get; }
        public string Particles_InFormat { get; }
        public string Particles_OutFormat { get; }

        //Compress
        public string Compress_Title { get; }
        public string Compress_Introduction { get; }
        public string Compress_Mode1 { get; }
        public string Compress_Mode2 { get; }
        public string Compress_Choose1 { get; }
        public string Compress_Choose2 { get; }
        public string Compress_Choose3 { get; }
        public string Compress_Choose4 { get; }
        public string Compress_Choose5 { get; }
        public string Compress_Choose6 { get; }
        public string Compress_Mode1_Batch { get; }
        public string Compress_Mode2_Batch { get; }
        public string Compress_Choose1_Batch { get; }
        public string Compress_Choose2_Batch { get; }
        public string Compress_Choose3_Batch { get; }
        public string Compress_Choose4_Batch { get; }
        public string Compress_Choose5_Batch { get; }
        public string Compress_Choose6_Batch { get; }

        //Package
        public string Package_Title { get; }
        public string Package_Introduction { get; }
        public string Package_Mode1 { get; }
        public string Package_Mode2 { get; }
        public string Package_Choose1 { get; }
        public string Package_Choose2 { get; }
        public string Package_Choose3 { get; }
        public string Package_Choose4 { get; }
        public string Package_Choose5 { get; }
        public string Package_Choose6 { get; }
        public string Package_ChangeImage { get; }
        public string Package_DeleteImage { get; }
        //HomePage
        public string HomePage_Title { get; }
        public string HomePage_Begin { get; }
        public string HomePage_Function { get; }
        public string HomePage_Permission { get; }
        public string HomePage_PermissionAsk { get; }
        public string HomePage_Agreement { get; }
        public string HomePage_Version { get; }
        public string HomePage_Author_String { get; }
        public string HomePage_Author { get; }
        public string HomePage_Thanks_String { get; }
        public string HomePage_Thanks { get; }
        public string HomePage_QQGroup_String { get; }
        public string HomePage_QQGroup { get; }
        public string HomePage_Course_String { get; }
        public string HomePage_Course { get; }
        public string HomePage_AppNewNotice_String { get; }
        public string HomePage_AppNewNotice { get; }
        //Share
        public string Share_FileNotFound { get; }
        public string Share_FolderNotFound { get; }
        public string Share_Finish { get; }
        public string Share_Finish_NoTime { get; }
        public string Share_Wrong { get; }
        public string Share_ChooseMode { get; }
        public string Share_Choose { get; }
        public string Share_Run { get; }
        public string Share_RunStatue { get; }
        public string Share_Waiting { get; }
        public string Share_Running { get; }
        public string Share_SingleMode { get; }
        public string Share_BatchMode { get; }

        //Permission
        public string Permission_Title { get; }
        public string Permission_Request1 { get; }
        public string Permission_Request2 { get; }
        public string Permission_RequestFinish { get; }
        public string Permission_GoTo { get; }
        public string Permission_OK { get; }
        public string Permission_Cancel { get; }

        //PickFile
        public string PickFile_AllFiles { get; }
        public string PickFile_NewFolder { get; }
        public string PickFile_Back { get; }
        public string PickFile_OK { get; }
        public string PickFile_Cancel { get; }
        public string PickFile_EnterFolderName { get; }
        public string PickFile_CreateWrong { get; }
        public string PickFile_NoPermission { get; }
        public string PickFile_NoPermissionToEnter { get; }
        public string PickFlie_SaveThere { get; }
        public string PickFlie_SaveFile { get; }
        public string PickFile_EnterFileName { get; }
        public string PickFile_ChooseThisFolder { get; }

        //Ad
        public string AD_Cancel { get; }
        public string AD_Title { get; }
    }
}
