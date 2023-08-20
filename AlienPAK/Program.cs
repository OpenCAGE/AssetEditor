using Assimp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlienPAK
{
    public static class SharedData
    {
        public static string pathToAI = "";
        public static bool openedViaOpenCAGE = false;
    }

    static class Program
    {
        static Dictionary<string, string> _args;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            _args = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            {
                var arguments = Environment.GetCommandLineArgs();
                for (int i = 0; i < arguments.Length; i++)
                {
                    var match = Regex.Match(arguments[i], "-([^=]+)=(.*)");
                    if (!match.Success) continue;
                    var vName = match.Groups[1].Value;
                    var vValue = match.Groups[2].Value;
                    _args[vName] = vValue;

                    if (_args[vName].Substring(_args[vName].Length - 1) == "\"")
                        _args[vName] = _args[vName].Substring(0, _args[vName].Length - 1);
                }
            }

#if DEBUG
            LogStream logstream = new LogStream(delegate (String msg, String userData) {
                Console.WriteLine(msg);
            });
            logstream.Attach();
#endif
            //Need DLLs in directory for image previews to work :(
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "DirectXTexNet.dll")) File.WriteAllBytes(AppDomain.CurrentDomain.BaseDirectory + "DirectXTexNet.dll", Properties.Resources.DirectXTexNet);
            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "x64");
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "x64/DirectXTexNetImpl.dll")) File.WriteAllBytes(AppDomain.CurrentDomain.BaseDirectory + "x64/DirectXTexNetImpl.dll", Properties.Resources.DirectXTexNetImpl_64);

            //...and now for model import/export too...
            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "/runtimes/win-x86/");
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "/runtimes/win-x86/assimp.dll")) File.WriteAllBytes(AppDomain.CurrentDomain.BaseDirectory + "/runtimes/win-x86/assimp.dll", Properties.Resources.assimp);
            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "/runtimes/win-x64/");
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "/runtimes/win-x64/assimp.dll")) File.WriteAllBytes(AppDomain.CurrentDomain.BaseDirectory + "/runtimes/win-x64/assimp.dll", Properties.Resources.assimp_64);
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "assimp.dll")) File.WriteAllBytes(AppDomain.CurrentDomain.BaseDirectory + "assimp.dll", Properties.Resources.assimp_64);

            //TODO: remove the need for the above!

            //Set path to AI
            if (GetArgument("pathToAI") != null)
            {
                SharedData.pathToAI = GetArgument("pathToAI");
                SharedData.openedViaOpenCAGE = true;
            }
            else
            {
                SharedData.pathToAI = Environment.CurrentDirectory;
            }

            //Verify location
            if (!File.Exists(SharedData.pathToAI + "/AI.exe")) 
                throw new Exception("This tool was launched incorrectly, or was not placed within the Alien: Isolation directory.");

            //Launch application
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(true);
            if (GetArgument("pathToAI") != null && (GetArgument("level") == null || GetArgument("mode") == null))
                Application.Run(new Landing());
            else 
                Application.Run(new Explorer(GetArgument("level"), GetArgument("mode")));
        }

        public static string GetArgument(string name)
        {
            if (_args.ContainsKey(name))
                return _args[name];
            return null;
        }
    }
}
