using Listening.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Listening.Infrastructure
{
    /// <summary>
    /// 听力服务仓储接口
    /// </summary>
    public interface IListeningRepository
    {
        /// <summary>
        /// 根据ID获取剧集
        /// </summary>
        Task<Episode?> GetByIdAsync(Guid id);

        /// <summary>
        /// 获取所有剧集（分页）
        /// </summary>
        Task<IEnumerable<Episode>> GetAllAsync(int pageIndex = 0, int pageSize = 20);

        /// <summary>
        /// 搜索剧集
        /// </summary>
        Task<IEnumerable<Episode>> SearchAsync(string keyword, int pageIndex = 0, int pageSize = 20);

        /// <summary>
        /// 添加剧集
        /// </summary>
        Task AddAsync(Episode episode);

        /// <summary>
        /// 更新剧集
        /// </summary>
        void Update(Episode episode);

        /// <summary>
        /// 删除剧集
        /// </summary>
        void Remove(Episode episode);

        /// <summary>
        /// 保存更改
        /// </summary>
        Task<int> SaveChangesAsync();

        
        Task<IEnumerable<Episode>> GetAllIncludingDeletedAsync(int pageIndex, int pageSize);

        /// <summary>
        /// 恢复已删除的剧集
        /// <summary>
        Task<Episode?> GetByIdIncludingDeletedAsync(Guid id);
    }
}