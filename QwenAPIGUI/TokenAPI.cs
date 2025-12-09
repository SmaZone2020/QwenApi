using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QwenAPIGUI
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    public partial class TokenAPI
    {
        private HttpListener? _httpListener;
        private string? _tokenUrl;
        public bool IsStart { get; set; } = false;

        public void Start()
        {
            if (_httpListener != null)
            {
                Console.WriteLine("HTTP 服务已在运行");
                return;
            }

            try
            {
                var listener = new HttpListener();
                int port = 0;
                string urlPrefix = "";
                for (int i = 0; i < 100; i++)
                {
                    port = new Random().Next(49152, 65535);
                    urlPrefix = $"http://localhost:{port}/";
                    try
                    {
                        listener.Prefixes.Add(urlPrefix);
                        listener.Start();
                        break;
                    }
                    catch
                    {
                        listener.Prefixes.Clear();
                        if (i == 99) throw new Exception("无法找到可用端口");
                    }
                }

                _httpListener = listener;
                _tokenUrl = $"http://localhost:{port}";

                _ = Task.Run(() => HandleHttpRequests());

                var exePath = Path.Combine(Application.StartupPath, "QwenWebAPI.exe");
                if (!File.Exists(exePath))
                {
                    Console.WriteLine("未找到 QwenWebAPI.exe");
                    StopHttpServer();
                    return;
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = $"\"{_tokenUrl}\"",
                    UseShellExecute = true
                });

                Console.WriteLine($"服务已启动：{_tokenUrl}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"启动失败: {ex.Message}");
                StopHttpServer();
            }
        }
        public async Task HandleHttpRequests()
        {
            try
            {
                while (_httpListener != null && _httpListener.IsListening)
                {
                    var context = await _httpListener.GetContextAsync();
                    var request = context.Request;
                    var response = context.Response;

                    if (request.Url?.AbsolutePath == "/token")
                    {
                        string tokenLine1 = Runtimes.Bx_UA;
                        string tokenLine2 = Runtimes.Cookies;
                        string tokenLine3 = Runtimes.Bx_Umidtoken;
                        string tokenResponse = $"{tokenLine1}{Environment.NewLine}{tokenLine2}{Environment.NewLine}{tokenLine3}";
                        
                        Console.WriteLine(tokenResponse);

                        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(tokenResponse);
                        response.ContentType = "text/plain";
                        response.ContentLength64 = buffer.Length;
                        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                    }
                    else
                    {
                        // 404
                        response.StatusCode = 404;
                    }

                    response.Close();
                }
            }
            catch (HttpListenerException)
            {
            }
            catch (Exception ex)
            {
                MessageBox.Show($"HTTP 服务错误: {ex.Message}");
            }
        }

        public void StopHttpServer()
        {
            if (_httpListener != null)
            {
                _httpListener.Stop();
                _httpListener.Close();
                _httpListener = null;
            }
        }

    }
}
