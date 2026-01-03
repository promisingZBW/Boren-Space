using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Zbw.JWT
{
    /// <summary>
    /// JWT服务实现
    /// </summary>
    public class JWTService : IJWTService
    {
        //依赖注入JWT配置选项
        private readonly JWTOptions _options;

        //IOptions<T> 是 ASP.NET Core 提供的一个接口，用于访问配置选项。它能够封装配置信息并提供对配置的强类型访问。
        //使用 IOptions<JWTOptions>，可以很容易地将配置项注入到服务中（这些配置选项一般在webapi项目的.json文件中定义）
        //而不需要手动解析配置文件。
        public JWTService(IOptions<JWTOptions> options)
        {
            _options = options.Value;
        }

        public string GenerateToken(IEnumerable<Claim> claims)
        {
            // 第1步：创建签名密钥
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
            //                                 ↑
            //                           将密钥字符串转换为字节数组
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            //                                            ↑
            //                                      使用HMAC SHA256算法签名
            // 黑名单机制需要添加JTI（JWT ID）到claims中
            var allClaims = claims.ToList();
            //这段代码可以在每次登录生成Token时，添加一个唯一的JTI（JWT ID）到claims中，以实现黑名单机制。这里实际上是注销机制，因为下次登录会重新生成新的jti，老的jti被加入黑名单也不影响。
            allClaims.Add(new Claim("jti", Guid.NewGuid().ToString()));

            // 第2步：创建JWT令牌
            var token = new JwtSecurityToken(
                issuer: _options.Issuer,    // 颁发者
                audience: _options.Audience,// 接收者 
                claims: allClaims,             // 用户信息，黑名单机制需要这一行也要从claims:Claims改成这样
                expires: DateTime.UtcNow.AddSeconds(_options.ExpireSeconds),// 过期时间
                signingCredentials: credentials// 签名凭据
            );

            // 第3步：将令牌对象转换为字符串
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                // 第1步：准备验证参数
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
                var parameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,              // 验证颁发者
                    ValidateAudience = true,            // 验证接收者
                    ValidateLifetime = true,            // 验证过期时间
                    ValidateIssuerSigningKey = true,    // 验证签名密钥
                    ValidIssuer = _options.Issuer,      // 期望的颁发者
                    ValidAudience = _options.Audience,  // 期望的接收者
                    IssuerSigningKey = key,             // 用于验证的密钥
                    ClockSkew = TimeSpan.Zero          // 时间偏差容忍（0=严格）
                };

                // 第2步：执行验证
                var handler = new JwtSecurityTokenHandler();
                var principal = handler.ValidateToken(token, parameters, out _);
                return principal;// 返回用户身份信息
            }
            catch
            {
                return null;
            }
        }
    }
}
