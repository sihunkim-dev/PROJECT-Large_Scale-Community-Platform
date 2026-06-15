using Large_Scale_CommunityPlatform.Data;
using Large_Scale_CommunityPlatform.Models.Dtos.Reaction;
using Large_Scale_CommunityPlatform.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Large_Scale_CommunityPlatform.Services.Reactions;

public class CommentReactionService
{
    private readonly ApplicationDbContext _context;
    private readonly ReactionCacheService _cacheService;

    private const string TargetType = "comment";

    public CommentReactionService(
        ApplicationDbContext context,
        ReactionCacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<ReactionActionResultDto> ReactAsync(
        long commentId,
        string userId,
        ReactionType reactionType)
    {
        var comment = await _context.Comments
            .FirstOrDefaultAsync(c => c.CommentId == commentId);

        if (comment == null || comment.IsHidden)
        {
            return ReactionActionResultDto.Fail("Comment not found");
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
        await _cacheService.RemoveAsync(TargetType, commentId);

        return ReactionActionResultDto.Success(
            "Comment reaction updated successfully",
            ToSummary(comment)
        );
    }

    public async Task<ReactionActionResultDto> RemoveAsync(long commentId, string userId)
    {
        var comment = await _context.Comments
            .FirstOrDefaultAsync(c => c.CommentId == commentId);

        if (comment == null || comment.IsHidden)
        {
            return ReactionActionResultDto.Fail("Comment not found");
        }

        var reaction = await _context.CommentReactions
            .FirstOrDefaultAsync(r => r.CommentId == commentId && r.UserId == userId);

        if (reaction == null)
        {
            return ReactionActionResultDto.Fail("Reaction does not exist");
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
        await _cacheService.RemoveAsync(TargetType, commentId);

        return ReactionActionResultDto.Success(
            "Comment reaction removed successfully",
            ToSummary(comment)
        );
    }

    public async Task<ReactionSummaryDto?> GetSummaryAsync(long commentId)
    {
        var cachedSummary = await _cacheService.GetAsync(TargetType, commentId);

        if (cachedSummary != null)
        {
            return cachedSummary;
        }

        var comment = await _context.Comments
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CommentId == commentId);

        if (comment == null || comment.IsHidden)
        {
            return null;
        }

        var summary = ToSummary(comment);

        await _cacheService.SetAsync(summary);

        return summary;
    }

    private static ReactionSummaryDto ToSummary(Comment comment)
    {
        return new ReactionSummaryDto
        {
            TargetType = TargetType,
            TargetId = comment.CommentId,
            likesCount = comment.LikeCount,
            dislikesCount = comment.DislikeCount
        };
    }
}