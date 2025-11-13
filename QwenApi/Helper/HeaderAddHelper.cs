using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QwenApi.Helper
{
    public static class HeaderAddHelper
    {
        public static RestRequest AddCommonHeaders(this RestRequest request)
        {
            request.AddHeader("bx-ua", Runtimes.cfgMgr.BxUa);
            request.AddHeader("bx-umidtoken", Runtimes.cfgMgr.BxUmidtoken);
            request.AddHeader("cookie", Runtimes.cfgMgr.Cookie);
            request.AddHeader("Referer", "https://chat.qwen.ai/");
            request.AddHeader("accept", "application/json");
            request.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/142.0.0.0 Safari/537.36 Edg/142.0.0.0");
            return request;
        }

        public static HttpRequestMessage AddCommonHeaders(this HttpRequestMessage request)
        {
            var cfg = Runtimes.cfgMgr;

            request.Headers.TryAddWithoutValidation("bx-ua", cfg.BxUa);
            request.Headers.TryAddWithoutValidation("bx-umidtoken", cfg.BxUmidtoken);
            request.Headers.TryAddWithoutValidation("cookie", cfg.Cookie);
            request.Headers.Referrer = new Uri("https://chat.qwen.ai/");
            request.Headers.TryAddWithoutValidation("accept", "application/json");
            request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/142.0.0.0 Safari/537.36 Edg/142.0.0.0");

            return request;
        }
    }
}
