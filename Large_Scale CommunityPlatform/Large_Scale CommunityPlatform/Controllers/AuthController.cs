using Large_Scale_CommunityPlatform.Data;
using Large_Scale_CommunityPlatform.Models.Dtos.User;
using Large_Scale_CommunityPlatform.Models.Entities;
using Large_Scale_CommunityPlatform.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Large_Scale_CommunityPlatform.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    //import user manager DI
    private readonly UserManager<User> _userManager;
    private readonly JwtTokenProvider _jwtTokenProvider;
    private readonly JwtRefreshTokenProvider _refreshTokenProvider;
    private readonly ApplicationDbContext _context;
    
    //constructor
    public AuthController(UserManager<User> userManager, JwtTokenProvider jwtTokenProvider, JwtRefreshTokenProvider refreshTokenProvider,  ApplicationDbContext context)
    {
        _userManager = userManager;
        _jwtTokenProvider = jwtTokenProvider;
        _refreshTokenProvider = refreshTokenProvider;
        _context = context;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegisterDto userRegisterDto)
    {
        //Define Model(DTO)
        var user = new User
        {
            UserName = userRegisterDto.Email,
            Email = userRegisterDto.Email,
            FullName =  userRegisterDto.FullName,
            DoB = userRegisterDto.DoB,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        var result = await _userManager.CreateAsync(user, userRegisterDto.Password);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }
        
        var roleResult = await _userManager.AddToRoleAsync(user, "User");

        if (!roleResult.Succeeded)
        {
            return BadRequest(roleResult.Errors);
        }

        return Ok(new
        {
            message = "User registration succeeded",
            email = user.Email,
        });
    }

    [AllowAnonymous]
    [HttpPost("signin")]
    public async Task<IActionResult> SignIn(UserSigninDto userSigninDto)
    {
        var user = await _userManager.FindByEmailAsync(userSigninDto.Email);

        if (user == null)
        {
            return Unauthorized(
                new
                {
                    message = "User not found",
                });
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, userSigninDto.Password);

        if (!isPasswordValid)
        {
            return Unauthorized(new
            {
                message = "Invalid password",
            });
        }
        
        var roles = await _userManager.GetRolesAsync(user);
        
        //Access Token
        var accessToken = _jwtTokenProvider.CreateAccessToken(user, roles);
        
        //Refresh Token
        var refreshToken = _refreshTokenProvider.GenerateRefreshToken();
        var refreshTokenHashed = _refreshTokenProvider.HashRefreshToken(refreshToken);
        
        //Create new instance for Refresh Token
        var refreshTokenEntity = new RefreshToken
        {
            TokenHash = refreshTokenHashed,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        
        _context.RefreshTokens.Add(refreshTokenEntity);
        //Save
        await _context.SaveChangesAsync();
        
        Response.Cookies.Append(
            "refreshToken",
            refreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = false, 
                SameSite = SameSiteMode.Strict,
                Expires = refreshTokenEntity.ExpiresAt
            });
        
        return Ok(new
        {
            message = "User successfully signed in",
            accessToken
        });
    }
    
    //Temporary
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

    [AllowAnonymous]
    [HttpPost("Refresh")]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies["refreshToken"];

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Unauthorized(new
            {
                message = "Refresh token is missing"
            });
        }

        var refreshTokenHash = _refreshTokenProvider.HashRefreshToken(refreshToken);
        
        var storedToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == refreshTokenHash);

        if (storedToken == null || !storedToken.IsActive)
        {
            return Unauthorized(new
            {
                message = "Invalid refresh token"
            });
        }

        var user = storedToken.User;

        var roles = await _userManager.GetRolesAsync(user);

        var newAccessToken = _jwtTokenProvider.CreateAccessToken(user, roles);

        // 기존 Refresh Token 폐기
        storedToken.RevokedAt = DateTime.UtcNow;
        storedToken.UpdatedAt = DateTime.UtcNow;

        // 새 Refresh Token 발급
        var newRefreshToken = _refreshTokenProvider.GenerateRefreshToken();
        var newRefreshTokenHash = _refreshTokenProvider.HashRefreshToken(newRefreshToken);

        var newRefreshTokenEntity = new RefreshToken
        {
            TokenHash = newRefreshTokenHash,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _context.RefreshTokens.Add(newRefreshTokenEntity);

        await _context.SaveChangesAsync();

        Response.Cookies.Append(
            "refreshToken",
            newRefreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // local HTTP 개발 중
                SameSite = SameSiteMode.Strict,
                Expires = newRefreshTokenEntity.ExpiresAt
            });

        return Ok(new
        {
            message = "Token refreshed successfully",
            accessToken = newAccessToken
        });
    }
    
    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies["refreshToken"];

        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            var refreshTokenHash = _refreshTokenProvider.HashRefreshToken(refreshToken);

            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.TokenHash == refreshTokenHash);

            if (storedToken != null && storedToken.IsActive)
            {
                storedToken.RevokedAt = DateTime.UtcNow;
                storedToken.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }
        }

        Response.Cookies.Delete("refreshToken");

        return Ok(new
        {
            message = "Logged out successfully"
        });
    }

}