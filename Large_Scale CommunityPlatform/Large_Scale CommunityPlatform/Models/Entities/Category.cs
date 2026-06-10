namespace Large_Scale_CommunityPlatform.Models.Entities;

public class Category
{
    public long CategoryId { get; set; }
    
    public string CategoryName { get; set; } = string.Empty;
    
    public ICollection<Post> Posts { get; set; } = new List<Post>();
}