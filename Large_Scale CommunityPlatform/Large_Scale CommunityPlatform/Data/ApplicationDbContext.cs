using Large_Scale_CommunityPlatform.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Large_Scale_CommunityPlatform.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        
    }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<PostReaction> PostReactions { get; set; }
    public DbSet<CommentReaction> CommentReactions { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    
    //Fluent API
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<RefreshToken>()
            .HasKey(rt => rt.TokenId);

        builder.Entity<RefreshToken>()
            .Property(t => t.TokenHash)
            .IsRequired()
            .HasMaxLength(256);
        
        builder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Entity<RefreshToken>()
            .HasIndex(rt => rt.TokenHash)
            .IsUnique();
        
        // Comment - Post
        builder.Entity<Comment>()
            .HasOne(c => c.Post)
            .WithMany(p => p.Comments)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade);
        
        
        builder.Entity<Category>()
            .HasOne(c => c.RequestedBy)
            .WithMany()
            .HasForeignKey(c => c.RequestedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Category>()
            .HasOne(c => c.ApprovedBy)
            .WithMany()
            .HasForeignKey(c => c.ApprovedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Category>()
            .HasIndex(c => c.CategoryName)
            .IsUnique();

        builder.Entity<Category>()
            .Property(c => c.CategoryName)
            .IsRequired()
            .HasMaxLength(100);
        
        // Comment - User
        builder.Entity<Comment>()
            .HasOne(c => c.User)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        //Post Reaction (POSTReaction - Post)
        builder.Entity<PostReaction>()
            .HasOne(pr => pr.Post)
            .WithMany(p => p.PostReactions)
            .HasForeignKey(pr => pr.PostId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Entity<Comment>()
            .HasOne(c => c.ParentComment)
            .WithMany(c => c.Replies)
            .HasForeignKey(c => c.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Comment>()
            .HasIndex(c => new { c.PostId, c.Path });

        builder.Entity<Comment>()
            .Property(c => c.CommentText)
            .HasColumnType("text");
        
        // Post - Category
        builder.Entity<Post>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Posts)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

// Post - User
        builder.Entity<Post>()
            .HasOne(p => p.User)
            .WithMany(u => u.Posts)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Post>()
            .Property(p => p.PostTitle)
            .IsRequired()
            .HasMaxLength(200);

        builder.Entity<Post>()
            .Property(p => p.Content)
            .IsRequired()
            .HasColumnType("text");

        builder.Entity<Post>()
            .HasIndex(p => p.CategoryId);

        builder.Entity<Post>()
            .HasIndex(p => p.UserId);

        builder.Entity<Post>()
            .HasIndex(p => p.CreatedAt);
        
        
        //POSTReaction - USEr
        builder.Entity<PostReaction>()
            .HasOne(pr => pr.User)
            .WithMany(u => u.PostReactions)
            .HasForeignKey(pr => pr.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Entity<PostReaction>()
            .HasIndex(pr => new { pr.PostId, pr.UserId })
            .IsUnique();
        
        
        builder.Entity<CommentReaction>()
            .HasOne(cr => cr.Comment)
            .WithMany(c => c.CommentReactions)
            .HasForeignKey(cr => cr.CommentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<CommentReaction>()
            .HasOne(cr => cr.User)
            .WithMany(u => u.CommentReactions)
            .HasForeignKey(cr => cr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        
        builder.Entity<CommentReaction>()
            .HasIndex(cr => new { cr.CommentId, cr.UserId })
            .IsUnique();
    }
}