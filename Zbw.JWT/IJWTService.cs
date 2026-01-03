using System.Collections.Generic;
using System.Security.Claims;

namespace Zbw.JWT
{
    /// <summary>
    /// JWT服务接口
    /// </summary>
    public interface IJWTService
    {
        /// <summary>
        /// 生成JWT Token
        /// </summary>
        string GenerateToken(IEnumerable<Claim> claims);

        /// <summary>
        /// 验证JWT Token
        /// </summary>
        ClaimsPrincipal? ValidateToken(string token);
    }
}
