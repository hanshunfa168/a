using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;

namespace OP970LaserEtch
{
    class MDX1000COM
    {
        private SerialPort _laser;
        public delegate void OnLaserDataReceive(int nFlag, int nResult, string strReadyStatus);
        public event OnLaserDataReceive OnLaserDataReceiveEvent;

        private Queue<LaserCmd> _listCmd = null;
        private object _obLock = null;
        private ManualResetEvent _cmdReceiveDone = null;
        private ManualResetEvent _workTerminated = null;
        private Thread _workThread = null;

        public MDX1000COM()
        {
            _laser = new SerialPort("COM8", 38400, Parity.None, 8, StopBits.One);
            _laser.ReadBufferSize = 2048;
            _laser.DataReceived +=new SerialDataReceivedEventHandler(LaserCom_DataReceived);

            _listCmd = new Queue<LaserCmd>();
            _cmdReceiveDone = new ManualResetEvent(true);

            _workTerminated = new ManualResetEvent(false);
            _workThread = new Thread(new ThreadStart(Work_Thread)) { IsBackground = true };
            _workThread.Start();

            _obLock = new object();
        }

        public bool IsConnected
        {
            get
            {
                return _laser.IsOpen;
            }
        }

        private void Work_Thread()
        {
            while (!_workTerminated.WaitOne(100))
            {
                if (_laser.IsOpen && _listCmd.Count > 0)
                {
                    _cmdReceiveDone.WaitOne();
                    _cmdReceiveDone.Reset();
                    HandleCmdToLaser(PopCmd());
                }
            }
        }

        private void HandleCmdToLaser(LaserCmd laserCmd)
        {
            //set callback data id
            //_laser.SetExtra(laserCmd.nFlag);

            string strCmd = string.Empty;
            switch (laserCmd.nCmdType)
            {
                case CmdType.CHANGE_PROGRAM:
                    strCmd = string.Format("GA,{0}\r", laserCmd.nProgramNum);
                    break;
                case CmdType.CHANGE_CODE_INFO:
                    strCmd = string.Format("C2,{0}", laserCmd.nProgramNum);
                    foreach (var item in laserCmd.dicInfo)
                    {
                        strCmd += string.Format(",{0},{1}", item.Key, item.Value);
                    }
                    strCmd += "\r";
                    break;
                case CmdType.CHECK_STATUE:
                    strCmd = "RE\r";
                    break;
                case CmdType.START_LASER:
                    strCmd = "NT\r";
                    break;
                case CmdType.RESET_LASER:
                    strCmd = "FY\r";
                    break;
                default:
                    break;
            }
            byte[] btDatas = Encoding.ASCII.GetBytes(strCmd);

            _laser.Write(btDatas, 0, btDatas.Length);
        }

        private void LaserCom_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //int nFlag = _laser.GetExtra<int>();
            int nFlag = 0;
            byte[] data = new byte[_laser.BytesToRead];
            _laser.Read(data, 0, _laser.BytesToRead);
            string strReceive = Encoding.ASCII.GetString(data);
            strReceive = strReceive.Replace("\r", "");
            string[] strSplitData = strReceive.Split(',');

            if (strSplitData.Length >= 2)
            {
                if (0 == strSplitData[0].CompareTo("GA") || 0 == strSplitData[0].CompareTo("C2") || 0 == strSplitData[0].CompareTo("NT"))
                {
                    if (strSplitData[1].CompareTo("0") == 0)
                    {
                        if (OnLaserDataReceiveEvent != null)
                        {
                            nFlag = GetNFlag(strSplitData[0]);
                            OnLaserDataReceiveEvent.Invoke(nFlag, 1, string.Empty);
                        }
                    }
                    else
                    {
                        OnLaserDataReceiveEvent.Invoke(nFlag, 0, strSplitData.Length > 3 ? strSplitData[2] : "1");
                    }
                }
                else if (0 == strSplitData[0].CompareTo("RE"))
                {
                    nFlag = GetNFlag(strSplitData[0]);
                    //0 ready 1 error 2 alerady lasering
                    if (strSplitData.Length >= 3)
                    {
                        if (strSplitData[1].CompareTo("0") == 0 && (strSplitData[2].CompareTo("0") == 0))
                        {
                            if (OnLaserDataReceiveEvent != null)
                            {
                                OnLaserDataReceiveEvent.Invoke(nFlag, 0, strSplitData[2]);
                            }
                        }
                        else if (strSplitData[1].CompareTo("0") == 0 && (strSplitData[2].CompareTo("0") == 2))
                        {
                            if (OnLaserDataReceiveEvent != null)
                            {
                                OnLaserDataReceiveEvent.Invoke(nFlag, 2, strSplitData[2]);
                            }
                        }
                        else
                        {
                            if (OnLaserDataReceiveEvent != null)
                            {
                                OnLaserDataReceiveEvent.Invoke(nFlag, 1, strSplitData[2]);
                            }
                        }
                    }
                    else
                    {
                        if (OnLaserDataReceiveEvent != null)
                        {
                            OnLaserDataReceiveEvent.Invoke(nFlag, 1, "1");
                        }
                    }
                }
            }

            _cmdReceiveDone.Set();
        }

        private int GetNFlag(string data)
        {
            int nFlag = 0;
            switch (data)
            {
                case "GA":
                    nFlag = 0x10;
                    break;
                case "RE":
                    nFlag = 0x20;
                    break;
                case "C2":
                    nFlag = 0x30;
                    break;
                case "NT":
                    nFlag = 0x40;
                    break;
            }

            return nFlag;
        }

        #region Private Function
        private void PushCmd(LaserCmd lc)
        {
            lock (_obLock)
            {
                _listCmd.Enqueue(lc);
            }
        }

        private LaserCmd PopCmd()
        {
            lock (_obLock)
            {
                return _listCmd.Dequeue();
            }
        }
        #endregion

        #region Public Interface
        /// <summary>
        /// connecte to laser 
        /// </summary>
        /// <param name="strIp"></param>
        /// <param name="nPort"></param>
        /// <returns></returns>
        public bool ConnectedMD()
        {
            if (!_laser.IsOpen)
            {
                try
                {
                    _laser.Open();
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
                return true;
        }

        public void CloseMD()
        {
            if (_laser.IsOpen)
            {
                _laser.Close();
            }
        }
        /// <summary>
        /// CHANGE LASER PROGRAM NUM 
        /// </summary>
        /// <param name="nProgramID"></param>
        public void ChangeProgram(int nFlag, int nProgramID)
        {
            if (_laser.IsOpen)
            {
                PushCmd(new LaserCmd() { nFlag = nFlag, nCmdType = CmdType.CHANGE_PROGRAM, nProgramNum = nProgramID });
            }
        }

        public void ChangeCodeInfo(int nFlag, int nProgramID, Dictionary<int, string> dicInfo)
        {
            if (_laser.IsOpen)
            {
                PushCmd(new LaserCmd() { nFlag = nFlag, nCmdType = CmdType.CHANGE_CODE_INFO, nProgramNum = nProgramID, dicInfo = dicInfo });
            }
        }

        public void CheckLaserStatus(int nFlag)
        {
            if (_laser.IsOpen)
            {
                PushCmd(new LaserCmd() { nFlag = nFlag, nCmdType = CmdType.CHECK_STATUE });
            }
        }

        public void StartLaser(int nFlag)
        {
            if (_laser.IsOpen)
            {
                PushCmd(new LaserCmd() { nFlag = nFlag, nCmdType = CmdType.START_LASER });
            }
        }

        public void LaserReset(int nFlag)
        {
            if (_laser.IsOpen)
            {
                PushCmd(new LaserCmd() { nFlag = nFlag, nCmdType = CmdType.RESET_LASER });
            }
        }
        #endregion

        public class LaserCmd
        {
            //callback id
            public int nFlag { get; set; }
            //cmd type
            public CmdType nCmdType { get; set; }
            //laser program number
            public int nProgramNum { get; set; }
            //laser data info int:Laser info num,string:Laser info data
            public Dictionary<int, string> dicInfo { get; set; }
        }
        /// <summary>
        /// laser cmd type
        /// </summary>
        public enum CmdType
        {
            CHANGE_PROGRAM = 0, //change program 0000-**** variable argv
            CHANGE_CODE_INFO = 1, //change code info 
            CHECK_STATUE = 2, //check laser ready
            START_LASER = 3,//strat laser code
            RESET_LASER = 4
        }
    }
}
