using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using LaserCode.Models;

namespace OP970LaserEtch
{
    public partial class Login : Form
    {
        public bool isSetUp = false;
        public Login()
        {
            InitializeComponent();

            this.KeyPreview = true;

        }

        #region 无边框窗体移动
        private bool isMouseDown = false;
        private Point FormLocation;
        private Point mouseOffset;
        private void Login_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isMouseDown = true;
                FormLocation = this.Location;
                mouseOffset = Control.MousePosition;
            }
        }

        private void Login_MouseUp(object sender, MouseEventArgs e)
        {
            isMouseDown = false;
        }

        private void Login_MouseMove(object sender, MouseEventArgs e)
        {
            int _x = 0;
            int _y = 0;
            if (isMouseDown)
            {
                Point pt = Control.MousePosition;
                _x = mouseOffset.X - pt.X;
                _y = mouseOffset.Y - pt.Y;

                this.Location = new Point(FormLocation.X - _x, FormLocation.Y - _y);
            }

        }

        #endregion
        #region 设置圆角矩形
        public void SetWindowRegion(int width, int height)
        {
            System.Drawing.Drawing2D.GraphicsPath FormPath;
            FormPath = new System.Drawing.Drawing2D.GraphicsPath();
            Rectangle rect = new Rectangle(0, 0, width, height);
            FormPath = GetRoundedRectPath(rect, 8);
            this.Region = new Region(FormPath);
        }

        private GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            int diameter = radius;
            Rectangle arcRect = new Rectangle(rect.Location, new Size(diameter, diameter));
            GraphicsPath path = new GraphicsPath();
            path.AddArc(arcRect, 180, 90);
            arcRect.X = rect.Right - diameter;
            path.AddArc(arcRect, 270, 90);
            arcRect.Y = rect.Bottom - diameter;
            path.AddArc(arcRect, 0, 90);
            arcRect.X = rect.Left;
            path.AddArc(arcRect, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void Type(Control sender, int p_1, double p_2)
        {
            GraphicsPath oPath = new GraphicsPath();
            oPath.AddClosedCurve(
                new Point[] {
            new Point(0, sender.Height / p_1),
            new Point(sender.Width / p_1, 0), 
            new Point(sender.Width - sender.Width / p_1, 0), 
            new Point(sender.Width, sender.Height / p_1),
            new Point(sender.Width, sender.Height - sender.Height / p_1), 
            new Point(sender.Width - sender.Width / p_1, sender.Height), 
            new Point(sender.Width / p_1, sender.Height),
            new Point(0, sender.Height - sender.Height / p_1) },
                (float)p_2);
            sender.Region = new Region(oPath);

        }

        private void Login_Paint(object sender, PaintEventArgs e)
        {
            Type(this, 20, 0.1);
        }
        #endregion

        private void Btn_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
        //当前用户
        private tb_user user = new tb_user();

        public tb_user CurUser
        {
            get
            {
                return user;
            }
        }

        private void Btn_Login_Click(object sender, EventArgs e)
        {
            progressBar.Value = 20;
            lbLoginMsg.Text = "账号密码校验中...";
            lbLoginMsg.Update();
            user.level = "";

            if ("" == cbUserName.Text)
            {
                //  MessageBox.Show("用户名不能为空！","温馨提示",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                lbLoginMsg.Text = "用户名不能为空！";
                lbLoginMsg.Update();
                progressBar.Value = 0;
                this.DialogResult = DialogResult.None;

                return;
            }
            if ("" == tbPw.Text)
            {
                // MessageBox.Show("密码不能为空！", "温馨提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                lbLoginMsg.Text = "密码不能为空！";
                lbLoginMsg.Update();
                progressBar.Value = 0;
                this.DialogResult = DialogResult.None;

                return;
            }

            user.user = cbUserName.Text;
            user.password = tbPw.Text;

            tb_user userRlt = LaserCode.BLL.BllUser.SelectData().Where(us => us.user == user.user).Where(us => us.password == user.password).FirstOrDefault();

            if (userRlt == null)
            {
                lbLoginMsg.Text = "密码或用户名错误，请重新输入";
                lbLoginMsg.Update();
                cbUserName.Text = "";
                tbPw.Text = "";
                this.DialogResult = DialogResult.None;
            }
            else
            {

                user = userRlt;
                cbUserName.Text = "";
                tbPw.Text = "";
                //isSetUp = true;
                lbLoginMsg.Text = "加载主窗体...";
                lbLoginMsg.Update();

                this.Hide();
                this.DialogResult = DialogResult.OK;
            }
        }

        private void Login_FormClosed(object sender, FormClosedEventArgs e)
        {
            //Environment.Exit(0);
        }
    }
}
