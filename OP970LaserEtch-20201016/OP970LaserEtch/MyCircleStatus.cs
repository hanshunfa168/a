using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OP970LaserEtch
{
    public partial class MyCircleStatus : UserControl
    {
        public MyCircleStatus()
        {
            InitializeComponent();
        }

        private Color _colorRed = Color.Red;
        private Color _colorGreen = Color.Green;

        private Color _curColor = Color.Blue;

        private string _strText = "Status";
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics gra = e.Graphics;
            gra.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Brush bush = new SolidBrush(_curColor);
            gra.FillEllipse(bush, 5, 5, this.Width - 10, this.Height - 10);

            Font myFont = new Font("宋体", 12, FontStyle.Bold);
            bush = new SolidBrush(Color.Black);//填充的颜色

            StringFormat sf = new StringFormat();

            sf.Alignment = StringAlignment.Center;

            sf.LineAlignment = StringAlignment.Center;

            e.Graphics.DrawString(_strText, myFont, bush, this.ClientRectangle, sf);
        }

        public void SetStatus(int nStatus, string strText)
        {
            _strText = strText;
            _curColor = nStatus == 0 ? Color.White : (nStatus == 1 ? _colorGreen : _colorRed);
            pictureBox1.Invalidate();
        }
    }
}
