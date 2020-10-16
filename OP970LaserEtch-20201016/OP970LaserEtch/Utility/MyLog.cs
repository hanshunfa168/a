using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HslCommunication;
using HslCommunication.LogNet;

namespace LaserCode.Utility
{
    public class MyLog
    {
        public enum MessageType
        {
            DEBUG = 0,
            ERROR = 1,
            FATAL = 2,
            INFO = 3,
            WARN = 4,
            EXCEPTION = 5,
            NONE = 6
        }
        //按单文件
        //ILogNet logNet = new LogNetSingle(Application.StartupPath + "\\Logs\\123.txt");
        //按大小多文件
        //ILogNet logNet = new LogNetFileSize(Application.StartupPath + "\\customer1", 2 * 1024 * 1024);
        //按多文件时间
        //ILogNet logNet = new LogNetDateTime(Application.StartupPath + "\\customer2", GenerateMode.ByEveryHour);//按每小时
        //ILogNet logNet = new LogNetDateTime(Application.StartupPath + "\\customer2", GenerateMode.ByEveryDay);//按每天
        private static ILogNet logNet = new LogNetDateTime(Application.StartupPath + "\\Logs", GenerateMode.ByEveryMonth);//按每月
        //ILogNet logNet = new LogNetDateTime(Application.StartupPath + "\\customer2", GenerateMode.ByEverySeason);//按每季度
        //ILogNet logNet = new LogNetDateTime(Application.StartupPath + "\\customer2", GenerateMode.ByEveryYear);//按每年
        public static void SetMessageDegree(int nDegree)
        {
            switch (nDegree)
            {
                case 0:
                    logNet.SetMessageDegree(HslMessageDegree.DEBUG);//所有等级存储
                    break;
                case 1:
                    logNet.SetMessageDegree(HslMessageDegree.INFO);//除DEBUG外，都存储
                    break;
                case 2:
                    logNet.SetMessageDegree(HslMessageDegree.WARN);//除DEBUG和INFO外，都存储
                    break;
                case 3:
                    logNet.SetMessageDegree(HslMessageDegree.ERROR);//只存储ERROR和FATAL
                    break;
                case 4:
                    logNet.SetMessageDegree(HslMessageDegree.FATAL);//只存储FATAL
                    break;
                case 5:
                    logNet.SetMessageDegree(HslMessageDegree.None);//不存储任何等级
                    break;
            }
        }

        public static void ShowLogView()
        {
            // 日志查看器
            using (FormLogNetView form = new FormLogNetView())
            {
                form.ShowDialog();
            }
        }

        public static void WriteMsg(MessageType degree, object obj, string strMsg, string strMark = "", Exception ex = null)
        {

            string strKey = (obj != null ? obj.GetType().FullName : "") + (string.IsNullOrEmpty(strMark) ? "" : "  Mark:" + strMark);

            switch (degree)
            {
                case MessageType.DEBUG:
                    logNet.WriteDebug(strKey, strMsg);
                    break;
                case MessageType.ERROR:
                    logNet.WriteError(strKey, strMsg);
                    break;
                case MessageType.FATAL:
                    logNet.WriteFatal(strKey, strMsg);
                    break;
                case MessageType.INFO:
                    logNet.WriteInfo(strKey, strMsg);
                    break;
                case MessageType.NONE:
                    logNet.WriteDescrition(strMsg);
                    break;
                case MessageType.WARN:
                    logNet.WriteWarn(strKey, strMsg);
                    break;
                case MessageType.EXCEPTION:
                    logNet.WriteException(strKey, ex);
                    break;
            }

        }
    }
}
