namespace Large_Scale_CommunityPlatform.Models.Entities;

public class PostReaction
{
    public long PostReactionId { get; set; }

    public long PostId { get; set; }
    public string UserId { get; set; } = string.Empty;
    
    public ReactionType ReactionType { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public User User { get; set; } = null!;
    public Post Post { get; set; } = null!;
}