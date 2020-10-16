using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OP970LaserEtch;

namespace LaserCode.Utility
{
    public class MyMsgBox
    {
        private static MyMessageBox mmb = new MyMessageBox();

        public static DialogResult Show(string strMsg = "Thank you to use",string strTitle = "MyMessageBox")
        {
            mmb.SetInfo(strTitle, strMsg);
            return mmb.ShowDialog();
        }
    }
}
