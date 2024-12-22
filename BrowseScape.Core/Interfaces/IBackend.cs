using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media.Imaging;

namespace BrowseScape.Core.Interfaces
{
  public interface IBackend
  {
    void SetupApp(AppBuilder builder);
    string GetActiveWindowTitle();
    Task RegisterAsync();
    Task UnregisterAsync();
    Task RegisterOrUnregisterAsync();
    Bitmap GetAppIcon(string path);
    void OpenSettings();
    public static string DataDir { get; private set; } = string.Empty;
    public static string CustomPathEnv { get; set; } = string.Empty;

  }
}
