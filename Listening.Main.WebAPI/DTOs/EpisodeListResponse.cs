using System;
using Listening.Domain.Utils;

namespace Listening.Main.WebAPI.DTOs
{
    /// <summary>
    /// 剧集列表响应（用户端简化版）
    /// </summary>
    public class EpisodeListResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? AudioUrl { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? SubtitleUrl { get; set; }
        public int Duration { get; set; }
        public DateTime CreateTime { get; set; }


        /// <summary>
        /// 时长显示（如：3min 25s）
        /// </summary>
        public string DurationDisplay => TimeFormatHelper.FormatDuration(Duration); // 统一格式

        /// <summary>
        /// 时间格式的时长显示（如：03:25）- 适合播放器界面
        /// </summary>
        public string DurationTimeFormat => TimeFormatHelper.FormatDurationAsTime(Duration); // 播放器格式
    }
}
