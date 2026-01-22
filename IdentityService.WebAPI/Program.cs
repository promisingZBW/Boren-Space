using CommonInitializer;
using IdentityService.Infrastructure;
using IdentityService.WebAPI.Middleware;// �� Token������������Ҫ��������
using Microsoft.AspNetCore.Authentication.JwtBearer; // ?? ����JWT֧��
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens; // ?? ����Token��֤
using System;
using System.Collections.Generic;
using System.Text; //  ���ӱ���֧��
using Zbw.ASPNETCore.Filters;
using Zbw.JWT; //  ����JWTѡ��
using Npgsql.EntityFrameworkCore.PostgreSQL;  

// 启用传统时间戳行为（兼容 DateTime.Now）
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// ����ͨ�÷���
builder.ConfigureExtraServices(new InitializerOptions
{
    LogFilePath = "logs/identity-service.log",
    EventBusQueueName = "IdentityService"
});

// ע�����ݷ�����صķ���
builder.Services.AddDbContext<IdentityDbContext>(options =>
{
    var connStr = builder.Configuration.GetValue<string>("DefaultDB:ConnStr");
    options.UseNpgsql(connStr);
});

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddSingleton<ITokenBlacklistService, TokenBlacklistService>(); // �� Token������������Ҫ��������
builder.Services.AddMemoryCache();// �����ڴ滺���ע��

// ?? ����JWT��֤
var jwtOptions = builder.Configuration.GetSection("JWT").Get<JWTOptions>();
if (jwtOptions != null)
{
    builder.Services.Configure<JWTOptions>(builder.Configuration.GetSection("JWT"));
    builder.Services.AddScoped<IJWTService, JWTService>();

    // ����JWT��֤
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });
}

builder.Services.AddAuthorization();

// ?? ����Swagger������JWT֧�֣�
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Identity Service API", Version = "v1" });

    // ����JWT��֤֧�ֵ�Swagger����ȻIdentity������Ҫ��������Token����Ҳ��������Ҫ��֤��API��
    if (jwtOptions != null)
    {
        c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement()
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    },
                    Scheme = "oauth2",
                    Name = "Bearer",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                },
                new List<string>()
            }
        });
    }
});

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


var app = builder.Build();

// �����м���ܵ�
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend"); // ʹ��CORS���ԣ�������������õ�CORS��Ӧ

// ����JWT�������м��������֤�м��֮��
app.UseAuthentication();
app.UseMiddleware<JwtBlacklistMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();