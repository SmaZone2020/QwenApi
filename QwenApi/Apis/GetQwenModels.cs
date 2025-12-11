using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QwenApi.Helper;
using QwenApi.Models.ResponseM;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static QwenApi.Apis.GetSessionHistory;

namespace QwenApi.Apis
{
    public class QwenModelList
    {
        [JsonProperty("data")]
        public List<QwenModelItem> Data = [];
    }

    public class QwenModelItem
    {
        [JsonProperty("id")]
        public required string ID { get; set; }

        [JsonProperty("name")]
        public required string Name { get; set; }

        [JsonProperty("object")]
        public required string Object { get; set; }

        [JsonProperty("owned_by")]
        public required string OwnedBy { get; set; }

        [JsonProperty("info")]
        public required QwenModelInfo Info { get; set; }
    }


    public class QwenModelInfo
    {
        [JsonProperty("id")]
        public required string ID { get; set; }

        [JsonProperty("user_id")]
        public required string UID { get; set; }

        [JsonProperty("base_model_id")]
        public string? BaseModelID { get; set; }

        [JsonProperty("name")]
        public required string Name { get; set; }

        [JsonProperty("meta")]
        public required QwenModelMeta MetaData { get; set; }

        [JsonProperty("access_control")]
        public string? AccessControl { get; set; }

        [JsonProperty("is_active")]
        public bool IsActive {  get; set; }

        [JsonProperty("is_visitor_active")]
        public bool IsVisitorActive { get; set; }

        [JsonProperty("updated_at")]
        public long UpdateAt { get; set; }

        [JsonProperty("created_at")]
        public long CreatedAt { get; set; }
    }

    public class QwenModelMeta
    {
        [JsonProperty("description")]
        public required string Description {  get; set; }
        
        [JsonProperty("capabilities")]
        public required QwenModelCapabilities Capabilities { get; set; }

        [JsonProperty("short_description")]
        public required string ShortDescription { get; set; }

        [JsonProperty("max_context_length")]
        public required long MaxContext { get; set; }

        [JsonProperty("max_thinking_generation_length")]
        public required long MaxThinkingGeneration { get; set; }

        [JsonProperty("max_summary_generation_length")]
        public required long MaxSummaryGeneration { get; set; }

        [JsonProperty("chat_type")]
        public required string[] ChatTpye { get; set; } = [];

        [JsonProperty("mcp")]
        public required string[] MCP { get; set; } = [];

        [JsonProperty("modality")]
        public required string[] Modality { get; set; } = [];

    }

    public class QwenModelCapabilities
    {
        [JsonProperty("vision")]
        public bool Vision {  get; set; }

        [JsonProperty("document")]
        public bool Document { get; set; }

        [JsonProperty("video")]
        public bool Video { get; set; }

        [JsonProperty("audio")]
        public bool Audio { get; set; }

        [JsonProperty("citations")]
        public bool Citations { get; set; }

        [JsonProperty("thinking_budget")]
        public bool ThinkingBudget { get; set; }

        [JsonProperty("thinking")]
        public bool Thinking { get; set; }

    }

    public class GetQwenModels
    {
        public static DateTime GetDateForTimestamp(long timestamp)
            => TimeZone.CurrentTimeZone.ToLocalTime(new(1970, 1, 1)).AddSeconds(timestamp);

        public static async Task<QwenModelList?> ExecuteAsync()
        {
            var request = new RestRequest($"/api/models", Method.Get);
            request.AddCommonHeaders();

            var response = await Runtimes.restClient.ExecuteAsync(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK || string.IsNullOrEmpty(response.Content))
            {
                return null;
            }

            var apiResponse = JsonConvert.DeserializeObject<QwenModelList>(response.Content);
            if (apiResponse != null && apiResponse.Data != null)
            {
                return apiResponse;
            }
            return null;
        }
    }
}
