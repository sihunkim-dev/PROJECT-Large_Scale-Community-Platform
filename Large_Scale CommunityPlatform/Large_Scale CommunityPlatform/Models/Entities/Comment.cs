namespace Large_Scale_CommunityPlatform.Models.Entities;

public class Comment
{
    public long CommentId { get; set; }
    public long PostId { get; set; }
    public string UserId { get; set; } = string.Empty;
    
    public string Path { get; set; } = string.Empty;
    public int Depth { get; set; }
    public string CommentText { get; set; } = string.Empty;

    public long LikeCount { get; set; } = 0;
    public long DislikeCount { get; set; } = 0;
    
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
    
    public User? User { get; set; }
    public Post? Post { get; set; }
    public ICollection<CommentReaction> CommentReactions { get; set; } = new List<CommentReaction>();
}