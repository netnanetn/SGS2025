using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;
using ImageFormat = System.Drawing.Imaging.ImageFormat;

public unsafe class H264StreamingDecoder : IDisposable
{
    private AVCodecContext* _ctx;
    private AVCodecParserContext* _parser;
    private AVFrame* _frame;
    private AVPacket* _pkt;
    private SwsContext* _sws;
    private bool _disposed;

    // Sự kiện trả về JPEG Base64 mỗi khi decode được 1 frame
    public event Action<string>? FrameDecoded;

    private int _frameCount = 0;

    private DateTime _lastFrameTime = DateTime.MinValue;
    private readonly int _frameIntervalMs = 200; // ~5 fps 200

    public H264StreamingDecoder(string ffmpegDllFolder)
    {
        // Chỉ định thư mục chứa DLL FFmpeg trước khi gọi bất kỳ API nào
        ffmpeg.RootPath = ffmpegDllFolder;

        // Tìm codec H.264
        var codec = ffmpeg.avcodec_find_decoder(AVCodecID.AV_CODEC_ID_H264);
        if (codec == null) throw new Exception("Không tìm thấy codec H.264 (avcodec_find_decoder == null)");

        // Cấp phát context
        _ctx = ffmpeg.avcodec_alloc_context3(codec);
        if (_ctx == null) throw new Exception("Không cấp phát được AVCodecContext");

        // Mở codec
        int err = ffmpeg.avcodec_open2(_ctx, codec, null);
        if (err < 0) ThrowFFmpegError("avcodec_open2", err);

        // Parser để chia NALU đúng cách từ stream
        _parser = ffmpeg.av_parser_init((int)AVCodecID.AV_CODEC_ID_H264);
        if (_parser == null) throw new Exception("Không khởi tạo được AVCodecParserContext");

        // Frame + Packet
        _frame = ffmpeg.av_frame_alloc();
        if (_frame == null) throw new Exception("Không cấp phát được AVFrame");

        _pkt = ffmpeg.av_packet_alloc();
        if (_pkt == null) throw new Exception("Không cấp phát được AVPacket");
    }

    /// <summary>
    /// Feed một chunk H264 (từ callback Dahua). Có thể gọi liên tục từ thread callback.
    /// </summary>
    public void Feed(ReadOnlySpan<byte> h264Chunk)
    {
        if (_disposed) return;
        if (h264Chunk.Length == 0) return;

        fixed (byte* pData = h264Chunk)
        {
            byte* inData = pData;
            int inLen = h264Chunk.Length;

            while (inLen > 0)
            {
                byte* outData = null;
                int outSize = 0;

                // Parser tách data thành packet hoàn chỉnh
                int used = ffmpeg.av_parser_parse2(
                    _parser,
                    _ctx,
                    &outData,
                    &outSize,
                    inData,
                    inLen,
                    ffmpeg.AV_NOPTS_VALUE,
                    ffmpeg.AV_NOPTS_VALUE,
                    0
                );

                if (used < 0)
                {
                    // Lỗi parse – bỏ chunk này
                    break;
                }

                inData += used;
                inLen -= used;

                if (outSize > 0)
                {
                    // Tạo packet từ parser output
                    _pkt->data = outData;
                    _pkt->size = outSize;

                    // Gửi packet vào decoder
                    int retSend = ffmpeg.avcodec_send_packet(_ctx, _pkt);
                    if (retSend < 0)
                    {
                        // EAGAIN hoặc lỗi: bỏ qua packet này
                        continue;
                    }

                    // Lấy tất cả frame có sẵn trong decoder
                    while (true)
                    {
                        int retRecv = ffmpeg.avcodec_receive_frame(_ctx, _frame);
                        if (retRecv == ffmpeg.AVERROR(ffmpeg.EAGAIN) || retRecv == ffmpeg.AVERROR_EOF)
                            break;
                        if (retRecv < 0)
                        {
                            // Lỗi decode frame – bỏ
                            break;
                        }

                        _frameCount++;
                        //if (_frameCount % 20 == 0) // lấy mỗi 5 frame
                        //{
                        //    string? b64 = ConvertFrameToJpegBase64(_frame, ref _sws);
                        //    if (b64 != null)
                        //        FrameDecoded?.Invoke(b64);
                        //}

                        var now = DateTime.UtcNow;
                        if ((now - _lastFrameTime).TotalMilliseconds >= _frameIntervalMs)
                        {
                            _lastFrameTime = now;

                            string? b64 = ConvertFrameToJpegBase64(_frame, ref _sws);
                            if (b64 != null)
                                FrameDecoded?.Invoke(b64);
                        }

                        // Convert YUV -> BGR (Bitmap)
                        //string? b64 = ConvertFrameToJpegBase64(_frame, ref _sws);
                        //if (b64 != null)
                        //{
                        //    FrameDecoded?.Invoke(b64);
                        //}

                        ffmpeg.av_frame_unref(_frame);
                    }

                    ffmpeg.av_packet_unref(_pkt);
                }
            }
        }
    }

    private static string? ConvertFrameToJpegBase64_FullQuantity(AVFrame* frame, ref SwsContext* sws)
    {
        int w = frame->width;
        int h = frame->height;
        if (w <= 0 || h <= 0) return null;

        // Tạo (hoặc tái sử dụng) SwsContext cho convert YUV -> BGR24
       
        if (sws == null)
        {
            sws = ffmpeg.sws_getContext(
            w, h, (AVPixelFormat)frame->format,
            w, h, AVPixelFormat.AV_PIX_FMT_BGR24,
            ffmpeg.SWS_BILINEAR, null, null, null
        );
        }
        if (sws == null) return null;

        using var bmp = new Bitmap(w, h, PixelFormat.Format24bppRgb);
        var data = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, bmp.PixelFormat);

        byte_ptrArray4 dstData = default;
        int_array4 dstLinesize = default;
        dstData[0] = (byte*)data.Scan0;
        dstLinesize[0] = data.Stride;

        ffmpeg.sws_scale(sws, frame->data, frame->linesize, 0, h, dstData, dstLinesize);
        bmp.UnlockBits(data);

        using var ms = new MemoryStream();
        bmp.Save(ms, ImageFormat.Jpeg);
        return Convert.ToBase64String(ms.ToArray());
    }
    private static string? ConvertFrameToJpegBase64(AVFrame* frame, ref SwsContext* sws)
    {
        int w = frame->width;
        int h = frame->height;
        if (w <= 0 || h <= 0) return null;

        if (sws == null)
        {
            sws = ffmpeg.sws_getContext(
                w, h, (AVPixelFormat)frame->format,
                w, h, AVPixelFormat.AV_PIX_FMT_BGR24,
                ffmpeg.SWS_BILINEAR, null, null, null
            );
        }
        if (sws == null) return null;

        using var bmp = new Bitmap(w, h, PixelFormat.Format24bppRgb);
        var data = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, bmp.PixelFormat);

        byte_ptrArray4 dstData = default;
        int_array4 dstLinesize = default;
        dstData[0] = (byte*)data.Scan0;
        dstLinesize[0] = data.Stride;

        ffmpeg.sws_scale(sws, frame->data, frame->linesize, 0, h, dstData, dstLinesize);
        bmp.UnlockBits(data);

        using var ms = new MemoryStream();
        // --- set chất lượng JPEG ---
        var encoder = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
        var encoderParams = new EncoderParameters(1);
        encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 80L); // 0–100
        bmp.Save(ms, encoder, encoderParams);

        return Convert.ToBase64String(ms.ToArray());
    }
    private static string? ConvertFrameToJpegBase64_new(AVFrame* frame, ref SwsContext* sws)
    {
        int targetW = 640;
        int targetH = 480;

        if (frame->width <= 0 || frame->height <= 0) return null;

        // Tạo SwsContext resize
        if (sws == null)
        {
            sws = ffmpeg.sws_getContext(
                frame->width, frame->height, (AVPixelFormat)frame->format,
                targetW, targetH, AVPixelFormat.AV_PIX_FMT_BGR24,
                ffmpeg.SWS_BILINEAR, null, null, null
            );
        }
        if (sws == null) return null;

        using var bmp = new Bitmap(targetW, targetH, PixelFormat.Format24bppRgb);
        var data = bmp.LockBits(new Rectangle(0, 0, targetW, targetH), ImageLockMode.WriteOnly, bmp.PixelFormat);

        byte_ptrArray4 dstData = default;
        int_array4 dstLinesize = default;
        dstData[0] = (byte*)data.Scan0;
        dstLinesize[0] = data.Stride;

        // scale frame gốc -> frame fixed size
        ffmpeg.sws_scale(sws, frame->data, frame->linesize, 0, frame->height, dstData, dstLinesize);

        bmp.UnlockBits(data);

        using var ms = new MemoryStream();
        var encoder = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
        var encoderParams = new EncoderParameters(1);
        encoderParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 80L);
        bmp.Save(ms, encoder, encoderParams);

        return Convert.ToBase64String(ms.ToArray());
    }

    public void Flush()
    {
        if (_disposed) return;

        // Gửi null packet để flush
        int ret = ffmpeg.avcodec_send_packet(_ctx, null);
        if (ret < 0) return;

        while (true)
        {
            ret = ffmpeg.avcodec_receive_frame(_ctx, _frame);
            if (ret == ffmpeg.AVERROR(ffmpeg.EAGAIN) || ret == ffmpeg.AVERROR_EOF) break;
            if (ret < 0) break;

            string? b64 = ConvertFrameToJpegBase64(_frame, ref _sws);
            if (b64 != null) FrameDecoded?.Invoke(b64);
            ffmpeg.av_frame_unref(_frame);
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        try { Flush(); } catch { }

        if (_sws != null) { ffmpeg.sws_freeContext(_sws); _sws = null; }
        if (_frame != null) { ffmpeg.av_frame_free((AVFrame**)_frame); _frame = null; }
        if (_pkt != null) { ffmpeg.av_packet_free((AVPacket**)_pkt); _pkt = null; }
        if (_ctx != null) { ffmpeg.avcodec_free_context((AVCodecContext**)_ctx); _ctx = null; }
        if (_parser != null) { ffmpeg.av_parser_close(_parser); _parser = null; }
    }

    private static void ThrowFFmpegError(string api, int err)
    {
        var errbuf = stackalloc byte[1024];
        ffmpeg.av_strerror(err, errbuf, 1024);
        string msg = Marshal.PtrToStringAnsi((IntPtr)errbuf) ?? $"ffmpeg error {err}";
        throw new Exception($"{api} failed: {msg} ({err})");
    }
}
