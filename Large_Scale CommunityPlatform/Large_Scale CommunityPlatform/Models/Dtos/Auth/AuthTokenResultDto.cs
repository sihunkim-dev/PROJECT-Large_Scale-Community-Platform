namespace Large_Scale_CommunityPlatform.Models.Dtos.Auth;

public class AuthTokenResultDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiresAt { get; set; }
}