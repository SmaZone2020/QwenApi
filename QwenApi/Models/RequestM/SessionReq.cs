using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QwenApi.Models.RequestM
{
    public class SessionReq
    {
        public string title { get; set; } = "新建对话";
        public List<string> models { get; set; } = ["qwen3-max-2025-10-30"];
        public string chat_mode { get; set; } = "normal";
        public string chat_type { get; set; } = "t2t";
        public long timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}
