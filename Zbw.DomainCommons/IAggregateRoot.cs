using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zbw.DomainCommons
{
    /// <summary>
    /// 聚合根接口 - DDD核心概念
    /// </summary>
    public interface IAggregateRoot : IEntity
    {
        /// <summary>
        /// 获取领域事件列表
        /// </summary>
        IReadOnlyList<IDomainEvent> GetDomainEvents();

        /// <summary>
        /// 清空领域事件
        /// </summary>
        void ClearDomainEvents();
    }
}
