using System.ComponentModel.DataAnnotations;

namespace FileService.WebAPI.DTOs
{
    /// <summary>
    /// 删除文件请求
    /// </summary>
    public class DeleteFileRequest
    {
        /// <summary>
        /// 文件ID
        /// </summary>
        [Required(ErrorMessage = "文件ID不能为空")]//验证：Required 特性确保该字段必须提供
        public Guid FileId { get; set; }//当模型验证失败时，ASP.NET Core 会自动将错误信息包装成 HTTP 响应，"文件ID不能为空"

        /// <summary>
        /// 确认删除（防止误删），用户必须显式设置为 true 才能进行删除操作
        /// </summary>
        [Required(ErrorMessage = "请确认删除")]
        public bool ConfirmDelete { get; set; }
    }
}