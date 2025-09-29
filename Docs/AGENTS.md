# AGENTS

Shared guide for humans and automation working on the EazyMenu platform (`eazymenu.ir`). Keep this document updated as the team grows.

## Primary Roles

| Agent | Responsibility | Key Touchpoints |
|-------|----------------|------------------|
| Product Owner (you) | Define requirements, review deliverables, prioritize backlog. | PRD, User Stories, `Docs/Todo.md`, UI/UX feedback. |
| Automation Assistant (GitHub Copilot) | Pair-programming support: scaffolding, refactors, documentation, tests, CI hygiene. | `Docs/ProgressLog.md`, `Docs/copilot-instructions.md`, pull requests. |
| Backend Engineer (future) | Domain modeling, application services, persistence & integrations. | `src/Domain`, `src/Application`, `src/Infrastructure`, database migrations. |
| Frontend Engineer (future) | MVC UI, responsive design, component library, accessibility. | `src/Presentation/Web`, `src/Presentation/Public`, Tailwind/Bootstrap assets. |
| DevOps Engineer (future) | CI/CD, infrastructure, observability, security compliance. | GitHub Actions, deployment manifests, monitoring dashboards. |

## Collaboration Norms

- **Source of truth:** User stories + PRD drive scope. Align tasks in `Docs/Todo.md` before coding.
- **Change cadence:** Each session updates `Docs/ProgressLog.md` (what changed, tests run) and keeps the Todo board accurate.
- **Communication:** Summaries stay lightweight but actionable (goal, key edits, validation, next steps).
- **Quality gates:** Prefer automated tests + `dotnet build`/`dotnet test` after significant code changes. Document any intentional skips.
- **Escalation:** Blocked on missing details? Note the assumption in the log + Todo and surface it to the product owner.

## How to Join / Update

1. Review `Docs/copilot-instructions.md` to follow repo-specific conventions.
2. Add your name/role in the table above.
3. Capture relevant rituals (standups, code review expectations) if the workflow evolves.
