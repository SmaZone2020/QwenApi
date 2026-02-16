using RestSharp;
using System.Net.Http;

namespace QwenApi.Common
{
    public class Runtimes
    {
        public readonly static ConfigManager cfgMgr = new();
        public readonly static string BaseUrl = "https://chat.qwen.ai/";
        public readonly static RestClient restClient = new(BaseUrl);
        public readonly static HttpClient httpClient = CreateHttpClient();

        private static HttpClient CreateHttpClient()
        {
            var handler = new HttpClientHandler
            {
                // 配置连接池
                MaxConnectionsPerServer = 10,
                // 启用自动重定向
                AllowAutoRedirect = true,
                // 启用压缩
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            };

            var client = new HttpClient(handler)
            {
                // 设置基础地址
                BaseAddress = new Uri(BaseUrl),
                // 设置默认超时
                Timeout = TimeSpan.FromSeconds(30)
            };

            // 设置默认请求头
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/142.0.0.0 Safari/537.36 Edg/142.0.0.0");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("Referer", BaseUrl);

            return client;
        }
    }
}
