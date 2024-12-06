using System.Threading.Tasks;
using BrowseScape.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BrowseScape.Core.Natives.MacOS.Services
{
  public class BrowserService : IBrowserService
  {
    private readonly IBackend _backend;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BrowserService> _logger;
    public BrowserService(IConfiguration configuration, IBackend backend, ILogger<BrowserService> logger)
    {
      _configuration = configuration;
      _backend = backend;
      _logger = logger;
    }
    public async Task LaunchAsync(string url, string windowTitle)
    {
    }
  }
}
