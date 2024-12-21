// ReSharper disable once CheckNamespace
namespace System.Runtime
{
  public static class OperatingSystemExtensions
  {
    public static string GetName()
    {
      if (OperatingSystem.IsWindows()) return "Windows";
      else if  (OperatingSystem.IsLinux()) return "Linux";
      else if  (OperatingSystem.IsMacOS()) return "MacOS";
      else if  (OperatingSystem.IsAndroid()) return "Android";
      else if  (OperatingSystem.IsIOS()) return "iOS";
      else if  (OperatingSystem.IsFreeBSD()) return "FreeBSD";
      else if  (OperatingSystem.IsBrowser()) return "Browser";
      return string.Empty;
    }
  }
}
