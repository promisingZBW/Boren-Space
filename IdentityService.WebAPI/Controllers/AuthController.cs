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
        private readonly ITokenBlacklistService _blacklistService; // �� Token������������Ҫ��������ֶ�

        public AuthController(
            IUserRepository userRepository,
            IPasswordService passwordService,
            IJWTService jwtService,
            ITokenBlacklistService blacklistService) // �� �����������
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _jwtService = jwtService;
            _blacklistService = blacklistService; // �� ���������ֵ
        }

        /// <summary>
        /// �û�ע��
        /// </summary>
        [HttpPost("register")]//POST ���� �㷺���ڴ������ύ���ݣ��ʺϴ������ӻ���б����Ե�����
        [Authorize(Roles = "Admin")] // ���ӹ���ԱȨ�޿���

        /*���㶨��һ������������ʹ�� [FromBody] ����ʱ����ʾ������������ݽ�����������壨body���л�ȡ��ͨ�����ڴ��� POST��PUT �ȷ����������塣
          �ڸ�ʾ���У�RegisterRequest ��һ����������ֶεĸ��϶������磬�û��������䡢����ȣ���
          ʹ�� [FromBody]��ASP.NET Core ���Զ����������е� JSON ���ݷ����л�Ϊ��� RegisterRequest ����
          ������������ JSON �����壺���û�ע��󷢵���̨�ģ�
          {
          "UserName": "exampleUser",
          "Email": "example@example.com",
          "Password": "securePassword"
          }
          �����û��ʹ��[FromBody]����ܽ��޷�֪����ϣ��������������ȡ��Щ���ݡ�ʹ�ú󣬿�ܻ��Զ�ʶ�𣬲�����Щ��Ϣ��䵽 RegisterRequest �����С�*/
        public async Task<ApiResponse<UserResponse>> Register([FromBody] RegisterRequest request)
        {
            // ����û����Ƿ��Ѵ���
            if (await _userRepository.IsUserNameExistAsync(request.UserName))
            {
                return ApiResponse<UserResponse>.ErrorResult("�û����Ѵ���", "USERNAME_EXISTS");
            }

            // ��������Ƿ��Ѵ���
            if (await _userRepository.IsEmailExistAsync(request.Email))
            {
                return ApiResponse<UserResponse>.ErrorResult("�����Ѵ���", "EMAIL_EXISTS");
            }

            // �����û�
            var passwordHash = _passwordService.HashPassword(request.Password);
            var user = new User(request.UserName, request.Email, passwordHash);

            if (!string.IsNullOrEmpty(request.PhoneNumber))
            {
                user.UpdateProfile(request.PhoneNumber);
            }

            // ������Ԫ���Զ�����
            // ? ��ʱ�����ֶ�����
            await _userRepository.AddAsync(user);

            // ֱ�ӷ������Ա��ɫ������ƣ�
            // 11111111-1111-1111-1111-111111111111�ǹ���Ա��ɫID����IdentityDbContext.cs�Ѿ����ú���
            //�����ǰ����32λ���ַ���ת����Guid���ͣ���ΪRoleId��Guid���͵�
            //����Կ��Է�װ���ID��������ʾ��
            //public static readonly Guid AdminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            //Ȼ��������ʹ��AdminRoleId������Ϊ�˼����������ֱ��ʹ��Guid.Parse
            var adminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111"); 
            await _userRepository.AssignRoleAsync(user.Id, adminRoleId);

            var response = await ConvertToUserResponse(user);
            return ApiResponse<UserResponse>.SuccessResult(response, "����Աע��ɹ�");
        }

        /// <summary>
        /// �û���¼
        /// </summary>
        [HttpPost("login")]
        public async Task<ApiResponse<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            // �����û���֧���û����������¼��
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
                return ApiResponse<LoginResponse>.ErrorResult("�û�������", "USER_NOT_FOUND");
            }

            if (!user.IsActive)
            {
                return ApiResponse<LoginResponse>.ErrorResult("�˻��ѱ�����", "ACCOUNT_DISABLED");
            }

            // ��֤����
            if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
            {
                return ApiResponse<LoginResponse>.ErrorResult("�������", "INVALID_PASSWORD");
            }

            // ��¼��¼ʱ��
            user.RecordLogin();

            // ��ȡ�û���ɫ
            var roles = await _userRepository.GetUserRolesAsync(user.Id);

            // ����JWT Token
            var claims = new List<Claim>
            {
                //����������������ַ������͵ģ���������Ҫ��user.Id��Guid����ת�����ַ������� 
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.UserName),
                new(ClaimTypes.Email, user.Email)
            };

            // ���ӽ�ɫ����
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));
            }

            var token = _jwtService.GenerateToken(claims);
            var expiresAt = DateTime.UtcNow.AddSeconds(86400); // 24Сʱ

            var userResponse = await ConvertToUserResponse(user);
            var loginResponse = new LoginResponse
            {
                Token = token,
                ExpiresAt = expiresAt,
                User = userResponse
            };

            return ApiResponse<LoginResponse>.SuccessResult(loginResponse, "��¼�ɹ�");
        }

        /// <summary>
        /// ��ȡ��ǰ�û���Ϣ
        /// </summary>
        [HttpGet("me")]
        [Authorize(Roles = "Admin")] // ���ӹ���ԱȨ�޿���
        public async Task<ApiResponse<UserResponse>> GetCurrentUser()
        {
            //���д��������֤���û��в����û���ʶ����������Claim����������ͨ�����û���¼ʱ���ɣ������ڱ�ʶ�û���
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            //Guid.TryParse(userIdClaim.Value, out var userId)����Ҫ��UserId���ַ������ͣ�126�У��ٻ���Guid����
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return ApiResponse<UserResponse>.ErrorResult("��Ч���û�����", "INVALID_USER_IDENTITY");
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<UserResponse>.ErrorResult("�û�������", "USER_NOT_FOUND");
            }

            var response = await ConvertToUserResponse(user);
            return ApiResponse<UserResponse>.SuccessResult(response);
        }

        /// <summary>
        /// �޸�����
        /// </summary>
        [HttpPost("change-password")]
        [Authorize(Roles = "Admin")] // ���ӹ���ԱȨ�޿���
        public async Task<ApiResponse> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return ApiResponse.ErrorResult("��Ч���û�����", "INVALID_USER_IDENTITY");
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse.ErrorResult("�û�������", "USER_NOT_FOUND");
            }

            // ��֤��ǰ����
            if (!_passwordService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            {
                return ApiResponse.ErrorResult("��ǰ�������", "INVALID_CURRENT_PASSWORD");
            }

            // ��������
            var newPasswordHash = _passwordService.HashPassword(request.NewPassword);
            user.UpdatePassword(newPasswordHash);

            return ApiResponse.SuccessResult("�����޸ĳɹ�");
        }

        /*
        /// <summary>
        /// ע����¼�򵥰汾����ʵ�ֺ���������
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public ApiResponse Logout()
        {
            // ��ʵ��Ӧ���У�����������ʵ��token����������
            // ĿǰJWT����״̬�ģ�����������ʵû�����κ���Ϣ���ͻ���ɾ��token����
            // ���Ҫʵ�ֺ��������ƣ�����Ҫ��Token�����ڷ������У����ڷ������н���
            return ApiResponse.SuccessResult("ע���ɹ�");
        }
        */

        /// <summary>
        /// ע����¼
        /// </summary>
        [HttpPost("logout")]
        //������Ա�ʾֻ�о��� authenticated �û����ܵ������������ȷ��δ��¼�û��޷�ע����
        [Authorize(Roles = "Admin")] // ���ӹ���ԱȨ�޿���
        public async Task<ApiResponse> Logout()
        {
            var token = ExtractTokenFromRequest();
            if (!string.IsNullOrEmpty(token))
            {
                //����һ��Ԫ�� (tokenId, expiry)������ tokenId �� JWT ��Ψһ��ʶ����expiry �����ƵĹ���ʱ��
                var (tokenId, expiry) = ExtractTokenInfo(token);
                if (!string.IsNullOrEmpty(tokenId))
                {
                    // ��Token���������
                    await _blacklistService.AddToBlacklistAsync(tokenId, expiry);
                }
            }

            return ApiResponse.SuccessResult("ע���ɹ�");
        }

        /// <summary>
        /// �����û�������Ա���ܣ�
        /// </summary>
        [HttpPost("disable-user/{userId}")]
        [Authorize(Roles = "Admin")] // ���ӹ���ԱȨ�޿���
        public async Task<ApiResponse> DisableUser(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse.ErrorResult("�û�������", "USER_NOT_FOUND");
            }

            // �����û�
            user.Deactivate();

            // ���û�����Token���������
            await _blacklistService.BlacklistUserTokensAsync(userId);

            return ApiResponse.SuccessResult("�û��ѱ�����");
        }

        //�� HTTP �����ͷ������ȡ Bearer ����
        private string? ExtractTokenFromRequest()
        {
            //ֱ�Ӵӵ�ǰ��� Request ������ȡ��Ȩͷ�������Ǵ� HttpContext �л�ȡ����������ȷ���ڿ�������ֱ�ӷ�������ͷ��Ϣ��
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (authHeader?.StartsWith("Bearer ") == true)
            {
                //ʹ�� Substring ����ȥ�� "Bearer " ǰ׺��������ʣ�ಿ�֣�ͨ����ʵ�ʵ����ơ�Trim() ����ȥ��ǰ��Ŀո�
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
