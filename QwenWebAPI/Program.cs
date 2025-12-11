using QwenApi;
using RestSharp;
using System;
using System.Collections.Generic;

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
Timer? configRefreshTimer = null;

if (args.Length > 0)
{
    Console.WriteLine($"将使用 {args[0]} 作为验证源");

    configRefreshTimer = new Timer(async _ =>
    {
        try
        {
            var client = new RestClient(args[0]);
            var request = new RestRequest("/token", Method.Get);
            var response = await client.ExecuteAsync(request);

            Console.WriteLine(response.ContentLength);

            var lines = response.Content.Split(Environment.NewLine);
            Runtimes.cfgMgr.LoadString(lines[0], lines[1], lines[2]);
            Console.WriteLine($"Load {Runtimes.cfgMgr.BxUa.Length},{Runtimes.cfgMgr.Cookie.Length},{Runtimes.cfgMgr.BxUmidtoken.Length}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"配置刷新失败: {ex}");
        }
    },
    null,
    TimeSpan.FromSeconds(5),
    TimeSpan.FromMinutes(30));
}
else
{
    Console.WriteLine($"加载配置: {Runtimes.cfgMgr.Load()}");
    Console.WriteLine($"{Runtimes.cfgMgr.BxUa.Length},{Runtimes.cfgMgr.Cookie.Length},{Runtimes.cfgMgr.BxUmidtoken.Length}");
    if (!Runtimes.cfgMgr.IsConfigured)
    {
        Console.WriteLine("配置不完整，请检查 config/config.txt");
        return;
    }
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();