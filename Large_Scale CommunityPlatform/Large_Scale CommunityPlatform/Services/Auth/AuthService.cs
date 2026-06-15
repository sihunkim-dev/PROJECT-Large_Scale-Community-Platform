using Large_Scale_CommunityPlatform.Data;
using Large_Scale_CommunityPlatform.Models.Dtos.Auth;
using Large_Scale_CommunityPlatform.Models.Dtos.User;
using Large_Scale_CommunityPlatform.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Large_Scale_CommunityPlatform.Services.Auth;

public class AuthService
{
    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly JwtTokenProvider _jwtTokenProvider;
    private readonly JwtRefreshTokenProvider _jwtRefreshTokenProvider;

    public AuthService(
        UserManager<User> userManager, 
        ApplicationDbContext dbContext, 
        JwtTokenProvider jwtTokenProvider, 
        JwtRefreshTokenProvider jwtRefreshTokenProvider)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _jwtTokenProvider = jwtTokenProvider;
        _jwtRefreshTokenProvider = jwtRefreshTokenProvider;
    }

    public async Task<IdentityResult> RegisterAsync(UserRegisterDto  userRegisterDto)
    {
        //Create user instance
        var user = new User
        {
            UserName = userRegisterDto.Email,
            Email = userRegisterDto.Email,
            FullName = userRegisterDto.FullName,
            DoB = userRegisterDto.DoB,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, userRegisterDto.Password);
        
        if (!result.Succeeded)
        {
            return result;
        }

        var roleResult = await _userManager.AddToRoleAsync(user, "User");

        return roleResult;
    }
    
    public async Task<AuthTokenResultDto?> SignInAsync(UserSigninDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);

        if (user == null)
        {
            return null;
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);

        if (!isPasswordValid)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);

        var accessToken = _jwtTokenProvider.CreateAccessToken(user, roles);

        var refreshToken = _jwtRefreshTokenProvider.GenerateRefreshToken();
        var refreshTokenHash = _jwtRefreshTokenProvider.HashRefreshToken(refreshToken);

        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

        var refreshTokenEntity = new RefreshToken
        {
            TokenHash = refreshTokenHash,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ExpiresAt = refreshTokenExpiresAt
        };

        _dbContext.RefreshTokens.Add(refreshTokenEntity);
        await _dbContext.SaveChangesAsync();

        return new AuthTokenResultDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            RefreshTokenExpiresAt = refreshTokenExpiresAt
        };
    }

    public async Task<AuthTokenResultDto?> RefreshAsync(string? refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return null;
        }

        var refreshTokenHash = _jwtRefreshTokenProvider.HashRefreshToken(refreshToken);

        var storedToken = await _dbContext.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == refreshTokenHash);

        if (storedToken == null || !storedToken.IsActive)
        {
            return null;
        }

        var user = storedToken.User;
        var roles = await _userManager.GetRolesAsync(user);

        var newAccessToken = _jwtTokenProvider.CreateAccessToken(user, roles);

        storedToken.RevokedAt = DateTime.UtcNow;
        storedToken.UpdatedAt = DateTime.UtcNow;

        var newRefreshToken = _jwtRefreshTokenProvider.GenerateRefreshToken();
        var newRefreshTokenHash = _jwtRefreshTokenProvider.HashRefreshToken(newRefreshToken);

        var newRefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

        var newRefreshTokenEntity = new RefreshToken
        {
            TokenHash = newRefreshTokenHash,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ExpiresAt = newRefreshTokenExpiresAt
        };

        _dbContext.RefreshTokens.Add(newRefreshTokenEntity);
        await _dbContext.SaveChangesAsync();

        return new AuthTokenResultDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            RefreshTokenExpiresAt = newRefreshTokenExpiresAt
        };
    }

    public async Task LogoutAsync(string? refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return;
        }

        var refreshTokenHash = _jwtRefreshTokenProvider.HashRefreshToken(refreshToken);

        var storedToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == refreshTokenHash);

        if (storedToken == null || !storedToken.IsActive)
        {
            return;
        }

        storedToken.RevokedAt = DateTime.UtcNow;
        storedToken.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
    }
}