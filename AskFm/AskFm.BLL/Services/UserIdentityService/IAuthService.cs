using AskFm.BLL.DTO.UserDTOs;
using AskFm.DAL.Models;

namespace AskFm.BLL.Services.UserIdentityService;

public interface IAuthService
{
    public Task<ServiceResult<AuthResponseDTO>> LoginAsync(LoginDTO request);
    public Task<ServiceResult<AuthResponseDTO>> RegisterAsync(RegisterUserDTO request);
    Task<ServiceResult<AuthResponseDTO>> RefreshTokenAsync(int id, string refreshToken);
    public Task<ServiceResult<bool>> RevokeRefreshTokenAsync(int id, string refreshToken);
    public Task<ServiceResult<bool>> Logout(int userId, string refreshToken);
    public Task<ServiceResult<bool>> ForgotPasswordAsync(string email);
    public Task<ServiceResult<bool>> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
    public Task<ServiceResult<bool>> SendEmailConfirmationAsync(string email);
    public Task<ServiceResult<bool>> ConfirmEmailAsync(ConfirmEmailDto confirmEmailDto);
}
