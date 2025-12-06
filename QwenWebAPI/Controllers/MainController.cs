using Microsoft.AspNetCore.Mvc;
using QwenApi;
using QwenApi.Apis;
using QwenApi.Models.RequestM;
using QwenApi.Models.ResponseM;
using static QwenApi.Apis.GetSessionHistory;

namespace QwenWebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MainController(ILogger<MainController> logger) : ControllerBase
    {
        private readonly ILogger<MainController> _logger = logger;

        [HttpGet("sessions")]
        public async Task<ActionResult<List<SessionItem>>> GetSessionList()
        {
            if (!Runtimes.cfgMgr.IsConfigured)
                return BadRequest("配置未完成，请检查 config/config.txt");

            var sessions = await QwenApi.Apis.GetSessionList.ExecuteAsync();
            if (sessions == null)
                return StatusCode(500, "无法获取会话列表");
            return Ok(sessions);
        }

        [HttpPost("sessions")]
        public async Task<ActionResult<SessionData>> CreateSession()
        {
            if (!Runtimes.cfgMgr.IsConfigured)
                return BadRequest("配置未完成");

            var resp = await NewSession.ExecuteAsync(new SessionReq());
            if (resp?.id == null)
                return StatusCode(500, "创建会话失败");

            var session = await QwenApi.Apis.GetSessionHistory.ExecuteAsync(resp.id);
            if (session == null)
                return StatusCode(500, "无法加载新会话");

            return Ok(session);
        }

        [HttpGet("sessions/{sessionId}")]
        public async Task<ActionResult<SessionData>> GetSessionHistory(string sessionId)
        {
            if (!Guid.TryParse(sessionId, out _))
                return BadRequest("无效的会话ID");

            var session = await QwenApi.Apis.GetSessionHistory.ExecuteAsync(sessionId);
            if (session == null)
                return NotFound("会话不存在或已失效");

            return Ok(session);
        }

        [HttpPost("sessions/{sessionId}/messages")]
        public async Task SendMessage(
            string sessionId,
            [FromBody] SendMessageRequest request)
        {
            if (!Runtimes.cfgMgr.IsConfigured)
            {
                Response.StatusCode = 400;
                await Response.WriteAsync("配置未完成");
                return;
            }

            if (string.IsNullOrEmpty(request.Content))
            {
                Response.StatusCode = 400;
                await Response.WriteAsync("消息内容不能为空");
                return;
            }

            string? parentId = null;
            var history = await QwenApi.Apis.GetSessionHistory.ExecuteAsync(sessionId);
            if (history?.Chat?.Messages?.Count > 0)
                parentId = history.Chat.Messages.Last().Id;

            Response.Headers.Append("Content-Type", "text/plain; charset=utf-8");
            Response.Headers.Append("Cache-Control", "no-cache");
            Response.Headers.Append("Connection", "keep-alive");

            try
            {
                await foreach (string jsonData in QwenApi.Apis.SendMessage.ExecuteAsync(
                    chatId: sessionId,
                    messageContent: request.Content,
                    parentId: parentId,
                    useThink: false))
                {
                    try
                    {
                        var token = Newtonsoft.Json.Linq.JToken.Parse(jsonData);
                        var delta = token["choices"]?[0]?["delta"];
                        if (delta == null) continue;

                        var content = delta["content"]?.ToString() ?? "";
                        var status = delta["status"]?.ToString();

                        if (!string.IsNullOrEmpty(content))
                        {
                            await Response.WriteAsync(content);
                            await Response.Body.FlushAsync();
                        }

                        if (status == "finished")
                        {
                            await Response.Body.FlushAsync();
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        await Response.WriteAsync($"\n[ERROR]{ex.Message}\n");
                        await Response.Body.FlushAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                await Response.WriteAsync($"\n[REQUEST_ERROR]{ex.Message}\n");
                await Response.Body.FlushAsync();
            }
        }
    }

    public class SendMessageRequest
    {
        public string Content { get; set; } = string.Empty;
    }
}