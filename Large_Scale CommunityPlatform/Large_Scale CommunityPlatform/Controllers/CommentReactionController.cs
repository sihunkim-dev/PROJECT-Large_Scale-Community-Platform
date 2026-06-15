using System.Security.Claims;
using Large_Scale_CommunityPlatform.Data;
using Large_Scale_CommunityPlatform.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Large_Scale_CommunityPlatform.Controllers;

[ApiController]
[Route("api/comments/{commentId:long}/reactions")]
public class CommentReactionController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CommentReactionController(ApplicationDbContext context)
    {
        _context = context;
    }

    [Authorize]
    [HttpPost("like")]
    public async Task<IActionResult> LikeComment(long commentId)
    {
        return await ReactToComment(commentId, ReactionType.Like);
    }

    [Authorize]
    [HttpPost("dislike")]
    public async Task<IActionResult> DislikeComment(long commentId)
    {
        return await ReactToComment(commentId, ReactionType.Dislike);
    }

    [Authorize]
    [HttpDelete]
    public async Task<IActionResult> RemoveCommentReaction(long commentId)
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
            return NotFound(new { message = "Comment not found" });
        }

        var reaction = await _context.CommentReactions
            .FirstOrDefaultAsync(r => r.CommentId == commentId && r.UserId == userId);

        if (reaction == null)
        {
            return BadRequest(new { message = "Reaction does not exist" });
        }

        if (reaction.ReactionType == ReactionType.Like)
        {
            comment.LikeCount = Math.Max(0, comment.LikeCount - 1);
        }
        else
        {
            comment.DislikeCount = Math.Max(0, comment.DislikeCount - 1);
        }

        _context.CommentReactions.Remove(reaction);
        comment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Comment reaction removed successfully",
            likeCount = comment.LikeCount,
            dislikeCount = comment.DislikeCount
        });
    }

    [AllowAnonymous]
    [HttpGet("summary")]
    public async Task<IActionResult> GetCommentReactionSummary(long commentId)
    {
        var comment = await _context.Comments
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CommentId == commentId);

        if (comment == null || comment.IsHidden)
        {
            return NotFound(new { message = "Comment not found" });
        }

        return Ok(new
        {
            commentId = comment.CommentId,
            likeCount = comment.LikeCount,
            dislikeCount = comment.DislikeCount
        });
    }

    private async Task<IActionResult> ReactToComment(long commentId, ReactionType reactionType)
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
            return NotFound(new { message = "Comment not found" });
        }

        var existingReaction = await _context.CommentReactions
            .FirstOrDefaultAsync(r => r.CommentId == commentId && r.UserId == userId);

        if (existingReaction == null)
        {
            var reaction = new CommentReaction
            {
                CommentId = commentId,
                UserId = userId,
                ReactionType = reactionType,
                CreatedAt = DateTime.UtcNow
            };

            _context.CommentReactions.Add(reaction);

            if (reactionType == ReactionType.Like)
            {
                comment.LikeCount += 1;
            }
            else
            {
                comment.DislikeCount += 1;
            }
        }
        else if (existingReaction.ReactionType == reactionType)
        {
            if (reactionType == ReactionType.Like)
            {
                comment.LikeCount = Math.Max(0, comment.LikeCount - 1);
            }
            else
            {
                comment.DislikeCount = Math.Max(0, comment.DislikeCount - 1);
            }

            _context.CommentReactions.Remove(existingReaction);
        }
        else
        {
            if (existingReaction.ReactionType == ReactionType.Like)
            {
                comment.LikeCount = Math.Max(0, comment.LikeCount - 1);
                comment.DislikeCount += 1;
            }
            else
            {
                comment.DislikeCount = Math.Max(0, comment.DislikeCount - 1);
                comment.LikeCount += 1;
            }

            existingReaction.ReactionType = reactionType;
        }

        comment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Comment reaction updated successfully",
            likeCount = comment.LikeCount,
            dislikeCount = comment.DislikeCount
        });
    }
}