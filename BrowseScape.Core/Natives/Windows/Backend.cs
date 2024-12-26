using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using BrowseScape.Core.Interfaces;
using BrowseScape.Core.Natives.Windows.Interop;
using BrowseScape.Core.Types;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Bitmap=Avalonia.Media.Imaging.Bitmap;

namespace BrowseScape.Core.Natives.Windows
{

  [SupportedOSPlatform("windows")]
  public class Backend : IBackend
  {

    private readonly INotificationManager _notificationManager;
    private readonly ILogger<Backend> _logger;

    public Backend(INotificationManager notificationManager, ILogger<Backend> logger)
    {
      _notificationManager = notificationManager;
      _logger = logger;
    }

    private static void FixWindowFrameOnWin10(Window w)
    {
      switch (w.WindowState)
      {
        case WindowState.Maximized:
        case WindowState.FullScreen:
          w.SystemDecorations = SystemDecorations.Full;
          break;
        case WindowState.Normal:
          w.SystemDecorations = SystemDecorations.BorderOnly;
          break;
      }
    }
    public static void SetupApp(AppBuilder builder)
    {
      // Fix drop shadow issue on Windows 10
      var v = new Ntdll.RTL_OSVERSIONINFOEX { dwOSVersionInfoSize = (uint)Marshal.SizeOf<Ntdll.RTL_OSVERSIONINFOEX>() };
      if (Ntdll.RtlGetVersion(ref v) != 0 || (v.dwMajorVersion >= 10 && v.dwBuildNumber >= 22000))
      {
        return;
      }
      Window.WindowStateProperty.Changed.AddClassHandler<Window>((w, _) => FixWindowFrameOnWin10(w));
      Control.LoadedEvent.AddClassHandler<Window>((w, _) => FixWindowFrameOnWin10(w));
    }
    public string GetActiveWindowTitle()
    {
      var result = string.Empty;
      const int NChars = 256;
      var buff = new StringBuilder(NChars);
      var handle = User32.GetForegroundWindow();

      if (User32.GetWindowText(handle, buff, NChars) > 0)
      {
        result = buff.ToString();
      }
      return result;
    }
    public void OpenSettings() => Process.Start(new ProcessStartInfo { FileName = $"ms-settings:defaultapps?registeredAppUser={Metadata.Name}", UseShellExecute = true });

    private string AppOpenUrlCommand => Metadata.Assembly.Replace(".dll", ".exe") + " %1";
    private string AppKey => $"SOFTWARE\\{Metadata.Name}";
    private string UrlKey => $"SOFTWARE\\Classes\\{Metadata.Name}URL";
    private string CapabilityKey => $"SOFTWARE\\{Metadata.Name}\\Capabilities";

    private readonly RegistryKey _registerKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\RegisteredApplications", true);
    private RegistryKey AppRegKey => Registry.CurrentUser.OpenSubKey(AppKey);
    private RegistryKey UrlRegKey => Registry.CurrentUser.OpenSubKey(UrlKey);


    private RegisterStatus GetRegisterStatus()
    {
      return RegisterStatus.Unregistered;
    }
    public Task RegisterAsync()
    {
      _logger.LogInformation("Registering...");

      var appReg = Registry.CurrentUser.CreateSubKey(AppKey);
      RegisterCapabilities(appReg);

      _registerKey?.SetValue(Metadata.Name, CapabilityKey);

      HandleUrls();

      OpenSettings();

      _logger.LogInformation($"Please set {Metadata.Name} as the default browser in Settings.");
      _notificationManager.Show(new Notification("Registered as a browser.", $"Please set {Metadata.Name} as the default browser in Settings."));
      return Task.CompletedTask;
    }

    private void RegisterCapabilities(RegistryKey appReg)
    {
      // Register capabilities.
      var capabilityReg = appReg.CreateSubKey("Capabilities");
      if (capabilityReg == null)
      {
        return;
      }
      capabilityReg.SetValue("ApplicationName", Metadata.Name);
      capabilityReg.SetValue("ApplicationIcon", $"{Metadata.Assembly.Replace(".dll", ".exe")},0");
      capabilityReg.SetValue("ApplicationDescription", Metadata.Description);

      // Set up protocols we want to handle.
      var urlAssocReg = capabilityReg.CreateSubKey("URLAssociations");
      if (urlAssocReg == null)
      {
        return;
      }
      urlAssocReg.SetValue("http", Metadata.Name + "URL");
      urlAssocReg.SetValue("https", Metadata.Name + "URL");
      urlAssocReg.SetValue("ftp", Metadata.Name + "URL");
      urlAssocReg.SetValue("ftps", Metadata.Name + "URL");
    }

    private void HandleUrls()
    {
      var handlerReg = Registry.CurrentUser.CreateSubKey(UrlKey);
      if (handlerReg == null)
      {
        return;
      }
      handlerReg.SetValue(string.Empty, Metadata.Name);
      handlerReg.SetValue("FriendlyTypeName", Metadata.Name);
      handlerReg.CreateSubKey("shell\\open\\command")?.SetValue("", AppOpenUrlCommand);
    }
    public Task UnregisterAsync()
    {
      throw new NotImplementedException();
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
      path = Environment.ExpandEnvironmentVariables(path);
        return null;
      
    }
  }
}
