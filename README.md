# PopStudio
In English:  
  
A project to convert many kinds of files used in PopCap Games.  
By using C# and .Net 6/.Net 7, this project can easily be used in Linux, MacOS, Windows and Android.  
Both English and Chinese are supported.  
Compile PopStudio.ConsoleProject to use PopStudio in Windows, Linux and MacOS with console.  
Compile PopStudio.MAUI to use PopStudio in Android with GUI.  
Compile PopStudio.WPF to use PopStudio in Windows with GUI.  
Compile PopStudio.Avalonia to use PopStudio in Linux and MacOS with GUI.  
  
Now this project supports these function:  
unpack and pack dz(including Android and BlackBerry)  
unpack and pack rsb(including Android, iOS, PS3, PS4 and Xbox360)  
unpack and pack pak(including Windows, MacOS, PS3, PSV and Xbox360)  
unpack and pack arcv(including Nds)  
cut and create atlas  
decode and encode ptx(including Android, iOS, PS3, PS4, PSV and Xbox360)  
decode and encode cdat(including Android and iOS)  
decode and encode tex(including Android and iOS)  
decode and encode txz(including Android and iOS)  
decode and encode xnb(including WindowsPhone)  
decode and encode reanim.compiled(including Windows, MacOS, Android, iOS, WindowsPhone, PS3, PSV and Xbox360)  
convert reanim formats to and from unpacked XFL projects and modern ZIP-based FLA files
decode and encode xml.compiled(including Windows, MacOS, Android, iOS, WindowsPhone, PS3, PSV and Xbox360)  
decode and encode trail.compiled(including Windows, MacOS, Android, iOS, WindowsPhone, PS3, PSV and Xbox360)  
decode and encode pam(including version 1-6)  
decode and encode RTON(including simple RTON and encrypted RTON but need your key)  
decompress and compress files  
use lua to run program  

___
If you know other file sturctures such as luc and pax, you can communicate with the author.  
___
If you want to communicate with the author, you can download QQ(a chatting software) in Google Play, App Store or Microsoft Store, and then register a QQ account number and enter our QQ group numbered 1017246977(The answer is "Github").
___
Contributors:

- 惋兮

___
This project has used:  
[DotNetZip](https://github.com/eropple/dotnetzip) to decompress and compress BZip2 files.  
[MaxRectsBinPack](http://wiki.unity3d.com/index.php/MaxRectsBinPack) to create atlas.  
  
reference:
[Real-Time DXT Compression](https://www.researchgate.net/publication/259000525_Real-Time_DXT_Compression) to encode DXT texture.  
[EveryFileExplorer](https://github.com/Gericom/EveryFileExplorer) to encode ETC1 texture.  
[pvrtccompressor](https://bitbucket.org/jthlim/pvrtccompressor) to encode PVRTCI texture.  
[PVR Native SDK](https://github.com/powervr-graphics/Native_SDK) to decode PVRTCI texture.  
___
In Chinese:  
  
一个用于转换很多宝开游戏使用的文件的项目。  
通过使用C#和.Net 6/.Net 7，这个项目可以很轻松地在Linux，MacOS，Windows和Android系统上使用。  
英文和中文都支持。  
编译PopStudio.ConsoleProject以在Windows，Linux和MacOS使用控制台版本。  
编译PopStudio.MAUI以在Android使用GUI版本。  
编译PopStudio.WPF以在Windows使用GUI版本。  
编译PopStudio.Avalonia以在Linux和MacOS使用GUI版本。  
  
现在这个项目支持如下功能：
解包打包dz（包括Android和BlackBerry）  
解包打包rsb（包括Android，iOS，PS3，PS4和Xbox360）  
解包打包pak（包括Windows，MacOS，PS3，PSV和Xbox360）  
解包打包arcv（包括Nds）  
图集裁剪与拼接  
解码编码ptx（包括Android，iOS，PS3，PS4，PSV和Xbox360）  
解码编码cdat（包括Android，iOS）  
解码编码tex（包括Android，iOS）  
解码编码txz（包括Android，iOS）  
解码编码xnb（包括WindowsPhone）  
解码编码reanim.compiled(包括Windows，MacOS，Android，iOS，WindowsPhone，PS3，PSV和Xbox360)  
在未打包的XFL工程、现代ZIP型FLA文件与任意已支持的reanim格式之间转换
解码编码xml.compiled(包括Windows，MacOS，Android，iOS，WindowsPhone，PS3，PSV和Xbox360)  
解码编码trail.compiled(包括Windows，MacOS，Android，iOS，WindowsPhone，PS3，PSV和Xbox360)  
解码编码pam（包括版本号1-6的）  
解码编码RTON（包括普通RTON和加密RTON，需自行提供密钥）  
压缩解压文件  
使用lua脚本调用程序  

___
如果你知道其他文件结构，例如luc和pax，你可以和作者交流。  
___
如果你想和作者交流，你可以使用QQ，加入群聊1017246977（备注“GitHub”）。  
___
贡献者：

- 惋兮

___
这个项目使用了：  
[DotNetZip](https://github.com/eropple/dotnetzip)用于解压和压缩BZip2文件。  
[MaxRectsBinPack](http://wiki.unity3d.com/index.php/MaxRectsBinPack)用于构建图集。  
  
参考：
[Real-Time DXT Compression](https://www.researchgate.net/publication/259000525_Real-Time_DXT_Compression)用于编码DXT纹理。  
[EveryFileExplorer](https://github.com/Gericom/EveryFileExplorer)用于编码ETC1纹理。  
[pvrtccompressor](https://bitbucket.org/jthlim/pvrtccompressor)用于编码PVRTCI纹理。  
[PVR Native SDK](https://github.com/powervr-graphics/Native_SDK)用于解码PVRTCI纹理。
