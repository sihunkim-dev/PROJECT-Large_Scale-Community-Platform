namespace Large_Scale_CommunityPlatform.Models.Dtos.Reaction;

public class ReactionSummaryDto
{
    public string TargetType { get; set; } = string.Empty;
    public long TargetId { get; set; }
    
    public long likesCount { get; set; }
    public long dislikesCount { get; set; }
}