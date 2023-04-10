using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlienPAK
{
    public static class SharedData
    {
        public static string pathToAI = "";
    }
    public enum AlienContentType 
    { 
        TEXTURE, 
        MODEL,
        UI,
        SCRIPT, 
        ANIMATION,
        MATERIAL_MAPPINGS,
        SHADER,
        NONE_SPECIFIED 
    };

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            //Need DLLs in directory for image previews to work :(
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "DirectXTexNet.dll")) File.WriteAllBytes(AppDomain.CurrentDomain.BaseDirectory + "DirectXTexNet.dll", Properties.Resources.DirectXTexNet);
            Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "x64");
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "x64/DirectXTexNetImpl.dll")) File.WriteAllBytes(AppDomain.CurrentDomain.BaseDirectory + "x64/DirectXTexNetImpl.dll", Properties.Resources.DirectXTexNetImpl_64);

            //Set paths
            if (args.Length > 0 && args[0] == "-opencage") for (int i = 1; i < args.Length; i++) SharedData.pathToAI += args[i] + " ";
            else SharedData.pathToAI = Environment.CurrentDirectory + " ";
            SharedData.pathToAI = SharedData.pathToAI.Substring(0, SharedData.pathToAI.Length - 1);

            //Verify location
            if (args.Length != 0 && args[0] == "-opencage" && !File.Exists(SharedData.pathToAI + "/AI.exe")) throw new Exception("This tool was launched incorrectly, or was not placed within the Alien: Isolation directory.");

            //Launch application
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(true);
            if (args.Length != 0 && args[0] == "-opencage") Application.Run(new Landing());
            else Application.Run(new Explorer(args, AlienContentType.NONE_SPECIFIED));
        }
    }
}
