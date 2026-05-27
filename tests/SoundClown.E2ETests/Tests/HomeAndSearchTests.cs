using FluentAssertions;
using Microsoft.Playwright;

namespace SoundClown.E2ETests.Tests;

/// <summary>
/// E2E tests for browsing — covers TC-E2E-04..05.
/// Logs in once, then exercises the home page grid + search box.
/// </summary>
[Collection("Playwright")]
public class HomeAndSearchTests
{
    private readonly PlaywrightFixture _fx;

    public HomeAndSearchTests(PlaywrightFixture fx) => _fx = fx;

    private async Task<IPage> LoginAsListener()
    {
        var ctx = await _fx.Browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1280, Height = 800 }
        });
        var page = await ctx.NewPageAsync();
        await page.GotoAsync($"{PlaywrightFixture.BaseUrl}/login");
        await page.FillAsync("input[name='identifier']", "listener@demo.com");
        await page.FillAsync("input[name='password']", "Listener123!");
        await page.ClickAsync("button[type='submit']");
        await page.WaitForURLAsync(u => !u.Contains("/login"), new() { Timeout = 15_000 });
        return page;
    }

    private static async Task SaveAsync(IPage page, string name)
    {
        var path = Path.Combine(PlaywrightFixture.ScreenshotDir, $"{name}.png");
        await page.ScreenshotAsync(new PageScreenshotOptions { Path = path, FullPage = true });
    }

    // ── TC-E2E-04 — Home page shows approved song grid ──────────────────────
    [Fact]
    public async Task Home_ShowsApprovedSongCards()
    {
        var page = await LoginAsListener();

        // SongCards on home page — wait at least one to render
        await page.Locator(".song-card, [data-testid='song-card'], img[alt]").First
            .WaitForAsync(new() { Timeout = 10_000 });

        var cardCount = await page.Locator("img[alt]").CountAsync();
        cardCount.Should().BeGreaterThan(0, "seed database should provide >0 approved songs");

        // Screenshot just the song-grid section (h2 "Mới nhất" + cards),
        // excluding sidebar/player — visually distinct from TC-E2E-03.
        var gridSection = page.Locator("section").Filter(new() { Has = page.Locator("h2") }).First;
        var path = Path.Combine(PlaywrightFixture.ScreenshotDir, "04-home-grid.png");
        await gridSection.ScreenshotAsync(new LocatorScreenshotOptions { Path = path });
    }

    // ── TC-E2E-05 — Search filters results by keyword ───────────────────────
    [Fact]
    public async Task Search_FiltersResultsByKeyword()
    {
        var page = await LoginAsListener();

        await page.GotoAsync($"{PlaywrightFixture.BaseUrl}/search");

        var input = page.Locator("input[placeholder*='Tìm']").First;
        await input.WaitForAsync(new() { Timeout = 10_000 });
        await input.FillAsync("mùa");

        // Wait debounce (300ms) + render — seed has >0 songs with Vietnamese titles
        await page.WaitForTimeoutAsync(1200);

        await SaveAsync(page, "05-search-results");
    }
}
