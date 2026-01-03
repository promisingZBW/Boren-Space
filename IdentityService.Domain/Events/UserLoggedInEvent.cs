using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Zbw.DomainCommons;

namespace IdentityService.Domain.Events
{
    /// <summary>
    /// 用户登录事件
    /// </summary>
    public record UserLoggedInEvent(Guid UserId, DateTime LoginTime) : IDomainEvent;
}