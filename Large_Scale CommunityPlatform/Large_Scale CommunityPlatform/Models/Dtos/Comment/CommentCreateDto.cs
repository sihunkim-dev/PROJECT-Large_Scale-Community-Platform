namespace Large_Scale_CommunityPlatform.Models.Dtos.Comment;

public class CommentCreateDto
{
    public long? ParentCommentId { get; set; }
    public string CommentText { get; set; } = string.Empty;
}