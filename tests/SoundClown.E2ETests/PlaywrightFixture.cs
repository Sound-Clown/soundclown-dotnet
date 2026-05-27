using Microsoft.Playwright;

namespace SoundClown.E2ETests;

/// <summary>
/// Shared Playwright fixture — one browser instance reused across all E2E tests
/// to avoid the ~1s startup cost per test. Each test gets its own isolated
/// BrowserContext (cookies/storage are not shared).
/// </summary>
public class PlaywrightFixture : IAsyncLifetime
{
    public static readonly string BaseUrl =
        Environment.GetEnvironmentVariable("E2E_BASE_URL") ?? "http://localhost:5000";

    public static readonly string ScreenshotDir = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Screenshots"));

    public IPlaywright Playwright { get; private set; } = null!;
    public IBrowser Browser { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Directory.CreateDirectory(ScreenshotDir);
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
            Args = new[] { "--no-sandbox" }
        });
    }

    public async Task DisposeAsync()
    {
        await Browser.DisposeAsync();
        Playwright.Dispose();
    }
}

[CollectionDefinition("Playwright")]
public class PlaywrightCollection : ICollectionFixture<PlaywrightFixture> { }
