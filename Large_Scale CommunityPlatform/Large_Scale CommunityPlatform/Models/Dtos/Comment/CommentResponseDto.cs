namespace Large_Scale_CommunityPlatform.Models.Dtos.Comment;

public class CommentResponseDto
{
    public long CommentId { get; set; }

    public long PostId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;

    public long? ParentCommentId { get; set; }

    public string Path { get; set; } = string.Empty;
    public int Depth { get; set; }

    public string CommentText { get; set; } = string.Empty;

    public long LikeCount { get; set; }
    public long DislikeCount { get; set; }

    public bool IsHidden { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }   
}