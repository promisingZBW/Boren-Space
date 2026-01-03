using System;
using Zbw.DomainCommons;

namespace Listening.Domain.Events
{
    /// <summary>
    /// 剧集删除事件
    /// </summary>
    public record EpisodeDeletedEvent(Guid EpisodeId) : IDomainEvent;
}
