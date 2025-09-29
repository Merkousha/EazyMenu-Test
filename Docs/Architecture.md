# Clean Architecture Overview for EazyMenu

## Vision
The EazyMenu platform (`eazymenu.ir`) delivers a multi-tenant SaaS service for restaurants and cafés. The solution embraces Clean Architecture to isolate business rules, orchestrate application workflows, and keep infrastructure frameworks replaceable.

## Layered Structure
- **Domain (`src/Domain`)**
  - Aggregates: `Restaurant`, `Tenant`, `Subscription`, `Menu`, `MenuCategory`, `MenuItem`, `Order`, `Reservation`, `PaymentTransaction`, `Notification`.
  - Value Objects: `TenantId`, `Money`, `Email`, `PhoneNumber`, `ScheduleSlot`, `QrCodeReference`.
  - Domain Services & Events for pricing, tenant provisioning, order lifecycle.
  - Repository contracts and interfaces for external services.
- **Application (`src/Application`)**
  - Use cases grouped under feature folders (`Restaurants`, `Menus`, `Orders`, `Reservations`, `Payments`, `Notifications`, `Auth`).
  - CQRS handlers (`Commands`, `Queries`) with MediatR pipeline behaviors (validation, logging, tenant context enforcement).
  - DTOs, validators (FluentValidation), and mapping profiles (Mapster/AutoMapper).
  - Interfaces defining ports to infrastructure (e.g., `IPaymentGateway`, `ISmsGateway`, `IQrCodeGenerator`).
- **Infrastructure (`src/Infrastructure`)**
  - Data access (EF Core, SQL Server) with multi-tenant schemas / filters.
  - Integrations: Zarinpal SDK client, Kavenegar SMS client, blob storage for assets, email provider, QR code generator (QRCoder).
  - Identity providers (OpenIddict / IdentityServer) and background services (Hangfire / Worker Service) for notifications.
  - Persistence migrations and configuration providers.
- **Presentation (`src/Presentation`)**
  - `EazyMenu.Web`: ASP.NET Core MVC application for admin/backoffice and onboarding wizard.
  - `EazyMenu.Public`: ASP.NET Core MVC application serving customer-facing menu, ordering, and reservations.
  - Shared UI components, Tailwind CSS configuration, localization resources.

## Cross-Cutting Concerns
- **Tenant Context** propagated via middleware and MediatR behaviors.
- **Observability**: Serilog for logging, OpenTelemetry exporters, health checks.
- **Security**: OAuth/OpenID Connect, MFA, rate limiting, data encryption.
- **Configuration**: Azure App Configuration / environment-based JSON files.

## Solution Layout
```
EazyMenu.sln
└── src
    ├── Domain
    │   ├── EazyMenu.Domain.csproj
    │   ├── Entities/
    │   ├── ValueObjects/
    │   ├── Events/
    │   └── Abstractions/
    ├── Application
    │   ├── EazyMenu.Application.csproj
    │   ├── Common/
    │   ├── Abstractions/
    │   ├── Features/
    │   │   ├── Restaurants/
    │   │   ├── Menus/
    │   │   ├── Orders/
    │   │   ├── Reservations/
    │   │   ├── Payments/
    │   │   └── Notifications/
    │   └── Behaviors/
    ├── Infrastructure
    │   ├── EazyMenu.Infrastructure.csproj
    │   ├── Persistence/
    │   ├── Identity/
    │   ├── Messaging/
    │   ├── Integrations/
    │   └── Config/
    └── Presentation
    ├── Web
    │   ├── EazyMenu.Web.csproj
    │   ├── Controllers/
    │   ├── Views/
    │   └── wwwroot/
    └── Public
      ├── EazyMenu.Public.csproj
      ├── Controllers/
      ├── Views/
      └── wwwroot/
└── tests
    ├── EazyMenu.UnitTests/
    └── EazyMenu.IntegrationTests/
```

## Feature Traceability
| Requirement | Layer Touchpoints |
|-------------|-------------------|
| ثبت‌نام و اشتراک | Presentation (Onboarding wizard), Application (Tenant provisioning command), Domain (`Subscription` aggregate), Infrastructure (SQL, payment) |
| احراز هویت | Presentation (MFA UI), Application (`Auth` commands), Infrastructure (Identity, SMS OTP) |
| وب‌سایت اختصاصی | Presentation (`Public` site generator), Application (`Websites` feature), Domain (`Restaurant` customization) |
| مدیریت منو | Presentation (menu editor UI), Application (`Menus` feature), Domain (`Menu` aggregate) |
| سفارش آنلاین | Presentation (ordering flow), Application (`Orders` commands/queries), Domain (`Order` aggregate), Infrastructure (payment, notifications) |
| رزرو میز | Presentation (reservation UI), Application (`Reservations` workflows), Domain (`Reservation` aggregate), Infrastructure (SMS/email) |
| پرداخت زرین‌پال | Application (`Payments` use cases), Infrastructure (Zarinpal integration) |
| پیامک کاوه‌نگار | Application (`Notifications` orchestration), Infrastructure (SMS gateway) |
| اعلان‌ها | Application (Notification dispatch), Infrastructure (SignalR/email), Presentation (dashboard toasts) |
| داشبورد تحلیلی | Presentation (charts), Application (`Analytics` queries), Infrastructure (Reporting DB) |

## Next Steps
1. Scaffold solution and projects under `src/` per structure above.
2. Configure dependency references (Presentation -> Application -> Domain, Application -> Domain, Infrastructure -> Application/Domain).
3. Add baseline CI/CD and documentation updates referencing `eazymenu.ir`.
