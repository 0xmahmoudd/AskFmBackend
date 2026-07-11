using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AskFm.BLL.DTO;
using AskFm.BLL.DTO.UserDTOs;
using AskFm.DAL;
using AskFm.DAL.Interfaces;
using AskFm.DAL.Models;
using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared;
namespace AskFm.BLL.Services.UserIdentityService;

public class AuthService : IAuthService
{
    private IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtOptions _jwtOptions;
    private readonly RedisCacheService _redisCacheService;
    private readonly IConfiguration _configuration;
    private readonly IEmailSender _emailSender;

    public AuthService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IOptions<JwtOptions> jwtOptions, RedisCacheService redisCacheService, IConfiguration configuration, IEmailSender emailSender)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _redisCacheService = redisCacheService;
        _configuration = configuration;
        _emailSender = emailSender;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<ServiceResult<AuthResponseDTO>> LoginAsync(LoginDTO request)
    {
        if (request == null)
        {
            var erros = new List<string>
            {
                "Invalid Email or Password."
            };
            return await ServiceResult<AuthResponseDTO>.Failure(erros);
        }

        var getUser = await _userManager.FindByEmailAsync(request.Email);


        if (getUser == null)
        {
            var erros = new List<string>
            {
                "Invalid Email or Password."
            };
            return await ServiceResult<AuthResponseDTO>.Failure(erros);
        }

        if (getUser.IsDeleted)
        {
            var erros = new List<string>
            {
                "Invalid Email or Password."
            };
            return await ServiceResult<AuthResponseDTO>.Failure(erros);
        }

        var passwordValid = await _userManager.CheckPasswordAsync(getUser, request.Password);
        if (!passwordValid)
        {
            var errors = new List<string> { "Invalid Email or Password." };
            return await ServiceResult<AuthResponseDTO>.Failure(errors);
        }

        var response = await GetAuthToken(getUser);
        return response;

    }

    public async Task<ServiceResult<AuthResponseDTO>> RegisterAsync(RegisterUserDTO request)
    {
        if (request == null)
        {
            var errors = new List<string> { "Invalid Request Data" };
            return await ServiceResult<AuthResponseDTO>.Failure(errors);
        }
        var oldUser = await _userManager.FindByEmailAsync(request.Email);
        if (oldUser != null && oldUser.IsDeleted)
        {
            var errors = new List<string> { "Email already exist" };
            return await ServiceResult<AuthResponseDTO>.Failure(errors);
        }

        var newUser = new ApplicationUser()
        {
            Name = request.Name,
            Email = request.Email,
            UserName = request.Username,
            Bio = request.Bio,
            AvatarPath = request.AvatarPath,
            LastSeen = DateTime.UtcNow
        };
        var createRsult = await _userManager.CreateAsync(newUser, request.Passwrod);


        if (createRsult.Succeeded == false)
        {

            var errors = createRsult.Errors.Select(e => e.Description).ToList();
            return await ServiceResult<AuthResponseDTO>.Failure(errors);


        }

        await SendEmailConfirmationAsync(newUser.Email);

        var response = await GetAuthToken(newUser);
        return response;
    }

    public async Task<ServiceResult<bool>> SendEmailConfirmationAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            var error = new List<string> { "Invalid Email." };
            return await ServiceResult<bool>.Failure(error);
        }

        if (user.EmailConfirmed)
        {
            var error = new List<string> { "Email already confirmed." };
            return await ServiceResult<bool>.Failure(error);
        }

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationUrl =
            $"{_configuration.GetValue<string>("ClientUrl")}/app/Auth/confirm-email?email={email}&token={Uri.EscapeDataString(token)}";

        await _emailSender.SendConfirmationLinkAsync(email, confirmationUrl);

        return await ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<bool>> ConfirmEmailAsync(ConfirmEmailDto confirmEmailDto)
    {
        var user = await _userManager.FindByEmailAsync(confirmEmailDto.Email);
        if (user == null)
        {
            var error = new List<string> { "Invalid Data" };
            return await ServiceResult<bool>.Failure(error);
        }

        if (user.EmailConfirmed)
        {
            return await ServiceResult<bool>.Success(true);
        }

        var result = await _userManager.ConfirmEmailAsync(user, confirmEmailDto.Token);
        if (result.Succeeded)
        {
            return await ServiceResult<bool>.Success(true);
        }

        return await ServiceResult<bool>.Failure(result.Errors.Select(e => e.Description).ToList());
    }

    public async Task<ServiceResult<AuthResponseDTO>> RefreshTokenAsync(int id, string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            var errors = new List<string> { "Invalid Token." };
            return await ServiceResult<AuthResponseDTO>.Failure(errors);
        }

        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
        {
            var errors = new List<string> { "Invalid User." };
            return await ServiceResult<AuthResponseDTO>.Failure(errors);
        }

        var oldRefreshToken = await _redisCacheService.GetCacheAsync<RefreshTokenDto>(AppConstants.UserRefreshTokenCacheKey(user.Id));
        if (oldRefreshToken == null
            || oldRefreshToken.IsExpired
            || oldRefreshToken.Token != refreshToken)
        {
            var errors = new List<string> { "Invalid Token." };
            return await ServiceResult<AuthResponseDTO>.Failure(errors);
        }
        await _redisCacheService.RemoveCacheAsync(AppConstants.UserRefreshTokenCacheKey(user.Id));
        return await GetAuthToken(user);
    }


    public async Task<ServiceResult<bool>> RevokeRefreshTokenAsync(int id, string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            var errors = new List<string> { "Invalid Token." };
            return await ServiceResult<bool>.Failure(errors);
        }

        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
        {
            var errors = new List<string> { "Invalid User." };
            return await ServiceResult<bool>.Failure(errors);
        }

        var userRefreshToken = await _redisCacheService.GetCacheAsync<RefreshTokenDto>(AppConstants.UserRefreshTokenCacheKey(user.Id));
        if (userRefreshToken == null || userRefreshToken.IsExpired)
        {
            var errors = new List<string> { "Invalid Token." };
            return await ServiceResult<bool>.Failure(errors);
        }

        await _redisCacheService.RemoveCacheAsync(AppConstants.UserRefreshTokenCacheKey(user.Id));
        return await ServiceResult<bool>.Success(true);

    }

    public async Task<ServiceResult<bool>> Logout(int userId, string refreshToken)
    {
        var result = await RevokeRefreshTokenAsync(userId, refreshToken);
        if (!result.success)
        {
            return result;
        }
        await RevokeJwtToken(userId);
        return await ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<bool>> ForgotPasswordAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            var error = new List<string> {"Invalid Email."};
            return await ServiceResult<bool>.Failure(error);
        }
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetUrl =
            $"{_configuration.GetValue<string>("ClientUrl")}/app/Auth/reset-password?email={email}&token={token}";

        await _emailSender.SendEmailAsync(email, "AskFm: Reset Password", resetUrl);

        return await ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<bool>> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
    {
        var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
        if (user == null)
        {
            var error = new List<string> { "Invalid Data" };
            return await ServiceResult<bool>.Failure(error);
        }
        var result = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);
        if (result.Succeeded)
        {
            return await ServiceResult<bool>.Success(true);
        }
        return await ServiceResult<bool>.Failure(result.Errors.Select(e => e.Description).ToList());
    }

    private async Task<ServiceResult<AuthResponseDTO>> GetAuthToken(ApplicationUser user)
    {
        // Generate New JWT Token
        string newTokenId = Guid.NewGuid().ToString();
        var token = await GenerateJwtToken(user, newTokenId);
        if (string.IsNullOrEmpty(token))
        {
            var errors = new List<string> { "Invalid Data" };
            return await ServiceResult<AuthResponseDTO>.Failure(errors);
        }
        var oldJwtId = await _redisCacheService.GetCacheAsync<string>(AppConstants.UserJwtCacheKey(user.Id));
        if (!string.IsNullOrEmpty(oldJwtId))
        {
            await _redisCacheService.RemoveCacheAsync(AppConstants.JwtCacheKey(oldJwtId));
            await _redisCacheService.RemoveCacheAsync(AppConstants.UserJwtCacheKey(user.Id));
        }

        await _redisCacheService.SetCacheAsync<int>(AppConstants.JwtCacheKey(newTokenId), user.Id, TimeSpan.FromMinutes(_jwtOptions.AccessExpiration));
        await _redisCacheService.SetCacheAsync<string>(AppConstants.UserJwtCacheKey(user.Id), newTokenId, TimeSpan.FromMinutes(_jwtOptions.AccessExpiration));

        //----------------------------------
        // Use the exist refreshToken or regenerate one

        var refreshToken = await _redisCacheService.GetCacheAsync<RefreshTokenDto>(AppConstants.UserRefreshTokenCacheKey(user.Id));
        if (refreshToken == null)
        {
            refreshToken = await generateRefreshToken();
            await _redisCacheService.SetCacheAsync<RefreshTokenDto>(AppConstants.UserRefreshTokenCacheKey(user.Id),
                refreshToken, TimeSpan.FromDays(refreshToken.ExpireAfter));
        }

        return await ServiceResult<AuthResponseDTO>.Success(new AuthResponseDTO()
        {
            Token = token,
            RefreshToken = refreshToken,
            IsAuthenticated = true,
            User = new ReadUserDTO
            {
                Name = user.Name,
                Email = user.Email,
                LastSeen = user.LastSeen,
                Bio = user.Bio,
                AvatarPath = user.AvatarPath,
                followerCount = user.FollowersCount
            }
        });
    }
    private async Task<RefreshTokenDto> generateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var generator = RandomNumberGenerator.Create();
        generator.GetBytes(randomNumber);

        return new RefreshTokenDto
        {
            Token = Convert.ToBase64String(randomNumber),
            ExpireOn = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("ExpireTimes:Refresh_Token_Exp")),
            CreatedOn = DateTime.UtcNow,
            ExpireAfter = _configuration.GetValue<int>("ExpireTimes:Refresh_Token_Exp")
        };

    }
    private Task<string> GenerateJwtToken(ApplicationUser appUser, string jti)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
            Expires = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessExpiration),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey)), SecurityAlgorithms.HmacSha256),
            Subject = new ClaimsIdentity(new Claim[]
            {
                new(ClaimTypes.Name, appUser.Name),
                new(ClaimTypes.Email, appUser.Email),
                new("UserId", appUser.Id.ToString()),
                new("jti",jti)
            })
        };
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        var accessToken = tokenHandler.WriteToken(securityToken);
        return Task.FromResult(accessToken);
    }

    private async Task RevokeJwtToken(int userId)
    {
        var oldJwtId = await _redisCacheService.GetCacheAsync<string>(AppConstants.UserJwtCacheKey(userId));
        if (string.IsNullOrEmpty(oldJwtId)) return;
        await _redisCacheService.RemoveCacheAsync(AppConstants.JwtCacheKey(oldJwtId));
        await _redisCacheService.RemoveCacheAsync(AppConstants.UserJwtCacheKey(userId));

    }

}
