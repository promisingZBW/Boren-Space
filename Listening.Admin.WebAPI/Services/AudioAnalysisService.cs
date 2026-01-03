using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Listening.Domain.Utils;

namespace Listening.Admin.WebAPI.Services
{
    /// <summary>
    /// 音频分析服务
    /// </summary>
    public class AudioAnalysisService
    {
        private readonly ILogger<AudioAnalysisService> _logger;

        public AudioAnalysisService(ILogger<AudioAnalysisService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 获取音频文件时长（秒）
        /// </summary>
        /// <param name="audioFile">音频文件</param>
        /// <returns>时长（秒）</returns>
        public async Task<int> GetAudioDurationInSecondsAsync(IFormFile audioFile)
        {
            try
            {
                _logger.LogInformation($"开始分析音频文件: {audioFile.FileName}");

                // 将上传的文件保存到临时位置
                var tempFilePath = Path.GetTempFileName();
                var audioTempPath = Path.ChangeExtension(tempFilePath, Path.GetExtension(audioFile.FileName));

                try
                {
                    // 保存上传的文件到临时路径
                    using (var stream = new FileStream(audioTempPath, FileMode.Create))
                    {
                        await audioFile.CopyToAsync(stream);
                    }

                    // 分析音频文件
                    var duration = GetAudioDuration(audioTempPath);

                    _logger.LogInformation($"音频文件 {audioFile.FileName} 时长: {duration} 秒");

                    return duration;
                }
                finally
                {
                    // 清理临时文件
                    if (File.Exists(audioTempPath))
                    {
                        File.Delete(audioTempPath);
                    }
                    if (File.Exists(tempFilePath))
                    {
                        File.Delete(tempFilePath);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"分析音频文件 {audioFile.FileName} 时发生错误");

                // 如果分析失败，返回0，不阻止文件上传
                return 0;
            }
        }

        /// <summary>
        /// 从文件路径获取音频时长
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>时长（秒）</returns>
        private int GetAudioDuration(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();

            try
            {
                switch (extension)
                {
                    case ".mp3":
                        return GetMp3Duration(filePath);
                    case ".wav":
                        return GetWavDuration(filePath);
                    case ".m4a":
                    case ".aac":
                        // 对于 M4A/AAC 文件，尝试使用 MediaFoundationReader
                        return GetMediaFoundationDuration(filePath);
                    default:
                        _logger.LogWarning($"不支持的音频格式: {extension}，尝试使用 MediaFoundation 处理");
                        return GetMediaFoundationDuration(filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"读取音频文件 {filePath} 时长失败");
                return 0;
            }
        }

        /// <summary>
        /// 获取MP3文件时长
        /// </summary>
        private int GetMp3Duration(string filePath)
        {
            try
            {
                using var reader = new Mp3FileReader(filePath);
                var totalTime = reader.TotalTime;
                return (int)Math.Round(totalTime.TotalSeconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"读取MP3文件 {filePath} 失败");
                throw;
            }
        }

        /// <summary>
        /// 获取WAV文件时长
        /// </summary>
        private int GetWavDuration(string filePath)
        {
            try
            {
                using var reader = new WaveFileReader(filePath);
                var totalTime = reader.TotalTime;
                return (int)Math.Round(totalTime.TotalSeconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"读取WAV文件 {filePath} 失败");
                throw;
            }
        }

        /// <summary>
        /// 使用 MediaFoundation 获取音频时长（支持 M4A, AAC 等格式）
        /// </summary>
        private int GetMediaFoundationDuration(string filePath)
        {
            try
            {
                // 使用 AudioFileReader，它内部会使用 MediaFoundation
                using var reader = new AudioFileReader(filePath);
                var totalTime = reader.TotalTime;
                return (int)Math.Round(totalTime.TotalSeconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"使用 MediaFoundation 读取文件 {filePath} 失败");
                // 如果 MediaFoundation 失败，尝试作为 MP3 处理
                try
                {
                    _logger.LogWarning($"回退到 MP3 解析器处理 {filePath}");
                    return GetMp3Duration(filePath);
                }
                catch
                {
                    _logger.LogError($"所有解析方法都失败了，文件: {filePath}");
                    throw;
                }
            }
        }

        /// <summary>
        /// 验证是否为支持的音频文件
        /// </summary>
        /// <param name="audioFile">音频文件</param>
        /// <returns>是否支持</returns>
        public static bool IsSupportedAudioFile(IFormFile audioFile)
        {
            var extension = Path.GetExtension(audioFile.FileName).ToLowerInvariant();
            // 添加更多支持的格式
            var supportedExtensions = new[] { ".mp3", ".wav", ".m4a", ".aac" };

            return supportedExtensions.Contains(extension);
        }
    }
}