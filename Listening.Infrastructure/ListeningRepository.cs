using Microsoft.EntityFrameworkCore;
using Listening.Domain;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Listening.Infrastructure
{
    /// <summary>
    /// 听力服务仓储实现
    /// </summary>
    public class ListeningRepository : IListeningRepository
    {
        private readonly ListeningDbContext _context;

        public ListeningRepository(ListeningDbContext context)
        {
            _context = context;
        }

        public async Task<Episode?> GetByIdAsync(Guid id)
        {
            return await _context.Episodes
                .Include(e => e.Sentences)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<Episode>> GetAllAsync(int pageIndex = 0, int pageSize = 20)
        {
            return await _context.Episodes
                .Include(e => e.Sentences)
                .OrderByDescending(e => e.CreateTime)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Episode>> SearchAsync(string keyword, int pageIndex = 0, int pageSize = 20)
        {
            var query = _context.Episodes.Include(e => e.Sentences).AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(e => e.Title.Contains(keyword));
            }

            return await query
                .OrderByDescending(e => e.CreateTime)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task AddAsync(Episode episode)
        {
            await _context.Episodes.AddAsync(episode);
        }

        public void Update(Episode episode)
        {
            _context.Episodes.Update(episode);
        }

        public void Remove(Episode episode)
        {
            _context.Episodes.Remove(episode);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }


        public async Task<IEnumerable<Episode>> GetAllIncludingDeletedAsync(int pageIndex, int pageSize)
        {
            return await _context.Episodes
                .IgnoreQueryFilters() //  忽略软删除过滤器
                .OrderBy(e => e.CreateTime)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Episode?> GetByIdIncludingDeletedAsync(Guid id)
        {
            return await _context.Episodes
                .IgnoreQueryFilters() //  忽略软删除过滤器
                .FirstOrDefaultAsync(e => e.Id == id);
        }
    }
}
