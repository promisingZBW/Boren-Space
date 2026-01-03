using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zbw.DomainCommons;

namespace IdentityService.Domain.Events
{
    /// <summary>
    /// 用户密码修改事件
    /// </summary>
    public record UserPasswordChangedEvent(Guid UserId) : IDomainEvent;

    /// <summary>
    /// 用户禁用事件
    /// </summary>
    public record UserDeactivatedEvent(Guid UserId) : IDomainEvent;

    /// <summary>
    /// 用户删除事件
    /// </summary>
    public record UserDeletedEvent(Guid UserId) : IDomainEvent;
}
