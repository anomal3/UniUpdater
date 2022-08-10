using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UniUpdate;
using UniUpdate.CustomControls;
using UniUpdate.SequentalDownload;

namespace TestDownloadWF
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string[] myargs = { "/link|http://update.blueboxproduction.ru/launcher/link.txt", $"/FS|{Environment.CurrentDirectory}\\test.bat" };
            string[] myargs2 = { "/link|http://update.blueboxproduction.ru/launcher/link2.txt", $"/FS|{Environment.CurrentDirectory}\\test.bat" };
            string[] onearg = { "/one|http://update.blueboxproduction.ru/launcher/update.zip", $"/FS|{Environment.CurrentDirectory}\\test.bat" };
            Updater updater = new Updater();
            updater.TopMostWindow = true;

            #region Если не нужно отдельного окна, а можно воспользоваться UserControl
            //flowLayoutPanel1.Controls.Add(fd);
            //updater.Start(myargs);
            #endregion

            updater.Start(myargs2, true);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OneUpdate oneUpdate = new OneUpdate();
            oneUpdate.progressBar = progressBar1;
            oneUpdate.lblPerc = lblPerc;
            oneUpdate.lblSpeed= lblSpeed;
            oneUpdate.lblTotal= lblTotal;
            oneUpdate.lblUpdate = lblUpdate;

            oneUpdate.DownloadFile("http://update.blueboxproduction.ru/launcher/update.zip", Environment.CurrentDirectory + "\\update.zip");

        }
    }
}
