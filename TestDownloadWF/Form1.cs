using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UniUpdate.CustomControls;

namespace TestDownloadWF
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Load += (s, e) => 
            {
                
                
            };
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string[] myargs = { "/link|http://update.blueboxproduction.ru/launcher/link.txt", "/FS|test.exe" };
            Updater updater = new Updater();
            updater.TopMostWindow = true;
            //flowLayoutPanel1.Controls.Add(fd);
            updater.Start(myargs, true);
        }
    }
}
