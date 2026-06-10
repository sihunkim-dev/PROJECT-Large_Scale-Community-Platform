namespace Large_Scale_CommunityPlatform.Models.Entities;

public class CommentReaction
{
    public long CommentReactionId { get; set; }
    public long PostId { get; set; }
    public string UserId { get; set; } = string.Empty;
    
    public ReactionType Type { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public User? User { get; set; }
    public Comment? Comment { get; set; }
    
}