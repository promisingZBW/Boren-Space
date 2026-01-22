using IdentityService.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection.Emit;
using Zbw.Infrastructure.EFCore;

namespace IdentityService.Infrastructure
{
    /// <summary>
    /// 身份服务数据库上下文
    /// </summary>
    public class IdentityDbContext : BaseDbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //调用基类（DbContext）的 OnModelCreating 方法，以确保在配置模型时不丢失基类的任何设置。
            base.OnModelCreating(modelBuilder);

            // 配置User实体
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(u => u.Id);

                entity.Property(u => u.UserName)
                    .IsRequired()//表示该字段是必填的
                    .HasMaxLength(50);

                entity.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(u => u.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(u => u.PhoneNumber)
                    .HasMaxLength(20);

                entity.Property(u => u.CreateTime)
                    .IsRequired();

                entity.Property(u => u.IsActive)
                    .IsRequired();

                entity.Property(u => u.IsDeleted)
                    .IsRequired();

                // entity.HasIndex(u => u.UserName)添加唯一索引
                //.IsUnique()表示索引唯一
                //.HasFilter("[IsDeleted] = 0");表示该唯一索引只在 IsDeleted 列的值为 0 时生效。这意味着：
                //如果某个用户名被标记为已删除（IsDeleted = 1），该用户名可以被其他非删除用户使用。
                entity.HasIndex(u => u.UserName)
                    .IsUnique()
                    .HasFilter("\"IsDeleted\" = false");

                entity.HasIndex(u => u.Email)
                    .IsUnique()
                    .HasFilter("\"IsDeleted\" = false");

                // 全局查询过滤器：排除已删除的用户,即 IsDeleted 为 true 的用户在查询中不会被返回
                entity.HasQueryFilter(u => !u.IsDeleted);
            });

            // 配置Role实体
            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Roles");
                entity.HasKey(r => r.Id);

                entity.Property(r => r.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(r => r.Description)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(r => r.CreateTime)
                    .IsRequired();

                // 角色名称唯一
                entity.HasIndex(r => r.Name)
                    .IsUnique();
            });

            // 配置UserRole实体
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.ToTable("UserRoles");
                entity.HasKey(ur => ur.Id);

                entity.Property(ur => ur.UserId)
                    .IsRequired();

                entity.Property(ur => ur.RoleId)
                    .IsRequired();

                entity.Property(ur => ur.AssignTime)
                    .IsRequired();

                // 配置外键关系
                entity.HasOne(ur => ur.User)// 每条 UserRole 记录只指向“一个”User（多对一）
                    .WithMany()             // 但一个 User 可以对应“多条”UserRole（WithMany）
                    .HasForeignKey(ur => ur.UserId)// 外键为 UserId
                    .OnDelete(DeleteBehavior.Cascade);// User 被删除时，关联的 UserRole 也级联删除

                entity.HasOne(ur => ur.Role)
                    .WithMany()
                    .HasForeignKey(ur => ur.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                // 用户-角色组合唯一    唯一索引：保证同一个用户和角色的组合不会重复
                entity.HasIndex(ur => new { ur.UserId, ur.RoleId })
                    .IsUnique();

                // 🎯 新增：全局查询过滤器，只显示未删除用户的角色关系
                entity.HasQueryFilter(ur => !ur.User.IsDeleted);
            });

            // 种子数据
            SeedData(modelBuilder);
        }

        //种子数据是用于填充数据库中的初始数据的一种机制。管理员是一定要有的角色，可以在这里预先创建好。
        //比如系统启动时需要的用户、角色、权限或配置等。
        private void SeedData(ModelBuilder modelBuilder)
        {
            // 创建默认角色role，而不是创建user
            //因为我们在Role中设置了 public Guid Id { get; private set; }，所以需要32位的Guid
            var adminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");

            //SeedData 方法向 Role 实体添加管理员角色数据。调用 SeedData(modelBuilder); 能确保管理员角色在数据库初始化时被插入。
            modelBuilder.Entity<Role>().HasData(
                new { Id = adminRoleId, Name = "Admin", Description = "系统管理员", CreateTime = DateTime.Now }
            );
        }
    }
}