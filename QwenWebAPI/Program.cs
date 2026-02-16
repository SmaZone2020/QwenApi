using QwenApi.Common;
using QwenApi.Services;
using QwenWebAPI;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

// Add services to the container.
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 注册依赖注入服务
builder.Services.AddSingleton<IQwenApiService, QwenApiService>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowAll");
System.Threading.Timer? configRefreshTimer = null;

// 初始化WebAPI配置
InitializeWebApiConfig();

if (args.Length > 0)
{
    Console.WriteLine($"��ʹ�� {args[0]} ��Ϊ��֤Դ");

    configRefreshTimer = new System.Threading.Timer(async _ =>
    {
        try
        {
            var client = new RestClient(args[0]);
            var request = new RestRequest("/token", Method.Get);
            var response = await client.ExecuteAsync(request);

            Console.WriteLine(response.ContentLength);

            var lines = response.Content.Split(Environment.NewLine);
            QwenApi.Common.Runtimes.cfgMgr.LoadString(lines[0], lines[1], lines[2]);
            Console.WriteLine($"Load {QwenApi.Common.Runtimes.cfgMgr.BxUa.Length},{QwenApi.Common.Runtimes.cfgMgr.Cookie.Length},{QwenApi.Common.Runtimes.cfgMgr.BxUmidtoken.Length}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"����ˢ��ʧ��: {ex}");
        }
    },
    null,
    TimeSpan.FromSeconds(5),
    TimeSpan.FromMinutes(30));
}
else
{
    Console.WriteLine($"��������: {QwenApi.Common.Runtimes.cfgMgr.Load()}");
    Console.WriteLine($"{QwenApi.Common.Runtimes.cfgMgr.BxUa.Length},{QwenApi.Common.Runtimes.cfgMgr.Cookie.Length},{QwenApi.Common.Runtimes.cfgMgr.BxUmidtoken.Length}");
    if (!QwenApi.Common.Runtimes.cfgMgr.IsConfigured)
    {
        Console.WriteLine("���ò����������� config/config.json");
        Environment.Exit(1);
    }
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

void InitializeWebApiConfig()
{
    // 加载WebAPI配置
    QwenWebAPI.Runtimes.ConfigManager.Load();
    
    // 如果AuthToken为空，生成一个随机的
    if (string.IsNullOrEmpty(QwenWebAPI.Runtimes.AuthToken))
    {
        QwenWebAPI.Runtimes.AuthToken = GenerateRandomAuthToken();
        QwenWebAPI.Runtimes.ConfigManager.AuthToken = QwenWebAPI.Runtimes.AuthToken;
        QwenWebAPI.Runtimes.ConfigManager.Save();
        Console.WriteLine($"身份验证Token: {QwenWebAPI.Runtimes.AuthToken}");
    }
    else
    {
        Console.WriteLine($"已加载Token");
    }
}

string GenerateRandomAuthToken()
{
    using var rng = RandomNumberGenerator.Create();
    var bytes = new byte[32];
    rng.GetBytes(bytes);
    return Convert.ToHexString(bytes).ToLower();
}