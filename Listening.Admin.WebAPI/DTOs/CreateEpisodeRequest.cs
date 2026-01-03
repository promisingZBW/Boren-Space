using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Listening.Admin.WebAPI.DTOs
{
    /// <summary>
    /// 创建剧集请求
    /// </summary>
    public class CreateEpisodeRequest
    {
        [Required(ErrorMessage = "标题不能为空")]
        [MaxLength(200, ErrorMessage = "标题长度不能超过200字符")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 描述信息
        /// </summary>
        [MaxLength(2000, ErrorMessage = "描述长度不能超过2000字符")]
        public string? Description { get; set; }

        /// <summary>
        /// 音频文件（MP3格式）- 必需
        /// </summary>
        [Required(ErrorMessage = "音频文件不能为空")]
        public IFormFile AudioFile { get; set; } = null!;

        /// <summary>
        /// 字幕文件（SRT格式）- 可选
        /// </summary>
        public IFormFile? SubtitleFile { get; set; }

        /// <summary>
        /// 封面图片 - 可选
        /// </summary>
        public IFormFile? CoverImage { get; set; }
    }
}