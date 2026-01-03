using System.ComponentModel.DataAnnotations;

namespace Listening.Admin.WebAPI.DTOs
{
    /// <summary>
    /// 更新剧集请求
    /// </summary>
    public class UpdateEpisodeRequest
    {
        [Required(ErrorMessage = "标题不能为空")]
        [MaxLength(200, ErrorMessage = "标题长度不能超过200字符")]
        public string Title { get; set; } = string.Empty;


        [Url(ErrorMessage = "请提供有效的URL地址")]
        public string? AudioUrl { get; set; }
        public int Duration { get; set; }

        public string? Description { get; set; } // 添加描述字段
    }
}
