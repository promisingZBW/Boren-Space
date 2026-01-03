using System;
using Zbw.DomainCommons;

namespace Listening.Domain
{
    /// <summary>
    /// 听力字幕句 - 实体
    /// 这个字幕实体实际并没有用到，episode.Sentences 在当前实现中是空的，因为后端没有将 SRT 文件解析为 Sentence 实体
    /// 实际的字幕解析在前端进行，通过 parseSRT() 函数，后端的 Sentence 实体设计用于存储字幕，但当前未被使用
    /// 如果要在后端解析字幕，需要添加 SRT 解析服务，在上传时将字幕内容转换为 Sentence 对象并保存到数据库
    /// 但也可以保留这个字幕实体，以备将来需要在后端处理字幕时使用
    /// 按理来说字幕的解析应该在后端完成，这样后期解析好的字幕可以直接存储在数据库中，不需要再在前端解析，然后解析好的字幕也可以在后端用作其他高级功能
    /// 如全局搜索，数据分析等
    /// </summary>
    public class Sentence : IEntity
    {
        public Guid Id { get; private set; }
        public string Content { get; private set; }
        public TimeSpan StartTime { get; private set; }
        public TimeSpan EndTime { get; private set; }

        public Sentence(string content, TimeSpan startTime, TimeSpan endTime)
        {
            Id = Guid.NewGuid();
            Content = content;
            StartTime = startTime;
            EndTime = endTime;
        }

        // EF Core 需要的无参构造函数
        private Sentence() { Content = string.Empty; }

        /// <summary>
        /// 更新内容
        /// </summary>
        public void UpdateContent(string content)
        {
            Content = content;
        }

        /// <summary>
        /// 更新时间段
        /// </summary>
        public void UpdateTimeSpan(TimeSpan startTime, TimeSpan endTime)
        {
            StartTime = startTime;
            EndTime = endTime;
        }
    }
}