using Amazon.S3;
using Amazon.S3.Model;
using FileService.Domain.Enums;
using FileService.Domain.Interfaces;
using FileService.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace FileService.Infrastructure.Storage
{
    /// <summary>
    /// AWS S3存储客户端实现
    /// </summary>
    public class AwsS3StorageClient : IStorageClient
    {
        private readonly IAmazonS3 _s3Client;
        private readonly AwsS3StorageOptions _options;
        //ILogger<T> 是 Microsoft.Extensions.Logging 命名空间中的接口，用于提供日志记录功能。
        private readonly ILogger<AwsS3StorageClient> _logger;

        public StorageType StorageType => StorageType.Public;

        public AwsS3StorageClient(IAmazonS3 s3Client, IOptions<StorageOptions> options, ILogger<AwsS3StorageClient> logger)
        {
            _s3Client = s3Client;
            _options = options.Value.AwsS3;
            _logger = logger;
        }

        /// <summary>
        /// 返回一个表示上传文件的公开可访问的 URI
        /// 公开可访问的 ：任何人都可以通过这个链接访问对应的文件，而无需特殊权限或认证。
        /// </summary>
        /// <param name="key"><文件在 S3 存储中的唯一标识名（可以是路径+文件名）/param>
        /// <param name="content"><要上传的文件内容的流/param>
        /// <param name="contentType"><文件的 MIME 类型，如 "image/jpeg"、"application/pdf" 等/param>
        /// <param name="cancellationToken"><用于取消异步操作的令牌，默认值为 default/param>
        /// <returns></returns>
        public async Task<Uri> SaveAsync(string key, Stream content, string contentType, CancellationToken cancellationToken = default)
        {
            //用 try-catch 块捕获可能发生的异常，以便记录日志和处理错误。
            try
            {
                // 创建 PutObjectRequest 对象，包含上传所需的信息
                var request = new PutObjectRequest
                {
                    BucketName = _options.BucketName,
                    Key = key,
                    InputStream = content,
                    ContentType = contentType,
                    // 移除 CannedACL，使用 Bucket Policy 来控制公开访问
                    ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256 // 启用服务端加密
                };

                //设置缓存策略
                //元数据是指用来描述、解释和管理其他数据的数据。
                //这里通过 Add 方法向元数据中添加了一个名为 Cache-Control 的属性，其值是 public, max-age=31536000
                //Cache-Control：这是一个 HTTP 头部字段，用于定义缓存机制的指令。
                //public：表示响应可以被任何缓存区缓存，包括浏览器和中间缓存服务器。
                //max-age=31536000：表示响应可以被缓存的最大时间为 31536000 秒（即 1 年）。在这段时间内，缓存的内容被认为是新鲜的，不需要重新向服务器请求。
                request.Metadata.Add("Cache-Control", "public, max-age=31536000"); // 1年缓存

                // 执行上传操作
                var response = await _s3Client.PutObjectAsync(request, cancellationToken);

                // 检查响应状态码，确保上传成功
                if (response.HttpStatusCode == HttpStatusCode.OK)
                {
                    _logger.LogInformation("文件上传到S3成功: {Key}", key);

                    // 构建公开访问URL
                    var publicUrl = GetPublicUrl(key);
                    return new Uri(publicUrl);
                }
                else
                {
                    _logger.LogError("S3上传失败，状态码: {StatusCode}", response.HttpStatusCode);
                    throw new InvalidOperationException($"S3上传失败，状态码: {response.HttpStatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "S3文件上传失败: {Key}", key);
                throw;
            }
        }


        public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new GetObjectMetadataRequest
                {
                    BucketName = _options.BucketName,
                    Key = key
                };

                await _s3Client.GetObjectMetadataAsync(request, cancellationToken);
                _logger.LogDebug("S3文件存在: {Key}", key);
                return true;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogDebug("S3文件不存在: {Key}", key);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查S3文件存在性失败: {Key}", key);
                return false;
            }
        }


        public async Task<Stream> GetAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = _options.BucketName,
                    Key = key
                };

                var response = await _s3Client.GetObjectAsync(request, cancellationToken);
                _logger.LogDebug("获取S3文件流成功: {Key}", key);
                return response.ResponseStream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取S3文件流失败: {Key}", key);
                throw;
            }
        }


        public async Task<bool> DeleteAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new DeleteObjectRequest
                {
                    BucketName = _options.BucketName,
                    Key = key
                };

                var response = await _s3Client.DeleteObjectAsync(request, cancellationToken);

                if (response.HttpStatusCode == HttpStatusCode.NoContent)
                {
                    _logger.LogInformation("S3文件删除成功: {Key}", key);
                    return true;
                }
                else
                {
                    _logger.LogWarning("S3文件删除响应异常: {Key}, 状态码: {StatusCode}", key, response.HttpStatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "S3文件删除失败: {Key}", key);
                return false;
            }
        }

        /// <summary>
        /// 构建公开访问URL
        /// </summary>
        private string GetPublicUrl(string key)
        {
            // 对key进行URL编码，处理中文字符和特殊字符
            var encodedKey = Uri.EscapeDataString(key).Replace("%2F", "/"); // 保留路径分隔符

            // 如果配置了自定义公开URL（如CDN），使用自定义URL
            if (!string.IsNullOrEmpty(_options.PublicBaseUrl))
            {
                return $"{_options.PublicBaseUrl.TrimEnd('/')}/{encodedKey}";
            }

            // 获取正确的区域配置
            var region = Environment.GetEnvironmentVariable("AWS_DEFAULT_REGION") ?? _options.Region;
            if (string.IsNullOrEmpty(region))
            {
                region = "us-east-1"; // 默认区域
            }

            /* 否则使用S3的标准URL
            如果没有自定义公开 URL，但存在 _options.ServiceUrl（例如，MinIO 的服务 URL），
            同样会形成 URL。具体逻辑与上面一样，在 URL 中添加了桶名称和文件的 key
            示例：
            AWS S3 URL：https://my-bucket.s3.amazonaws.com/my-file.txt
            MinIO URL：https://minio.example.com/my-bucket/my-file.txt
            */
            if (!string.IsNullOrEmpty(_options.ServiceUrl))
            {
                // 自定义服务URL（如MinIO）
                return $"{_options.ServiceUrl.TrimEnd('/')}/{_options.BucketName}/{encodedKey}";
            }
            else
            {
                // AWS S3标准URL
                return $"https://{_options.BucketName}.s3.{region}.amazonaws.com/{encodedKey}";
            }
        }
    }
}