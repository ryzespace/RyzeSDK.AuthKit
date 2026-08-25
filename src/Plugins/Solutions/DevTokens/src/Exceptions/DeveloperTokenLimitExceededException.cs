using Core;

namespace DevTokens.Exceptions;

/// <summary>
/// Represents domain exception raised when developer exceeds the
/// maximum number of developer tokens allowed.
/// </summary>
/// <param name="developerId">The unique identifier of the developer who exceeded the limit.</param>
/// <param name="limit">The maximum number of tokens allowed per developer.</param>
public class DeveloperTokenLimitExceededException(Guid developerId, int limit)
    : DomainException($"Developer '{developerId}' cannot have more than {limit} tokens.");