using QwenApi.Apis;
using QwenApi.Models.ResponseM;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace QwenApi.Services
{
    /// <summary>
    /// Qwen API服务接口，定义了与Qwen AI交互的核心方法
    /// </summary>
    public interface IQwenApiService
    {
        /// <summary>
        /// 获取可用的模型列表
        /// </summary>
        /// <returns>模型列表</returns>
        Task<QwenApi.Apis.QwenModelList> GetModelsAsync();

        /// <summary>
        /// 获取会话列表
        /// </summary>
        /// <returns>会话列表</returns>
        Task<List<SessionItem>> GetSessionsAsync();

        /// <summary>
        /// 获取会话历史记录
        /// </summary>
        /// <param name="sessionId">会话ID</param>
        /// <returns>会话历史数据</returns>
        Task<GetSessionHistory.SessionData> GetSessionHistoryAsync(string sessionId);

        /// <summary>
        /// 创建新会话
        /// </summary>
        /// <returns>新会话信息</returns>
        Task<NewReturn> CreateSessionAsync();

        /// <summary>
        /// 发送消息到指定会话
        /// </summary>
        /// <param name="chatId">会话ID</param>
        /// <param name="messageContent">消息内容</param>
        /// <param name="parentId">父消息ID</param>
        /// <param name="model">模型名称，默认为qwen3-max-2025-10-30</param>
        /// <param name="useThink">是否使用思考模式，默认为false</param>
        /// <param name="imgUrl">图片URL数组，默认为null</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>消息响应的流式数据</returns>
        IAsyncEnumerable<string> SendMessageAsync(string chatId, string messageContent, string? parentId, string model = "qwen3-max-2025-10-30", bool useThink = false, string[]? imgUrl = null, CancellationToken cancellationToken = default);
    }
}
