using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using QwenApi.Helper;
using QwenApi.Models.RequestM;
using QwenApi.Models.ResponseM;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace QwenApi.Apis
{

    public class ChatCompletionRequest
    {
        [JsonProperty("stream")]
        public bool Stream { get; set; } = true;

        [JsonProperty("incremental_output")]
        public bool IncrementalOutput { get; set; } = true;

        [JsonProperty("chat_id")]
        public string ChatId { get; set; }

        [JsonProperty("chat_mode")]
        public string ChatMode { get; set; } = "normal";

        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("parent_id")]
        public string ParentId { get; set; }

        [JsonProperty("messages")]
        public List<ChatMessageRequest> Messages { get; set; }

        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }
    }

    public class ChatMessageRequest
    {
        [JsonProperty("fid")]
        public string Fid { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("parentId")]
        public string ParentId { get; set; }

        [JsonProperty("childrenIds")]
        public List<string> ChildrenIds { get; set; } = new List<string>();

        [JsonProperty("role")]
        public string Role { get; set; } = "user";

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("user_action")]
        public string UserAction { get; set; } = "chat";

        [JsonProperty("files")]
        public List<QwenFile> Files { get; set; } = [];

        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty("models")]
        public List<string> Models { get; set; }

        [JsonProperty("chat_type")]
        public string ChatType { get; set; } = "t2t";

        [JsonProperty("feature_config")]
        public FeatureConfig FeatureConfig { get; set; }

        [JsonProperty("extra")]
        public ExtraRequest Extra { get; set; }

        [JsonProperty("sub_chat_type")]
        public string SubChatType { get; set; } = "t2t";

        [JsonProperty("parent_id")]
        public string Parent_id { get; set; }
    }
    public class QwenFile
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("file")]
        public QwenFileInfo File { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("collection_name")]
        public string CollectionName { get; set; }

        [JsonProperty("progress")]
        public int Progress { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("greenNet")]
        public string GreenNet { get; set; }

        [JsonProperty("size")]
        public int Size { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("itemId")]
        public string ItemId { get; set; }

        [JsonProperty("file_type")]
        public string FileType { get; set; }

        [JsonProperty("showType")]
        public string ShowType { get; set; }

        [JsonProperty("file_class")]
        public string FileClass { get; set; }

        [JsonProperty("uploadTaskId")]
        public string UploadTaskId { get; set; }
    }

    public class QwenFileInfo
    {
        [JsonProperty("created_at")]
        public long CreatedAt { get; set; }

        [JsonProperty("data")]
        public object Data { get; set; }

        [JsonProperty("filename")]
        public string Filename { get; set; }

        [JsonProperty("hash")]
        public object Hash { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("meta")]
        public QwenFileMeta Meta { get; set; }

        [JsonProperty("update_at")]
        public long UpdateAt { get; set; }
    }

    public class QwenFileMeta
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("size")]
        public int Size { get; set; }

        [JsonProperty("content_type")]
        public string ContentType { get; set; }
    }

    public class FeatureConfig
    {
        [JsonProperty("thinking_enabled")]
        public bool ThinkingEnabled { get; set; } = true;

        [JsonProperty("output_schema")]
        public string OutputSchema { get; set; } = "phase";

        [JsonProperty("instructions")]
        public object Instructions { get; set; } = null;

        [JsonProperty("thinking_budget")]
        public long ThinkingBudget { get; set; } = 81920;

        [JsonProperty("research_mode")]
        public string ResearchMode { get; set; } = "normal";
    }

    public class ExtraRequest
    {
        [JsonProperty("meta")]
        public MetaContainer Meta { get; set; }
    }

    public class MetaContainer
    {
        [JsonProperty("subChatType")]
        public string SubChatType { get; set; } = "t2t";
    }

    public class SendMessage
    {
        public static async IAsyncEnumerable<string> ExecuteAsync(
            string chatId,
            string messageContent,
            string? parentId,
            string model = "qwen3-max-2025-10-30",
            bool useThink = false,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var chatMessage = new ChatMessageRequest
            {
                ParentId = parentId,
                Parent_id = parentId,
                Content = messageContent,
                Timestamp = timestamp,
                Models = [model],
                FeatureConfig = new() { ThinkingEnabled = useThink },
                Extra = new()
                {
                    Meta = new() { SubChatType = "t2t" }
                }
            };

            var chatReq = new ChatCompletionRequest
            {
                ChatId = chatId,
                Model = model,
                ParentId = parentId,
                Messages = [chatMessage],
                Timestamp = timestamp
            };

            var request = new HttpRequestMessage(HttpMethod.Post, $"{Runtimes.BaseUrl}api/v2/chat/completions?chat_id={chatId}")
            {
                Content = new StringContent(JsonConvert.SerializeObject(chatReq), Encoding.UTF8, "application/json")
            };

            request.AddCommonHeaders();

            using var httpClient = new HttpClient();
            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);

            string line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (line.StartsWith("data: ", StringComparison.Ordinal))
                {
                    var json = line["data: ".Length..].Trim();
                    if (!string.IsNullOrEmpty(json))
                        yield return json;
                }
            }
        }

        public static QwenFile? CreatFile(string fileName)
        {
            if (!File.Exists(fileName)) return null;
            return new()
            {
                Type = GetFileType(fileName),
                Url = fileName,
            };

        }

        public static string GetFileType(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return "file";

            string extension = Path.GetExtension(filePath);
            if (ImageExtensions.Contains(extension))
                return "image";

            return "file";
        }

        private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".tif", ".webp", ".svg", ".ico"
        };

    }
}
