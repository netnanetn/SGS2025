using JerryShaw.HCNet;
using QuickNV.DahuaNetSDK;
using QuickNV.DahuaNetSDK.Api;
using SGS2025Client.SDKCameraServices.CameraFactory;
using System;
using System.Runtime.InteropServices;

namespace SGS2025Client.SDKCameraServices.Dahua
{
    public class DahuaCameraSession : ICameraSession
    {
        private DhSession _session;
        private bool _initialized = false;
        private int _realPlayHandle = -1;

        private string _ip;
        private int _port;
        private string _username;
        private string _password;

        private object _lock = new();
        private string _latestBase64Image;

        private fSnapRevCallBack _snapRevCallBack; // field của lớp
        private fRealDataCallBackEx2 _realDataCallback;

        private H264StreamingDecoder _decoder;
        private DateTime _lastFrameTime = DateTime.MinValue;
        private readonly int _frameIntervalMs = 10; // 5 FPS

        private byte[] _latestByteImage;
        private readonly object _fileLock = new object();

        private readonly string _imageFolder = @"C:\TVS\Images\temp";
        public DahuaCameraSession(string ip, int port, string username, string password)
        {
            _ip = ip;
            _port = port;
            _username = username;
            _password = password;
        }

        public void Init()
        {
            if (!_initialized)
            {
                DhSession.Init();
                NETClient.Init(null, IntPtr.Zero, null);  // Init NETClient chỉ gọi 1 lần

                _snapRevCallBack = new fSnapRevCallBack(SnapRevCallBack);
                NETClient.SetSnapRevCallBack(_snapRevCallBack, IntPtr.Zero);

                _initialized = true;
                InitDecoder();
            }

            _session = DhSession.Login(_ip, _port, _username, _password);
            if (_session == null)
                throw new Exception("Login failed");
          //  NETClient.RealPlay(_session.UserId, 0, IntPtr.Zero);
            StartRealPlay();




        }
        void InitDecoder()
        { 
            var path = AppDomain.CurrentDomain.BaseDirectory.Replace("AppX\\", "");
            _decoder = new H264StreamingDecoder(path);
            _decoder.FrameDecoded += base64 =>
            {
                // _latestBase64Image = $"data:image/jpeg;base64,{base64}";
                _latestByteImage = base64;
            };
        }
        private void SnapRevCallBack(IntPtr lLoginID, IntPtr pBuf, uint RevLen, uint EncodeType, uint CmdSerial, IntPtr dwUser)
        {
            if (EncodeType == 10 && RevLen > 0)
            {
                byte[] data = new byte[RevLen];
                Marshal.Copy(pBuf, data, 0, (int)RevLen);

                // string base64 = Convert.ToBase64String(data);
                string base64 = "data:image/jpeg;base64," + Convert.ToBase64String(data);
                // Nếu bạn đang ở một class không phải Component, cần có cách gọi UI cập nhật (Event hoặc callback)
                lock (_lock)
                {
                    _latestBase64Image = base64;
                }

            }
        }
        private void RealDataCallback(IntPtr lRealHandle, uint dwDataType, IntPtr pBuffer, uint dwBufSize, IntPtr param, IntPtr dwUser)
        {
            // dwDataType: loại dữ liệu, thường quan tâm:
            // 0: hệ thống
            // 1: header
            // 2: stream dữ liệu (video/audio)
            if (dwBufSize == 0 || pBuffer == IntPtr.Zero)
                return;

            // Nếu chỉ muốn lấy ảnh JPEG (keyframe) thì cần tách từ stream H.264/H.265 -> phức tạp
            // Ở đây ví dụ copy dữ liệu ra byte[]

            byte[] buffer = new byte[dwBufSize];
            Marshal.Copy(pBuffer, buffer, 0, (int)dwBufSize);


            //test



             _decoder.Feed(buffer);
            //var now = DateTime.Now;
            //if ((now - _lastFrameTime).TotalMilliseconds >= _frameIntervalMs)
            //{
            //    _lastFrameTime = now;
            //    _decoder.Feed(buffer);
            //}


            //end

            //string base64 = Convert.ToBase64String(buffer);

            //// Nếu muốn hiển thị trực tiếp dạng <img src="...">
            //string imgBase64 = "data:image/jpeg;base64," + base64;
            //lock (_lock)
            //{
            //    _latestBase64Image = imgBase64;
            //}

            // TODO: đưa vào decoder H264/H265 để lấy frame ảnh
            // hoặc lưu ra file .h264 để test
            //  File.WriteAllBytes("frame_" + DateTime.Now.Ticks + ".h264", buffer);
        }
        

         
        public void StartRealPlay(int channel = 0)
        {
            _realDataCallback = new fRealDataCallBackEx2(RealDataCallback);
            var inRealPlay = new NET_IN_REALPLAY
            {
                dwSize = (uint)Marshal.SizeOf<NET_IN_REALPLAY>(),
                nChannelID = channel,
                rType = EM_RealPlayType.Realplay_1,//EM_RealPlayType.Realplay, // chuyển sang luồng phụ xem mượt ko
                hWnd = IntPtr.Zero,
                dwUser = IntPtr.Zero,
                cbRealData = _realDataCallback
            };

            var outRealPlay = new NET_OUT_REALPLAY
            {
                dwSize = (uint)Marshal.SizeOf<NET_OUT_REALPLAY>() // ✅ cũng phải set
            };


            // var ret = NETClient.RealPlay(_session.UserId, 0, IntPtr.Zero);
            // if (ret < 1) throw new Exception("RealPlay failed: " + NETClient.GetLastError()); 

            var ret = NETClient.RealPlayEx2(_session.UserId, ref inRealPlay, ref outRealPlay, 3000);
            if (ret == IntPtr.Zero)
            {
                var e = ("RealPlay failed: " + NETClient.GetLastError());
            }

            _realPlayHandle = (int)ret;

            //   _realPlayHandle = (int)outRealPlay.lRealPlayHandle;

            // TODO: Đăng ký callback để lấy frame hoặc lấy frame theo API SDK
        }
        public void StopRealPlay()
        {
            if (_realPlayHandle >= 0)
            {
                NETClient.StopRealPlay(_realPlayHandle);
                _realPlayHandle = -1;
            }
            if (_session != null)
            {
                _session.Logout();
                DhSession.Cleanup();
                _session = null;
            }
        }
        public byte[] SnapPicture2(int channel = 0)
        {
            if (_session == null)
                throw new InvalidOperationException("Not logged in");

            var snapParam = new NET_MANUAL_SNAP_PARAMETER
            {
                nChannel = channel,
                bySequence = Guid.NewGuid().ToString("N"), // Chuỗi định danh ngẫu nhiên
                byReserved = new byte[60]
            };
            try
            {
                var jpegData = _session.PictureService.ManualSnap(channel);
                return jpegData;
            }
            catch (Exception e)
            {
                return null;
            }
            
        }
        public byte[] SnapPicture(int channel = 0)
        {
            if (_session == null)
                throw new InvalidOperationException("Not logged in");

            var snapParam = new NET_MANUAL_SNAP_PARAMETER
            {
                nChannel = channel,
                bySequence = Guid.NewGuid().ToString("N"), // Chuỗi định danh ngẫu nhiên
                byReserved = new byte[60]
            };
            try
            {
                var jpegData = _session.PictureService.ManualSnap(channel);
                return jpegData;
            }
            catch (Exception e)
            {
                return null;
            }

        }
        public string GetBase64Image(int channel = 0)
        {
           
            try
            {
                //_session.PictureService.ManualSnap(channel);
                //return _latestBase64Image;

                // Giả sử bạn có hàm lấy ảnh byte[] raw, ví dụ:
                byte[] jpegData = SnapPicture(); // hoặc gọi ManualSnap(channel)

                if (jpegData == null || jpegData.Length == 0)
                    return _latestBase64Image;

                string base64 = "data:image/jpeg;base64," + Convert.ToBase64String(jpegData);
                 
                return base64;
            }
            catch (Exception e)
            {
                return null;
            }
        }
        public string GetBase64Image()
        {
            try
            {
                byte[] jpegData = SnapPicture();

                if (jpegData == null || jpegData.Length == 0)
                    return _latestBase64Image;

                string base64 = "data:image/jpeg;base64," + Convert.ToBase64String(jpegData);
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
        public string GetBase64ImageByConvert(int channel = 0)
        {

            try
            {
                return _latestBase64Image;

               
            }
            catch (Exception e)
            {
                return null;
            }
        }
        public string CaptureToUrl(string camId)
        {
            try
            {
                if (_latestByteImage == null || _latestByteImage.Length == 0) return null;

                string finalPath = Path.Combine(_imageFolder, $"{camId}.jpg");
                string tempPath = finalPath + ".tmp";

                // Ghi ra file tạm
                File.WriteAllBytes(tempPath, _latestByteImage);

                // Đổi tên file tạm -> file chính (atomic, không bị denied)
                File.Move(tempPath, finalPath, true);
            }
            catch (Exception ex)
            {
              //  Console.WriteLine($"❌ SaveCameraImage error {camId}: {ex.Message}");
            }
            return $"https://local.tvs/temp/{camId}.jpg";
            try
            {

                string filePath = Path.Combine(_imageFolder, $"{camId}.jpg");
                string tempPath = filePath + ".tmp";
                if (_latestByteImage == null) return null;
                lock (_fileLock)
                {
                    using (var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        fs.Write(_latestByteImage, 0, _latestByteImage.Length);
                    }
                    File.Move(tempPath, filePath, true); // overwrite an toàn
                }


                //string filePath = Path.Combine(_imageFolder, $"{camId}.jpg");
                //if (_latestByteImage == null) return null;
                //File.WriteAllBytes(filePath, _latestByteImage);

                // Trả về URL cho Blazor
                return $"https://local.tvs/temp/{camId}.jpg";
            }
            finally
            {
            }
        }
        public void Logout()
        {
            if (_session != null)
            {
                _session.Logout();
                _session = null;
            }
        }

        public void Dispose()
        {
            Logout();
            if (_initialized)
            {
                DhSession.Cleanup();
                _initialized = false;
            }
        }
        public void Stop()
        {
            if (_session != null)
            {
                try
                {
                    _session.Logout();
                }
                catch { /* bỏ qua lỗi nếu có */ }
                _session = null;
            }

            if (_initialized)
            {
                try
                {
                    DhSession.Cleanup();
                }
                catch { }
                _initialized = false;
            }
        }
    }
}
