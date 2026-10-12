--Check the rainy version
if (rainy.getversion() < 10) then error("The rainy version is too low! ") end
--Set language string
local loc = {
	[0] = { --ZHCN
		Name = "TextureBatchEncode.lua 作者：萌新迎风听雨",
		Description = "说明：一个将指定文件夹内的所有png图像转为纹理图像的批处理脚本",
		Error = "请输入文件夹位置",
		OutFormat = "请选择纹理图像格式",
        DeleteTitle = "删除文件",
        Delete = "是否删除被转换的文件",
		Finish = "执行完成！"
	},
	[1] = { --ENUS
		Name = "TextureBatchEncode.lua Author: YingFengTingYu",
		Description = "Description: A batch script that converts all PNG images in the specified folder into texture images",
		Error = "Please enter the file path",
		OutFormat = "Choose texture format",
        DeleteTitle = "Delete Files",
        Delete = "Do you want to delete the converted file",
		Finish = "Execution complete! "
	}
};
loc = loc[rainy.getlanguage()];
--Show info
print(loc.Name);
print(loc.Description);
--Ask the file path and the out format
local filepath = args[1] or rainy.choosefolder() or error(loc.Error);
local mode = args[2];
local ex;
local format = args[4] or 0;
if mode == nil then
    local m = rainy.sheet(loc.OutFormat,"PTX(rsb)","cdat(Android,iOS)","tex(iOS)","txz(Android,iOS)","tex(TV)","ptx(xbox360)","dds / ptx(ps3)","ptx(psv)","xnb(WindowsPhone)");
    local tab = {["PTX(rsb)"] = 0, ["cdat(Android,iOS)"] = 1,["tex(iOS)"] = 2,["txz(Android,iOS)"] = 3,["tex(TV)"] = 4,["ptx(xbox360)"] = 5,["dds / ptx(ps3)"] = 6,["ptx(psv)"] = 7,["xnb(WindowsPhone)"] = 8};
    mode = tab[m];
    if (mode == 0) then
        local cho = rainy.sheet(loc.OutFormat,"ARGB8888(0)", "ABGR8888(0)", "RGBA4444(1)", "RGB565(2)", "RGBA5551(3)", "RGBA4444_Block(21)", "RGB565_Block(22)", "RGBA5551_Block(23)", "XRGB8888_A8(149)", "ARGB8888(BE)(0)", "ARGB8888_Padding(BE)(0)", "DXT1_RGB(35)", "DXT3_RGBA(36)", "DXT5_RGBA(37)", "DXT5(5)", "DXT5(BE)(5)", "ETC1_RGB(32)", "ETC1_RGB_A8(147)", "ETC1_RGB_A_Palette(147)", "ETC1_RGB_A_Palette(150)", "PVRTC_4BPP_RGBA(30)", "PVRTC_4BPP_RGB_A8(148)");
        local tab3 = {["ARGB8888(0)"] = 0,["ABGR8888(0)"] = 1,["RGBA4444(1)"] = 2,["RGB565(2)"] = 3,["RGBA5551(3)"] = 4,["RGBA4444_Block(21)"] = 5,["RGB565_Block(22)"] = 6,["RGBA5551_Block(23)"] = 7,["XRGB8888_A8(149)"] = 8,["ARGB8888(BE)(0)"] = 9,["ARGB8888_Padding(BE)(0)"] = 10,["DXT1_RGB(35)"] = 11,["DXT3_RGBA(36)"] = 12,["DXT5_RGBA(37)"] = 13,["DXT5(5)"] = 14,["DXT5(BE)(5)"] = 15,["ETC1_RGB(32)"] = 16,["ETC1_RGB_A8(147)"] = 17,["ETC1_RGB_A_Palette(147)"] = 18,["ETC1_RGB_A_Palette(150)"] = 19,["PVRTC_4BPP_RGBA(30)"] = 20,["PVRTC_4BPP_RGB_A8(148)"] = 21};
        format = tab3[cho];
    end
    if (mode == 2 or mode == 3) then
        local cho = rainy.sheet(loc.OutFormat,"ABGR8888(1)", "RGBA4444(2)", "RGBA5551(3)", "RGB565(4)");
        local tab3 = {["ABGR8888(1)"] = 0,["RGBA4444(2)"] = 1,["RGBA5551(3)"] = 2,["RGB565(4)"] = 3};
        format = tab3[cho];
    end
    if (mode == 4) then
        local cho = rainy.sheet(loc.OutFormat,"LUT8(1)(Invalid)", "ARGB8888(2)", "ARGB4444(3)", "ARGB1555(4)", "RGB565(5)", "ABGR8888(6)", "RGBA4444(7)", "RGBA5551(8)", "XRGB8888(9)", "LA88(10)");
        local tab3 = {["LUT8(1)(Invalid)"] = 0,["ARGB8888(2)"] = 1,["ARGB4444(3)"] = 2,["ARGB1555(4)"] = 3,["RGB565(5)"] = 4,["ABGR8888(6)"] = 5,["RGBA4444(7)"] = 6,["RGBA5551(8)"] = 7,["XRGB8888(9)"] = 8,["LA88(10)"] = 9};
        format = tab3[cho];
    end
    if (mode == 6) then
        local cho = rainy.sheet(loc.OutFormat,"DXT5_RGBA", "ATC_RGB (ATC)", "ATC_RGBA4 (ATCA)", "ATC_RGBA_Interpolated (ATCI)");
        local formats = {["DXT5_RGBA"] = 0,["ATC_RGB (ATC)"] = 1,["ATC_RGBA4 (ATCA)"] = 2,["ATC_RGBA_Interpolated (ATCI)"] = 3};
        format = formats[cho];
    end
end
local extensions = {[0] = ".PTX",[1] = ".cdat",[2] = ".tex",[3] = ".txz",[4] = ".tex",[5] = ".ptx",[6] = ".ptx",[7] = ".ptx",[8] = ".xnb"};
ex = (mode == 6 and format ~= 0) and ".dds" or extensions[mode];
--Ask if you want to delete the old file
local delete = args[3];
if delete == nil then
    delete = rainy.alert(loc.Delete, loc.DeleteTitle, true);
end
--Compute and create the generated folder
local files = rainy.getfiles(filepath);
for i=1,#files do
    local a = string.lower(rainy.getfileextension(files[i]));
    local b = rainy.getfilenamewithoutextension(files[i]);
    if (a == ".png") then
        rainy.encodeimage(files[i],rainy.formatpath(rainy.getfilepath(files[i]) .. "\\" .. b .. ex),mode,format);
        if delete then
            rainy.deletefile(files[i]);
        end
    end
end
--Finish
rainy.alert(loc.Finish);
