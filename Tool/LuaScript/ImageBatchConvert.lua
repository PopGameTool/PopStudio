--Check the rainy version
if (rainy.getversion() < 10) then error("The rainy version is too low! ") end
--Set language string
local loc = {
	[0] = { --ZHCN
		Name = "ImageBatchConvert.lua 作者：萌新迎风听雨",
		Description = "说明：批量图片格式转换。纹理与位图互转、位图互转，支持等比缩放、递归子目录、覆盖开关与失败清单",
		Error = "请输入文件夹位置",
		Mode = "请选择转换方向",
		Mode0 = "纹理转位图",
		Mode1 = "位图转纹理",
		Mode2 = "位图转位图",
		Container = "请选择纹理容器",
		Finish = "执行完成！",
		Fail = "转换失败，请看上面的错误信息"
	},
	[1] = { --ENUS
		Name = "ImageBatchConvert.lua Author: YingFengTingYu",
		Description = "Description: Batch image format conversion. Texture <-> bitmap and bitmap <-> bitmap, with proportional resize, recursion, an overwrite switch and a failure list",
		Error = "Please enter the file path",
		Mode = "Choose the converting direction",
		Mode0 = "Texture to bitmap",
		Mode1 = "Bitmap to texture",
		Mode2 = "Bitmap to bitmap",
		Container = "Choose texture container",
		Finish = "Execution complete! ",
		Fail = "The conversion failed, see the error above"
	}
};
loc = loc[rainy.getlanguage()];
--Show info
print(loc.Name);
print(loc.Description);
--Arguments: input, outdir, mode(0/1/2), pixelFormat, maxEdge, containerOrBitmapFormat, jpegQuality
local argv = args or {};
--Ask the input and output folder. The output folder defaults to "<input>_converted" so that
--the result never lands inside the scanned folder.
local filepath = argv[1] or rainy.choosefolder() or error(loc.Error);
local outdir = argv[2] or rainy.formatpath(filepath .. "_converted");
--Ask the converting direction
local mode = argv[3];
if mode == nil then
	local m = rainy.sheet(loc.Mode, loc.Mode0, loc.Mode1, loc.Mode2);
	local tab = {[loc.Mode0] = 0, [loc.Mode1] = 1, [loc.Mode2] = 2};
	mode = tab[m];
end
--Ask the texture container, only used by mode 0 and mode 1
local container = argv[6];
if (mode == 0 or mode == 1) and container == nil then
	local m = rainy.sheet(loc.Container, "PTX(rsb)", "cdat(Android,iOS)", "tex(iOS)", "txz(Android,iOS)", "tex(TV)", "ptx(Xbox360)", "dds / ptx(PS3)", "ptx(PSV)", "xnb(Windows Phone)");
	local tab = {["PTX(rsb)"] = 0, ["cdat(Android,iOS)"] = 1, ["tex(iOS)"] = 2, ["txz(Android,iOS)"] = 3, ["tex(TV)"] = 4, ["ptx(Xbox360)"] = 5, ["dds / ptx(PS3)"] = 6, ["ptx(PSV)"] = 7, ["xnb(Windows Phone)"] = 8};
	container = tab[m];
end
--The pixel format is the index of the format list, NOT the number in the brackets of the label
--shown in the app. For example "ABGR8888(0)" is index 1.
local pixelformat = argv[4] or 0;
--Proportional resize. 0 means "do not resize", it only shrinks, never enlarges.
local maxedge = argv[5] or 0;
--Run. The last two arguments are "recursive" and "overwrite".
local r;
if mode == 0 then
	r = rainy.decodeimages(filepath, outdir, container, true, true);
elseif mode == 1 then
	r = rainy.encodeimages(filepath, outdir, container, pixelformat, true, true);
else
	r = rainy.convertbitmaps(filepath, outdir, argv[6] or "png", argv[7] or 90, maxedge, true, true);
end
--Report
if r == nil then
	print(loc.Fail);
	return;
end
print(string.format("total=%d success=%d skipped=%d failed=%d time=%.3fs", r.total, r.success, r.skipped, r.failed, r.time));
for i = 1, #r.failures do
	print(string.format("[%d] %s : %s", i, r.failures[i].input, r.failures[i].message));
end
rainy.alert(loc.Finish);
