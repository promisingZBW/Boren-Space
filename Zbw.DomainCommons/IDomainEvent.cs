using MediatR;

namespace Zbw.DomainCommons
{
    /// <summary>
    /// 领域事件接口
    /// 领域事件: 业务发生时触发的事件（如"用户注册成功"）
    /// 继承INotification: 利用MediatR模式，实现事件的发布订阅
    /// 解耦设计: 事件发布者不需要知道谁在监听
    /// MediatR的作用：
    /// 解耦: Controller不直接调用业务逻辑
    /// 统一: 所有请求都通过MediatR路由
    /// AOP: 可以添加日志、验证、缓存等横切关注点
    /// 
    /// </summary>
    public interface IDomainEvent : INotification
    {
    }
}