using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.Infrastructure.Options
{
    /// <summary>
    /// 存储配置选项
    /// </summary>
    public class StorageOptions
    {
        public const string SectionName = "Storage";

        /// <summary>
        /// 本地备份存储配置
        /// </summary>
        public LocalStorageOptions Local { get; set; } = new();

        /// <summary>
        /// AWS S3存储配置
        /// </summary>
        public AwsS3StorageOptions AwsS3 { get; set; } = new();
    }

    /// <summary>
    /// 本地存储配置
    /// </summary>
    public class LocalStorageOptions
    {
        /// <summary>
        /// 本地存储根路径
        /// </summary>
        public string RootPath { get; set; } = "/data/fileservice/backup";

        /// <summary>
        /// 本地存储访问基础URL
        /// </summary>
        public string BaseUrl { get; set; } = "http://localhost:5000/files";

        /// <summary>
        /// 是否启用本地存储
        /// </summary>
        public bool Enabled { get; set; } = true;
    }

    /// <summary>
    /// AWS S3存储配置
    /// </summary>
    public class AwsS3StorageOptions
    {
        /// <summary>
        /// AWS访问密钥ID
        /// </summary>
        public string AccessKeyId { get; set; } = string.Empty;

        /// <summary>
        /// AWS访问密钥
        /// </summary>
        public string SecretAccessKey { get; set; } = string.Empty;

        /// <summary>
        /// AWS区域
        /// </summary>
        public string Region { get; set; } = "us-east-1";

        /// <summary>
        /// S3存储桶名称
        /// </summary>
        public string BucketName { get; set; } = string.Empty;

        /// <summary>
        /// 自定义服务URL（用于兼容其他S3兼容服务）
        /// </summary>
        public string? ServiceUrl { get; set; }

        /// <summary>
        /// 是否使用路径样式访问
        /// </summary>
        public bool ForcePathStyle { get; set; } = false;

        /// <summary>
        /// 是否启用AWS S3存储
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// 公开访问的基础URL（如果使用CDN）
        /// </summary>
        public string? PublicBaseUrl { get; set; }
    }
}