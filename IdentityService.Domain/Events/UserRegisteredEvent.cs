using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Zbw.DomainCommons;

namespace IdentityService.Domain.Events
{
    /// <summary>
    /// 用户注册事件
    /// </summary>
    public record UserRegisteredEvent(Guid UserId, string UserName, string Email) : IDomainEvent;
}