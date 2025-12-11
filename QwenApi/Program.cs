using QwenApi;
using QwenApi.Apis;
using Spectre.Console;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

Console.WriteLine($"加载配置: {Runtimes.cfgMgr.Load()}");
Console.WriteLine($"{Runtimes.cfgMgr.BxUa.Length},{Runtimes.cfgMgr.Cookie.Length},{Runtimes.cfgMgr.BxUmidtoken.Length}");
if (!Runtimes.cfgMgr.IsConfigured)
{
    AnsiConsole.MarkupLine("[red]配置不完整，请检查 config/config.txt[/]");
    return;
}

var qwenModels = await GetQwenModels.ExecuteAsync();
if (qwenModels == null || qwenModels.Data.Count == 0)
{
    AnsiConsole.MarkupLine("[red]未能获取模型列表[/]");
    return;
}
else
{
    AnsiConsole.MarkupLine("[#FFA500]模型列表:[/]");
    var modelTable = new Table().Border(TableBorder.Rounded).Expand();
    modelTable.AddColumn("[#2EC4B6]索引[/]");
    modelTable.AddColumn("[#2EC4B6]ID[/]");
    modelTable.AddColumn("[#2EC4B6]MCP[/]");
    modelTable.AddColumn("[#2EC4B6]更新时间[/]");
    for (int i = 0; i < qwenModels.Data.Count; i++)
    {
        var x = qwenModels.Data[i];
        modelTable.AddRow(
            $"[grey]{i}[/]",
            $"[#2EC4B6]{Markup.Escape($"{x.ID}")}[/]",
            $"[#2EC4B6]{Markup.Escape($"{string.Join(",", x.Info.MetaData.MCP)}")}[/]",
            $"[grey]{GetQwenModels.GetDateForTimestamp(x.Info.UpdateAt):yyyy-MM-dd HH:mm:ss}[/]"
        );
    }
    AnsiConsole.Write(modelTable);
}

var sessions = await GetSessionList.ExecuteAsync();
if (sessions == null || sessions.Count == 0)
{
    AnsiConsole.MarkupLine("[red]未能获取会话列表[/]");
    return;
}

AnsiConsole.MarkupLine("\n[#7B61FF]会话列表:[/]");
var sessionTable = new Table().Border(TableBorder.Rounded).Expand();
sessionTable.AddColumn("[#7B61FF]序号[/]");
sessionTable.AddColumn("[#7B61FF]标题[/]");
sessionTable.AddColumn("[#7B61FF]ID[/]");
sessionTable.AddColumn("[#7B61FF]更新时间[/]");
for (int i = 0; i < sessions.Count; i++)
{
    sessionTable.AddRow(
        $"[grey]{i}[/]",
        $"[#7B61FF]{Markup.Escape($"{sessions[i].Title}")}[/]",
        $"[grey]{Markup.Escape($"{sessions[i].ID}")}[/]",
        $"[grey]{Markup.Escape($"{GetQwenModels.GetDateForTimestamp(sessions[i].UpdatedAt):yyyy-MM-dd HH:mm:ss}")}[/]"
    );
}
AnsiConsole.Write(sessionTable);

AnsiConsole.MarkupLine("\n[#FFA500]请输入会话序号、ID，或输入 'new' 创建新对话，'exit' 退出:[/]");
GetSessionHistory.SessionData? currentSession = null;
string? input = AnsiConsole.Prompt(new TextPrompt<string>("[#FFA500]>[/] ").AllowEmpty());

if (string.IsNullOrEmpty(input) || input.Equals("exit", StringComparison.OrdinalIgnoreCase))
{
    return;
}

if (input.Equals("new", StringComparison.OrdinalIgnoreCase))
{
    var newSessionResp = await NewSession.ExecuteAsync(new());
    if (newSessionResp == null)
    {
        AnsiConsole.MarkupLine("[red]创建新会话失败[/]");
        return;
    }

    currentSession = await GetSessionHistory.ExecuteAsync(newSessionResp.id);
    if (currentSession == null)
    {
        AnsiConsole.MarkupLine("[red]无法加载新会话[/]");
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
        AnsiConsole.MarkupLine("[red]无效输入：请提供有效序号、ID，或 'new'[/]");
        return;
    }

    currentSession = await GetSessionHistory.ExecuteAsync(selectedId);
    if (currentSession == null)
    {
        AnsiConsole.MarkupLine($"[red]无法加载会话: {Markup.Escape(selectedId)}[/]");
        return;
    }
}

Console.Title = $"Qwen - {currentSession.Title} - {currentSession.Id}";
AnsiConsole.MarkupLine($"\n[#2EC4B6]已加载会话:[/] [#7B61FF]{Markup.Escape(currentSession.Title)}[/] ([grey]{currentSession.Id}[/]) 共 [grey]{currentSession.Chat.Messages.Count}[/] 条消息");

while (true)
{
    var userInput = AnsiConsole.Prompt(new TextPrompt<string>("[#FFA500]>>>[/] ").AllowEmpty()).Trim();

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
                var token = JToken.Parse(jsonData);

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
                        //输出结束后操作，留空，还不知道用来干啥
                    }
                    continue;
                }

                var color = phase switch
                {
                    "think" => "#FFD54F",
                    "answer" => "#66BB6A",
                    _ => "grey"
                };

                AnsiConsole.Write(new Markup($"[{color}]{Markup.Escape(content)}[/]"));

                if (status == "finished")
                    AnsiConsole.WriteLine();
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]\n解析错误 {Markup.Escape(ex.Message)}[/]");
            }
        }

        currentSession = await GetSessionHistory.ExecuteAsync(currentSession.Id);
        if (currentSession == null)
        {
            AnsiConsole.MarkupLine("[red]会话已失效[/]");
            break;
        }
    }
    catch (Exception ex)
    {
        AnsiConsole.MarkupLine($"[red]\n请求失败 {Markup.Escape(ex.Message)}[/]");
    }

    AnsiConsole.WriteLine();
}