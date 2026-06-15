namespace Large_Scale_CommunityPlatform.Models.Dtos.Reaction;

public class ReactionActionResultDto
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;

    public ReactionSummaryDto? summary { get; set; }

    public static ReactionActionResultDto Fail(string message)
    {
        return new ReactionActionResultDto
        {
            IsSuccess = false,
            Message = message,
        };
    }

    public static ReactionActionResultDto Success(string message, ReactionSummaryDto summary)
    {
        return new ReactionActionResultDto
        {
            IsSuccess = true,
            Message = message,
            summary =  summary
        };
    }

}