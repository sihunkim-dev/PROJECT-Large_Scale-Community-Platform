using System.Security.Claims;
using Large_Scale_CommunityPlatform.Data;
using Large_Scale_CommunityPlatform.Models.Dtos.Comment;
using Large_Scale_CommunityPlatform.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Large_Scale_CommunityPlatform.Controllers;

[ApiController]
[Route("api")]
public class CommentController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CommentController(ApplicationDbContext context)
    {
        _context = context;
    }

    [Authorize]
    [HttpPost("posts/{postId:long}/comments")]
    public async Task<IActionResult> CreateComment(long postId, CommentCreateDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
        {
            return Unauthorized();
        }

        var commentText = dto.CommentText.Trim();

        if (string.IsNullOrWhiteSpace(commentText))
        {
            return BadRequest(new
            {
                message = "Comment text is required"
            });
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

        Comment? parentComment = null;

        if (dto.ParentCommentId != null)
        {
            parentComment = await _context.Comments
                .FirstOrDefaultAsync(c =>
                    c.CommentId == dto.ParentCommentId &&
                    c.PostId == postId);

            if (parentComment == null || parentComment.IsHidden)
            {
                return BadRequest(new
                {
                    message = "Parent comment does not exist"
                });
            }
        }

        var comment = new Comment
        {
            PostId = postId,
            UserId = userId,
            ParentCommentId = dto.ParentCommentId,
            CommentText = commentText,
            Depth = parentComment == null ? 0 : parentComment.Depth + 1,
            Path = string.Empty,
            LikeCount = 0,
            DislikeCount = 0,
            IsHidden = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        var pathSegment = comment.CommentId.ToString("D10");

        comment.Path = parentComment == null
            ? pathSegment
            : $"{parentComment.Path}/{pathSegment}";

        await _context.SaveChangesAsync();

        var createdComment = await _context.Comments
            .Include(c => c.User)
            .FirstAsync(c => c.CommentId == comment.CommentId);

        return Ok(ToResponse(createdComment));
    }

    [AllowAnonymous]
    [HttpGet("posts/{postId:long}/comments")]
    public async Task<IActionResult> GetCommentsByPost(long postId)
    {
        var postExists = await _context.Posts
            .AnyAsync(p => p.PostId == postId && !p.IsHidden);

        if (!postExists)
        {
            return NotFound(new
            {
                message = "Post not found"
            });
        }

        var comments = await _context.Comments
            .AsNoTracking()
            .Include(c => c.User)
            .Where(c => c.PostId == postId)
            .OrderBy(c => c.Path)
            .Select(c => new CommentResponseDto
            {
                CommentId = c.CommentId,
                PostId = c.PostId,
                UserId = c.UserId,
                UserName = c.User != null ? c.User.UserName ?? string.Empty : string.Empty,
                ParentCommentId = c.ParentCommentId,
                Path = c.Path,
                Depth = c.Depth,
                CommentText = c.IsHidden ? "[deleted]" : c.CommentText,
                LikeCount = c.LikeCount,
                DislikeCount = c.DislikeCount,
                IsHidden = c.IsHidden,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync();

        return Ok(comments);
    }

    [Authorize]
    [HttpPut("comments/{commentId:long}")]
    public async Task<IActionResult> UpdateComment(long commentId, CommentUpdateDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
        {
            return Unauthorized();
        }

        var comment = await _context.Comments
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.CommentId == commentId);

        if (comment == null || comment.IsHidden)
        {
            return NotFound(new
            {
                message = "Comment not found"
            });
        }

        if (comment.UserId != userId)
        {
            return Forbid();
        }

        var commentText = dto.CommentText.Trim();

        if (string.IsNullOrWhiteSpace(commentText))
        {
            return BadRequest(new
            {
                message = "Comment text is required"
            });
        }

        comment.CommentText = commentText;
        comment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(ToResponse(comment));
    }

    [Authorize]
    [HttpDelete("comments/{commentId:long}")]
    public async Task<IActionResult> DeleteComment(long commentId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null)
        {
            return Unauthorized();
        }

        var comment = await _context.Comments
            .FirstOrDefaultAsync(c => c.CommentId == commentId);

        if (comment == null || comment.IsHidden)
        {
            return NotFound(new
            {
                message = "Comment not found"
            });
        }

        var isOwner = comment.UserId == userId;
        var isAdmin = User.IsInRole("Admin");
        var isCategoryManager = User.IsInRole("CategoryManager");

        if (!isOwner && !isAdmin && !isCategoryManager)
        {
            return Forbid();
        }

        comment.IsHidden = true;
        comment.CommentText = "[deleted]";
        comment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Comment deleted successfully"
        });
    }

    private static CommentResponseDto ToResponse(Comment comment)
    {
        return new CommentResponseDto
        {
            CommentId = comment.CommentId,
            PostId = comment.PostId,
            UserId = comment.UserId,
            UserName = comment.User?.UserName ?? string.Empty,
            ParentCommentId = comment.ParentCommentId,
            Path = comment.Path,
            Depth = comment.Depth,
            CommentText = comment.IsHidden ? "[deleted]" : comment.CommentText,
            LikeCount = comment.LikeCount,
            DislikeCount = comment.DislikeCount,
            IsHidden = comment.IsHidden,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        };
    }
}