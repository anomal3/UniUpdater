using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UniUpdate.CustomControls
{
    public partial class frmDownload : UserControl
    {
        public frmDownload(string[] args)
        {
            InitializeComponent();
            Start(args);
        }

        #region Variables

        string urlBase;

        string fileStart;

        List<string> downloadFiles = new List<string>();

        WebClient wc = new WebClient();

        Random random = new Random();

        #endregion

        #region Properties

        public string SizeDownload
        {
            get
            {
                return _sizeDownload;
            }
            set
            {
                _sizeDownload = value;
                lblSize.Text = value;
            }
        }

        private string _sizeDownload;

        #endregion

        /// <summary>
        /// Afther Initialize args
        /// </summary>
        /// <param name="args"></param>
        public async void Start(string[] args)
        {
            if (args.Length != 0)
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13;

                foreach (var arg in args)
                {

                    var input = arg.Remove(0, 1);

                    string InArgs = input.Substring(input.IndexOf('|') + 1).Replace(" ", "");
                    input = input.Substring(0, input.LastIndexOf('|'));

                    switch (input)
                    {
                        case "link":
                            urlBase =  await new WebClient().DownloadStringTaskAsync(new Uri(InArgs));
                            break;
                        case "FS":
                            fileStart = InArgs;
                            break;
                    }
                }

                Thread th = new Thread(download);
                th.Start();
            }
        }


        internal async void download()
        {
            int i = 0;
            if (!string.IsNullOrWhiteSpace(urlBase))
                downloadFiles = urlBase.Split('\n').ToList();
            else
            {
                MessageBox.Show("There is no argument to load.\r\nThe App will close!", "Start error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ExitCode();
            }

            
            wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(Downloader_DownloadProgressChanged);

            try
            {

               // label1.Invoke(new Action(() => label1.Text = downloadFiles[i]));
                var Name = downloadFiles[i];
                string _chk = Name.Substring(Name.LastIndexOf("/") + 1);

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

               // wc.DownloadFileAsync(new Uri(downloadFiles[i] + "?random=" + random.Next().ToString()), Environment.CurrentDirectory + @"/" + _chk);

                wc.DownloadFileCompleted += (args, e1) =>
                {
                    ++i;
                    if (i < downloadFiles.Count) download();
                    else
                    {
                        File.Delete("link.txt");
                        Application.Exit();
                    }
                };
            }
            catch { }
        }

        void Downloader_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {

           // lblSpeed.Invoke(new Action(() => lblSpeed.Text = string.Format("{0} kb/s", (e.BytesReceived / 1024 / sw.Elapsed.TotalSeconds).ToString("0.00"))));
           // progressBar1.Invoke(new Action(() => progressBar1.Value = e.ProgressPercentage));
           // lblPerc.Invoke(new Action(() => lblPerc.Text = e.ProgressPercentage.ToString() + "%"));
           //
           // lblPerc.Invoke(new Action(() => lblDownload.Text = string.Format("{0} MB's / {1} MB's",
           //     (e.BytesReceived / 1024d / 1024d).ToString("0.00"),
           //     (e.TotalBytesToReceive / 1024d / 1024d).ToString("0.00"))));

        }

        private void ExitCode() { System.Diagnostics.Process.Start(fileStart); Application.Exit(); }
    }
}
