namespace Large_Scale_CommunityPlatform.Models.Entities;

public class RefreshToken
{
    public long TokenId { get; set; }
    
    public string TokenHash { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;
    
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt != default;
    public bool IsActive => !IsExpired && !IsRevoked;
}