using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Listening.Admin.WebAPI.Services
{
    /// <summary>
    /// 音频分析服务（跨平台版本，使用 FFmpeg）
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
        public async Task<int> GetAudioDurationInSecondsAsync(IFormFile audioFile)
        {
            try
            {
                _logger.LogInformation($"开始分析音频文件: {audioFile.FileName}");

                // 保存到临时文件
                var tempFilePath = Path.GetTempFileName();
                var audioTempPath = Path.ChangeExtension(tempFilePath, Path.GetExtension(audioFile.FileName));

                try
                {
                    using (var stream = new FileStream(audioTempPath, FileMode.Create))
                    {
                        await audioFile.CopyToAsync(stream);
                    }

                    // 使用 FFmpeg 获取时长
                    var duration = await GetAudioDurationUsingFFmpegAsync(audioTempPath);

                    _logger.LogInformation($"音频文件 {audioFile.FileName} 时长: {duration} 秒");

                    return duration;
                }
                finally
                {
                    // 清理临时文件
                    if (File.Exists(audioTempPath))
                        File.Delete(audioTempPath);
                    if (File.Exists(tempFilePath))
                        File.Delete(tempFilePath);
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
        /// 使用 FFmpeg/FFprobe 获取音频时长（跨平台方案）
        /// </summary>
        private async Task<int> GetAudioDurationUsingFFmpegAsync(string filePath)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "ffprobe",
                    Arguments = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{filePath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = startInfo };
                process.Start();

                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    _logger.LogError($"FFprobe 执行失败: {error}");
                    return 0;
                }

                // 解析输出（格式：123.456 秒）
                if (double.TryParse(output.Trim(), System.Globalization.NumberStyles.Float, 
                    System.Globalization.CultureInfo.InvariantCulture, out var durationSeconds))
                {
                    return (int)Math.Round(durationSeconds);
                }

                _logger.LogWarning($"无法解析 FFprobe 输出: {output}");
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"使用 FFmpeg 获取音频时长失败: {filePath}");
                return 0;
            }
        }

        /// <summary>
        /// 验证是否为支持的音频文件
        /// </summary>
        public static bool IsSupportedAudioFile(IFormFile audioFile)
        {
            var extension = Path.GetExtension(audioFile.FileName).ToLowerInvariant();
            var supportedExtensions = new[] { ".mp3", ".wav", ".m4a", ".aac", ".ogg", ".flac" };
            return Array.Exists(supportedExtensions, ext => ext == extension);
        }
    }
}