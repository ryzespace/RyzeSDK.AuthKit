namespace Domain.Features.DeveloperTokens;

/// <summary>
/// Exception thrown when a developer exceeds the allowed number of tokens.
/// </summary>
/// <param name="developerId">The unique identifier of the developer who exceeded the limit.</param>
/// <param name="limit">The maximum number of tokens allowed per developer.</param>
public class DeveloperTokenLimitExceededException(Guid developerId, int limit)
    : DomainException($"Developer '{developerId}' cannot have more than {limit} tokens.");