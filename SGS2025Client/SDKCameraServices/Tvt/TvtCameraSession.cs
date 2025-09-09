using DevSdkByCS;
using JerryShaw.HCNet;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.System;

namespace SGS2025Client.SDKCameraServices.Tvt
{
    public class TvtCameraSession
    {
        private int _userID = -1;
        private int _realHandle = -1;
        private int _playPort = -1;

        private string _ip;
        private int _port;
        private string _username;
        private string _password;

        private string _latestBase64Image;
        private object _lock = new();
         
        private static LIVE_DATA_CALLBACK _liveDataCallback;
        private H264StreamingDecoder _decoder;
        private DateTime _lastFrameTime = DateTime.MinValue;
        private readonly int _frameIntervalMs = 10; // 5 FPS

        public TvtCameraSession(string ip, int port, string username, string password)
        {
            _ip = ip;
            _port = port;
            _username = username;
            _password = password;
        }

        public void Init()
        {
            if (!DevSdkHelper.NET_SDK_Init())
                throw new Exception("NET_DVR_Init failed.");
            var devInfo = new NET_SDK_DEVICEINFO();
            _userID = DevSdkHelper.NET_SDK_Login(_ip, (ushort)_port, _username, _password, ref devInfo);
            if (_userID < 0)
                throw new Exception("Login failed: " + CHCNetSDK.NET_DVR_GetLastError());
           // InitDecoder();
            int channelNum = devInfo.videoInputNum;
            //Start Live
            NET_SDK_CLIENTINFO clientInfo = new NET_SDK_CLIENTINFO();
            clientInfo.lChannel = int.Parse("0");
            clientInfo.hPlayWnd = IntPtr.Zero;
            clientInfo.streamType = 1;

            _liveDataCallback = new LIVE_DATA_CALLBACK(RealDataCallback);
          //  var _realHandle = (int)DevSdkHelper.NET_SDK_LivePlay(_userID, ref clientInfo, _liveDataCallback, IntPtr.Zero);
          //  if (_realHandle < 0)    throw new Exception("RealPlay failed: " + CHCNetSDK.NET_DVR_GetLastError());





            
        }
        void InitDecoder()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory.Replace("AppX\\", "");
            _decoder = new H264StreamingDecoder(path);
            _decoder.FrameDecoded += base64 =>
            {
                _latestBase64Image = $"data:image/jpeg;base64,{base64}";
            };
        }

        private void RealDataCallback(long lLiveHandle, NET_SDK_FRAME_INFO frameInfo, IntPtr pBuffer, IntPtr pUser)
        {
            //int curDivide = -1;
            //if (PBAudioDecHandles == null)
            //{
            //    PBAudioDecHandles = new int[16];
            //}
            //for (int i = 0; i < 16; i++)
            //{
            //    if (PBAudioDecHandles[i] == lLiveHandle)
            //    {
            //        curDivide = i;
            //    }
            //}
 

            if (frameInfo.frameType == 5)//video format frame
            {
                if (frameInfo.length == Marshal.SizeOf(typeof(BITMAPINFOHEADER)))
                {
                    BITMAPINFOHEADER bitmapinfo = (BITMAPINFOHEADER)Marshal.PtrToStructure(pBuffer, typeof(BITMAPINFOHEADER));
                    if (DevSdkHelper.GetbiCompression(bitmapinfo.biCompression) == "H264")
                        Debug.Print("is H264 \n");
                    else if (DevSdkHelper.GetbiCompression(bitmapinfo.biCompression) == "HEVC")
                        Debug.Print("is H265 \n");
                }

                SDK_FRAME_INFO info = new SDK_FRAME_INFO();
                info.keyFrame = frameInfo.keyFrame;
                info.nLength = frameInfo.length;
                info.nHeight = frameInfo.height;
                info.nWidth = frameInfo.width;
                info.nStamp = frameInfo.time;
                info.frameType = frameInfo.frameType;


                //int bb = Marshal.SizeOf(info);
                //byte[] aa = StructToBytes(info, bb);
                //MyWriteLog(aa, "record11_" + lLiveHandle + ".txt");//
                //MyWriteFile(aa, "record11_" + lLiveHandle + ".txt");//
                //uint len = frameInfo.length;
                //aa = new byte[len];
                //Marshal.Copy(pBuffer, aa, 0, (int)len);
                //if (dataIndex > 0)
                //MyWriteLog(aa, "record11_" + lLiveHandle + ".txt");
                //MyWriteFile(aa, "record11_" + lLiveHandle + ".txt");//
                //dataIndex++;
            }
            else if (frameInfo.frameType == 1)//video data frame
            {
                SDK_FRAME_INFO info = new SDK_FRAME_INFO();
                info.keyFrame = frameInfo.keyFrame;
                info.nLength = frameInfo.length;
                info.nHeight = frameInfo.height;
                info.nWidth = frameInfo.width;
                info.nStamp = frameInfo.time;
                info.frameType = frameInfo.frameType;

                int bb = Marshal.SizeOf(info);
                byte[] aa = StructToBytes(info, bb);
                uint len = frameInfo.length;
                aa = new byte[len];
                Marshal.Copy(pBuffer, aa, 0, (int)len);


                //byte[] buffer = new byte[pBuffer];
                //Marshal.Copy(pBuffer, buffer, 0, (int)pBuffer);
                 
                _decoder.Feed(aa);

                
            }
             
        }
        private byte[] StructToBytes(object structObj, int size)
        {
            byte[] bytes = new byte[size];
            IntPtr structPtr = Marshal.AllocHGlobal(size);
            //将结构体拷到分配好的内存空间
            Marshal.StructureToPtr(structObj, structPtr, false);
            //从内存空间拷贝到byte 数组
            Marshal.Copy(structPtr, bytes, 0, size);
            //释放内存空间
            Marshal.FreeHGlobal(structPtr);
            return bytes;
        }
        public string GetBase64Image()
        {
            try
            {
                int jpegSize = 8 * 1024 * 1024;
                byte[] buffer = new byte[jpegSize];
                uint actualSize = 0;

                GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                nint ptr = Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0);

                bool result = PlayCtrl.PlayM4_GetJPEG(_playPort, ptr, (uint)jpegSize, ref actualSize);
                handle.Free();

                if (!result || actualSize == 0)
                    return null;

                string base64 = "data:image/jpeg;base64," + Convert.ToBase64String(buffer, 0, (int)actualSize);
                lock (_lock)
                {
                    _latestBase64Image = base64;
                }

                return base64;
            }
            catch
            {
                return null;
            }
        }
        public string GetBase64ImageByCapture()
        {
            IntPtr sp = IntPtr.Zero;
            try
            {
                int picSize = 8 * 1024 * 1024;
                sp = Marshal.AllocHGlobal(picSize);
                int size = 0;

                bool ret = DevSdkHelper.NET_SDK_CaptureJpeg(_userID, 0, 0, sp, picSize, ref size);
                if (ret && size > 0)
                {
                    byte[] data = new byte[size];
                    Marshal.Copy(sp, data, 0, size);

                    string base64 = Convert.ToBase64String(data);
                    string dataUrl = $"data:image/jpeg;base64,{base64}";

                    lock (_lock)
                    {
                        _latestBase64Image = dataUrl;
                    }
                    return dataUrl;
                }

                return _latestBase64Image;
            }
            catch
            {
                return _latestBase64Image;
            }
            finally
            {
                if (sp != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(sp); // ✅ luôn free
                }
            }
        }

        public void Stop()
        {
            if (_realHandle >= 0)
            {
                CHCNetSDK.NET_DVR_StopRealPlay(_realHandle);
                _realHandle = -1;
            }

            if (_userID >= 0)
            {
                CHCNetSDK.NET_DVR_Logout(_userID);
                _userID = -1;
            }

            PlayCtrl.PlayM4_Stop(_playPort);
            PlayCtrl.PlayM4_CloseStream(_playPort);
            PlayCtrl.PlayM4_FreePort(_playPort);
        }
    }
}
