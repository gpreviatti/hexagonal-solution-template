namespace E2eTests.Common;

public sealed class Configurations
{
    public int NavigationTimeoutMs { get; private set; }
    public int WaitForSelectorTimeoutMs { get; private set; }
    public int APICallTimeoutMs { get; private set; }
    public string WebAppUrl { get; set; }

    public Configurations()
    {
        NavigationTimeoutMs = int.TryParse(Environment.GetEnvironmentVariable("NAVIGATION_TIMEOUT_MS"), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var navTimeout) ? navTimeout : 30000;
        WaitForSelectorTimeoutMs = int.TryParse(Environment.GetEnvironmentVariable("WAIT_FOR_SELECTOR_TIMEOUT_MS"), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var waitTimeout) ? waitTimeout: 10000;
        APICallTimeoutMs = int.TryParse(Environment.GetEnvironmentVariable("API_CALL_TIMEOUT_MS"), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var apiTimeout) ? apiTimeout : 15000;
    }
}
