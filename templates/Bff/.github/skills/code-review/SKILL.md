---
name: code-review
description: 'BFF code review gate for standards, tests, complexity <= 30, modern C#, and security validation.'
---

# Code Review (BFF Template)

Use this skill to review changes in the BFF template and block merge when quality gates fail.

## Goals

Guarantee that proposed changes:
- Follow this template standards and architecture (Ports and Adapters / Hexagonal).
- Include proper tests for new/changed behavior.
- Keep cyclomatic complexity under **30** per method/function.
- Use latest modern C# patterns (C# 13 style in this repository).
- Introduce no known security vulnerabilities or secret exposure.

## Scope (BFF)

Prioritize review of:
- `src/WebApp` (minimal APIs, middleware, endpoint registration)
- `src/Infrastructure` (HTTP/gRPC adapters, cache, resilience, telemetry)
- `src/Contracts` (DTO/proto contracts used by BFF)
- `tests/IntegrationTests`, `tests/CommonTests`, `tests/LoadTests`

## Required Inputs

Before reviewing, collect:
1. User request / acceptance criteria.
2. Git changes (staged + unstaged).
3. Relevant instruction files (especially .NET/C# rules).
4. Template conventions from `templates/Bff/Readme.md`.

## Mandatory Review Workflow

1. **Requirement coverage**
   - Confirm all requested behavior is implemented.
   - Flag out-of-scope changes.

2. **Architecture compliance**
   - Keep business contracts in `src/Contracts`.
   - Keep adapters in `src/Infrastructure`.
   - Keep inbound API composition in `src/WebApp`.
   - No leakage of external concerns into contracts.

3. **Testing gate**
   - Every new endpoint/behavior must have tests.
   - Prefer integration tests for API behavior (`tests/IntegrationTests`).
   - Update common test helpers when needed (`tests/CommonTests`).
   - For performance-sensitive endpoint changes, validate or update k6 scripts (`tests/LoadTests`).

4. **Cyclomatic complexity gate (hard fail > 30)**
   - Evaluate changed methods and new methods.
   - If any method exceeds 30, request refactor (extract methods, strategy pattern, guard clauses).

5. **Modern C# gate**
   - Require current language idioms used by template:
     - file-scoped namespaces
     - records for immutable contracts
     - collection expressions where appropriate
     - async/await for I/O
     - pattern matching / switch expressions where helpful
   - Flag legacy patterns when a modern equivalent improves maintainability.

6. **Security and vulnerability gate**
   - Check for hardcoded secrets, tokens, connection strings with passwords.
   - Verify input validation and safe error handling.
   - Flag vulnerable dependency usage or unsafe API patterns.
   - Ensure logs do not expose sensitive data.

## Review Checklist (all must pass)

- [ ] Requested behavior implemented completely.
- [ ] No unrelated or risky side effects.
- [ ] New/changed logic has adequate automated tests.
- [ ] Cyclomatic complexity <= 30 for each changed method.
- [ ] Modern C# patterns aligned with repository standards.
- [ ] No obvious security vulnerabilities or secret leaks.
- [ ] Build/tests pass for affected projects.

## Severity Policy

- **Critical**: Security issue, secret leak, or complexity > 30 in core flow.
- **High**: Missing tests for new behavior, architecture violation.
- **Medium**: Modern C# standard violations, maintainability concerns.
- **Low**: Naming/documentation/style improvements.

## Output Format

Return a concise report with:
1. **Decision**: `APPROVED` or `CHANGES_REQUIRED`
2. **Failed gates** with exact file + line references.
3. **Concrete fixes** for each failed gate.
4. **Positive notes** for well-implemented patterns.

If any hard gate fails, decision must be `CHANGES_REQUIRED`.
