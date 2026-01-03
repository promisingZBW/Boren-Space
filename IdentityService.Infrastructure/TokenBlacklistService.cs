using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Memory;

namespace IdentityService.Infrastructure
{
    /// <summary>
    /// Token黑名单服务实现（使用内存缓存方便学习）
    /// 后续可以替换为Redis分布式缓存
    /// </summary>
    public class TokenBlacklistService : ITokenBlacklistService
    {
        private readonly IMemoryCache _cache;
        /*
         * BLACKLIST_PREFIX：用作令牌黑名单的键前缀，所有被加入黑名单的令牌都会以这个前缀开头。
         * 例如，令牌ID为abc123的黑名单键可能是blacklist:abc123。
            USER_PREFIX：用作用户黑名单的键前缀，用于标识特定用户的黑名单状态。
        比如，用户ID为12345的黑名单键可能是user_blacklist:12345。
         */
        private const string BLACKLIST_PREFIX = "blacklist:";
        private const string USER_PREFIX = "user_blacklist:";

        public TokenBlacklistService(IMemoryCache cache)
        {
            _cache = cache;
        }

        //该方法添加令牌到内存缓存中，直到该令牌的过期时间到达。专注于处理具体某个令牌的状态。
        public Task AddToBlacklistAsync(string tokenId, DateTime expireTime)
        {
            var key = BLACKLIST_PREFIX + tokenId;// blacklist:abc123
            //使用 expireTime 减去当前 UTC 时间 (DateTime.UtcNow)，计算令牌距离过期还有多少时间。
            var expiration = expireTime.Subtract(DateTime.UtcNow);

            if (expiration.TotalSeconds > 0)
            {
                _cache.Set(key, "revoked", expiration);//"revoked" 字符串表示该令牌被标记为无效
            }

            return Task.CompletedTask;//表示该异步操作已完成。由于此方法并没有执行任何异步操作，这里直接返回一个已完成的任务。
        }

        //检查令牌是否在黑名单中
        public Task<bool> IsBlacklistedAsync(string tokenId)
        {
            var key = BLACKLIST_PREFIX + tokenId;
            //调用内存缓存的 TryGetValue 方法，尝试从缓存中获取指定键的值。out _ 表示我们不需要记录取出的值，只关心键的存在性。
            var exists = _cache.TryGetValue(key, out _);
            //使用 Task.FromResult 方法封装 exists 布尔值并返回。这个方法创建一个已完成的任务，携带指定结果。
            return Task.FromResult(exists);
        }

        //该方法将整个用户的所有令牌列入黑名单，并设置一个较长的有效期（例如 25 小时），
        //确保在此期间该用户的所有新令牌和现有令牌都无法被使用。
        public Task BlacklistUserTokensAsync(Guid userId)
        {
            var key = USER_PREFIX + userId.ToString();
            // 设置用户黑名单标记，25小时过期（比Token过期时间长一点，多一层保险）
            _cache.Set(key, DateTime.UtcNow, TimeSpan.FromHours(25));
            return Task.CompletedTask;
        }


        /// <summary>
        /// 检查用户是否被全局禁用
        /// </summary>
        public Task<bool> IsUserBlacklistedAsync(Guid userId)
        {
            var key = USER_PREFIX + userId.ToString();
            return Task.FromResult(_cache.TryGetValue(key, out _));
        }
    }
}