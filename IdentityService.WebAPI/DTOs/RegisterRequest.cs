using System.ComponentModel.DataAnnotations;

namespace IdentityService.WebAPI.DTOs
{
    /// <summary>
    /// 用户注册请求
    /// </summary>
    public class RegisterRequest
    {
        [Required(ErrorMessage = "用户名不能为空")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "用户名长度必须在3-50字符之间")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "邮箱不能为空")]
        [EmailAddress(ErrorMessage = "邮箱格式不正确")]
        [StringLength(100, ErrorMessage = "邮箱长度不能超过100字符")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "密码不能为空")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "密码长度必须在6-100字符之间")]
        public string Password { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "手机号码长度不能超过20字符")]
        public string? PhoneNumber { get; set; }
    }
}