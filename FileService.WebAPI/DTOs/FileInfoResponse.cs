namespace FileService.WebAPI.DTOs
{
    /// <summary>
    /// 文件信息响应
    /// </summary>
    public class FileInfoResponse
    {
        /// <summary>
        /// 文件ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// 文件大小（字节）
        /// </summary>
        public long FileSizeInBytes { get; set; }

        /// <summary>
        /// 文件SHA256哈希
        /// </summary>
        public string FileSHA256Hash { get; set; } = string.Empty;

        /// <summary>
        /// 上传者ID
        /// </summary>
        public Guid UploaderId { get; set; }

        /// <summary>
        /// 备份存储URL
        /// </summary>
        public string? BackupUrl { get; set; }

        /// <summary>
        /// 远程存储URL
        /// </summary>
        public string? RemoteUrl { get; set; }

        /// <summary>
        /// 内容类型
        /// </summary>
        public string ContentType { get; set; } = string.Empty;

        /// <summary>
        /// 上传时间
        /// </summary>
        public DateTime UploadTime { get; set; }

        /// <summary>
        /// 统一下载链接
        /// </summary>
        public string DownloadUrl { get; set; } = string.Empty;

        /// <summary>
        /// 文件大小（格式化显示）
        /// </summary>
        public string FileSizeFormatted => FormatFileSize(FileSizeInBytes);

        /// <summary>
        /// 格式化文件大小
        /// </summary>
        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}