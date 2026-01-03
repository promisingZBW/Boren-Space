namespace FileService.WebAPI.DTOs
{
    /// <summary>
    /// 删除文件响应
    /// </summary>
    public class DeleteFileResponse
    {
        /// <summary>
        /// 删除的文件ID
        /// </summary>
        public Guid FileId { get; set; }

        /// <summary>
        /// 删除的文件名
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// 文件大小（字节）
        /// </summary>
        public long FileSizeInBytes { get; set; }

        /// <summary>
        /// 删除时间
        /// </summary>
        public DateTime DeleteTime { get; set; }

        /// <summary>
        /// 删除操作的详细消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 是否同时删除了远程存储文件
        /// </summary>
        public bool RemoteDeleted { get; set; }

        /// <summary>
        /// 是否同时删除了本地备份文件
        /// </summary>
        public bool BackupDeleted { get; set; }
    }
}
