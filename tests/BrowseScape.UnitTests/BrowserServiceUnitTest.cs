using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BrowseScape.Core.Interfaces;
using BrowseScape.UnitTests.Fixtures;
using Xunit;
using Xunit.Abstractions;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace BrowseScape.UnitTests
{
  public class BrowserServiceUnitTest : TestBed<TestProjectFixture>
  {
    private readonly IBrowserService _browserService;

    public BrowserServiceUnitTest(ITestOutputHelper testOutputHelper, TestProjectFixture fixture) : base(testOutputHelper, fixture)
    {
      _browserService = _fixture.GetService<IBrowserService>(testOutputHelper);
    }

    private void CloseBrowser(string browser)
    {
      var items = Process.GetProcessesByName(browser);
      foreach (var process in items)
        process.Kill();
    }

    [Theory]
    [InlineData("https://maps.google.com", "chromium")]
    [InlineData("https://youtube.com", "opera")]
    public async Task OpenBrowserByUrl(string url, string browser = "")
    {
      CloseBrowser(browser);
      var result = await _browserService.LaunchAsync(url);
      Thread.Sleep(1000);
      var process = Process.GetProcessById(result);
      if (process == null) process = Process.GetProcessesByName(browser).First();
      Assert.True(result != 0 && process != null);
      process.Kill();
    }
    
    [Theory]
    [InlineData("https://twitter.com", "opera", "Microsoft Teams")]
    [InlineData("https://amazon.com", "msedge", "Outlook")]
    public async Task OpenBrowserBySource(string url, string browser = "", string source = "")
    {
      CloseBrowser(browser);
      var result = await _browserService.LaunchAsync(url, source);
      Thread.Sleep(1000);
      var process = Process.GetProcessById(result);
      if (process == null) process = Process.GetProcessesByName(browser).First();
      Assert.True(result != 0 && process != null);
      process.Kill();
    }
  }
}
