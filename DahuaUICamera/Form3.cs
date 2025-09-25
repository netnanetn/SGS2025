using DahuaUICamera.SDKTvt;
using DevSdkByCS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DahuaUICamera
{
    public partial class Form3 : Form
    {
        private string CameraUrl1 = "", CameraUser1 = "", CameraPassword1 = "", CameraUrl2 = "", CameraUser2 = "", CameraPassword2 = "", CameraUrl3 = "", CameraUser3 = "", CameraPassword3 = "";
        private int CameraChannel1 = 0, CameraChannel2 = 0, CameraChannel3 = 0;
        public string DauGhiPort = "9008";
        #region declare camera hik
        private uint iLastErr1 = 0;
        private Int32 m_lUserID1 = -1;
        private bool m_bInitSDK1 = false;
        private bool m_bRecord1 = false;
        private bool m_bTalk1 = false;
        private Int32 m_lRealHandle1 = -1;
        private int lVoiceComHandle1 = -1;
        private string str1;



        private uint iLastErr2 = 0;
        private Int32 m_lUserID2 = -1;
        private bool m_bInitSDK2 = false;
        private bool m_bRecord2 = false;
        private bool m_bTalk2 = false;
        private Int32 m_lRealHandle2 = -1;
        private int lVoiceComHandle2 = -1;
        private string str2;


        private uint iLastErr3 = 0;
        private Int32 m_lUserID3 = -1;
        private bool m_bInitSDK3 = false;
        private bool m_bRecord3 = false;
        private bool m_bTalk3 = false;
        private Int32 m_lRealHandle3 = -1;
        private int lVoiceComHandle3 = -1;
        private string str3;

        private NET_SDK_DEVICEINFO oNET_SDK_DEVICEINFO;
        private static LIVE_DATA_CALLBACK myldc = null;
        private int playHandle1, playHandle2, playHandle3;
        #endregion
        public Form3()
        {
            InitializeComponent();
            if (!DevSdkHelper.NET_SDK_Init())
            {
                MessageBox.Show("SDK init failed!");
                return;
            }
            CameraUrl1 = "192.168.10.233"; ;
            CameraUser1 = "admin";
            CameraPassword1 = "A123456a@";

            LivePlayTVT01();
        }
        private void LivePlayTVT01()
        {

            bool isConnected = false;
            int Port = Int32.Parse(this.DauGhiPort);
            if (1 == 1)
            {
                m_lUserID1 = DevSdkHelper.NET_SDK_Login(this.CameraUrl1, (ushort)Port, this.CameraUser1, this.CameraPassword1, ref oNET_SDK_DEVICEINFO);
                if (m_lUserID1 < 1)
                {
                    MessageBox.Show("Login in failed! End ...");
                    isConnected = false;
                }
                else
                {
                    isConnected = true;

                    int channelNum = oNET_SDK_DEVICEINFO.videoInputNum;
                    //Start Live
                    NET_SDK_CLIENTINFO clientInfo = new NET_SDK_CLIENTINFO();
                    clientInfo.lChannel = CameraChannel1;
                    clientInfo.hPlayWnd = cameraPreview1.Handle;
                    clientInfo.streamType = 1;

                    myldc = null;
                    playHandle1 = (int)DevSdkHelper.NET_SDK_LivePlay(m_lUserID1, ref clientInfo, myldc, IntPtr.Zero);
                    if (playHandle1 != -1)
                    {
                        StatusCamera1.Text = "Connected";
                    }
                    else
                    {
                        UInt32 dwErrorCode = DevSdkHelper.NET_SDK_GetLastError();
                        string strMsg = String.Format("{0} \r\nerror code: {1}", "failed to play live", dwErrorCode);
                        MessageBox.Show(strMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
            }
        }
        private bool m_bManualAlarm = false;
        private void btnAlarm_Click(object sender, EventArgs e)
        {
            int lManualCount = oNET_SDK_DEVICEINFO.sensorOutputNum;
            int[] pChannel = new int[lManualCount];
            int[] pValue = new int[lManualCount];
            if (!m_bManualAlarm)
            {
                for (int i = 0; i < lManualCount; i++)
                {
                    pChannel[i] = i;
                    pValue[i] = 1;
                }
                bool ret = DevSdkHelper.NET_SDK_SetDeviceManualAlarm(m_lUserID1, pChannel, pValue, lManualCount, true);
                if (ret)
                {
                    //((Button)sender).Text = "CleanAlarm";
                    m_bManualAlarm = true;
                }
                else
                {
                    MessageBox.Show("Alarm ON error");
                }
            }
            else
            {
                for (int i = 0; i < lManualCount; i++)
                {
                    pChannel[i] = i;
                    pValue[i] = 0;
                }
                bool ret = DevSdkHelper.NET_SDK_SetDeviceManualAlarm(m_lUserID1, pChannel, pValue, lManualCount, false);
                if (ret)
                {
                    //((Button)sender).Text = "ManualAlarm";
                    m_bManualAlarm = false;
                }
                else
                {
                    MessageBox.Show("Alarm OFF error");
                }
            }
        }

        private void btnSetTime_Click(object sender, EventArgs e)
        { 
            int timestamp = (int)(DateTime.UtcNow.AddDays(-10) - new DateTime(1970, 1, 1)).TotalSeconds;

            bool result = DevSdkHelper.NET_SDK_ChangTime(m_lUserID1, timestamp);
            if (result)
                Console.WriteLine("Set thời gian thành công!");
            else
                Console.WriteLine("Set thời gian thất bại!");
        }
    }
}
