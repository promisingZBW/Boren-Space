using System;
using Zbw.DomainCommons;

namespace IdentityService.Domain
{
    /// <summary>
    /// 角色实体
    /// </summary>
    public class Role : IEntity
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public DateTime CreateTime { get; private set; }

        public Role(string name, string description)
        {
            Id = Guid.NewGuid();
            Name = name;
            Description = description;
            CreateTime = DateTime.Now;
        }

        // EF Core 需要的无参构造函数
        private Role()
        {
            Name = string.Empty;//设置空字符串以避免空引用异常
            Description = string.Empty;
        }

        /// <summary>
        /// 更新角色信息
        /// </summary>
        public void Update(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}