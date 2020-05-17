using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AlienPAK
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            //Need DLLs in directory for image previews to work :(
            if (!File.Exists("DirectXTexNet.dll")) File.WriteAllBytes("DirectXTexNet.dll", Properties.Resources.DirectXTexNet);
            Directory.CreateDirectory("x64");
            if (!File.Exists("x64/DirectXTexNetImpl.dll")) File.WriteAllBytes("x64/DirectXTexNetImpl.dll", Properties.Resources.DirectXTexNetImpl_64);

            //Launch application
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Explorer(args));
        }
    }
}
