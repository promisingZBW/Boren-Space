using FileService.Domain.Entities;
using FileService.Domain.Enums;
using FileService.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace FileService.Domain.Services
{
    /// <summary>
    /// 文件服务领域服务
    /// </summary>
    public class FSDomainService
    {
        private readonly IFSRepository _repository;
        private readonly IStorageClient _backupStorage;  // 本地备份存储
        private readonly IStorageClient _remoteStorage;  // AWS S3云存储

        public FSDomainService(IFSRepository repository, IEnumerable<IStorageClient> storageClients)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));

            // 通过StorageType区分不同的存储客户端
            var clients = storageClients.ToList();
            _backupStorage = clients.FirstOrDefault(c => c.StorageType == StorageType.Backup)
                ?? throw new InvalidOperationException("未找到备份存储客户端");
            _remoteStorage = clients.FirstOrDefault(c => c.StorageType == StorageType.Public)
                ?? throw new InvalidOperationException("未找到公网存储客户端");
        }


        /// <summary>
        /// 上传文件的核心业务逻辑
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <param name="fileName">原始文件名</param>
        /// <param name="contentType">文件MIME类型</param>
        /// <param name="uploaderId">上传者ID</param>
        /// <param name="storageType">存储类型</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>上传的文件项</returns>
        public async Task<UploadedItem> UploadAsync(Stream stream, string fileName,
            string contentType, Guid uploaderId, StorageType storageType,
            CancellationToken cancellationToken = default)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("文件名不能为空", nameof(fileName));
            if (string.IsNullOrWhiteSpace(contentType)) throw new ArgumentException("内容类型不能为空", nameof(contentType));

            // 1. 计算文件哈希值和大小
            string hash = await ComputeSha256HashAsync(stream);
            long fileSize = stream.Length;

            // 2. 检查文件是否已存在（去重逻辑）
            var existingItem = await _repository.FindFileAsync(fileSize, hash);
            if (existingItem != null)
            {
                return existingItem;
            }

            // 3. 生成存储键
            string storageKey = GenerateStorageKey(hash, fileName);

            Uri? backupUrl = null;
            Uri? remoteUrl = null;

            // 🔧 4. 根据存储类型选择不同的存储策略
            switch (storageType)
            {
                case StorageType.Backup:
                    // 只保存到本地备份存储
                    stream.Position = 0;
                    backupUrl = await _backupStorage.SaveAsync(storageKey, stream, contentType, cancellationToken);
                    break;

                case StorageType.Public:
                    // 只保存到远程公共存储（AWS S3）
                    stream.Position = 0;
                    remoteUrl = await _remoteStorage.SaveAsync(storageKey, stream, contentType, cancellationToken);
                    break;

                default:
                    throw new ArgumentException($"不支持的存储类型: {storageType}", nameof(storageType));
            }

            // 5. 创建领域实体
            var uploadedItem = new UploadedItem(
                fileName: fileName,
                fileSizeInBytes: fileSize,
                fileSHA256Hash: hash,
                fileType: GetFileTypeFromContentType(contentType),
                uploaderId: uploaderId,
                backupUrl: backupUrl,
                remoteUrl: remoteUrl,
                storageKey: storageKey,
                contentType: contentType
            );

            return uploadedItem;
        }


        // 添加一个辅助方法来根据 ContentType 确定 FileType
        private static FileType GetFileTypeFromContentType(string contentType)
        {
            return contentType.ToLowerInvariant() switch
            {
                var ct when ct.StartsWith("audio/") => FileType.Audio,
                var ct when ct.StartsWith("image/") => FileType.Image,
                var ct when ct.StartsWith("text/") => FileType.Subtitle,
                _ => FileType.Other
            };
        }


        /// <summary>
        /// 生成存储键 - URL安全版本
        /// 格式：年/月/日/哈希值前8位/URL安全文件名
        /// 例如：2024/01/15/abc12345/20240115_143022_a1b2c3d4.mp3
        /// </summary>
        private static string GenerateStorageKey(string hash, string fileName)
        {
            var today = DateTime.Today;
            var hashPrefix = hash.Substring(0, Math.Min(8, hash.Length));

            // 🔧 生成URL安全的文件名，但保留扩展名
            var urlSafeFileName = GenerateUrlSafeFileName(fileName);

            return $"{today.Year:D4}/{today.Month:D2}/{today.Day:D2}/{hashPrefix}/{urlSafeFileName}";
        }

        /// <summary>
        /// 生成URL安全的文件名（用于存储Key）
        /// 保留扩展名，但用时间戳+随机字符串替换文件名
        /// 因为原中文名，中文不能直接用于URL路径，可能导致访问问题
        /// </summary>
        private static string GenerateUrlSafeFileName(string originalFileName)
        {
            if (string.IsNullOrWhiteSpace(originalFileName))
                return "unnamed";

            // 获取扩展名（如.mp3）
            var extension = Path.GetExtension(originalFileName);

            // 生成URL安全的文件名：时间戳 + 随机字符串
            var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd_HHmmss");
            var random = Guid.NewGuid().ToString("N")[..8]; // 取前8位

            return $"{timestamp}_{random}{extension}";
        }



        /// <summary>
        /// 异步计算文件流的SHA256哈希值
        /// </summary>
        private static async Task<string> ComputeSha256HashAsync(Stream stream)
        {
            //using 确保该实例在使用完后能自动释放资源，以避免内存泄漏
            using var sha256 = SHA256.Create();
            var hashBytes = await sha256.ComputeHashAsync(stream);
            //使用 Convert.ToHexString(hashBytes) 将字节数组（hashBytes）转换为十六进制字符串表示
            //ToLowerInvariant() 将字符串转换为小写，确保哈希值的一致性（通常哈希值在表示时使用小写格式）
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }



        /// <summary>
        /// 删除文件的核心业务逻辑
        /// </summary>
        /// <param name="fileId">文件ID</param>
        /// <param name="currentUserId">当前用户ID</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>删除结果 (成功标志, 消息, 远程删除状态, 备份删除状态)</returns>
        public async Task<(bool Success, string Message, bool RemoteDeleted, bool BackupDeleted)> DeleteAsync(
            Guid fileId,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            // 1. 获取文件信息并验证权限
            var file = await _repository.GetByIdAndUploaderAsync(fileId, currentUserId);
            if (file == null)
            {
                return (false, "文件不存在或无权限删除", false, false);
            }

            if (file.IsDeleted)
            {
                return (false, "文件已被删除", false, false);
            }

            bool remoteDeleted = true;
            bool backupDeleted = true;

            // 2. 删除远程存储文件（AWS S3）
            if (file.RemoteUrl != null)
            {
                try
                {
                    remoteDeleted = await _remoteStorage.DeleteAsync(file.StorageKey, cancellationToken);
                    if (remoteDeleted)
                    {
                        Console.WriteLine($"远程文件删除成功: {file.StorageKey}");
                    }
                    else
                    {
                        Console.WriteLine($"警告：远程文件删除失败 - {file.StorageKey}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"远程文件删除异常：{ex.Message}");
                    remoteDeleted = false;
                }
            }

            // 3. 删除本地备份文件
            if (file.BackupUrl != null)
            {
                try
                {
                    backupDeleted = await _backupStorage.DeleteAsync(file.StorageKey, cancellationToken);
                    if (backupDeleted)
                    {
                        Console.WriteLine($"本地备份文件删除成功: {file.StorageKey}");
                    }
                    else
                    {
                        Console.WriteLine($"警告：本地备份文件删除失败 - {file.StorageKey}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"本地备份文件删除异常：{ex.Message}");
                    backupDeleted = false;
                }
            }

            // 4. 软删除数据库记录
            var dbDeleted = await _repository.SoftDeleteAsync(fileId, currentUserId);
            if (!dbDeleted)
            {
                return (false, "数据库删除失败", remoteDeleted, backupDeleted);
            }

            var message = "文件删除成功";
            if (!remoteDeleted || !backupDeleted)
            {
                message += "（部分存储删除失败，请检查日志）";
            }

            return (true, message, remoteDeleted, backupDeleted);
        }

    }
}