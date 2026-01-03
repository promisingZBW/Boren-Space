using FileService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Zbw.Infrastructure.EFCore;

namespace FileService.Infrastructure.Data
{
    /// <summary>
    /// 文件服务数据库上下文
    /// </summary>
    public class FSDbContext : BaseDbContext
    {
        public DbSet<UploadedItem> UploadedItems { get; set; }

        public FSDbContext(DbContextOptions<FSDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 配置UploadedItem实体
            modelBuilder.Entity<UploadedItem>(entity =>
            {
                entity.ToTable("UploadedItems");
                entity.HasKey(e => e.Id);

                // 基本属性配置
                entity.Property(e => e.FileName)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.FileSHA256Hash)
                    .IsRequired()
                    .HasMaxLength(64); // SHA256固定64个字符

                entity.Property(e => e.StorageKey)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.ContentType)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.FileSizeInBytes)
                    .IsRequired();

                entity.Property(e => e.UploadTime)
                    .IsRequired();

                entity.Property(e => e.UploaderId)
                    .IsRequired();

                entity.Property(e => e.IsDeleted)
                    .IsRequired()
                    .HasDefaultValue(false);

                // URI类型转换 - 允许为空
                entity.Property(e => e.BackupUrl)
                    .HasConversion(
                        uri => uri != null ? uri.ToString() : null,  // 🔧 处理null值
                        str => !string.IsNullOrEmpty(str) ? new Uri(str) : null)  // 🔧 处理null字符串
                    .HasMaxLength(1000);  // 🔧 移除 .IsRequired()

                entity.Property(e => e.RemoteUrl)
                    .HasConversion(
                        uri => uri != null ? uri.ToString() : null,  // 🔧 处理null值
                        str => !string.IsNullOrEmpty(str) ? new Uri(str) : null)  // 🔧 处理null字符串
                    .HasMaxLength(1000);  // 🔧 移除 .IsRequired()

                // 索引配置
                entity.HasIndex(e => new { e.FileSizeInBytes, e.FileSHA256Hash })
                    .HasDatabaseName("IX_UploadedItems_FileSize_Hash")
                    .IsUnique(); // 用于文件去重

                entity.HasIndex(e => e.UploaderId)
                    .HasDatabaseName("IX_UploadedItems_UploaderId");

                entity.HasIndex(e => e.UploadTime)
                    .HasDatabaseName("IX_UploadedItems_UploadTime");

                entity.HasIndex(e => e.IsDeleted)
                    .HasDatabaseName("IX_UploadedItems_IsDeleted");

                // 全局查询过滤器：排除已删除的记录
                entity.HasQueryFilter(e => !e.IsDeleted);
            });
        }
    }
}