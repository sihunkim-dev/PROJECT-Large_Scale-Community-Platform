namespace Large_Scale_CommunityPlatform.Models.Entities;

public class CommentReaction
{
    public long CommentReactionId { get; set; }
    
    public long CommentId { get; set; } 
    public string UserId { get; set; } = string.Empty;
    
    public ReactionType ReactionType { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Comment Comment { get; set; }= null!;
}