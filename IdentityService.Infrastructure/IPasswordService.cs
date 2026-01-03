using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Infrastructure
{
    /// <summary>
    /// 密码服务接口
    /// </summary>
    public interface IPasswordService
    {
        /// <summary>
        /// 哈希密码
        /// </summary>
        string HashPassword(string password);

        /// <summary>
        /// 验证密码
        /// </summary>
        bool VerifyPassword(string password, string hashedPassword);
    }
}