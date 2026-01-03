using System;
using System.Threading.Tasks;
using Zbw.DomainCommons;

namespace Zbw.EventBus
{
    /// <summary>
    /// 事件总线接口
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// 发布事件
        /// </summary>
        Task PublishAsync<T>(T @event) where T : IDomainEvent;

        /// <summary>
        /// 订阅事件，它告诉事件总线，当特定类型的事件发生时，应该调用哪个处理器来处理这个事件。
        /// </summary>
        void Subscribe<T, THandler>()
            where T : IDomainEvent //T 是一个领域事件类型，必须实现 IDomainEvent 接口
            where THandler : class, IEventHandler<T>;// THandler 是一个事件处理器类型，必须是一个类，并且实现了 IEventHandler<T> 接口
    }
}
