namespace Large_Scale_CommunityPlatform.Models.Entities;

public class Category
{
    public long CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public CategoryStatus Status { get; set; } = CategoryStatus.Pending;

    public string RequestedById { get; set; } = string.Empty;
    public User RequestedBy { get; set; } = null!;

    public string? ApprovedById { get; set; }
    public User? ApprovedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedAt { get; set; }
    public DateTime? RejectedAt { get; set; }

    public ICollection<Post> Posts { get; set; } = new List<Post>();
}