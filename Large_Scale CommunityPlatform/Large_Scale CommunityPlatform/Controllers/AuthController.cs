using Large_Scale_CommunityPlatform.Data;
using Large_Scale_CommunityPlatform.Models.Dtos.User;
using Large_Scale_CommunityPlatform.Models.Entities;
using Large_Scale_CommunityPlatform.Services;
using Large_Scale_CommunityPlatform.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Large_Scale_CommunityPlatform.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    //import user manager DI
    
    //constructor
    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegisterDto userRegisterDto)
    {
        
        var result = await _authService.RegisterAsync(userRegisterDto);
        
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }
       
        return Ok(new
        {
            message = "User registration succeeded",
            email = userRegisterDto.Email,
        });
    }

    [AllowAnonymous]
    [HttpPost("signin")]
    public async Task<IActionResult> SignIn(UserSigninDto userSigninDto)
    {
        var result = await _authService.SignInAsync(userSigninDto);

        if (result == null)
        {
            return Unauthorized(new
            {
                message = "Invalid email or password"
            });
        }

        SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiresAt);

        return Ok(new
        {
            message = "User successfully signed in",
            accessToken = result.AccessToken
        });
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies["refreshToken"];

        var result = await _authService.RefreshAsync(refreshToken);

        if (result == null)
        {
            return Unauthorized(new
            {
                message = "Invalid refresh token"
            });
        }

        SetRefreshTokenCookie(result.RefreshToken, result.RefreshTokenExpiresAt);

        return Ok(new
        {
            message = "Token refreshed successfully",
            accessToken = result.AccessToken
        });
    }
    
    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies["refreshToken"];

        await _authService.LogoutAsync(refreshToken);

        Response.Cookies.Delete("refreshToken");

        return Ok(new
        {
            message = "Logged out successfully"
        });
    }
    
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
            email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
            name = User.Identity?.Name,
            roles = User.FindAll(System.Security.Claims.ClaimTypes.Role)
                .Select(r => r.Value)
                .ToList()
        });
    }
    
    private void SetRefreshTokenCookie(string refreshToken, DateTime expiresAt)
    {
        Response.Cookies.Append(
            "refreshToken",
            refreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Strict,
                Expires = expiresAt
            });
    }

}