namespace Domain;

/// <summary>
/// Base class for all domain-specific exceptions.
/// </summary>
public abstract class DomainException(string message) : Exception(message);