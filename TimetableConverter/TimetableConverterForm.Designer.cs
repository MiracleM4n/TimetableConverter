namespace TimetableConverter
{
    partial class frmTemperatureConversion
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmTemperatureConversion));
            this.btnExport = new System.Windows.Forms.Button();
            this.lblUsername = new System.Windows.Forms.Label();
            this.lblPassword = new System.Windows.Forms.Label();
            this.lblFileLocation = new System.Windows.Forms.Label();
            this.txtFileLocation = new System.Windows.Forms.TextBox();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.btnFileLocation = new System.Windows.Forms.Button();
            this.tbxMain = new System.Windows.Forms.TextBox();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.cbxCampus = new System.Windows.Forms.ComboBox();
            this.lblCampus = new System.Windows.Forms.Label();
            this.TimetableConversionTips = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(12, 239);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(324, 60);
            this.btnExport.TabIndex = 10;
            this.btnExport.Text = "&Export my Calendar!";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnGo_Click);
            // 
            // lblUsername
            // 
            this.lblUsername.AutoSize = true;
            this.lblUsername.Location = new System.Drawing.Point(9, 109);
            this.lblUsername.Name = "lblUsername";
            this.lblUsername.Size = new System.Drawing.Size(58, 13);
            this.lblUsername.TabIndex = 2;
            this.lblUsername.Text = "&Username:";
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(12, 148);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(56, 13);
            this.lblPassword.TabIndex = 4;
            this.lblPassword.Text = "&Password:";
            // 
            // lblFileLocation
            // 
            this.lblFileLocation.AutoSize = true;
            this.lblFileLocation.Location = new System.Drawing.Point(12, 197);
            this.lblFileLocation.Name = "lblFileLocation";
            this.lblFileLocation.Size = new System.Drawing.Size(66, 13);
            this.lblFileLocation.TabIndex = 7;
            this.lblFileLocation.Text = "&File location:";
            // 
            // txtFileLocation
            // 
            this.txtFileLocation.Location = new System.Drawing.Point(12, 213);
            this.txtFileLocation.Name = "txtFileLocation";
            this.txtFileLocation.ReadOnly = true;
            this.txtFileLocation.Size = new System.Drawing.Size(230, 20);
            this.txtFileLocation.TabIndex = 8;
            this.TimetableConversionTips.SetToolTip(this.txtFileLocation, "Location of file where the calendar will be exported too.");
            this.txtFileLocation.TextChanged += new System.EventHandler(this.txtFileLocation_TextChanged);
            // 
            // txtUsername
            // 
            this.txtUsername.Location = new System.Drawing.Point(12, 125);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(136, 20);
            this.txtUsername.TabIndex = 3;
            this.TimetableConversionTips.SetToolTip(this.txtUsername, "Username / Student ID for MyCampus.");
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(12, 164);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(136, 20);
            this.txtPassword.TabIndex = 5;
            this.TimetableConversionTips.SetToolTip(this.txtPassword, "Password used for MyCampus.");
            // 
            // btnFileLocation
            // 
            this.btnFileLocation.Location = new System.Drawing.Point(248, 213);
            this.btnFileLocation.Name = "btnFileLocation";
            this.btnFileLocation.Size = new System.Drawing.Size(24, 20);
            this.btnFileLocation.TabIndex = 9;
            this.btnFileLocation.Text = "...";
            this.btnFileLocation.UseVisualStyleBackColor = true;
            this.btnFileLocation.Click += new System.EventHandler(this.btnFileLocation_Click);
            // 
            // tbxMain
            // 
            this.tbxMain.Enabled = false;
            this.tbxMain.Location = new System.Drawing.Point(12, 10);
            this.tbxMain.Multiline = true;
            this.tbxMain.Name = "tbxMain";
            this.tbxMain.ReadOnly = true;
            this.tbxMain.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbxMain.Size = new System.Drawing.Size(324, 96);
            this.tbxMain.TabIndex = 1;
            this.tbxMain.Text = resources.GetString("tbxMain.Text");
            this.TimetableConversionTips.SetToolTip(this.tbxMain, "Output textbox.");
            // 
            // cbxCampus
            // 
            this.cbxCampus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxCampus.FormattingEnabled = true;
            this.cbxCampus.Items.AddRange(new object[] {
            "Durham College",
            "UOIT"});
            this.cbxCampus.Location = new System.Drawing.Point(215, 124);
            this.cbxCampus.Name = "cbxCampus";
            this.cbxCampus.Size = new System.Drawing.Size(121, 21);
            this.cbxCampus.TabIndex = 7;
            this.TimetableConversionTips.SetToolTip(this.cbxCampus, "Dropdown for Campus / School to scrape from.");
            // 
            // lblCampus
            // 
            this.lblCampus.AutoSize = true;
            this.lblCampus.Location = new System.Drawing.Point(212, 108);
            this.lblCampus.Name = "lblCampus";
            this.lblCampus.Size = new System.Drawing.Size(48, 13);
            this.lblCampus.TabIndex = 6;
            this.lblCampus.Text = "&Campus:";
            // 
            // frmTemperatureConversion
            // 
            this.AcceptButton = this.btnExport;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(348, 311);
            this.Controls.Add(this.lblCampus);
            this.Controls.Add(this.cbxCampus);
            this.Controls.Add(this.tbxMain);
            this.Controls.Add(this.btnFileLocation);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.txtUsername);
            this.Controls.Add(this.txtFileLocation);
            this.Controls.Add(this.lblFileLocation);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.lblUsername);
            this.Controls.Add(this.btnExport);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmTemperatureConversion";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Timetable Converter Form";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmTemperatureConversion_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Label lblUsername;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.Label lblFileLocation;
        private System.Windows.Forms.TextBox txtFileLocation;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Button btnFileLocation;
        private System.Windows.Forms.TextBox tbxMain;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.ComboBox cbxCampus;
        private System.Windows.Forms.Label lblCampus;
        private System.Windows.Forms.ToolTip TimetableConversionTips;
    }
}

