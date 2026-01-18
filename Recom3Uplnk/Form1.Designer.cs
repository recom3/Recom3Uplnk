namespace Recom3Uplnk
{
    partial class Form1
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.btLogin = new System.Windows.Forms.Button();
            this.tbEmail = new System.Windows.Forms.TextBox();
            this.tbPassword = new System.Windows.Forms.TextBox();
            this.lbEmail = new System.Windows.Forms.Label();
            this.lbPassword = new System.Windows.Forms.Label();
            this.tbMsg = new System.Windows.Forms.TextBox();
            this.pbGoggles = new System.Windows.Forms.PictureBox();
            this.lbOwner = new System.Windows.Forms.Label();
            this.lbReconOS = new System.Windows.Forms.Label();
            this.btHelp = new System.Windows.Forms.Button();
            this.ilHelp = new System.Windows.Forms.ImageList(this.components);
            this.btHome = new System.Windows.Forms.Button();
            this.btLogout = new System.Windows.Forms.Button();
            this.btRetry = new System.Windows.Forms.Button();
            this.btSync = new System.Windows.Forms.Button();
            this.lbMsg = new System.Windows.Forms.Label();
            this.lbMapsVer = new System.Windows.Forms.Label();
            this.lbSerial = new System.Windows.Forms.Label();
            this.pbSnow2 = new System.Windows.Forms.PictureBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.btMap = new System.Windows.Forms.Button();
            this.progressBar2 = new System.Windows.Forms.ProgressBar();
            ((System.ComponentModel.ISupportInitialize)(this.pbGoggles)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbSnow2)).BeginInit();
            this.SuspendLayout();
            // 
            // btLogin
            // 
            this.btLogin.Location = new System.Drawing.Point(245, 182);
            this.btLogin.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btLogin.Name = "btLogin";
            this.btLogin.Size = new System.Drawing.Size(56, 19);
            this.btLogin.TabIndex = 0;
            this.btLogin.Text = "Login";
            this.btLogin.UseVisualStyleBackColor = true;
            this.btLogin.Click += new System.EventHandler(this.btLogin_Click);
            // 
            // tbEmail
            // 
            this.tbEmail.Location = new System.Drawing.Point(84, 98);
            this.tbEmail.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tbEmail.Name = "tbEmail";
            this.tbEmail.Size = new System.Drawing.Size(218, 20);
            this.tbEmail.TabIndex = 1;
            // 
            // tbPassword
            // 
            this.tbPassword.Location = new System.Drawing.Point(85, 152);
            this.tbPassword.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tbPassword.Name = "tbPassword";
            this.tbPassword.PasswordChar = '*';
            this.tbPassword.Size = new System.Drawing.Size(218, 20);
            this.tbPassword.TabIndex = 2;
            // 
            // lbEmail
            // 
            this.lbEmail.AutoSize = true;
            this.lbEmail.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.lbEmail.Location = new System.Drawing.Point(82, 67);
            this.lbEmail.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbEmail.Name = "lbEmail";
            this.lbEmail.Size = new System.Drawing.Size(35, 13);
            this.lbEmail.TabIndex = 3;
            this.lbEmail.Text = "Email:";
            // 
            // lbPassword
            // 
            this.lbPassword.AutoSize = true;
            this.lbPassword.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.lbPassword.Location = new System.Drawing.Point(82, 128);
            this.lbPassword.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbPassword.Name = "lbPassword";
            this.lbPassword.Size = new System.Drawing.Size(56, 13);
            this.lbPassword.TabIndex = 4;
            this.lbPassword.Text = "Password:";
            // 
            // tbMsg
            // 
            this.tbMsg.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(31)))), ((int)(((byte)(31)))));
            this.tbMsg.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbMsg.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.tbMsg.Enabled = false;
            this.tbMsg.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.tbMsg.Location = new System.Drawing.Point(81, 254);
            this.tbMsg.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tbMsg.Name = "tbMsg";
            this.tbMsg.ReadOnly = true;
            this.tbMsg.Size = new System.Drawing.Size(433, 13);
            this.tbMsg.TabIndex = 5;
            // 
            // pbGoggles
            // 
            this.pbGoggles.Image = global::Recom3Uplnk.Properties.Resources.snow_goggle;
            this.pbGoggles.Location = new System.Drawing.Point(316, 67);
            this.pbGoggles.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.pbGoggles.Name = "pbGoggles";
            this.pbGoggles.Size = new System.Drawing.Size(198, 206);
            this.pbGoggles.TabIndex = 6;
            this.pbGoggles.TabStop = false;
            this.pbGoggles.Visible = false;
            // 
            // lbOwner
            // 
            this.lbOwner.AutoSize = true;
            this.lbOwner.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.lbOwner.Location = new System.Drawing.Point(532, 107);
            this.lbOwner.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbOwner.Name = "lbOwner";
            this.lbOwner.Size = new System.Drawing.Size(41, 13);
            this.lbOwner.TabIndex = 7;
            this.lbOwner.Text = "Owner;";
            this.lbOwner.Visible = false;
            // 
            // lbReconOS
            // 
            this.lbReconOS.AutoSize = true;
            this.lbReconOS.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.lbReconOS.Location = new System.Drawing.Point(532, 149);
            this.lbReconOS.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbReconOS.Name = "lbReconOS";
            this.lbReconOS.Size = new System.Drawing.Size(57, 13);
            this.lbReconOS.TabIndex = 8;
            this.lbReconOS.Text = "ReconOS:";
            this.lbReconOS.Visible = false;
            // 
            // btHelp
            // 
            this.btHelp.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(31)))), ((int)(((byte)(31)))));
            this.btHelp.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btHelp.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btHelp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btHelp.ForeColor = System.Drawing.Color.Transparent;
            this.btHelp.Image = global::Recom3Uplnk.Properties.Resources.help_button_up;
            this.btHelp.Location = new System.Drawing.Point(51, 347);
            this.btHelp.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btHelp.Name = "btHelp";
            this.btHelp.Size = new System.Drawing.Size(36, 39);
            this.btHelp.TabIndex = 9;
            this.btHelp.UseVisualStyleBackColor = true;
            this.btHelp.Click += new System.EventHandler(this.btHelp_Click);
            this.btHelp.MouseLeave += new System.EventHandler(this.btHelp_MouseLeave);
            this.btHelp.MouseHover += new System.EventHandler(this.btHelp_MouseHover);
            // 
            // ilHelp
            // 
            this.ilHelp.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilHelp.ImageStream")));
            this.ilHelp.TransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(31)))), ((int)(((byte)(31)))));
            this.ilHelp.Images.SetKeyName(0, "help_button_up.png");
            this.ilHelp.Images.SetKeyName(1, "help_button_hover.png");
            // 
            // btHome
            // 
            this.btHome.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(31)))), ((int)(((byte)(31)))));
            this.btHome.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btHome.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btHome.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btHome.ForeColor = System.Drawing.Color.Transparent;
            this.btHome.Image = global::Recom3Uplnk.Properties.Resources.engage_home_button_up;
            this.btHome.Location = new System.Drawing.Point(92, 347);
            this.btHome.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btHome.Name = "btHome";
            this.btHome.Size = new System.Drawing.Size(36, 39);
            this.btHome.TabIndex = 10;
            this.btHome.UseVisualStyleBackColor = true;
            this.btHome.Click += new System.EventHandler(this.btHome_Click);
            // 
            // btLogout
            // 
            this.btLogout.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(31)))), ((int)(((byte)(31)))));
            this.btLogout.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btLogout.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btLogout.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btLogout.ForeColor = System.Drawing.Color.Transparent;
            this.btLogout.Image = global::Recom3Uplnk.Properties.Resources.logout_button_up;
            this.btLogout.Location = new System.Drawing.Point(132, 347);
            this.btLogout.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btLogout.Name = "btLogout";
            this.btLogout.Size = new System.Drawing.Size(36, 39);
            this.btLogout.TabIndex = 11;
            this.btLogout.UseVisualStyleBackColor = true;
            this.btLogout.Click += new System.EventHandler(this.btLogout_Click);
            // 
            // btRetry
            // 
            this.btRetry.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(31)))), ((int)(((byte)(31)))));
            this.btRetry.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btRetry.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btRetry.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btRetry.ForeColor = System.Drawing.Color.Transparent;
            this.btRetry.Image = global::Recom3Uplnk.Properties.Resources.try_again_btn;
            this.btRetry.Location = new System.Drawing.Point(552, 347);
            this.btRetry.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btRetry.Name = "btRetry";
            this.btRetry.Size = new System.Drawing.Size(116, 39);
            this.btRetry.TabIndex = 12;
            this.btRetry.UseVisualStyleBackColor = true;
            this.btRetry.Visible = false;
            this.btRetry.Click += new System.EventHandler(this.btRetry_Click);
            // 
            // btSync
            // 
            this.btSync.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(31)))), ((int)(((byte)(31)))));
            this.btSync.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btSync.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btSync.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btSync.ForeColor = System.Drawing.Color.Transparent;
            this.btSync.Image = global::Recom3Uplnk.Properties.Resources.sync_btn;
            this.btSync.Location = new System.Drawing.Point(643, 32);
            this.btSync.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btSync.Name = "btSync";
            this.btSync.Size = new System.Drawing.Size(76, 39);
            this.btSync.TabIndex = 13;
            this.btSync.UseVisualStyleBackColor = true;
            this.btSync.Visible = false;
            this.btSync.Click += new System.EventHandler(this.btSync_Click);
            // 
            // lbMsg
            // 
            this.lbMsg.AutoSize = true;
            this.lbMsg.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.lbMsg.Location = new System.Drawing.Point(89, 269);
            this.lbMsg.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbMsg.Name = "lbMsg";
            this.lbMsg.Size = new System.Drawing.Size(0, 13);
            this.lbMsg.TabIndex = 14;
            // 
            // lbMapsVer
            // 
            this.lbMapsVer.AutoSize = true;
            this.lbMapsVer.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.lbMapsVer.Location = new System.Drawing.Point(532, 169);
            this.lbMapsVer.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbMapsVer.Name = "lbMapsVer";
            this.lbMapsVer.Size = new System.Drawing.Size(54, 13);
            this.lbMapsVer.TabIndex = 15;
            this.lbMapsVer.Text = "Maps ver:";
            this.lbMapsVer.Visible = false;
            // 
            // lbSerial
            // 
            this.lbSerial.AutoSize = true;
            this.lbSerial.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.lbSerial.Location = new System.Drawing.Point(532, 128);
            this.lbSerial.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lbSerial.Name = "lbSerial";
            this.lbSerial.Size = new System.Drawing.Size(76, 13);
            this.lbSerial.TabIndex = 16;
            this.lbSerial.Text = "Serial Number:";
            this.lbSerial.Visible = false;
            // 
            // pbSnow2
            // 
            this.pbSnow2.Image = global::Recom3Uplnk.Properties.Resources.anow2_logo;
            this.pbSnow2.Location = new System.Drawing.Point(518, 21);
            this.pbSnow2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.pbSnow2.Name = "pbSnow2";
            this.pbSnow2.Size = new System.Drawing.Size(129, 68);
            this.pbSnow2.TabIndex = 17;
            this.pbSnow2.TabStop = false;
            this.pbSnow2.Visible = false;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(132, 284);
            this.progressBar1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(476, 19);
            this.progressBar1.TabIndex = 18;
            this.progressBar1.Visible = false;
            // 
            // btMap
            // 
            this.btMap.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(31)))), ((int)(((byte)(31)))));
            this.btMap.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btMap.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btMap.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btMap.ForeColor = System.Drawing.Color.Transparent;
            this.btMap.Image = global::Recom3Uplnk.Properties.Resources.map_btn;
            this.btMap.Location = new System.Drawing.Point(518, 185);
            this.btMap.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btMap.Name = "btMap";
            this.btMap.Size = new System.Drawing.Size(76, 39);
            this.btMap.TabIndex = 19;
            this.btMap.UseVisualStyleBackColor = true;
            this.btMap.Click += new System.EventHandler(this.btMap_Click);
            // 
            // progressBar2
            // 
            this.progressBar2.Location = new System.Drawing.Point(132, 323);
            this.progressBar2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.progressBar2.Name = "progressBar2";
            this.progressBar2.Size = new System.Drawing.Size(476, 19);
            this.progressBar2.TabIndex = 20;
            this.progressBar2.Visible = false;
            // 
            // Form1
            // 
            this.AcceptButton = this.btLogin;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(31)))), ((int)(((byte)(31)))));
            this.ClientSize = new System.Drawing.Size(759, 413);
            this.Controls.Add(this.progressBar2);
            this.Controls.Add(this.btMap);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.pbSnow2);
            this.Controls.Add(this.lbSerial);
            this.Controls.Add(this.lbMapsVer);
            this.Controls.Add(this.lbMsg);
            this.Controls.Add(this.btSync);
            this.Controls.Add(this.btRetry);
            this.Controls.Add(this.btLogout);
            this.Controls.Add(this.btHome);
            this.Controls.Add(this.btHelp);
            this.Controls.Add(this.lbReconOS);
            this.Controls.Add(this.lbOwner);
            this.Controls.Add(this.pbGoggles);
            this.Controls.Add(this.tbMsg);
            this.Controls.Add(this.lbPassword);
            this.Controls.Add(this.lbEmail);
            this.Controls.Add(this.tbPassword);
            this.Controls.Add(this.tbEmail);
            this.Controls.Add(this.btLogin);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.pbGoggles)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbSnow2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btLogin;
        private System.Windows.Forms.TextBox tbEmail;
        private System.Windows.Forms.TextBox tbPassword;
        private System.Windows.Forms.Label lbEmail;
        private System.Windows.Forms.Label lbPassword;
        private System.Windows.Forms.TextBox tbMsg;
        private System.Windows.Forms.PictureBox pbGoggles;
        private System.Windows.Forms.Label lbOwner;
        private System.Windows.Forms.Label lbReconOS;
        private System.Windows.Forms.Button btHelp;
        private System.Windows.Forms.ImageList ilHelp;
        private System.Windows.Forms.Button btHome;
        private System.Windows.Forms.Button btLogout;
        private System.Windows.Forms.Button btRetry;
        private System.Windows.Forms.Button btSync;
        private System.Windows.Forms.Label lbMsg;
        private System.Windows.Forms.Label lbMapsVer;
        private System.Windows.Forms.Label lbSerial;
        private System.Windows.Forms.PictureBox pbSnow2;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button btMap;
        private System.Windows.Forms.ProgressBar progressBar2;
    }
}

