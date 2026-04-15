---
name: integration-tests-generator
description: 'Generate integration tests for HTTP, gRPC, and messaging flows in this hexagonal template. Use when creating end-to-end API, consumer, or repository integration scenarios with WebApplicationFactory and real infrastructure dependencies.'
---

# Integration Tests Generator — Hexagonal Architecture Template

Generate robust integration tests that validate behavior across layers (WebApp, Application, Infrastructure), not just isolated unit behavior.

## When to Use

Activate this skill when:
- User asks for integration tests (HTTP, gRPC, messaging)
- User wants to validate endpoint + persistence + serialization together
- User requests `WebApplicationFactory`-based testing
- User asks to validate consumer + producer workflows
- User asks for repository integration checks against configured DB

## Skill-local References

| File | Purpose |
|------|---------|
| `./references/http-test-template.md` | HTTP integration test scaffold |
| `./references/grpc-test-template.md` | gRPC integration test scaffold |
| `./references/messaging-test-template.md` | Messaging integration test scaffold |
| `./references/run-commands.md` | Integration test run commands |

> Keep this skill self-contained by using local references from this folder.

---

## Project Conventions

- Use `[Collection("WebApplicationFactoryCollectionDefinition")]` for shared app factory lifecycle
- Use `CustomWebApplicationFactory<Program>` for integration setup
- Use `BaseHttpFixture` for HTTP tests
- Use `ApiGrpcHelper` for gRPC channel creation
- Use `BaseMessagingFixture` for producer/consumer integration flow
- Keep test naming style: `GivenContext_WhenCondition_ThenExpectedResult`

### HTTP-specific conventions

- Place tests under `tests/IntegrationTests/WebApp/Http/{Context}/`
- Use fixture classes inheriting from `BaseHttpFixture`
- Set helper and endpoint route in constructor (`_fixture.SetApiHelper(...)`, `_fixture.ResourceUrl = ...`)
- For endpoints that require headers (e.g., `CorrelationId`), call `_fixture.ApiHelper.AddHeaders(...)`
- Use `ApiHelper.DeSerializeResponse<T>(result)` to assert typed response payloads
- Cover both success and failure scenarios (status code + payload semantics)
- For update/delete scenarios, create prerequisite data in fixture helper methods when needed

### gRPC-specific conventions

- Place tests under `tests/IntegrationTests/WebApp/Grpc/{Context}/`
- Inherit from `CommonTests.Fixtures.BaseFixture` for shared helpers
- Inject `CustomWebApplicationFactory<Program>` via constructor
- Build `ApiGrpcHelper` with `customWebApplicationFactory.CreateClient()`
- Create a `GrpcChannel` via `ApiGrpcHelper.AsGrpcClientChannel()`
- Instantiate the generated client (for current proto: `GrpcOrder.OrderService.OrderServiceClient`)
- Cover both success and failure scenarios (e.g., existing and non-existing IDs)
- Assert payload shape on success (`Success`, empty `Message`, non-null `Data`, `Items` checks)
- Assert error semantics on failure (`Success == false`, non-empty `Message`, `Data == null`)

### Messaging-specific conventions

- Place tests under `tests/IntegrationTests/WebApp/Messaging/{Context}/`
- Use fixture classes inheriting from `BaseMessagingFixture`
- Resolve services via `_fixture.SetServices(factory)`
- Publish messages via `_fixture.HandleProducerAsync(message, queueName)`
- Verify side effects through repository queries (e.g., `Repository.GetQueryable<TEntity>(...)`)
- Include deduplication/idempotency scenario when consumer behavior supports it (publish same message twice and assert single persisted record)
- Keep assertions deterministic by filtering on message-identifying fields

---

## Test Type Selection

- **HTTP tests**: endpoint contract, status code, JSON content type, body payload
- **gRPC tests**: proto contract, generated client call, success + failure response semantics, payload mapping
- **Messaging tests**: producer invocation, queue routing, eventual side effects

---

## Output Expectations

When used, this skill should generate:
- New test class in the proper IntegrationTests namespace/folder
- Fixture usage aligned with existing helpers
- At least one positive and one failure-path assertion per scenario
- Deterministic assertions (avoid flaky sleeps unless consumer lag requires delay)
- HTTP tests following current Orders style (`BaseHttpFixture`, status code + deserialized payload assertions)
- gRPC tests following current `GetOrderGrpcTest` structure (client setup in constructor, request/response assertions)
- Messaging tests following current `CreateNotificationTest` structure (produce message, query repository, assert side effects and deduplication)

---

## Checklist

- [ ] Correct folder used: `tests/IntegrationTests/WebApp/{Http|Grpc|Messaging}/...`
- [ ] `CustomWebApplicationFactory<Program>` injected via collection fixture
- [ ] Correlation ID included where required by API contract
- [ ] Assertions validate status and payload shape
- [ ] HTTP tests include both valid and invalid request scenarios
- [ ] HTTP tests assert both HTTP status and deserialized response semantics
- [ ] gRPC tests include both valid and invalid request scenarios
- [ ] gRPC assertions verify `Success`, `Message`, and `Data` consistency
- [ ] Messaging tests verify persisted side effects through repository query
- [ ] Messaging tests cover deduplication/idempotency when applicable
- [ ] Commands documented/validated using `./references/run-commands.md`
