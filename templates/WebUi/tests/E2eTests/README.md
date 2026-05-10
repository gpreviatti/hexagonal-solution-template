# E2E Tests with Playwright

This folder contains end-to-end (E2E) tests for the Blazor WebApp using Playwright.NET and xUnit.

## Overview

The E2E tests cover core user flows:
1. **Page Load** - Navigate to home page and verify summary + orders table load
2. **Order Summary** - Verify summary displays correct total orders and revenue
3. **Orders Table** - Verify orders table displays with expected columns and data
4. **Expand Items** - Verify ability to expand order items and view nested items table

## Architecture

- **Pages/** - Page Object Models (e.g., `HomePage.cs`) that encapsulate selectors and interactions
- **Fixtures/** - Test fixtures managing browser lifecycle (`BrowserFixture.cs`)
- **Tests/** - Test classes with test scenarios (`HomePageTests.cs`)
- **Config/** - Configuration loader (`E2EConfig.cs`) for environment-specific settings
- **Utilities/** - Reusable Playwright extensions (`PlaywrightExtensions.cs`)

## Prerequisites

- .NET 10.0 SDK
- MockApi running on `http://localhost:5001` (or configured via `MOCKAPI_URL`)

## Running Tests Locally

### Quick Start

1. **Terminal 1** - Start WebApp:
   ```bash
   dotnet watch run --project src/WebApp/WebApp.csproj
   ```

2. **Terminal 2** - Start MockApi:
   ```bash
   dotnet watch run --project src/MockApi/MockApi.csproj
   ```

3. **Terminal 3** - Run E2E tests:
   ```bash
   dotnet test tests/E2eTests/E2eTests.csproj -v normal
   ```

### Run Specific Test

```bash
dotnet test tests/E2eTests/E2eTests.csproj --filter "GivenHomePageWhenNavigatingThenShouldLoadSummaryAndOrdersTable"
```

### Run with Verbose Output

```bash
dotnet test tests/E2eTests/E2eTests.csproj -v detailed
```

## Running Tests in Docker

### Prerequisites

- Docker & Docker Compose installed
- Services defined in `docker-compose-local.yml`

### Steps

1. Start services:
   ```bash
   docker-compose -f docker-compose-local.yml up --build
   ```

2. Wait for services to be healthy (check logs for readiness)

3. Run tests with Docker environment:
   ```bash
   export TEST_ENVIRONMENT=docker
   dotnet test tests/E2eTests/E2eTests.csproj -v normal
   ```

Or pass as inline environment variable:
```bash
TEST_ENVIRONMENT=docker dotnet test tests/E2eTests/E2eTests.csproj
```

## Configuration

Configuration is loaded from `appsettings.json` and can be overridden via environment variables:

| Environment Variable | Purpose | Default (Local) | Default (Docker) |
|---|---|---|---|
| `WEBAPP_URL` | WebApp base URL | `http://localhost:5000` | `http://webapp:5000` |
| `MOCKAPI_URL` | MockApi base URL | `http://localhost:5001` | `http://mockapi:5001` |
| `TEST_ENVIRONMENT` | Environment profile (local\|docker) | `local` | — |
| `NAVIGATION_TIMEOUT_MS` | Page navigation timeout | 30000 | — |
| `WAIT_FOR_SELECTOR_TIMEOUT_MS` | Selector wait timeout | 10000 | — |
| `API_CALL_TIMEOUT_MS` | API call timeout | 15000 | — |

### Example: Override WebApp URL

```bash
WEBAPP_URL=http://staging.example.com dotnet test tests/E2eTests/E2eTests.csproj
```

## Test Structure

All tests in `Tests/HomePageTests.cs` follow this pattern:

```csharp
[Fact]
public async Task GivenContextWhenActionThenShouldResult()
{
    // Arrange
    Assert.NotNull(_homePage);

    // Act
    await _homePage.NavigateAsync();

    // Assert
    var result = await _homePage.GetSummaryTotalOrdersAsync();
    Assert.NotEmpty(result);
}
```

### Page Object Model Pattern

The `HomePage.cs` class encapsulates:
- **Selectors** - CSS/data-testid selectors as private constants
- **Methods** - Actions like `NavigateAsync()`, `WaitForSummaryAsync()`, etc.
- **Assertions** - Verification methods like `IsOrdersTableVisibleAsync()`

This reduces test code duplication and centralizes selector management.

## Data Dependencies

Tests rely on the MockApi to generate random order/item data:
- `GET /orders` - Returns 1-5 random orders
- `GET /orders/summary` - Returns random summary data (total count, revenue)

**Important**: Tests do not mock the API; they test real integration with MockApi.

## Troubleshooting

### Tests Hang or Timeout

**Symptom**: Test execution takes >30s or hangs indefinitely

**Causes**:
- WebApp/MockApi not running on expected ports
- Network connectivity issues
- Services responding slowly

**Fix**:
1. Verify services are running: `curl http://localhost:5000` and `curl http://localhost:5001`
2. Increase timeout: `NAVIGATION_TIMEOUT_MS=60000 dotnet test tests/E2eTests/`
3. Check service logs for errors

### Playwright Browser Not Found

**Symptom**: `PlaywrightException: Chromium executable not found`

**Fix** - Install browser binaries:
```bash
pwsh bin/Debug/net10.0/playwright.ps1 install-deps
```

Or on Linux:
```bash
bash bin/Debug/net10.0/playwright.sh install-deps
```

### Tests Fail with "Element Not Found"

**Symptom**: `PlaywrightException: Timeout 10000ms exceeded waiting for locator`

**Causes**:
- Home page HTML structure changed (selectors need update)
- data-testid attributes missing from HTML
- Page load incomplete before assertion

**Fix**:
1. Verify data-testid attributes exist in `src/WebApp/Components/Pages/Home.razor`
2. Increase `WaitForSelectorTimeoutMs` in `appsettings.json`
3. Update selectors in `Pages/HomePage.cs` if HTML structure changed

## CI/CD Integration

To integrate E2E tests into your CI/CD pipeline:

1. **Add test job** to GitHub Actions / GitLab CI / Azure Pipelines
2. **Use Docker environment** for consistency:
   ```yaml
   - name: Start services
     run: docker-compose -f docker-compose-local.yml up -d

   - name: Wait for services
     run: sleep 10  # Or use health checks

   - name: Run E2E tests
     env:
       TEST_ENVIRONMENT: docker
     run: dotnet test tests/E2eTests/
   ```

3. **Publish test results** (optional):
   ```bash
   dotnet test tests/E2eTests/ --logger "trx;LogFileName=test-results.trx"
   ```

## Further Enhancements

- **Error Resilience Tests** - Test MockApi failures, graceful fallback UI (deferred)
- **Screenshot/Video Artifacts** - Capture on test failure for debugging
- **Performance Monitoring** - Track page load times, API latency
- **Parallel Test Execution** - xUnit runs tests in parallel by default (safe with current fixture design)
- **Visual Regression Testing** - Compare page screenshots against baselines

## References

- [Playwright.NET Documentation](https://playwright.dev/dotnet/)
- [xUnit Documentation](https://xunit.net/)
- [Page Object Model Pattern](https://playwright.dev/dotnet/docs/pom)
