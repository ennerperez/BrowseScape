using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using BrowseScape.Core.Interfaces;
using BrowseScape.Core.Types;
using Microsoft.Extensions.Logging;

namespace BrowseScape.Core.Natives.MacOS.Services
{
  public class DefaultBrowserService : IDefaultBrowserService
  {
    private readonly INotificationManager _notificationManager;
    private readonly ILogger<DefaultBrowserService> _logger;

    public DefaultBrowserService(INotificationManager notificationManager, ILogger<DefaultBrowserService> logger)
    {
      _notificationManager = notificationManager;
      _logger = logger;
    }

    private RegisterStatus GetRegisterStatus()
    {
      return RegisterStatus.Unregistered;
    }
    public Task RegisterAsync()
    {
      _logger.LogInformation("Registering...");
      
      /*RegistryKey? appReg = Registry.CurrentUser.CreateSubKey(AppKey);
      RegisterCapabilities(appReg);
      _registerKey?.SetValue(_appID, CapabilityKey);
      HandleUrls();*/

      OpenSettings();
      
      _logger.LogInformation($"Done. Please set {Metadata.Product} as the default browser in Settings.");
      _notificationManager.Show(new Notification("Registered as a browser.", $"Please set {Metadata.Product} as the default browser in Settings."));
      return Task.CompletedTask;
    }
    
    private static void OpenSettings() => Process.Start(new ProcessStartInfo
    {
      FileName = "x-apple.systempreferences:com.apple.Desktop-Settings.extension",
      UseShellExecute = true
    });
    public Task UnregisterAsync()
    {
      throw new System.NotImplementedException();
    }
    public async Task RegisterOrUnregisterAsync()
    {
      var status = GetRegisterStatus();

      if (status == RegisterStatus.Unregistered)
      {
        await RegisterAsync();
        return;
      }

      if (status == RegisterStatus.Registered)
      {
        await UnregisterAsync();
        return;
      }

      if (status == RegisterStatus.Updated)
      {
        await UnregisterAsync();// Unregister the old path
        await RegisterAsync();// Register with the new path
        _notificationManager.Show(new Notification("Updated location", $"{Metadata.Product} has been re-registered with a new path."));
      }
    }
  }
}
