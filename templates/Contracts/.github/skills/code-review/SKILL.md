---
name: code-review
description: 'Contracts code review gate for standards, tests, complexity <= 30, modern C#, and security validation.'
---

# Code Review (Contracts Template)

Use this skill to review changes in the Contracts template and block merge when quality gates fail.

## Goals

Guarantee that proposed changes:
- Follow contract-layer standards in this template.
- Include proper unit tests for all new/changed contracts.
- Keep cyclomatic complexity under **30** per method/function.
- Use latest modern C# patterns (C# 13 style in this repository).
- Introduce no known vulnerabilities or secret exposure.

## Scope (Contracts)

Prioritize review of:
- `src/Contracts/Common`
- `src/Contracts/*`
- `src/Contracts/Protos`
- `tests/UnitTests`

## Required Inputs

Before reviewing, collect:
1. User request / acceptance criteria.
2. Git changes (staged + unstaged).
3. Relevant instruction files (especially .NET/C# rules).
4. Template conventions from `templates/Contracts/Readme.md`.

## Mandatory Review Workflow

1. **Requirement coverage**
   - Confirm requested contract/proto changes are fully implemented.
   - Flag out-of-scope edits.

2. **Contract design compliance**
   - Prefer immutable `sealed record` contracts.
   - Keep contract payloads simple and serialization-safe.
   - Validate proto naming and field numbering consistency.

3. **Testing gate**
   - Every new/changed contract must have unit tests in `tests/UnitTests`.
   - Tests must validate construction, defaults, and expected property mapping.
   - Ensure test names follow project convention (`Given...When...Then...` or repository standard).

4. **Cyclomatic complexity gate (hard fail > 30)**
   - Evaluate changed/new methods in contracts, helpers, and test utilities.
   - If complexity exceeds 30, require decomposition/refactor.

5. **Modern C# gate**
   - Enforce modern C# idioms where applicable:
     - file-scoped namespaces
     - sealed records for immutable contracts
     - collection expressions
     - pattern matching where it improves clarity

6. **Security and vulnerability gate**
   - No hardcoded secrets in contracts/tests.
   - No sensitive data in examples/tests/log output.
   - Check dependency and API usage for known unsafe patterns.

## Review Checklist (all must pass)

- [ ] Requested contract/proto behavior implemented.
- [ ] Tests added/updated for every new or changed contract.
- [ ] Cyclomatic complexity <= 30 in changed methods.
- [ ] Modern C# patterns aligned with repository standards.
- [ ] No vulnerability indicators or secret leaks.
- [ ] Build/tests pass for affected projects.

## Severity Policy

- **Critical**: Security issue, secret leak, or complexity > 30.
- **High**: Missing contract tests, breaking contract change without validation.
- **Medium**: Modern C# standard violations.
- **Low**: Naming/documentation/style improvements.

## Output Format

Return a concise report with:
1. **Decision**: `APPROVED` or `CHANGES_REQUIRED`
2. **Failed gates** with file + line references.
3. **Concrete fixes** for each failed gate.
4. **Positive notes** for well-implemented patterns.

If any hard gate fails, decision must be `CHANGES_REQUIRED`.
