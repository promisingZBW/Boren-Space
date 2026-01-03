using Listening.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using Zbw.Infrastructure.EFCore;

namespace Listening.Infrastructure
{
    /// <summary>
    /// 听力服务数据库上下文
    /// </summary>
    public class ListeningDbContext : BaseDbContext
    {
        public DbSet<Episode> Episodes { get; set; }
        public DbSet<Sentence> Sentences { get; set; }

        public ListeningDbContext(DbContextOptions<ListeningDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 配置Episode实体
            modelBuilder.Entity<Episode>(entity =>
            {
                entity.ToTable("Episodes");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);


                entity.Property(e => e.AudioUrl)
                    .HasConversion(
                        uri => uri == null ? null : uri.ToString(),
                        str => string.IsNullOrEmpty(str) ? null : new Uri(str));

                entity.Property(e => e.CreateTime)
                    .IsRequired();

                entity.Property(e => e.IsDeleted)
                    .IsRequired();

                // 配置一对多关系：Episode -> Sentences
                entity.HasMany(e => e.Sentences)
                    .WithOne()
                    .HasForeignKey("EpisodeId")
                    .OnDelete(DeleteBehavior.Cascade);

                // 全局查询过滤器：排除已删除的记录
                //每当你查询对应的实体类型时（如 DbSet<TEntity>），这个过滤器会自动应用。
                //无论是使用 LINQ 查询，还是从数据库加载数据，都会自动排除 IsDeleted 属性为 true 的记录。
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // 配置Sentence实体
            modelBuilder.Entity<Sentence>(entity =>
            {
                entity.ToTable("Sentences");
                entity.HasKey(s => s.Id);

                entity.Property(s => s.Content)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.Property(s => s.StartTime)
                    .IsRequired();

                entity.Property(s => s.EndTime)
                    .IsRequired();

                // 添加EpisodeId外键属性
                entity.Property<Guid>("EpisodeId")
                    .IsRequired();
            });
        }
    }
}