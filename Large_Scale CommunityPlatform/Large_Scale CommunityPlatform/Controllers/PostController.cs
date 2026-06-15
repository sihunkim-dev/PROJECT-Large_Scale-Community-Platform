using System.Security.Claims;
using Large_Scale_CommunityPlatform.Data;
using Large_Scale_CommunityPlatform.Models.Dtos.Post;
using Large_Scale_CommunityPlatform.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Large_Scale_CommunityPlatform.Controllers;

[ApiController]
[Route("api/posts")]
public class PostController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PostController(ApplicationDbContext context)
    {
        _context = context;
    }

    
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreatePost(PostCreateDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId is null)
        {
            return Unauthorized();
        }
        
        var title = dto.PostTitle.Trim();
        var content = dto.Content.Trim();
        
        if (string.IsNullOrWhiteSpace(title))
        {
            return BadRequest(new
            {
                message = "Post title is required"
            });
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            return BadRequest(new
            {
                message = "Post content is required"
            });
        }

        var categoryExists = await _context.Categories
            .AnyAsync(c => c.CategoryId == dto.CategoryId);

        if (!categoryExists)
        {
            return BadRequest(new
            {
                message = "Category does not exist"
            });
        }

        var post = new Post
        {
            CategoryId = dto.CategoryId,
            UserId = userId,
            PostTitle = title,
            Content = content,
            ViewCount = 0,
            LikesCount = 0,
            DislikesCount = 0,
            IsHidden = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Posts.Add(post);
        await _context.SaveChangesAsync();

        var createdPost = await _context.Posts
            .Include(p => p.Category)
            .Include(p => p.User)
            .FirstAsync(p => p.PostId == post.PostId);

        return CreatedAtAction(
            nameof(GetPost), 
            new { postId = createdPost.PostId },
            ToResponse(createdPost)
        );

    }
    
    [Authorize]
    [HttpPut("{postId:long}")]
    public async Task<IActionResult> UpdatePost(long postId, PostUpdateDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
        {
            return Unauthorized();
        }

        var post = await _context.Posts
            .Include(p => p.Category)
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.PostId == postId);

        if (post == null || post.IsHidden)
        {
            return NotFound(new
            {
                message = "Post not found"
            });
        }

        if (post.UserId != userId)
        {
            return Forbid();
        }

        var title = dto.PostTitle.Trim();
        var content = dto.Content.Trim();

        if (string.IsNullOrWhiteSpace(title))
        {
            return BadRequest(new
            {
                message = "Post title is required"
            });
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            return BadRequest(new
            {
                message = "Post content is required"
            });
        }

        post.PostTitle = title;
        post.Content = content;
        post.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(ToResponse(post));
    }
    
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetPosts()
    {
        var posts = await _context.Posts
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.User)
            .Where(p => !p.IsHidden)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PostResponseDto
            {
                PostId = p.PostId,
                CategoryId = p.CategoryId,
                CategoryName = p.Category != null ? p.Category.CategoryName : string.Empty,
                UserId = p.UserId,
                UserName = p.User != null ? p.User.UserName ?? string.Empty : string.Empty,
                PostTitle = p.PostTitle,
                Content = p.Content,
                ViewCount = p.ViewCount,
                LikeCount = p.LikesCount,
                DislikeCount = p.DislikesCount,
                IsHidden = p.IsHidden,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToListAsync();

        return Ok(posts);
    }
    
    
    [AllowAnonymous]
    [HttpGet("{postId:long}")]
    public async Task<IActionResult> GetPost(long postId)
    {
        var post = await _context.Posts
            .Include(p => p.Category)
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.PostId == postId);

        if (post == null || post.IsHidden)
        {
            return NotFound(new
            {
                message = "Post not found"
            });
        }

        post.ViewCount += 1;
        await _context.SaveChangesAsync();

        return Ok(ToResponse(post));
    }
    
    [Authorize]
    [HttpDelete("{postId:long}")]
    public async Task<IActionResult> DeletePost(long postId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
        {
            return Unauthorized();
        }

        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.PostId == postId);

        if (post == null || post.IsHidden)
        {
            return NotFound(new
            {
                message = "Post not found"
            });
        }

        var isOwner = post.UserId == userId;
        var isAdmin = User.IsInRole("Admin");
        var isCategoryManager = User.IsInRole("CategoryManager");

        if (!isOwner && !isAdmin && !isCategoryManager)
        {
            return Forbid();
        }

        post.IsHidden = true;
        post.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Post deleted successfully"
        });
    }
    
    private static PostResponseDto ToResponse(Post post)
    {
        return new PostResponseDto
        {
            PostId = post.PostId,
            CategoryId = post.CategoryId,
            CategoryName = post.Category?.CategoryName ?? string.Empty,
            UserId = post.UserId,
            UserName = post.User?.UserName ?? string.Empty,
            PostTitle = post.PostTitle,
            Content = post.Content,
            ViewCount = post.ViewCount,
            LikeCount = post.LikesCount,
            DislikeCount = post.DislikesCount,
            IsHidden = post.IsHidden,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt
        };
    }
}