using System.IO;
using System.Runtime.Versioning;
using Avalonia;
using BrowseScape.Core.Interfaces;

namespace BrowseScape.Core.Natives
{
  // ReSharper disable once InconsistentNaming
  [SupportedOSPlatform("macOS")]
  public class MacOS : IBackend
  {
    public void SetupApp(AppBuilder builder)
    {
      builder.With(new MacOSPlatformOptions()
      {
        DisableDefaultApplicationMenuItems = true,
      });

      var customPathFile = Path.Combine(IBackend.DataDir, "PATH");
      if (File.Exists(customPathFile))
      {
        IBackend.CustomPathEnv = File.ReadAllText(customPathFile).Trim();
      }
    }
    
  }
}
