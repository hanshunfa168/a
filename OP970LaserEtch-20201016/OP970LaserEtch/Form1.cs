using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MetroFramework.Forms;
using LaserCode;
using System.Threading;
using System.Diagnostics;
using LaserCode.Models;
using Funkit.Utility;
using TopaKit;
using LaserCode.Utility;
using LaserCode.BLL;
using System.Net.Sockets;
using System.Net;
using System.Xml;
using OP970LaserEtch.ServiceReference1;
using HslCommunication.Profinet.AllenBradley;
using HslCommunication;

namespace OP970LaserEtch
{
    public partial class Form1 : MetroForm
    {
        private Login curLogin = new Login();
        private ManualResetEvent _workTerminated = null;
        private Thread _workThread = null;

        //private MDX1000 _laser = null;
        private MDX1000COM _laser = null;

        //private CimKit _cimConnect = null;

        Stopwatch stopWatch = new Stopwatch();

        private int _nCodeOneLength = 0;
        private string _strCodeOne = string.Empty;
        private string _strTime = string.Empty;
        private string[] _strRlt = null;
        private string CimUnloadMsg = "";

        private int _nCodeTwoLength = 0;
        private string _strCodeTwo = string.Empty;

        private int _nPcWritePlc = 0;
        private int _nCurPcWritePlc = 0;
        private int _nPlcWritePc = 0;
        private bool[] _bTriggerRise = null;

        //asyc data read control
        private bool[] _bTriggerScanerCodeCompleted = null;
        private bool[] _bReadScanerCodeCompleted = null;

        private bool _bTriggerStartLaserCheck = false;
        private bool _bLaserStatusReady = false;
        private bool _bLaserStatusResult = false;

        //recipe config
        private List<tb_key> _listKey = null;
        private List<tb_recipe> _listRecipe = null;
        private List<tb_user> _listUser = null;

        private List<recipeinfo> _listInfo = null;

        private tb_recipe _currentRecipe = new tb_recipe();

        //private DgvDetail _recipeDetial = null;

        private bool _bLaserError = false;
        private bool _bCIMByPass = false;
        private bool _bReWork = false;
        private bool _bRecipeState = false;
        private bool _bDeviceIsRunning = false;

        private bool _bLoadRlt = false;
        private bool isRead = false;

        private string _strSnValue = string.Empty;
        string CimInfo = "";

        int okCount = 0;
        int ngCount = 0;

        public Form1()
        {
            InitializeComponent();
            metroTabControl1.SelectedIndex = 0;
            cmbType.SelectedIndex = 0;
            Control.CheckForIllegalCrossThreadCalls = false;

            Initialize();

            okCount = int.Parse(ConfigHelper.GetAppConfig("OkNum"));
            ngCount = int.Parse(ConfigHelper.GetAppConfig("NgNum"));

            timer1.Start();
        }

        private void Initialize()
        {
            InitializeControl();
            InitializeData();
            InitializeDevice();
        }

        private void InitializeControl()
        {
            mylabel_station.SetText(ConfigHelper.GetAppConfig("StationID"), true);

            _listInfo = new List<recipeinfo>();

            //_recipeDetial = new DgvDetail(dgv_detail);
            //dgv_detail.ReadOnly = true;
            //dgv_detail.AutoGenerateColumns = false;

            status_plc.SetStatus(0, "PLC");
            status_laser.SetStatus(0, "Laser");
            status_load_result.SetStatus(0, "LoadR");
            status_CIM.SetStatus(0, "CIM");
            status_unload_result.SetStatus(0, "UnLoadR");

            myLabel_Scan1.SetText("Load Code & Result", true);
            myLabel_Scan2.SetText("Check Laser Code", true);
            myLabel_UnLoad.SetText("UnLoad Code & Result", true);

            //ShowWorkMessage("InitializeControl");

            dgv_key.tsb_add.Click += key_tsb_add_Click;
            dgv_key.tsb_delete.Click += key_tsb_delete_Click;
            dgv_key.tsb_apply.Click += key_tsb_apply_Click;
            dgv_key.tsb_refresh.Click += key_tsb_refresh_Click;
            dgv_key.InitializeGraph(new string[] { "id", "key_name" }, new string[] { "ID", "关键字名称" }, new bool[] { true, false });
            dgv_key.SetContextMenu(contextMenuStrip1);

            dgv_recipe.tsb_add.Click += recipe_tsb_add_Click;
            dgv_recipe.tsb_delete.Click += recipe_tsb_delete_Click;
            dgv_recipe.tsb_apply.Click += recipe_tsb_apply_Click;
            dgv_recipe.tsb_refresh.Click += recipe_tsb_refresh_Click;
            dgv_recipe.InitializeGraph(new string[] { "id", "recipe_name", "laser_pro", "sn_key" }, new string[] { "ID", "配方名", "LaserID", "条码Key" }, new bool[] { true, false, false, false });
            dgv_recipe.dgv.SelectionChanged += recipe_dgv_SelectionChanged;

            dgv_recipe_info.tsb_add.Click += info_tsb_add_Click;
            dgv_recipe_info.tsb_delete.Click += info_tsb_delete_Click;
            dgv_recipe_info.tsb_apply.Click += info_tsb_apply_Click;
            dgv_recipe_info.tsb_refresh.Click += info_tsb_refresh_Click;
            dgv_recipe_info.InitializeGraph(new string[] { "nID", "nLaserVar", "strTemplate", "strData" }, new string[] { "ID", "Laser变量号", "模板内容", "刻印数据" }, new bool[] { true, false, true, true });
            dgv_recipe_info.dgv.SelectionChanged += info_dgv_SelectionChanged;

            dgv_user.tsb_add.Click += user_tsb_add_Click;
            dgv_user.tsb_delete.Click += user_tsb_delete_Click;
            dgv_user.tsb_apply.Click += user_tsb_apply_Click;
            dgv_user.tsb_refresh.Click += user_tsb_refresh_Click;
            //dgv_user.InitializeGraph(new string[] { "id", "user","password", "level" }, new string[] { "ID", "用户名","密码", "操作权限" }, new bool[] { true, false, false,false });
            dgv_user.InitializeGraph(new string[] { "id", "user", "level" }, new string[] { "ID", "用户名", "操作权限" }, new bool[] { true, true, true }, false);
        }


        private void InitializeData()
        {
            //ResolveRecipeToDetial(_currentRecipe);

            _bTriggerRise = new bool[32];
            _bTriggerScanerCodeCompleted = new bool[10];
            _bReadScanerCodeCompleted = new bool[10];

            RefreshKeys();
            RefreshRecipe();
            RefreshUser();

            _workTerminated = new ManualResetEvent(false);
            _workThread = new Thread(new ThreadStart(Word_Thread)) { IsBackground = true };

        }

        private void ShowWorkMessage(string msg)
        {
            if (m_nCount >= 10)
            {
                tb_work_info.Text = "";
                m_nCount = 0;
            }
            else
            {
                tb_work_info.Text += string.Format("LaserCode Tips: {0}  {1}{2}", DateTime.Now, msg, Environment.NewLine);
            }
            //if (tb_work_info.Text.Length < 1000)
            //    tb_work_info.Text += string.Format("LaserCode Tips: {0}  {1}{2}", DateTime.Now, msg, Environment.NewLine);
            //else
            //{
            //    tb_work_info.Clear();
            //    tb_work_info.Text += string.Format("LaserCode Tips: {0}  {1}{2}", DateTime.Now, msg, Environment.NewLine);
            //}
        }

        private void RefreshKeys()
        {
            _listKey = BllKeys.SelectData().ToList();
            dgv_key.dgv.DataSource = _listKey;
        }

        private void RefreshRecipe()
        {
            _listRecipe = BllRecipe.SelectData().ToList();
            dgv_recipe.dgv.DataSource = _listRecipe;
        }

        private void RefreshUser()
        {
            _listUser = BllUser.SelectData().ToList();
            dgv_user.dgv.DataSource = _listUser;
        }

        private void InitializeDevice()
        {
            ShowWorkMessage("InitializeDevice");

            //_laser = new MDX1000();
            //_laser.OnLaserDataReceiveEvent += _laser_OnLaserDataReceiveEvent;

            //if (!_laser.ConnectedMD("192.168.99.253", 50002))
            //{
            //    MyMsgBox.Show("刻印机连接失败，请检查配置文件和网络连接。", "软件提示");
            //    System.Environment.Exit(0);
            //}
            //ShowWorkMessage("刻印机连接成功");

            _laser = new MDX1000COM();
            _laser.OnLaserDataReceiveEvent += _laser_OnLaserDataReceiveEvent;
            if (!_laser.ConnectedMD())
            {
                MyMsgBox.Show("刻印机连接失败，请检查配置文件和网络连接。", "软件提示");
                System.Environment.Exit(0);
            }
            ShowWorkMessage("刻印机连接成功");

            //if (!AbSocket.Open("192.168.99.40", 44818, ABReadCallback))
            //{
            //    MyMsgBox.Show("PLC连接失败，请检查配置文件和网络连接。", "软件提示");
            //    System.Environment.Exit(0);
            //}
            //else
            //{
            //    AbSocket.readCallback += ABReadCallback;

            //    Thread.Sleep(1000);

            //    _workThread.Start();
            //    ShowWorkMessage("PLC连接成功,IP:192.168.99.40");
            //    ShowWorkMessage("软件重新启动,请下载配方");
            //}
            if (!ConnPlc())
            {
                MyMsgBox.Show("PLC连接失败，请检查配置文件和网络连接。", "软件提示");
                System.Environment.Exit(0);
            }
            else
            {
                _workThread.Start();
                ShowWorkMessage("PLC连接成功,IP:192.168.99.40");
                ShowWorkMessage("软件重新启动,请下载配方");
            }
        }

        AllenBradleyNet m_abPlc = new AllenBradleyNet("192.168.99.40");
        //连接PLC
        private bool                                                                                                                                                                     ConnPlc()
        {            OperateResult operateResult = m_abPlc.ConnectServer();
            return operateResult.IsSuccess;
        }

        private bool Load970(string code)
        {
            string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string program = "OP970";
            string station = "970A01";
            string product = _currentRecipe.recipe_name;
            string command = "LOAD";
            string paramName = "PCB";
            string paramValue = code;
            string loadRequest = "<request><time>{0}</time><program><![CDATA[{1}]]></program><station><![CDATA[{2}]]></station><product><![CDATA[{3}]]></product><command><![CDATA[{4}]]></command><params><param name='{5}'><![CDATA[{6}]]></param></params></request>";
            loadRequest = string.Format(loadRequest, time, program, station, product, command, paramName, paramValue);
            if (cmbType.SelectedIndex == 0)
            {
                try
                {
                    Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    IPAddress ip = IPAddress.Parse("127.0.0.1");
                    IPEndPoint endPoint = new IPEndPoint(ip, 1752);
                    client.Connect(endPoint);
                    byte[] buffer = Encoding.UTF8.GetBytes(loadRequest);
                    client.SendTo(buffer, endPoint);
                    byte[] recvBuffer = new byte[1024];
                    client.Receive(recvBuffer);
                    string response = Encoding.UTF8.GetString(recvBuffer);
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(response);
                    XmlNode node = doc.SelectSingleNode("/response/result");
                    string result = node.InnerText.Trim();
                    node = doc.SelectSingleNode("/response/message2");
                    if (node != null)
                    {
                        _strRlt = node.InnerText.Trim().Split(';');
                    }
                    client.Close();
                    return result == "0" ? true : false;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            else
            {
                WcfServiceClient wcfClient = new WcfServiceClient();
                string response = wcfClient.GetCommandData(loadRequest);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(response);
                XmlNode node = doc.SelectSingleNode("/response/result");
                string result = node.InnerText.Trim();
                node = doc.SelectSingleNode("/response/message2");
                if (node != null)
                {
                    _strRlt = node.InnerText.Trim().Split(';');
                }
                return result == "0" ? true : false;
            }
        }

        private bool Unload970(string code, bool rlt, string LaserMsg)
        {
            string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string program = "OP970";
            string station = "970A01";
            string product = _currentRecipe.recipe_name;
            string command = "UNLOAD";
            string paramName1 = "PCB";
            string paramValue1 = code;
            string paramName2 = "RESULT";
            string paramValue2 = rlt ? "P" : "F";
            string savename = "SAVEDATA";
            string savedata = LaserMsg;
            string loadRequest = "<request><time>{0}</time><program><![CDATA[{1}]]></program><station><![CDATA[{2}]]></station><product><![CDATA[{3}]]></product><command><![CDATA[{4}]]></command><params><param name='{5}'><![CDATA[{6}]]></param><param name='{7}'><![CDATA[{8}]]></param><param name='{9}'><![CDATA[{10}]]></param></params></request>";
            loadRequest = string.Format(loadRequest, time, program, station, product, command, paramName1, paramValue1, paramName2, paramValue2, savename, savedata);
            if ("Socket".Equals(cmbType.SelectedItem.ToString()))
            {
                try
                {
                    Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    IPAddress ip = IPAddress.Parse("127.0.0.1");
                    IPEndPoint endPoint = new IPEndPoint(ip, 1752);
                    client.Connect(endPoint);
                    byte[] buffer = Encoding.UTF8.GetBytes(loadRequest);
                    client.SendTo(buffer, endPoint);
                    byte[] recvBuffer = new byte[1024];
                    client.Receive(recvBuffer);
                    string response = Encoding.UTF8.GetString(recvBuffer);
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(response);
                    XmlNode node = doc.SelectSingleNode("/response/result");
                    string result = node.InnerText.Trim();
                    client.Close();
                    client.Dispose();
                    return result == "0" ? true : false;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            else
            {
                WcfServiceClient wcfClient = new WcfServiceClient();
                string response = wcfClient.GetCommandData(loadRequest);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(response);
                XmlNode node = doc.SelectSingleNode("/response/result");
                string result = node.InnerText.Trim();
                return result == "0" ? true : false;
            }

        }

        private void ABReadCallback(byte[] readBytes, int readLength, AbDataType type, int index)
        {
            if (0x10 == index)
            {
                if (AbDataType.DInt == type)
                {
                    _nPlcWritePc = BitConverter.ToInt32(readBytes, 0);
                    isRead = false;
                }
            }
            else if (0x20 == index)
            {
                //getScaner1Code
                if (AbDataType.DInt == type)
                {
                    _nCodeOneLength = BitConverter.ToInt32(readBytes, 0);
                }
                else if (AbDataType.Byte == type)
                {
                    _strCodeOne = Encoding.ASCII.GetString(readBytes).Substring(0, _nCodeOneLength);
                    _bReadScanerCodeCompleted[0] = true;
                }
            }
            else if (0x30 == index)
            {
                if (AbDataType.DInt == type)
                {
                    _nCodeTwoLength = BitConverter.ToInt32(readBytes, 0);
                }
                else if (AbDataType.Byte == type)
                {
                    _strCodeTwo = Encoding.ASCII.GetString(readBytes).Substring(0, _nCodeTwoLength);
                    _bReadScanerCodeCompleted[1] = true;
                }
            }
        }
        //private void Word_Thread()
        //{
        //    while (!_workTerminated.WaitOne(100))
        //    {
        //        if (!_bRecipeState)
        //        {
        //            continue;
        //        }
        //        if (!isRead)
        //        {
        //            isRead = true;
        //            AbSocket.Read("PlcWritePc", 1, 0x10);
        //        }

        //        if (_nCurPcWritePlc != _nPcWritePlc)
        //        {
        //            AbSocket.Write("PlcReadPc", BitConverter.GetBytes(_nPcWritePlc), AbDataType.DInt);
        //            _nCurPcWritePlc = _nPcWritePlc;
        //        }
        //        _bCIMByPass = (_nPlcWritePc & (1 << 17)) > 0;
        //        _bDeviceIsRunning = (_nPlcWritePc & (1 << 19)) > 0;
        //        //_bReWork = (_nPlcWritePc & (1 << 20)) > 0;

        //        for (int nIndex = 0; nIndex < 32; nIndex++)
        //        {
        //            bool bTrigger = (_nPlcWritePc & (1 << nIndex)) > 0;

        //            if (bTrigger && !_bTriggerRise[nIndex])
        //            {
        //                switch (nIndex)
        //                {
        //                    case 0://Load getCode - CheckSuffix - RequireCim - getLaserCode - replaySingal

        //                        if (!_bTriggerScanerCodeCompleted[0])
        //                        {
        //                            ShowWorkMessage("Product Load & Scan Code");

        //                            status_load_result.SetStatus(0, "LoadWait");
        //                            status_unload_result.SetStatus(0, "UnLoadWait");

        //                            myLabel_Scan1.SetText("LoadWait", true);
        //                            myLabel_Scan2.SetText("LaserWait", true);
        //                            myLabel_UnLoad.SetText("UnLoadWait", true);

        //                            AbSocket.Read("LoadScan.String.LEN", 1, 0x20);
        //                            AbSocket.Read("LoadScan.String.DATA[0]", 82, 0x20);
        //                            _bReadScanerCodeCompleted[0] = false;
        //                            _bTriggerScanerCodeCompleted[0] = true;
        //                        }
        //                        else
        //                        {
        //                            if (_bReadScanerCodeCompleted[0])
        //                            {
        //                                ShowWorkMessage("Scan Load Code:" + _strCodeOne);

        //                                string strSuffix = _strCodeOne.Substring(_strCodeOne.Length - 2, 2);
        //                                if (0 == strSuffix.CompareTo(_currentRecipe.recipe_name))
        //                                {
        //                                    _bLoadRlt = false;

        //                                    CimInfo = "";
        //                                    _strTime = "";

        //                                    if (_bCIMByPass)
        //                                    {
        //                                        _bLoadRlt = true;
        //                                        //if (Load970(_strCodeOne))
        //                                        //{
        //                                        //    if (_strRlt != null && _strRlt.Length >= 2)
        //                                        //    {
        //                                        //        CimInfo = _strRlt[0].Substring(4, 4);
        //                                        //        _strTime = FormatDate(_strRlt[1]);
        //                                        //        if (_strTime == "null")
        //                                        //        {
        //                                        //            ShowWorkMessage("Get Time From CIM Error");
        //                                        //            _bLoadRlt = false;
        //                                        //        }
        //                                        //    }
        //                                        //}
        //                                        CimInfo = "0000";
        //                                        _strTime = DateTime.Now.ToString("dd.MM.yy");
        //                                    }
        //                                    else
        //                                    {
        //                                        stopWatch.Reset();
        //                                        stopWatch.Start();

        //                                        _bLoadRlt = Load970(_strCodeOne);

        //                                        if (_strRlt != null && _strRlt.Length >= 2)
        //                                        {
        //                                            CimInfo = _strRlt[0].Substring(4, 4);
        //                                            _strTime = FormatDate(_strRlt[1]);
        //                                            if (_strTime == "null")
        //                                            {
        //                                                ShowWorkMessage("Get Time From CIM Error");
        //                                                _bLoadRlt = false;
        //                                            }
        //                                        }
        //                                        else
        //                                        {
        //                                            _bLoadRlt = false;
        //                                        }

        //                                        stopWatch.Stop();
        //                                        long time = stopWatch.ElapsedMilliseconds;
        //                                        ShowWorkMessage("数据库访问时间:" + time.ToString() + "毫秒");

        //                                    }

        //                                    SetPlcResult(_bLoadRlt, 0);

        //                                    if (_bLoadRlt)
        //                                    {
        //                                        ShowWorkMessage("Get Seri From CIM:" + CimInfo);
        //                                        ShowWorkMessage("Get Time From CIM:" + _strTime);
        //                                        status_load_result.SetStatus(1, "LoadOK");
        //                                        ProductInfoLog.WriteMsg(ProductInfoLog.MessageType.INFO, null, BuildLogMess("OK", _strCodeOne, "..."));
        //                                    }

        //                                    else
        //                                    {
        //                                        status_load_result.SetStatus(2, "LoadNG");
        //                                        ShowWorkMessage("Get Data From CIM NG");
        //                                        ProductInfoLog.WriteMsg(ProductInfoLog.MessageType.INFO, null, BuildLogMess("NG", _strCodeOne, "..."));
        //                                    }

        //                                }
        //                                else
        //                                {
        //                                    SetPlcResult(false, 0);
        //                                    status_load_result.SetStatus(2, "LoadNG");
        //                                    ProductInfoLog.WriteMsg(ProductInfoLog.MessageType.INFO, null, BuildLogMess("NG", _strCodeOne, "..."));
        //                                }
        //                                _bTriggerRise[nIndex] = true;
        //                            }
        //                        }
        //                        break;
        //                    case 1://UnLoad getCode - CheckSuffixAndData - UpdateCim - replaySingal
        //                        if (!_bTriggerScanerCodeCompleted[1])
        //                        {
        //                            ShowWorkMessage("Product UnLoad & Scan Code");

        //                            AbSocket.Read("PrintCim.String[0].LEN", 1, 0x30);
        //                            AbSocket.Read("PrintCim.String[0].DATA[0]", 82, 0x30);
        //                            _bReadScanerCodeCompleted[1] = false;
        //                            _bTriggerScanerCodeCompleted[1] = true;
        //                        }
        //                        else
        //                        {
        //                            if (_bReadScanerCodeCompleted[1])
        //                            {
        //                                ShowWorkMessage("Scan Code:" + _strCodeTwo);
        //                                /*
        //                                 * hanshunfa update
        //                                 */
        //                                if (0 == _strCodeTwo.CompareTo(_strSnValue))
        //                                //if (_strCodeTwo.Contains(_currentRecipe.recipe_name))
        //                                {
        //                                    myLabel_Scan2.SetText(_strCodeTwo, true);

        //                                    stopWatch.Reset();
        //                                    stopWatch.Start();

        //                                    if (_bCIMByPass || Unload970(_strCodeOne, true, CimUnloadMsg))
        //                                    {
        //                                        stopWatch.Stop();
        //                                        long time = stopWatch.ElapsedMilliseconds;
        //                                        ShowWorkMessage("数据库访问时间:" + time.ToString() + "毫秒");

        //                                        SetPlcResult(true, 1);
        //                                        myLabel_UnLoad.SetText(_strCodeOne, true);
        //                                        status_unload_result.SetStatus(1, "UnLoadOK");
        //                                        ShowWorkMessage("UnLoad Code:" + _strCodeOne);

        //                                        ProductInfoLog.WriteMsg(ProductInfoLog.MessageType.INFO, null, BuildLogMess("OK", _strCodeOne, _strCodeTwo));

        //                                        okCount = okCount + 1;
        //                                    }
        //                                    else
        //                                    {
        //                                        SetPlcResult(false, 1);
        //                                        myLabel_UnLoad.SetText(_strCodeOne, false);
        //                                        status_unload_result.SetStatus(2, "UnLoadNG");
        //                                        ProductInfoLog.WriteMsg(ProductInfoLog.MessageType.INFO, null, BuildLogMess("NG", _strCodeOne, _strCodeTwo));

        //                                        ngCount = ngCount + 1;
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    if (!_bCIMByPass)
        //                                    {
        //                                        Unload970(_strCodeOne, false, CimUnloadMsg);
        //                                    }
        //                                    myLabel_Scan2.SetText(_strCodeTwo, false);
        //                                    SetPlcResult(false, 1);
        //                                    status_unload_result.SetStatus(2, "UnLoadNG");
        //                                    ProductInfoLog.WriteMsg(ProductInfoLog.MessageType.INFO, null, BuildLogMess("NG", _strCodeOne, _strCodeTwo));

        //                                    ngCount = ngCount + 1;
        //                                }
        //                                _bTriggerRise[nIndex] = true;
        //                            }
        //                        }
        //                        break;
        //                    case 2://LaserCode checklaserstatus - changeCodeContent - triggerLaser - replaySingal
        //                        if (!_bReWork)
        //                        {
        //                            if (!_bTriggerStartLaserCheck)
        //                            {
        //                                ShowWorkMessage("Start Laser -- CheckStatus(0x20)");

        //                                _bTriggerStartLaserCheck = true;
        //                                _bLaserStatusReady = false;
        //                                _laser.CheckLaserStatus(0x20);
        //                            }
        //                            else if (_bLaserStatusReady)
        //                            {
        //                                _bLaserStatusReady = false;

        //                                if (_bLaserStatusResult)
        //                                {
        //                                    Dictionary<int, string> info = new Dictionary<int, string>();
        //                                    for (int j = 0; j < 20; j++)
        //                                    {
        //                                        int nLaserVar = -1;
        //                                        string strLaserDate = string.Empty;

        //                                        switch (j)
        //                                        {
        //                                            case 0:
        //                                                nLaserVar = _currentRecipe.key1_index;
        //                                                strLaserDate = _currentRecipe.key1_data;
        //                                                break;
        //                                            case 2:
        //                                                nLaserVar = _currentRecipe.key2_index;
        //                                                strLaserDate = _currentRecipe.key2_data;
        //                                                break;
        //                                            case 3:
        //                                                nLaserVar = _currentRecipe.key3_index;
        //                                                strLaserDate = _currentRecipe.key3_data;
        //                                                break;
        //                                            case 4:
        //                                                nLaserVar = _currentRecipe.key4_index;
        //                                                strLaserDate = _currentRecipe.key4_data;
        //                                                break;
        //                                            case 5:
        //                                                nLaserVar = _currentRecipe.key5_index;
        //                                                strLaserDate = _currentRecipe.key5_data;
        //                                                break;
        //                                            case 6:
        //                                                nLaserVar = _currentRecipe.key6_index;
        //                                                strLaserDate = _currentRecipe.key6_data;
        //                                                break;
        //                                            case 7:
        //                                                nLaserVar = _currentRecipe.key7_index;
        //                                                strLaserDate = _currentRecipe.key7_data;
        //                                                break;
        //                                            case 8:
        //                                                nLaserVar = _currentRecipe.key8_index;
        //                                                strLaserDate = _currentRecipe.key8_data;
        //                                                break;
        //                                            case 9:
        //                                                nLaserVar = _currentRecipe.key9_index;
        //                                                strLaserDate = _currentRecipe.key9_data;
        //                                                break;
        //                                            case 10:
        //                                                nLaserVar = _currentRecipe.key10_index;
        //                                                strLaserDate = _currentRecipe.key10_data;
        //                                                break;
        //                                            case 11:
        //                                                nLaserVar = _currentRecipe.key11_index;
        //                                                strLaserDate = _currentRecipe.key11_data;
        //                                                break;
        //                                            case 12:
        //                                                nLaserVar = _currentRecipe.key12_index;
        //                                                strLaserDate = _currentRecipe.key12_data;
        //                                                break;
        //                                            case 13:
        //                                                nLaserVar = _currentRecipe.key13_index;
        //                                                strLaserDate = _currentRecipe.key13_data;
        //                                                break;
        //                                            case 14:
        //                                                nLaserVar = _currentRecipe.key14_index;
        //                                                strLaserDate = _currentRecipe.key14_data;
        //                                                break;
        //                                            case 15:
        //                                                nLaserVar = _currentRecipe.key15_index;
        //                                                strLaserDate = _currentRecipe.key15_data;
        //                                                break;
        //                                            case 16:
        //                                                nLaserVar = _currentRecipe.key16_index;
        //                                                strLaserDate = _currentRecipe.key16_data;
        //                                                break;
        //                                            case 17:
        //                                                nLaserVar = _currentRecipe.key17_index;
        //                                                strLaserDate = _currentRecipe.key17_data;
        //                                                break;
        //                                            case 18:
        //                                                nLaserVar = _currentRecipe.key18_index;
        //                                                strLaserDate = _currentRecipe.key18_data;
        //                                                break;
        //                                            case 19:
        //                                                nLaserVar = _currentRecipe.key19_index;
        //                                                strLaserDate = _currentRecipe.key19_data;
        //                                                break;
        //                                            case 20:
        //                                                nLaserVar = _currentRecipe.key20_index;
        //                                                strLaserDate = _currentRecipe.key20_data;
        //                                                break;
        //                                            default:
        //                                                break;
        //                                        }

        //                                        if (-1 != nLaserVar)
        //                                        {
        //                                            info.Add(nLaserVar, ReplaceSpecialData(strLaserDate));
        //                                        }

        //                                    }
        //                                    foreach (var item in info)
        //                                    {
        //                                        if (item.Key == _currentRecipe.sn_key)
        //                                        {
        //                                            _strSnValue = item.Value;
        //                                            CimUnloadMsg = "LaserCode:" + _strSnValue;
        //                                        }
        //                                    }

        //                                    ShowWorkMessage(string.Format("Start Laser -- GetLaserSN Key:{0} Value:{1}", _currentRecipe.sn_key, _strSnValue));
        //                                    ShowWorkMessage("Start Laser -- ChangeCodeInfo(0x30)");

        //                                    _laser.ChangeCodeInfo(0x30, _currentRecipe.laser_pro, info);
        //                                }
        //                                else
        //                                {
        //                                    _bTriggerStartLaserCheck = false;
        //                                    SetPlcResult(false, 2);
        //                                }
        //                                _bTriggerRise[nIndex] = true;
        //                            }
        //                        }
        //                        else
        //                        {
        //                            SetPlcResult(true, 2);
        //                            _bTriggerRise[nIndex] = true;
        //                        }
        //                        break;
        //                    case 18: //LaserErrReset
        //                        _laser.LaserReset(0x50);
        //                        _bTriggerRise[nIndex] = true;
        //                        break;
        //                    default:
        //                        break;
        //                }
        //            }
        //            else if (!bTrigger && _bTriggerRise[nIndex])
        //            {
        //                switch (nIndex)
        //                {
        //                    case 0:
        //                        ResetPlcResult(0);
        //                        _bTriggerRise[nIndex] = false;
        //                        _bTriggerScanerCodeCompleted[0] = false;
        //                        break;
        //                    case 1:
        //                        ResetPlcResult(1);
        //                        _bTriggerRise[nIndex] = false;
        //                        _bTriggerScanerCodeCompleted[1] = false;
        //                        break;
        //                    case 2:
        //                        ResetPlcResult(2);
        //                        _bTriggerRise[nIndex] = false;
        //                        break;
        //                    case 18:
        //                        _bTriggerRise[nIndex] = false;
        //                        break;
        //                    default:
        //                        break;
        //                }
        //            }
        //        }

        //    }
        //}

       private int m_nCount = 0;
        private void Word_Thread()
        {
            while (!_workTerminated.WaitOne(100))
            {
                if (!_bRecipeState)
                {
                    continue;
                }
                if (!isRead)
                {
                    isRead = true;
                    //AbSocket.Read("PlcWritePc", 1, 0x10);
                    OperateResult<int> op = m_abPlc.ReadInt32("PlcWritePc");
                    _nPlcWritePc = op.Content;
                    isRead = false;
                }

                if (_nCurPcWritePlc != _nPcWritePlc)
                {
                    //AbSocket.Write("PlcReadPc", BitConverter.GetBytes(_nPcWritePlc), AbDataType.DInt);
                    m_abPlc.Write("PlcReadPc", _nPcWritePlc);
                    _nCurPcWritePlc = _nPcWritePlc;
                }
                _bCIMByPass = (_nPlcWritePc & (1 << 17)) > 0;
                _bDeviceIsRunning = (_nPlcWritePc & (1 << 19)) > 0;
                //_bReWork = (_nPlcWritePc & (1 << 20)) > 0;

                for (int nIndex = 0; nIndex < 32; nIndex++)
                {
                    bool bTrigger = (_nPlcWritePc & (1 << nIndex)) > 0;

                    if (bTrigger && !_bTriggerRise[nIndex])
                    {
                        switch (nIndex)
                        {
                            case 0://Load getCode - CheckSuffix - RequireCim - getLaserCode - replaySingal
                                m_nCount++;
                                if (!_bTriggerScanerCodeCompleted[0])
                                {
                                    ShowWorkMessage("Product Load & Scan Code");

                                    status_load_result.SetStatus(0, "LoadWait");
                                    status_unload_result.SetStatus(0, "UnLoadWait");

                                    myLabel_Scan1.SetText("LoadWait", true);
                                    myLabel_Scan2.SetText("LaserWait", true);
                                    myLabel_UnLoad.SetText("UnLoadWait", true);

                                    //AbSocket.Read("LoadScan.String.LEN", 1, 0x20);
                                    //AbSocket.Read("LoadScan.String.DATA[0]", 82, 0x20);
                                    OperateResult<short> op1 = m_abPlc.ReadInt16("LoadScan.String.LEN");

                                    OperateResult<string> op2 = m_abPlc.ReadString("LoadScan.String.DATA", (ushort)op1.Content);
                                    _strCodeOne = op2.Content;
                                    _bReadScanerCodeCompleted[0] = true;
                                    _bTriggerScanerCodeCompleted[0] = true;
                                }
                                else
                                {
                                    if (_bReadScanerCodeCompleted[0])
                                    {
                                        ShowWorkMessage("Scan Load Code:" + _strCodeOne);

                                        string strSuffix = _strCodeOne.Substring(_strCodeOne.Length - 2, 2);
                                        if (0 == strSuffix.CompareTo(_currentRecipe.recipe_name))
                                        {
                                            _bLoadRlt = false;

                                            CimInfo = "";
                                            _strTime = "";

                                            if (_bCIMByPass)
                                            {
                                                _bLoadRlt = true;
                                                //if (Load970(_strCodeOne))
                                                //{
                                                //    if (_strRlt != null && _strRlt.Length >= 2)
                                                //    {
                                                //        CimInfo = _strRlt[0].Substring(4, 4);
                                                //        _strTime = FormatDate(_strRlt[1]);
                                                //        if (_strTime == "null")
                                                //        {
                                                //            ShowWorkMessage("Get Time From CIM Error");
                                                //            _bLoadRlt = false;
                                                //        }
                                                //    }
                                                //}
                                                CimInfo = "0000";
                                                _strTime = DateTime.Now.ToString("dd.MM.yy");
                                            }
                                            else
                                            {
                                                stopWatch.Reset();
                                                stopWatch.Start();

                                                _bLoadRlt = Load970(_strCodeOne);

                                                if (_strRlt != null && _strRlt.Length >= 2)
                                                {
                                                    CimInfo = _strRlt[0].Substring(4, 4);
                                                    _strTime = FormatDate(_strRlt[1]);
                                                    if (_strTime == "null")
                                                    {
                                                        ShowWorkMessage("Get Time From CIM Error");
                                                        _bLoadRlt = false;
                                                    }
                                                }
                                                else
                                                {
                                                    _bLoadRlt = false;
                                                }

                                                stopWatch.Stop();
                                                long time = stopWatch.ElapsedMilliseconds;
                                                ShowWorkMessage("数据库访问时间:" + time.ToString() + "毫秒");

                                            }

                                            SetPlcResult(_bLoadRlt, 0);

                                            if (_bLoadRlt)
                                            {
                                                ShowWorkMessage("Get Seri From CIM:" + CimInfo);
                                                ShowWorkMessage("Get Time From CIM:" + _strTime);
                                                status_load_result.SetStatus(1, "LoadOK");
                                                ProductInfoLog.WriteMsg(ProductInfoLog.MessageType.INFO, null, BuildLogMess("OK", _strCodeOne, "..."));
                                            }

                                            else
                                            {
                                                status_load_result.SetStatus(2, "LoadNG");
                                                ShowWorkMessage("Get Data From CIM NG");
                                                ProductInfoLog.WriteMsg(ProductInfoLog.MessageType.INFO, null, BuildLogMess("NG", _strCodeOne, "..."));
                                            }

                                        }
                                        else
                                        {
                                            SetPlcResult(false, 0);
                                            status_load_result.SetStatus(2, "LoadNG");
                                            ProductInfoLog.WriteMsg(ProductInfoLog.MessageType.INFO, null, BuildLogMess("NG", _strCodeOne, "..."));
                                        }
                                        _bTriggerRise[nIndex] = true;
                                    }
                                }
                                break;
                            case 1://UnLoad getCode - CheckSuffixAndData - UpdateCim - replaySingal
                                if (!_bTriggerScanerCodeCompleted[1])
                                {
                                    ShowWorkMessage("Product UnLoad & Scan Code");

                                    //AbSocket.Read("PrintCim.String[0].LEN", 1, 0x30);
                                    //AbSocket.Read("PrintCim.String[0].DATA[0]", 82, 0x30);

                                    OperateResult<short> op1 = m_abPlc.ReadInt16("PrintCimCode.LEN");
                                    OperateResult<string> op2 = m_abPlc.ReadString("PrintCimCode.DATA", (ushort)op1.Content);
                                    _strCodeTwo = op2.Content;

                                    _bReadScanerCodeCompleted[1] = true;
                                    _bTriggerScanerCodeCompleted[1] = true;
                                }
                                else
                                {
                                    if (_bReadScanerCodeCompleted[1])
                                    {
                                        ShowWorkMessage("Scan Code:" + _strCodeTwo);
                                        /*
                                         * hanshunfa update
                                         */
                                         //(0 == _strCodeTwo.CompareTo(_strSnValue))
                                        if (_strCodeTwo.Contains(_currentRecipe.recipe_name))
                                        {
                                            myLabel_Scan2.SetText(_strCodeTwo, true);

                                            stopWatch.Reset();
                                            stopWatch.Start();

                                            if (_bCIMByPass || Unload970(_strCodeOne, true, CimUnloadMsg))
                                            {
                                                stopWatch.Stop();
                                                long time = stopWatch.ElapsedMilliseconds;
                                                ShowWorkMessage("数据库访问时间:" + time.ToString() + "毫秒");

                                                SetPlcResult(true, 1);
                                                myLabel_UnLoad.SetText(_strCodeOne, true);
                                                status_unload_result.SetStatus(1, "UnLoadOK");
                                                ShowWorkMessage("UnLoad Code:" + _strCodeOne);

                                                ProductInfoLog.WriteMsg(ProductInfoLog.MessageType.INFO, null, BuildLogMess("OK", _strCodeOne, _strCodeTwo));

                                                okCount = okCount + 1;
                                            }
                                            else
                                            {
                                                SetPlcResult(false, 1);
                                                myLabel_UnLoad.SetText(_strCodeOne, false);
                                                status_unload_result.SetStatus(2, "UnLoadNG");
                                                ProductInfoLog.WriteMsg(ProductInfoLog.MessageType.INFO, null, BuildLogMess("NG", _strCodeOne, _strCodeTwo));

                                                ngCount = ngCount + 1;
                                            }
                                        }
                                        else
                                        {
                                            if (!_bCIMByPass)
                                            {
                                                Unload970(_strCodeOne, false, CimUnloadMsg);
                                            }
                                            myLabel_Scan2.SetText(_strCodeTwo, false);
                                            SetPlcResult(false, 1);
                                            status_unload_result.SetStatus(2, "UnLoadNG");
                                            ProductInfoLog.WriteMsg(ProductInfoLog.MessageType.INFO, null, BuildLogMess("NG", _strCodeOne, _strCodeTwo));

                                            ngCount = ngCount + 1;
                                        }
                                        _bTriggerRise[nIndex] = true;
                                    }
                                }
                                break;
                            case 2://LaserCode checklaserstatus - changeCodeContent - triggerLaser - replaySingal
                                if (!_bReWork)
                                {
                                    if (!_bTriggerStartLaserCheck)
                                    {
                                        ShowWorkMessage("Start Laser -- CheckStatus(0x20)");

                                        _bTriggerStartLaserCheck = true;
                                        _bLaserStatusReady = false;
                                        _laser.CheckLaserStatus(0x20);
                                    }
                                    else if (_bLaserStatusReady)
                                    {
                                        _bLaserStatusReady = false;

                                        if (_bLaserStatusResult)
                                        {
                                            Dictionary<int, string> info = new Dictionary<int, string>();
                                            for (int j = 0; j < 20; j++)
                                            {
                                                int nLaserVar = -1;
                                                string strLaserDate = string.Empty;

                                                switch (j)
                                                {
                                                    case 0:
                                                        nLaserVar = _currentRecipe.key1_index;
                                                        strLaserDate = _currentRecipe.key1_data;
                                                        break;
                                                    case 2:
                                                        nLaserVar = _currentRecipe.key2_index;
                                                        strLaserDate = _currentRecipe.key2_data;
                                                        break;
                                                    case 3:
                                                        nLaserVar = _currentRecipe.key3_index;
                                                        strLaserDate = _currentRecipe.key3_data;
                                                        break;
                                                    case 4:
                                                        nLaserVar = _currentRecipe.key4_index;
                                                        strLaserDate = _currentRecipe.key4_data;
                                                        break;
                                                    case 5:
                                                        nLaserVar = _currentRecipe.key5_index;
                                                        strLaserDate = _currentRecipe.key5_data;
                                                        break;
                                                    case 6:
                                                        nLaserVar = _currentRecipe.key6_index;
                                                        strLaserDate = _currentRecipe.key6_data;
                                                        break;
                                                    case 7:
                                                        nLaserVar = _currentRecipe.key7_index;
                                                        strLaserDate = _currentRecipe.key7_data;
                                                        break;
                                                    case 8:
                                                        nLaserVar = _currentRecipe.key8_index;
                                                        strLaserDate = _currentRecipe.key8_data;
                                                        break;
                                                    case 9:
                                                        nLaserVar = _currentRecipe.key9_index;
                                                        strLaserDate = _currentRecipe.key9_data;
                                                        break;
                                                    case 10:
                                                        nLaserVar = _currentRecipe.key10_index;
                                                        strLaserDate = _currentRecipe.key10_data;
                                                        break;
                                                    case 11:
                                                        nLaserVar = _currentRecipe.key11_index;
                                                        strLaserDate = _currentRecipe.key11_data;
                                                        break;
                                                    case 12:
                                                        nLaserVar = _currentRecipe.key12_index;
                                                        strLaserDate = _currentRecipe.key12_data;
                                                        break;
                                                    case 13:
                                                        nLaserVar = _currentRecipe.key13_index;
                                                        strLaserDate = _currentRecipe.key13_data;
                                                        break;
                                                    case 14:
                                                        nLaserVar = _currentRecipe.key14_index;
                                                        strLaserDate = _currentRecipe.key14_data;
                                                        break;
                                                    case 15:
                                                        nLaserVar = _currentRecipe.key15_index;
                                                        strLaserDate = _currentRecipe.key15_data;
                                                        break;
                                                    case 16:
                                                        nLaserVar = _currentRecipe.key16_index;
                                                        strLaserDate = _currentRecipe.key16_data;
                                                        break;
                                                    case 17:
                                                        nLaserVar = _currentRecipe.key17_index;
                                                        strLaserDate = _currentRecipe.key17_data;
                                                        break;
                                                    case 18:
                                                        nLaserVar = _currentRecipe.key18_index;
                                                        strLaserDate = _currentRecipe.key18_data;
                                                        break;
                                                    case 19:
                                                        nLaserVar = _currentRecipe.key19_index;
                                                        strLaserDate = _currentRecipe.key19_data;
                                                        break;
                                                    case 20:
                                                        nLaserVar = _currentRecipe.key20_index;
                                                        strLaserDate = _currentRecipe.key20_data;
                                                        break;
                                                    default:
                                                        break;
                                                }

                                                if (-1 != nLaserVar)
                                                {
                                                    info.Add(nLaserVar, ReplaceSpecialData(strLaserDate));
                                                }

                                            }
                                            foreach (var item in info)
                                            {
                                                if (item.Key == _currentRecipe.sn_key)
                                                {
                                                    _strSnValue = item.Value;
                                                    CimUnloadMsg = "LaserCode:" + _strSnValue;
                                                }
                                            }

                                            ShowWorkMessage(string.Format("Start Laser -- GetLaserSN Key:{0} Value:{1}", _currentRecipe.sn_key, _strSnValue));
                                            ShowWorkMessage("Start Laser -- ChangeCodeInfo(0x30)");

                                            _laser.ChangeCodeInfo(0x30, _currentRecipe.laser_pro, info);
                                        }
                                        else
                                        {
                                            _bTriggerStartLaserCheck = false;
                                            SetPlcResult(false, 2);
                                        }
                                        _bTriggerRise[nIndex] = true;
                                    }
                                }
                                else
                                {
                                    SetPlcResult(true, 2);
                                    _bTriggerRise[nIndex] = true;
                                }
                                break;
                            case 18: //LaserErrReset
                                _laser.LaserReset(0x50);
                                _bTriggerRise[nIndex] = true;
                                break;
                            default:
                                break;
                        }
                    }
                    else if (!bTrigger && _bTriggerRise[nIndex])
                    {
                        switch (nIndex)
                        {
                            case 0:
                                ResetPlcResult(0);
                                _bTriggerRise[nIndex] = false;
                                _bTriggerScanerCodeCompleted[0] = false;
                                break;
                            case 1:
                                ResetPlcResult(1);
                                _bTriggerRise[nIndex] = false;
                                _bTriggerScanerCodeCompleted[1] = false;
                                break;
                            case 2:
                                ResetPlcResult(2);
                                _bTriggerRise[nIndex] = false;
                                break;
                            case 18:
                                _bTriggerRise[nIndex] = false;
                                break;
                            default:
                                break;
                        }
                    }
                }

            }
        }
        /// <summary>
        /// replace special data ; DATE...
        /// </summary>
        /// <param name="strLaserDate">sourceData</param>
        /// <returns></returns>
        private string ReplaceSpecialData(string strLaserDate)
        {
            //英文字符(y、M、d、h、m、s、f) 分别代替(年、月、日、时、分、秒、毫秒)
            string strTmp = "";
            strTmp = strLaserDate.Replace("<DATE>", _strTime);
            strTmp = strTmp.Replace("<SEQUENCE>", CimInfo);
            return strTmp;
        }

        private string BuildLogMess(string status, string seriCode, string laserCode)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("产品信息");
            sb.AppendLine(string.Format("DateTime   :{0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
            sb.AppendLine(string.Format("ProductCode:{0}", seriCode));
            sb.AppendLine(string.Format("LaserCode  :{0}", laserCode));
            sb.AppendLine(string.Format("Status     :{0}", status));
            //sb.AppendLine("");

            return sb.ToString();
        }

        private string FormatDate(string strDateCode)
        {
            //string strDate = strDateCode.Substring(0, 4);

            //if(Regex.IsMatch(strDate,@"^\d+$"))
            //{
            //    int year = int.Parse(DateTime.Now.ToString("yyyy").Insert(3, strDate.Substring(3, 1)).Substring(0, 4));
            //    int days = int.Parse(strDate.Substring(0, 3));

            //    DateTime dt = new DateTime(year, 1, 1);
            //    dt = dt.AddDays(days - 1);

            //    return dt.ToString("dd.MM.yy");
            //}
            //else
            //{
            //    return DateTime.Now.ToString("dd.MM.yy");
            //}
            try
            {
                DateTime dt = Convert.ToDateTime(strDateCode);

                return dt.ToString("dd.MM.yy");
            }
            catch
            {
                return "null";
            }

        }

        /// <summary>
        /// Set Replay Plc Result
        /// </summary>
        /// <param name="bOk"></param>
        /// <param name="nType"></param>
        private void SetPlcResult(bool bOk, int nType)
        {

            int nOk = 0, nNg = 0;
            string strType = string.Empty;
            switch (nType)
            {
                case 0:
                    strType = "Load";
                    nOk = 0;
                    nNg = 1;
                    myLabel_Scan1.SetText(_strCodeOne, bOk);
                    break;
                case 1:
                    strType = "UnLoad";

                    nOk = 2;
                    nNg = 3;
                    break;
                case 2:
                    strType = "Laser";

                    nOk = 4;
                    nNg = 5;
                    break;
                default:
                    break;
            }
            if (bOk)
            {
                _nPcWritePlc |= (1 << nOk);
                _nPcWritePlc &= ~(1 << nNg);
            }
            else
            {
                _nPcWritePlc |= (1 << nNg);
                _nPcWritePlc &= ~(1 << nOk);
            }

            ShowWorkMessage(string.Format("SetPlcResult:{0},Type:{1}", bOk, strType));

        }

        private void ResetPlcResult(int nType)
        {
            int nOk = 0, nNg = 0;
            string strType = string.Empty;

            switch (nType)
            {
                case 0:
                    strType = "Load";

                    nOk = 0;
                    nNg = 1;
                    break;
                case 1:
                    strType = "UnLoad";

                    nOk = 2;
                    nNg = 3;
                    break;
                case 2:
                    strType = "Laser";

                    nOk = 4;
                    nNg = 5;
                    break;
                default:
                    break;
            }

            _nPcWritePlc &= ~(1 << nNg);
            _nPcWritePlc &= ~(1 << nOk);

            ShowWorkMessage(string.Format("ResetPlcType:{0}", strType));

        }


        private void _laser_OnLaserDataReceiveEvent(int nFlag, int nResult, string strReadyStatus)
        {
            switch (nFlag)
            {
                case 0x10://change recipe
                    ShowWorkMessage("配方下载处理完毕(0x10)");

                    MyMsgBox.Show(nResult > 0 ? "配方下载成功" : "配方下载失败:" + strReadyStatus, "配方下载提示");
                    break;
                case 0x20://check status
                    _bLaserStatusReady = true;
                    _bLaserError = false;
                    _bLaserStatusResult = (nResult == 0);

                    if (nResult == 1)
                        _nPcWritePlc |= (1 << 18);
                    else
                        _nPcWritePlc &= ~(1 << 18);
                    break;
                case 0x30://change info
                    _bTriggerStartLaserCheck = false;
                    if (nResult > 0)
                    {
                        ShowWorkMessage("Start Laser -- StartLaser(0x40)");

                        _laser.StartLaser(0x40);
                    }
                    else
                        SetPlcResult(false, 2);
                    break;
                case 0x40://start laser
                    SetPlcResult(nResult > 0, 2);
                    if (nResult == 0)
                        ngCount = ngCount + 1;
                    break;
                default:
                    break;
            }
        }


        #region key option

        private void key_tsb_add_Click(object sender, EventArgs e)
        {
            LaserCode.BLL.BllKeys.InsertData(new tb_key() { key_name = "<name>", update_time = DateTime.Now.ToString() });
            RefreshKeys();
        }
        private void key_tsb_delete_Click(object sender, EventArgs e)
        {
            if (null == dgv_key.dgv.CurrentRow)
                return;
            int nPK = Convert.ToInt32(dgv_key.dgv.CurrentRow.Cells[0].Value);
            if (dgv_key.dgv.CurrentRow != null && nPK > 0 && dgv_key.dgv.CurrentRow.Index >= 0)
            {
                if (DialogResult.OK == MyMsgBox.Show(string.Format("是否删除ID为{0},关键字为{1}的数据，单击确定删除。", nPK, dgv_key.dgv.CurrentRow.Cells[1].Value), "软件提示"))
                {
                    if (LaserCode.BLL.BllKeys.DeleteDataByPK(nPK) == LaserCode.BLL.BLLBase<tb_key>.ExecuteResult.Exec_Success)
                    {
                        dgv_key.SetMessage("删除数据完成");
                    }
                    else
                        dgv_key.SetMessage("删除数据失败");
                    RefreshKeys();
                }
            }
            else
                dgv_key.SetMessage("请先选择需要删除的数据行");
        }
        private void key_tsb_apply_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dgv_key.dgv.Rows.Count; i++)
            {
                bool bHaveChange = false;
                for (int j = 0; j < dgv_key.dgv.Columns.Count; j++)
                {
                    DataGridViewCellStyle cell = dgv_key.dgv.Rows[i].Cells[j].Style;
                    if (cell.ForeColor == Color.Red)
                    {
                        bHaveChange = true;
                        cell.ForeColor = Color.Black;
                    }
                }

                if (bHaveChange)
                {
                    BllKeys.UpdateDataByModel(GetKeyModelByID(Convert.ToInt32(dgv_key.dgv.Rows[i].Cells[0].Value)));
                }
            }

            dgv_key.ClearWaitApply();
            dgv_key.SetMessage("修改完毕");
        }
        private void key_tsb_refresh_Click(object sender, EventArgs e)
        {
            RefreshKeys();
        }

        private tb_key GetKeyModelByID(int nID)
        {
            if (dgv_key.dgv.DataSource != null)
                return (dgv_key.dgv.DataSource as List<tb_key>).Where(it => it.id == nID).First();
            else
                return new tb_key();
        }


        #endregion


        #region recipe option
        private void recipe_tsb_refresh_Click(object sender, EventArgs e)
        {
            RefreshRecipe();
        }

        private void recipe_tsb_apply_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dgv_recipe.dgv.Rows.Count; i++)
            {
                bool bHaveChange = false;
                for (int j = 0; j < dgv_recipe.dgv.Columns.Count; j++)
                {
                    DataGridViewCellStyle cell = dgv_recipe.dgv.Rows[i].Cells[j].Style;
                    if (cell.ForeColor == Color.Red)
                    {
                        bHaveChange = true;
                        cell.ForeColor = Color.Black;
                    }
                }

                if (bHaveChange)
                {
                    tb_recipe recipe = GetRecipeModelByID(Convert.ToInt32(dgv_recipe.dgv.Rows[i].Cells[0].Value));
                    if (SugarBase.DB.Queryable<tb_recipe>().ToList().Any(it => it.recipe_name == recipe.recipe_name))
                    {
                        MessageBox.Show("存在程序名：" + recipe.recipe_name + "，请修改后再次【应用】");
                        RefreshRecipe();
                        dgv_recipe.SetMessage("修改失败");
                        return;
                    }
                    else
                    {
                        BllRecipe.UpdateDataByModel(GetRecipeModelByID(Convert.ToInt32(dgv_recipe.dgv.Rows[i].Cells[0].Value)));
                    }
                }
            }
            if (dgv_recipe.dgv.CurrentRow != null && dgv_recipe.dgv.CurrentRow.Index >= 0)
            {
                _nSelectLaserPro = Convert.ToInt32(dgv_recipe.dgv.CurrentRow.Cells[2].Value);
                _nSelectRecipeID = Convert.ToInt32(dgv_recipe.dgv.CurrentRow.Cells[0].Value);
                _strSelectRecipeName = dgv_recipe.dgv.CurrentRow.Cells[1].Value.ToString();
            }
            dgv_recipe.ClearWaitApply();
            dgv_recipe.SetMessage("修改完毕");
        }

        private void recipe_tsb_delete_Click(object sender, EventArgs e)
        {
            if (null == dgv_recipe.dgv.CurrentRow)
                return;
            int nPK = Convert.ToInt32(dgv_recipe.dgv.CurrentRow.Cells[0].Value);
            if (dgv_recipe.dgv.CurrentRow != null && nPK > 0 && dgv_recipe.dgv.CurrentRow.Index >= 0)
            {
                if (DialogResult.OK == MessageBox.Show(string.Format("是否删除ID为{0},配方名称为{1}的数据，单击确定删除。", nPK, dgv_recipe.dgv.CurrentRow.Cells[1].Value), "软件提示"))
                {
                    if (LaserCode.BLL.BllRecipe.DeleteDataByPK(nPK) == LaserCode.BLL.BLLBase<tb_recipe>.ExecuteResult.Exec_Success)
                    {
                        dgv_recipe.SetMessage("删除数据完成");
                    }
                    else
                        dgv_recipe.SetMessage("删除数据失败");
                    RefreshRecipe();
                }
            }
            else
                dgv_recipe.SetMessage("请先选择需要删除的数据行");
        }

        private void recipe_tsb_add_Click(object sender, EventArgs e)
        {
            //BLL.BllRecipe.InsertData(new tb_recipe() { recipe_name = "RCN", laser_pro = 0, update_time = DateTime.Now.ToString() });

            tb_recipe recipe = new tb_recipe();
            LaserCode.BLL.BllRecipe.GetSingleByPK(Convert.ToInt32(dgv_recipe.dgv.CurrentRow.Cells[0].Value), ref recipe);

            recipe.recipe_name = "__";
            LaserCode.BLL.BllRecipe.InsertData(recipe);
            RefreshRecipe();
        }

        private tb_recipe GetRecipeModelByID(int nID)
        {
            if (dgv_recipe.dgv.DataSource != null)
                return (dgv_recipe.dgv.DataSource as List<tb_recipe>).Where(it => it.id == nID).First();
            else
                return new tb_recipe();
        }

        private int _nSelectRecipeID = 0;
        private string _strSelectRecipeName = string.Empty;
        private int _nSelectLaserPro = 0;

        private void recipe_dgv_SelectionChanged(object sender, EventArgs e)
        {
            if (dgv_recipe.dgv.CurrentRow != null && dgv_recipe.dgv.CurrentRow.Index >= 0)
            {
                tb_recipe recipe = new tb_recipe();
                if (BLLBase<tb_recipe>.ExecuteResult.Exec_Success == BllRecipe.GetSingleByPK(Convert.ToInt32(dgv_recipe.dgv.CurrentRow.Cells[0].Value), ref recipe))
                {
                    _nSelectRecipeID = recipe.id;
                    _nSelectLaserPro = recipe.laser_pro;
                    _strSelectRecipeName = recipe.recipe_name;

                    lb_SelectID.Text = recipe.id.ToString();
                    lb_SelectLaser.Text = recipe.laser_pro.ToString();
                    lb_SelectRecipe.Text = recipe.recipe_name;

                    _listInfo.Clear();

                    for (int nIndex = 1; nIndex <= 20; nIndex++)
                    {
                        recipeinfo info = new recipeinfo();
                        info.nID = nIndex;

                        switch (nIndex)
                        {
                            case 1:
                                info.nLaserVar = recipe.key1_index;
                                info.strData = recipe.key1_data;
                                info.strTemplate = recipe.key1_template;
                                break;
                            case 2:
                                info.nLaserVar = recipe.key2_index;
                                info.strData = recipe.key2_data;
                                info.strTemplate = recipe.key2_template;
                                break;
                            case 3:
                                info.nLaserVar = recipe.key3_index;
                                info.strData = recipe.key3_data;
                                info.strTemplate = recipe.key3_template;
                                break;
                            case 4:
                                info.nLaserVar = recipe.key4_index;
                                info.strData = recipe.key4_data;
                                info.strTemplate = recipe.key4_template;
                                break;
                            case 5:
                                info.nLaserVar = recipe.key5_index;
                                info.strData = recipe.key5_data;
                                info.strTemplate = recipe.key5_template;
                                break;
                            case 6:
                                info.nLaserVar = recipe.key6_index;
                                info.strData = recipe.key6_data;
                                info.strTemplate = recipe.key6_template;
                                break;
                            case 7:
                                info.nLaserVar = recipe.key7_index;
                                info.strData = recipe.key7_data;
                                info.strTemplate = recipe.key7_template;
                                break;
                            case 8:
                                info.nLaserVar = recipe.key8_index;
                                info.strData = recipe.key8_data;
                                info.strTemplate = recipe.key8_template;
                                break;
                            case 9:
                                info.nLaserVar = recipe.key9_index;
                                info.strData = recipe.key9_data;
                                info.strTemplate = recipe.key9_template;
                                break;
                            case 10:
                                info.nLaserVar = recipe.key10_index;
                                info.strData = recipe.key10_data;
                                info.strTemplate = recipe.key10_template;
                                break;
                            case 11:
                                info.nLaserVar = recipe.key11_index;
                                info.strData = recipe.key11_data;
                                info.strTemplate = recipe.key11_template;
                                break;
                            case 12:
                                info.nLaserVar = recipe.key12_index;
                                info.strData = recipe.key12_data;
                                info.strTemplate = recipe.key12_template;
                                break;
                            case 13:
                                info.nLaserVar = recipe.key13_index;
                                info.strData = recipe.key13_data;
                                info.strTemplate = recipe.key13_template;
                                break;
                            case 14:
                                info.nLaserVar = recipe.key14_index;
                                info.strData = recipe.key14_data;
                                info.strTemplate = recipe.key14_template;
                                break;
                            case 15:
                                info.nLaserVar = recipe.key15_index;
                                info.strData = recipe.key15_data;
                                info.strTemplate = recipe.key15_template;
                                break;
                            case 16:
                                info.nLaserVar = recipe.key16_index;
                                info.strData = recipe.key16_data;
                                info.strTemplate = recipe.key16_template;
                                break;
                            case 17:
                                info.nLaserVar = recipe.key17_index;
                                info.strData = recipe.key17_data;
                                info.strTemplate = recipe.key17_template;
                                break;
                            case 18:
                                info.nLaserVar = recipe.key18_index;
                                info.strData = recipe.key18_data;
                                info.strTemplate = recipe.key18_template;
                                break;
                            case 19:
                                info.nLaserVar = recipe.key19_index;
                                info.strData = recipe.key19_data;
                                info.strTemplate = recipe.key19_template;
                                break;
                            case 20:
                                info.nLaserVar = recipe.key20_index;
                                info.strData = recipe.key20_data;
                                info.strTemplate = recipe.key20_template;
                                break;
                            default:
                                info.nLaserVar = 0;
                                info.strData = "<null>";
                                break;
                        }

                        if (info.nLaserVar != -1 || info.strTemplate != string.Empty)
                        {
                            _listInfo.Add(info);
                        }
                    }

                    info_tsb_refresh_Click(null, new EventArgs());
                }
            }
        }
        #endregion

        #region recipe info option
        private void info_tsb_refresh_Click(object sender, EventArgs e)
        {
            if (_listInfo.Count > 0)
            {
                dgv_recipe_info.dgv.DataSource = null;
                dgv_recipe_info.dgv.DataSource = _listInfo;
            }
            else if (_listInfo.Count == 0 && dgv_recipe_info.dgv.DataSource != null)
            {
                dgv_recipe_info.dgv.DataSource = null;
            }
        }

        private void info_tsb_apply_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dgv_recipe_info.dgv.Rows.Count; i++)
            {
                //bool bHaveChange = false;
                for (int j = 0; j < dgv_recipe_info.dgv.Columns.Count; j++)
                {
                    DataGridViewCellStyle cell = dgv_recipe_info.dgv.Rows[i].Cells[j].Style;
                    if (cell.ForeColor == Color.Red)
                    {
                        //bHaveChange = true;
                        cell.ForeColor = Color.Black;
                    }
                }
            }

            if (dgv_recipe_info.dgv.Rows.Count > 0)
            {
                tb_recipe recipe = new tb_recipe();
                recipe.id = _nSelectRecipeID;
                recipe.sn_key = (int)dgv_recipe.dgv.CurrentRow.Cells[3].Value;
                recipe.laser_pro = _nSelectLaserPro;
                recipe.recipe_name = _strSelectRecipeName;

                recipeinfo info = new recipeinfo();

                for (int nIndex = 1; nIndex <= _listInfo.Count; nIndex++)
                {
                    info = _listInfo[nIndex - 1];

                    switch (nIndex)
                    {
                        case 1:
                            recipe.key1_index = info.nLaserVar;
                            recipe.key1_data = info.strData;
                            recipe.key1_template = info.strTemplate;

                            break;
                        case 2:
                            recipe.key2_index = info.nLaserVar;
                            recipe.key2_data = info.strData;
                            recipe.key2_template = info.strTemplate;

                            break;
                        case 3:
                            recipe.key3_index = info.nLaserVar;
                            recipe.key3_data = info.strData;
                            recipe.key3_template = info.strTemplate;

                            break;
                        case 4:
                            recipe.key4_index = info.nLaserVar;
                            recipe.key4_data = info.strData;
                            recipe.key4_template = info.strTemplate;

                            break;
                        case 5:
                            recipe.key5_index = info.nLaserVar;
                            recipe.key5_data = info.strData;
                            recipe.key5_template = info.strTemplate;

                            break;
                        case 6:
                            recipe.key6_index = info.nLaserVar;
                            recipe.key6_data = info.strData;
                            recipe.key6_template = info.strTemplate;

                            break;
                        case 7:
                            recipe.key7_index = info.nLaserVar;
                            recipe.key7_data = info.strData;
                            recipe.key7_template = info.strTemplate;

                            break;
                        case 8:
                            recipe.key8_index = info.nLaserVar;
                            recipe.key8_data = info.strData;
                            recipe.key8_template = info.strTemplate;

                            break;
                        case 9:
                            recipe.key9_index = info.nLaserVar;
                            recipe.key9_data = info.strData;
                            recipe.key9_template = info.strTemplate;

                            break;
                        case 10:
                            recipe.key10_index = info.nLaserVar;
                            recipe.key10_data = info.strData;
                            recipe.key10_template = info.strTemplate;

                            break;
                        case 11:
                            recipe.key11_index = info.nLaserVar;
                            recipe.key11_data = info.strData;
                            recipe.key11_template = info.strTemplate;

                            break;
                        case 12:
                            recipe.key12_index = info.nLaserVar;
                            recipe.key12_data = info.strData;
                            recipe.key12_template = info.strTemplate;

                            break;
                        case 13:
                            recipe.key13_index = info.nLaserVar;
                            recipe.key13_data = info.strData;
                            recipe.key13_template = info.strTemplate;

                            break;
                        case 14:
                            recipe.key14_index = info.nLaserVar;
                            recipe.key14_data = info.strData;
                            recipe.key14_template = info.strTemplate;

                            break;
                        case 15:
                            recipe.key15_index = info.nLaserVar;
                            recipe.key15_data = info.strData;
                            recipe.key15_template = info.strTemplate;

                            break;
                        case 16:
                            recipe.key16_index = info.nLaserVar;
                            recipe.key16_data = info.strData;
                            recipe.key16_template = info.strTemplate;

                            break;
                        case 17:
                            recipe.key17_index = info.nLaserVar;
                            recipe.key17_data = info.strData;
                            recipe.key17_template = info.strTemplate;

                            break;
                        case 18:
                            recipe.key18_index = info.nLaserVar;
                            recipe.key18_data = info.strData;
                            recipe.key18_template = info.strTemplate;

                            break;
                        case 19:
                            recipe.key19_index = info.nLaserVar;
                            recipe.key19_data = info.strData;
                            recipe.key19_template = info.strTemplate;

                            break;
                        case 20:
                            recipe.key20_index = info.nLaserVar;
                            recipe.key20_data = info.strData;
                            recipe.key20_template = info.strTemplate;

                            break;
                        default:
                            break;
                    }
                }

                BllRecipe.UpdateDataByModel(recipe);

            }
            // }
            //  }

            dgv_recipe_info.ClearWaitApply();
            dgv_recipe_info.SetMessage("修改完毕");
        }

        private void info_tsb_delete_Click(object sender, EventArgs e)
        {
            if (null == dgv_recipe_info.dgv.CurrentRow)
                return;
            int nPK = Convert.ToInt32(dgv_recipe_info.dgv.CurrentRow.Index);
            if (dgv_recipe_info.dgv.CurrentRow != null && dgv_recipe_info.dgv.CurrentRow.Index >= 0)
            {
                if (DialogResult.OK == MyMsgBox.Show(string.Format("是否删除ID为{0},Laser参数为{1}的数据，单击确定删除。", dgv_recipe_info.dgv.CurrentRow.Cells[0].Value, dgv_recipe_info.dgv.CurrentRow.Cells[1].Value), "软件提示"))
                {
                    _listInfo.RemoveAt(nPK);
                    dgv_recipe_info.SetMessage("删除数据完成");

                    info_tsb_refresh_Click(null, new EventArgs());
                }
            }
            else
                dgv_recipe_info.SetMessage("请先选择需要删除的数据行");
        }

        private void info_tsb_add_Click(object sender, EventArgs e)
        {
            if (_listInfo.Count < 20)
            {
                _listInfo.Add(new recipeinfo() { nID = _listInfo.Count > 0 ? _listInfo[_listInfo.Count - 1].nID + 1 : 1, nLaserVar = -1, strTemplate = "<company>", strData = "kstopa" });
                info_tsb_refresh_Click(null, new EventArgs());
            }
            else
            {
                MessageBox.Show("注意,最多只能添加20条数据", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }

        private void info_dgv_SelectionChanged(object sender, EventArgs e)
        {
            if (dgv_recipe_info.dgv.DataSource != null && dgv_recipe_info.dgv.CurrentRow != null && dgv_recipe_info.dgv.CurrentRow.Index >= 0 && dgv_recipe_info.dgv.CurrentRow.Cells[0].Value != null)
            {
                tb_Templete_Content.Text = dgv_recipe_info.dgv.CurrentRow.Cells[2].Value.ToString();
                tb_Templete_Data.Text = dgv_recipe_info.dgv.CurrentRow.Cells[3].Value.ToString();
            }
        }
        #endregion

        #region 用户管理
        private void user_tsb_refresh_Click(object sender, EventArgs e)
        {
            RefreshUser();
        }

        private void user_tsb_apply_Click(object sender, EventArgs e)
        {

            for (int i = 0; i < dgv_user.dgv.Rows.Count; i++)
            {
                bool bHaveChange = false;
                for (int j = 0; j < dgv_user.dgv.Columns.Count; j++)
                {
                    DataGridViewCellStyle cell = dgv_user.dgv.Rows[i].Cells[j].Style;
                    if (cell.ForeColor == Color.Red)
                    {
                        bHaveChange = true;
                        cell.ForeColor = Color.Black;
                    }
                }

                if (bHaveChange)
                {
                    BllUser.UpdateDataByModel(GetUserModelByID(Convert.ToInt32(dgv_user.dgv.Rows[i].Cells[0].Value)));
                }
            }

            dgv_user.ClearWaitApply();
            dgv_user.SetMessage("修改完毕");
        }

        private void user_tsb_delete_Click(object sender, EventArgs e)
        {
            if (dgv_user.dgv.RowCount > 1)
            {
                BllUser.DeleteDataByPK(Convert.ToInt32(dgv_user.dgv.CurrentRow.Cells[0].Value));
                RefreshUser();
            }

        }

        private void user_tsb_add_Click(object sender, EventArgs e)
        {
            LaserCode.BLL.BllUser.InsertData(new tb_user());
            RefreshUser();
        }

        private tb_user GetUserModelByID(int nID)
        {
            if (dgv_user.dgv.DataSource != null)
                return (dgv_user.dgv.DataSource as List<tb_user>).Where(it => it.id == nID).First();
            else
                return new tb_user();
        }

        #endregion


        private void 添加到模板区ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dgv_key.dgv.CurrentRow != null && dgv_key.dgv.CurrentRow.Index >= 0)
            {
                tb_Templete_Content.Text += dgv_key.dgv.CurrentRow.Cells[1].Value.ToString();
            }
        }

        private void 添加到模板区ClearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tb_Templete_Content.Text = string.Empty;
            tb_Templete_Data.Text = string.Empty;

            if (dgv_key.dgv.CurrentRow != null && dgv_key.dgv.CurrentRow.Index >= 0)
            {
                tb_Templete_Content.Text += dgv_key.dgv.CurrentRow.Cells[1].Value.ToString();
            }
        }

        private void btn_Complile_Template_Click(object sender, EventArgs e)
        {
            if (null == dgv_recipe_info.dgv.CurrentRow)
                return;
            if (dgv_recipe_info.dgv.CurrentRow != null && dgv_recipe_info.dgv.CurrentRow.Index >= 0)
            {
                int nCurrentSelectRowId = Convert.ToInt32(dgv_recipe_info.dgv.CurrentRow.Cells[0].Value);
                dgv_recipe_info.dgv.CurrentRow.Cells[2].Value = tb_Templete_Content.Text.Trim();

                bool bHaveSpecialData = false;
                //special key Date..
                if (tb_Templete_Content.Text.IndexOf("<DATE>") >= 0 || tb_Templete_Content.Text.IndexOf("<SEQUENCE>") >= 0)
                {
                    tb_Templete_Data.Text = tb_Templete_Content.Text.Replace("<DATE>", "<DATE>");
                    tb_Templete_Data.Text = tb_Templete_Data.Text.Replace("<SEQUENCE>", "<SEQUENCE>");
                    bHaveSpecialData = true;
                }
                int nReplaceIndex = 0;
                foreach (recipeinfo item in _listInfo)
                {
                    if (item.nID != nCurrentSelectRowId)
                    {
                        if (tb_Templete_Content.Text.IndexOf(item.strTemplate) >= 0)
                        {
                            if (nReplaceIndex == 0 && !bHaveSpecialData)
                            {
                                tb_Templete_Data.Text = tb_Templete_Content.Text.Replace(item.strTemplate, item.strData);
                            }
                            else
                                tb_Templete_Data.Text = tb_Templete_Data.Text.Replace(item.strTemplate, item.strData);
                            nReplaceIndex++;
                        }
                    }
                }

                dgv_recipe_info.dgv.CurrentRow.Cells[3].Value = tb_Templete_Data.Text.Trim();
            }

            info_tsb_apply_Click(null, new EventArgs());
        }


        private void btn_DownLoad_Recipe_Click(object sender, EventArgs e)
        {
            if (!_bDeviceIsRunning)
            {
                if (DialogResult.OK == MyMsgBox.Show(string.Format("是否下载配方名称为{0}，镭射程序号为:{1} 的配方。", _strSelectRecipeName, _nSelectLaserPro), "配方下载"))
                {
                    BllRecipe.GetSingleByPK(_nSelectRecipeID, ref _currentRecipe);

                    _laser.ChangeProgram(0x10, _nSelectLaserPro);

                    _bRecipeState = true;

                    lbCurType.Text = _currentRecipe.recipe_name;

                    ConfigHelper.UpdateAppConfig("CurrentRecipe", _currentRecipe.id.ToString());
                }
            }
            else
            {
                MessageBox.Show("设备运行中，禁止切换配方", "软件提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }

        private bool _bFlash = false;
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (_laser == null)
                return;

            lb_time.Text = DateTime.Now.ToString();

            //_bFlash = !_bFlash;

            //mylabel_station.SetStatus(_bFlash);

            if (_bCIMByPass)
                status_CIM.SetStatus(2, "CIMByPass");
            else
                status_CIM.SetStatus(1, "CIM");

            if (!_bLaserError)
            {
                _laser.CheckLaserStatus(0x20);
                _bLaserError = true;
            }

            if (!_bFlash)
            {
                //status_plc.SetStatus(AbSocket.GetStatus() ? 1 : 2, "PLC");
                status_plc.SetStatus(true ? 1 : 2, "PLC");
                status_laser.SetStatus(_laser.IsConnected ? 1 : 2, "Laser");
            }
            else
            {
                status_plc.SetStatus(0, "PLC");
                status_laser.SetStatus(0, "Laser");
            }

            if (_bReWork)
            {
                status_rework.SetStatus(2, "Rework");
            }
            else
            {
                status_rework.SetStatus(1, "Normal");
            }

            lbOkNum.Text = okCount.ToString().Trim();
            lbNgNum.Text = ngCount.ToString().Trim();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            tb_work_info.SelectionStart = tb_work_info.Text.Length;
            tb_work_info.ScrollToCaret();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            ConfigHelper.UpdateAppConfig("OkNum", okCount.ToString());
            ConfigHelper.UpdateAppConfig("NgNum", ngCount.ToString());

            if (_laser.IsConnected)
                _laser.CloseMD();

            //if (AbSocket.GetStatus())
            //    AbSocket.Close();

            //_cimConnect.Close();

            Thread.Sleep(500);

        }

        private void metroTabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (metroTabControl1.SelectedIndex)
            {
                case 0:
                    MinitorHandle();
                    break;
                case 1:
                    ParamHandle();
                    break;
                case 2:
                    UserHandle();
                    break;
                default:
                    break;
            }

        }

        private void MinitorHandle()
        {
            curLogin.CurUser.level = "";
            dgv_user.InitializeGraph(new string[] { "id", "user", "level" }, new string[] { "ID", "用户名", "操作权限" }, new bool[] { true, true, true }, false);
            RefreshUser();
        }

        private void ParamHandle()
        {
            //if (curLogin.CurUser.level != "admin")
            //    curLogin.ShowDialog();

            if (!(curLogin.CurUser.level == "admin"))
            {
                dgv_key.InitializeGraph(new string[] { "id", "key_name" }, new string[] { "ID", "关键字名称" }, new bool[] { true, true }, false);
                dgv_recipe.InitializeGraph(new string[] { "id", "recipe_name", "laser_pro", "sn_key" }, new string[] { "ID", "配方名", "LaserID", "条码Key" }, new bool[] { true, true, true, true }, false);
                dgv_recipe_info.InitializeGraph(new string[] { "nID", "nLaserVar", "strTemplate", "strData" }, new string[] { "ID", "Laser变量号", "模板内容", "刻印数据" }, new bool[] { true, true, true, true }, false);
                btn_Complile_Template.Visible = false;
            }
            else
            {
                dgv_key.InitializeGraph(new string[] { "id", "key_name" }, new string[] { "ID", "关键字名称" }, new bool[] { true, false });
                dgv_recipe.InitializeGraph(new string[] { "id", "recipe_name", "laser_pro", "sn_key" }, new string[] { "ID", "配方名", "LaserID", "条码Key" }, new bool[] { true, false, false, false });
                dgv_recipe_info.InitializeGraph(new string[] { "nID", "nLaserVar", "strTemplate", "strData" }, new string[] { "ID", "Laser变量号", "模板内容", "刻印数据" }, new bool[] { true, false, true, true });
                btn_Complile_Template.Visible = true;
            }


        }

        private void UserHandle()
        {
            if (curLogin.CurUser.level != "admin")
            {

            
                if (curLogin.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {

                    if (!(curLogin.CurUser.level == "admin"))
                    {
                        MessageBox.Show("操作员无权操作");
                        metroTabControl1.SelectedIndex = 0;
                        //dgv_user.InitializeGraph(new string[] { "id", "user", "level" }, new string[] { "ID", "用户名", "操作权限" }, new bool[] { true, true, true }, false);
                        //RefreshUser();
                    }
                    else
                    {
                        dgv_user.InitializeGraph(new string[] { "id", "user", "password", "level" }, new string[] { "ID", "用户名", "密码", "操作权限" }, new bool[] { true, false, false, false });
                        RefreshUser();
                    }
                }
                else
                {
                    metroTabControl1.SelectedIndex = 0;
                }
            }
        }

        private void btCountClear_Click(object sender, EventArgs e)
        {
            //AbSocket.Read("LoadScan.String.LEN", 1, 0x20);
            //AbSocket.Read("LoadScan.String.DATA[0]", 82, 0x20);

            //AbSocket.Read("PrintCim.String[0].LEN", 1, 0x30);
            //AbSocket.Read("PrintCim.String[0].DATA[0]", 82, 0x30);
            ngCount = 0;
            okCount = 0;
        }

        private void btnStartLaser_Click(object sender, EventArgs e)
        {
            //_laser.StartLaser(0x40);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
        }

        private void checkBoxRework_CheckedChanged(object sender, EventArgs e)
        {
            _bReWork = checkBoxRework.Checked;
        }

        private void splitContainer6_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "启动Rework功能")
            {
                _bReWork = true;
                button1.Text = "Rework启用中";
                button1.BackColor = Color.Lime;
            }
            else
            {
                _bReWork = false;
                button1.Text = "启动Rework功能";
                button1.BackColor = Color.White;
            }
        }

        private void dgv_recipe_Load(object sender, EventArgs e)
        {

        }
    }
}
