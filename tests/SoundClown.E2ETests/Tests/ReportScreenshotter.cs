using Microsoft.Playwright;

namespace SoundClown.E2ETests.Tests;

/// <summary>
/// Utility "test" that screenshots the coverage report for inclusion in the
/// project documentation. Opt-in via env var SCREENSHOT_COVERAGE=1 — skipped
/// in normal CI runs.
/// </summary>
[Collection("Playwright")]
public class ReportScreenshotter
{
    private readonly PlaywrightFixture _fx;
    public ReportScreenshotter(PlaywrightFixture fx) => _fx = fx;

    [Fact]
    public async Task ScreenshotCoverageReport()
    {
        if (Environment.GetEnvironmentVariable("SCREENSHOT_COVERAGE") != "1")
            return; // skip unless explicitly requested

        var repoRoot = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        var indexHtml = Path.Combine(repoRoot, "docs", "coverage-report", "index.html");

        if (!File.Exists(indexHtml))
            throw new FileNotFoundException("Coverage report not found", indexHtml);

        var ctx = await _fx.Browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1280, Height = 900 }
        });
        var page = await ctx.NewPageAsync();
        await page.GotoAsync($"file://{indexHtml}");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var outPath = Path.Combine(PlaywrightFixture.ScreenshotDir, "06-coverage-report.png");
        await page.ScreenshotAsync(new PageScreenshotOptions { Path = outPath, FullPage = true });
    }
}
