using FileService.Domain.Enums;
using FileService.Domain.Interfaces;
using FileService.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FileService.Infrastructure.Storage
{
    /// <summary>
    /// 本地存储客户端实现
    /// </summary>
    public class LocalStorageClient : IStorageClient
    {
        private readonly LocalStorageOptions _options;
        private readonly ILogger<LocalStorageClient> _logger;

        public StorageType StorageType => StorageType.Backup;

        public LocalStorageClient(IOptions<StorageOptions> options, ILogger<LocalStorageClient> logger)
        {
            _options = options.Value.Local;
            _logger = logger;

            // 确保根目录存在
            if (!Directory.Exists(_options.RootPath))
            {
                Directory.CreateDirectory(_options.RootPath);
                _logger.LogInformation("创建本地存储根目录: {RootPath}", _options.RootPath);
            }
        }

        public async Task<Uri> SaveAsync(string key, Stream content, string contentType, CancellationToken cancellationToken = default)
        {
            try
            {
                var filePath = GetFullPath(key);
                var directory = Path.GetDirectoryName(filePath);
                
                // 确保目录存在
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // 保存文件
                using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                await content.CopyToAsync(fileStream, cancellationToken);

                _logger.LogInformation("文件保存成功: {Key} -> {FilePath}", key, filePath);

                // 返回访问URL
                var url = $"{_options.BaseUrl.TrimEnd('/')}/{key}";
                return new Uri(url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存文件失败: {Key}", key);
                throw;
            }
        }

        public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var filePath = GetFullPath(key);
                var exists = File.Exists(filePath);
                
                _logger.LogDebug("检查文件存在性: {Key} -> {Exists}", key, exists);
                return Task.FromResult(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查文件存在性失败: {Key}", key);
                return Task.FromResult(false);
            }
        }

        public async Task<Stream> GetAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var filePath = GetFullPath(key);

                // 使用Task.Run来避免阻塞
                var fileStream = await Task.Run(() =>
                {
                    if (!File.Exists(filePath))
                    {
                        throw new FileNotFoundException($"文件不存在: {key}");
                    }
                    //FileStream的构造函数本身仍然是同步的，需要使用Task.Run来避免阻塞
                    return new FileStream(
                        filePath,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.Read,
                        bufferSize: 8192,
                        useAsync: true);
                }, cancellationToken);

                _logger.LogDebug("获取文件流成功: {Key}", key);
                return fileStream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取文件流失败: {Key}", key);
                throw;
            }
        }

        public Task<bool> DeleteAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var filePath = GetFullPath(key);
                
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("删除文件成功: {Key}", key);
                    return Task.FromResult(true);
                }

                _logger.LogWarning("尝试删除不存在的文件: {Key}", key);
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除文件失败: {Key}", key);
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// 获取文件的完整物理路径
        /// key是相对路径
        /// </summary>
        private string GetFullPath(string key)
        {
            // 规范化路径，防止路径遍历攻击
            //这行代码将路径中所有的反斜杠（\）替换为正斜杠（/），以确保路径的一致性。
            //使用 Trim('/') 方法去掉路径开头和结尾的斜杠，确保路径格式的规范。
            var normalizedKey = key.Replace('\\', '/').Trim('/');

            // 检查路径安全性
            //攻击者可能会利用 .. 来试图访问系统的上一层目录和文件
            if (normalizedKey.Contains("..") || Path.IsPathRooted(normalizedKey))
            {
                throw new ArgumentException($"非法的文件键: {key}");
            }
            //Path.Combine 方法将指定的根路径（_options.RootPath）和规范化后的相对路径合并成完整路径。
            return Path.Combine(_options.RootPath, normalizedKey.Replace('/', Path.DirectorySeparatorChar));
        }
    }
}