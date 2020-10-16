using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TopaKit
{
    public enum AbDataType
    { 
        Bool = 0x00C1,
	    Byte = 0x00C2,
	    Int = 0x00C3,
	    DInt = 0x00C4,
	    Real = 0x00CA
    }
    public static class AbSocket
    {
        #region 变量定义
        public delegate void OnReadCallback(byte[] readBytes, int readLength, AbDataType type, int index);
        private static TCPClient client = null;
        private static byte[] session = new byte[4];
        private static byte[] ottd = new byte[4];
        public static OnReadCallback readCallback { get; set; }
        //private static int currentFlag = -1;
        //private static int currentIndex = -1;
        private static Queue<StateObject> stateQueue = new Queue<StateObject>();
        private static StateObject currentState = null;
        private static bool status = false;
        private static long seqCount = 0;
        private static ManualResetEvent readDone = new ManualResetEvent(false);
        private static ManualResetEvent trigger = new ManualResetEvent(true);
        private static Thread workThread = null;
        private static ManualResetEvent writeDone = new ManualResetEvent(false);
        #endregion

        #region 公共方法
        public static bool GetStatus()
        {
            return status;
        }
        public static bool Open(string ip, int port, OnReadCallback onRead = null)
        {
            client = new TCPClient();
            client.ReceiveCallbackEvent += ReceiveCallback;
            if (!client.Open(ip, port))
                return false;
            workThread = new Thread(new ThreadStart(WorkThread));
            workThread.IsBackground = true;
            workThread.Start();
            RegisterSession();
            return true;
        }

        private static void WorkThread()
        {
            for (; ; )
            {
                if (stateQueue.Count > 0)
                {
                    trigger.WaitOne();
                    trigger.Reset();
                    currentState = Pop();
                    client.Send(currentState, -1);
                }
            }
        }

        public static void Close()
        {
            //UnRegisterSession();
            CloseCIM();
        }

        private static void Push(StateObject state)
        {
            lock (stateQueue)
            {
                stateQueue.Enqueue(state);
            }
        }

        private static StateObject Pop()
        {
            lock (stateQueue)
            {
                return stateQueue.Dequeue();
            }
        }

        public static void Read(string strTag, int nDataSize, int index)
        {
            StateObject state = new StateObject();
            state.oprFlag = 2;
            state.idxFlag = index;
		    int i=0;
		    int nLength = strTag.Length;
		    byte[] szPath = new byte[255];
		    int nPathLen = 0;
		    GenPath(strTag, ref szPath, ref nPathLen);

		    state.buffer[i++] = 0x70;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = (byte)(0x28 + nPathLen);
		    state.buffer[i++] = 0x00;

		    state.buffer[i++] = session[0];
            state.buffer[i++] = session[1];
            state.buffer[i++] = session[2];
            state.buffer[i++] = session[3];

		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;

		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;

		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;

		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;

		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;

		    state.buffer[i++] = 0x02;
		    state.buffer[i++] = 0x00;

		    state.buffer[i++] = 0xa1;
		    state.buffer[i++] = 0x00;

		    state.buffer[i++] = 0x04;
		    state.buffer[i++] = 0x00;

		    state.buffer[i++] = ottd[0];
            state.buffer[i++] = ottd[1];
            state.buffer[i++] = ottd[2];
            state.buffer[i++] = ottd[3];

		    state.buffer[i++] = 0xb1;
		    state.buffer[i++] = 0x00;

		    state.buffer[i++] = (byte)(0x14 + nPathLen); // length
		    state.buffer[i++] = 0x00;

		    // seq count
		    if (seqCount > 0xffff)
                seqCount = 0;
            seqCount++;
            state.buffer[i++] = (byte)(seqCount & 0xff);
            state.buffer[i++] = (byte)((seqCount >> 8) & 0xff);

		    state.buffer[i++] = 0x0a;
		    state.buffer[i++] = 0x02;
		    state.buffer[i++] = 0x20;
		    state.buffer[i++] = 0x02;
		    state.buffer[i++] = 0x24;
		    state.buffer[i++] = 0x01;
		    state.buffer[i++] = 0x01;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x04;
		    state.buffer[i++] = 0x00;

		    //
		    state.buffer[i++] = 0x52;
		    state.buffer[i++] = (byte)(nPathLen / 2); // word size

		    for (int j=0; j<nPathLen; j++)
			    state.buffer[i++] = szPath[j];

		    state.buffer[i++] = (byte)(nDataSize & 0xff);
		    state.buffer[i++] = (byte)((nDataSize >> 8) & 0xff);

		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;

		    state.bufferLen = i;
            //client.Send(state);
            Push(state);
        }

        public static void Write(string strTag, byte[] pszData, AbDataType dt)
	    {
		    StateObject state = new StateObject();
		    state.oprFlag = 3;
		    int i=0;
		    int nLength = strTag.Length;

		    int nDataLen = 0;
		    switch(dt)
		    {
		    case AbDataType.Bool:
			    nDataLen = 1;
			    break;
		    case AbDataType.Int:
                nDataLen = 2;
                break;
		    case AbDataType.Real:
            case AbDataType.DInt:
			    nDataLen = 4;
			    break;
		    case AbDataType.Byte:
			    nDataLen = pszData.Length;
			    break;
		    }

		    byte[] szPath = new byte[255];
		    int nPathLen = 0;
		    GenPath(strTag, ref szPath, ref nPathLen);

		    state.buffer[i++] = 0x70;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = (byte)(0x2a + nPathLen + nDataLen);
		    state.buffer[i++] = 0x00;

		    state.buffer[i++] = session[0];
            state.buffer[i++] = session[1];
            state.buffer[i++] = session[2];
            state.buffer[i++] = session[3];

		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;

		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;

		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;

		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;

		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;

		    state.buffer[i++] = 0x02;
		    state.buffer[i++] = 0x00;

		    state.buffer[i++] = 0xa1;
		    state.buffer[i++] = 0x00;

		    state.buffer[i++] = 0x04;
		    state.buffer[i++] = 0x00;

		    state.buffer[i++] = ottd[0];
            state.buffer[i++] = ottd[1];
            state.buffer[i++] = ottd[2];
            state.buffer[i++] = ottd[3];

		    state.buffer[i++] = 0xb1;
		    state.buffer[i++] = 0x00;

		    state.buffer[i++] = (byte)(0x16 + nPathLen + nDataLen); // length
		    state.buffer[i++] = 0x00;

		    // seq count
		    if (seqCount > 0xffff)
                seqCount = 0;
            seqCount++;
            state.buffer[i++] = (byte)(seqCount & 0xff);
            state.buffer[i++] = (byte)((seqCount >> 8) & 0xff);

		    state.buffer[i++] = 0x0a;
		    state.buffer[i++] = 0x02;
		    state.buffer[i++] = 0x20;
		    state.buffer[i++] = 0x02;
		    state.buffer[i++] = 0x24;
		    state.buffer[i++] = 0x01;
		    state.buffer[i++] = 0x01;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x04;
		    state.buffer[i++] = 0x00;
		    //
		    state.buffer[i++] = 0x53;
		    state.buffer[i++] = (byte)(nPathLen / 2); // word size

		    for (int j=0; j<nPathLen; j++)
			    state.buffer[i++] = szPath[j];

		    switch(dt)
		    {
		    case AbDataType.Int:
			    state.buffer[i++] = 0xc3;
			    state.buffer[i++] = 0x00;
			    state.buffer[i++] = 0x01;
			    state.buffer[i++] = 0x00;
			    state.buffer[i++] = 0x00;
			    state.buffer[i++] = 0x00;
			    state.buffer[i++] = 0x00;
			    state.buffer[i++] = 0x00;
			    state.buffer[i++] = pszData[0];
			    state.buffer[i++] = pszData[1];
			    break;
		    case AbDataType.Real:
			    state.buffer[i++] = 0xca;
			    state.buffer[i++] = 0x00;
			    state.buffer[i++] = 0x01;
			    state.buffer[i++] = 0x00;
			    state.buffer[i++] = 0x00;
			    state.buffer[i++] = 0x00;
			    state.buffer[i++] = 0x00;
			    state.buffer[i++] = 0x00;
			    state.buffer[i++] = pszData[0];
			    state.buffer[i++] = pszData[1];
			    state.buffer[i++] = pszData[2];
			    state.buffer[i++] = pszData[3];
			    break;
		    case AbDataType.Bool:
			    state.buffer[i++] = 0xc1;
			    state.buffer[i++] = 0x00;
			    state.buffer[i++] = 0x01;
			    state.buffer[i++] = 0x00;
			    state.buffer[i++] = 0x00;
			    state.buffer[i++] = 0x00;
			    state.buffer[i++] = 0x00;
			    state.buffer[i++] = 0x00;
			    state.buffer[i++] = pszData[0];
			    break;
            case AbDataType.DInt:
			    state.buffer[i++] = 0xc4;
			    state.buffer[i++] = 0x00;
			    state.buffer[i++] = 0x01;
			    state.buffer[i++] = 0x00;
			    state.buffer[i++] = 0x00;
			    state.buffer[i++] = 0x00;
			    state.buffer[i++] = 0x00;
			    state.buffer[i++] = 0x00;
			    state.buffer[i++] = pszData[0];
			    state.buffer[i++] = pszData[1];
			    state.buffer[i++] = pszData[2];
			    state.buffer[i++] = pszData[3];
			    break;
		    case AbDataType.Byte:
			    {
				    state.buffer[i++] = 0xc2;
				    state.buffer[i++] = 0x00;
				    state.buffer[i++] = (byte)(nDataLen & 0xff);
				    state.buffer[i++] = (byte)((nDataLen >> 8) & 0xff);
				    state.buffer[i++] = 0x00;
				    state.buffer[i++] = 0x00;
				    state.buffer[i++] = 0x00;
				    state.buffer[i++] = 0x00;
				    for (int j=0; j<nDataLen; j++)
				    {
					    state.buffer[i++] = pszData[j];
				    }
			    }
			    break;
		    default:
			    break;
		    }
		    state.bufferLen = i;
            Push(state);
            //client.Send(state);
	    }
        #endregion

        #region 私有方法
        private static void RegisterSession()
	    {
		    // Get Session
            StateObject state = new StateObject();
		    state.oprFlag = 0;
		    int i=0;
		    state.buffer[i++] = 0x65;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x04;

		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;

		    state.buffer[i++] = 0x0a;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;

		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;

		    state.buffer[i++] = 0x01;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;

		    state.bufferLen = i;
            //client.Send(state);
            Push(state);
	    }
        private static void UnRegisterSession()
        {
            // Get Session
            StateObject state = new StateObject();
            state.oprFlag = 5;
            int i = 0;
            state.buffer[i++] = 0x66;
            state.buffer[i++] = 0x00;

            state.buffer[i++] = 0x04;
            state.buffer[i++] = 0x00;

            state.buffer[i++] = session[0];
            state.buffer[i++] = session[1];
            state.buffer[i++] = session[2];
            state.buffer[i++] = session[3];

            state.buffer[i++] = 0x00;
            state.buffer[i++] = 0x00;
            state.buffer[i++] = 0x00;
            state.buffer[i++] = 0x00;

            state.buffer[i++] = 0x00;
            state.buffer[i++] = 0x00;
            state.buffer[i++] = 0x00;
            state.buffer[i++] = 0x00;
            state.buffer[i++] = 0x00;
            state.buffer[i++] = 0x00;
            state.buffer[i++] = 0x00;
            state.buffer[i++] = 0x00;

            state.buffer[i++] = 0x00;
            state.buffer[i++] = 0x00;
            state.buffer[i++] = 0x00;
            state.buffer[i++] = 0x00;

            state.buffer[i++] = 0x01;
            state.buffer[i++] = 0x00;
            state.buffer[i++] = 0x00;
            state.buffer[i++] = 0x00;

            state.bufferLen = i;
            //client.Send(state);
            Push(state);
        }
        private static void OpenCIM()
	    {
		    StateObject state = new StateObject();
            state.oprFlag = 1;
		    int i=0;
		    state.buffer[i++] = 0x6f;
		    state.buffer[i++] = 0x00;

		    state.buffer[i++] = 0x40;
		    state.buffer[i++] = 0x00;

            state.buffer[i++] = session[0];
            state.buffer[i++] = session[1];
            state.buffer[i++] = session[2];
            state.buffer[i++] = session[3];

		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;

		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;

		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;

		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;

		    state.buffer[i++] = 0x01;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x02;
		    state.buffer[i++] = 0x00;

		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;

		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;

		    state.buffer[i++] = 0xb2;
		    state.buffer[i++] = 0x00;

		    state.buffer[i++] = 0x30; //
		    state.buffer[i++] = 0x00;

		    state.buffer[i++] = 0x54;
		    state.buffer[i++] = 0x02;

		    state.buffer[i++] = 0x20;
		    state.buffer[i++] = 0x06;
		    state.buffer[i++] = 0x24;
		    state.buffer[i++] = 0x01;
		    state.buffer[i++] = 0x0a; //
		    state.buffer[i++] = 0x05;

		    state.buffer[i++] = 0x00; //
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;

		    state.buffer[i++] = 0x01; // TO
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x20;

		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;

		    state.buffer[i++] = 0x01;
		    state.buffer[i++] = 0x01;

		    state.buffer[i++] = 0x01; // Serial Number == TO
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x20;

		    state.buffer[i++] = 0x01;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x00;

		    state.buffer[i++] = 0x40;
		    state.buffer[i++] = 0x4b;
		    state.buffer[i++] = 0x4c;
		    state.buffer[i++] = 0x00;

		    state.buffer[i++] = 0xf8;
		    state.buffer[i++] = 0x43;

		    state.buffer[i++] = 0x40;
		    state.buffer[i++] = 0x4b;
		    state.buffer[i++] = 0x4c;
		    state.buffer[i++] = 0x00;

		    state.buffer[i++] = 0xf8;
		    state.buffer[i++] = 0x43;

		    state.buffer[i++] = 0xa3;
		    state.buffer[i++] = 0x03;

		    state.buffer[i++] = 0x01;
		    state.buffer[i++] = 0x00;
		    state.buffer[i++] = 0x20;
		    state.buffer[i++] = 0x02;
		    state.buffer[i++] = 0x24;
		    state.buffer[i++] = 0x01;

		    //state.buffer[i++] = 0x2c;
		    //state.buffer[i++] = 0x01;

		    state.bufferLen = i;
            //client.Send(state);
            Push(state);
	    }
        private static void CloseCIM()
        {
            status = false;
            StateObject state = new StateObject();
            state.oprFlag = 4;
            int i = 0;
            state.buffer[i++] = 0x6f;
            state.buffer[i++] = 0x00;
            state.buffer[i++] = 0x26;
            state.buffer[i++] = 0x00;

            state.buffer[i++] = session[0];
            state.buffer[i++] = session[1];
            state.buffer[i++] = session[2];
            state.buffer[i++] = session[3];

            state.buffer[i++] = 0x00;
            state.buffer[i++] = 0x00;
            state.buffer[i++] = 0x00;
            state.buffer[i++] = 0x00;

            state.buffer[i++] = 0x00;
            state.buffer[i++] = 0x00;
            state.buffer[i++] = 0x00;
            state.buffer[i++] = 0x00;
            state.buffer[i++] = 0x00;
            state.buffer[i++] = 0x00;
            state.buffer[i++] = 0x00;
            state.buffer[i++] = 0x00;

            state.buffer[i++] = 0x00;
            state.buffer[i++] = 0x00;
            state.buffer[i++] = 0x00;
            state.buffer[i++] = 0x00;

            state.buffer[i++] = 0x00;
            state.buffer[i++] = 0x00;
            state.buffer[i++] = 0x00;
            state.buffer[i++] = 0x00;

            state.buffer[i++] = 0x01;
            state.buffer[i++] = 0x00;
            state.buffer[i++] = 0x02;
            state.buffer[i++] = 0x00;

            state.buffer[i++] = 0x00;
            state.buffer[i++] = 0x00;

            state.buffer[i++] = 0x00;
            state.buffer[i++] = 0x00;

            state.buffer[i++] = 0xb2;
            state.buffer[i++] = 0x00;

            state.buffer[i++] = 0x16; //
            state.buffer[i++] = 0x00;

            state.buffer[i++] = 0x4E;
            state.buffer[i++] = 0x02;

            state.buffer[i++] = 0x20;
            state.buffer[i++] = 0x06;
            state.buffer[i++] = 0x24;
            state.buffer[i++] = 0x01;

            state.buffer[i++] = 0x0a; //
            state.buffer[i++] = 0x05;

            state.buffer[i++] = 0x00; //
            state.buffer[i++] = 0x00;

            state.buffer[i++] = 0x01;
            state.buffer[i++] = 0x01;

            state.buffer[i++] = 0x01; // TO
            state.buffer[i++] = 0x00;
            state.buffer[i++] = 0x00;
            state.buffer[i++] = 0x20;

            //state.buffer[i++] = ottd[0]; // TO
            //state.buffer[i++] = ottd[1];
            //state.buffer[i++] = ottd[2];
            //state.buffer[i++] = ottd[3];

            state.buffer[i++] = 0x02;
            state.buffer[i++] = 0x00;

            state.buffer[i++] = 0x20;
            state.buffer[i++] = 0x02;
            state.buffer[i++] = 0x24;
            state.buffer[i++] = 0x01;
            //state.buffer[i++] = 0x00;
            //state.buffer[i++] = 0x01;

            state.bufferLen = i;
            //client.Send(state);
            Push(state);
        }
        private static void ReceiveCallback(StateObject state)
        {
            if (state.bufferLen == -1)
            {
                status = true;
                return;
            }
            else if (state.bufferLen == -2)
            {
                // 断开连接
                status = false;
                return;
            }

            byte[] readBytes = new byte[state.bufferLen];
            Array.Copy(state.buffer, readBytes, state.bufferLen);
            int readLength = state.bufferLen;

            if (currentState == null)
                return;

            int currentFlag = currentState.oprFlag;
            if (currentFlag == 0)
            {
                if (readLength < 0x07)
                {
                    status = false;
                    goto __END__;
                }
                //status = true;
                session[0] = readBytes[4];
                session[1] = readBytes[5];
                session[2] = readBytes[6];
                session[3] = readBytes[7];
                OpenCIM();
            }
            else if (currentFlag == 1)
            {
                status = true;
                ottd[0] = readBytes[44];
                ottd[1] = readBytes[45];
                ottd[2] = readBytes[46];
                ottd[3] = readBytes[47];
            }
            else if (currentFlag == 2)
            {
                if (readLength < 61 || readBytes[48] != 0 || readBytes[49] != 0)
                {
                    status = false;
                    goto __END__;
                }
                status = true;
                int nType = readBytes[59];
                nType = (nType << 8);
                nType += readBytes[58];
                AbDataType dt = (AbDataType)nType;
                byte[] data = new byte[readLength - 60];
                Array.Copy(readBytes, 60, data, 0, readLength - 60);
                readCallback(data, readLength - 60, dt, currentState.idxFlag);
                readDone.Set();
            }
            else if (currentFlag == 3)
            {
                if (readLength < 58 || readBytes[48] != 0 || readBytes[49] != 0)
                {
                    status = false;
                    goto __END__;
                }
                status = true;
                writeDone.Reset();
            }
            else if (currentFlag == 4)
            {
                status = false;
                UnRegisterSession();
            }
            else if (currentFlag == 5)
            {
                status = false;
                client.Close();
            }
            __END__:
            trigger.Set();
        }
        private static void GenPath(string strTag, ref byte[] pszPath, ref int nPathLen)
        {
            string tag = strTag;
            int idx = 0;
            int nPos = 0;
            do
            {
                nPos = tag.IndexOf('.');
                if (nPos < 0)
                    nPos = tag.Length;
                string str = tag.Substring(0, nPos);
                int nLen = str.Length;
                int nSubPosStart = str.IndexOf('[');
                int nSubPosEnd = str.IndexOf(']');
                if (nSubPosStart < 0 || nSubPosEnd < 0)
                {
                    // 没有下标
                    pszPath[idx++] = 0x91;
                    pszPath[idx++] = (byte)nLen; /* + nLen % 2*/
                    for (int i = 0; i < nLen; i++)
                    {
                        pszPath[idx++] = (byte)str[i];
                    }
                    if (nLen % 2 == 1)
                        pszPath[idx++] = 0x00;
                }
                else
                {
                    // 有下标
                    nLen = nSubPosStart;
                    pszPath[idx++] = 0x91;
                    pszPath[idx++] = (byte)nLen; /* + nLen % 2*/
                    for (int i = 0; i < nLen; i++)
                    {
                        pszPath[idx++] = (byte)str[i];
                    }
                    if (nLen % 2 == 1)
                        pszPath[idx++] = 0x00;

                    int nSub = Convert.ToInt32(str.Substring(nSubPosStart + 1, nSubPosEnd - nSubPosStart - 1));
                    pszPath[idx++] = 0x2a;
                    pszPath[idx++] = 0x00;
                    pszPath[idx++] = (byte)(nSub & 0xff);
                    pszPath[idx++] = (byte)((nSub >> 8) & 0xff);
                    pszPath[idx++] = 0x00;
                    pszPath[idx++] = 0x00;
                }

                if (nPos + 1 < tag.Length)
                {
                    tag = tag.Remove(0, nPos + 1);
                }
                else
                {
                    tag = tag.Remove(0, nPos);
                }
            }
            while (nPos != -1 && (tag != String.Empty));
            nPathLen = idx;
        }
        #endregion 
    }
}