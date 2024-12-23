using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.Notifications;
using Avalonia.Media.Imaging;
using BrowseScape.Core.Interfaces;
using BrowseScape.Core.Types;
using Microsoft.Extensions.Logging;

namespace BrowseScape.Core.Natives.MacOS
{
  // ReSharper disable once InconsistentNaming
  [SupportedOSPlatform("macOS")]
  public class Backend : IBackend
  {

    private readonly INotificationManager _notificationManager;
    private readonly ILogger<Backend> _logger;

    public Backend(INotificationManager notificationManager, ILogger<Backend> logger)
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
      _registerKey?.SetValue(_appID, CapabilityKey);*/
      HandleUrls();

      OpenSettings();

      _logger.LogInformation($"Please set {Metadata.Name} as the default browser in Settings.");
      _notificationManager.Show(new Notification("Registered as a browser.", $"Please set {Metadata.Name} as the default browser in Settings."));
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
        _notificationManager.Show(new Notification("Updated location", $"{Metadata.Name} has been re-registered with a new path."));
      }
    }
    public Bitmap GetAppIcon(string path)
    {
      throw new System.NotImplementedException();
    }
    public void SetupApp(AppBuilder builder)
    {
      builder.With(new MacOSPlatformOptions { DisableDefaultApplicationMenuItems = true, });

      var customPathFile = Path.Combine(IBackend.DataDir, "PATH");
      if (File.Exists(customPathFile))
      {
        IBackend.CustomPathEnv = File.ReadAllText(customPathFile).Trim();
      }
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
