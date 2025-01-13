using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using BrowseScape.Core.Interfaces;
using BrowseScape.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BrowseScape.Core.Services
{
  public class BrowserService : IBrowserService
  {
    private readonly IBackend _backend;
    private readonly INotificationManager _notificationManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BrowserService> _logger;
    public BrowserService(INotificationManager notificationManager, IConfiguration configuration, IBackend backend, ILogger<BrowserService> logger)
    {
      _notificationManager = notificationManager;
      _configuration = configuration;
      _backend = backend;
      _logger = logger;
    }

    public Task<int> LaunchAsync(string url, string windowTitle = "")
    {
      try
      {
        if (string.IsNullOrEmpty(windowTitle))
        {
          windowTitle = _backend.GetActiveWindowTitle();
        }
        IBrowser? browser = null;

        var browsers = new Dictionary<string, Browser>();
        _configuration.Bind("browsers", browsers);

        _logger.LogInformation($"Attempting to launch \"{url}\" for \"{windowTitle}\"");

        var urlPreferences = new Dictionary<string, string>();
        _configuration.Bind("urls", urlPreferences);

        var sourcePreferences = new Dictionary<string, string>();
        _configuration.Bind("sources", sourcePreferences);
        
        var typesPreferences = new Dictionary<string, string>();
        _configuration.Bind("types", typesPreferences);

        var urlMatchKey = urlPreferences.FirstOrDefault(m => new Regex(m.Key.Replace("*", ".*")).Match(url).Success).Key;
        var sourceMatchKey = sourcePreferences.FirstOrDefault(m => new Regex(m.Key.Replace("*", ".*")).Match(windowTitle).Success).Key;
        var typeMatchKey = typesPreferences.FirstOrDefault(m => url.EndsWith(m.Key.Replace("*.", "."))).Key;

        var value = _configuration["general:default"] ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(urlMatchKey))
        {
          value = urlPreferences[urlMatchKey];
        }
        else if (!string.IsNullOrWhiteSpace(sourceMatchKey))
        {
          value = sourcePreferences[sourceMatchKey];
        }
        else if (!string.IsNullOrWhiteSpace(typeMatchKey))
        {
          value = typesPreferences[typeMatchKey];
        }

        /* LAUNCH */

        try
        {

          if (string.IsNullOrWhiteSpace(value)) throw new ApplicationException("Browser cannot be launched without a value.");

          browser = browsers[value];

          if (string.IsNullOrWhiteSpace(browser.Path)) throw new ApplicationException("Browser path cannot be launched without a value.");
          var process = Process.Start(Environment.ExpandEnvironmentVariables(browser.Path), $"{browser.Args} \"{url}\"");
          _notificationManager.Show(new Notification("Browser launched.", $"{browser.Name} was launched."));
          return Task.FromResult(process.Id);
        }
        catch (Exception e)
        {
          _logger.LogError(e, $"Failed to launch \"{url}\" for \"{browser?.Name}\", {{Message}}", e.Message);
        }

      }
      catch (Exception e)
      {
        _logger.LogError(e, "Failed to launch browser, {Message}", e.Message);
        return Task.FromResult(0);
      }
      return Task.FromResult(0);
    }
  }
}
