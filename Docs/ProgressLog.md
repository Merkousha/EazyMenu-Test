# Progress Log

A running history of significant work completed in this repository.

## How to Use
- Append new entries to the top of the file with the current date so the latest progress stays visible.
- Summarize what was finished, notable commands/tests that ran, and any follow-up actions.
- Reference related tasks in `Docs/Todo.md` when closing items.

## 2025-09-29
- Converted both `EazyMenu.Web` and `EazyMenu.Public` presentation projects from Razor Pages to ASP.NET Core MVC (controllers, views, layouts, updated startup pipelines).
- Updated `Docs/Architecture.md` and `README.md` to reflect the MVC presentation approach.
- Added clean scaffolding artifacts for the admin and public MVC apps (layouts, view imports, placeholder screens).
- Ensured solution builds and automated tests run successfully after the conversion.

## 2025-09-29 (earlier)
- Scaffolded the Clean Architecture solution: Domain, Application, Infrastructure, Presentation (Web/Public), and `tests` projects.
- Implemented initial domain primitives (`TenantId`, `Money`, `Restaurant`, `MenuCategory`, `MenuItem`) and a sample onboarding use case with an in-memory provisioning service.
- Created architecture overview documentation plus README with structure, commands, and domain context (`eazymenu.ir`).
- Replaced template tests with unit/integration tests covering value objects and the onboarding command flow.
- Ran `dotnet build` and `dotnet test` to validate the initial scaffold.
