using CommonInitializer;
using Listening.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using System; 

// 启用传统时间戳行为（兼容 DateTime.Now）
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// ����ͨ�÷���
builder.ConfigureExtraServices(new InitializerOptions
{
    LogFilePath = "logs/listening-main.log",
    EventBusQueueName = "Listening.Main"
});

// ע�����������ض��ķ���
builder.Services.AddDbContext<ListeningDbContext>(options =>
{
    var connStr = builder.Configuration.GetValue<string>("DefaultDB:ConnStr");
    options.UseNpgsql(connStr);
});

builder.Services.AddScoped<IListeningRepository, ListeningRepository>();

// 配置CORS - 允许前端跨域访问
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",           // 本地开发
                "http://3.107.216.226",            // EC2 前端
                "https://*.vercel.app"             // Vercel 部署
              )
              .SetIsOriginAllowedToAllowWildcardSubdomains()  // 允许 Vercel 子域名
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// ����SignalR֧�֣�Ϊ������ʵʱ����׼����
builder.Services.AddSignalR();

var app = builder.Build();

// ʹ��ͨ���м������
app.UseZbwDefault();

// 启用CORS
app.UseCors("AllowFrontend");

// ����SignalR Hub��������������ʵʱѧϰ���ȣ�
// app.MapHub<LearningHub>("/hubs/learning");

app.Run();
