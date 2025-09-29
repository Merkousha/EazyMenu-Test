# EazyMenu Platform

EazyMenu (`eazymenu.ir`) is a multi-tenant SaaS platform that helps restaurants and cafés launch digital menus, ordering, and reservation experiences without bespoke development. This repository hosts the Clean Architecture solution that powers the product roadmap described in the PRD and user stories.

## Solution Structure
```
EazyMenu.sln
├── src
│   ├── Domain                // Core business rules, aggregates, value objects
│   ├── Application           // Use cases, command handlers, application services
│   ├── Infrastructure        // Persistence, integrations (Zarinpal, Kavenegar, QR, storage)
│   └── Presentation          // ASP.NET Core MVC front-ends (Web admin + Public ordering)
└── tests
    ├── EazyMenu.UnitTests        // Domain and application unit coverage
    └── EazyMenu.IntegrationTests // Cross-layer scenarios and adapters
```

Key layers reference each other in one direction to keep dependencies under control:
- `Presentation` depends on `Application` + `Infrastructure` for orchestrating workflows.
- `Infrastructure` implements ports defined in `Application` and reuses `Domain` models.
- `Application` coordinates domain logic and exposes commands/queries.
- `Domain` holds the enterprise models with no framework dependencies.

## Clean Architecture Building Blocks
- **Tenant provisioning** flow is stubbed via `RegisterTenantCommand` and an in-memory provisioning service, ready to be replaced with SQL Server + messaging.
- **Value objects** such as `TenantId` and `Money` encapsulate critical invariants (identities, currency handling).
- **Presentation** projects are bootstrapped ASP.NET Core MVC apps configured to consume `AddApplicationServices()` and `AddInfrastructureServices()`.
- **Tests** verify foundational behaviors and provide executable documentation for the onboarding workflow.

## Getting Started
```powershell
# Restore, build, and test the full solution
cd d:\Git\EazyMenu
 dotnet build
 dotnet test
```

> ℹ️ The solution currently uses the .NET 9 preview SDK. Ensure a compatible SDK is installed before building.

## Next Steps
- Implement real persistence (EF Core with SQL Server) and tenant-aware schemas.
- Integrate external services: Zarinpal payments, Kavenegar SMS, email/notification channels.
- Expand the onboarding wizard, menu management UI, and public ordering flow according to the PRD.
- Harden security (OAuth/OIDC, MFA) and add observability (logging, metrics, tracing).
