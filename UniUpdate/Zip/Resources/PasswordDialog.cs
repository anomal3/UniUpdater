using System.Windows.Forms;

namespace Ionic.Zip
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;

    public partial class PasswordDialog : Form
    {
        public PasswordDialog()
        {
            InitializeComponent();
        }

        private Label prompt;
        private TextBox textBox1;
        private bool wasCanceled = false;
        public string EntryName
        {
            set
            {
                prompt.Text = "Enter the password for " + value;
            }
        }
        public string Password
        {
            get
            {
                if (wasCanceled) return null;
                return textBox1.Text;
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            wasCanceled = true;
            this.Close();
        }

        private void InitializeComponent()
        {
            this.prompt = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // prompt
            // 
            this.prompt.AutoSize = true;
            this.prompt.Location = new System.Drawing.Point(12, 9);
            this.prompt.Name = "prompt";
            this.prompt.Size = new System.Drawing.Size(35, 13);
            this.prompt.TabIndex = 0;
            this.prompt.Text = "label1";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(15, 138);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(257, 20);
            this.textBox1.TabIndex = 1;
            // 
            // PasswordDialog
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.prompt);
            this.Name = "PasswordDialog";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
