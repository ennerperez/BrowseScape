using System;
using Avalonia;
using BrowseScape.Core.Natives;

namespace BrowseScape.Core.Interfaces
{
  public interface IBackend
  {
    void SetupApp(AppBuilder builder);

    public static string DataDir { get; private set; } = string.Empty;
    public static string CustomPathEnv { get; set; } = string.Empty;

    public static IBackend GetBackend()
    {
      if (OperatingSystem.IsWindows())
      {
        return new Windows();
      }
      if (OperatingSystem.IsMacOS())
      {
        return new MacOS();
      }
      if (OperatingSystem.IsLinux())
      {
        return new Linux();
      }

      throw new PlatformNotSupportedException();

    }
  }
}
