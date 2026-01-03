namespace Zbw.JWT
{
    /// <summary>
    /// JWT配置选项
    /// 这些配置选项一般在webapi项目的.json文件中定义
    /// </summary>
    public class JWTOptions
    {
        public string SecretKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpireSeconds { get; set; } = 3600; // 默认1小时
    }
}