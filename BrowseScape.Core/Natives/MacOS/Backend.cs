using System.IO;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia;
using BrowseScape.Core.Interfaces;

namespace BrowseScape.Core.Natives.MacOS
{
  // ReSharper disable once InconsistentNaming
  [SupportedOSPlatform("macOS")]
  public class Backend : IBackend
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
    public string GetActiveWindowTitle()
    {
      throw new System.NotImplementedException();
    }
    public Task RegisterAsync()
    {
      throw new System.NotImplementedException();
    }
    public Task UnregisterAsync()
    {
      throw new System.NotImplementedException();
    }

  }
}
