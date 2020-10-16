using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OP970LaserEtch
{
    public partial class MyMessageBox : Form
    {
        public MyMessageBox()
        {
            InitializeComponent();
        }

        public void SetInfo(string strTitle, string strMsg)
        {
            btn_Accept.Focus();
            lb_Title.Text = strTitle;
            lb_Message.Text = strMsg;
        }

        private void btn_Accept_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;

            this.Hide();
        }
    }
}
