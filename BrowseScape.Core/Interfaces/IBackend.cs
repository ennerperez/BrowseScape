using System;
using System.Threading.Tasks;
using Avalonia;

namespace BrowseScape.Core.Interfaces
{
  public interface IBackend
  {
    void SetupApp(AppBuilder builder);

    string GetActiveWindowTitle();
    
    Task RegisterAsync();
    Task UnregisterAsync();

    public static string DataDir { get; private set; } = string.Empty;
    public static string CustomPathEnv { get; set; } = string.Empty;

    public static IBackend GetBackend()
    {
      if (OperatingSystem.IsWindows())
      {
        return new Natives.Windows.Backend();
      }
      if (OperatingSystem.IsMacOS())
      {
        return new Natives.MacOS.Backend();
      }
      if (OperatingSystem.IsLinux())
      {
        return new Natives.Linux.Backend();
      }

      throw new PlatformNotSupportedException();

    }
  }
}
