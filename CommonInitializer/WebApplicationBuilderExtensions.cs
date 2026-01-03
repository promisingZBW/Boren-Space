using FluentValidation.AspNetCore;
using IdentityService.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Reflection;
using Zbw.ASPNETCore.Filters;
using Zbw.Commons;
using Zbw.JWT;

namespace CommonInitializer
{
    /// <summary>
    /// Web应用程序构建器扩展方法
    /// </summary>
    public static class WebApplicationBuilderExtensions
    {
        /// <summary>
        /// 配置额外服务
        /// </summary>
        public static void ConfigureExtraServices(this WebApplicationBuilder builder, InitializerOptions initOptions)
        {
            var services = builder.Services;
            var configuration = builder.Configuration;
            services.AddScoped<UnitOfWorkFilter>();

            // 配置日志
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .WriteTo.Console()
                .WriteTo.File(initOptions.LogFilePath, rollingInterval: RollingInterval.Day)
                .CreateLogger();

            builder.Host.UseSerilog();

            // 获取所有引用的程序集
            var assemblies = ReflectionHelper.GetAllReferencedAssemblies();

            // 添加所有DbContext
            services.AddAllDbContexts(ctx =>
            {
                string? connStr = configuration.GetValue<string>("DefaultDB:ConnStr");
                if (string.IsNullOrEmpty(connStr))
                {
                    throw new InvalidOperationException("数据库连接字符串未配置");
                }
                ctx.UseSqlServer(connStr);
            }, assemblies);

            // 配置身份验证
            services.AddAuthentication();
            services.AddAuthorization();

            // 配置JWT
            var jwtOptions = configuration.GetSection("JWT").Get<JWTOptions>();
            if (jwtOptions != null)
            {
                services.Configure<JWTOptions>(configuration.GetSection("JWT"));
                services.AddScoped<IJWTService, JWTService>();
            }

            // 添加MediatR
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(assemblies));

            // 配置MVC，添加全局过滤器
            services.AddControllers(options =>
            {
                options.Filters.Add<UnitOfWorkFilter>();
            });

            // 配置FluentValidation
            services.AddFluentValidationAutoValidation();

            // 配置Swagger
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            // 配置CORS
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });
        }

        /// <summary>
        /// 添加所有DbContext (简化版本)
        /// </summary>
        private static void AddAllDbContexts(this IServiceCollection services,
            Action<DbContextOptionsBuilder> optionsAction,
            Assembly[] assemblies)
        {
            // 暂时先不自动注册，当我们创建具体的DbContext时再手动注册
            // 这样可以避免泛型推断问题

            // 示例：当我们有具体的DbContext时，会这样注册：
            // services.AddDbContext<ListeningDbContext>(optionsAction);
            // services.AddDbContext<IdentityDbContext>(optionsAction);

            //Console.WriteLine($"发现 {assemblies.Length} 个程序集，将在创建具体DbContext时手动注册");

            /*
             * 其实这里注册数据库需要引用具体业务项目，
             * 但是在微服务架构中，通用组件不应该依赖具体的业务项目
             * 不过这里为了能够注册DbContext，还是引用了具体的业务项目IdentityService.Infrastructure/Listening.Infrastructure
             */

            Console.WriteLine("🔍 开始注册所有DbContext...");

            try
            {
                //  直接注册已知的DbContext类型
                Console.WriteLine("📦 注册 IdentityDbContext...");
                services.AddDbContext<IdentityService.Infrastructure.IdentityDbContext>(optionsAction);
                services.AddScoped<DbContext>(provider =>
                    provider.GetRequiredService<IdentityService.Infrastructure.IdentityDbContext>());
                Console.WriteLine("✅ IdentityDbContext 注册成功");

                Console.WriteLine("📦 注册 ListeningDbContext...");
                 //如果 ListeningDbContext 存在，取消注释下面两行
                 services.AddDbContext<Listening.Infrastructure.ListeningDbContext>(optionsAction);
                    services.AddScoped<DbContext>(provider => 
                     provider.GetRequiredService<Listening.Infrastructure.ListeningDbContext>());
                Console.WriteLine("✅ ListeningDbContext 注册成功");

                ///如果 FSDbContext 存在，取消注释下面两行
                Console.WriteLine("📦 注册 FSDbContext.cs");
                 services.AddDbContext<FileService.Infrastructure.Data.FSDbContext>(optionsAction);
                 services.AddScoped<DbContext>(provider => 
                     provider.GetRequiredService<FileService.Infrastructure.Data.FSDbContext>());
                Console.WriteLine("✅ FSDbContext 注册成功");


                //  未来添加新的DbContext时，在这里直接添加：
                // Console.WriteLine("📦 注册 YourNewDbContext...");
                // services.AddDbContext<YourNamespace.YourNewDbContext>(optionsAction);
                // services.AddScoped<DbContext>(provider => 
                //     provider.GetRequiredService<YourNamespace.YourNewDbContext>());
                // Console.WriteLine("✅ YourNewDbContext 注册成功");

                Console.WriteLine("🎯 所有DbContext注册完成");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ DbContext注册失败: {ex.Message}");
                throw; // 重新抛出异常，确保启动失败而不是默默忽略
            }

        }
    }
}