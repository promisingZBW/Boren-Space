using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace CommonInitializer
{
    /// <summary>
    /// Web应用程序扩展方法
    /// </summary>
    public static class WebApplicationExtensions
    {
        /// <summary>
        /// 使用默认中间件配置
        /// </summary>
        public static void UseZbwDefault(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
        }
    }
}