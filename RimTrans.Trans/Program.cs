using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using RimTrans.Builder;
using RimTrans.Builder.Crawler;

namespace RimTrans.Trans
{
    class Program
    {
        #region Face Text

        static Random random = new Random();

        static string[] faceTextGood = {
            "╭(￣▽￣)╯   ",
            "(●′?｀●)    ",
            "o(>ω<)o     ",
            " (*ﾟ∇ﾟ)     ",
            " (*´∀`)     ",
            " ( ﾟ∀ﾟ)     ",
            " (￣∇￣)    ",
            "(`・ω・´)   ",
            "(′；ω；‘)   ",
            "(^・ω・^ )  ",
            "╭(●｀?′●)╯  ",
            "(=^･ω･^=)   ",
            "o(*≥▽≤)ツ   ",
            "o(ノﾟ∀ﾟ)ノ  ",
            "(ノ≧∇≦)ノ ",
            "(=^･ｪ･^=)   ",
        };
        public static string FaceGood()
        {
            return faceTextGood[random.Next(9)];
        }

        static string[] faceTextBad =
        {
            "(ﾟДﾟ≡ﾟдﾟ)!? ",
            " ( ;´Д`)    ",
            "(*゜ロ゜)ノ ",
            "Σ( ￣д￣；) ",
            "(っ °Д °;)っ",
            "Σ( ° △ °|||)",
            " (╯°Д°)╯    ",
            "（＞д＜）   ",
            "（ ＴДＴ）  ",
            "(´Ａ｀。)   ",
            "o(￣ヘ￣o＃)",
            "ヽ(#`Д´)ﾉ   ",
            " (|||ﾟдﾟ)   ",
            " ヽ(≧Д≦)ノ",
            " ヽ(#`Д´)ﾉ  ",
            "_(:з」∠)_  ",
        };
        public static string FaceBad()
        {
            return faceTextBad[random.Next(9)];
        }

        #endregion

        class Context
        {
            public class Language
            {
                public string RealName = null;
                public string NativeName = null;
                public bool IsCustom = false;
                public string CustomPath = null;
            }

            public string CorePath = null;
            public bool CleanModeOn = false;
            public string ModPath = null;
            public List<Language> Languages = new List<Language>();
        }

        static void Main(string[] args)
        {
            if (args.Length == 0) return;

            #region Application Info

            Assembly asm = Assembly.GetExecutingAssembly();
            string title = asm.GetCustomAttribute<AssemblyTitleAttribute>().Title;
            string version = asm.GetCustomAttribute<AssemblyFileVersionAttribute>().Version;
            string copyright = asm.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;

            Console.Title = title;
            Console.OutputEncoding = Encoding.UTF8;
            Log.WriteLine();
            Log.WriteLine(ConsoleColor.Cyan, $" {FaceGood()}{FaceGood()}{FaceGood()}");
            Log.WriteLine();
            Log.WriteLine(ConsoleColor.Cyan, $" {title}");
            Log.WriteLine();
            Log.WriteLine(ConsoleColor.Cyan, $" Version {version}");
            Log.WriteLine();
            Log.WriteLine(ConsoleColor.Cyan, $" {copyright}");
            Log.WriteLine();

            #endregion

            #region Check  Arguments

            string projectFile = null;
            string dllPath = null;
            string generateOption = null;
            var context = new Context();

            foreach (string argument in args)
            {
                if (argument.StartsWith("-p:") && argument.Length > 3)
                {
                    projectFile = argument.Substring(3);
                }
                else if (argument.StartsWith("-Core:") && argument.Length > 6)
                {
                    context.CorePath = argument.Substring(6);
                }
                else if (argument.StartsWith("-Dll:") && argument.Length > 5)
                {
                    dllPath = argument.Substring(5);
                }
                else if (argument == "-Clean")
                {
                    context.CleanModeOn = true;
                }
            }

            // Check Project File
            if (string.IsNullOrWhiteSpace(projectFile) || !File.Exists(projectFile))
            {
                Log.Error();
                Log.WriteLine(ConsoleColor.Red, $"Project File {projectFile} NO FOUND.");
                Log.WriteLine();
                Console.Write("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            // Check DLL Path
            if (string.IsNullOrWhiteSpace(dllPath) || !File.Exists(dllPath))
            {
                Log.Error();
                Log.WriteLine(ConsoleColor.Red, $"DLL {dllPath} NO FOUND.");
                Log.WriteLine();
                Console.Write("Press any key to exit...");
                Console.ReadKey();
                return;
            }
            TypeLoader.Init(dllPath);

            Log.WriteLine(ConsoleColor.Green, $"======== Start Project  {FaceGood()} ========");
            Log.WriteLine();
            Log.WriteLine(ConsoleColor.Green, "Porject File: ");
            Log.Indent();
            Log.WriteLine(ConsoleColor.Cyan, projectFile);

            XDocument doc = XDocument.Load(projectFile);
            XElement root = doc.Root;
            context.ModPath = root.Element("ModPath").Value;

            // Check Mod Path
            if (string.IsNullOrWhiteSpace(context.ModPath) || !Directory.Exists(context.ModPath))
            {
                Log.Error();
                Log.WriteLine(ConsoleColor.Red, $"Mod Directory {context.ModPath} NO FOUND.");
                Log.WriteLine();
                Console.Write("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            Log.WriteLine(ConsoleColor.Green, "Mod Path: ");
            Log.Indent();
            Log.WriteLine(ConsoleColor.Cyan, context.ModPath);

            generateOption = root.Element("GenerateOption").Value;
            Log.WriteLine(ConsoleColor.Green, "Generate Option: ");
            Log.Indent();
            Log.Write(ConsoleColor.Cyan, generateOption + " Mode");
            if (context.CleanModeOn)
            {
                Log.WriteLine(ConsoleColor.Green, " | Clean Mode");
            }
            else
            {
                Log.WriteLine();
            }

            // Check Core Path
            if (generateOption == "Standard" &&
                (string.IsNullOrWhiteSpace(context.CorePath) || !Directory.Exists(context.CorePath)))
            {
                generateOption = "Core";
                Log.Warning();
                Log.WriteLine(ConsoleColor.Yellow, "Core Directory NO FOUND.");
                Log.Indent();
                Log.Write(ConsoleColor.Yellow, "Changed Generate Option to ");
                Log.WriteLine(ConsoleColor.Cyan, "Core Mode");
            }

            if (generateOption == "Standard")
            {
                Log.WriteLine(ConsoleColor.Green, "Core Path: ");
                Log.Indent();
                Log.WriteLine(ConsoleColor.Cyan, context.CorePath);
            }

            XElement Languages = root.Element("Languages");
            foreach (XElement li in Languages.Elements())
            {
                // If IsChecked
                if (string.Compare(li.Element("IsChecked").Value, "false", true) == 0)
                    continue;

                var language = new Context.Language
                {
                    RealName = li.Element("RealName").Value,
                    NativeName = li.Element("NativeName").Value,
                    IsCustom = (string.Compare(li.Element("IsCustom").Value, "true", true) == 0),
                };

                // Check Language Real Name
                if (string.IsNullOrWhiteSpace(language.RealName))
                {
                    Log.Error();
                    Log.WriteLine(ConsoleColor.Red, "Missing Language Name.");
                    continue;
                }

                if (language.IsCustom)
                {
                    language.CustomPath = li.Element("CustomPath").Value;
                }

                context.Languages.Add(language);
            }

            Log.WriteLine();

            #endregion

            #region Process translation

            if (generateOption == "Core")
            {
                GenerateInCoreMode(context);
            }
            else if (generateOption == "Standard")
            {
                GenerateInStandardMode(context);
            }
            else if (generateOption == "Special")
            {
                GenerateInSpecialMode(context);
            }

            #endregion

            // End
            Log.WriteLine(ConsoleColor.Green, $"======== Completed Project  {FaceGood()}========");
            Log.WriteLine();
            Console.Write("Press any key to exit...");
            Console.ReadKey();
            return;
        }

        static IEnumerable<string> ListVersionFolders(string modPath)
        {
            var loadFoldersPath = Path.Combine(modPath, "LoadFolders.xml");
            var dirNames = new[] {
                "", "Common", "1.0", "1.1", "1.2", "1.3", "1.4", "1.5", "1.6"
            };
            if (File.Exists(loadFoldersPath))
            {
                dirNames = XDocument.Load(loadFoldersPath).Root.Elements()
                    .SelectMany(x => x.Elements())
                    .Select(x => x.Value.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).TrimStart('\\'))
                    .Distinct()
                    .ToArray();
            }
            foreach (var dirName in dirNames)
            {
                var dirPath = Path.Combine(modPath, dirName);
                if (!Directory.Exists(dirPath))
                    continue;
                yield return dirName;
            }
        }

        static void GenerateInCoreMode(Context context)
        {
            foreach (var dirName in ListVersionFolders(context.ModPath))
            {
                Log.WriteLine(ConsoleColor.Green, "======== Start Processing Defs and Original Language Data ========");
                string modPath = Path.Combine(context.ModPath, dirName);
                string defsPath = Path.Combine(modPath, "Defs");
                Console.WriteLine(defsPath);
                string keyedPath_English = Path.Combine(modPath, "Languages", "English", "Keyed");
                string stringsPath_English = Path.Combine(modPath, "Languages", "English", "Strings");
                DefinitionData Defs = DefinitionData.Load(defsPath);
                Capture capture = Capture.Parse(Defs);
                capture.ProcessFieldNames(Defs);
                InjectionData DefInjected_Original = InjectionData.Parse("Original", Defs);
                KeyedData Keyed_English = KeyedData.Load("English", keyedPath_English);
                Log.WriteLine(ConsoleColor.Green, "======== Completed Processing Defs and Original Language Data ========");
                Log.WriteLine();

                foreach (var lang in context.Languages)
                {
                    string realName = lang.RealName;
                    string nativeName = lang.NativeName;

                    Log.WriteLine(ConsoleColor.Green, $"======== Start Processing Language: {realName} ( {nativeName} ) ========");

                    string langPath = Path.Combine(modPath, "Languages", realName);
                    if (lang.IsCustom)
                    {
                        langPath = Path.Combine(lang.CustomPath, dirName, "Languages", realName);
                        Log.WriteLine(ConsoleColor.Cyan, "Use Custom Language Output Directory: ");
                    }
                    else
                    {
                        Log.WriteLine(ConsoleColor.Cyan, "Language Path: ");
                    }
                    Log.Indent();
                    Log.WriteLine(ConsoleColor.Cyan, langPath);

                    string defInjectedPath = Path.Combine(langPath, "DefInjected");
                    string keyedPath = Path.Combine(langPath, "Keyed");
                    string stringsPath = Path.Combine(langPath, "Strings");

                    InjectionData DefInjected_New = new InjectionData(realName, DefInjected_Original);
                    KeyedData Keyed_New = new KeyedData(realName, Keyed_English);

                    if (context.CleanModeOn)
                    {
                        DirectoryHelper.CleanDirectory(defInjectedPath, "*.xml");
                        DirectoryHelper.CleanDirectory(keyedPath, "*.xml");
                        DirectoryHelper.CleanDirectory(stringsPath, "*.txt");
                    }
                    else
                    {
                        InjectionData DefInjected_Existed = InjectionData.Load(realName, defInjectedPath, true);
                        DefInjected_New.MatchExisted(DefInjected_Existed);

                        KeyedData Keyed_Existed = KeyedData.Load(realName, keyedPath, true);
                        Keyed_New.MatchExisted(Keyed_Existed);
                    }

                    DefInjected_New.Save(defInjectedPath);
                    Keyed_New.Save(keyedPath);
                    DirectoryHelper.CopyDirectoryEx(stringsPath_English, stringsPath, "*.txt");

                    Log.WriteLine(ConsoleColor.Green, $"======== Completed Processing Language: {realName} ( {nativeName} ) ========");
                    Log.WriteLine();
                }
            }
        }

        static void GenerateInStandardMode(Context context)
        {
            Log.WriteLine(ConsoleColor.Green, "======== Start Processing Core Defs and Original Language Data ========");
            string core_defsPath = Path.Combine(context.CorePath, "Defs");
            string core_langPath = Path.Combine(context.CorePath, "Languages");
            string core_keyedPath_English = Path.Combine(core_langPath, "English", "Keyed");

            DefinitionData Core_Defs = DefinitionData.Load(core_defsPath);
            Capture capture = Capture.Parse(Core_Defs);
            capture.ProcessFieldNames(Core_Defs);
            InjectionData Core_DefInjected_Original = InjectionData.Parse("Original", Core_Defs);
            KeyedData Core_Keyed_English = KeyedData.Load("English", core_keyedPath_English);
            Log.WriteLine(ConsoleColor.Green, "======== Completed Processing Core Defs and Original Language Data ========");
            Log.WriteLine();

            foreach (var dirName in ListVersionFolders(context.ModPath))
            {
                Log.WriteLine(ConsoleColor.Green, "======== Start Processing Mod Defs and Original Language Data ========");
                string modPath = Path.Combine(context.ModPath, dirName);
                string defsPath = Path.Combine(modPath, "Defs");
                string keyedPath_English = Path.Combine(modPath, "Languages", "English", "Keyed");
                string stringsPath_English = Path.Combine(modPath, "Languages", "English", "Strings");

                DefinitionData Defs = DefinitionData.Load(defsPath, Core_Defs);
                capture.ProcessFieldNames(Defs);
                InjectionData DefInjected_Original = InjectionData.Parse("Original", Defs);
                KeyedData Keyed_English = KeyedData.Load("English", keyedPath_English);
                Log.WriteLine(ConsoleColor.Green, "======== Completed Processing Mod Defs and Original Language Data ========");
                Log.WriteLine();

                foreach (var lang in context.Languages)
                {
                    string realName = lang.RealName;
                    string nativeName = lang.NativeName;

                    Log.WriteLine(ConsoleColor.Green, $"======== Start Processing Language: {realName} ( {nativeName} ) ========");

                    string langPath = Path.Combine(modPath, "Languages", realName);
                    if (lang.IsCustom)
                    {
                        langPath = Path.Combine(lang.CustomPath, dirName, "Languages", realName);
                        Log.WriteLine(ConsoleColor.Cyan, "Use Custom Language Output Directory: ");
                    }
                    else
                    {
                        Log.WriteLine(ConsoleColor.Cyan, "Language Path: ");
                    }
                    Log.Indent();
                    Log.WriteLine(ConsoleColor.Cyan, langPath);

                    // Check Custom Path
                    if (string.IsNullOrWhiteSpace(langPath))
                    {
                        Log.Error();
                        Log.WriteLine(ConsoleColor.Red, "Invalid Custom Output Directory Path.");
                        continue;
                    }

                    string core_defInjectedPath = Path.Combine(core_langPath, realName, "DefInjected");
                    string core_keyedPath = Path.Combine(core_langPath, realName, "Keyed");

                    InjectionData Core_DefInjected_New = new InjectionData(realName, Core_DefInjected_Original);
                    InjectionData Core_DefInjected_Existed = InjectionData.Load(realName, core_defInjectedPath);
                    Core_DefInjected_New.MatchExisted(Core_DefInjected_Existed);

                    KeyedData Core_Keyed_New = new KeyedData(realName, Core_Keyed_English);
                    KeyedData Core_Keyed_Existed = KeyedData.Load(realName, core_keyedPath);
                    Core_Keyed_New.MatchExisted(Core_Keyed_Existed);

                    string defInjectedPath = Path.Combine(langPath, "DefInjected");
                    string keyedPath = Path.Combine(langPath, "Keyed");
                    string stringsPath = Path.Combine(langPath, "Strings");

                    InjectionData DefInjected_New = new InjectionData(realName, DefInjected_Original);
                    DefInjected_New.MatchCore(Core_DefInjected_New);

                    KeyedData Keyed_New = new KeyedData(realName, Keyed_English);
                    Keyed_New.MatchCore(Core_Keyed_New);

                    if (context.CleanModeOn)
                    {
                        DirectoryHelper.CleanDirectory(defInjectedPath, "*.xml");
                        DirectoryHelper.CleanDirectory(keyedPath, "*.xml");
                        DirectoryHelper.CleanDirectory(stringsPath, "*.txt");
                    }
                    else
                    {
                        InjectionData DefInjected_Existed = InjectionData.Load(realName, defInjectedPath, true);
                        DefInjected_New.MatchExisted(DefInjected_Existed);

                        KeyedData Keyed_Existed = KeyedData.Load(realName, keyedPath, true);
                        Keyed_New.MatchExisted(Keyed_Existed);
                    }

                    DefInjected_New.Save(defInjectedPath);
                    Keyed_New.Save(keyedPath);
                    DirectoryHelper.CopyDirectoryEx(stringsPath_English, stringsPath, "*.txt");

                    Log.WriteLine(ConsoleColor.Green, $"======== Completed Processing Language: {realName} ( {nativeName} ) ========");
                    Log.WriteLine();
                }
            }
        }

        static void GenerateInSpecialMode(Context context)
        {
            Log.WriteLine(ConsoleColor.Green, "======== Start Processing Core Defs and Original Language Data ========");
            string core_defsPath = Path.Combine(context.CorePath, "Defs");
            string core_langPath = Path.Combine(context.CorePath, "Languages");
            string core_keyedPath_English = Path.Combine(core_langPath, "English", "Keyed");

            DefinitionData Core_Defs = DefinitionData.Load(core_defsPath);
            Capture capture = Capture.Parse(Core_Defs);
            capture.ProcessFieldNames(Core_Defs);
            //InjectionData Core_DefInjected_Original = InjectionData.Parse(Core_Defs);
            //KeyedData Core_Keyed_English = KeyedData.Load(core_keyedPath_English);
            Log.WriteLine(ConsoleColor.Green, "======== Completed Processing Core Defs ========");
            Log.WriteLine();

            foreach (var dirName in ListVersionFolders(context.ModPath))
            {
                Log.WriteLine(ConsoleColor.Green, "======== Start Processing Mod Defs and Original Language Data ========");
                string modPath = Path.Combine(context.ModPath, dirName);
                string defsPath = Path.Combine(modPath, "Defs");
                string keyedPath_English = Path.Combine(modPath, "Languages", "English", "Keyed");
                string stringsPath_English = Path.Combine(modPath, "Languages", "English", "Strings");

                DefinitionData Defs = DefinitionData.Load(defsPath, Core_Defs);
                capture.ProcessFieldNames(Defs);
                InjectionData DefInjected_Original = InjectionData.Parse("Original", Defs);
                KeyedData Keyed_English = KeyedData.Load("English", keyedPath_English);
                Log.WriteLine(ConsoleColor.Green, "======== Completed Processing Mod Defs and Original Language Data ========");
                Log.WriteLine();

                foreach (var lang in context.Languages)
                {
                    string realName = lang.RealName;
                    string nativeName = lang.NativeName;

                    Log.WriteLine(ConsoleColor.Green, $"======== Start Processing Language: {realName} ( {nativeName} ) ========");

                    string langPath = Path.Combine(modPath, "Languages", realName);
                    if (lang.IsCustom)
                    {
                        langPath = Path.Combine(lang.CustomPath, dirName, "Languages", realName);
                        Log.WriteLine(ConsoleColor.Cyan, "Use Custom Language Output Directory: ");
                    }
                    else
                    {
                        Log.WriteLine(ConsoleColor.Cyan, "Language Path: ");
                    }
                    Log.Indent();
                    Log.WriteLine(ConsoleColor.Cyan, langPath);

                    // Check Custom Path
                    if (string.IsNullOrWhiteSpace(langPath))
                    {
                        Log.Error();
                        Log.WriteLine(ConsoleColor.Red, "Invalid Custom Output Directory Path.");
                        continue;
                    }

                    string core_defInjectedPath = Path.Combine(core_langPath, realName, "DefInjected");
                    string core_keyedPath = Path.Combine(core_langPath, realName, "Keyed");

                    //InjectionData Core_DefInjected_New = new InjectionData(Core_DefInjected_Original);
                    //InjectionData Core_DefInjected_Existed = InjectionData.Load(core_defInjectedPath);
                    //Core_DefInjected_New.MatchExisted(Core_DefInjected_Existed);

                    //KeyedData Core_Keyed_New = new KeyedData(Core_Keyed_English);
                    //KeyedData Core_Keyed_Existed = KeyedData.Load(core_keyedPath);
                    //Core_Keyed_New.MatchExisted(Core_Keyed_Existed);

                    string defInjectedPath = Path.Combine(langPath, "DefInjected");
                    string keyedPath = Path.Combine(langPath, "Keyed");
                    string stringsPath = Path.Combine(langPath, "Strings");

                    InjectionData DefInjected_New = new InjectionData(realName, DefInjected_Original);
                    //DefInjected_New.MatchCore(Core_DefInjected_New);

                    KeyedData Keyed_New = new KeyedData(realName, Keyed_English);
                    //Keyed_New.MatchCore(Core_Keyed_New);

                    if (context.CleanModeOn)
                    {
                        DirectoryHelper.CleanDirectory(defInjectedPath, "*.xml");
                        DirectoryHelper.CleanDirectory(keyedPath, "*.xml");
                        DirectoryHelper.CleanDirectory(stringsPath, "*.txt");
                    }
                    else
                    {
                        InjectionData DefInjected_Existed = InjectionData.Load(realName, defInjectedPath, true);
                        DefInjected_New.MatchExisted(DefInjected_Existed);

                        KeyedData Keyed_Existed = KeyedData.Load(realName, keyedPath, true);
                        Keyed_New.MatchExisted(Keyed_Existed);
                    }

                    DefInjected_New.Save(defInjectedPath);
                    Keyed_New.Save(keyedPath);
                    DirectoryHelper.CopyDirectoryEx(stringsPath_English, stringsPath, "*.txt");

                    Log.WriteLine(ConsoleColor.Green, $"======== Completed Processing Language: {realName} ( {nativeName} ) ========");
                    Log.WriteLine();
                }
            }
        }
    }
}
