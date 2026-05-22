---
name: code-review
description: 'Full template code review gate for DDD/SOLID standards, tests, complexity <= 30, modern C#, and security validation.'
---

# Code Review (Full Template)

Use this skill to perform end-to-end review for the Full Hexagonal template and block merge when quality gates fail.

## Goals

Guarantee that proposed changes:
- Follow DDD, SOLID, and Hexagonal architecture standards.
- Include proper tests for all new/changed behavior.
- Keep cyclomatic complexity under **30** per method/function.
- Use latest modern C# patterns (C# 13 style in this repository).
- Introduce no known vulnerabilities or secret exposure.

## Scope (Full)

Prioritize review of:
- `src/Domain` (entities, invariants, domain behavior)
- `src/Application` (use cases, DTOs, validators, ports)
- `src/Infrastructure` (EF, cache, messaging, telemetry adapters)
- `src/WebApp` (minimal APIs, composition, middleware)
- `tests/UnitTests`, `tests/IntegrationTests`, `tests/LoadTests`

## Required Inputs

Before reviewing, collect:
1. User request / acceptance criteria.
2. Git changes (staged + unstaged).
3. Relevant instruction files (especially .NET/C# rules).
4. Template conventions from `templates/Full/Readme.md`.

## Mandatory Review Workflow

1. **Requirement coverage**
   - Confirm all requested features were implemented.
   - Flag unrelated modifications.

2. **DDD/SOLID architecture gate**
   - Domain stays pure (no infrastructure concerns).
   - Application orchestrates via ports/interfaces.
   - Infrastructure implements adapters only.
   - WebApp composes and exposes endpoints only.
   - Validate SRP and proper abstraction boundaries.

3. **Testing gate**
   - Domain/application changes must include or update unit tests.
   - API/integration behavior changes must include integration tests.
   - Throughput-critical endpoint changes should update/load-test scripts when relevant.
   - Ensure tests are deterministic and meaningful.

4. **Cyclomatic complexity gate (hard fail > 30)**
   - Evaluate changed/new methods across all layers.
   - If any exceeds 30, require refactor (split logic, extract strategies, simplify branching).

5. **Modern C# gate**
   - Enforce current repository language style:
     - file-scoped namespaces
     - sealed records/classes where appropriate
     - collection expressions
     - async/await for I/O paths
     - pattern matching / switch expressions for clarity

6. **Security and vulnerability gate**
   - No hardcoded credentials/secrets.
   - Input validation at boundaries.
   - Safe exception/logging (no PII/secrets).
   - Flag risky dependency or insecure coding patterns.

## Review Checklist (all must pass)

- [ ] Requested behavior implemented and complete.
- [ ] DDD/SOLID and layer boundaries respected.
- [ ] New/changed logic covered with appropriate tests.
- [ ] Cyclomatic complexity <= 30 for each changed method.
- [ ] Modern C# patterns aligned with repository standards.
- [ ] No obvious security vulnerabilities or secret exposure.
- [ ] Build/tests pass for affected projects.

## Severity Policy

- **Critical**: Security issue, secret leak, or complexity > 30.
- **High**: Missing tests for new behavior, DDD/layer boundary violation.
- **Medium**: Modern C# standard violations or maintainability issues.
- **Low**: Naming/documentation/style improvements.

## Output Format

Return a concise report with:
1. **Decision**: `APPROVED` or `CHANGES_REQUIRED`
2. **Failed gates** with file + line references.
3. **Concrete fixes** for each failed gate.
4. **Positive notes** for good patterns observed.

If any hard gate fails, decision must be `CHANGES_REQUIRED`.
