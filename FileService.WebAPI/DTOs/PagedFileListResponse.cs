namespace FileService.WebAPI.DTOs
{
    /// <summary>
    /// 分页文件列表响应
    /// </summary>
    public class PagedFileListResponse
    {
        /// <summary>
        /// 文件列表
        /// </summary>
        public List<FileInfoResponse> Files { get; set; } = new();

        /// <summary>
        /// 总文件数
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 当前页码
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// 每页数量
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// 是否有下一页
        /// </summary>
        public bool HasNextPage { get; set; }

        /// <summary>
        /// 是否有上一页
        /// </summary>
        public bool HasPreviousPage { get; set; }
    }
}