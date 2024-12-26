using System.IO;
using BrowseScape.Core;
// ReSharper disable once CheckNamespace
using Avalonia.Rendering.Composition;

namespace System.Runtime
{
  public static class OperatingSystemExtensions
  {
    public static string GetName()
    {
      if (OperatingSystem.IsWindows()) return "Windows";
      else if (OperatingSystem.IsLinux()) return "Linux";
      else if (OperatingSystem.IsMacOS()) return "MacOS";
      else if (OperatingSystem.IsAndroid()) return "Android";
      else if (OperatingSystem.IsIOS()) return "iOS";
      else if (OperatingSystem.IsFreeBSD()) return "FreeBSD";
      else if (OperatingSystem.IsBrowser()) return "Browser";
      return string.Empty;
    }

    public static string GetDataDir()
    {
      string dataDir;
      var osAppDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
      if (string.IsNullOrEmpty(osAppDataDir))
      {
        dataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), $".{Metadata.Name.ToLower()}");
      }
      else
      {
        dataDir = Path.Combine(osAppDataDir, Metadata.Name);
      }

      if (!Directory.Exists(dataDir))
      {
        Directory.CreateDirectory(dataDir);
      }
      return dataDir;
    }
  }
}
