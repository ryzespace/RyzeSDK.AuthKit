namespace Core;

/// <summary>
/// Represents the base exception type for domain specific errors.
/// </summary>
/// <remarks>
/// Domain exceptions describe business rule violations or other errors
/// originating from the application domain.
/// </remarks>
public abstract class DomainException(string message) : Exception(message);