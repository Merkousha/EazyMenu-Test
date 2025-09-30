# Copilot Instructions

Guidelines for AI assistants collaborating on the EazyMenu Clean Architecture solution.

## Coding Standards
- **Language/Framework:** .NET 9, ASP.NET Core MVC, C# 12. Respect nullable reference types and implicit usings.
- **Architecture:** Maintain dependency direction (Presentation → Application → Domain; Infrastructure implements Application contracts). Do not reference Presentation from lower layers.
- **Style:** Prefer explicit namespaces, file-scoped where appropriate. When introducing new types, include XML/summary comments if the behavior is non-trivial.
- **Localization:** Default UI copy is Persian (fa-IR); keep new strings consistent.

## Workflow Expectations
1. **Plan first:** Use the Todo list tool to track tasks. Break down work into actionable todos.
2. **Gather context:** Consult `Docs/PRD.md`, user stories, `Docs/Todo.md`, and `AGENTS.md` before coding.
3. **Implementation:** Use Clean Architecture boundaries; surface new interfaces in Application before adding infrastructure implementations.
4. **Validation:** Run `dotnet build` and `dotnet test` after meaningful code changes. Record results in the session summary.
5. **Documentation:** Update `Docs/ProgressLog.md` after each work session and adjust `Docs/Todo.md` to reflect status changes.

## Testing & Quality
- Add or update unit/integration tests when changing behavior.
- Keep tests deterministic; avoid external service calls without abstractions/mocking.
- Note any failed or skipped checks with rationale and TODOs.

## Version Control & Commits
- Use descriptive commit messages summarizing the change scope (“Switch presentation layer to MVC”, “Introduce Zarinpal payment client stub”).
- Group related file changes together; avoid unrelated formatting noise.

## Safety & Secrets
- Never commit secrets (API keys, connection strings). Use configuration placeholders or environment variables.
- Highlight security-sensitive changes or assumptions in the progress log.

## Communication
- Summaries should include: actions taken, validation steps, requirement coverage, and next steps.
- Call out assumptions or information gaps so the product owner can unblock them.

Keeping these instructions fresh ensures all agents—human or automated—stay aligned as the platform evolves.
