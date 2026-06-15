namespace Large_Scale_CommunityPlatform.Models.Dtos.Post;

public class PostCreateDto
{
    public long CategoryId { get; set; }
    public string PostTitle { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}