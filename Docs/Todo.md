# Work Tracker

Quick snapshot of what we're working on, what's queued next, and what has been delivered.

## How to Use
- Move items between sections (`In Progress`, `Up Next`, `Done`) at the end of each working session.
- Keep checkboxes in sync with actual status; link to issues or PRs when helpful.
- When closing an item here, log the details in `Docs/ProgressLog.md` for long-term history.

## In Progress
- [ ] Flesh out MVC controllers/views with real application logic tied to `Application` layer use cases.

## Up Next
- [ ] Implement tenant-aware persistence (EF Core + SQL Server) and connect Infrastructure services.
- [ ] Integrate external providers: Zarinpal payments, Kavenegar SMS, email/notification channels.
- [ ] Build onboarding wizard, menu management UI, and ordering flows per user stories.
- [ ] Harden security (OAuth/OIDC, MFA) and add observability (logging, metrics, tracing).

## Done
- [x] Convert presentation layer projects to ASP.NET Core MVC and update documentation.
- [x] Scaffold Clean Architecture solution with Domain, Application, Infrastructure, Presentation, and tests.
- [x] Implement core domain/value objects and sample onboarding command + tests.
- [x] Document architecture vision, solution layout, and repo usage in `Docs/Architecture.md` and `README.md`.
