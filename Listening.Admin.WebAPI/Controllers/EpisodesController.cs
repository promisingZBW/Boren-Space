using Listening.Admin.WebAPI.DTOs;
using Listening.Admin.WebAPI.Services;
using Listening.Domain;
using Listening.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Zbw.ASPNETCore.DTOs;
using Listening.Domain.Utils;

namespace Listening.Admin.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class EpisodesController : ControllerBase
    {
        private readonly IListeningRepository _repository;
        private readonly FileValidationService _fileValidation;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<EpisodesController> _logger;
        private readonly AudioAnalysisService _audioAnalysis;

        public EpisodesController(
            IListeningRepository repository,
            FileValidationService fileValidation,
            IHttpClientFactory httpClientFactory,
            ILogger<EpisodesController> logger,
            AudioAnalysisService audioAnalysis)
        {
            _repository = repository;
            _fileValidation = fileValidation;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _audioAnalysis = audioAnalysis;
        }

        /// <summary>
        /// Create episode with file upload support
        /// </summary>
        [HttpPost]
        public async Task<ApiResponse<EpisodeResponse>> Create([FromForm] CreateEpisodeRequest request)
        {
            try
            {
                // 1. Validate audio file
                var audioValidation = _fileValidation.ValidateAudioFile(request.AudioFile);
                if (!audioValidation.IsValid)
                    return ApiResponse<EpisodeResponse>.ErrorResult($"Audio file validation failed: {audioValidation.ErrorMessage}");

                // 2. Validate subtitle file (optional)
                if (request.SubtitleFile != null)
                {
                    var subtitleValidation = _fileValidation.ValidateSubtitleFile(request.SubtitleFile);
                    if (!subtitleValidation.IsValid)
                        return ApiResponse<EpisodeResponse>.ErrorResult($"Subtitle file validation failed: {subtitleValidation.ErrorMessage}");
                }

                // 3. Validate cover image (optional)
                if (request.CoverImage != null)
                {
                    var imageValidation = _fileValidation.ValidateImageFile(request.CoverImage);
                    if (!imageValidation.IsValid)
                        return ApiResponse<EpisodeResponse>.ErrorResult($"Cover image validation failed: {imageValidation.ErrorMessage}");
                }

                // 4. Analyze audio duration
                _logger.LogInformation("Analyzing audio file duration...");
                var audioDurationInSeconds = await _audioAnalysis.GetAudioDurationInSecondsAsync(request.AudioFile);
                _logger.LogInformation($"Audio duration analyzed: {audioDurationInSeconds} seconds ({TimeFormatHelper.FormatDuration(audioDurationInSeconds)})");

                // 5. Upload audio file to FileService
                var audioFileId = await UploadFileToFileService(request.AudioFile, FileService.Domain.Enums.FileType.Audio);

                // 6. Upload subtitle file (if exists)
                Guid? subtitleFileId = null;
                if (request.SubtitleFile != null)
                    subtitleFileId = await UploadFileToFileService(request.SubtitleFile, FileService.Domain.Enums.FileType.Subtitle);

                // 7. Upload cover image (if exists)
                Guid? coverImageFileId = null;
                if (request.CoverImage != null)
                    coverImageFileId = await UploadFileToFileService(request.CoverImage, FileService.Domain.Enums.FileType.Image);

                // 8. Create Episode
                var episode = new Episode(request.Title);

                // Update basic info
                episode.UpdateInfo(request.Title, request.Description);

                // Set audio URL and duration
                var audioDownloadUrl = new Uri($"http://localhost:7292/api/File/{audioFileId}/download");
                episode.UpdateAudio(audioDownloadUrl, audioDurationInSeconds);

                // Set subtitle URL
                if (subtitleFileId.HasValue)
                {
                    var subtitleUrl = new Uri($"http://localhost:7292/api/File/{subtitleFileId}/download");
                    episode.UpdateSubtitle(subtitleUrl);
                }

                // Set cover image URL
                if (coverImageFileId.HasValue)
                {
                    var coverImageUrl = new Uri($"http://localhost:7292/api/File/{coverImageFileId}/download");
                    episode.UpdateCoverImage(coverImageUrl);
                }

                // 9. Save to database
                await _repository.AddAsync(episode);

                var response = ConvertToResponse(episode);

                var durationDisplay = TimeFormatHelper.FormatDuration(audioDurationInSeconds);
                return ApiResponse<EpisodeResponse>.SuccessResult(response,
                    $"Episode created successfully. Duration: {durationDisplay}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating episode");
                var errorMessage = $"Failed to create episode: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $" | Inner error: {ex.InnerException.Message}";
                }
                return ApiResponse<EpisodeResponse>.ErrorResult(errorMessage);
            }
        }

        /// <summary>
        /// Upload file to FileService
        /// </summary>
        private async Task<Guid> UploadFileToFileService(IFormFile file, FileService.Domain.Enums.FileType fileType)
        {
            var httpClient = _httpClientFactory.CreateClient();

            // Get JWT Token from current request and add to FileService request
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            using var content = new MultipartFormDataContent();
            using var fileContent = new StreamContent(file.OpenReadStream());
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "File", file.FileName);

            // Set storage type to Public
            content.Add(new StringContent("Public"), "StorageType");

            // Upload to FileService
            var response = await httpClient.PostAsync("http://localhost:7292/api/File/upload", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"File upload failed: {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();

            // Parse FileService response JSON
            using var jsonDoc = System.Text.Json.JsonDocument.Parse(responseContent);
            var dataElement = jsonDoc.RootElement.GetProperty("data");
            var fileIdString = dataElement.GetProperty("fileId").GetString();

            return Guid.Parse(fileIdString!);
        }

        /// <summary>
        /// Get all episodes
        /// </summary>
        [HttpGet]
        public async Task<ApiResponse<IEnumerable<EpisodeResponse>>> GetAll(
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = 20)
        {
            var episodes = await _repository.GetAllAsync(pageIndex, pageSize);
            var response = episodes.Select(ConvertToResponse);
            return ApiResponse<IEnumerable<EpisodeResponse>>.SuccessResult(response);
        }

        /// <summary>
        /// Get episode by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ApiResponse<EpisodeResponse>> GetById(Guid id)
        {
            var episode = await _repository.GetByIdAsync(id);
            if (episode == null)
            {
                return ApiResponse<EpisodeResponse>.ErrorResult("Episode not found", "EPISODE_NOT_FOUND");
            }

            var response = ConvertToResponse(episode);
            return ApiResponse<EpisodeResponse>.SuccessResult(response);
        }

        /// <summary>
        /// Update episode (only basic info, not files)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ApiResponse<EpisodeResponse>> Update(Guid id, [FromBody] UpdateEpisodeRequest request)
        {
            var episode = await _repository.GetByIdAsync(id);
            if (episode == null)
            {
                return ApiResponse<EpisodeResponse>.ErrorResult("Episode not found", "EPISODE_NOT_FOUND");
            }

            // Update basic info
            episode.UpdateInfo(request.Title, request.Description);

            // Update audio URL if provided
            if (!string.IsNullOrEmpty(request.AudioUrl))
            {
                episode.UpdateAudio(new Uri(request.AudioUrl), request.Duration);
            }

            _repository.Update(episode);

            var response = ConvertToResponse(episode);
            return ApiResponse<EpisodeResponse>.SuccessResult(response, "Episode updated successfully");
        }

        /// <summary>
        /// Delete episode (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ApiResponse> Delete(Guid id)
        {
            var episode = await _repository.GetByIdAsync(id);
            if (episode == null)
            {
                return ApiResponse.ErrorResult("Episode not found", "EPISODE_NOT_FOUND");
            }

            episode.SoftDelete();
            await _repository.SaveChangesAsync();

            return ApiResponse.SuccessResult("Episode deleted successfully");
        }

        /// <summary>
        /// Get all episodes including deleted ones
        /// </summary>
        [HttpGet("all-including-deleted")]
        public async Task<ApiResponse<IEnumerable<EpisodeResponse>>> GetAllIncludingDeleted(
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = 20)
        {
            var episodes = await _repository.GetAllIncludingDeletedAsync(pageIndex, pageSize);
            var response = episodes.Select(ConvertToResponse);
            return ApiResponse<IEnumerable<EpisodeResponse>>.SuccessResult(response);
        }

        /// <summary>
        /// Restore deleted episode
        /// </summary>
        [HttpPost("{id}/restore")]
        public async Task<ApiResponse<EpisodeResponse>> RestoreEpisode(Guid id)
        {
            var episode = await _repository.GetByIdIncludingDeletedAsync(id);
            if (episode == null)
            {
                return ApiResponse<EpisodeResponse>.ErrorResult("Episode not found", "EPISODE_NOT_FOUND");
            }

            if (!episode.IsDeleted)
            {
                return ApiResponse<EpisodeResponse>.ErrorResult("Episode is not deleted", "EPISODE_NOT_DELETED");
            }

            episode.Restore();
            _repository.Update(episode);

            var response = ConvertToResponse(episode);
            return ApiResponse<EpisodeResponse>.SuccessResult(response, "Episode restored successfully");
        }

        /// <summary>
        /// Convert Episode entity to EpisodeResponse DTO
        /// </summary>
        private EpisodeResponse ConvertToResponse(Episode episode)
        {
            return new EpisodeResponse
            {
                Id = episode.Id,
                Title = episode.Title,
                Description = episode.Description,
                AudioUrl = episode.AudioUrl?.ToString(),
                CoverImageUrl = episode.CoverImageUrl?.ToString(),
                SubtitleUrl = episode.SubtitleUrl?.ToString(),
                Duration = episode.Duration,
                CreateTime = episode.CreateTime,
                Sentences = episode.Sentences.Select(s => new SentenceResponse
                {
                    Id = s.Id,
                    Content = s.Content,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime
                }).ToList()
            };
        }
    }
}
