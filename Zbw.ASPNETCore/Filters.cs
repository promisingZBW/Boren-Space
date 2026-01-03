using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;


namespace Zbw.ASPNETCore.Filters
{
    /// <summary>
    /// 工作单元过滤器 - 自动管理数据库事务
    /// </summary>

    public class UnitOfWorkFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)

        {
            // ⭐ Action开始：Controller方法执行前
            // Controller方法开始执行
            var result = await next();

            // 🔥 只有在Action执行成功后才保存更改

            if (result.Exception == null)
            {
                Console.WriteLine("UnitOfWorkFilter: Action执行成功，开始保存数据");
                var serviceProvider = context.HttpContext.RequestServices;
                var dbContexts = serviceProvider.GetServices<DbContext>().ToList();
                Console.WriteLine($"找到 {dbContexts.Count} 个DbContext");
                foreach (var dbContext in dbContexts)
                {
                    Console.WriteLine($"UnitOfWorkFilter: 准备保存 {dbContext.GetType().Name}");
                    try
                    {
                        //收集在Controller执行过程中的所有数据库变更，一次性提交所有变更（原子操作），提交数据库事务
                        var changes = await dbContext.SaveChangesAsync();
                        Console.WriteLine($"UnitOfWorkFilter: {dbContext.GetType().Name} 保存了 {changes} 行");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"保存 {dbContext.GetType().Name} 时出错: {ex.Message}");
                        throw; // 如果有异常，事务自动回滚，不调用SaveChangesAsync
                    }
                }
            }
            else
            {
                Console.WriteLine($"❌ Action执行失败，跳过保存: {result.Exception?.Message}");
            }
        }

    }

}
