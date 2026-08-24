# Contributing to AuthKit

Thank you for contributing to AuthKit.

Small, focused changes are easier to review, test, and maintain. Contributions should aim to improve the project without introducing unnecessary complexity or unrelated changes.

Please read and follow the [Code of Conduct](CODE_OF_CONDUCT.md) before contributing.

## Before You Start

Before opening an issue or pull request:

* check existing issues and pull requests
* make sure the change is not already being worked on
* keep the change focused on one problem or feature
* explain why a behavioral change is needed when it is not self-evident
* avoid unrelated refactoring or cleanup

For larger changes, it is recommended to open an issue first to discuss the proposed approach.

## Local Setup

AuthKit uses the standard .NET toolchain.

Build the solution with:

```bash
dotnet build AuthKit.slnx
```

Run the test suite with:

```bash
dotnet test AuthKit.slnx
```

If your change affects token issuance, validation, authentication, authorization, persistence, or integration behavior, make sure the relevant tests are run and the affected flow is verified locally.

## Branches

Create branches from `main`.

Use short, descriptive branch names that indicate the purpose of the change.

Examples:

```bash
git checkout -b fix/auth-store
git checkout -b feat/token-rotation
git checkout -b refactor/security-middleware
```

Avoid mixing unrelated changes in the same branch.

## Code Style

Follow the existing conventions in the surrounding code.

In particular:

* keep the style already used in the file
* prefer small, incremental changes over broad rewrites
* match existing naming and project structure
* keep APIs and abstractions as simple as possible
* introduce abstractions only when they solve a real problem
* avoid speculative features or infrastructure
* preserve existing behavior unless the change explicitly requires otherwise

When adding new security-sensitive behavior, favor explicit and auditable code over unnecessary abstraction.

## Tests

Changes should include appropriate tests when practical.

Tests are especially important for changes involving:

* authentication
* authorization
* token issuance
* token validation
* token storage
* token expiration or revocation
* cryptographic operations
* Keycloak integration
* middleware and access enforcement

Bug fixes should preferably include regression test demonstrating the original problem.

## Pull Requests

A pull request should clearly describe:

* what changed
* why it changed
* how it was tested
* any relevant design decisions
* linked issues or discussions, if applicable

For behavioral or security-sensitive changes, explain the relevant flow and any assumptions that reviewers should be aware of.

Keep pull requests small and focused whenever possible. Smaller changes are easier to review, test, and merge.

Pull requests may be requested to include additional tests, documentation, or design changes before they are merged.

## Security Sensitive Changes

Do not disclose security vulnerabilities through normal issues or pull requests.

If you discover potential security vulnerability in AuthKit, follow the process described in [SECURITY.md](SECURITY.md).

Do not commit:

* passwords
* API keys
* access tokens
* private keys
* credentials
* production configuration containing secrets
* other sensitive authentication material

Use local configuration or environment variables for development secrets.

## Reporting Issues

For normal bugs, open a GitHub issue and include:

* what you expected to happen
* what happened instead
* steps to reproduce the issue
* relevant logs or error messages
* your operating system and .NET version when relevant
* the affected AuthKit version or commit when known

Please remove secrets, credentials, tokens, and other sensitive information before posting logs or configuration.

**Do not use public issues to report security vulnerabilities.** See [SECURITY.md](SECURITY.md) instead.

## Documentation

Changes that modify public APIs, configuration, authentication flows, or user facing behavior should include corresponding documentation updates when appropriate.

Documentation should remain consistent with the actual implementation.

## License

By contributing to AuthKit, you agree that your contribution will be licensed under the project's [MIT License + Commons Clause](../LICENSE).

Please make sure you have the right to submit the contribution under these terms.
