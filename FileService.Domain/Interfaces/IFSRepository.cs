using FileService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileService.Domain.Interfaces
{
    /// <summary>
    /// 文件服务仓储接口
    /// </summary>
    public interface IFSRepository
    {
        /// <summary>
        /// 根据文件大小和哈希值查找文件（用于去重）
        /// </summary>
        /// <param name="fileSize">文件大小</param>
        /// <param name="sha256Hash">SHA256哈希值</param>
        /// <returns>找到的文件项，如果不存在返回null</returns>
        Task<UploadedItem?> FindFileAsync(long fileSize, string sha256Hash);

        /// <summary>
        /// 根据ID获取文件
        /// </summary>
        /// <param name="id">文件ID</param>
        /// <returns>文件项</returns>
        Task<UploadedItem?> GetByIdAsync(Guid id);

        /// <summary>
        /// 获取用户上传的文件列表
        /// </summary>
        /// <param name="uploaderId">上传者ID</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页大小</param>
        /// <returns>文件列表</returns>
        Task<IEnumerable<UploadedItem>> GetByUploaderAsync(Guid uploaderId, int pageIndex, int pageSize);

        /// <summary>
        /// 获取所有文件列表
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">页大小</param>
        /// <returns>文件列表</returns>
        Task<IEnumerable<UploadedItem>> GetAllAsync(int pageIndex, int pageSize);

        /// <summary>
        /// 添加文件
        /// </summary>
        /// <param name="item">文件项</param>
        /// <returns></returns>
        Task AddAsync(UploadedItem item);

        /// <summary>
        /// 更新文件
        /// </summary>
        /// <param name="item">文件项</param>
        void Update(UploadedItem item);

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="item">文件项</param>
        void Delete(UploadedItem item);

        /// <summary>
        /// 手动保存更改（通常由UnitOfWork处理）
        /// </summary>
        /// <returns></returns>
        Task SaveChangesAsync();

        /// <summary>
        /// 分页获取用户文件列表
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="skip">跳过的记录数</param>
        /// <param name="take">获取的记录数</param>
        /// <param name="sortBy">排序字段</param>
        /// <param name="sortDesc">是否降序</param>
        /// <returns>文件列表和总数</returns>
        Task<(List<UploadedItem> files, int totalCount)> GetUserFilesPagedAsync(
            Guid userId,
            int skip,
            int take,
            string sortBy = "UploadTime",
            bool sortDesc = true);


        /// <summary>
        /// 根据ID和上传者ID获取文件（用于权限验证）
        /// </summary>
        Task<UploadedItem?> GetByIdAndUploaderAsync(Guid id, Guid uploaderId);


        /// <summary>
        /// 软删除文件
        /// </summary>
        Task<bool> SoftDeleteAsync(Guid id, Guid uploaderId);
    }
}