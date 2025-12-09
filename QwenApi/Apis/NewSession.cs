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
    public class NewSession
    {
        public static async Task<NewReturn> ExecuteAsync(SessionReq reqdata)
        {
            var request = new RestRequest("/api/v2/chats/new", Method.Post);
            request = request.AddCommonHeaders()
                             .AddJsonBody(JsonConvert.SerializeObject(reqdata));

            var response = await Runtimes.restClient.ExecuteAsync(request);

            if (response.StatusCode != System.Net.HttpStatusCode.OK || string.IsNullOrEmpty(response.Content))
            {
                return null;
            }

            var apiResponse = JsonConvert.DeserializeObject<ApiResponse<JToken>>(response.Content);
            if (apiResponse?.IsSuccess == true && apiResponse.Data != null)
            {
                return apiResponse.Data.ToObject<NewReturn>();
            }
            return null;
        }
    }
}
