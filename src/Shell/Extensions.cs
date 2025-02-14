using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using BrowseScape.Core.Interfaces;
using BrowseScape.Core.Natives.Windows.Interop;
using BrowseScape.Shell.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BrowseScape.Shell
{
  public static class Extensions
  {
    public static IServiceCollection AddShell(this IServiceCollection serviceCollection, AppBuilder? builder = null)
    {
      serviceCollection.AddSingleton<INotificationManager, WindowNotificationManager>();
      serviceCollection.AddSingleton<INotificationService, NotificationService>();
      return serviceCollection;
    }

    public static void SetupApp(this AppBuilder builder)
    {
      if (OperatingSystem.IsLinux())
      {
        builder.With(new X11PlatformOptions { EnableIme = true, });
      }
      else if (OperatingSystem.IsMacOS())
      {
        builder.With(new MacOSPlatformOptions { DisableDefaultApplicationMenuItems = true });
      }
      else if (OperatingSystem.IsWindows())
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
  }
}
