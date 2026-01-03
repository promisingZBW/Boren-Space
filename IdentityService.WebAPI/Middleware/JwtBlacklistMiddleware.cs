using IdentityService.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.WebAPI.Middleware
{
    /// <summary>
    /// JWT黑名单检查中间件
    /// 中间件一般在请求到达服务器之前（controllers之前）对其进行预处理
    /// 
    /// 在 ASP.NET Core 中，依赖注入（DI）系统支持三种生命周期，分别为：Singleton、Scoped 和 Transient。
    /// 1. Singleton
    ///定义：所有请求共享同一个实例。
    ///生命周期：
    ///在应用程序启动时创建实例，并在整个应用程序运行期间保持。
    ///适合一些只需要一个共享实例的服务，例如配置管理、日志记录等。
    ///注意：如果此实例保留状态，可能需要考虑并发访问的问题。
    ///2. Scoped
    ///定义：在每个请求的上下文中创建一个新的实例。
    ///生命周期：
    ///每当处理 HTTP 请求时，都会为该请求生成一个新的实例。
    ///当请求结束时，该实例会被释放。
    /// 
    /// 中间件是Singleton生命周期 - 应用启动时创建一次，整个应用生命周期内复用，不会消失
    /// 如果直接像下面这样写，可能导致这个实例一直存在，有并发问题
    /// public class JwtBlacklistMiddleware
    ///{
    /// private readonly ITokenBlacklistService _blacklistService; // Scoped 服务

    ///public JwtBlacklistMiddleware(RequestDelegate next, ITokenBlacklistService blacklistService)
    ///{
    ///_next = next;
    ///_blacklistService = blacklistService; // 危险！
    ///}
    ///}
    ///
    /// 而利用using var scope = _serviceScopeFactory.CreateScope();为每个请求创建新的Scope，每次调用新的实例，每次用完释放
    /// 生命周期安全
    /// </summary>
    public class JwtBlacklistMiddleware
    {
        //_next 用于调用链中的下一个中间件。通过调用 _next(context)，可以将控制权传递给下一个中间件，确保请求得到进一步处理
        private readonly RequestDelegate _next;
        //在 InvokeAsync 方法中，通过 _serviceScopeFactory.CreateScope() 创建新的服务范围，从而获取具体的服务（如 ITokenBlacklistService），
        //并确保其正确释放。这对于管理依赖的生命周期和作用域非常重要。
        //有关生命周期看上面解释
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public JwtBlacklistMiddleware(RequestDelegate next, IServiceScopeFactory serviceScopeFactory)
        {
            _next = next;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            //ExtractTokenFromHeader：从 HTTP 请求头中提取 JWT
            var token = ExtractTokenFromHeader(context);

            if (!string.IsNullOrEmpty(token))
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var blacklistService = scope.ServiceProvider.GetRequiredService<ITokenBlacklistService>();

                //ExtractTokenInfo：解码 JWT，提取其中的令牌 ID 和用户 ID。
                var (tokenId, userId) = ExtractTokenInfo(token);

                // 检查Token是否在黑名单中
                if (!string.IsNullOrEmpty(tokenId) && await blacklistService.IsBlacklistedAsync(tokenId))
                {
                    await WriteUnauthorizedResponse(context, "Token已被撤销");
                    return;
                }

                // 检查用户是否被全局禁用
                if (userId.HasValue)
                {
                    var userBlacklistService = blacklistService as TokenBlacklistService;
                    if (userBlacklistService != null && await userBlacklistService.IsUserBlacklistedAsync(userId.Value))
                    {
                        //构造并发送 401 Unauthorized 的 JSON 响应。
                        await WriteUnauthorizedResponse(context, "用户已被禁用");
                        return;
                    }
                }
            }

            await _next(context);
        }


        //从 HTTP 请求中安全地提取 Bearer 令牌（这里的Baerer令牌就是JWT令牌）
        //Bearer 令牌 是一个广义的术语，表示任何可以用来认证用户的令牌。
        //JWT 令牌 是 Bearer 令牌的一个具体实现，具有特定的格式和结构。所有的 JWT 令牌都是 Bearer 令牌，但并不是所有的 Bearer 令牌都是 JWT。
        private string? ExtractTokenFromHeader(HttpContext context)
        {
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (authHeader?.StartsWith("Bearer ") == true)
            {
                return authHeader.Substring("Bearer ".Length).Trim();
            }
            return null;
        }

        //ExtractTokenInfo：解码 JWT，提取其中的令牌 ID 和用户 ID。
        private (string? tokenId, Guid? userId) ExtractTokenInfo(string token)
        {
            try
            {
                //使用 JwtSecurityTokenHandler 类来处理和读取传入的 JWT 令牌。
                var jwtHandler = new JwtSecurityTokenHandler();
                //ReadJwtToken(token) 方法将令牌解析为 JwtSecurityToken 对象。
                var jsonToken = jwtHandler.ReadJwtToken(token);

                var tokenId = jsonToken.Claims.FirstOrDefault(c => c.Type == "jti")?.Value;
                var userIdClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                Guid? userId = null;
                //Guid.TryParse 方法: 这个方法尝试将字符串转换为 GUID。
                //如果转换成功，返回 true，并通过 out 参数 parsedUserId 返回解析后的 GUID 值。
                if (Guid.TryParse(userIdClaim, out var parsedUserId))
                {
                    userId = parsedUserId;
                }

                return (tokenId, userId);
            }
            //如果令牌格式不正确或解析失败，返回 null
            catch
            {
                return (null, null);
            }
        }

        //将构造好的响应对象（response）变为 JSON 字符串，并将其异步写入到 HTTP 响应中，
        //向客户端返回 401 Unauthorized 状态的 JSON 响应体
        private async Task WriteUnauthorizedResponse(HttpContext context, string message)
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";

            var response = new { message, success = false, errorCode = "TOKEN_REVOKED" };
            /*
             例如，如果 response 是 { message: "Unauthorized", success: false, errorCode: "TOKEN_REVOKED" }，
            序列化后会得到字符串 {"message":"Unauthorized","success":false,"errorCode":"TOKEN_REVOKED"}
             */
            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
        }
    }
}