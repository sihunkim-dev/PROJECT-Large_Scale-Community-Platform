namespace Large_Scale_CommunityPlatform.Models.Entities;

public class Comment
{
    public long CommentId { get; set; }
    public long PostId { get; set; }
    public string UserId { get; set; } = string.Empty;
    
    public long? ParentCommentId { get; set; }
    
    public string Path { get; set; } = string.Empty;
    public int Depth { get; set; } = 0;
    
    public string CommentText { get; set; } = string.Empty;

    public long LikeCount { get; set; } = 0;
    public long DislikeCount { get; set; } = 0;

    public bool IsHidden { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public User? User { get; set; }
    public Post? Post { get; set; }
    
    public Comment? ParentComment { get; set; }
    
    public ICollection<CommentReaction> CommentReactions { get; set; } = new List<CommentReaction>();
    public ICollection<Comment> Replies { get; set; } = new List<Comment>();
    
}