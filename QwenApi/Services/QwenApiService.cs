using QwenApi.Apis;
using QwenApi.Models.ResponseM;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace QwenApi.Services
{
    public class QwenApiService : IQwenApiService
    {
        public async Task<QwenApi.Apis.QwenModelList> GetModelsAsync()
        {
            return await GetQwenModels.ExecuteAsync();
        }

        public async Task<List<SessionItem>> GetSessionsAsync()
        {
            return await GetSessionList.ExecuteAsync();
        }

        public async Task<GetSessionHistory.SessionData> GetSessionHistoryAsync(string sessionId)
        {
            return await GetSessionHistory.ExecuteAsync(sessionId);
        }

        public async Task<NewReturn> CreateSessionAsync()
        {
            return await NewSession.ExecuteAsync(new());
        }

        public IAsyncEnumerable<string> SendMessageAsync(string chatId, string messageContent, string? parentId, string model = "qwen3-max-2025-10-30", bool useThink = false, string[]? imgUrl = null, CancellationToken cancellationToken = default)
        {
            return SendMessage.ExecuteAsync(chatId, messageContent, parentId, model, useThink, imgUrl, cancellationToken);
        }
    }
}
