namespace Domain.Exceptions;

public class KeycloakOperationException : Exception
{
    public KeycloakOperationException(string message) : base(message) { }
    
    public KeycloakOperationException(string message, Exception innerException) 
        : base(message, innerException) { }
}