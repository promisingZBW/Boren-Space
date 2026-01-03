using System;
using Zbw.DomainCommons;

namespace Listening.Domain.Events
{
    /// <summary>
    /// 剧集音频更新事件
    /// </summary>
    public record EpisodeAudioUpdatedEvent(Guid EpisodeId, Uri AudioUrl, int Duration) : IDomainEvent;
}
