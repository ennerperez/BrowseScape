using System.Runtime.Versioning;
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
  }
}
