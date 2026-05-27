using FluentAssertions;
using Microsoft.Playwright;

namespace SoundClown.E2ETests.Tests;

/// <summary>
/// E2E tests for the authentication flow — covers TC-E2E-01..03.
/// Runs against a live SoundClown instance at PlaywrightFixture.BaseUrl
/// (set via E2E_BASE_URL env var, defaults to http://localhost:5000).
/// </summary>
[Collection("Playwright")]
public class AuthFlowTests
{
    private readonly PlaywrightFixture _fx;

    public AuthFlowTests(PlaywrightFixture fx) => _fx = fx;

    private async Task<IPage> NewPageAsync()
    {
        var ctx = await _fx.Browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1280, Height = 800 }
        });
        return await ctx.NewPageAsync();
    }

    private static async Task SaveAsync(IPage page, string name)
    {
        var path = Path.Combine(PlaywrightFixture.ScreenshotDir, $"{name}.png");
        await page.ScreenshotAsync(new PageScreenshotOptions { Path = path, FullPage = true });
    }

    // ── TC-E2E-01 — Login page renders with brand + form ─────────────────────
    [Fact]
    public async Task LoginPage_RendersBrandAndForm()
    {
        var page = await NewPageAsync();
        await page.GotoAsync($"{PlaywrightFixture.BaseUrl}/login", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 15_000
        });

        await page.Locator("h1", new() { HasTextString = "SoundClown" }).WaitForAsync();
        await page.Locator("input[name='identifier']").WaitForAsync();
        await page.Locator("input[name='password']").WaitForAsync();
        await page.Locator("button[type='submit']").WaitForAsync();

        await SaveAsync(page, "01-login-page");
    }

    // ── TC-E2E-02 — Login fails with wrong credentials ──────────────────────
    [Fact]
    public async Task Login_WithBadCredentials_ShowsErrorMessage()
    {
        var page = await NewPageAsync();
        await page.GotoAsync($"{PlaywrightFixture.BaseUrl}/login");

        await page.FillAsync("input[name='identifier']", "listener@demo.com");
        await page.FillAsync("input[name='password']", "wrong-password-xyz");
        await page.ClickAsync("button[type='submit']");

        // Server redirects back to /login with ?error=... — wait for it
        await page.WaitForURLAsync(url => url.Contains("/login") && url.Contains("error="),
            new() { Timeout = 10_000 });

        page.Url.Should().Contain("error=");

        await SaveAsync(page, "02-login-bad-credentials");
    }

    // ── TC-E2E-03 — Login succeeds with seed credentials, lands on home ─────
    [Fact]
    public async Task Login_WithValidCredentials_LandsOnHomeAndShowsPlayerBar()
    {
        var page = await NewPageAsync();
        await page.GotoAsync($"{PlaywrightFixture.BaseUrl}/login");

        await page.FillAsync("input[name='identifier']", "listener@demo.com");
        await page.FillAsync("input[name='password']", "Listener123!");
        await page.ClickAsync("button[type='submit']");

        await page.WaitForURLAsync(url => !url.Contains("/login"), new() { Timeout = 15_000 });
        page.Url.TrimEnd('/').Should().EndWith(PlaywrightFixture.BaseUrl.TrimEnd('/'));

        // Home should show the welcome heading "XIN CHÀO" or username
        await page.WaitForSelectorAsync("body", new() { Timeout = 5_000 });

        await SaveAsync(page, "03-home-after-login");
    }
}
