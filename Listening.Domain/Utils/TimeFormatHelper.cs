using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Listening.Domain.Utils
{
    /// <summary>
    /// 时间格式化帮助类
    /// </summary>
    public static class TimeFormatHelper
    {
        /// <summary>
        /// 将秒数转换为友好的时间格式字符串
        /// </summary>
        /// <param name="totalSeconds">总秒数</param>
        /// <returns>格式化的时间字符串（如：3min 25s）</returns>
        public static string FormatDuration(int totalSeconds)
        {
            if (totalSeconds <= 0)
                return "0s";

            var minutes = totalSeconds / 60;
            var seconds = totalSeconds % 60;

            if (minutes > 0)
            {
                return $"{minutes}min {seconds}s";
            }
            else
            {
                return $"{seconds}s";
            }
        }

        /// <summary>
        /// 将秒数转换为 MM:SS 格式
        /// </summary>
        /// <param name="totalSeconds">总秒数</param>
        /// <returns>MM:SS 格式的字符串（如：03:25）</returns>
        public static string FormatDurationAsTime(int totalSeconds)
        {
            if (totalSeconds <= 0)
                return "00:00";

            var timeSpan = TimeSpan.FromSeconds(totalSeconds);
            return timeSpan.ToString(@"mm\:ss");
        }

        /// <summary>
        /// 将秒数转换为详细的时间格式
        /// </summary>
        /// <param name="totalSeconds">总秒数</param>
        /// <returns>详细格式的字符串（如：1h 23min 45s）</returns>
        public static string FormatDurationDetailed(int totalSeconds)
        {
            if (totalSeconds <= 0)
                return "0s";

            var hours = totalSeconds / 3600;
            var minutes = (totalSeconds % 3600) / 60;
            var seconds = totalSeconds % 60;

            var parts = new List<string>();

            if (hours > 0)
                parts.Add($"{hours}h");
            if (minutes > 0)
                parts.Add($"{minutes}min");
            if (seconds > 0)
                parts.Add($"{seconds}s");

            return string.Join(" ", parts);
        }
    }
}
