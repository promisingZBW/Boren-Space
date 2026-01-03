using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BCrypt.Net;

namespace IdentityService.Infrastructure
{
    /// <summary>
    /// 密码服务实现
    /// </summary>
    public class PasswordService : IPasswordService
    {
        //接收一个明文密码，并返回其哈希值
        //BCrypt.Net.BCrypt.HashPassword：使用 BCrypt 算法生成密码的哈希。
        //BCrypt.Net.BCrypt.GenerateSalt(12)：生成一个盐值，使用强度为 12 的哈希，以增加密码保护的强度。
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
        }

        //验证输入的明文密码是否与已存储的哈希密码匹配
        public bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch
            {
                return false;
            }
        }
    }
}