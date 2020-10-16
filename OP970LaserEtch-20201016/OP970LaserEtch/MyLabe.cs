using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace OP970LaserEtch
{
    public partial class MyLabe : UserControl
    {
        public MyLabe()
        {
            InitializeComponent();
        }

        private Color _clCenter = Color.Lime;
        private Color _clSurround = Color.LightGray;

        public void SetColor(Color center, Color surround)
        {
            _clCenter = center;
            _clSurround = surround;
            label1.Invalidate();
        }

        public void SetText(string strText, bool bOK)
        {
            _clCenter = bOK ? Color.Lime : Color.OrangeRed;
            _clSurround = bOK ? Color.LightGray : Color.LightPink;
            label1.Text = strText;
            label1.Invalidate();
        }
        public void SetStatus(bool bOK)
        {
            _clCenter = bOK ? Color.Lime : Color.OrangeRed;
            _clSurround = bOK ? Color.LightGray : Color.LightPink;
            label1.Invalidate();
        }
        private void label1_Paint(object sender, PaintEventArgs e)
        {
            DrawRoundRect(e.Graphics, label1);

            DrawString(e.Graphics, label1);
        }

        private void DrawRoundRect(Graphics graphics, Label label)
        {
            float X = float.Parse(label.Width.ToString()) - 1;
            float Y = float.Parse(label.Height.ToString()) - 1;
            PointF[] points = {
                new PointF(4,0),
                new PointF(X-4,0),
                new PointF(X-2,2),
                new PointF(X,4),
                new PointF(X,Y-4),
                new PointF(X-2,Y-2),
                new PointF(X-4,Y),
                new PointF(4,Y),
                new PointF(2,Y-2),
                new PointF(0,Y-4),
                new PointF(0,4),
                new PointF(2,2)
        };

            GraphicsPath path = new GraphicsPath();
            path.AddLines(points);
            Pen pen = new Pen(Color.FromArgb(150, Color.Gray), 1) { DashStyle = DashStyle.Solid };
            graphics.DrawPath(pen, path);
            PathGradientBrush p = new PathGradientBrush(path);

            p.CenterColor = _clCenter;
            Color[] color = { _clSurround };

            p.SurroundColors = color;
            graphics.FillPath(p, path);
        }


        private void DrawString(Graphics graphics, Label label)
        {
            Font font = new Font("宋体", 22.0f, FontStyle.Regular);
            StringFormat stringFormat = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            Brush brush = new SolidBrush(Color.Black);
            RectangleF rect = new RectangleF(0, 0, label.Width, label.Height);
            graphics.DrawString(label.Text, font, brush, rect, stringFormat);
        }
    }
}
