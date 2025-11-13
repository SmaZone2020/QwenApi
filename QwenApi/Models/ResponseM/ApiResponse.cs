using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QwenApi.Models.ResponseM
{
    public class ApiResponse<T>
    {
        [JsonProperty("success")]
        public bool IsSuccess { get; set; }
        [JsonProperty("request_id")]
        public string RequestID { get; set; }
        [JsonProperty("data")]
        public T Data { get; set; }
    }
}
