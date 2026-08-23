namespace AuthKit.Plugins.Abstractions;

/// <summary>
/// Specifies the transport location from which an API key should be retrieved.
/// </summary>
/// <remarks>
/// <para>
/// The API key can be supplied through different metadata locations depending
/// on the transport used by the incoming request, such as HTTP or gRPC.
/// </para>
/// <para>
/// For HTTP requests, API keys may be provided through request headers, query
/// string parameters, or cookies. For gRPC requests, API keys are typically
/// provided through request metadata, which is represented by
/// <see cref="Header"/>.
/// </para>
/// <para>
/// The selected location determines where the AuthKit authentication pipeline
/// searches for the API key before attempting validation.
/// </para>
/// </remarks>
public enum AuthKitApiKeyLocation
{
    /// <summary>
    /// Retrieves the API key from request headers or transport metadata.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For HTTP requests, the API key is retrieved from configured request
    /// header, such as <c>X-Api-Key</c>.
    /// </para>
    /// <para>
    /// For gRPC requests, the API key is retrieved from the request metadata.
    /// </para>
    /// </remarks>
    Header,

    /// <summary>
    /// Retrieves the API key from request query parameter.
    /// </summary>
    /// <remarks>
    /// The API key is expected to be supplied as part of the HTTP request URL,
    /// for example <c>?api_key=...</c>.
    /// </remarks>
    /// <para>
    /// This location is only applicable to HTTP-based requests and is not
    /// available for native gRPC calls.
    /// </para>
    Query,

    /// <summary>
    /// Retrieves the API key from a request cookie.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The API key is expected to be stored in an HTTP cookie sent with the
    /// incoming request.
    /// </para>
    /// <para>
    /// This location is primarily intended for HTTP browser-based scenarios
    /// and is not available for native gRPC calls.
    /// </para>
    /// </remarks>
    Cookie
}
