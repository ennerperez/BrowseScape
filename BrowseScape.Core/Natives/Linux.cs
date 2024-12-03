using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia;
using BrowseScape.Core.Interfaces;

namespace BrowseScape.Core.Natives
{

  [SupportedOSPlatform("linux")]
  public class Linux : IBackend
  {
    public void SetupApp(AppBuilder builder)
    {
      builder.With(new X11PlatformOptions() { EnableIme = true, });
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
