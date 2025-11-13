using QwenApi;
using QwenApi.Apis;

Console.WriteLine($"加载配置: {Runtimes.cfgMgr.Load()}");
Console.WriteLine($"{Runtimes.cfgMgr.BxUa.Length},{Runtimes.cfgMgr.Cookie.Length},{Runtimes.cfgMgr.BxUmidtoken.Length}");
if (!Runtimes.cfgMgr.IsConfigured)
{
    Console.WriteLine("配置不完整，请检查 config/config.txt");
    return;
}

var sessions = await GetSessionList.ExecuteAsync();
if (sessions == null || sessions.Count == 0)
{
    Console.WriteLine("未能获取会话列表");
    return;
}

Console.WriteLine("会话列表:");
for (int i = 0; i < sessions.Count; i++)
{
    Console.WriteLine($"{i}: {sessions[i].Title} ({sessions[i].ID})");
}
Console.WriteLine("\n请输入会话序号、ID，或输入 'new' 创建新对话，'exit' 退出:");

GetSessionHistory.SessionData? currentSession = null;
string? input = Console.ReadLine()?.Trim();

if (string.IsNullOrEmpty(input) || input.Equals("exit", StringComparison.OrdinalIgnoreCase))
{
    return;
}

if (input.Equals("new", StringComparison.OrdinalIgnoreCase))
{
    var newSessionResp = await NewSession.ExecuteAsync(new());
    if (newSessionResp == null)
    {
        Console.WriteLine("创建新会话失败");
        return;
    }

    currentSession = await GetSessionHistory.ExecuteAsync(newSessionResp.id);
    if (currentSession == null)
    {
        Console.WriteLine("无法加载新会话");
        return;
    }
}
else
{
    string selectedId;
    if (int.TryParse(input, out int index) && index >= 0 && index < sessions.Count)
    {
        selectedId = sessions[index].ID;
    }
    else if (Guid.TryParse(input, out _) && sessions.Any(s => s.ID == input))
    {
        selectedId = input;
    }
    else
    {
        Console.WriteLine("无效输入：请提供有效序号、ID，或 'new'");
        return;
    }

    currentSession = await GetSessionHistory.ExecuteAsync(selectedId);
    if (currentSession == null)
    {
        Console.WriteLine($"无法加载会话: {selectedId}");
        return;
    }
}

Console.Title = $"Qwen - {currentSession.Title} - {currentSession.Id}";
Console.WriteLine($"\n已加载会话: {currentSession.Title} ({currentSession.Id}) 共 {currentSession.Chat.Messages.Count} 条消息");

while (true)
{
    Console.Write(">>> ");
    var userInput = Console.ReadLine()?.Trim();

    if (string.IsNullOrEmpty(userInput))
        continue;

    if (userInput.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
        userInput.Equals("quit", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }

    string? parentId = currentSession.Chat.Messages.Count > 0
        ? currentSession.Chat.Messages.Last().Id
        : null;

    try
    {
        await foreach (string jsonData in SendMessage.ExecuteAsync(
            chatId: currentSession.Id,
            messageContent: userInput,
            parentId: parentId,
            useThink: false))
        {
            try
            {
                var token = Newtonsoft.Json.Linq.JToken.Parse(jsonData);

                if (token["choices"]?.Any() != true) continue;

                var delta = token["choices"]?[0]?["delta"];
                if (delta == null) continue;

                var content = delta["content"]?.ToString() ?? "";
                var phase = delta["phase"]?.ToString()?.ToLowerInvariant() ?? "answer";
                var status = delta["status"]?.ToString();

                if (string.IsNullOrEmpty(content))
                {
                    if (status == "finished")
                    {
                        Console.WriteLine("\n[回答完成]");
                    }
                    continue;
                }

                var original = Console.ForegroundColor;
                Console.ForegroundColor = phase switch
                {
                    "think" => ConsoleColor.Yellow,
                    "answer" => ConsoleColor.Green,
                    _ => original
                };

                Console.Write(content);
                Console.ForegroundColor = original;

                if (status == "finished")
                    Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[解析错误] {ex.Message}");
            }
        }

        currentSession = await GetSessionHistory.ExecuteAsync(currentSession.Id);
        if (currentSession == null)
        {
            Console.WriteLine("会话已失效");
            break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n[请求失败] {ex.Message}");
    }

    Console.WriteLine();
}