using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniUpdate.CustomControls;

namespace TestDownload
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string[] myargs = { "/link|http://update.blueboxproduction.ru/launcher/link.txt", "/FS|starterargs.exe" };
            Updater fd = new Updater();
            fd.Show();
            fd.Start(myargs);
            Console.ReadLine();
        }
    }
}
