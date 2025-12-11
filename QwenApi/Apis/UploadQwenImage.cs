using Newtonsoft.Json;
using QwenApi.Helper;
using QwenApi.Models.ResponseM;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace QwenApi.Apis
{
    public class UploadQwenImage
    {
        public static async Task<string?> ExecuteAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Image file not found.", filePath);

            var fileBytes = await File.ReadAllBytesAsync(filePath);
            var fileName = Path.GetFileName(filePath);

            var stsToken = await GetStsTokenAsync(fileBytes, fileName);
            if (stsToken == null) return null;

            var canonicalPath = "/" + stsToken.FilePath;

            var ossUrl = stsToken.FileUrl.Split('?')[0];
            var uri = new Uri(ossUrl);
            var host = uri.Host;

            var date = DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ", CultureInfo.InvariantCulture);
            var mimeType = GetMimeType(Path.GetExtension(fileName).ToLowerInvariant());

            var headers = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["content-type"] = mimeType,
                ["host"] = host,
                ["x-oss-content-sha256"] = "UNSIGNED-PAYLOAD",
                ["x-oss-date"] = date,
                ["x-oss-security-token"] = stsToken.SecurityToken
            };

            var signedHeaders = string.Join(";", headers.Keys.OrderBy(k => k));

            var canonicalRequest = new StringBuilder();
            canonicalRequest.Append("PUT\n");
            canonicalRequest.Append(canonicalPath + "\n");
            canonicalRequest.Append("\n"); // empty query

            foreach (var key in headers.Keys.OrderBy(k => k))
            {
                canonicalRequest.Append($"{key}:{headers[key]}\n");
            }
            canonicalRequest.Append("\n"); // end of headers
            canonicalRequest.Append(signedHeaders + "\n");
            canonicalRequest.Append("UNSIGNED-PAYLOAD");

            var cr = canonicalRequest.ToString();
            Console.WriteLine("---- YOUR CANONICAL REQUEST ----");
            Console.WriteLine(cr);
            Console.WriteLine("---- END ----");

            var canonicalRequestHash = ToHexString(Sha256Hash(cr));

            var region = stsToken.Region.Replace("oss-", ""); 
            var credentialScope = $"{date.Substring(0, 8)}/{region}/oss/aliyun_v4_request";
            var stringToSign = $"OSS4-HMAC-SHA256\n{date}\n{credentialScope}\n{canonicalRequestHash}";

            // Signing Key
            var kSecret = Encoding.UTF8.GetBytes("aliyun_v4" + stsToken.AccessKeySecret);
            var kDate = HmacSha256(Encoding.UTF8.GetBytes(date.Substring(0, 8)), kSecret);
            var kRegion = HmacSha256(Encoding.UTF8.GetBytes(region), kDate);
            var kService = HmacSha256(Encoding.UTF8.GetBytes("oss"), kRegion);
            var kSigning = HmacSha256(Encoding.UTF8.GetBytes("aliyun_v4_request"), kService);

            var signature = ToHexString(HmacSha256(Encoding.UTF8.GetBytes(stringToSign), kSigning));
            var authHeader = $"OSS4-HMAC-SHA256 Credential={stsToken.AccessKeyId}/{credentialScope},Signature={signature}";

            // 发送请求
            using var request = new HttpRequestMessage(HttpMethod.Put, ossUrl);
            request.Headers.TryAddWithoutValidation("Authorization", authHeader);
            request.Headers.TryAddWithoutValidation("x-oss-content-sha256", "UNSIGNED-PAYLOAD");
            request.Headers.TryAddWithoutValidation("x-oss-date", date);
            request.Headers.TryAddWithoutValidation("x-oss-security-token", stsToken.SecurityToken);

            var content = new StreamContent(new MemoryStream(fileBytes));
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mimeType);
            request.Content = content;

            using var client = new HttpClient();
            var response = await client.SendAsync(request);

            return response.IsSuccessStatusCode ? stsToken.FileId : null;
        }

        private static async Task<StsTokenResponseData?> GetStsTokenAsync(byte[] fileBytes, string fileName)
        {
            var request = new RestRequest("/api/v2/files/getstsToken", Method.Post);
            request.AddCommonHeaders();
            request.AddJsonBody(new
            {
                filename = fileName,
                filesize = fileBytes.Length,
                filetype = "image"
            });

            var response = await Runtimes.restClient.ExecuteAsync(request);
            if (response.StatusCode != HttpStatusCode.OK || string.IsNullOrEmpty(response.Content))
                return null;

            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<StsTokenResponseData>>(response.Content);
            return apiResponse?.IsSuccess == true ? apiResponse.Data : null;
        }

        private static string GetMimeType(string extension)
        {
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }

        private static byte[] HmacSha256(byte[] key, byte[] data)
        {
            using var hmac = new HMACSHA256(key);
            return hmac.ComputeHash(data);
        }

        private static byte[] Sha256Hash(string input)
        {
            return SHA256.HashData(Encoding.UTF8.GetBytes(input));
        }

        private static string ToHexString(byte[] bytes)
        {
            return Convert.ToHexStringLower(bytes);
        }
    }

    public class StsTokenResponseData
    {
        [JsonProperty("bucketname")]
        public string BucketName { get; set; } = "qwen-webui-prod";

        [JsonProperty("access_key_id")]
        public string AccessKeyId { get; set; } = null!;

        [JsonProperty("access_key_secret")]
        public string AccessKeySecret { get; set; } = null!;

        [JsonProperty("security_token")]
        public string SecurityToken { get; set; } = null!;

        [JsonProperty("file_url")]
        public string FileUrl { get; set; } = null!;

        [JsonProperty("file_id")]
        public string FileId { get; set; } = null!;

        [JsonProperty("region")]
        public string Region { get; set; } = null!;

        [JsonProperty("file_path")]
        public string FilePath { get; set; } = null!;
    }

    public class ApiResponse<T>
    {
        [JsonProperty("success")]
        public bool IsSuccess { get; set; }

        [JsonProperty("data")]
        public T? Data { get; set; }
    }
}