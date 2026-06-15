using Large_Scale_CommunityPlatform.Data;
using Large_Scale_CommunityPlatform.Models.Dtos.Reaction;
using Large_Scale_CommunityPlatform.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Large_Scale_CommunityPlatform.Services.Reactions;

public class PostReactionService
{
    private readonly ApplicationDbContext _context;
    private readonly ReactionCacheService _cacheService;

    private const string TargetType = "post";

    public PostReactionService(ApplicationDbContext context, ReactionCacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<ReactionActionResultDto> ReactAsync(
        long postId,
        string userId,
        ReactionType reactionType)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.PostId == postId);

        if (post == null || post.IsHidden)
        {
            return ReactionActionResultDto.Fail("Post not found");
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
        await _cacheService.RemoveAsync(TargetType, postId);

        return ReactionActionResultDto.Success(
            "Post reaction updated successfully",
            ToSummary(post)
        );
    }
    
    public async Task<ReactionActionResultDto> RemoveAsync(long postId, string userId)
    {
        var post = await _context.Posts
            .FirstOrDefaultAsync(p => p.PostId == postId);

        if (post == null || post.IsHidden)
        {
            return ReactionActionResultDto.Fail("Post not found");
        }

        var reaction = await _context.PostReactions
            .FirstOrDefaultAsync(r => r.PostId == postId && r.UserId == userId);

        if (reaction == null)
        {
            return ReactionActionResultDto.Fail("Reaction does not exist");
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
        await _cacheService.RemoveAsync(TargetType, postId);

        return ReactionActionResultDto.Success(
            "Post reaction removed successfully",
            ToSummary(post)
        );
    }

    public async Task<ReactionSummaryDto?> GetSummaryAsync(long postId)
    {
        var cachedSummary = await _cacheService.GetAsync(TargetType, postId);

        if (cachedSummary != null)
        {
            return cachedSummary;
        }

        var post = await _context.Posts
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PostId == postId);

        if (post == null || post.IsHidden)
        {
            return null;
        }

        var summary = ToSummary(post);

        await _cacheService.SetAsync(summary);

        return summary;
    }

    private static ReactionSummaryDto ToSummary(Post post)
    {
        return new ReactionSummaryDto
        {
            TargetType = TargetType,
            TargetId = post.PostId,
            likesCount = post.LikesCount,
            dislikesCount = post.DislikesCount
        };
    }


}