using System.Security.Claims;
using Large_Scale_CommunityPlatform.Data;
using Large_Scale_CommunityPlatform.Models.Entities;
using Large_Scale_CommunityPlatform.Services.Reactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Large_Scale_CommunityPlatform.Controllers;

[ApiController]
[Route("api/comments/{commentId:long}/reactions")]
public class CommentReactionController : ControllerBase
{
    private readonly CommentReactionService _commentReactionService;

    public CommentReactionController(CommentReactionService commentReactionService)
    {
        _commentReactionService = commentReactionService;
    }
    
    [Authorize]
    [HttpPost("like")]
    public async Task<IActionResult> LikeComment(long commentId)
    {
        return await React(commentId, ReactionType.Like);
    }

    [Authorize]
    [HttpPost("dislike")]
    public async Task<IActionResult> DislikeComment(long commentId)
    {
        return await React(commentId, ReactionType.Dislike);
    }

    [Authorize]
    [HttpDelete]
    public async Task<IActionResult> RemoveCommentReaction(long commentId)
    {
        var userId = GetUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _commentReactionService.RemoveAsync(commentId, userId);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Message });
        }

        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("summary")]
    public async Task<IActionResult> GetCommentReactionSummary(long commentId)
    {
        var summary = await _commentReactionService.GetSummaryAsync(commentId);

        if (summary == null)
        {
            return NotFound(new { message = "Comment not found" });
        }

        return Ok(summary);
    }

    private async Task<IActionResult> React(long commentId, ReactionType reactionType)
    {
        var userId = GetUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _commentReactionService.ReactAsync(commentId, userId, reactionType);

        if (!result.IsSuccess)
        {
            return NotFound(new { message = result.Message });
        }

        return Ok(result);
    }

    private string? GetUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}