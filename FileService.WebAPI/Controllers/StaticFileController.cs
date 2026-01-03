using FileService.Domain.Enums;
using FileService.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileService.WebAPI.Controllers
{
    /// <summary>
    /// 静态文件访问控制器
    /// </summary>
    [ApiController]
    [Route("files")]
    [AllowAnonymous]
    public class StaticFileController : ControllerBase
    {
        private readonly IStorageClient _localStorageClient;
        private readonly IStorageClient _remoteStorageClient;
        private readonly ILogger<StaticFileController> _logger;
        private readonly IFSRepository _repository;

        public StaticFileController(
            IEnumerable<IStorageClient> storageClients,
            ILogger<StaticFileController> logger,
            IFSRepository repository)
        {
            var clients = storageClients.ToList();
            _localStorageClient = clients.First(x => x.StorageType == StorageType.Backup);
            _remoteStorageClient = clients.First(x => x.StorageType == StorageType.Public);
            _logger = logger;
            _repository = repository;
        }

        /// <summary>
        /// 直接路径访问本地文件
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns>文件流</returns>
        [HttpGet("{*path}")]
        public async Task<IActionResult> GetFile(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    return BadRequest("文件路径不能为空");
                }

                // 检查文件是否存在
                if (!await _localStorageClient.ExistsAsync(path))
                {
                    return NotFound("文件不存在");
                }

                // 获取文件流
                var stream = await _localStorageClient.GetAsync(path);

                // 尝试根据文件扩展名确定 Content-Type
                var contentType = GetContentType(path);

                return File(stream, contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取静态文件失败: {Path}", path);
                return StatusCode(500, "服务器内部错误");
            }
        }


        private string GetContentType(string path)
        {
            var extension = Path.GetExtension(path).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".pdf" => "application/pdf",
                ".txt" => "text/plain",
                ".mp3" => "audio/mpeg",
                ".mp4" => "video/mp4",
                ".json" => "application/json",
                _ => "application/octet-stream"
            };
        }


        /// <summary>
        /// 通过文件ID下载（不包含中文字符的URL）
        /// </summary>
        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadById(Guid id)
        {
            try
            {
                var file = await _repository.GetByIdAsync(id);
                if (file == null || file.IsDeleted)
                {
                    return NotFound("文件不存在");
                }

                // 记录公开下载
                _logger.LogInformation("公开下载文件: {FileName} ({FileId})", file.FileName, id);

                // 重定向到S3的URL安全链接（现在StorageKey是URL安全的）
                if (file.RemoteUrl != null)
                {
                    return Redirect(file.RemoteUrl.ToString());
                }

                // 备选：通过存储Key访问本地文件
                if (!string.IsNullOrEmpty(file.StorageKey))
                {
                    var stream = await _localStorageClient.GetAsync(file.StorageKey);
                    return File(stream, file.ContentType, file.FileName); // 用户看到原始中文文件名
                }

                return NotFound("文件无法访问");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "通过ID下载文件失败: {FileId}", id);
                return StatusCode(500, "下载失败");
            }
        }


    }
}
