using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Infrastructure
{
    /// <summary>
    /// Token黑名单服务接口
    /// </summary>
    public interface ITokenBlacklistService
    {
        /// <summary>
        /// 将Token加入黑名单
        /// </summary>
        Task AddToBlacklistAsync(string tokenId, DateTime expireTime);

        /// <summary>
        /// 检查Token是否在黑名单中
        /// </summary>
        Task<bool> IsBlacklistedAsync(string tokenId);

        /// <summary>
        /// 将用户的所有Token加入黑名单
        /// </summary>
        Task BlacklistUserTokensAsync(Guid userId);
    }
}