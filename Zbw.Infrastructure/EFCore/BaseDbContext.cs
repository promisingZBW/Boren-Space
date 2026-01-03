using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Zbw.DomainCommons;

namespace Zbw.Infrastructure.EFCore
{
    /// <summary>
    /// 数据库上下文基类，提供通用功能
    /// </summary>
    public abstract class BaseDbContext : DbContext
    {
        public BaseDbContext(DbContextOptions options) : base(options)
        {
        }

        /// <summary>
        /// 保存更改并发布领域事件
        /// </summary>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // 从 EF Core 变更跟踪器中收集领域事件
            var domainEvents = ChangeTracker.Entries<IEntity>()
                .Where(x => x.Entity is IAggregateRoot)
                .SelectMany(x => ((IAggregateRoot)x.Entity).GetDomainEvents())
                .ToList();

            // 保存到数据库
            var result = await base.SaveChangesAsync(cancellationToken);

            // 然后发布事件（目前注释掉，MediatR 就是在这里用）
            // await PublishDomainEventsAsync(domainEvents);

            return result;
        }
    }
}
