using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IdentityService.Domain;

namespace IdentityService.Infrastructure
{
    /// <summary>
    /// 用户仓储接口
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// 根据ID获取用户
        /// </summary>
        Task<User?> GetByIdAsync(Guid id);

        /// <summary>
        /// 根据用户名获取用户
        /// </summary>
        Task<User?> GetByUserNameAsync(string userName);

        /// <summary>
        /// 根据邮箱获取用户
        /// </summary>
        Task<User?> GetByEmailAsync(string email);

        /// <summary>
        /// 获取所有用户（分页）
        /// </summary>
        Task<IEnumerable<User>> GetAllAsync(int pageIndex = 0, int pageSize = 20);

        /// <summary>
        /// 搜索用户
        /// </summary>
        Task<IEnumerable<User>> SearchAsync(string keyword, int pageIndex = 0, int pageSize = 20);

        /// <summary>
        /// 检查用户名是否存在
        /// </summary>
        Task<bool> IsUserNameExistAsync(string userName);

        /// <summary>
        /// 检查邮箱是否存在
        /// </summary>
        Task<bool> IsEmailExistAsync(string email);

        /// <summary>
        /// 添加用户（上传实体）
        /// </summary>
        Task AddAsync(User user);

        /// <summary>
        /// 更新用户
        /// </summary>
        void Update(User user);

        /// <summary>
        /// 删除用户
        /// </summary>
        void Remove(User user);

        /// <summary>
        /// 获取用户的角色
        /// </summary>
        Task<IEnumerable<Role>> GetUserRolesAsync(Guid userId);

        /// <summary>
        /// 为用户分配角色
        /// </summary>
        Task AssignRoleAsync(Guid userId, Guid roleId);

        /// <summary>
        /// 移除用户角色
        /// </summary>
        Task RemoveRoleAsync(Guid userId, Guid roleId);

        /// <summary>
        /// 数据库保存更改
        /// 其实不需要，因为通过EF Core的工作单元模式可以自动保存上传的实体更改
        /// </summary>
        Task<int> SaveChangesAsync();
    }
}
