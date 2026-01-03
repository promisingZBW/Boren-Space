using IdentityService.Domain;
using IdentityService.Infrastructure;
using IdentityService.WebAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Zbw.ASPNETCore.DTOs;
using Zbw.JWT;

namespace IdentityService.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly IJWTService _jwtService;
        private readonly ITokenBlacklistService _blacklistService; // ← Token黑名单机制需要添加这个字段

        public AuthController(
            IUserRepository userRepository,
            IPasswordService passwordService,
            IJWTService jwtService,
            ITokenBlacklistService blacklistService) // ← 添加这个参数
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _jwtService = jwtService;
            _blacklistService = blacklistService; // ← 添加这个赋值
        }

        /// <summary>
        /// 用户注册
        /// </summary>
        [HttpPost("register")]//POST 请求 广泛用于创建或提交数据，适合处理复杂或具有保密性的请求
        [Authorize(Roles = "Admin")] // 添加管理员权限控制

        /*当你定义一个方法参数并使用 [FromBody] 特性时，表示这个参数的数据将从请求的主体（body）中获取，通常用于处理 POST、PUT 等方法的请求体。
          在该示例中，RegisterRequest 是一个包含多个字段的复合对象（例如，用户名、邮箱、密码等）。
          使用 [FromBody]，ASP.NET Core 会自动将请求体中的 JSON 数据反序列化为这个 RegisterRequest 对象。
          假设你有以下 JSON 请求体：（用户注册后发到后台的）
          {
          "UserName": "exampleUser",
          "Email": "example@example.com",
          "Password": "securePassword"
          }
          如果你没有使用[FromBody]，框架将无法知道你希望从请求体中提取这些数据。使用后，框架会自动识别，并把这些信息填充到 RegisterRequest 对象中。*/
        public async Task<ApiResponse<UserResponse>> Register([FromBody] RegisterRequest request)
        {
            // 检查用户名是否已存在
            if (await _userRepository.IsUserNameExistAsync(request.UserName))
            {
                return ApiResponse<UserResponse>.ErrorResult("用户名已存在", "USERNAME_EXISTS");
            }

            // 检查邮箱是否已存在
            if (await _userRepository.IsEmailExistAsync(request.Email))
            {
                return ApiResponse<UserResponse>.ErrorResult("邮箱已存在", "EMAIL_EXISTS");
            }

            // 创建用户
            var passwordHash = _passwordService.HashPassword(request.Password);
            var user = new User(request.UserName, request.Email, passwordHash);

            if (!string.IsNullOrEmpty(request.PhoneNumber))
            {
                user.UpdateProfile(request.PhoneNumber);
            }

            // 工作单元会自动保存
            // ? 临时添加手动保存
            await _userRepository.AddAsync(user);

            // 直接分配管理员角色（简化设计）
            // 11111111-1111-1111-1111-111111111111是管理员角色ID，在IdentityDbContext.cs已经设置好了
            //这里是把这个32位的字符串转换成Guid类型，因为RoleId是Guid类型的
            //你可以可以封装这个ID，如下所示：
            //public static readonly Guid AdminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            //然后在这里使用AdminRoleId，但是为了简单起见，这里直接使用Guid.Parse
            var adminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111"); 
            await _userRepository.AssignRoleAsync(user.Id, adminRoleId);

            var response = await ConvertToUserResponse(user);
            return ApiResponse<UserResponse>.SuccessResult(response, "管理员注册成功");
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        [HttpPost("login")]
        public async Task<ApiResponse<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            // 查找用户（支持用户名或邮箱登录）
            User? user = null;
            if (request.UserNameOrEmail.Contains("@"))
            {
                user = await _userRepository.GetByEmailAsync(request.UserNameOrEmail);
            }
            else
            {
                user = await _userRepository.GetByUserNameAsync(request.UserNameOrEmail);
            }

            if (user == null)
            {
                return ApiResponse<LoginResponse>.ErrorResult("用户不存在", "USER_NOT_FOUND");
            }

            if (!user.IsActive)
            {
                return ApiResponse<LoginResponse>.ErrorResult("账户已被禁用", "ACCOUNT_DISABLED");
            }

            // 验证密码
            if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
            {
                return ApiResponse<LoginResponse>.ErrorResult("密码错误", "INVALID_PASSWORD");
            }

            // 记录登录时间
            user.RecordLogin();

            // 获取用户角色
            var roles = await _userRepository.GetUserRolesAsync(user.Id);

            // 生成JWT Token
            var claims = new List<Claim>
            {
                //这里的声明都得是字符串类型的，所以这里要将user.Id的Guid类型转换成字符串类型 
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.UserName),
                new(ClaimTypes.Email, user.Email)
            };

            // 添加角色声明
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));
            }

            var token = _jwtService.GenerateToken(claims);
            var expiresAt = DateTime.UtcNow.AddSeconds(86400); // 24小时

            var userResponse = await ConvertToUserResponse(user);
            var loginResponse = new LoginResponse
            {
                Token = token,
                ExpiresAt = expiresAt,
                User = userResponse
            };

            return ApiResponse<LoginResponse>.SuccessResult(loginResponse, "登录成功");
        }

        /// <summary>
        /// 获取当前用户信息
        /// </summary>
        [HttpGet("me")]
        [Authorize(Roles = "Admin")] // 添加管理员权限控制
        public async Task<ApiResponse<UserResponse>> GetCurrentUser()
        {
            //这行代码从已验证的用户中查找用户标识符的声明（Claim）。此声明通常在用户登录时生成，并用于标识用户。
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            //Guid.TryParse(userIdClaim.Value, out var userId)这里要把UserId的字符串类型（126行）再换成Guid类型
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return ApiResponse<UserResponse>.ErrorResult("无效的用户身份", "INVALID_USER_IDENTITY");
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<UserResponse>.ErrorResult("用户不存在", "USER_NOT_FOUND");
            }

            var response = await ConvertToUserResponse(user);
            return ApiResponse<UserResponse>.SuccessResult(response);
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        [HttpPost("change-password")]
        [Authorize(Roles = "Admin")] // 添加管理员权限控制
        public async Task<ApiResponse> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return ApiResponse.ErrorResult("无效的用户身份", "INVALID_USER_IDENTITY");
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse.ErrorResult("用户不存在", "USER_NOT_FOUND");
            }

            // 验证当前密码
            if (!_passwordService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            {
                return ApiResponse.ErrorResult("当前密码错误", "INVALID_CURRENT_PASSWORD");
            }

            // 更新密码
            var newPasswordHash = _passwordService.HashPassword(request.NewPassword);
            user.UpdatePassword(newPasswordHash);

            return ApiResponse.SuccessResult("密码修改成功");
        }

        /*
        /// <summary>
        /// 注销登录简单版本，不实现黑名单机制
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public ApiResponse Logout()
        {
            // 在实际应用中，可以在这里实现token黑名单机制
            // 目前JWT是无状态的，即服务器其实没保存任何信息，客户端删除token即可
            // 如果要实现黑名单机制，则需要将Token储存在服务器中，并在服务器中禁用
            return ApiResponse.SuccessResult("注销成功");
        }
        */

        /// <summary>
        /// 注销登录
        /// </summary>
        [HttpPost("logout")]
        //这个特性表示只有经过 authenticated 用户才能调用这个方法，确保未登录用户无法注销。
        [Authorize(Roles = "Admin")] // 添加管理员权限控制
        public async Task<ApiResponse> Logout()
        {
            var token = ExtractTokenFromRequest();
            if (!string.IsNullOrEmpty(token))
            {
                //返回一个元组 (tokenId, expiry)。其中 tokenId 是 JWT 中唯一标识符，expiry 是令牌的过期时间
                var (tokenId, expiry) = ExtractTokenInfo(token);
                if (!string.IsNullOrEmpty(tokenId))
                {
                    // 将Token加入黑名单
                    await _blacklistService.AddToBlacklistAsync(tokenId, expiry);
                }
            }

            return ApiResponse.SuccessResult("注销成功");
        }

        /// <summary>
        /// 禁用用户（管理员功能）
        /// </summary>
        [HttpPost("disable-user/{userId}")]
        [Authorize(Roles = "Admin")] // 添加管理员权限控制
        public async Task<ApiResponse> DisableUser(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse.ErrorResult("用户不存在", "USER_NOT_FOUND");
            }

            // 禁用用户
            user.Deactivate();

            // 将用户所有Token加入黑名单
            await _blacklistService.BlacklistUserTokensAsync(userId);

            return ApiResponse.SuccessResult("用户已被禁用");
        }

        //从 HTTP 请求的头部中提取 Bearer 令牌
        private string? ExtractTokenFromRequest()
        {
            //直接从当前类的 Request 属性提取授权头，而不是从 HttpContext 中获取。这样可以确保在控制器中直接访问请求头信息。
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (authHeader?.StartsWith("Bearer ") == true)
            {
                //使用 Substring 方法去掉 "Bearer " 前缀，并返回剩余部分，通常是实际的令牌。Trim() 用于去除前后的空格。
                return authHeader.Substring("Bearer ".Length).Trim();
            }
            return null;
        }

        private (string? tokenId, DateTime expiry) ExtractTokenInfo(string token)
        {
            try
            {
                var jwtHandler = new JwtSecurityTokenHandler();
                var jsonToken = jwtHandler.ReadJwtToken(token);

                var tokenId = jsonToken.Claims.FirstOrDefault(c => c.Type == "jti")?.Value;
                var expiry = jsonToken.ValidTo;

                return (tokenId, expiry);
            }
            catch
            {
                return (null, DateTime.UtcNow);
            }
        }


        private async Task<UserResponse> ConvertToUserResponse(User user)
        {
            var roles = await _userRepository.GetUserRolesAsync(user.Id);

            return new UserResponse
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                CreateTime = user.CreateTime,
                LastLoginTime = user.LastLoginTime,
                IsActive = user.IsActive,
                Roles = roles.Select(r => r.Name).ToList()
            };
        }
    }
}
