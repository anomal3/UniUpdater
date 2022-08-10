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
using UniUpdate.SequentalDownload;

namespace UniUpdate.CustomControls
{
    /// <summary>
    /// 
    /// </summary>
    public partial class Updater : UserControl
    {
        /// <summary>
        /// Экземпляр без аргуметов. 
        /// </summary>
        public Updater()
        {
            InitializeComponent();
            wc.DownloadFileCompleted += Wc_DownloadFileCompleted;
        }

        private async void Wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            ++i;
            if (i < downloadFiles.Count) download();

            else
            {
                BeginInvoke(new Action(() => { lblFileProgress.Text = "Preparing Install..."; }));
                await Task.Delay(2500);
                ExitCode();
            }
        }

        #region Variables

        string urlBase;

        string fileStart;

        private List<string> downloadFiles = new List<string>();

        private WebClient wc = new WebClient();

        private protected Random random = new Random();

        private int i = 0;

        private string rootDownloadURL;

        #endregion

        #region Properties

        /// <summary>
        /// Окно загрузки поверх других окон. Нужен только если выполняется в отдельном окне
        /// </summary>
        [Browsable(true)]
        [Description("Вызывать поверх других окон")]
        public bool TopMostWindow
        { 
            get { return _topMostWindow; }
            set { _topMostWindow = value; }
        }

        private bool _topMostWindow;
        #endregion

        /// <summary>
        /// Запуск скачивания. 
        /// </summary>
        /// <param name="args"></param>
        /// <param name="WinForm"></param>
        /// <param name="OneFile"></param>
        public async void Start(string[] args, bool WinForm = false)
        {
            if (WinForm)
            {
                Form form = new Form();
                form.Size = new Size(830, 50);
                form.FormBorderStyle = FormBorderStyle.None;
                form.Controls.Add(this);
                this.Location = new Point(5, 0);
                form.TopMost = _topMostWindow;
                form.StartPosition = FormStartPosition.CenterScreen;
                form.ShowInTaskbar = false;
                form.Show();
            }
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
                            urlBase = await new WebClient().DownloadStringTaskAsync(new Uri(InArgs));
                            if (!string.IsNullOrWhiteSpace(urlBase))
                            {
                                rootDownloadURL = urlBase.Split('\n').First();
                                downloadFiles = urlBase.Split('\n').Skip(1).ToList();

                                if (downloadFiles[0].StartsWith("one"))
                                {
                                    OneFileUpdate(downloadFiles[0].Substring(downloadFiles[0].IndexOf('|') + 1));
                                    continue;
                                }
                                Thread th = new Thread(download);
                                th.Start();

                                
                            }
                            else
                            {
                                MessageBox.Show("There is no argument to load.\r\nThe App will close!", "Error Arguments", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                ExitCode();
                            }

                            break;

                        case "FS":
                            fileStart = InArgs;
                            break;

                        case "one":
                            OneFileUpdate(InArgs);
                            break;
                    }
                }
            }
            
        }


        private async void OneFileUpdate(string InArgs)
        {
            OneUpdate oneUpdate = new OneUpdate();
            oneUpdate.progressBar = prBar;
            //oneUpdate.lblSpeed.Text = speedPerc;
            oneUpdate.lblPerc = lblPersentage;
            oneUpdate.lblTotal = lblSize;
            oneUpdate.lblUpdate = lblFileProgress;
            oneUpdate.DownloadFile(InArgs, Environment.CurrentDirectory + "\\update.zip");
            lblFileProgress.Text = InArgs;
            while (!oneUpdate.IsUpdate)
                await Task.Delay(1000);

            await Task.Delay(1000);
            ExitCode();
        }

        private void download()
        {
            wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(Downloader_DownloadProgressChanged);
            string seperate = "/";
            try
            {
                lblFileProgress.Invoke(new Action(() => { lblFileProgress.Text = downloadFiles[i]; }));
                string _chk = downloadFiles[i].Substring(downloadFiles[i].LastIndexOf("/") + 1);

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                if (downloadFiles[i].Split('/').Length > 1)
                {
                    //Значит есть подпапка... Вычисляем
                    var folder = downloadFiles[i].Substring(0, downloadFiles[i].IndexOf("/"));
                    if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, folder)))
                    {
                        Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, folder));
                        seperate = $"/{folder}/";
                    }
                    else seperate = $"/{folder}/";
                }
                wc.DownloadFileAsync(new Uri(Path.Combine(rootDownloadURL, downloadFiles[i]) + "?random=" + random.Next().ToString()), Environment.CurrentDirectory + seperate + _chk);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        void Downloader_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            prBar.Invoke(new Action(() => prBar.Value = e.ProgressPercentage));
            lblPersentage.Invoke(new Action(() => { lblPersentage.Text = e.ProgressPercentage.ToString() + "%"; }));

            lblSize.Invoke(new Action(() =>
            {
                lblSize.Text = string.Format("{0} MB's / {1} MB's",
                    (e.BytesReceived / 1024d / 1024d).ToString("0.00"),
                    (e.TotalBytesToReceive / 1024d / 1024d).ToString("0.00"));
            }));
        }

        private void ExitCode() 
        { 
            if(File.Exists(fileStart))
                System.Diagnostics.Process.Start(fileStart);
            Application.Exit();
        }
    }
}
