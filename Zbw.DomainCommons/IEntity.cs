using System;

namespace Zbw.DomainCommons
{
    /// <summary>
    /// 实体基接口
    /// 实体(Entity): 有唯一标识符，生命周期内标识不变
    /// Guid作为ID: 全局唯一，微服务间不会冲突
    /// 接口设计: 所有业务实体都实现这个接口，保证一致性
    /// </summary>
    public interface IEntity
    {
        public Guid Id { get; }
    }
}