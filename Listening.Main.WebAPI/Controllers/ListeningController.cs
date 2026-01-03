using Listening.Infrastructure;
using Listening.Main.WebAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zbw.ASPNETCore.DTOs;

namespace Listening.Main.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous] 
    public class ListeningController : ControllerBase
    {
        private readonly IListeningRepository _repository;

        public ListeningController(IListeningRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// 这个接口用于获取剧集列表，支持分页查询。
        /// 下面返回的信息包括剧集的ID、标题、描述、音频URL、封面图片URL、字幕URL、持续时间、创建时间。
        /// 返回的信息很具体，但事实上这里只需要展示剧集的基本信息，不需要包含音频URL、字幕URL等详细内容。
        /// 但返回了这些信息也无妨，客户端可以根据需要选择性使用。
        /// </summary>
        [HttpGet("episodes")]
        public async Task<ApiResponse<IEnumerable<EpisodeListResponse>>> GetEpisodes(
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = 20)
        {
            var episodes = await _repository.GetAllAsync(pageIndex, pageSize);
            var response = episodes.Select(episode => new EpisodeListResponse
            {
                Id = episode.Id,
                Title = episode.Title,
                Description = episode.Description,
                AudioUrl = episode.AudioUrl?.ToString(),
                CoverImageUrl = episode.CoverImageUrl?.ToString(),
                SubtitleUrl = episode.SubtitleUrl?.ToString(),
                Duration = episode.Duration,
                CreateTime = episode.CreateTime,
            });

            return ApiResponse<IEnumerable<EpisodeListResponse>>.SuccessResult(response);
        }

        /// <summary>
        /// 这个接口用于获取指定ID的剧集详细信息。
        /// 但其实上面获取剧集列表的接口GetEpisodes返回的信息已经过于详细
        /// 在我这个项目中，其实前端只需要从GetEpisodes返回的信息获取音频地址和字幕地址即可播放和显示字幕，
        /// 但是这里仍然保留了这个接口以防后续有更详细信息的需求。比如返回一些只有在查看某单一剧集详情时才需要的信息。
        /// 而且如果未来有几百个剧集，列表接口返回的数据很大，这时候可以简化GetEpisodes，GetEpisodeDetail便有意义了。
        /// </summary>
        [HttpGet("episodes/{id}")]
        public async Task<ApiResponse<EpisodeDetailResponse>> GetEpisodeDetail(Guid id)
        {
            var episode = await _repository.GetByIdAsync(id);
            if (episode == null)
            {
                return ApiResponse<EpisodeDetailResponse>.ErrorResult("EPISODE_NOT_FOUND");
            }

            var response = new EpisodeDetailResponse
            {
                Id = episode.Id,
                Title = episode.Title,
                Description = episode.Description,
                AudioUrl = episode.AudioUrl?.ToString(),
                CoverImageUrl = episode.CoverImageUrl?.ToString(),
                SubtitleUrl = episode.SubtitleUrl?.ToString(),
                Duration = episode.Duration,
                Sentences = episode.Sentences
                    .OrderBy(s => s.StartTime)
                    .Select(s => new SentenceDetailResponse
                    {
                        Id = s.Id,
                        Content = s.Content,
                        StartTime = s.StartTime,
                        EndTime = s.EndTime
                    }).ToList()
            };

            return ApiResponse<EpisodeDetailResponse>.SuccessResult(response);
        }

        /// <summary>
        /// 
        /// </summary>
        [HttpPost("search")]
        public async Task<ApiResponse<IEnumerable<EpisodeListResponse>>> Search([FromBody] SearchRequest request)
        {
            var episodes = await _repository.SearchAsync(request.Keyword ?? "", request.PageIndex, request.PageSize);
            var response = episodes.Select(episode => new EpisodeListResponse
            {
                Id = episode.Id,
                Title = episode.Title,
                Description = episode.Description,
                AudioUrl = episode.AudioUrl?.ToString(),
                CoverImageUrl = episode.CoverImageUrl?.ToString(),
                SubtitleUrl = episode.SubtitleUrl?.ToString(),
                Duration = episode.Duration,
                CreateTime = episode.CreateTime,
            });

            return ApiResponse<IEnumerable<EpisodeListResponse>>.SuccessResult(response);
        }

        /// <summary>
        /// 
        /// </summary>
        [HttpGet("episodes/popular")]
        public async Task<ApiResponse<IEnumerable<EpisodeListResponse>>> GetPopularEpisodes()
        {
            
            var episodes = await _repository.GetAllAsync(0, 10);
            var response = episodes.Select(episode => new EpisodeListResponse
            {
                Id = episode.Id,
                Title = episode.Title,
                Description = episode.Description,
                AudioUrl = episode.AudioUrl?.ToString(),
                CoverImageUrl = episode.CoverImageUrl?.ToString(),
                SubtitleUrl = episode.SubtitleUrl?.ToString(),
                Duration = episode.Duration,
                CreateTime = episode.CreateTime,
            });

            return ApiResponse<IEnumerable<EpisodeListResponse>>.SuccessResult(response);
        }

        /// <summary>
        /// 
        /// </summary>
        [HttpGet("sentences/{id}")]
        public async Task<ApiResponse<SentenceDetailResponse>> GetSentence(Guid id)
        {
            
            var episodes = await _repository.GetAllAsync(0, int.MaxValue);
            var sentence = episodes
                .SelectMany(e => e.Sentences)
                .FirstOrDefault(s => s.Id == id);

            if (sentence == null)
            {
                return ApiResponse<SentenceDetailResponse>.ErrorResult("���Ӳ�����", "SENTENCE_NOT_FOUND");
            }

            var response = new SentenceDetailResponse
            {
                Id = sentence.Id,
                Content = sentence.Content,
                StartTime = sentence.StartTime,
                EndTime = sentence.EndTime
            };

            return ApiResponse<SentenceDetailResponse>.SuccessResult(response);
        }
    }
}