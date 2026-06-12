using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace Large_Scale_CommunityPlatform.Services;

public class JwtRefreshTokenProvider
{
    public string GenerateRefreshToken()
    {
        var rnadomBytes = RandomNumberGenerator.GetBytes(64);
        return WebEncoders.Base64UrlEncode(rnadomBytes);
    }

    public string HashRefreshToken(string refreshToken)
    {
        var tokenBytes = Encoding.UTF8.GetBytes(refreshToken);
        
        var hashBytes = SHA256.HashData(tokenBytes);

        return Convert.ToHexString(hashBytes);
    }
}