using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UniUpdate.SequentalDownload
{
    public class OneUpdate
    {
        #region Variable
        private Stopwatch sw = new Stopwatch();    // The stopwatch which we will be using to calculate the download speed

        SynchronizationContext context;

        #endregion

        #region Properties

        public System.Windows.Forms.Label lblPerc
        {
            get
            {
                return _lblPerc;
            }

             set
            {
                _lblPerc = value;
            }
        }
        public System.Windows.Forms.Label lblSpeed
        {
            get
            {
                return _lblSpeed;
            }

             set
            {
                _lblSpeed = value;
            }
        }
        public System.Windows.Forms.Label lblUpdate
        {
            get
            {
                return _lblUpdate;
            }

             set
            {
                _lblUpdate = value;
            }
        }
        public System.Windows.Forms.Label lblTotal
        {
            get
            {
                return _lblTotal;
            }

             set
            {
                _lblTotal = value;
            }
        }

        public System.Windows.Forms.ProgressBar progressBar
        {
            get
            {
                return _progressBar;
            }

             set
            {
                _progressBar = value;
            }
        }

        public bool IsUpdate
        {
            get { return isUpdate; }
            set { isUpdate = value; }
        }

        private System.Windows.Forms.Label _lblPerc = new System.Windows.Forms.Label();
        private System.Windows.Forms.Label _lblSpeed = new System.Windows.Forms.Label();
        private System.Windows.Forms.Label _lblUpdate = new System.Windows.Forms.Label();
        private System.Windows.Forms.Label _lblTotal = new System.Windows.Forms.Label();
        private System.Windows.Forms.ProgressBar _progressBar = new System.Windows.Forms.ProgressBar();
        private bool isUpdate;

        #endregion

        #region Zip

        private void Zip_ExtractProgress(object sender, ExtractProgressEventArgs e)
        {
            switch (e.EventType)
            {
                case ZipProgressEventType.Extracting_AfterExtractEntry:
                    if (context != null)
                        context.Send(
                            (o) =>
                            {
                                // --->
                                _lblUpdate.Text = "Install...";
                                _lblUpdate.Text = string.Format(
                                                "Update {0} in {1} files",
                                                e.EntriesExtracted,
                                                e.EntriesTotal
                                                );
                                try
                                {
                                    _progressBar.Value = e.EntriesExtracted;


                                }
                                catch (Exception ex)
                                {
                                    System.Windows.Forms.MessageBox.Show(ex.ToString());
                                }
                            },
                            null);
                    break;
            }
        }

        /// <summary>
        /// Extract handing
        /// </summary>
        /// <param name="path"></param>
        /// <param name="zip"></param>
        public async void  ExtractAsync(string path, ZipFile zip)
        {
            isUpdate = await zip.ExtractAll(path, true);
            _lblUpdate.Invoke(new Action(() => { _lblUpdate.Text = "Update sucessfull!"; }));
            _lblPerc.Invoke(new Action(() => { _lblPerc.Text = ""; }));
            zip.Dispose();
            await Task.Delay(1000);
            File.Delete(@"update.zip");
        }
        #endregion

        #region Download File
        private async void Completed(object sender, AsyncCompletedEventArgs e)
        {
            // Reset the stopwatch.
            sw.Reset();

            if (e.Cancelled == true)
            {
                System.Windows.Forms.MessageBox.Show("Download has been canceled.");
            }
            else
            {
                try
                {
                    _lblPerc.Text = "Unpacking...";
                    Thread.Sleep(2000);
                    var zip = ZipFile.Read(@"update.zip");
                    zip.ExtractProgress += Zip_ExtractProgress;
                    _progressBar.Maximum = zip.Count;

                    context = SynchronizationContext.Current;
                    await Task.Run(() => {
                        new Thread(
                        delegate ()
                        {
                            ExtractAsync(Environment.CurrentDirectory, zip);
                        }).Start();
                    });
                }
                catch (IOException IOex) { System.Windows.Forms.MessageBox.Show(IOex.ToString()); }
            }
        }

        private async void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            // Calculate download speed and output it to labelSpeed.
            _lblSpeed.Text = string.Format("Speed: {0} mbit/s", (e.BytesReceived / 1024d / 1024d / sw.Elapsed.TotalSeconds).ToString("0.00"));

            // Update the progressbar percentage only when the value is not the same.
            _progressBar.Value = e.ProgressPercentage;

            // Show the percentage on our label.
            _lblPerc.Text = e.ProgressPercentage.ToString() + "%";

            // Update the label with how much data have been downloaded so far and the total size of the file we are currently downloading
            _lblTotal.Text = string.Format("Download : {0} MB in {1} MB",
                (e.BytesReceived / 1024d / 1024d).ToString("0.00"),
                (e.TotalBytesToReceive / 1024d / 1024d).ToString("0.00"));
            await Task.Delay(1);
        }

        /// <summary>
        /// Download one Zip archive for update
        /// </summary>
        /// <param name="urlAddress">string URL</param>
        /// <param name="location">Full filenanme</param>
        public void DownloadFile(string urlAddress, string location)
        {
            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);

                // The variable that will be holding the url address (making sure it starts with http://)
                Uri URL = urlAddress.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ? new Uri(urlAddress) : new Uri("http://" + urlAddress);

                // Start the stopwatch which we will be using to calculate the download speed
                sw.Start();

                try
                {
                    // Start downloading the file
                    webClient.DownloadFileAsync(URL, location);
                }
                catch (Exception ex)
                {
                    string trace = ex.StackTrace;

                    if (ex.StackTrace.Length > 1300)
                        trace = ex.StackTrace.Substring(0, 1300) + " [...] (traceback cut short)";

                    System.Windows.Forms.MessageBox.Show(ex.Message + Environment.NewLine +
                        ex.Source + " raised a " + ex.GetType().ToString() + Environment.NewLine +
                        trace);
                }
            }
        }
        #endregion
    }
}

