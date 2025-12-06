using QwenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

Console.WriteLine($"加载配置: {Runtimes.cfgMgr.Load()}");
Console.WriteLine($"{Runtimes.cfgMgr.BxUa.Length},{Runtimes.cfgMgr.Cookie.Length},{Runtimes.cfgMgr.BxUmidtoken.Length}");
if (!Runtimes.cfgMgr.IsConfigured)
{
    Console.WriteLine("配置不完整，请检查 config/config.txt");
    return;
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
