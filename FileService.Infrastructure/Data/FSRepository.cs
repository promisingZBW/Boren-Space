using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FileService.Domain.Entities;
using FileService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileService.Infrastructure.Data
{
    /// <summary>
    /// 文件服务仓储实现
    /// </summary>
    public class FSRepository : IFSRepository
    {
        private readonly FSDbContext _context;

        public FSRepository(FSDbContext context)
        {
            _context = context;
        }

        public async Task<UploadedItem?> FindFileAsync(long fileSize, string sha256Hash)
        {
            return await _context.UploadedItems
                .Where(x => x.FileSizeInBytes == fileSize && x.FileSHA256Hash == sha256Hash)
                .FirstOrDefaultAsync();
        }

        public async Task<UploadedItem?> GetByIdAsync(Guid id)
        {
            return await _context.UploadedItems
                .Where(x => x.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<UploadedItem>> GetByUploaderAsync(Guid uploaderId, int pageIndex, int pageSize)
        {
            return await _context.UploadedItems
                .Where(x => x.UploaderId == uploaderId)
                .OrderByDescending(x => x.UploadTime)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<UploadedItem>> GetAllAsync(int pageIndex, int pageSize)
        {
            return await _context.UploadedItems
                .OrderByDescending(x => x.UploadTime)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task AddAsync(UploadedItem item)
        {
            await _context.UploadedItems.AddAsync(item);
        }

        public void Update(UploadedItem item)
        {
            _context.UploadedItems.Update(item);
        }

        public void Delete(UploadedItem item)
        {
            _context.UploadedItems.Remove(item);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }


        public async Task<(List<UploadedItem> files, int totalCount)> GetUserFilesPagedAsync(
            Guid userId,
            int skip,
            int take,
            string sortBy = "UploadTime",
            bool sortDesc = true)
        {
            var query = _context.UploadedItems
                .Where(f => f.UploaderId == userId && !f.IsDeleted);

            // 应用排序
            query = sortBy.ToLower() switch
            {
                "filename" => sortDesc
                    ? query.OrderByDescending(f => f.FileName)
                    : query.OrderBy(f => f.FileName),
                "filesize" => sortDesc
                    ? query.OrderByDescending(f => f.FileSizeInBytes)
                    : query.OrderBy(f => f.FileSizeInBytes),
                "uploadtime" or _ => sortDesc
                    ? query.OrderByDescending(f => f.UploadTime)
                    : query.OrderBy(f => f.UploadTime)
            };

            // 获取总数
            var totalCount = await query.CountAsync();

            // 获取分页数据
            var files = await query
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return (files, totalCount);
        }



        /// <summary>
        /// 根据ID和上传者ID获取文件（用于权限验证，三个条件必须同时满足）
        /// </summary>
        public async Task<UploadedItem?> GetByIdAndUploaderAsync(Guid id, Guid uploaderId)
        {
            return await _context.UploadedItems
                .Where(x => x.Id == id && x.UploaderId == uploaderId && !x.IsDeleted)
                .FirstOrDefaultAsync();//返回第一个匹配的记录
        }

        /// <summary>
        /// 软删除文件
        /// </summary>
        public async Task<bool> SoftDeleteAsync(Guid id, Guid uploaderId)
        {
            var file = await GetByIdAndUploaderAsync(id, uploaderId);
            if (file == null)
            {
                return false; // 文件不存在或无权限
            }

            file.MarkAsDeleted();
            return true; // UnitOfWork会自动保存
        }
    }
}