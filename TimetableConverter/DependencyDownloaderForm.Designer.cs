namespace TimetableConverter
{
    partial class frmDependencyDownloader
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.DependencyDownloaderTips = new System.Windows.Forms.ToolTip(this.components);
            this.tbxMain = new System.Windows.Forms.TextBox();
            this.btnDownload = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tbxMain
            // 
            this.tbxMain.Location = new System.Drawing.Point(12, 137);
            this.tbxMain.Multiline = true;
            this.tbxMain.Name = "tbxMain";
            this.tbxMain.ReadOnly = true;
            this.tbxMain.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbxMain.Size = new System.Drawing.Size(324, 96);
            this.tbxMain.TabIndex = 10;
            this.tbxMain.Text = "Dependencies required before this program can be used. Click the download button " +
    "to download them.";
            this.tbxMain.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // btnDownload
            // 
            this.btnDownload.Location = new System.Drawing.Point(12, 239);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(324, 60);
            this.btnDownload.TabIndex = 11;
            this.btnDownload.Text = "&Download Dependencies";
            this.btnDownload.UseVisualStyleBackColor = true;
            this.btnDownload.Click += new System.EventHandler(this.btnDownload_Click);
            // 
            // frmDependencyDownloader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(348, 311);
            this.Controls.Add(this.btnDownload);
            this.Controls.Add(this.tbxMain);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmDependencyDownloader";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Dependency Downloader Form";
            this.Activated += new System.EventHandler(this.frmDependencyDownloader_Activated);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolTip DependencyDownloaderTips;
        private System.Windows.Forms.TextBox tbxMain;
        private System.Windows.Forms.Button btnDownload;
    }
}