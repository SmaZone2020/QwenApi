using Newtonsoft.Json;
using QwenApi.Helper;
using QwenApi.Models.ResponseM;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QwenApi.Apis
{
    public static class GetSessionList
    {
        public static async Task<List<SessionItem>> ExecuteAsync()
        {
            var page = 1;
            var allItems = new List<SessionItem>();

            while (true)
            {
                var request = new RestRequest($"/api/v2/chats/?page={page}", Method.Get);
                request = request.AddCommonHeaders();

                var response = await Runtimes.restClient.ExecuteAsync(request);

                if (response.StatusCode != System.Net.HttpStatusCode.OK || string.IsNullOrEmpty(response.Content))
                {
                    break;
                }

                try
                {
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<SessionItem>>>(response.Content);
                    if (apiResponse?.Data == null || apiResponse.Data.Count == 0)
                    {
                        break;
                    }

                    allItems.AddRange(apiResponse.Data);
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
