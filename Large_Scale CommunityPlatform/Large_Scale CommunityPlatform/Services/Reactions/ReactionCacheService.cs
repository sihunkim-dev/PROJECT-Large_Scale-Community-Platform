using System.Text.Json;
using Large_Scale_CommunityPlatform.Models.Dtos.Reaction;
using StackExchange.Redis;

namespace Large_Scale_CommunityPlatform.Services.Reactions;


public class ReactionCacheService
{
    private readonly IDatabase _redis;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    public ReactionCacheService(IConnectionMultiplexer redis)
    {
        _redis = redis.GetDatabase();
    }

    private static string GetKey(string targetType, long targetId)
    {
        return $"reaction-summary:{targetType.ToLower()}:{targetId}";
    }

    public async Task<ReactionSummaryDto?> GetAsync(string targetType, long targetId)
    {
        var value = await _redis.StringGetAsync(GetKey(targetType, targetId));

        if (value.IsNullOrEmpty)
        {
            return null;
        }

        return JsonSerializer.Deserialize<ReactionSummaryDto>(value.ToString());
    }

    public async Task SetAsync(ReactionSummaryDto summary)
    {
        var json = JsonSerializer.Serialize(summary);

        await _redis.StringSetAsync(
            GetKey(summary.TargetType, summary.TargetId),
            json,
            CacheDuration
        );
    }

    public async Task RemoveAsync(string targetType, long targetId)
    {
        await _redis.KeyDeleteAsync(GetKey(targetType, targetId));
    }
}