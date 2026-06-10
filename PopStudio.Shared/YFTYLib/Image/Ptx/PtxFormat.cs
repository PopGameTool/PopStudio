namespace PopStudio.Image.Ptx
{
    internal enum PtxFormat
    {
        ARGB8888, //ABRG8888
        RGBA4444,
        RGB565,
        RGBA5551,
        DXT5_RGBA_MortonBlock = 5,
        RGBA4444_Block = 21,
        RGB565_Block,
        RGBA5551_Block,
        PVRTC_4BPP_RGBA = 30,
        PVRTC_2BPP_RGBA,
        ETC1_RGB,
        DXT1_RGB = 35,
        DXT3_RGBA,
        DXT5_RGBA,
        ATC_RGB,
        ATC_RGBA4,
        ETC1_RGB_A8 = 147, //ETC1_RGB_A_Palette
        PVRTC_4BPP_RGBA_A8,
        ARGB8888_A8, //ABGR8888_A8
        ETC1_RGB_A_Palette,
        ASTC_44 = 160,
        ASTC_55 = 161,
        ASTC_66 = 162,
        ASTC_88 = 163,
    }
}
