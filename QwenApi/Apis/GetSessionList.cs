using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QwenApi.Helper;
using QwenApi.Models.ResponseM;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace QwenApi.Apis
{
    public static class GetSessionList
    {
        public static async Task<List<SessionItem>> ExecuteAsync()
        {
            var page = 1;
            const int maxPages = 100;
            var allItems = new List<SessionItem>();

            while (page <= maxPages)
            {
                try
                {
                    var request = new RestRequest($"/api/v2/chats/?page={page}", Method.Get);
                    request = request.AddCommonHeaders();

                    var response = await Runtimes.restClient.ExecuteAsync(request);

                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        Debug.WriteLine($"[GetSessionList] HTTP {response.StatusCode} on page {page}");
                        break;
                    }

                    if (string.IsNullOrWhiteSpace(response.Content))
                    {
                        Debug.WriteLine($"[GetSessionList] Empty response on page {page}");
                        break;
                    }

                    var jObj = JObject.Parse(response.Content);

                    if (jObj["success"]?.Value<bool>() != true)
                    {
                        var errorMsg = jObj["data"]?["message"]?.ToString()
                                    ?? jObj["message"]?.ToString()
                                    ?? "NONE";
                        break;
                    }

                    var dataToken = jObj["data"];
                    if (dataToken == null)
                    {
                        break;
                    }

                    if (dataToken.Type != JTokenType.Array)
                    {
                        break;
                    }

                    var items = dataToken.ToObject<List<SessionItem>>();
                    if (items == null || items.Count == 0)
                    {
                        break;
                    }

                    allItems.AddRange(items);
                    page++;
                }
                catch
                {
                    break;
                }
            }

            return allItems;
        }
    }
}