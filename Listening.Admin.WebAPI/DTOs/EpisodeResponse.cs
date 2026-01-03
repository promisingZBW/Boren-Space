using Listening.Admin.WebAPI.Services;
using System;
using System.Collections.Generic;
using Listening.Domain.Utils;

namespace Listening.Admin.WebAPI.DTOs
{
    /// <summary>
    /// 剧集响应
    /// </summary>
    public class EpisodeResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? AudioUrl { get; set; }
        public int Duration { get; set; }
        public DateTime CreateTime { get; set; }
        public List<SentenceResponse> Sentences { get; set; } = new();

        /// <summary>
        /// 描述信息
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 封面图片URL
        /// </summary>
        public string? CoverImageUrl { get; set; } 

        /// <summary>
        /// 字幕文件URL
        /// </summary>
        public string? SubtitleUrl { get; set; }

        /// <summary>
        /// 友好的时长显示（如：3min 25s）
        /// </summary>
        public string DurationDisplay => TimeFormatHelper.FormatDuration(Duration); // 使用共享工具类

        /// <summary>
        /// 时间格式的时长显示（如：03:25）
        /// </summary>
        public string DurationTimeFormat => TimeFormatHelper.FormatDurationAsTime(Duration); // 提供多种格式
    }

    /// <summary>
    /// 句子响应
    /// </summary>
    public class SentenceResponse
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
