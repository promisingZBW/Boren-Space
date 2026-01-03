using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace FileService.Domain.Enums
{
    /// <summary>
    /// 文件类型枚举
    /// </summary>
    public enum FileType
    {
        /// <summary>
        /// 音频文件
        /// </summary>
        Audio,

        /// <summary>
        /// 字幕文件
        /// </summary>
        Subtitle,

        /// <summary>
        /// 图片文件
        /// </summary>
        Image,

        /// <summary>
        /// 其他类型
        /// </summary>
        Other
    }
}