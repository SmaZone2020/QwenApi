using QwenApi.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QwenApi
{
    public class Runtimes
    {
        public readonly static ConfigManager cfgMgr = new();
        public readonly static string BaseUrl = "https://chat.qwen.ai/";
        public readonly static RestClient restClient = new(BaseUrl);
    }
}
