namespace Large_Scale_CommunityPlatform.Models.Entities;

public class Post
{
    //PK/FK
    public long PostId { get; set; }
    public long CategoryId { get; set; }
    
    public string UserId { get; set; } = string.Empty;
    
    public string PostTitle { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    
    public long ViewCount { get; set; }
    public long LikesCount { get; set; }
    public long DislikesCount { get; set; }
    
    public bool IsHidden { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public User? User { get; set; }
    public Category? Category { get; set; }
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<PostReaction> PostReactions { get; set; } = new List<PostReaction>();
}