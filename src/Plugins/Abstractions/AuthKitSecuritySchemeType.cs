namespace AuthKit.Plugins.Abstractions;

/// <summary>
/// Specifies the authentication mechanism represented by an AuthKit security scheme.
/// </summary>
/// <remarks>
/// <para>
/// The selected value determines how the corresponding
/// <see cref="AuthKitSecuritySchemeDescriptor"/> describes the credentials
/// and authentication flow exposed by an AuthKit plugin.
/// </para>
/// <para>
/// This enumeration describes the authentication mechanism at the metadata
/// level. It does not itself perform authentication, validate credentials,
/// or establish an authentication session.
/// </para>
/// </remarks>
public enum AuthKitSecuritySchemeType
{
    /// <summary>
    /// Authentication using an API key supplied through a configured location.
    /// </summary>
    /// <remarks>
    /// The credential location is specified by
    /// <see cref="AuthKitSecuritySchemeDescriptor.In"/>.
    /// Common locations include an HTTP header, query parameter, or cookie.
    /// </remarks>
    ApiKey,

    /// <summary>
    /// Authentication using an HTTP authentication scheme.
    /// </summary>
    /// <remarks>
    /// The HTTP authentication scheme is specified by
    /// <see cref="AuthKitSecuritySchemeDescriptor.Scheme"/>.
    /// Examples include <c>Basic</c>, <c>Bearer</c>, and other HTTP
    /// authentication schemes.
    /// </remarks>
    Http,

    /// <summary>
    /// Authentication using the OAuth 2.0 authorization framework.
    /// </summary>
    /// <remarks>
    /// OAuth 2.0 schemes describe authorization flows in which a client
    /// obtains an access token from an authorization server and presents
    /// that token when accessing protected resources.
    /// </remarks>
    OAuth2,

    /// <summary>
    /// Authentication using the OpenID Connect identity layer.
    /// </summary>
    /// <remarks>
    /// OpenID Connect extends OAuth 2.0 with an identity layer and is used
    /// to authenticate users through an OpenID Connect identity provider.
    /// </remarks>
    OpenIdConnect
}
