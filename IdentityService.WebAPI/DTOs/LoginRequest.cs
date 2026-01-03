using System.ComponentModel.DataAnnotations;

namespace IdentityService.WebAPI.DTOs
{
    /// <summary>
    /// 用户登录请求
    /// </summary>
    public class LoginRequest
    {
        [Required(ErrorMessage = "用户名或邮箱不能为空")]
        public string UserNameOrEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "密码不能为空")]
        public string Password { get; set; } = string.Empty;
    }
}