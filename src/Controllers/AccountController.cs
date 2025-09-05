using GateEntryExit.Domain;
using GateEntryExit.Dtos.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Scryber.OpenType;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using GateEntryExit.Service.Token;
using Azure;
using System.Text.Encodings.Web;
using Scryber.Components;

namespace GateEntryExit.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly UrlEncoder _urlEncoder;

        private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

        public AccountController(UserManager<AppUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration,
        UrlEncoder urlEncoder,
        ITokenService tokenService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _urlEncoder = urlEncoder;
            _configuration = configuration;
            _tokenService = tokenService;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<AuthResponseDto> Register(RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Model state is not valid!"
                };
            }

            var user = new AppUser
            {
                Email = registerDto.Email,
                FullName = registerDto.FullName,
                UserName = registerDto.Email
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                return new AuthResponseDto
                {
                    IsSuccess = true,
                    Message = "Unable to register!" // result.Errors
                };
            }

            await _userManager.AddToRoleAsync(user, "User");            

            return new AuthResponseDto
            {
                IsSuccess = true,
                Message = "Account Created Sucessfully!"
            };
        }

        [AllowAnonymous]
        [HttpPost("update-profile")]
        public async Task<AuthResponseDto> UpdateProfile(UpdateProfileDto updateProfileDto)
        {
            if (!ModelState.IsValid)
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Model state is not valid!"
                };
            }

            var user = await _userManager.FindByEmailAsync(updateProfileDto.Email);
            user.Email = updateProfileDto.Email;
            user.FullName = updateProfileDto.FullName;

            var result = await _userManager.UpdateAsync(user);

            return new AuthResponseDto
            {
                IsSuccess = true,
                Message = "Profile updated Sucessfully!"
            };
        }

        [AllowAnonymous]
        [HttpPost("login")]

        public async Task<AuthResponseDto> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Model state is not valid!"
                };
            }

            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user is null)
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "User not found with this email",
                };
            }

            var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);

            if (!result)
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Invalid Password."
                };
            }

            var isTfaEnabled = await _userManager.GetTwoFactorEnabledAsync(user);

            var token = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();
            _ = int.TryParse(_configuration.GetSection("JWTSetting").GetSection("RefreshTokenValidityIn").Value!, out int RefreshTokenValidityIn);
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(RefreshTokenValidityIn);
            await _userManager.UpdateAsync(user);

            return new AuthResponseDto
            {
                Token = token,
                IsSuccess = true,
                Message = "Login Success.",
                RefreshToken = refreshToken,
                IsTfaEnabled = isTfaEnabled,
                IsTfaSuccess = false
            };
        }

        [HttpPost("login-tfa")]
        public async Task<AuthResponseDto> LoginTfa([FromBody] TfaDto tfaDto)
        {
            var user = await _userManager.FindByEmailAsync(tfaDto.Email);

            if (user == null)
                return new AuthResponseDto { Message = "Invalid Authentication" };

            var validVerification =
              await _userManager.VerifyTwoFactorTokenAsync(
                 user, _userManager.Options.Tokens.AuthenticatorTokenProvider, tfaDto.Code);
            if (!validVerification)
                return new AuthResponseDto { Message = "Invalid Token Verification" };

            if (user.RefreshToken != tfaDto.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Invalid client request"
                };

            return new AuthResponseDto { IsSuccess = true, IsTfaEnabled = true };
        }

        [AllowAnonymous]
        [HttpPost("refresh-token")]

        public async Task<AuthResponseDto> RefreshToken(TokenDto tokenDto)
        {
            if (!ModelState.IsValid)
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Model state is not valid!"
                };
            }

            var principal = _tokenService.GetPrincipalFromExpiredToken(tokenDto.Token);
            var user = await _userManager.FindByEmailAsync(tokenDto.Email);

            if (principal is null || user is null || user.RefreshToken != tokenDto.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "Invalid client request"
                };

            var newJwtToken = _tokenService.GenerateAccessToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();
            _ = int.TryParse(_configuration.GetSection("JWTSetting").GetSection("RefreshTokenValidityIn").Value!, out int RefreshTokenValidityIn);

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(RefreshTokenValidityIn);

            await _userManager.UpdateAsync(user);

            return new AuthResponseDto
            {
                IsSuccess = true,
                Token = newJwtToken,
                RefreshToken = newRefreshToken,
                Message = "Refreshed token successfully"
            };
        }

        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<AuthResponseDto> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);

            if (user is null)
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "User does not exist with this email"
                };
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = $"http://localhost:4200/reset-password?email={user.Email}&token={WebUtility.UrlEncode(token)}";

            if (false)
            {
                return new AuthResponseDto
                {
                    IsSuccess = true,
                    Message = "Email sent with password reset link. Please check your email."
                };
            }
            else
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "response.Content!.ToString()"
                };
            }
        }


        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<AuthResponseDto> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
            // resetPasswordDto.Token = WebUtility.UrlDecode(resetPasswordDto.Token);

            if (user is null)
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "User does not exist with this email"
                };
            }

            var result = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);

            if (result.Succeeded)
            {
                return new AuthResponseDto
                {
                    IsSuccess = true,
                    Message = "Password reset Successfully"
                };
            }

            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = result.Errors.FirstOrDefault()!.Description
            };
        }


        [HttpPost("change-password")]
        public async Task<AuthResponseDto> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            var user = await _userManager.FindByEmailAsync(changePasswordDto.Email);
            if (user is null)
            {
                return new AuthResponseDto
                {
                    IsSuccess = false,
                    Message = "User does not exist with this email"
                };
            }

            var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);

            if (result.Succeeded)
            {
                return new AuthResponseDto
                {
                    IsSuccess = true,
                    Message = "Password changed successfully"
                };
            }

            return new AuthResponseDto
            {
                IsSuccess = false,
                Message = result.Errors.FirstOrDefault()!.Description
            };
        }

        [HttpGet("user-detail")]
        public async Task<UserDetailDto> GetUserDetail()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(currentUserId!);

            if (user is null)
            {
                throw new Exception("User not found");
            }

            return new UserDetailDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Roles = [.. await _userManager.GetRolesAsync(user)],
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                AccessFailedCount = user.AccessFailedCount,
            };
        }

        [HttpGet]
        public async Task<IEnumerable<UserDetailDto>> GetUsers()
        {
            return await _userManager.Users.Select(u => new UserDetailDto
            {
                Id = u.Id,
                Email = u.Email,
                FullName = u.FullName,
                Roles = _userManager.GetRolesAsync(u).Result.ToArray()
            }).ToListAsync();
        }

        [HttpGet("tfa-setup")]
        public async Task<TfaSetupDto> GetTfaSetup(string email)
        {
            var user = await _userManager.FindByNameAsync(email);

            if (user == null)
                return new TfaSetupDto { Error = "User does not exist" };

            var isTfaEnabled = await _userManager.GetTwoFactorEnabledAsync(user);

            var authenticatorKey = await _userManager.GetAuthenticatorKeyAsync(user);
            if (authenticatorKey == null)
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                authenticatorKey = await _userManager.GetAuthenticatorKeyAsync(user);
            }
            var formattedKey = GenerateQrCode(email, authenticatorKey);

            return new TfaSetupDto
            { IsTfaEnabled = isTfaEnabled, AuthenticatorKey = authenticatorKey, FormattedKey = formattedKey };
        }

        private string GenerateQrCode(string email, string unformattedKey)
        {
            return string.Format(
            AuthenticatorUriFormat,
                _urlEncoder.Encode("GateEntryExit"),
                _urlEncoder.Encode(email),
                unformattedKey);
        }

        [HttpPost("tfa-setup")]
        public async Task<TfaSetupDto> PostTfaSetup([FromBody] TfaSetupDto tfaModel)
        {
            var user = await _userManager.FindByNameAsync(tfaModel.Email);

            var isValidCode = await _userManager
                .VerifyTwoFactorTokenAsync(user,
                  _userManager.Options.Tokens.AuthenticatorTokenProvider,
                  tfaModel.Code);

            if (isValidCode)
            {
                await _userManager.SetTwoFactorEnabledAsync(user, true);
                return new TfaSetupDto { IsTfaEnabled = true };
            }
            else
            {
                return new TfaSetupDto { Error = "Invalid code" };
            }
        }

        [HttpDelete("tfa-setup")]
        public async Task<TfaSetupDto> DeleteTfaSetup(string email)
        {
            var user = await _userManager.FindByNameAsync(email);

            if (user == null)
            {
                return new TfaSetupDto { Error = "User not exist" };
            }
            else
            {
                await _userManager.SetTwoFactorEnabledAsync(user, false);
                return new TfaSetupDto { IsTfaEnabled = false };
            }
        }

    }
}
