using CommonInitializer;
using IdentityService.Infrastructure;
using IdentityService.WebAPI.Middleware;// ← Token黑名单服务需要添加这行
using Microsoft.AspNetCore.Authentication.JwtBearer; // ?? 添加JWT支持
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens; // ?? 添加Token验证
using System;
using System.Collections.Generic;
using System.Text; //  添加编码支持
using Zbw.ASPNETCore.Filters;
using Zbw.JWT; //  添加JWT选项

var builder = WebApplication.CreateBuilder(args);

// 配置通用服务
builder.ConfigureExtraServices(new InitializerOptions
{
    LogFilePath = "logs/identity-service.log",
    EventBusQueueName = "IdentityService"
});

// 注册数据服务相关的服务
builder.Services.AddDbContext<IdentityDbContext>(options =>
{
    var connStr = builder.Configuration.GetValue<string>("DefaultDB:ConnStr");
    options.UseSqlServer(connStr);
});

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddSingleton<ITokenBlacklistService, TokenBlacklistService>(); // ← Token黑名单服务需要添加这行
builder.Services.AddMemoryCache();// 添加内存缓存的注册

// ?? 配置JWT认证
var jwtOptions = builder.Configuration.GetSection("JWT").Get<JWTOptions>();
if (jwtOptions != null)
{
    builder.Services.Configure<JWTOptions>(builder.Configuration.GetSection("JWT"));
    builder.Services.AddScoped<IJWTService, JWTService>();

    // 添加JWT认证
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

// ?? 配置Swagger（包含JWT支持）
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Identity Service API", Version = "v1" });

    // 添加JWT认证支持到Swagger（虽然Identity服务主要用于生成Token，但也可以有需要认证的API）
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

// 配置cors原因：前端直接调用登录接口
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")  // 你的前端端口
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});


var app = builder.Build();

// 配置中间件管道
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend"); // 使用CORS策略，这段与上面配置的CORS对应

// 配置JWT黑名单中间件（在认证中间件之后）
app.UseAuthentication();
app.UseMiddleware<JwtBlacklistMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();