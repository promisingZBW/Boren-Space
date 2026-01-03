using Listening.Domain.Utils;
using System;
using System.Collections.Generic;

namespace Listening.Main.WebAPI.DTOs
{
    /// <summary>
    /// 剧集详情响应（用户学习用）
    /// </summary>
    public class EpisodeDetailResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? AudioUrl { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? SubtitleUrl { get; set; }
        public int Duration { get; set; }
        /// <summary>
        /// 友好的时长显示（如：3min 25s）
        /// </summary>
        public string DurationDisplay => TimeFormatHelper.FormatDuration(Duration); // 统一格式

        /// <summary>
        /// 时间格式的时长显示（如：03:25）- 适合播放器界面
        /// </summary>
        public string DurationTimeFormat => TimeFormatHelper.FormatDurationAsTime(Duration); // 播放器格式
        public List<SentenceDetailResponse> Sentences { get; set; } = new();
    }

    /// <summary>
    /// 句子详情响应
    /// </summary>
    public class SentenceDetailResponse
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string StartTimeDisplay => StartTime.ToString(@"mm\:ss");
        public string EndTimeDisplay => EndTime.ToString(@"mm\:ss");
    }
}