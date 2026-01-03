using System.ComponentModel.DataAnnotations;

namespace Listening.Main.WebAPI.DTOs
{
    /// <summary>
    /// 搜索请求
    /// </summary>
    public class SearchRequest
    {
        [MaxLength(100, ErrorMessage = "搜索关键词长度不能超过100字符")]
        public string? Keyword { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "页码必须大于等于0")]
        public int PageIndex { get; set; } = 0;

        [Range(1, 50, ErrorMessage = "每页数量必须在1-50之间")]
        public int PageSize { get; set; } = 20;
    }
}