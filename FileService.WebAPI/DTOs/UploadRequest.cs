using System.ComponentModel.DataAnnotations;

namespace FileService.WebAPI.DTOs
{
    /// <summary>
    /// 文件上传请求
    /// </summary>
    public class UploadRequest
    {
        /// <summary>
        /// 上传的文件
        /// </summary>
        [Required(ErrorMessage = "请选择要上传的文件")]
        public IFormFile File { get; set; } = null!;

        /// <summary>
        /// 存储类型：Backup=备份存储, Public=公开存储
        /// </summary>
        [Required(ErrorMessage = "请指定存储类型")]
        public string StorageType { get; set; } = null!;
    }
}