# CLAUDE.md

Behavioral guidelines to reduce common LLM coding mistakes. Merge with project-specific instructions as needed.

**Tradeoff:** These guidelines bias toward caution over speed. For trivial tasks, use judgment.

## 1. Think Before Coding

**Don't assume. Don't hide confusion. Surface tradeoffs.**

Before implementing:
- State your assumptions explicitly. If uncertain, ask.
- If multiple interpretations exist, present them - don't pick silently.
- If a simpler approach exists, say so. Push back when warranted.
- If something is unclear, stop. Name what's confusing. Ask.

## 2. Simplicity First

**Minimum code that solves the problem. Nothing speculative.**

- No features beyond what was asked.
- No abstractions for single-use code.
- No "flexibility" or "configurability" that wasn't requested.
- No error handling for impossible scenarios.
- If you write 200 lines and it could be 50, rewrite it.

Ask yourself: "Would a senior engineer say this is overcomplicated?" If yes, simplify.

## 3. Surgical Changes

**Touch only what you must. Clean up only your own mess.**

When editing existing code:
- Don't "improve" adjacent code, comments, or formatting.
- Don't refactor things that aren't broken.
- Match existing style, even if you'd do it differently.
- If you notice unrelated dead code, mention it - don't delete it.

When your changes create orphans:
- Remove imports/variables/functions that YOUR changes made unused.
- Don't remove pre-existing dead code unless asked.

The test: Every changed line should trace directly to the user's request.

## 4. Goal-Driven Execution

**Define success criteria. Loop until verified.**

Transform tasks into verifiable goals:
- "Add validation" → "Write tests for invalid inputs, then make them pass"
- "Fix the bug" → "Write a test that reproduces it, then make it pass"
- "Refactor X" → "Ensure tests pass before and after"

For multi-step tasks, state a brief plan:
```
1. [Step] → verify: [check]
2. [Step] → verify: [check]
3. [Step] → verify: [check]
```

Strong success criteria let you loop independently. Weak criteria ("make it work") require constant clarification.

---

**These guidelines are working if:** fewer unnecessary changes in diffs, fewer rewrites due to overcomplication, and clarifying questions come before implementation rather than after mistakes.

---

## Tech stack boundaries

Do not introduce alternatives to the choices below unless the user explicitly asks.

### Do not use

- **MySQL** or other databases — use **SQLite** only
- **MediatR**, CQRS buses, domain events, event sourcing
- Rich **DDD** (aggregates, domain services, ubiquitous language ceremony)
- Generic repositories, specification frameworks, extra "core"/shared abstraction layers
- Swapping **MudBlazor**, **FastEndpoints**, **Kiota**, or other listed stack components
- Hand-rolled API clients/DTOs that duplicate **Kiota**-generated types

### Backend (`src/backend`)

| Concern | Use |
|--------|-----|
| Runtime | .NET 10 |
| API | FastEndpoints — vertical slices (one folder per use case) |
| Data | Entity Framework Core + SQLite |
| Validation | FluentValidation on request DTOs |
| API docs | Scalar + OpenAPI |
| Errors | Result pattern → Problem Details (RFC 7807) at the HTTP boundary |
| Logging | Serilog (structured) |
| Domain | Anemic — thin entities; logic in handlers/services |

Slice layout: endpoint, request/response, validator, handler per feature (e.g. `Features/<Name>/<Action>/`).

### Frontend (`src/frontend`)

| Concern | Use |
|--------|-----|
| Host | Blazor WebAssembly |
| UI | MudBlazor |
| API client | Kiota from OpenAPI |
| Components | `.razor` + `.razor.cs` code-behind |

OpenAPI is the contract: regenerate the Kiota client when the API surface changes.

**Folder layout** — group by domain, mirroring backend `Features/<Name>/`:

```
src/frontend/
├── Layout/              # App shell (MainLayout, NavMenu)
├── Infrastructure/      # DI, API wiring, cross-cutting client setup
├── TrackrApi/           # Kiota-generated client (do not hand-edit)
└── Features/
    ├── <Name>/          # One folder per domain (Accounts, Categories, …)
    │   ├── <Name>Page.razor (+ .cs)   # Routable pages only (@page)
    │   └── *Dialog.razor (+ .cs)      # Feature-specific dialogs/components
    ├── Home/            # Landing / dashboard
    └── Shared/          # UI used by 2+ features (e.g. NotFound)
```

Conventions:

- Routable pages: `*Page` suffix (e.g. `AccountsPage.razor` with `@page "/accounts"`).
- Create/edit dialogs: `*FormDialog` (handles both modes via parameters).
- Namespace matches folder: `frontend.Features.Accounts`.
- Do not put feature UI in a flat `Pages/` folder or duplicate a top-level `Components/<Name>/` tree — keep each domain self-contained under `Features/<Name>/`.
- Extract to `Features/Shared/` only when at least two features need the same component or helper.
