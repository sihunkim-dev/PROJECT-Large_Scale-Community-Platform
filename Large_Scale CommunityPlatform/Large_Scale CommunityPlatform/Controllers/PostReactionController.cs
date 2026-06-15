using System.Security.Claims;
using Large_Scale_CommunityPlatform.Data;
using Large_Scale_CommunityPlatform.Models.Entities;
using Large_Scale_CommunityPlatform.Services.Reactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Large_Scale_CommunityPlatform.Controllers;

[ApiController]
[Route("api/posts/{postId:long}/reactions")]
public class PostReactionController : ControllerBase
{
    private readonly PostReactionService _postReactionService;
    
    public PostReactionController(PostReactionService postReactionService)
    {
        _postReactionService = postReactionService;
    }

    [Authorize]
    [HttpPost("like")]
    public async Task<IActionResult> LikePost(long postId)
    {
        return await React(postId, ReactionType.Like);
    }

    [Authorize]
    [HttpPost("dislike")]
    public async Task<IActionResult> DislikePost(long postId)
    {
        return await React(postId, ReactionType.Dislike);
    }

    [Authorize]
    [HttpDelete]
    public async Task<IActionResult> RemovePostReaction(long postId)
    {
        var userId = GetUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _postReactionService.RemoveAsync(postId, userId);

        if (!result.IsSuccess)
        {
            return BadRequest(new { message = result.Message });
        }

        return Ok(result);
    }
    
    [AllowAnonymous]
    [HttpGet("summary")]
    public async Task<IActionResult> GetPostReactionSummary(long postId)
    {
        var summary = await _postReactionService.GetSummaryAsync(postId);

        if (summary == null)
        {
            return NotFound(new { message = "Post not found" });
        }

        return Ok(summary);
    }
    
    private async Task<IActionResult> React(long postId, ReactionType reactionType)
    {
        var userId = GetUserId();

        if (userId == null)
        {
            return Unauthorized();
        }
        
        var result = await _postReactionService.ReactAsync(postId, userId, reactionType);

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