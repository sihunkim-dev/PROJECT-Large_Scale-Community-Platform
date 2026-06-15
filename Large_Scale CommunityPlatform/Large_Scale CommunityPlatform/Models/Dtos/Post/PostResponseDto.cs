namespace Large_Scale_CommunityPlatform.Models.Dtos.Post;

public class PostResponseDto
{
    public long PostId { get; set; }

    public long CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;

    public string PostTitle { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    public long ViewCount { get; set; }
    public long LikeCount { get; set; }
    public long DislikeCount { get; set; }

    public bool IsHidden { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}