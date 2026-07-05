using System.ComponentModel.DataAnnotations;
using AskFm.BLL.DTO.UserDTOs;
using AskFm.BLL.Services;
using AskFm.BLL.Services.UserIdentityService;
using AskFm.DAL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Sprache;

namespace AskFm.API.Controllers;
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private IAuthService  _authService;
    private IUserService _userService;

    public AuthController(IAuthService authService, IUserService userService)
    {
        _authService = authService;
        _userService = userService;
    }

    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> RegisterUser(RegisterUserDTO registerUser)
    {
        if (registerUser == null)
        {
            return BadRequest(new List<String>{"Invalid data"});
        }
        ServiceResult<AuthResponseDTO> result = await _authService.RegisterAsync(registerUser);
        if (!result.success)
        {
            return BadRequest(result.Errors);
        }
        setRefreshToken(result.Data.RefreshToken.Token,result.Data.RefreshToken.ExpireOn);
        return Ok(result);
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login(LoginDTO login)
    {
        if (login == null)
        {
            return BadRequest(new List<String>{"Invalid data"});
        }
        ServiceResult<AuthResponseDTO> result = await _authService.LoginAsync(login);
        if (!result.success)
        {
            return BadRequest(result.Errors);

        }

        setRefreshToken(result.Data.RefreshToken.Token,result.Data.RefreshToken.ExpireOn);

        return Ok(result);
    }

    [HttpPost]
    [Route("refresh-token/{id}")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<IActionResult> RefreshToken(int id)
    {
        string refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized("Invalid Token");
        }
        ServiceResult<AuthResponseDTO> result = await _authService.RefreshTokenAsync(id,refreshToken);

        if (!result.success)
        {
            return BadRequest(result.Errors);
        }
        setRefreshToken(result.Data.RefreshToken.Token,result.Data.RefreshToken.ExpireOn);
        return Ok(result);
    }

    [HttpPost("logout/{id}")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public async Task<IActionResult> Logout(int id)
    {
        var currentUser = await _userService.GetCurrentUserAsync();
        if (currentUser.Data == null || currentUser.Data.Id != id)
        {
            return BadRequest("Invalid data");
        }
        string refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
        {
            return BadRequest("token Is required");
        }

        var result = await _authService.Logout(currentUser.Data.Id,refreshToken);

        if (!result.success)
        {
            return BadRequest(result.Errors);
        }

        return Ok(result);
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
    {

        var result = await _authService.ForgotPasswordAsync(forgotPasswordDto.Email);
        if (!result.success)
        {
            return BadRequest(result.Errors);
        }

        return Ok("Check Your Email");
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
    {
        if (resetPasswordDto == null)
        {
            return BadRequest("Invalid Data");
        }

        var result = await _authService.ResetPasswordAsync(resetPasswordDto);
        if (!result.success)
        {
            return BadRequest(result.Errors);
        }

        return Ok("Password Reset Success");
    }
    [HttpPost]
    [AllowAnonymous]
    [Route("send-email-confirmation")]
    public async Task<IActionResult> SendEmailConfirmation(ForgotPasswordDto request)
    {
        var result = await _authService.SendEmailConfirmationAsync(request.Email);
        if (!result.success)
        {
            return BadRequest(result.Errors);
        }

        return Ok("Check Your Email");
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(ConfirmEmailDto confirmEmailDto)
    {
        if (confirmEmailDto == null)
        {
            return BadRequest("Invalid Data");
        }

        var result = await _authService.ConfirmEmailAsync(confirmEmailDto);
        if (!result.success)
        {
            return BadRequest(result.Errors);
        }

        return Ok("Email Confirmed Successfully");
    }

    private void setRefreshToken(string refreshToken,DateTime expires)
    {
        var cookieOption = new CookieOptions()
        {
            HttpOnly = true,
            Expires = expires.ToLocalTime()
        };
       Response.Cookies.Append("refreshToken", refreshToken, cookieOption);
    }

}
