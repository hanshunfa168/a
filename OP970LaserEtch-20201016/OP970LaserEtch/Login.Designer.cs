namespace OP970LaserEtch
{
    partial class Login
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
            this.cbUserName = new System.Windows.Forms.ComboBox();
            this.lbLoginMsg = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.tbPw = new System.Windows.Forms.TextBox();
            this.Btn_Cancel = new System.Windows.Forms.Button();
            this.Btn_Login = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cbUserName
            // 
            this.cbUserName.Font = new System.Drawing.Font("宋体", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.cbUserName.FormattingEnabled = true;
            this.cbUserName.Items.AddRange(new object[] {
            "admin",
            "operator"});
            this.cbUserName.Location = new System.Drawing.Point(270, 195);
            this.cbUserName.Name = "cbUserName";
            this.cbUserName.Size = new System.Drawing.Size(256, 27);
            this.cbUserName.TabIndex = 13;
            this.cbUserName.Text = "operator";
            // 
            // lbLoginMsg
            // 
            this.lbLoginMsg.AutoSize = true;
            this.lbLoginMsg.BackColor = System.Drawing.Color.Transparent;
            this.lbLoginMsg.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbLoginMsg.ForeColor = System.Drawing.Color.Red;
            this.lbLoginMsg.Location = new System.Drawing.Point(304, 387);
            this.lbLoginMsg.Name = "lbLoginMsg";
            this.lbLoginMsg.Size = new System.Drawing.Size(127, 21);
            this.lbLoginMsg.TabIndex = 12;
            this.lbLoginMsg.Text = "请先登录...";
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(198, 443);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(361, 18);
            this.progressBar.TabIndex = 11;
            // 
            // tbPw
            // 
            this.tbPw.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbPw.Font = new System.Drawing.Font("宋体", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tbPw.Location = new System.Drawing.Point(270, 256);
            this.tbPw.Name = "tbPw";
            this.tbPw.PasswordChar = '*';
            this.tbPw.Size = new System.Drawing.Size(256, 24);
            this.tbPw.TabIndex = 10;
            this.tbPw.UseSystemPasswordChar = true;
            // 
            // Btn_Cancel
            // 
            this.Btn_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Btn_Cancel.Location = new System.Drawing.Point(407, 326);
            this.Btn_Cancel.Name = "Btn_Cancel";
            this.Btn_Cancel.Size = new System.Drawing.Size(153, 44);
            this.Btn_Cancel.TabIndex = 9;
            this.Btn_Cancel.Text = "退出";
            this.Btn_Cancel.UseVisualStyleBackColor = true;
            this.Btn_Cancel.Click += new System.EventHandler(this.Btn_Cancel_Click);
            // 
            // Btn_Login
            // 
            this.Btn_Login.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Btn_Login.Location = new System.Drawing.Point(198, 326);
            this.Btn_Login.Name = "Btn_Login";
            this.Btn_Login.Size = new System.Drawing.Size(153, 44);
            this.Btn_Login.TabIndex = 8;
            this.Btn_Login.Text = "登录";
            this.Btn_Login.UseVisualStyleBackColor = true;
            this.Btn_Login.Click += new System.EventHandler(this.Btn_Login_Click);
            // 
            // Login
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::OP970LaserEtch.Properties.Resources.Login_BK;
            this.ClientSize = new System.Drawing.Size(706, 503);
            this.Controls.Add(this.cbUserName);
            this.Controls.Add(this.lbLoginMsg);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.tbPw);
            this.Controls.Add(this.Btn_Cancel);
            this.Controls.Add(this.Btn_Login);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Login";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Login";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Login_FormClosed);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Login_Paint);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Login_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Login_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Login_MouseUp);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbUserName;
        public System.Windows.Forms.Label lbLoginMsg;
        public System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.TextBox tbPw;
        private System.Windows.Forms.Button Btn_Cancel;
        private System.Windows.Forms.Button Btn_Login;

    }
}