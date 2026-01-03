using System.Threading.Tasks;
using Zbw.DomainCommons;

namespace Zbw.EventBus
{
    /// <summary>
    /// 事件处理器接口
    /// IDomainEvent是一个领域事件的标记接口，表示这是一个领域事件
    /// 领域事件在这个项目中有很多实现，比如用户注册事件、订单创建事件等
    /// 
    /// T是一个泛型参数，表示具体的领域事件类型
    /// 当 T = UserRegisteredEvent 时：
    ///  Task HandleAsync(UserRegisteredEvent @event);
    /// 当 T = OrderCreatedEvent 时：  
    /// Task HandleAsync(OrderCreatedEvent @event);
    /// 当 T = UserLoginEvent 时：
    /// Task HandleAsync(UserLoginEvent @event);
    /// 
    /// @event 就是一个参数名，代表"任何实现了IDomainEvent的事件对象"。
    /// public interface IUserRegisteredHandler
    ///{
    ///Task HandleAsync(UserRegisteredEvent userEvent);
    ///}

    ///public interface IUserLoginHandler
    ///{
    ///Task HandleAsync(UserLoginEvent loginEvent);
    ///}

    /// public interface IOrderCreatedHandler
    ///{
    ///Task HandleAsync(OrderCreatedEvent orderEvent);
    ///}
    ///... 每种事件都要写一个接口！
    ///
    ///假设有继承关系：
    ///public class BaseEvent : IDomainEvent { }
    ///public class UserEvent : BaseEvent{ }
    /// 有了 in 关键字，可以这样：
    ///IEventHandler<BaseEvent> generalHandler = new SomeHandler();
    ///IEventHandler<UserEvent> specificHandler = generalHandler; // ✅ 
    ///
    /// 如果你能处理"更大范围"的事件，那你也能处理"更小范围"的事件
    /// 就像：如果你会修"所有汽车"，那你肯定也会修"奔驰汽车"
    /// </summary>
    public interface IEventHandler<in T> where T : IDomainEvent // where表示泛型参数 T 必须实现 IDomainEvent 接口
    {
        Task HandleAsync(T @event);
    }
}
