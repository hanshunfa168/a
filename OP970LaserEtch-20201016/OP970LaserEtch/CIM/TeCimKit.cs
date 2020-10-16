using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using ClassPriorStep;
//using ZFforVWalias;
using Funkit.Utility;

namespace LaserCode
{
    public class CimKit
    {
        //private static clsPriorStep cim = new clsPriorStep();

        ////private static clsZFforVWalias cimTime = new clsZFforVWalias();

        //string strRunMode = "Online";
        //string _strStationId;
        //string strPath;
        //string strUser;
        //string strPassWord;

        //public bool isInit = false;
        //public CimKit()
        //{
        //    _strStationId = ConfigHelper.GetAppConfig("StationID");
        //    strPath = ConfigHelper.GetAppConfig("Server");
        //    strUser = ConfigHelper.GetAppConfig("UserName");
        //    strPassWord = ConfigHelper.GetAppConfig("PSW");
        //}
        ///// <summary>
        ///// 初始化cim连接
        ///// </summary>
        ///// <param name="StationID">工位名称490A01</param>
        ///// <param name="Path2LineDBF">产线路径J:\Product\DATA\FLEX1</param>
        ///// <param name="PriorStepEnabled">状态是否更新 Yes</param>
        ///// <param name="OracleLogin">数据库连接FLEX1_USER</param>
        ///// <param name="OraclePassword">数据库连接密码flex1user</param>
        ///// <param name="RunMode"></param>
        //public bool Open(string StationID, string Path2LineDBF, string PriorStepEnabled, string OracleLogin, string OraclePassword,ref string RunMode)
        //{
        //    try
        //    {
        //        if(!isInit)
        //        {
        //            isInit = true;
        //            cim.Initialize(StationID, Path2LineDBF, PriorStepEnabled, OracleLogin, OraclePassword, "PCBs", RunMode);
        //        }                             
        //        return true;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}
        ///// <summary>
        ///// 退出
        ///// </summary>
        //public bool Close()
        //{
        //    try
        //    {
        //        if (isInit)
        //        {
        //            isInit = false;
        //            cim.CloseAllFiles();
        //        }
        //        //isInit = false;
        //        //cim.CloseAllFiles();
        //        return true;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}
        ///// <summary>
        ///// 状态检查
        ///// </summary>
        ///// <param name="sn">条码</param>
        ///// <param name="PSPassed">目前设置为false</param>
        ///// <param name="psinfo">返回产品状态信息</param>
        ///// <returns>true检查成功，可以生产，false检查失败不可以生产</returns>
        //public bool GetPriorStep(string sn, ref bool PSPassed, ref string psinfo)
        //{
        //    try
        //    {
        //        if (Open(_strStationId, strPath, "Yes", strUser, strPassWord, ref strRunMode))
        //        {
        //            bool status = cim.GetPriorStep(sn, PSPassed, psinfo);
        //            return status;
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //        //bool status = cim.GetPriorStep(sn, PSPassed, psinfo);
        //        //return status;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //    finally
        //    {
        //        Close();
        //    }
        //}
        ///// <summary>
        ///// 状态更新
        ///// </summary>
        ///// <param name="TRWserNr">产品序列号码</param>
        ///// <param name="NewStatus">产品加工结果，9表示ok，0表示ng</param>
        ///// <param name="Breakaway">vbNullString</param>
        ///// <returns>true更新成功，false更新失败</returns>
        //public bool SetPriorStep(string TRWserNr, string NewStatus, ref string Breakaway)
        //{
        //    try
        //    {
        //        if (Open(_strStationId, strPath, "Yes", strUser, strPassWord, ref strRunMode))
        //        {
        //            bool status = cim.SetPriorStep(TRWserNr, NewStatus, Breakaway);
        //            return status;
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //        //bool status = cim.SetPriorStep(TRWserNr, NewStatus, Breakaway);
        //        //return status;

        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //    finally
        //    {
        //        Close();
        //    }
        //}

        //public bool SetStationID(string strStationID)
        //{
        //    try
        //    {
        //        cim.setStnID(strStationID);
        //        return true;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        /////<summary>
        ///// 获取时间
        ///// </summary>
        /////<param name = trwSn>产品SN</param>
        ///// <return>返回三个参数,以分号分隔</return>
        ///// VWSN: 需要刻录的VW序列号
        ///// DATE:返回需要刻录的日期
        ///// OK||NOK:如果正常，这位为OK；如果不正常，这位是NOK，机器不允许刻录
        ///// 

        //public bool GetSysTime(string trwSn,out string[] strRlt)
        //{
        //    try
        //    {
        //        string strResult = "";
        //        //bool status = cimTime.GetVWalias(trwSn,ref strResult);
        //        bool status = true;
        //        strRlt = strResult.Split(';');

        //        return status;
        //    }
        //    catch
        //    {
        //        strRlt = new string[] { "", "" };
        //        return false;
        //    }
        //}


    }
}
