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
using Npgsql.EntityFrameworkCore.PostgreSQL;  

// 启用传统时间戳行为（兼容 DateTime.Now）
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// ����ͨ�÷���ʹ����ȷ�ķ�������
var initOptions = builder.Configuration.GetSection("InitializerOptions").Get<InitializerOptions>()
    ?? new InitializerOptions { LogFilePath = "logs/app.log" };

builder.ConfigureExtraServices(initOptions);

// ע�� FileService �� DbContext
builder.Services.AddDbContext<FSDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultDB");
    options.UseNpgsql(connectionString);
});

// ��ʽע��Ϊ���� DbContext������ UnitOfWork��
builder.Services.AddScoped<DbContext>(provider =>
    provider.GetRequiredService<FSDbContext>());

// ע�� FileService ��ط���
builder.Services.AddFileServiceInfrastructure(builder.Configuration);
builder.Services.AddScoped<IFSRepository, FSRepository>();
builder.Services.AddScoped<FSDomainService>();

// ����JWT��֤
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

// ���ÿ�����
builder.Services.AddControllers(options =>
{
    options.Filters.Add<UnitOfWorkFilter>();
});

// ����Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "FileService API", Version = "v1" });

    // ����JWT��֤֧�ֵ�Swagger
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

// �����м���ܵ�
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");  // 使用CORS策略
app.UseAuthentication();  // ��Ҫ����֤�м��
app.UseAuthorization();   // ��Ҫ����Ȩ�м��
app.MapControllers();

app.Run();