using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Amazon.S3;
using FileService.Domain.Interfaces;
using FileService.Domain.Services;
using FileService.Infrastructure.Data;
using FileService.Infrastructure.Options;
using FileService.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FileService.Infrastructure.Extensions
{
    /// <summary>
    /// 依赖注入扩展方法
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加FileService基础设施服务
        /// </summary>
        public static IServiceCollection AddFileServiceInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // 配置选项
            services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.SectionName));

            // 注册数据库上下文
            services.AddDbContext<FSDbContext>(options =>
            {
                var connStr = configuration.GetValue<string>("DefaultDB:ConnStr");
                options.UseSqlServer(connStr);
            });

            // 注册仓储
            services.AddScoped<IFSRepository, FSRepository>();

            // 注册领域服务
            services.AddScoped<FSDomainService>();

            // 注册存储客户端
            services.AddStorageClients(configuration);

            return services;
        }

        /// <summary>
        /// 注册存储客户端
        /// </summary>
        private static IServiceCollection AddStorageClients(
            this IServiceCollection services,
            IConfiguration configuration)
        {

            // 是空合并运算符。它会判断 Get<StorageOptions>() 的结果是否为 null。
            //如果配置节中没有找到有效的值（例如，StorageOptions 没有在配置中定义），则创建一个新的 StorageOptions 实例作为默认值。
            var storageOptions = configuration.GetSection(StorageOptions.SectionName).Get<StorageOptions>()
                ?? new StorageOptions();


            //判断是哪种存储方式启用了，然后注册相应的存储客户端
            // 注册本地存储客户端
            if (storageOptions.Local.Enabled)
            {
                services.AddScoped<IStorageClient, LocalStorageClient>();
            }

            // 注册AWS S3存储客户端
            if (storageOptions.AwsS3.Enabled)
            {
                // 配置AWS S3客户端
                // 直接连 AWS 官方 S3，默认配置
                services.AddAWSService<IAmazonS3>(configuration.GetAWSOptions());


                //将 IAmazonS3 接口注册为 Scoped 生命周期的服务，即每个 HTTP 请求会创建一个新的实例
                //provider 是 IServiceProvider 类型，代表 DI 容器
                //在工厂方法中，它允许你访问已注册的其他服务
                services.AddScoped<IAmazonS3>(provider =>
                {
                    var options = storageOptions.AwsS3;
                    //GetRequiredService<T>() 获取的实例类型是 T
                    var logger = provider.GetRequiredService<ILogger<AwsS3StorageClient>>();

                    // 🎯 优先使用环境变量，其次使用配置文件
                    var accessKeyId = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID")
                                     ?? options.AccessKeyId;
                    var secretAccessKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY")
                                         ?? options.SecretAccessKey;
                    var region = Environment.GetEnvironmentVariable("AWS_DEFAULT_REGION")
                                ?? options.Region;

                    // 验证必要的配置
                    if (string.IsNullOrEmpty(accessKeyId) || string.IsNullOrEmpty(secretAccessKey))
                    {
                        logger.LogWarning("⚠️ AWS凭据未配置：AccessKeyId或SecretAccessKey为空");
                        throw new InvalidOperationException("AWS凭据未配置，请设置环境变量或配置文件");
                    }

                    if (string.IsNullOrEmpty(region))
                    {
                        logger.LogWarning("⚠️ AWS区域未配置，使用默认值：us-east-1");
                        region = "us-east-1";
                    }

                    logger.LogInformation("🔧 AWS S3配置：Region={Region}, BucketName={BucketName}",
                        region, options.BucketName);
                    logger.LogInformation("🔑 使用环境变量AWS凭据：{UseEnvCredentials}",
                        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID")));

                    var config = new AmazonS3Config
                    {                 
                        RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region),
                        //类似于https://s3.amazonaws.com/bucket-name/object-key的URL格式
                        ForcePathStyle = options.ForcePathStyle
                    };

                    // 如果配置了自定义服务URL（如MinIO），可以指定地址
                    if (!string.IsNullOrEmpty(options.ServiceUrl))
                    {
                        config.ServiceURL = options.ServiceUrl;
                        config.ForcePathStyle = true; // MinIO通常需要路径样式
                        logger.LogInformation("🌐 使用自定义S3服务URL：{ServiceUrl}", options.ServiceUrl);
                    }

                    return new AmazonS3Client(accessKeyId, secretAccessKey, config);
                });

                //把你的存储抽象 IStorageClient 的 S3 实现注册进来。
                //之后业务层只面向 IStorageClient 编程，不直接依赖 AWS SDK
                //当有人需要 IStorageClient 时，请给他一个 AwsS3StorageClient 的实例，这么做的处是解耦，
                //即业务代码不直接依赖于具体的存储实现（如 AWS S3），而是依赖于抽象接口 IStorageClient。
                //而且易于切换实现，只需替换AwsS3StorageClient即可
                services.AddScoped<IStorageClient, AwsS3StorageClient>();
            }

            return services;
        }
    }
}