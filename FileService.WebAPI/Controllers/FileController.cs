using FileService.Domain.Enums;
using FileService.Domain.Interfaces;
using FileService.Domain.Services;
using FileService.WebAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Zbw.ASPNETCore.DTOs;

namespace FileService.WebAPI.Controllers
{
    /// <summary>
    /// 文件管理控制器
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FileController : ControllerBase
    {
        private readonly FSDomainService _domainService;
        private readonly IFSRepository _repository;
        private readonly ILogger<FileController> _logger;


        public FileController(
            FSDomainService domainService,
            IFSRepository repository,
            IEnumerable<IStorageClient> storageClients,
            ILogger<FileController> logger)
        {
            _domainService = domainService;
            _repository = repository;
            _logger = logger;

        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="request">上传请求</param>
        /// <returns>上传结果</returns>
        [HttpPost("upload")]
        [Authorize(Roles = "Admin")] //只有管理员可以上传
        public async Task<ApiResponse<UploadResponse>> Upload([FromForm] UploadRequest request)
        {
            try
            {
                // 验证存储类型
                if (!Enum.TryParse<StorageType>(request.StorageType, true, out var storageType))
                {
                    return ApiResponse<UploadResponse>.ErrorResult("无效的存储类型");
                }

                // 获取当前用户ID
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var uploaderId))
                {
                    return ApiResponse<UploadResponse>.ErrorResult("无法获取用户信息");
                }

                // 验证文件
                if (request.File.Length == 0)
                {
                    return ApiResponse<UploadResponse>.ErrorResult("文件不能为空");
                }

                // 文件大小限制 (50MB)
                const long maxFileSize = 50 * 1024 * 1024;
                if (request.File.Length > maxFileSize)
                {
                    return ApiResponse<UploadResponse>.ErrorResult("文件大小不能超过50MB");
                }

                // 调用领域服务上传文件
                using var stream = request.File.OpenReadStream();
                var uploadedItem = await _domainService.UploadAsync(
                    stream,
                    request.File.FileName,
                    request.File.ContentType,
                    uploaderId,
                    storageType
                   );

                // AddAsync将实体添加到DbContext，之后工作单元才能自动保存到数据库
                await _repository.AddAsync(uploadedItem);

                _logger.LogInformation("文件上传成功: {FileName} by {UserId}",
                    request.File.FileName, uploaderId);

                var response = new UploadResponse
                {
                    FileId = uploadedItem.Id,
                    FileName = uploadedItem.FileName,
                    FileSizeInBytes = uploadedItem.FileSizeInBytes,
                    BackupUrl = uploadedItem.BackupUrl?.ToString(),
                    RemoteUrl = uploadedItem.RemoteUrl?.ToString(),
                    UploadTime = uploadedItem.UploadTime
                };

                return ApiResponse<UploadResponse>.SuccessResult(response, "文件上传成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "文件上传失败: {FileName}", request.File?.FileName);
                return ApiResponse<UploadResponse>.ErrorResult("文件上传失败");
            }
        }

        /// <summary>
        /// 获取文件信息
        /// </summary>
        /// <param name="id">文件ID</param>
        /// <returns>文件信息</returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ApiResponse<FileInfoResponse>> GetFileInfo(Guid id)
        {
            try
            {
                var file = await _repository.GetByIdAsync(id);
                if (file == null)
                {
                    return ApiResponse<FileInfoResponse>.ErrorResult("文件不存在");
                }

                var response = new FileInfoResponse
                {
                    Id = file.Id,
                    FileName = file.FileName,
                    FileSizeInBytes = file.FileSizeInBytes,
                    FileSHA256Hash = file.FileSHA256Hash,
                    UploaderId = file.UploaderId,
                    BackupUrl = file.BackupUrl?.ToString(),
                    RemoteUrl = file.RemoteUrl?.ToString(),
                    ContentType = file.ContentType,
                    UploadTime = file.UploadTime
                };

                return ApiResponse<FileInfoResponse>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取文件信息失败: {FileId}", id);
                return ApiResponse<FileInfoResponse>.ErrorResult("获取文件信息失败");
            }
        }


        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="id">文件ID</param>
        /// <returns>文件流</returns>
        [HttpGet("{id}/download")]
        [AllowAnonymous] // 添加这行，允许匿名访问，不需要登录就能下载文件
        public async Task<IActionResult> Download(Guid id)
        {
            try
            {
                var file = await _repository.GetByIdAsync(id);
                if (file == null || file.IsDeleted)
                {
                    return NotFound("文件不存在");
                }


                // ?? 移除用户认证检查，记录匿名下载
                _logger.LogInformation("公开下载文件: {FileName} ({FileId})", file.FileName, id);

                // ?? 重定向到StaticFile控制器，使用文件ID（无中文字符）
                //FileController:负责用户认证、权限检查、业务逻辑
                //StaticFileController: 负责文件服务、静态资源访问
                //这段代码返回: HTTP 302 重定向到 /StaticFile/DownloadById?id=123
                //把下载请求转发给 StaticFile 控制器处理，职责分离
                return RedirectToAction("DownloadById", "StaticFile", new { id = id });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "下载文件失败: {FileId}", id);
                return BadRequest("下载文件失败");
            }
        }

        /// <summary>
        /// 获取当前用户的文件列表（增强版）   不知道这个功能有没有用，再看
        /// </summary>
        /// <param name="page">页码（从1开始）</param>
        /// <param name="pageSize">每页数量（默认20，最大100）</param>
        /// <param name="sortBy">排序字段（UploadTime, FileName, FileSize）</param>
        /// <param name="sortDesc">是否降序排列</param>
        /// <returns>文件列表</returns>
        [HttpGet("my-files")]
        [AllowAnonymous]
        public async Task<ApiResponse<PagedFileListResponse>> GetMyFiles(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string sortBy = "UploadTime",
            [FromQuery] bool sortDesc = true)
        {
            try
            {
                // 参数验证
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 20;
                if (pageSize > 100) pageSize = 100;

                // 获取当前用户ID
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                 ?? User.FindFirst("sub")?.Value
                                 ?? User.FindFirst("userId")?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var currentUserId))
                {
                    return ApiResponse<PagedFileListResponse>.ErrorResult("无法获取用户信息");
                }

                // 计算跳过的记录数
                var skip = (page - 1) * pageSize;

                // 获取文件列表（这里需要Repository支持分页和排序）
                var (files, totalCount) = await _repository.GetUserFilesPagedAsync(
                    currentUserId,
                    skip,
                    pageSize,
                    sortBy,
                    sortDesc);

                // 转换为响应格式
                var fileList = files.Select(f => new FileInfoResponse
                {
                    Id = f.Id,
                    FileName = f.FileName,
                    FileSizeInBytes = f.FileSizeInBytes,
                    FileSHA256Hash = f.FileSHA256Hash,
                    UploaderId = f.UploaderId,
                    BackupUrl = f.BackupUrl?.ToString(),
                    RemoteUrl = f.RemoteUrl?.ToString(),
                    ContentType = f.ContentType,
                    UploadTime = f.UploadTime,
                    //新增：统一的下载链接
                    DownloadUrl = $"/api/File/{f.Id}/download"
                }).ToList();

                // 构建分页响应
                var response = new PagedFileListResponse
                {
                    Files = fileList,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    HasNextPage = page * pageSize < totalCount,
                    HasPreviousPage = page > 1
                };

                _logger.LogInformation("用户 {UserId} 获取文件列表: 第{Page}页，共{Total}个文件",
                    currentUserId, page, totalCount);

                return ApiResponse<PagedFileListResponse>.SuccessResult(response,
                    $"获取到第{page}页，共{response.TotalPages}页，{totalCount}个文件");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户文件列表失败");
                return ApiResponse<PagedFileListResponse>.ErrorResult("获取文件列表失败");
            }
        }


        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="request">删除请求</param>
        /// <returns>删除结果</returns>
        [HttpDelete("delete")]
        [Authorize(Roles = "Admin")]
        public async Task<ApiResponse<DeleteFileResponse>> DeleteFile([FromBody] DeleteFileRequest request)
        {
            try
            {
                // 验证确认删除
                if (!request.ConfirmDelete)
                {
                    return ApiResponse<DeleteFileResponse>.ErrorResult("请确认删除操作");
                }

                // 获取当前用户ID
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(userIdClaim, out var currentUserId))
                {
                    return ApiResponse<DeleteFileResponse>.ErrorResult("无法获取用户信息");
                }

                // 先获取文件信息用于响应
                var fileInfo = await _repository.GetByIdAndUploaderAsync(request.FileId, currentUserId);
                if (fileInfo == null)
                {
                    return ApiResponse<DeleteFileResponse>.ErrorResult("文件不存在或无权限删除");
                }

                // 执行删除操作
                var (success, message, remoteDeleted, backupDeleted) = await _domainService.DeleteAsync(
                    request.FileId,
                    currentUserId);

                if (!success)
                {
                    return ApiResponse<DeleteFileResponse>.ErrorResult(message);
                }

                _logger.LogInformation("文件删除成功: {FileName} ({FileId}) by {UserId}.",
                    fileInfo.FileName, request.FileId, currentUserId);

                var response = new DeleteFileResponse
                {
                    FileId = request.FileId,
                    FileName = fileInfo.FileName,
                    FileSizeInBytes = fileInfo.FileSizeInBytes,
                    DeleteTime = DateTime.UtcNow,
                    Message = message,
                    RemoteDeleted = remoteDeleted,
                    BackupDeleted = backupDeleted
                };

                return ApiResponse<DeleteFileResponse>.SuccessResult(response, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除文件失败: {FileId}", request.FileId);
                return ApiResponse<DeleteFileResponse>.ErrorResult("删除文件失败");
            }
        }



        /// <summary>
        /// 直接获取文件内容（避免重定向CORS问题）
        /// </summary>
        /// <param name="id">文件ID</param>
        /// <returns>文件流</returns>
        [HttpGet("{id}/content")]
        [AllowAnonymous] // 允许匿名访问
        public async Task<IActionResult> GetFileContent(Guid id)
        {
            try
            {
                var file = await _repository.GetByIdAsync(id);
                if (file == null || file.IsDeleted)
                {
                    return NotFound("文件不存在");
                }

                _logger.LogInformation("直接获取文件内容: {FileName} ({FileId})", file.FileName, id);

                // 从远程存储获取文件
                if (file.RemoteUrl != null)
                {
                    using var httpClient = new HttpClient();
                    var response = await httpClient.GetAsync(file.RemoteUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var contentStream = await response.Content.ReadAsStreamAsync();

                        // 设置CORS头
                        // 不需要，也不能在后端设置CORS头，试图修改来自远程存储的响应头
                        //Response.Headers.Add("Access-Control-Allow-Origin", "*");
                        //Response.Headers.Add("Access-Control-Allow-Methods", "GET, HEAD, OPTIONS");
                        //Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Range");

                        /*
                         这里的 enableRangeProcessing: true 参数允许客户端进行范围请求（Range Requests）
                        什么叫范围请求？
                        当你在网页上用 <video> 播放一个视频时，比如：<video controls src="/api/video.mp4"></video>
                        浏览器并不会一次性下载整个视频文件，而是会根据播放进度，分段请求视频数据。
                        比如服务器发送一个请求，带上一个 Range 头部：Range: bytes=0-65535   表示请求视频的前64KB数据。
                        然后服务器只返回这部分数据，浏览器开始播放。随着播放进度的推进，浏览器会继续发送更多的 Range 请求，获取后续的数据段。
                        当用户快进到视频/音频的中间位置时，浏览器会发送另一个请求：Range: bytes=1048576-2097151  请求第1MB到第2MB的数据。
                        如果enableRangeProcessing: false，服务器会忽略这些Range请求，始终返回整个文件。
                        所以每次跳转都要下载整个文件，且无法跳到对应位置，而是从头播放，因为服务器看不到浏览器的Range请求。
                         */
                        return File(contentStream, file.ContentType, file.FileName, enableRangeProcessing: true);
                    }
                }

                return NotFound("文件内容无法获取");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取文件内容失败: {FileId}", id);
                return StatusCode(500, "获取文件失败");
            }
        }


    }
}