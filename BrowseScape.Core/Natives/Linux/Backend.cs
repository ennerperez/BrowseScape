using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media.Imaging;
using BrowseScape.Core.Interfaces;

namespace BrowseScape.Core.Natives.Linux
{

  [SupportedOSPlatform("linux")]
  public class Backend : IBackend
  {
    public void SetupApp(AppBuilder builder)
    {
      builder.With(new X11PlatformOptions { EnableIme = true, });
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
    public Task RegisterOrUnregisterAsync()
    {
      throw new System.NotImplementedException();
    }
    public Bitmap GetAppIcon(string path)
    {
      throw new System.NotImplementedException();
    }
    public void OpenSettings()
    {
      throw new System.NotImplementedException();
    }
  }
}
