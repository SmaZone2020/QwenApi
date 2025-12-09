using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json.Linq;
using QwenApi.Apis;
using QwenApi.Models.ResponseM;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Timer = System.Threading.Timer;

namespace QwenAPIGUI
{
    public partial class Form1 : Form
    {
        private Timer? periodicTimer;

        public Form1()
        {
            InitializeComponent();
            this.Load += async (s, e) =>
            {
                await webView21.EnsureCoreWebView2Async(null);
            };

            webView21.CoreWebView2InitializationCompleted += (sender, e) =>
            {
                if (e.IsSuccess)
                {
                    webView21.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);

                    webView21.CoreWebView2.WebResourceRequested += async (s, args) =>
                    {
                        try
                        {
                            var headers = args.Request.Headers;
                            var watchKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "bx-umidtoken", "bx-ua" };

                            foreach (var h in headers)
                            {
                                if (!watchKeys.Contains(h.Key))
                                    continue;

                                var key = h.Key;
                                var value = h.Value ?? string.Empty;
                                var display = $"{key}: {value}";

                                Runtimes.UpdateTime = DateTime.Now;
                                if (key.Equals("bx-ua")) Runtimes.Bx_UA = value;
                                else if (key.Equals("bx-umidtoken")) Runtimes.Bx_Umidtoken = value;

                                Runtimes.Cookies = await GetAllCookiesAsStringAsync();
                                Debug.WriteLine($"{Runtimes.Bx_UA.Length},{Runtimes.Cookies.Length},{Runtimes.Bx_Umidtoken.Length}");
                                QwenApi.Runtimes.cfgMgr.LoadString(Runtimes.Bx_UA, Runtimes.Cookies, Runtimes.Bx_Umidtoken);

                                void addItem()
                                {
                                    if (!bxList.Items.Contains(display))
                                        bxList.Items.Add(display);
                                }
                                RefreshChat();
                                if (bxList.InvokeRequired)
                                {
                                    bxList.BeginInvoke((Action)addItem);
                                }
                                else
                                {
                                    addItem();
                                }


                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error in WebResourceRequested: {ex}");
                        }
                    };
                }

                if (periodicTimer == null)
                {
                    periodicTimer = new System.Threading.Timer(_ =>
                    {
                        if (webView21.InvokeRequired)
                        {
                            webView21.Invoke((MethodInvoker)delegate
                            {
                                DoReloadAndSetTitle();
                            });
                        }
                        else
                        {
                            DoReloadAndSetTitle();
                        }
                    }, null, dueTime: 0, period: 60000);
                }
                else
                {
                    periodicTimer.Change(0, 60000);
                }

                void DoReloadAndSetTitle()
                {
                    try
                    {
                        webView21.CoreWebView2.ExecuteScriptAsync("location.reload()");

                        this.Text = $"{DateTime.Now:MM-dd HH:mm:ss} 已更新Token";
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"UI Thread Timer error: {ex}");
                    }
                }
            };


        }

        private void button1_Click(object sender, EventArgs e)
        {
            webView21.CoreWebView2.ExecuteScriptAsync(@$"
    fetch(""https://chat.qwen.ai/api/v2/auths/signin"", {{
      method: ""POST"",
      headers: {{
        ""Content-Type"": ""application/json""
      }},
      body: JSON.stringify({{
        email: ""{emailInput.Text}"",
        password: ""{Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(passwordInput.Text)))}""
      }})
    }})
    .then(response => location.reload())
    .then(data => location.reload())
    .catch(error => console.error(""Error:"", error));
    ");
        }

        private async void refreshBtn_Click(object sender, EventArgs e)
        {
            await RefreshChat();
        }

        public async Task RefreshChat()
        {
            Runtimes.SessionItems = await QwenApi.Apis.GetSessionList.ExecuteAsync();

            chatList.Items.Clear();
            foreach (var item in Runtimes.SessionItems)
            {
                chatList.Items.Add($"{item.Title}     {item.ID}");
            }
        }

        private async Task<string> GetAllCookiesAsStringAsync(string uri = "https://chat.qwen.ai")
        {
            if (webView21?.CoreWebView2 == null) return null;

            try
            {
                uri ??= webView21.Source.ToString();

                var cookies = await webView21.CoreWebView2.CookieManager.GetCookiesAsync(uri);
                var cookieStrings = cookies.Select(c => $"{c.Name}={c.Value}");
                return string.Join("; ", cookieStrings);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"获取完整 Cookie 失败: {ex.Message}");
                return null;
            }
        }

        private async void chatList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (chatList.SelectedIndex != -1)
            {
                userInput.Enabled = true;
                sendMessageBtn.Enabled = true;

                var id = chatList.SelectedItem.ToString().Split("     ")[1];
                var ChatHis = await QwenApi.Apis.GetSessionHistory.ExecuteAsync(id);

                Runtimes.CurrentSession = ChatHis;

                chatHistory.Controls.Clear();

                if (ChatHis.Chat.History.Messages == null) return;

                foreach (var msg in ChatHis.Chat.History.Messages)
                {
                    var role = msg.Value.Role;
                    var content = msg.Value.Content;

                    if (role == "assistant")
                    {
                        foreach (var item in msg.Value.Content_List)
                        {
                            content += $"{item.Content}\n";
                        }
                    }

                    if (string.IsNullOrWhiteSpace(content)) continue;

                    Label? messageLabel = new()
                    {
                        AutoSize = false,
                        MaximumSize = new Size(chatHistory.Width - 20, 0),
                        Padding = new Padding(10),
                        Margin = new Padding(5),
                        Text = content,
                        UseMnemonic = false,
                        ForeColor = Color.Black,
                        BackColor = role == "user" ? Color.LightBlue : Color.LightGray,
                        TextAlign = role == "user"
                                        ? ContentAlignment.MiddleRight
                                        : ContentAlignment.MiddleLeft
                    };


                    messageLabel.Size = new Size(messageLabel.MaximumSize.Width, messageLabel.GetPreferredSize(Size.Empty).Height);


                    chatHistory.Controls.Add(messageLabel);
                }

                chatHistory.ScrollControlIntoView(chatHistory.Controls[chatHistory.Controls.Count - 1]);

            }
            else
            {
                userInput.Enabled = false;
                sendMessageBtn.Enabled = false;
            }
        }

        private async void newChatBtn_Click(object sender, EventArgs e)
        {
            await QwenApi.Apis.NewSession.ExecuteAsync(new());
            await RefreshChat();
            await Task.Delay(200);
            chatList.SelectedIndex = 0;
        }

        private async void sendMessageBtn_Click(object sender, EventArgs e)
        {
            string? parentId = Runtimes.CurrentSession.Chat.Messages.Count > 0
                    ? Runtimes.CurrentSession.Chat.Messages.Last().Id
                    : null;

            Label userMessageLabel = new()
            {
                AutoSize = false,
                MaximumSize = new Size(chatHistory.Width - 20, 0),
                Padding = new Padding(10),
                Margin = new Padding(5),
                Text = userInput.Text,
                UseMnemonic = false,
                ForeColor = Color.Black,
                BackColor = Color.LightBlue,
                TextAlign = ContentAlignment.MiddleRight
            };
            userMessageLabel.Size = new Size(userMessageLabel.MaximumSize.Width, userMessageLabel.GetPreferredSize(Size.Empty).Height);

            void addUserMsg()
            {
                chatHistory.Controls.Add(userMessageLabel);
                chatHistory.ScrollControlIntoView(userMessageLabel);
            }

            if (chatHistory.InvokeRequired) chatHistory.Invoke((Action)addUserMsg);
            else addUserMsg();

            Label assistantLabel = new()
            {
                AutoSize = false,
                MaximumSize = new Size(chatHistory.Width - 20, 0),
                Padding = new Padding(10),
                Margin = new Padding(5),
                Text = string.Empty,
                UseMnemonic = false,
                ForeColor = Color.Black,
                BackColor = Color.LightGray,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var prevUserInput = userInput.Text;
            userInput.Clear();
            sendMessageBtn.Enabled = false;

            try
            {
                bool assistantAdded = false;

                await foreach (string jsonData in SendMessage.ExecuteAsync(
                    chatId: Runtimes.CurrentSession.Id,
                    messageContent: prevUserInput,
                    parentId: parentId,
                    useThink: false))
                {
                    JToken token;
                    try
                    {
                        token = JToken.Parse(jsonData);
                    }
                    catch
                    {
                        continue;
                    }

                    if (token["choices"]?.Any() != true) continue;

                    var delta = token["choices"]?[0]?["delta"];
                    if (delta == null) continue;

                    var content = delta["content"]?.ToString() ?? string.Empty;
                    if (string.IsNullOrEmpty(content)) continue;

                    void updateAssistant()
                    {
                        if (!assistantAdded)
                        {
                            chatHistory.Controls.Add(assistantLabel);
                            assistantAdded = true;
                        }

                        assistantLabel.Text += content;
                        assistantLabel.Size = new Size(assistantLabel.MaximumSize.Width, assistantLabel.GetPreferredSize(Size.Empty).Height);
                        chatHistory.ScrollControlIntoView(assistantLabel);
                    }

                    if (chatHistory.InvokeRequired) chatHistory.Invoke((Action)updateAssistant);
                    else updateAssistant();
                }

                try
                {
                    var updated = await QwenApi.Apis.GetSessionHistory.ExecuteAsync(Runtimes.CurrentSession.Id);
                    Runtimes.CurrentSession = updated;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"刷新会话历史失败: {ex}");
                }
            }
            finally
            {
                sendMessageBtn.Enabled = true;
                userInput.Enabled = true;
            }
        }

        private void userInput_TextChanged(object sender, EventArgs e)
        {
            if (userInput.Text.Length > 0) sendMessageBtn.Enabled = true;
            else sendMessageBtn.Enabled = false;
        }
    }
}
