using FileService.Domain.Enums;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FileService.Domain.Interfaces
{
    /// <summary>
    /// 存储客户端接口
    /// </summary>
    public interface IStorageClient
    {
        /// <summary>
        /// 存储类型
        /// </summary>
        StorageType StorageType { get; }

        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="key">文件的key（一般是文件路径的一部分）</param>
        /// <param name="content">文件内容</param>
        /// <param name="contentType">文件MIME类型</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>存储返回的可以被访问的文件Url</returns>
        Task<Uri> SaveAsync(string key, Stream content, string contentType, CancellationToken cancellationToken = default);

        /// <summary>
        /// 检查文件是否存在
        /// </summary>
        /// <param name="key">文件键</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>是否存在</returns>
        Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取文件流
        /// </summary>
        /// <param name="key">文件键</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>文件流</returns>
        Task<Stream> GetAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="key">文件键</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>是否删除成功</returns>
        Task<bool> DeleteAsync(string key, CancellationToken cancellationToken = default);
    }
}