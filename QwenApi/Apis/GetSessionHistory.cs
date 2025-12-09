using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QwenApi.Helper;
using QwenApi.Models.RequestM;
using QwenApi.Models.ResponseM;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QwenApi.Apis
{
    public class GetSessionHistory
    {
        public class SessionData
        {
            public string Id { get; set; }
            public string UserId { get; set; }
            public string Title { get; set; }
            public Chat Chat { get; set; }
            public long UpdatedAt { get; set; }
            public long CreatedAt { get; set; }
            public string ShareId { get; set; }
            public bool Archived { get; set; }
            public bool Pinned { get; set; }
            public Meta Meta { get; set; }
            public string FolderId { get; set; }
            public List<string> CurrentResponseIds { get; set; }
            public string CurrentId { get; set; }
            public string ChatType { get; set; }
            public List<string> Models { get; set; } // 注意：顶层 models 为 null，但 chat.models 有值
        }

        public class Chat
        {
            public History History { get; set; }
            public List<string> Models { get; set; }
            public List<Message> Messages { get; set; }
        }
        public class History
        {
            public Dictionary<string, Message> Messages { get; set; }
            public string CurrentId { get; set; }
            public List<string> CurrentResponseIds { get; set; }
        }
        public class Message
        {
            public string Id { get; set; }
            public string Role { get; set; }
            public string Content { get; set; }

            public string ReasoningContent { get; set; }
            public string Model { get; set; }
            public string ModelName { get; set; }
            public int? ModelIdx { get; set; }
            public bool? IsStop { get; set; }
            public bool Done { get; set; }
            public object Info { get; set; }
            public Dictionary<string, object> Meta { get; set; } 
            public Extra Extra { get; set; }
            public string FeedbackId { get; set; }
            public string TurnId { get; set; }
            public object Annotation { get; set; }

            // User & Assistant 共有
            public List<string> Models { get; set; }
            public string ChatType { get; set; }
            public string SubChatType { get; set; }
            public bool Edited { get; set; }
            public object Error { get; set; }
            public FeatureConfig FeatureConfig { get; set; }
            public string ParentId { get; set; }
            public List<string> ChildrenIds { get; set; }
            public List<object> Files { get; set; } 
            public long Timestamp { get; set; }
            public List<ContentItem> Content_List { get; set; }
        }

        public class FeatureConfig
        {
            public bool ThinkingEnabled { get; set; }
            public string OutputSchema { get; set; }
            public object Instructions { get; set; }
            public string ResearchMode { get; set; }
        }

        public class Extra
        {
            public MetaContainer Meta { get; set; }
            public double? EndTime { get; set; }
        }

        public class MetaContainer
        {
            public string SubChatType { get; set; }
        }

        public class ContentItem
        {
            public string Content { get; set; }
            public string Phase { get; set; }
            public string Status { get; set; }
            public object Extra { get; set; }
            public string Role { get; set; }
            public Usage Usage { get; set; }
        }

        public class Usage
        {
            public int InputTokens { get; set; }
            public int OutputTokens { get; set; }
            public int TotalTokens { get; set; }
        }

        public class Meta
        {
            public long Timestamp { get; set; }
            public List<string> Tags { get; set; }
        }

        public static async Task<SessionData> ExecuteAsync(string id)
        {
            var request = new RestRequest($"/api/v2/chats/{id}", Method.Get);
            request = request.AddCommonHeaders();

            var response = await Runtimes.restClient.ExecuteAsync(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK ||
                string.IsNullOrEmpty(response.Content))
            {
                return null;
            }

            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<JToken>>(response.Content);
            if (apiResponse?.IsSuccess == true && apiResponse.Data != null)
            {
                return apiResponse.Data.ToObject<SessionData>();
            }
            return null;

        }
    }
}
