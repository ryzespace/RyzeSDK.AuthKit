# Architecture Decision Records

This directory stores AuthKit.Server architectural decisions as ADRs. It is not changelog and not rewritten commit history. It is curated set of technical decisions that explains why the system has its current shape and which constraints that imposes on future work.

ADRs are grouped by architecture area. Numbering is global, so decision ID does not depend on the file it lives in. Moving a file to another area does not change its number, and the number should be treated as stable decision identifier.

## How To Read This Collection

Start with the area that matches what you are changing:

- if the change affects key management, token key bindings, or core domain contracts, read the `AuthKit.Core` decisions below
- if it affects the REST or gRPC surface, CLI, configuration, or how the server is composed and hosted, see the Host decisions
- if it affects the plugin system, solutions, or plugin abstractions, see the Plugins decisions

Each ADR contains:

- `Context`:
  the layer and architectural location of the decision
- `Problem`:
  the specific technical tension the decision resolves
- `Decision`:
  the chosen direction and responsibility boundary
- `Rejected`:
  realistic alternatives that were intentionally not selected
- `Consequences`:
  maintenance impact, constraints, side effects, and practical implications for future code

## How To Use ADRs During Changes

ADRs do not replace reading the code, but they reduce the cost of understanding decisions that have already been made. When you change a given area:

1. find the matching area
2. read the 2-4 closest ADRs, not just one
3. check whether the new change extends the current model or actually breaks it
4. if decision is no longer true, add a new ADR instead of silently drifting away from the current direction

This collection is meant to preserve consistency across Core, Host, and Plugins. In AuthKit.Server, much of the cost of change comes from contracts between layers rather than from any single class.

## Category Map

The table below shows the architecture areas and their current scope.

| Area | Scope |
| --- | --- |
| Core | Shared domain model: signing key management, token key bindings, error contracts, and Core interfaces. |
| Host | Application composition and external surface: REST, gRPC, CLI, configuration, and key-material hosting. |
| Plugins | Plugin system: solutions, plugin abstractions, and extension boundaries. |

## Current ADRs

| ID | Title | Area | Status | Date |
|----|-------|------|--------|------|
| [ADR-001](./001-centralize-signing-key-management.md) | Centralize Signing Key Management Through A Core Key Store Abstraction | Core | accepted | 2026-08-26 |
| [ADR-002](./002-encrypt-keystore-at-rest.md) | Encrypt Persisted Keystore Material At Rest Through A Pluggable Encryptor | Core | accepted | 2026-08-26 |
| [ADR-003](./003-signing-key-lifecycle-immutable-transitions.md) | Model Signing Key Lifecycle As Immutable State Transitions | Core | accepted | 2026-08-26 |
| [ADR-004](./004-token-key-bindings-domain.md) | Treat Developer Token To Signing Key Bindings As A Core Domain | Core | accepted | 2026-08-26 |
| [ADR-005](./005-public-keys-via-jwks.md) | Publish Public Keys Through JWKS Exposing Only Non-Revoked Keys | Core | accepted | 2026-08-26 |
| [ADR-006](./006-kid-as-generated-guid.md) | Derive The JWT Key Identifier As A Generated GUID | Core | accepted | 2026-08-26 |
| [ADR-007](./007-default-signing-algorithm-rsa-4096.md) | Default Signing Algorithm Is RSA-4096 With RS256 | Core | accepted | 2026-08-26 |
| [ADR-008](./008-standardized-error-response.md) | Standardize API Errors Through A Core Error Response Contract | Core | accepted | 2026-08-26 |
| [ADR-009](./009-dynamic-plugin-discovery.md) | Discover And Load Plugins Dynamically Through The IAuthKitPlugin Contract | Plugins | accepted | 2026-08-26 |
| [ADR-010](./010-plugin-loading-from-directory.md) | Load Plugins From A Configurable Directory At Startup | Host | accepted | 2026-08-26 |
| [ADR-011](./011-keystore-persisted-as-singleton-marten-document.md) | Persist The Encrypted Keystore As A Singleton Marten Document | Host | accepted | 2026-08-26 |
| [ADR-012](./012-token-key-bindings-persisted-in-marten.md) | Persist Token Key Bindings In Marten | Host | accepted | 2026-08-26 |
| [ADR-013](./013-dual-rest-and-grpc-transport.md) | Expose Both REST And gRPC Transport Surfaces | Host | accepted | 2026-08-26 |
| [ADR-014](./014-error-responses-via-middleware.md) | Render HTTP Errors As RFC 7807 Problem Details Via Middleware | Host | accepted | 2026-08-26 |
| [ADR-015](./015-keycloak-external-jwt-authority.md) | Use Keycloak As The External JWT Authority | Host | accepted | 2026-08-26 |
| [ADR-016](./016-marten-and-wolverine-infrastructure.md) | Use Marten And Wolverine As Host Infrastructure | Host | accepted | 2026-08-26 |

## Relationships Between Areas

The most common architectural flow in AuthKit.Server looks like this:

`Core -> Host -> Plugins`

This is not strict dependency diagram of the project, but it is useful map for reading decisions. In practice:

- `Core` defines what the system considers to be domain data and the cryptographic contracts (keys, bindings, errors)
- `Host` defines how that domain is exposed and composed into a running server (API surfaces, CLI, configuration)
- `Plugins` extend behavior on top of the stable Core and Host contracts

## When To Add A New ADR

A new ADR is worth adding when change:

- shifts responsibility boundaries between layers
- introduces a new data contract or new durable artifact
- changes the execution model of key management, hosting, or plugin loading
- adds a new provider or plugin specific behavior that no longer fits the current model
- replaces an earlier decision with a different conscious trade off

It is usually not worth adding an ADR for:

- an ordinary refactor without an architectural change in direction
- a cosmetic local API change limited to one file or class
- a documentation or test only adjustment that does not change system contracts
