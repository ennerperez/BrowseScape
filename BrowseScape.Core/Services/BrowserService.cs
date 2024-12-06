using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BrowseScape.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BrowseScape.Core.Services
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

    public Task LaunchAsync(string url)
    {
      try
      {
        var windowTitle = _backend.GetActiveWindowTitle();
        var browser = string.Empty;

        var browsers = new Dictionary<string, string>();
        _configuration.Bind("browsers", browsers);

        var arguments = new Dictionary<string, string>();
        _configuration.Bind("arguments", arguments);

        _logger.LogInformation($"Attempting to launch \"{url}\" for \"{windowTitle}\"");

        var urlPreferences = new Dictionary<string, string>();
        _configuration.Bind("urls", urlPreferences);

        var sourcePreferences = new Dictionary<string, string>();
        _configuration.Bind("sources", sourcePreferences);
        
        var urlMatchKey = urlPreferences.FirstOrDefault(m => new Regex(m.Key.Replace("*", ".*")).Match(url).Success).Key;
        var sourceMatchKey = sourcePreferences.FirstOrDefault(m => new Regex(m.Key.Replace("*", ".*")).Match(windowTitle).Success).Key;

        var value = string.Empty;
        if (!string.IsNullOrWhiteSpace(urlMatchKey))
        {
          value = urlPreferences[urlMatchKey];
        }
        else if (!string.IsNullOrWhiteSpace(sourceMatchKey))
        {
          value = sourcePreferences[sourceMatchKey];
        } 

        /* LAUNCH */

        try
        {

          if (string.IsNullOrWhiteSpace(value)) throw new ApplicationException("Browser cannot be launched without a value.");

          browser = browsers[value];
          arguments.TryGetValue(value, out var argument);

          var path = Environment.ExpandEnvironmentVariables(browser);
          Process.Start(path, $"{argument} \"{url}\"");
        }
        catch (Exception e)
        {
          _logger.LogError(e, $"Failed to launch \"{url}\" for \"{browser}\", {{Message}}", e.Message);
        }

      }
      catch (Exception e)
      {
        _logger.LogError(e, "Failed to launch browser, {Message}", e.Message);
      }
      return Task.CompletedTask;
    }
  }
}
