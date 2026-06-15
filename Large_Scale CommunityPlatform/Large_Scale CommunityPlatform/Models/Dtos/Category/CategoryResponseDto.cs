namespace Large_Scale_CommunityPlatform.Models.Dtos.Category;

public class CategoryResponseDto
{
    public long CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public string RequestedById { get; set; } = string.Empty;
    public string? ApprovedById { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
}