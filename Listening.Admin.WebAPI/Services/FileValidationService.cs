using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


/*
    FileValidationService为什么放在 Listening.Admin.WebAPI 项目中？
    1. 业务逻辑归属
    FileValidationService 是专门为听力管理功能服务的
    它验证的是听力相关的文件类型（音频、字幕、封面）
    属于 Listening.Admin.WebAPI 的业务逻辑层
    2. 依赖关系
    该服务直接被 EpisodesController 使用
    不需要被其他微服务共享
    避免不必要的跨项目依赖
 */

namespace Listening.Admin.WebAPI.Services
{
    /// <summary>
    /// 文件验证服务
    /// </summary>
    public class FileValidationService
    {
        //_allowedFileTypes 通常用于文件上传、文件存储等操作，需要根据文件扩展名判断合法性
        private readonly Dictionary<string, string[]> _allowedFileTypes = new()
        {
            { "audio", new[] { ".mp3", ".wav", ".m4a", ".aac" } },
            { "subtitle", new[] { ".srt", ".vtt" } },
            { "image", new[] { ".jpg", ".jpeg", ".png", ".webp" } }
        };

        //_allowedMimeTypes 通常用于 HTTP 响应或请求中，帮助服务器或客户端理解文件内容的类型
        private readonly Dictionary<string, string[]> _allowedMimeTypes = new()
        {
            //这里表示与音频文件相关的 MIME 类型，例如 audio/mpeg 对应于 .mp3 文件
            { "audio", new[] { "audio/mpeg", "audio/wav", "audio/mp4", "audio/aac", "audio/x-m4a", "audio/m4a" } },
            { "subtitle", new[] { "text/plain", "application/x-subrip" } },
            { "image", new[] { "image/jpeg", "image/png", "image/webp" } }
        };

        /// <summary>
        /// 验证音频文件
        /// </summary>
        public (bool IsValid, string ErrorMessage) ValidateAudioFile(IFormFile file)
        {
            var basicValidation = ValidateFile(file, "audio", maxSizeInMB: 100);
            if (!basicValidation.IsValid)
                return basicValidation;

            // 额外验证：检查是否为支持的音频文件
            if (!AudioAnalysisService.IsSupportedAudioFile(file))
            {
                return (false, "不支持的音频文件格式");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// 验证字幕文件
        /// </summary>
        public (bool IsValid, string ErrorMessage) ValidateSubtitleFile(IFormFile file)
        {
            return ValidateFile(file, "subtitle", maxSizeInMB: 5);
        }

        /// <summary>
        /// 验证图片文件
        /// </summary>
        public (bool IsValid, string ErrorMessage) ValidateImageFile(IFormFile file)
        {
            return ValidateFile(file, "image", maxSizeInMB: 10);
        }

        /// <summary>
        /// 通用文件验证方法
        /// </summary>
        private (bool IsValid, string ErrorMessage) ValidateFile(IFormFile file, string fileCategory, int maxSizeInMB)
        {
            if (file == null || file.Length == 0)
                return (false, "文件不能为空");

            // 检查文件大小
            var maxSizeInBytes = maxSizeInMB * 1024 * 1024;
            if (file.Length > maxSizeInBytes)
                return (false, $"文件大小不能超过 {maxSizeInMB}MB");

            // 检查文件扩展名
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedFileTypes[fileCategory].Contains(extension))
                return (false, $"不支持的文件格式。支持的格式：{string.Join(", ", _allowedFileTypes[fileCategory])}");

            // 检查MIME类型（可选，因为有些客户端可能不准确）
            if (!string.IsNullOrEmpty(file.ContentType) &&
                !_allowedMimeTypes[fileCategory].Contains(file.ContentType.ToLowerInvariant()))
            {
                // 只是警告，不阻止上传
                Console.WriteLine($"警告：文件MIME类型可能不准确：{file.ContentType}");
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// 获取文件类型枚举
        /// </summary>
        public FileService.Domain.Enums.FileType GetFileType(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (_allowedFileTypes["audio"].Contains(extension))
                return FileService.Domain.Enums.FileType.Audio;
            if (_allowedFileTypes["image"].Contains(extension))
                return FileService.Domain.Enums.FileType.Image;

            return FileService.Domain.Enums.FileType.Other;
        }
    }
}
