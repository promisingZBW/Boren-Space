using System.ComponentModel.DataAnnotations;

namespace IdentityService.WebAPI.DTOs
{
    /// <summary>
    /// 修改密码请求
    /// </summary>
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "当前密码不能为空")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "新密码不能为空")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "新密码长度必须在6-100字符之间")]
        public string NewPassword { get; set; } = string.Empty;
    }
}