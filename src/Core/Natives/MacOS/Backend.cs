using System.Diagnostics;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using BrowseScape.Core.Interfaces;
using BrowseScape.Core.Types;
using Microsoft.Extensions.Logging;
using Notification = BrowseScape.Core.Models.Notification;

namespace BrowseScape.Core.Natives.MacOS
{
  // ReSharper disable once InconsistentNaming
  [SupportedOSPlatform("macOS")]
  public class Backend : IBackend
  {

    private readonly INotificationService _notificationService;
    private readonly ILogger<Backend> _logger;

    public Backend(INotificationService notificationService, ILogger<Backend> logger)
    {
      _notificationService = notificationService;
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
      _registerKey?.SetValue(_appID, CapabilityKey);*/
      HandleUrls();

      OpenSettings();

      _logger.LogInformation($"Please set {Metadata.Name} as the default browser in Settings.");
      _notificationService.Show(new Core.Models.Notification("Registered as a browser.", $"Please set {Metadata.Name} as the default browser in Settings."));
      return Task.CompletedTask;
    }

    private void HandleUrls()
    {
    }
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
        _notificationService.Show(new Notification("Updated location", $"{Metadata.Name} has been re-registered with a new path."));
      }
    }
    public byte[] GetAppIcon(string path)
    {
      throw new System.NotImplementedException();
    }
    public string GetActiveWindowTitle()
    {
#if MACOS
      var windowInfo = QuartzCore.CGWindowListCopyWindowInfo(CGWindowListOption.OnScreenOnly, 0);
      var values = (NSArray)Runtime.GetNSObject<NSArray>(windowInfo);

      var windowList = new List<QuartzCore.kCGWindow>();
      for (ulong i = 0, len = values.Count; i < len; i++)
      {
        var window = Runtime.GetNSObject(values.ValueAt(i));
        var item = new QuartzCore.kCGWindow();
        item.Read(window);
        windowList.Add(item);
      }
#endif
      throw new System.NotImplementedException();
    }
    public void OpenSettings() => Process.Start(new ProcessStartInfo { FileName = "x-apple.systempreferences:com.apple.Desktop-Settings.extension", UseShellExecute = true });


  }
}
