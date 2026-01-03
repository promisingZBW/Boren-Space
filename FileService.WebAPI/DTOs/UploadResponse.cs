namespace FileService.WebAPI.DTOs
{
    /// <summary>
    /// 文件上传响应
    /// </summary>
    public class UploadResponse
    {
        /// <summary>
        /// 文件ID
        /// </summary>
        public Guid FileId { get; set; }

        /// <summary>
        /// 原始文件名
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// 文件大小（字节）
        /// </summary>
        public long FileSizeInBytes { get; set; }

        /// <summary>
        /// 备份存储URL
        /// </summary>
        public string? BackupUrl { get; set; }

        /// <summary>
        /// 远程存储URL
        /// </summary>
        public string? RemoteUrl { get; set; }

        /// <summary>
        /// 上传时间
        /// </summary>
        public DateTime UploadTime { get; set; }
    }
}