using PopStudio.Language.Languages;
using System.Text;
using System.Diagnostics;

namespace PopStudio.Platform
{
    internal class ConsoleAPI : YFAPI
    {
        public override void InternalUnpack(string inFile, string outFile, int format, bool changeimage, bool delete)
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            base.InternalUnpack(inFile, outFile, format, changeimage, delete);
            s.Stop();
            ConsoleWriter.WriteSuccessLine(MAUIStr.Obj.Share_Finish, s.ElapsedMilliseconds / 1000m);
        }

        public override void InternalPack(string inFile, string outFile, int format)
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            base.InternalPack(inFile, outFile, format);
            s.Stop();
            ConsoleWriter.WriteSuccessLine(MAUIStr.Obj.Share_Finish, s.ElapsedMilliseconds / 1000m);
        }

        public override bool InternalCutImage(string inFile, string outFolder, string infoFile, string itemName, int format)
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            bool ans = base.InternalCutImage(inFile, outFolder, infoFile, itemName, format);
            s.Stop();
            ConsoleWriter.WriteSuccessLine(MAUIStr.Obj.Share_Finish, s.ElapsedMilliseconds / 1000m);
            return ans;
        }

        public override bool InternalSpliceImage(string inFile, string outFolder, string infoFile, string itemName, int format, int maxWidth, int maxHeight)
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            bool ans = base.InternalSpliceImage(inFile, outFolder, infoFile, itemName, format, maxWidth, maxHeight);
            s.Stop();
            ConsoleWriter.WriteSuccessLine(MAUIStr.Obj.Share_Finish, s.ElapsedMilliseconds / 1000m);
            return ans;
        }

        public override void InternalDecodeImage(string inFile, string outFile, int format)
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            base.InternalDecodeImage(inFile, outFile, format);
            s.Stop();
            ConsoleWriter.WriteSuccessLine(MAUIStr.Obj.Share_Finish, s.ElapsedMilliseconds / 1000m);
        }

        public override void InternalEncodeImage(string inFile, string outFile, int format, int format2)
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            base.InternalEncodeImage(inFile, outFile, format, format2);
            s.Stop();
            ConsoleWriter.WriteSuccessLine(MAUIStr.Obj.Share_Finish, s.ElapsedMilliseconds / 1000m);
        }

        public override void InternalParseReanim(string inFile, string outFile, int outformat, bool predictTweens = false)
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            base.InternalParseReanim(inFile, outFile, outformat, predictTweens);
            s.Stop();
            ConsoleWriter.WriteSuccessLine(MAUIStr.Obj.Share_Finish, s.ElapsedMilliseconds / 1000m);
        }

        public override void InternalParseParticles(string inFile, string outFile, int outformat)
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            base.InternalParseParticles(inFile, outFile, outformat);
            s.Stop();
            ConsoleWriter.WriteSuccessLine(MAUIStr.Obj.Share_Finish, s.ElapsedMilliseconds / 1000m);
        }

        public override void InternalParseTrail(string inFile, string outFile, int outformat)
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            base.InternalParseTrail(inFile, outFile, outformat);
            s.Stop();
            ConsoleWriter.WriteSuccessLine(MAUIStr.Obj.Share_Finish, s.ElapsedMilliseconds / 1000m);
        }

        public override void InternalPam(string inFile, string outFile, int informat, int outformat)
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            base.InternalPam(inFile, outFile, informat, outformat);
            s.Stop();
            ConsoleWriter.WriteSuccessLine(MAUIStr.Obj.Share_Finish, s.ElapsedMilliseconds / 1000m);
        }

        public override void InternalDecodeRTON(string inFile, string outFile, int format)
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            base.InternalDecodeRTON(inFile, outFile, format);
            s.Stop();
            ConsoleWriter.WriteSuccessLine(MAUIStr.Obj.Share_Finish, s.ElapsedMilliseconds / 1000m);
        }

        public override void InternalEncodeRTON(string inFile, string outFile, int format)
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            base.InternalEncodeRTON(inFile, outFile, format);
            s.Stop();
            ConsoleWriter.WriteSuccessLine(MAUIStr.Obj.Share_Finish, s.ElapsedMilliseconds / 1000m);
        }

        public override void InternalDecompress(string inFile, string outFile, int format)
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            base.InternalDecompress(inFile, outFile, format);
            s.Stop();
            ConsoleWriter.WriteSuccessLine(MAUIStr.Obj.Share_Finish, s.ElapsedMilliseconds / 1000m);
        }

        public override void InternalCompress(string inFile, string outFile, int format)
        {
            Stopwatch s = new Stopwatch();
            s.Start();
            base.InternalCompress(inFile, outFile, format);
            s.Stop();
            ConsoleWriter.WriteSuccessLine(MAUIStr.Obj.Share_Finish, s.ElapsedMilliseconds / 1000m);
        }

        public override void InternalDoFile(string filepath, params object[] args)
        {
            base.InternalDoFile(filepath, args);
            ConsoleWriter.WriteSuccessLine(MAUIStr.Obj.Share_Finish_NoTime);
        }

        public override void InternalLoadTextBox(object o)
        {
        }

        public override void InternalPrint(params object[] os)
        {
            StringBuilder str = new StringBuilder();
            if (os.Length != 0)
            {
                string nil = "nil";
                for (int i = 0; i < os.Length; i++)
                {
                    str.Append((os[i]?.ToString()) ?? nil);
                    str.Append(' ');
                }
                str.Remove(str.Length - 1, 1);
            }
            Console.WriteLine(str);
        }

        public override bool? InternalAlert(string text, string title, bool ask)
        {
            if (ask)
            {
                Console.WriteLine(title);
                Console.WriteLine(text);
                Console.WriteLine(MAUIStr.Obj.Console_API_Alert_Ask, "OK", "Cancel");
                return Console.ReadKey().KeyChar.ToString().ToUpper() == "Y";
            }
            else
            {
                Console.WriteLine(title);
                Console.WriteLine(text);
                Console.WriteLine(MAUIStr.Obj.Console_API_Alert_Continue);
                return null;
            }
        }

        public override string InternalPrompt(string text, string title, string defaulttext)
        {
            Console.WriteLine(title);
            Console.WriteLine(text);
            Console.Write(defaulttext);
            return Console.ReadLine();
        }

        public override string InternalSheet(string title, params string[] items)
        {
            Console.WriteLine(title);
            for (int i = 0; i < items.Length; i++)
            {
                Console.WriteLine(MAUIStr.Obj.Console_API_ActionSheet_Item, i, items[i]);
            }
            string inPut = Console.ReadLine();
            try
            {
                return items[Convert.ToInt32(inPut)];
            }
            catch (Exception)
            {
                return null;
            }
        }

        public override string InternalChooseFolder()
        {
            Console.WriteLine(MAUIStr.Obj.Console_API_ChooseFolder_Ask);
            string path = Console.ReadLine();
            if (path.StartsWith('"')) path = path[1..^1];
            return path;
        }

        public override string InternalChooseOpenFile()
        {
            Console.WriteLine(MAUIStr.Obj.Console_API_ChooseOpenFile_Ask);
            string path = Console.ReadLine();
            if (path.StartsWith('"')) path = path[1..^1];
            return path;
        }

        public override string InternalChooseSaveFile()
        {
            Console.WriteLine(MAUIStr.Obj.Console_API_ChooseSaveFile_Ask);
            string path = Console.ReadLine();
            if (path.StartsWith('"')) path = path[1..^1];
            return path;
        }

        public override void InternalOpenUrl(string url) => Permission.OpenUrl(url);
    }
}
