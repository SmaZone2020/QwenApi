using Newtonsoft.Json;
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
    public class RemoveResult
    {
        public bool status { get; set; }
    }
    internal class RemoveSession
    {
        public static async Task<RemoveResult> ExecuteAsync(string id)
        {
            var request = new RestRequest($"/api/v2/chats/{id}", Method.Delete);
            request = request.AddCommonHeaders();

            var response = await Runtimes.restClient.ExecuteAsync(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK || string.IsNullOrEmpty(response.Content))
            {
                return null;
            }

            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<RemoveResult>>(response.Content);
            return apiResponse?.Data;
        }
    }
}
