using FileService.Domain.Enums;
using System;
using System.Collections.Generic;
using Zbw.DomainCommons;

namespace FileService.Domain.Entities
{
    /// <summary>
    /// 上传的文件项 - 聚合根
    /// </summary>
    public class UploadedItem : IAggregateRoot
    {
        public Guid Id { get; private set; }

        /// <summary>
        /// 用户上传的原始文件名（不含路径）
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// 文件大小（字节）
        /// </summary>
        public long FileSizeInBytes { get; private set; }

        /// <summary>
        /// 文件SHA256哈希值（用于去重检测）
        /// 两个文件的大小和SHA256都相同的概率非常小，可认为是相同文件
        /// </summary>
        public string FileSHA256Hash { get; private set; }


        /// <summary>
        /// 音频文件类型
        /// </summary>
        public FileType FileType { get; private set; } // 新增字段

        /// <summary>
        /// 文件Content-Type（MIME类型）
        /// 告诉 FileService 这个文件是什么类型（如 audio/mpeg、image/jpeg）
        /// </summary>
        public string ContentType { get; private set; }


        /// <summary>
        /// 上传时间
        /// </summary>
        public DateTime UploadTime { get; private set; }

        /// <summary>
        /// 上传者用户ID
        /// </summary>
        public Guid UploaderId { get; private set; }

        /// <summary>
        /// 备份文件URL（本地存储，内网高速稳定设备）
        /// </summary>
        public Uri? BackupUrl { get; private set; }

        /// <summary>
        /// 远程文件URL（云存储，供外部访问）
        /// </summary>
        public Uri? RemoteUrl { get; private set; }

        /// <summary>
        /// 存储键（文件在存储系统中的路径标识）
        /// </summary>
        public string StorageKey { get; private set; }



        /// <summary>
        /// 是否已删除（软删除）
        /// </summary>
        public bool IsDeleted { get; private set; }

        private readonly List<IDomainEvent> _domainEvents = new();

        /// <summary>
        /// 构造函数
        /// </summary>
        public UploadedItem(string fileName, long fileSizeInBytes, string fileSHA256Hash,
            FileType fileType, string contentType, Guid uploaderId, string storageKey,
            Uri? backupUrl = null, Uri? remoteUrl = null)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("文件名不能为空", nameof(fileName));
            if (fileSizeInBytes <= 0)
                throw new ArgumentException("文件大小必须大于0", nameof(fileSizeInBytes));
            if (string.IsNullOrEmpty(fileSHA256Hash))
                throw new ArgumentException("文件哈希值不能为空", nameof(fileSHA256Hash));
            if (string.IsNullOrEmpty(storageKey))
                throw new ArgumentException("存储键不能为空", nameof(storageKey));
            if (string.IsNullOrEmpty(contentType))
                throw new ArgumentException("MIME类型不能为空", nameof(contentType));

            Id = Guid.NewGuid();
            FileName = fileName;
            FileSizeInBytes = fileSizeInBytes;
            FileSHA256Hash = fileSHA256Hash;
            FileType = fileType; // 设置文件类型
            ContentType = contentType; // 设置MIME类型
            UploaderId = uploaderId;
            StorageKey = storageKey;
            BackupUrl = backupUrl;
            RemoteUrl = remoteUrl;

            //协调世界时间（UTC）
            //应用程序往往在不同的地区和时区运行。使用UTC可以简化时间数据的存储和交换，
            //使得在不同地区的系统之间更容易协调和处理数据。
            UploadTime = DateTime.UtcNow;
            IsDeleted = false;

            _domainEvents = new List<IDomainEvent>();
        }

        // EF Core 需要的无参构造函数
        private UploadedItem()
        {
            FileName = string.Empty;
            FileSHA256Hash = string.Empty;
            StorageKey = string.Empty;
            ContentType = string.Empty;
            _domainEvents = new List<IDomainEvent>();
        }

        /// <summary>
        /// 更新备份URL
        /// </summary>
        public void UpdateBackupUrl(Uri? backupUrl)
        {
            BackupUrl = backupUrl;
        }

        /// <summary>
        /// 更新远程URL
        /// </summary>
        public void UpdateRemoteUrl(Uri? remoteUrl)
        {
            RemoteUrl = remoteUrl;
        }

        /// <summary>
        /// 更新存储键
        /// </summary>
        public void UpdateStorageKey(string storageKey)
        {
            if (string.IsNullOrWhiteSpace(storageKey))
                throw new ArgumentException("存储键不能为空", nameof(storageKey));

            StorageKey = storageKey;
        }

        /// <summary>
        /// 软删除文件
        /// </summary>
        public void MarkAsDeleted()
        {
            IsDeleted = true;
            // 可以在这里添加文件删除事件
        }

        /// <summary>
        /// 恢复已删除的文件
        /// </summary>
        public void Restore()
        {
            IsDeleted = false;
        }

        public IReadOnlyList<IDomainEvent> GetDomainEvents()
        {
            return _domainEvents.AsReadOnly();
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }
}