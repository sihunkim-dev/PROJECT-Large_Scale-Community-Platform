using Microsoft.AspNetCore.Identity;

namespace Large_Scale_CommunityPlatform.Models.Entities;

public class User : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    //Date of Birth
    public DateTime DoB { get; set; }
    
    //Navigation
    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<PostReaction> PostReactions { get; set; } = new List<PostReaction>();
    public ICollection<CommentReaction> CommentReactions { get; set; } = new List<CommentReaction>();
    
}