using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using IdentityService.Domain;

namespace IdentityService.Infrastructure
{
    /// <summary>
    /// 用户仓储实现
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly IdentityDbContext _context;

        public UserRepository(IdentityDbContext context)
        {
            _context = context;
        }

        //根据用户的唯一标识符（ID）异步获取用户信息。
        //返回符合条件的第一个用户或 null（如果未找到）
        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByUserNameAsync(string userName)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        //异步获取所有用户，支持分页
        //pageIndex：当前页码，默认为 0
        //pageSize：每页的用户数量，默认为 20
        public async Task<IEnumerable<User>> GetAllAsync(int pageIndex = 0, int pageSize = 20)
        {
            return await _context.Users
                .OrderByDescending(u => u.CreateTime)//使用 OrderByDescending 对用户按创建时间降序排序。最近创建的用户在前面。
                .Skip(pageIndex * pageSize)//使用 Skip 跳过前面指定数量的用户。
                .Take(pageSize)//使用 Take 获取当前页，需要的pageSize数量的用户。
                .ToListAsync();//返回指定页的用户列表。
        }

        //keyword用于搜索的关键字，可以是用户名或电子邮件的一部分
        //SearchAsync 方法主要用于在用户表中根据给定的关键字搜索用户，并允许分页查询。
        public async Task<IEnumerable<User>> SearchAsync(string keyword, int pageIndex = 0, int pageSize = 20)
        {
            //使用 _context.Users 获取用户集合，并将其转换为可查询的 LINQ 查询。AsQueryable 允许对查询进行动态修改。
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(u => u.UserName.Contains(keyword) || u.Email.Contains(keyword));
            }

            return await query
                .OrderByDescending(u => u.CreateTime)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> IsUserNameExistAsync(string userName)
        {
            return await _context.Users.AnyAsync(u => u.UserName == userName);
        }

        public async Task<bool> IsEmailExistAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public void Update(User user)
        {
            _context.Users.Update(user);
        }

        public void Remove(User user)
        {
            _context.Users.Remove(user);
        }

        public async Task<IEnumerable<Role>> GetUserRolesAsync(Guid userId)
        {
            return await _context.UserRoles //用户角色表
                .Where(ur => ur.UserId == userId)
                .Include(ur => ur.Role)//它告诉 EF Core 在执行查询时，也要检索并包含与主实体相关的其他实体。
                                       //例如，在你的代码中，使用 Include(ur => ur.Role) 表示在查询 UserRoles 时，
                                       //EF Core 也要加载与每个用户角色关联的 Role 信息。
                                       //这会有效地减少后续查询的需要，因为相关数据已经被一起加载了。
                .Select(ur => ur.Role)//使用 Select(ur => ur.Role) 是为了只提取与 UserRoles 相关的 Role 对象，
                                      //返回的结果将仅包含角色数据，而不是用户角色的整体信息。
                .ToListAsync();
        }

        public async Task AssignRoleAsync(Guid userId, Guid roleId)
        {
            var exists = await _context.UserRoles
                .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);//使用 AnyAsync 方法检测 UserRoles 表中是否已有记录，条件是 UserId 与 roleId 匹配。

            if (!exists)
            {
                var userRole = new UserRole(userId, roleId);
                await _context.UserRoles.AddAsync(userRole);
            }
        }

        public async Task RemoveRoleAsync(Guid userId, Guid roleId)
        {
            var userRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

            if (userRole != null)
            {
                _context.UserRoles.Remove(userRole);
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
