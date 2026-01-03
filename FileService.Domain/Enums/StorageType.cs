using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.Domain.Enums
{
    /// <summary>
    /// 存储类型枚举
    /// </summary>
    public enum StorageType
    {
        /// <summary>
        /// 本地备份存储（内网高速稳定设备，如NAS）
        /// </summary>
        Backup,

        /// <summary>
        /// 公网存储（云存储，供外部访问）
        /// </summary>
        Public
    }
}