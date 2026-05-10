using Microsoft.Extensions.Configuration;

namespace E2eTests.Common;

public sealed class Configurations
{
    private readonly IConfiguration _configuration;
    public int NavigationTimeoutMs { get; private set; }
    public int WaitForSelectorTimeoutMs { get; private set; }
    public int APICallTimeoutMs { get; private set; }

    public Configurations()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables();

        _configuration = builder.Build();

        NavigationTimeoutMs = int.TryParse(
            Environment.GetEnvironmentVariable("NAVIGATION_TIMEOUT_MS"),
            System.Globalization.NumberStyles.Integer,
            System.Globalization.CultureInfo.InvariantCulture,
            out var navTimeout)
            ? navTimeout
            : int.Parse(_configuration["Timeouts:NavigationTimeoutMs"] ?? "30000", System.Globalization.CultureInfo.InvariantCulture);

        WaitForSelectorTimeoutMs = int.TryParse(
            Environment.GetEnvironmentVariable("WAIT_FOR_SELECTOR_TIMEOUT_MS"),
            System.Globalization.NumberStyles.Integer,
            System.Globalization.CultureInfo.InvariantCulture,
            out var waitTimeout)
            ? waitTimeout
            : int.Parse(_configuration["Timeouts:WaitForSelectorTimeoutMs"] ?? "10000", System.Globalization.CultureInfo.InvariantCulture);

        APICallTimeoutMs = int.TryParse(
            Environment.GetEnvironmentVariable("API_CALL_TIMEOUT_MS"),
            System.Globalization.NumberStyles.Integer,
            System.Globalization.CultureInfo.InvariantCulture,
            out var apiTimeout)
            ? apiTimeout
            : int.Parse(_configuration["Timeouts:APICallTimeoutMs"] ?? "15000", System.Globalization.CultureInfo.InvariantCulture);
    }
}
