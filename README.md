<div align="center">

# RyzeSDK.AuthKit

### Developer Authentication & SDK Access Service

[![License](https://img.shields.io/badge/license-MIT%20%2B%20Commons%20Clause-7c3aed?style=flat-square)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com)

</div>

![banners](banners.png)
---

## Overview
RyzeSDK.AuthKit is the **centralized microservice** for handling **developer authentication, SDK token issuance, and access verification** across the RyzeSpace SDK ecosystem.

It ensures that only authorized developers can access SDK methods and provides a secure, auditable token-based authentication mechanism.

## Why AuthKit Matters

In a distributed SDK ecosystem, AuthKit provides:

- **Secure Access** — Issue and validate dev tokens for SDK usage
- **Role Enforcement** — Restrict SDK methods to authorized roles (SDK_Dev)
- **Audit & Traceability** — Track issued tokens and access events
- **Extensibility** — Easy to add policies, limits, or new developer roles

## Token Flow Example

- **Developer registers → DevAccessService** issues a token
- **SDK client** (RyzeSdkClient) stores token
- **SDK methods** attach token to REST/gRPC requests
- **AuthKit** verifies token & role → executes method if authorized

## Token Issuance & SDK Request Flow
```mermaid
sequenceDiagram
    participant Dev as Developer
    participant Keycloak as Keycloak
    participant AuthKit as AuthKit API
    participant SDK as RyzeSdkClient
    participant API as Target API (e.g. Marketplace)

    Dev->>Keycloak: Authenticate via Keycloak (JWT access token)
    Keycloak-->>Dev: Returns access_token (Keycloak JWT)

    Dev->>AuthKit: Request Developer Token<br/>Authorization: Bearer <keycloak_token>
    AuthKit->>AuthKit: Validate Keycloak token<br/>and create DeveloperToken (JWT)
    AuthKit-->>Dev: Returns X-Developer-Token (AuthKit JWT)

    SDK->>API: Request with<br/>Authorization: Bearer <keycloak_token><br/>X-Developer-Token: <authkit_token>
    API->>AuthKit: Validate developer token via REST
    AuthKit->>Keycloak: Validate user session & roles
    AuthKit-->>API: DeveloperToken valid ✅
    API-->>SDK: 200 OK — Operation authorized
```

## External REST vs Internal gRPC Call Flow
```mermaid
flowchart TD
   Dev[Developer] -->|Authenticate| Keycloak[Keycloak JWT]
   Keycloak --> Dev

   Dev -->|Request Dev Token| AuthKit[AuthKit API]
   AuthKit -->|Validate Keycloak token| Keycloak
   AuthKit -->|Return Dev Token| Dev

   Dev -->|Request Service Token| AuthKitService[AuthKit API]
   AuthKitService -->|Validate Dev Token| AuthKit
   AuthKitService -->|Return Service Token| Dev

   Dev -->|Configure SDK| SDK[RyzeSdkClient]

   SDK -->|REST request| API_REST[Target API REST]
   SDK -->|gRPC request| API_GRPC[Target API gRPC]

   subgraph REST_Flow
      API_REST -->|Pass tokens to middleware| AuthKit_REST[AuthKit Middleware REST]
      AuthKit_REST -->|Validate Dev & Service Tokens| TokenDB[Token Database]
      AuthKit_REST -->|Validate Keycloak JWT| Keycloak
      AuthKit_REST -->|Return auth result| API_REST
      API_REST -->|200 OK / 403 Forbidden| SDK
   end

   subgraph gRPC_Flow
      API_GRPC -->|Pass tokens to middleware| AuthKit_GRPC[AuthKit Middleware gRPC]
      AuthKit_GRPC -->|Validate Dev & Service Tokens| TokenDB
      AuthKit_GRPC -->|Validate Keycloak JWT| Keycloak
      AuthKit_GRPC -->|Return auth result| API_GRPC
      API_GRPC -->|200 OK / 403 Forbidden| SDK
   end
```

## SDK Function Call Flow (AuthKit → gRPC Microservices)
```mermaid
flowchart TD
   classDef token fill:#fef3c7,stroke:#f59e0b,stroke-width:1px,color:#b45309;
   classDef service fill:#fef3c7,stroke:#fef3c7,stroke-width:1px,color:#92400e;
   classDef internal fill:#dbeafe,stroke:#3b82f6,stroke-width:1px,color:#1e40af;

%% Actors
   Dev[Developer] -->|Has Keycloak JWT,<br/>DeveloperToken,<br/>ServiceToken| SDK[RyzeSdkClient]
   class Dev,SDK token;

%% REST request from external SDK
   SDK -->|REST request with tokens| API_Controller[API Controller]
   API_Controller -->|Validate tokens internally via AuthKit| AuthKit[AuthKit API]

%% gRPC requests
   API_Controller -->|gRPC call| GRPC_Service[gRPC Service]
   GRPC_Service -->|gRPC call to other microservice| AnotherService_GRPC[Another gRPC Service]

%% Responses
   GRPC_Service -->|Return result| API_Controller
   AnotherService_GRPC -->|Return result| GRPC_Service
   API_Controller -->|Return response| SDK

%% Styling
   class Dev,SDK,AuthKit token;
   class API_Controller,GRPC_Service service;
   class AnotherService_GRPC internal;
```

## Contributing

We welcome contributions that improve contract clarity, expand integration patterns, or enhance type safety.

### Development Guidelines

1. **Branch Strategy** — Create feature branches from `main`
   ```bash
      git checkout -b feature/token-policies
   ```
2. **Versioning** — Use semantic versioning (MAJOR.MINOR.PATCH)
    - MAJOR: Breaking changes
    - MINOR: New contracts (backward compatible)
    - PATCH: Bug fixes, documentation

3. **Pull Request** — Provide clear descriptions of changes and impact

## License

**MIT License + Commons Clause**

The RyzeSpace.Contracts library is open source for personal, educational, and research purposes. Commercial use requires explicit permission.

### Permitted Use ✓

- Personal projects and learning
- Academic research and education
- Open source contributions
- Non-commercial experimentation

### Restricted Use ✗

- Commercial products and services
- SaaS platform offerings
- Software resale or licensing
- Consulting services without approval

See [LICENSE](LICENSE) for complete terms.

---

<div align="center">

### Part of the RyzeSpace Ecosystem

**Democratizing access to computational resources through decentralized sharing**

*Every idle GPU, every spare CPU cycle — unlocking potential in the RyzeSpace network*

<br/>

**[Documentation](https://docs.ryzespace.com)** • **[Platform](https://ryzespace.com)** • **[Community](https://discord.gg/JsQx8cQ5yp)**

<br/>

<sub>Built with precision by the RyzeSpace team</sub>

</div>