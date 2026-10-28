# AGENTS.md

Shared rules for AI coding agents working in the Contracts template, the shared contracts layer of a hexagonal solution. Project overview, layout, build commands, and reference examples are in [Readme.md](Readme.md). Each project has its own `AGENTS.md` with the rules specific to it, listed below.

## Projects

| Project | Path | Rules |
|---|---|---|
| Contracts | `src/Contracts/` | [AGENTS.md](src/Contracts/AGENTS.md) |
| Unit tests | `tests/UnitTests/` | [AGENTS.md](tests/UnitTests/AGENTS.md) |

## Rules

- Put package versions in `Directory.Packages.props`, never in `.csproj` files.
- `TreatWarningsAsErrors` is enabled. Any warning fails the build.

## Skills

Use these before writing code:

| Skill | Use when |
|---|---|
| [contract-generator](.github/skills/contract-generator/SKILL.md) | Creating request, response, DTO, or proto contracts |
| [code-review](.github/skills/code-review/SKILL.md) | Reviewing changes before merge |
