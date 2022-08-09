namespace UniUpdate.CustomControls
{
    partial class frmDownload
    {
        /// <summary> 
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов

        /// <summary> 
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.prBar = new System.Windows.Forms.ProgressBar();
            this.lblSize = new UniUpdate.CustomControls.TransparentLabel();
            this.lblPersentage = new System.Windows.Forms.Label();
            this.lblFileProgress = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // prBar
            // 
            this.prBar.Location = new System.Drawing.Point(0, 8);
            this.prBar.Name = "prBar";
            this.prBar.Size = new System.Drawing.Size(820, 18);
            this.prBar.TabIndex = 0;
            // 
            // lblSize
            // 
            this.lblSize.BackColor = System.Drawing.Color.Transparent;
            this.lblSize.Location = new System.Drawing.Point(296, 11);
            this.lblSize.Name = "lblSize";
            this.lblSize.Opacity = 0;
            this.lblSize.Size = new System.Drawing.Size(251, 12);
            this.lblSize.TabIndex = 1;
            this.lblSize.Text = "20MB\'s \\ 1000MB\'s";
            this.lblSize.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblSize.TransparentBackColor = System.Drawing.Color.Transparent;
            // 
            // lblPersentage
            // 
            this.lblPersentage.Location = new System.Drawing.Point(784, 29);
            this.lblPersentage.Name = "lblPersentage";
            this.lblPersentage.Size = new System.Drawing.Size(33, 21);
            this.lblPersentage.TabIndex = 2;
            this.lblPersentage.Text = "100%";
            // 
            // lblFileProgress
            // 
            this.lblFileProgress.AutoEllipsis = true;
            this.lblFileProgress.Location = new System.Drawing.Point(3, 29);
            this.lblFileProgress.Name = "lblFileProgress";
            this.lblFileProgress.Size = new System.Drawing.Size(775, 20);
            this.lblFileProgress.TabIndex = 3;
            this.lblFileProgress.Text = "https://www.blueboxproduction.ru/";
            // 
            // frmDownload
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblFileProgress);
            this.Controls.Add(this.lblPersentage);
            this.Controls.Add(this.lblSize);
            this.Controls.Add(this.prBar);
            this.Name = "frmDownload";
            this.Size = new System.Drawing.Size(820, 49);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ProgressBar prBar;
        private TransparentLabel lblSize;
        private System.Windows.Forms.Label lblPersentage;
        private System.Windows.Forms.Label lblFileProgress;
    }
}
