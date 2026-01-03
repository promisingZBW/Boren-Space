using IdentityService.Domain.Events;
using System;
using System.Collections.Generic;
using Zbw.DomainCommons;

namespace IdentityService.Domain
{
    /// <summary>
    /// 用户实体 - 聚合根
    /// </summary>
    public class User : IAggregateRoot
    {
        public Guid Id { get; private set; }
        public string UserName { get; private set; }
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }
        public string? PhoneNumber { get; private set; }
        public DateTime CreateTime { get; private set; }
        public DateTime? LastLoginTime { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsDeleted { get; private set; }

        private readonly List<IDomainEvent> _domainEvents = new();

        public User(string userName, string email, string passwordHash)
        {
            Id = Guid.NewGuid();
            UserName = userName;
            Email = email;
            PasswordHash = passwordHash;
            CreateTime = DateTime.Now;
            IsActive = true;
            IsDeleted = false;

            // 添加用户注册事件
            _domainEvents.Add(new UserRegisteredEvent(Id, userName, email));
        }

        // EF Core 需要的无参构造函数
        private User()
        {
            UserName = string.Empty;
            Email = string.Empty;
            PasswordHash = string.Empty;
        }

        /// <summary>
        /// 更新密码
        /// </summary>
        public void UpdatePassword(string newPasswordHash)
        {
            PasswordHash = newPasswordHash;
            _domainEvents.Add(new UserPasswordChangedEvent(Id));
        }

        /// <summary>
        /// 更新用户信息
        /// </summary>
        public void UpdateProfile(string? phoneNumber)
        {
            PhoneNumber = phoneNumber;
        }

        /// <summary>
        /// 记录登录
        /// </summary>
        public void RecordLogin()
        {
            LastLoginTime = DateTime.Now;
            _domainEvents.Add(new UserLoggedInEvent(Id, LastLoginTime.Value));
        }

        /// <summary>
        /// 激活用户
        /// </summary>
        public void Activate()
        {
            IsActive = true;
        }

        /// <summary>
        /// 禁用用户
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
            _domainEvents.Add(new UserDeactivatedEvent(Id));
        }

        /// <summary>
        /// 软删除
        /// </summary>
        public void SoftDelete()
        {
            IsDeleted = true;
            IsActive = false;
            _domainEvents.Add(new UserDeletedEvent(Id));
        }

        public IReadOnlyList<IDomainEvent> GetDomainEvents()
        {
            return _domainEvents.AsReadOnly();
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }
}