using Microsoft.AspNetCore.Mvc;
using QwenApi.Apis;
using QwenApi.Common;
using QwenApi.Models.RequestM;
using QwenApi.Models.ResponseM;
using QwenApi.Services;
using static QwenApi.Apis.GetSessionHistory;

namespace QwenWebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MainController(ILogger<MainController> logger, IQwenApiService qwenApiService) : ControllerBase
    {
        private readonly ILogger<MainController> _logger = logger;
        private readonly IQwenApiService _qwenApiService = qwenApiService;

        private bool IsAuthValid()
        {
            if (!Request.Headers.TryGetValue("Auth", out var authHeader))
                return false;

            if (string.IsNullOrEmpty(Runtimes.AuthToken))
                return false;

            return string.Equals(authHeader.ToString(), Runtimes.AuthToken, StringComparison.Ordinal);
        }

        [HttpGet("sessions")]
        public async Task<ActionResult<List<SessionItem>>> GetSessionList()
        {
            if (!IsAuthValid())
                return Unauthorized("��Ч��Auth����ͷ");

            if (!QwenApi.Common.Runtimes.cfgMgr.IsConfigured)
                return BadRequest("����δ��ɣ����� config/config.json");

            var sessions = await _qwenApiService.GetSessionsAsync();
            if (sessions == null)
                return StatusCode(500, "�޷���ȡ�Ự�б�");
            return Ok(sessions);
        }

        [HttpGet("models")]
        public async Task<ActionResult<List<QwenModelItem>>> GetModelList()
        {
            if (!IsAuthValid())
                return Unauthorized("��Ч��Auth����ͷ");

            var models = await _qwenApiService.GetModelsAsync();
            if (models == null)
                return StatusCode(500, "�޷���ȡģ���б�");
            return Ok(models.Data);
        }

        [HttpPost("sessions")]
        public async Task<ActionResult<SessionData>> CreateSession()
        {
            if (!IsAuthValid())
                return Unauthorized("��Ч��Auth����ͷ");

            if (!QwenApi.Common.Runtimes.cfgMgr.IsConfigured)
                return BadRequest("����δ���");

            var resp = await _qwenApiService.CreateSessionAsync();
            if (resp?.id == null)
                return StatusCode(500, "�����Ựʧ��");

            var session = await _qwenApiService.GetSessionHistoryAsync(resp.id);
            if (session == null)
                return StatusCode(500, "�޷������»Ự");

            return Ok(session);
        }

        [HttpGet("sessions/{sessionId}")]
        public async Task<ActionResult<SessionData>> GetSessionHistory(string sessionId)
        {
            if (!IsAuthValid())
                return Unauthorized("��Ч��Auth����ͷ");

            if (!Guid.TryParse(sessionId, out _))
                return BadRequest("��Ч�ĻỰID");

            var session = await _qwenApiService.GetSessionHistoryAsync(sessionId);
            if (session == null)
                return NotFound("�Ự�����ڻ���ʧЧ");

            return Ok(session);
        }

        [HttpPost("sessions/{sessionId}/messages")]
        public async Task SendMessage(
            string sessionId,
            [FromBody] SendMessageRequest request)
        {
            if (!IsAuthValid())
            {
                Response.StatusCode = 401;
                await Response.WriteAsync("��Ч��Auth����ͷ");
                return;
            }

            if (!QwenApi.Common.Runtimes.cfgMgr.IsConfigured)
            {
                Response.StatusCode = 400;
                await Response.WriteAsync("����δ���");
                return;
            }

            if (string.IsNullOrEmpty(request.Content))
            {
                Response.StatusCode = 400;
                await Response.WriteAsync("��Ϣ���ݲ���Ϊ��");
                return;
            }

            string? parentId = null;
            var history = await _qwenApiService.GetSessionHistoryAsync(sessionId);
            if (history?.Chat?.Messages?.Count > 0)
                parentId = history.Chat.Messages.Last().Id;

            Response.Headers.Append("Content-Type", "text/plain; charset=utf-8");
            Response.Headers.Append("Cache-Control", "no-cache");
            Response.Headers.Append("Connection", "keep-alive");

            try
            {
                await foreach (string jsonData in _qwenApiService.SendMessageAsync(
                    chatId: sessionId,
                    messageContent: request.Content,
                    parentId: parentId,
                    useThink: false,
                    imgUrl: request.FileUrl))
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
        public string[]? FileUrl {  get; set; } = null;
    }
}