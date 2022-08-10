namespace Ionic.Zip
{
    // The using statements must be inside the namespace scope, because when the SFX is being 
    // generated, this module gets concatenated with other source code and then compiled.

    using System;
    using System.Reflection;
    using System.IO;
    using System.Windows.Forms;

    public partial class WinFormsSelfExtractorStub : Form
    {
        //const string IdString = "DotNetZip Self Extractor, see http://www.codeplex.com/DotNetZip";
        const string DllResourceName = "Ionic.Zip.dll";
        private Label txtExtractDirectory;
        private Label txtComment;
        private Label label2;
        private Label label1;
        private Button btnContents;
        private Button btnExtract;
        private CheckBox chk_OpenExplorer;
        private CheckBox chk_Overwrite;
        private Button btnDirBrowse;
        private Label lblStatus;
        private Button btnCancel;
        private ProgressBar progressBar1;
        private ProgressBar progressBar2;
        int entryCount;

        delegate void ExtractEntryProgress(ExtractProgressEventArgs e);

        void _SetDefaultExtractLocation()
	{
	    // ok, this looks odd, I know. First we set the Textbox to contain a 
	    // particular string.  Then we test to see if the value begins with 
	    // the first part of the string and ends with the last part, and if it
	    // does, then we change the value.  When would that not get replaced? 
	    //

	    // Well, here's the thing.  This module has to compile as it is, as a
	    // standalone sample.  But then, inside DotNetZip, when generating an SFX, 
	    // we do a text.Replace on @@VALUE and insert a different value. 

	    // So the effect is, with a straight compile, the value gets SpecialFolder.Personal. 
	    // If you replace @@VALUE with something else, it stays and does not get replaced. 

            this.txtExtractDirectory.Text = "@@VALUE";

            if (this.txtExtractDirectory.Text.StartsWith("@@") && 
		this.txtExtractDirectory.Text.EndsWith("VALUE"))
	    {
		this.txtExtractDirectory.Text = 
		    System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal),
					   ZipName);
	    }
	}


        public WinFormsSelfExtractorStub()
        {
            InitializeComponent();
            _setCancel = true;
	    entryCount= 0;
	    _SetDefaultExtractLocation();

            try
            {
                if ((zip.Comment != null) && (zip.Comment != ""))
                {
                    txtComment.Text = zip.Comment;
                }
                else
                {
                    label2.Visible = false;
                    txtComment.Visible = false;
                    this.Size = new System.Drawing.Size(this.Width, this.Height - 113);
                }
            }
            catch
            {
		// why would this ever fail?  Not sure. 
                label2.Visible = false;
                txtComment.Visible = false;
                this.Size = new System.Drawing.Size(this.Width, this.Height - 113);
            }
        }


        static WinFormsSelfExtractorStub()
        {
	    // This is important to resolve the Ionic.Zip.dll inside the extractor. 
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(Resolver);
        }


        static System.Reflection.Assembly Resolver(object sender, ResolveEventArgs args)
        {
            Assembly a1 = Assembly.GetExecutingAssembly();
            Assembly a2 = null;

            Stream s = a1.GetManifestResourceStream(DllResourceName);
            int n = 0;
            int totalBytesRead = 0;
            byte[] bytes = new byte[1024];
            do
            {
                n = s.Read(bytes, 0, bytes.Length);
                totalBytesRead += n;
            }
            while (n > 0);

            byte[] block = new byte[totalBytesRead];
            s.Seek(0, System.IO.SeekOrigin.Begin);
            s.Read(block, 0, block.Length);

            a2 = Assembly.Load(block);

            return a2;
        }



        private void btnDirBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            // Default to the My Documents folder.
            folderBrowserDialog1.RootFolder = Environment.SpecialFolder.Personal;
            folderBrowserDialog1.SelectedPath = txtExtractDirectory.Text;
            folderBrowserDialog1.ShowNewFolderButton = true;

            folderBrowserDialog1.Description = "Select the directory for the extracted files.";

            // Show the FolderBrowserDialog.
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                txtExtractDirectory.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void btnExtract_Click(object sender, EventArgs e)
        {
            KickoffExtract();
        }


        private void KickoffExtract()
        {
            // disable most of the UI: 
            this.btnContents.Enabled = false;
            this.btnExtract.Enabled = false;
            this.chk_OpenExplorer.Enabled = false;
            this.chk_Overwrite.Enabled = false;
            this.txtExtractDirectory.Enabled = false;
            this.btnDirBrowse.Enabled = false;
            this.btnExtract.Text = "Extracting...";
            System.Threading.Thread _workerThread = new System.Threading.Thread(this.DoExtract);
            _workerThread.Name = "Zip Extractor thread";
            _workerThread.Start(null);
            this.Cursor = Cursors.WaitCursor;
        }


        private void DoExtract(Object p)
        {
            string targetDirectory = txtExtractDirectory.Text;
            bool WantOverwrite = chk_Overwrite.Checked;
            bool extractCancelled = false;
            _setCancel = false;
            string currentPassword = "";
            SetProgressBars();

            try
            {
                zip.ExtractProgress += ExtractProgress;
                foreach (global::Ionic.Zip.ZipEntry entry in zip)
                {
                    if (_setCancel) { extractCancelled = true; break; }
                    if (entry.Encryption == global::Ionic.Zip.EncryptionAlgorithm.None)
                        try
                        {
                            entry.Extract(targetDirectory, WantOverwrite);
		    entryCount++;
                        }
                        catch (Exception ex1)
                        {
                            DialogResult result = MessageBox.Show(String.Format("Failed to extract entry {0} -- {1}", entry.FileName, ex1.Message.ToString()),
                                 String.Format("Error Extracting {0}", entry.FileName), MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);

                            if (result == DialogResult.Cancel)
                            {
                                extractCancelled = true;
                                break;
                            }
                        }
                    else
                    {
                        if (currentPassword == "")
                        {
                            do
                            {
                                currentPassword = PromptForPassword(entry.FileName);
                            }
                            while (currentPassword == "");
                        }

                        if (currentPassword == null)
                        {
                            extractCancelled = true;
                            currentPassword = "";
                            break;
                        }
                        else
                        {
                            try
                            {
                                entry.ExtractWithPassword(targetDirectory, WantOverwrite, currentPassword);
		    entryCount++;
                            }
                            catch (Exception ex2)
                            {
                                // TODO: probably want a retry here in the case of bad password.
                                DialogResult result = MessageBox.Show(String.Format("Failed to extract the password-encrypted entry {0} -- {1}", entry.FileName, ex2.Message.ToString()),
                                    String.Format("Error Extracting {0}", entry.FileName), MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);

                                if (result == DialogResult.Cancel)
                                {
                                    extractCancelled = true;
                                    break;
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception)
            {
                MessageBox.Show("The self-extracting zip file is corrupted.",
                    "Error Extracting", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                Application.Exit();
            }


            SetUiDone();

            if (extractCancelled) return;

            if (chk_OpenExplorer.Checked)
            {
                string w = System.Environment.GetEnvironmentVariable("WINDIR");
                if (w == null) w = "c:\\windows";
                try
                {
                    System.Diagnostics.Process.Start(Path.Combine(w, "explorer.exe"), targetDirectory);
                }
                catch { }
            }
        }

        private void SetUiDone()
        {
            if (this.btnExtract.InvokeRequired)
            {
                this.btnExtract.Invoke(new MethodInvoker(this.SetUiDone));
            }
            else
            {
                this.lblStatus.Text = String.Format("Finished extracting {0} entries.", entryCount);
                btnExtract.Text = "Extracted.";
                btnExtract.Enabled = false;
                btnCancel.Text = "Quit";
                _setCancel = true;
                this.Cursor = Cursors.Default;
            }
        }

        private void ExtractProgress(object sender, ExtractProgressEventArgs e)
        {
            if (e.EventType == ZipProgressEventType.Extracting_EntryBytesWritten)
            {
                StepEntryProgress(e);
            }

            else if (e.EventType == ZipProgressEventType.Extracting_AfterExtractEntry)
            {
                StepArchiveProgress(e);
            }
            if (_setCancel)
                e.Cancel = true;
        }

        private void StepArchiveProgress(ExtractProgressEventArgs e)
        {
            if (this.progressBar1.InvokeRequired)
            {
                this.progressBar2.Invoke(new ExtractEntryProgress(this.StepArchiveProgress), new object[] { e });
            }
            else
            {
                this.progressBar1.PerformStep();

                // reset the progress bar for the entry:
                this.progressBar2.Value = this.progressBar2.Maximum = 1;
                this.lblStatus.Text = "";
                this.Update();
            }
        }

        private void StepEntryProgress(ExtractProgressEventArgs e)
        {
            if (this.progressBar2.InvokeRequired)
            {
                this.progressBar2.Invoke(new ExtractEntryProgress(this.StepEntryProgress), new object[] { e });
            }
            else
            {
                if (this.progressBar2.Maximum == 1)
                {
                    // reset
                    Int64 max = e.TotalBytesToTransfer;
                    _progress2MaxFactor = 0;
                    while (max > System.Int32.MaxValue)
                    {
                        max /= 2;
                        _progress2MaxFactor++;
                    }
                    this.progressBar2.Maximum = (int)max;
                    this.lblStatus.Text = String.Format("Extracting {0}/{1}: {2} ...",
                        this.progressBar1.Value, zip.Entries.Count, e.CurrentEntry.FileName);
                }

                int xferred = (int)(e.BytesTransferred >> _progress2MaxFactor);

                this.progressBar2.Value = (xferred >= this.progressBar2.Maximum)
                    ? this.progressBar2.Maximum
                    : xferred;

                this.Update();
            }
        }

        private void SetProgressBars()
        {
            if (this.progressBar1.InvokeRequired)
            {
                this.progressBar1.Invoke(new MethodInvoker(this.SetProgressBars));
            }
            else
            {
                this.progressBar1.Value = 0;
                this.progressBar1.Maximum = zip.Entries.Count;
                this.progressBar1.Minimum = 0;
                this.progressBar1.Step = 1;
                this.progressBar2.Value = 0;
                this.progressBar2.Minimum = 0;
                this.progressBar2.Maximum = 1; // will be set later, for each entry.
                this.progressBar2.Step = 1;
            }
        }

        private String ZipName
        {
            get
            {
                return System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);
            }
        }

        private Stream ZipStream
        {
            get
            {
                if (_s != null) return _s;

                // There are only two embedded resources.
                // One of them is the zip dll.  The other is the zip archive.
                // We load the resouce that is NOT the DLL, as the zip archive.
                Assembly a = Assembly.GetExecutingAssembly();
                string[] x = a.GetManifestResourceNames();
                _s = null;
                foreach (string name in x)
                {
                    if ((name != DllResourceName) && (name.EndsWith(".zip")))
                    {
                        _s = a.GetManifestResourceStream(name);
                        break;
                    }
                }

                if (_s == null)
                {
                    MessageBox.Show("No Zip archive found.",
                           "Error Extracting", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                    Application.Exit();
                }
                return _s;
            }
        }

        private ZipFile zip
        {
            get
            {
                if (_zip == null)
                    _zip = global::Ionic.Zip.ZipFile.Read(ZipStream);
                return _zip;
            }
        }

        private string PromptForPassword(string entryName)
        {
            PasswordDialog dlg1 = new PasswordDialog();
            dlg1.EntryName = entryName;
            dlg1.ShowDialog();
            return dlg1.Password;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (_setCancel == false)
                _setCancel = true;
            else
                Application.Exit();
        }

        // workitem 6413
        private void btnContents_Click(object sender, EventArgs e)
        {
            ZipContentsDialog dlg1 = new ZipContentsDialog();
            dlg1.ZipFile = zip;
            dlg1.ShowDialog();
            return;
        }


        private int _progress2MaxFactor;
        private bool _setCancel;
        Stream _s;
        global::Ionic.Zip.ZipFile _zip;

        private void InitializeComponent()
        {
            this.txtExtractDirectory = new System.Windows.Forms.Label();
            this.txtComment = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnContents = new System.Windows.Forms.Button();
            this.btnExtract = new System.Windows.Forms.Button();
            this.chk_OpenExplorer = new System.Windows.Forms.CheckBox();
            this.chk_Overwrite = new System.Windows.Forms.CheckBox();
            this.btnDirBrowse = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.progressBar2 = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // txtExtractDirectory
            // 
            this.txtExtractDirectory.AutoSize = true;
            this.txtExtractDirectory.Location = new System.Drawing.Point(12, 9);
            this.txtExtractDirectory.Name = "txtExtractDirectory";
            this.txtExtractDirectory.Size = new System.Drawing.Size(35, 13);
            this.txtExtractDirectory.TabIndex = 0;
            this.txtExtractDirectory.Text = "label1";
            // 
            // txtComment
            // 
            this.txtComment.AutoSize = true;
            this.txtComment.Location = new System.Drawing.Point(12, 41);
            this.txtComment.Name = "txtComment";
            this.txtComment.Size = new System.Drawing.Size(35, 13);
            this.txtComment.TabIndex = 0;
            this.txtComment.Text = "label1";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 69);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "label1";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 102);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "label1";
            // 
            // btnContents
            // 
            this.btnContents.Location = new System.Drawing.Point(12, 289);
            this.btnContents.Name = "btnContents";
            this.btnContents.Size = new System.Drawing.Size(75, 23);
            this.btnContents.TabIndex = 1;
            this.btnContents.Text = "btnContents";
            this.btnContents.UseVisualStyleBackColor = true;
            // 
            // btnExtract
            // 
            this.btnExtract.Location = new System.Drawing.Point(12, 342);
            this.btnExtract.Name = "btnExtract";
            this.btnExtract.Size = new System.Drawing.Size(75, 23);
            this.btnExtract.TabIndex = 2;
            this.btnExtract.Text = "btnExtract";
            this.btnExtract.UseVisualStyleBackColor = true;
            // 
            // chk_OpenExplorer
            // 
            this.chk_OpenExplorer.AutoSize = true;
            this.chk_OpenExplorer.Location = new System.Drawing.Point(15, 130);
            this.chk_OpenExplorer.Name = "chk_OpenExplorer";
            this.chk_OpenExplorer.Size = new System.Drawing.Size(114, 17);
            this.chk_OpenExplorer.TabIndex = 3;
            this.chk_OpenExplorer.Text = "chk_OpenExplorer";
            this.chk_OpenExplorer.UseVisualStyleBackColor = true;
            // 
            // chk_Overwrite
            // 
            this.chk_Overwrite.AutoSize = true;
            this.chk_Overwrite.Location = new System.Drawing.Point(15, 153);
            this.chk_Overwrite.Name = "chk_Overwrite";
            this.chk_Overwrite.Size = new System.Drawing.Size(95, 17);
            this.chk_Overwrite.TabIndex = 3;
            this.chk_Overwrite.Text = "chk_Overwrite";
            this.chk_Overwrite.UseVisualStyleBackColor = true;
            // 
            // btnDirBrowse
            // 
            this.btnDirBrowse.Location = new System.Drawing.Point(555, 289);
            this.btnDirBrowse.Name = "btnDirBrowse";
            this.btnDirBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnDirBrowse.TabIndex = 1;
            this.btnDirBrowse.Text = "btnDirBrowse";
            this.btnDirBrowse.UseVisualStyleBackColor = true;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(12, 196);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(47, 13);
            this.lblStatus.TabIndex = 0;
            this.lblStatus.Text = "lblStatus";
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(555, 342);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "btnCancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(12, 212);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(618, 23);
            this.progressBar1.TabIndex = 4;
            // 
            // progressBar2
            // 
            this.progressBar2.Location = new System.Drawing.Point(15, 251);
            this.progressBar2.Name = "progressBar2";
            this.progressBar2.Size = new System.Drawing.Size(618, 23);
            this.progressBar2.TabIndex = 4;
            // 
            // WinFormsSelfExtractorStub
            // 
            this.ClientSize = new System.Drawing.Size(642, 387);
            this.Controls.Add(this.progressBar2);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.chk_Overwrite);
            this.Controls.Add(this.chk_OpenExplorer);
            this.Controls.Add(this.btnExtract);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnDirBrowse);
            this.Controls.Add(this.btnContents);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtComment);
            this.Controls.Add(this.txtExtractDirectory);
            this.Name = "WinFormsSelfExtractorStub";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }



    class WinFormsSelfExtractorStubProgram
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new WinFormsSelfExtractorStub());
        }
    }
}
