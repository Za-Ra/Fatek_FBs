using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;


namespace LOBBY
{
    class Link_Fatek_Net
    {

        public PLC_Info PLCInfo = new PLC_Info();

        public Socket SocketPLC;
        //public IPHostEntry hostEntry;
        IPAddress PLCIP; //= IPAddress.Parse("192.168.2.20");
        IPEndPoint ipe; //= new IPEndPoint(PLCIP, 500);
        //Socket tempSocket; //=  new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        public bool ConnectPLC(string PLC_IP, int IPE)
        {
            bool bConnectPLC = false;

            try
            {
                PLCIP = IPAddress.Parse(PLC_IP);
                ipe = new IPEndPoint(PLCIP, IPE);
                Socket tempSocket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                tempSocket.Connect(ipe);
                SocketPLC = tempSocket;
                //SocketPLC.Connect(ipe);
                bConnectPLC = true;
            }
            catch (Exception err)
            {
                bConnectPLC = false;
            }
            return bConnectPLC;
        }

        public byte[] SendPLC(int ID, PLC_Info.FunctionID cmd, string Msg)
        {
            bool SendPLC_State = false;

            byte[] PLCRsp = new byte[512];

            PLCInfo.Set_PLC_ID(ID);
            PLCInfo.PLC_Command = ((int)cmd).ToString();

            byte[] sendCom = PLCInfo.CombineCommand(PLCInfo.PLC_ID + PLCInfo.PLC_Command + Msg);
            try
            {
                SocketPLC.Send(sendCom, sendCom.Length, 0);
                SocketPLC.Receive(PLCRsp, PLCRsp.Length, 0);

                SendPLC_State = true;
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }

            return PLCRsp;
        }

    }





    class PLC_Info
    {
        public enum FunctionID
        {
            ReadSystem = 40,
            PLC_StartUp = 41,
            MultiplePointCtrl = 42,
            ReadEnablePoint = 43,
            WriteStatePoint = 44,
            ReadStatePoint = 45,
            ReadReg = 46,
            WriteReg = 47,
            ReadRegPoint = 48,
            //WriteRegPoint = 4E,
            //WriteRegPoint = 4E,
            LoadProgram = 50,
            ReadSystemState = 53
        }

        public enum StartUp
        {
            STOP = 0,
            RUN = 1
        }

        public enum Control
        {
            Disable = 1,
            Enable = 2,
            Set = 3,
            Reset = 4
        }


        public string PLC_ID;
        public string PLC_Command;

        public string Rsp;
        

        public void Set_PLC_ID(int P_id)
        {
            PLC_ID = String.Format("{0:X2}", P_id);
        }

        public void Get_PLC_Response(byte[] rsp)
        {

        }

        //// This is check sum (Longitudinal Redundancy Check)
        public byte[] calculateLRC(byte[] bytes)
        {
            byte LRC = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                LRC += bytes[i];
            }
            byte[] byteConverterUse = new byte[1];
            byteConverterUse[0] = LRC;
            return Encoding.ASCII.GetBytes(BitConverter.ToString(byteConverterUse));
        }
        public byte[] CombineCommand(string CalStrCom)
        {
            byte[] asciiBytes = Encoding.ASCII.GetBytes(CalStrCom);
            byte[] CombineCommand_b = new byte[asciiBytes.GetLength(0) + 4];
            CombineCommand_b[0] = (byte)0x02;
            asciiBytes.CopyTo(CombineCommand_b, 1);
            byte[] ChkSUM = calculateLRC(CombineCommand_b);
            CombineCommand_b[asciiBytes.GetLength(0) + 1] = ChkSUM[0];
            CombineCommand_b[asciiBytes.GetLength(0) + 2] = ChkSUM[1];
            CombineCommand_b[asciiBytes.GetLength(0) + 3] = (byte)0x03;

            return CombineCommand_b;
        }


    }
}
