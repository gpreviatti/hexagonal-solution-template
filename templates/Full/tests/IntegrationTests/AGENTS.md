# AGENTS.md — IntegrationTests

Rules for AI coding agents working in `tests/IntegrationTests/`. Shared rules (test naming, mutation survival rate) are in [AGENTS.md](../../AGENTS.md). Test strategy is in [Readme.md](../../Readme.md#-testing-strategy).

## Rules

- HTTP and gRPC endpoints need integration tests.
- Use `BaseHttpFixture` for HTTP tests and `BaseMessagingFixture` for messaging tests.
- Annotate HTTP integration tests with `[Collection("WebApplicationFactoryCollectionDefinition")]`.
