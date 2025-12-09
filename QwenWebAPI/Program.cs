using QwenApi;
using RestSharp;
using System;
using System.Collections.Generic;



// Add services to the container.
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if(args.Length > 0)
{
    Console.WriteLine($"将使用 {args[0]} 作为验证源");
    await Task.Run(() =>
    {
        Timer timer = new(_ =>
        {
            try
            {
                var rsc = new RestClient(args[0]);
                var result = rsc.ExecuteAsync(new RestRequest("/token", Method.Get)).GetAwaiter().GetResult();

                Console.WriteLine(result.ContentLength);

                var lines = result.Content.Split(Environment.NewLine);
                Runtimes.cfgMgr.LoadString(lines[0], lines[1], lines[2]);
                Console.WriteLine($"Load {Runtimes.cfgMgr.BxUa.Length},{Runtimes.cfgMgr.Cookie.Length},{Runtimes.cfgMgr.BxUmidtoken.Length}");
            }
            catch{}
        },
        state: null,
        dueTime: 5000,
        period: 1800000
        );
    });
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
