using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;
using System.Text;

namespace DahuaUICamera
{
    public class CameraApiServer
    {
        private HttpListener _listener;

        public void Start(int port = 8082)
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://*:{port}/SendAlarmStatus");
            _listener.Start();

            _ = Task.Run(async () =>
            {
                while (_listener.IsListening)
                {
                    var context = await _listener.GetContextAsync();
                    if (context.Request.HttpMethod == "POST")
                    {
                        using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
                        string json = await reader.ReadToEndAsync();
                       // var alarm = JsonSerializer.Deserialize<AlarmStatus>(json);
                       // Console.WriteLine($"Alarm from {alarm.CameraId}, type: {alarm.AlarmType}, status: {alarm.Status}");

                        byte[] response = Encoding.UTF8.GetBytes("{\"Message\":\"Received\"}");
                        context.Response.ContentType = "application/json";
                        context.Response.OutputStream.Write(response, 0, response.Length);
                        context.Response.Close();
                    }
                }
            });
        }

        public void Stop()
        {
            _listener?.Stop();
        }
    }
}
