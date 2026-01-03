using System;
using System.Collections.Generic;
using Zbw.DomainCommons;

namespace Listening.Domain
{
    /// <summary>
    /// 听力剧集 - 聚合根
    /// </summary>
    public class Episode : IAggregateRoot
    {
        public Guid Id { get; private set; }          // 唯一标识符
        public string Title { get; private set; }     // 剧集标题

        //Uri 类型不仅在编译期帮你保证地址格式正确，
        //也在运行时提供了解析与操作 URL 的丰富能力，并且能够无缝对接 .NET 中大量依赖 URI 的 API。
        public Uri? AudioUrl { get; private set; }    // 音频文件URL（可空）

        /// <summary>
        /// 封面图片URL
        /// </summary>
        public Uri? CoverImageUrl { get; private set; } // 新增封面字段

        /// <summary>
        /// 字幕文件URL
        /// </summary>
        public Uri? SubtitleUrl { get; private set; } // 新增字幕字段

        /// <summary>
        /// 剧集描述
        /// </summary>
        public string? Description { get; private set; } // 新增描述字段

        public int Duration { get; private set; }     // 持续时间（秒）
        public DateTime CreateTime { get; private set; } // 创建时间
        public bool IsDeleted { get; private set; }   // 是否已删除（软删除）


        // 句子列表 - 聚合内的实体
        private readonly List<Sentence> _sentences = new();

        /// <summary>
        /// public IReadOnlyList<Sentence> Sentences 
        ///{
        ///    get 
        ///    {
        ///        return _sentences.AsReadOnly();
        ///    }
        ///}
        /// </summary>
        public IReadOnlyList<Sentence> Sentences => _sentences.AsReadOnly();
        //     ↑                                    ↑
        //  只读访问，外部不能修改，        防止外部修改内部集合
        //  本函数的AddSentence可以               

        private readonly List<IDomainEvent> _domainEvents = new();
        //                   ↑
        //              存储领域事件的私有集合

        public Episode(string title)
        {
            Id = Guid.NewGuid();    // 生成唯一ID
            Title = title;          // 设置标题
            CreateTime = DateTime.Now; // 记录创建时间
            IsDeleted = false;      // 初始状态为未删除
        }

        private Episode() { Title = string.Empty; }
        //      ↑
        // EF Core需要的无参构造函数（ORM框架要求）

        public void UpdateAudio(Uri audioUrl, int duration)
        {
            AudioUrl = audioUrl;
            Duration = duration;
        }

        /// <summary>
        /// 更新封面图片
        /// </summary>
        public void UpdateCoverImage(Uri? coverImageUrl)
        {
            CoverImageUrl = coverImageUrl;
        }

        /// <summary>
        /// 更新字幕文件
        /// </summary>
        public void UpdateSubtitle(Uri? subtitleUrl)
        {
            SubtitleUrl = subtitleUrl;
        }

        /// <summary>
        /// 更新剧集信息
        /// </summary>
        public void UpdateInfo(string title, string? description = null)
        {
            Title = title;
            Description = description;
        }


        /// <summary>
        /// 恢复已删除的剧集
        /// </summary>
        public void Restore()
        {
            IsDeleted = false;
        }



        /// <summary>
        /// 更新标题
        /// </summary>
        /// <param name="title">新的标题</param>
        public void UpdateTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("标题不能为空", nameof(title));

            Title = title;
        }


        public void AddSentence(string content, TimeSpan startTime, TimeSpan endTime)
        {
            var sentence = new Sentence(content, startTime, endTime);
            _sentences.Add(sentence);
        }

        public void SoftDelete()
        {
            IsDeleted = true;
        }

        //接口实现的基本规则：必须实现所有方法
        // 实现IAggregateRoot接口 - 确保返回类型完全匹配
        public IReadOnlyList<IDomainEvent> GetDomainEvents()
        {
            return _domainEvents.AsReadOnly();// 返回只读副本
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();// 清空事件（通常在保存后）
        }
    }
}