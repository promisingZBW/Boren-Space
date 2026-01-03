using CommonInitializer;
using FileService.Domain.Interfaces;
using FileService.Domain.Services;
using FileService.Infrastructure.Data;
using FileService.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Zbw.ASPNETCore.Filters;
using Zbw.JWT;

var builder = WebApplication.CreateBuilder(args);

// 配置通用服务（使用正确的方法名）
var initOptions = builder.Configuration.GetSection("InitializerOptions").Get<InitializerOptions>()
    ?? new InitializerOptions { LogFilePath = "logs/app.log" };

builder.ConfigureExtraServices(initOptions);

// 注册 FileService 的 DbContext
builder.Services.AddDbContext<FSDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultDB");
    options.UseSqlServer(connectionString);
});

// 显式注册为基类 DbContext（用于 UnitOfWork）
builder.Services.AddScoped<DbContext>(provider =>
    provider.GetRequiredService<FSDbContext>());

// 注册 FileService 相关服务
builder.Services.AddFileServiceInfrastructure(builder.Configuration);
builder.Services.AddScoped<IFSRepository, FSRepository>();
builder.Services.AddScoped<FSDomainService>();

// 配置JWT认证
var jwtOptions = builder.Configuration.GetSection("JWT").Get<JWTOptions>();
if (jwtOptions != null)
{
    builder.Services.Configure<JWTOptions>(builder.Configuration.GetSection("JWT"));
    builder.Services.AddScoped<IJWTService, JWTService>();

    // 配置JWT认证
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


// 配置控制器
builder.Services.AddControllers(options =>
{
    options.Filters.Add<UnitOfWorkFilter>();
});

// 添加Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "FileService API", Version = "v1" });

    // 添加JWT认证支持到Swagger
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
app.UseCors();
app.UseAuthentication();  // 重要：认证中间件
app.UseAuthorization();   // 重要：授权中间件
app.MapControllers();

app.Run();