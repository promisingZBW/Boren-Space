using CommonInitializer;
using Listening.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// 配置通用服务
builder.ConfigureExtraServices(new InitializerOptions
{
    LogFilePath = "logs/listening-main.log",
    EventBusQueueName = "Listening.Main"
});

// 注册听力服务特定的服务
builder.Services.AddDbContext<ListeningDbContext>(options =>
{
    var connStr = builder.Configuration.GetValue<string>("DefaultDB:ConnStr");
    options.UseSqlServer(connStr);
});

builder.Services.AddScoped<IListeningRepository, ListeningRepository>();

// 添加SignalR支持（为将来的实时功能准备）
builder.Services.AddSignalR();

var app = builder.Build();

// 使用通用中间件配置
app.UseZbwDefault();

// 配置SignalR Hub（将来可以用于实时学习进度）
// app.MapHub<LearningHub>("/hubs/learning");

app.Run();
