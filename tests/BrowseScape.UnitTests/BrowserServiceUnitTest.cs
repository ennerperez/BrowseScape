using System.Threading.Tasks;
using BrowseScape.Core.Interfaces;
using BrowseScape.UnitTests.Fixtures;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace BrowseScape.UnitTests
{
  public class BrowserServiceUnitTest : TestBed<TestProjectFixture>
  {
    private readonly IBackend _backend;
    private readonly IBrowserService _browserService;

    public BrowserServiceUnitTest(ITestOutputHelper testOutputHelper, TestProjectFixture fixture) : base(testOutputHelper, fixture)
    {
      _backend = _fixture.GetService<IBackend>(testOutputHelper);
      _browserService = _fixture.GetService<IBrowserService>(testOutputHelper);
    }

    [Theory]
    [InlineData("https://www.google.com")]
    public async Task Launch(string url)
    {
      var result = await _browserService.LaunchAsync(url);
      Assert.True(result);
    }
  }
}
