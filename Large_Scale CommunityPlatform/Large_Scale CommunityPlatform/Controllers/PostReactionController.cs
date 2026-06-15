using System.Security.Claims;
using Large_Scale_CommunityPlatform.Data;
using Large_Scale_CommunityPlatform.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Large_Scale_CommunityPlatform.Controllers;

[ApiController]
[Route("api/posts/{postId:long}/reactions")]
public class PostReactionController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PostReactionController(ApplicationDbContext context)
    {
        _context = context;
    }

    [Authorize]
    [HttpPost("like")]
    public async Task<IActionResult> LikePost(long postId)
    {
        return await ReactToPost(postId, ReactionType.Like);
    }

    [Authorize]
    [HttpPost("dislike")]
    public async Task<IActionResult> DislikePost(long postId)
    {
        return await ReactToPost(postId, ReactionType.Dislike);
    }

    [Authorize]
    [HttpDelete]
    public async Task<IActionResult> RemovePostReaction(long postId)
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
            return NotFound(new { message = "Post not found" });
        }

        var reaction = await _context.PostReactions
            .FirstOrDefaultAsync(r => r.PostId == postId && r.UserId == userId);

        if (reaction == null)
        {
            return BadRequest(new { message = "Reaction does not exist" });
        }

        if (reaction.ReactionType == ReactionType.Like)
        {
            post.LikesCount = Math.Max(0, post.LikesCount - 1);
        }
        else
        {
            post.DislikesCount = Math.Max(0, post.DislikesCount - 1);
        }

        _context.PostReactions.Remove(reaction);
        post.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Post reaction removed successfully",
            likeCount = post.LikesCount,
            dislikeCount = post.DislikesCount
        });
    }

    [AllowAnonymous]
    [HttpGet("summary")]
    public async Task<IActionResult> GetPostReactionSummary(long postId)
    {
        var post = await _context.Posts
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PostId == postId);

        if (post == null || post.IsHidden)
        {
            return NotFound(new { message = "Post not found" });
        }

        return Ok(new
        {
            postId = post.PostId,
            likeCount = post.LikesCount,
            dislikeCount = post.DislikesCount
        });
    }

    private async Task<IActionResult> ReactToPost(long postId, ReactionType reactionType)
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
            return NotFound(new { message = "Post not found" });
        }

        var existingReaction = await _context.PostReactions
            .FirstOrDefaultAsync(r => r.PostId == postId && r.UserId == userId);

        if (existingReaction == null)
        {
            var reaction = new PostReaction
            {
                PostId = postId,
                UserId = userId,
                ReactionType = reactionType,
                CreatedAt = DateTime.UtcNow
            };

            _context.PostReactions.Add(reaction);

            if (reactionType == ReactionType.Like)
            {
                post.LikesCount += 1;
            }
            else
            {
                post.DislikesCount += 1;
            }
        }
        else if (existingReaction.ReactionType == reactionType)
        {
            if (reactionType == ReactionType.Like)
            {
                post.LikesCount = Math.Max(0, post.LikesCount - 1);
            }
            else
            {
                post.DislikesCount = Math.Max(0, post.DislikesCount - 1);
            }

            _context.PostReactions.Remove(existingReaction);
        }
        else
        {
            if (existingReaction.ReactionType == ReactionType.Like)
            {
                post.LikesCount = Math.Max(0, post.LikesCount - 1);
                post.DislikesCount += 1;
            }
            else
            {
                post.DislikesCount = Math.Max(0, post.DislikesCount - 1);
                post.LikesCount += 1;
            }

            existingReaction.ReactionType = reactionType;
        }

        post.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Post reaction updated successfully",
            likeCount = post.LikesCount,
            dislikeCount = post.DislikesCount
        });
    }
}