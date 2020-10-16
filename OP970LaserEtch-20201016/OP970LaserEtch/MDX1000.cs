using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HPSocketCS;
using System.Threading;

namespace LaserCode
{
    /// <summary>
    /// Keyence Laser MDX1000
    /// </summary>
    public class MDX1000
    {
        private TcpClient _laser = null;
        private ManualResetEvent _workTerminated = null;
        private Thread _workThread = null;

        private Queue<LaserCmd> _listCmd = null;
        private ManualResetEvent _cmdReceiveDone = null;

        private object _obLock = null;

        //OnReceiveEvent
        public delegate void OnLaserDataReceive(int nFlag, int nResult, string strReadyStatus);
        public event OnLaserDataReceive OnLaserDataReceiveEvent;

        public bool IsConnected
        {
            get
            {
                return _laser.IsStarted;
            }
        }
        public MDX1000()
        {
            _laser = new TcpClient();
            _laser.OnConnect += _laser_OnConnect;
            _laser.OnClose += _laser_OnClose;
            _laser.OnReceive += _laser_OnReceive;
            _laser.OnSend += _laser_OnSend;

            _listCmd = new Queue<LaserCmd>();
            _cmdReceiveDone = new ManualResetEvent(true);

            _workTerminated = new ManualResetEvent(false);
            _workThread = new Thread(new ThreadStart(Work_Thread)) { IsBackground = true };
            _workThread.Start();

            _obLock = new object();
        }


        private void Work_Thread()
        {
            while (!_workTerminated.WaitOne(100))
            {
                if (_laser.IsStarted && _listCmd.Count > 0)
                {
                    _cmdReceiveDone.WaitOne();
                    _cmdReceiveDone.Reset();
                    HandleCmdToLaser(PopCmd());
                }
            }
        }

        //cmd,data,\r(0x0DH)
        private void HandleCmdToLaser(LaserCmd laserCmd)
        {
            //set callback data id
            _laser.SetExtra(laserCmd.nFlag);

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

            _laser.Send(btDatas, btDatas.Length);
        }

        #region Laser CallBack Function
        private HandleResult _laser_OnSend(TcpClient sender, byte[] bytes)
        {
            return HandleResult.Ok;
        }

        private HandleResult _laser_OnReceive(TcpClient sender, byte[] bytes)
        {
            int nFlag = _laser.GetExtra<int>();

            string strReceive = Encoding.ASCII.GetString(bytes);
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
            return HandleResult.Ok;
        }

        private HandleResult _laser_OnClose(TcpClient sender, SocketOperation enOperation, int errorCode)
        {
            return HandleResult.Ok;
        }

        private HandleResult _laser_OnConnect(TcpClient sender)
        {
            return HandleResult.Ok;
        }
        #endregion

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
        public bool ConnectedMD(string strIp, ushort nPort)
        {
            if (!_laser.IsStarted)
                return _laser.Connect(strIp, nPort, false);
            else
                return true;
        }

        public void CloseMD()
        {
            if (_laser.IsStarted)
            {
                _laser.Stop();
                _laser.Destroy();
            }
        }
        /// <summary>
        /// CHANGE LASER PROGRAM NUM 
        /// </summary>
        /// <param name="nProgramID"></param>
        public void ChangeProgram(int nFlag, int nProgramID)
        {
            if (_laser.IsStarted)
            {
                PushCmd(new LaserCmd() { nFlag = nFlag, nCmdType = CmdType.CHANGE_PROGRAM, nProgramNum = nProgramID });
            }
        }

        public void ChangeCodeInfo(int nFlag, int nProgramID, Dictionary<int, string> dicInfo)
        {
            if (_laser.IsStarted)
            {
                PushCmd(new LaserCmd() { nFlag = nFlag, nCmdType = CmdType.CHANGE_CODE_INFO, nProgramNum = nProgramID, dicInfo = dicInfo });
            }
        }

        public void CheckLaserStatus(int nFlag)
        {
            if (_laser.IsStarted)
            {
                PushCmd(new LaserCmd() { nFlag = nFlag, nCmdType = CmdType.CHECK_STATUE });
            }
        }

        public void StartLaser(int nFlag)
        {
            if (_laser.IsStarted)
            {
                PushCmd(new LaserCmd() { nFlag = nFlag, nCmdType = CmdType.START_LASER });
            }
        }

        public void LaserReset(int nFlag)
        {
            if (_laser.IsStarted)
            {
                PushCmd(new LaserCmd() { nFlag = nFlag, nCmdType = CmdType.RESET_LASER });
            }
        }
        #endregion
    }

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
        RESET_LASER=4
    }
}
