--Check the rainy version
if (rainy.getversion() < 10) then error("The rainy version is too low! ") end
--Set language string
local loc = {
	[0] = { --ZHCN
		Name = "TextureBatchDecode.lua 作者：萌新迎风听雨",
		Description = "说明：一个将指定文件夹内的所有纹理图像转为png图像的批处理脚本",
		Error = "请输入文件夹位置",
		OutFormat = "请选择纹理图像格式",
        DeleteTitle = "删除文件",
        Delete = "是否删除被转换的文件",
		Finish = "执行完成！"
	},
	[1] = { --ENUS
		Name = "TextureBatchDecode.lua Author: YingFengTingYu",
		Description = "Description: A batch script that converts all texture images in the specified folder into PNG images",
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
if mode == nil then
    local m = rainy.sheet(loc.OutFormat,"PTX(rsb)","cdat(Android,iOS)","tex(iOS)","txz(Android,iOS)","tex(TV)","ptx(xbox360)","dds / ptx(ps3)","ptx(psv)","xnb(WindowsPhone)");
    local tab = {["PTX(rsb)"] = 0, ["cdat(Android,iOS)"] = 1,["tex(iOS)"] = 2,["txz(Android,iOS)"] = 3,["tex(TV)"] = 4,["ptx(xbox360)"] = 5,["dds / ptx(ps3)"] = 6,["ptx(psv)"] = 7,["xnb(WindowsPhone)"] = 8};
    mode = tab[m];
end
local extensions = {[0] = ".ptx",[1] = ".cdat",[2] = ".tex",[3] = ".txz",[4] = ".tex",[5] = ".ptx",[6] = ".ptx",[7] = ".ptx",[8] = ".xnb"};
ex = extensions[mode];
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
    if (a == ex or (mode == 6 and a == ".dds")) then
        rainy.decodeimage(files[i],rainy.formatpath(rainy.getfilepath(files[i]) .. "\\" .. b .. ".png"),mode);
        if delete then
            rainy.deletefile(files[i]);
        end
    end
end
--Finish
rainy.alert(loc.Finish);
