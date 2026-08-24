# Security Policy

AuthKit provides developer authentication, SDK token issuance, token validation, and access enforcement. Because these components directly participate in authentication and authorization, security issues affecting them may have a significant impact on applications integrating AuthKit.

Token issuance, storage, validation, session handling, and authorization logic are considered security-sensitive components.

> [!IMPORTANT]
> **Do not publicly disclose security vulnerabilities or exploit details.**
> Please report security issues privately so they can be investigated and addressed before public disclosure.

> [!WARNING]
> AuthKit is under active development. The security model and implementation are still being hardened, and breaking changes may be introduced as security improvements are made.

## Supported Versions

AuthKit is currently in active development.

Security fixes are applied to the `main` branch first. Where appropriate, security fixes may also be backported to supported stable releases.

Because AuthKit is under active development, users should generally run the latest available version or commit when possible.

## Reporting a Vulnerability

Please report suspected security vulnerabilities directly to the maintainers through a private communication channel.

A useful report should include:

* a clear description of the vulnerability
* affected component or functionality
* steps required to reproduce the issue
* security impact and potential attack scenario
* affected version, release, or commit
* proof of concept, if available
* any known prerequisites or limitations

Please avoid including credentials, private keys, real access tokens, personal data, or other sensitive information in the report.

### Public Issues

If no private reporting channel is currently available, open a minimal public issue **without exploit details or a working proof of concept**.

The issue should only indicate that a potential security vulnerability exists and request private follow-up from the maintainers.

## Response Process

The maintainers aim to:

1. acknowledge receipt of a security report within **7 days**
2. reproduce and validate the reported issue
3. assess its severity and potential impact
4. determine affected versions and components
5. prepare and implement an appropriate fix
6. verify the fix through testing and security review where appropriate
7. coordinate disclosure after a fix or mitigation is available

Response times may vary depending on the complexity and severity of the issue.

## Scope

This security policy covers vulnerabilities affecting AuthKit components, including:

* AuthKit REST API
* AuthKit gRPC API
* developer authentication
* SDK token issuance
* token validation and verification
* token storage and handling
* token revocation and lifecycle management
* authorization and access enforcement
* role and permission checks
* Keycloak integration
* session handling
* authentication and authorization middleware
* security-sensitive configuration and key management

Issues in dependencies may also be considered when AuthKit's integration or configuration introduces or materially contributes to the vulnerability.

## Out of Scope

The following are generally considered out of scope unless they result in a demonstrable security impact:

* formatting, style, or documentation issues
* theoretical vulnerabilities without a reproducible attack path
* vulnerabilities requiring unrealistic or unavailable assumptions
* issues in unsupported environments
* denial-of-service caused solely by intentionally exhausting resources available to the attacker
* reports that only describe best-practice improvements without demonstrating a security impact

Out of scope issues may still be considered at the maintainers' discretion.

## Severity

Security reports are evaluated based on factors such as:

* whether authentication can be bypassed
* whether authorization can be bypassed
* whether tokens can be forged, stolen, or replayed
* whether sensitive credentials or cryptographic material can be exposed
* the privileges required to exploit the issue
* whether user or system interaction is required
* the potential confidentiality, integrity, and availability impact

The maintainers may use established severity frameworks such as **CVSS** when appropriate.

## Disclosure

Please allow the maintainers reasonable time to investigate, reproduce, and remediate a reported vulnerability before publicly disclosing technical details.

After a fix or effective mitigation is available, the maintainers may coordinate public disclosure with the reporter.

Disclosure timing may depend on:

* severity and exploitability
* availability of a fix or mitigation
* affected versions
* whether the vulnerability is already being actively exploited
* coordination with affected users or downstream projects

The maintainers reserve the right to delay disclosure when immediate publication could materially increase risk to users.

## Security Updates

Security fixes may be included in regular releases or published as dedicated security releases when appropriate.

Users are encouraged to keep AuthKit and its security-sensitive dependencies up to date.
