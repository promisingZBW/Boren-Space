using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zbw.DomainCommons;

namespace IdentityService.Domain
{
    /// <summary>
    /// 用户角色关联
    /// </summary>
    public class UserRole : IEntity
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public Guid RoleId { get; private set; }
        public DateTime AssignTime { get; private set; }

        // 导航属性
        public User User { get; private set; } = null!;
        public Role Role { get; private set; } = null!;

        public UserRole(Guid userId, Guid roleId)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            RoleId = roleId;
            AssignTime = DateTime.Now;
        }

        // EF Core 需要的无参构造函数
        private UserRole() { }
    }
}