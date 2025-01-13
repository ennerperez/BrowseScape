using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace BrowseScape.Core.Interfaces
{
  public interface IBackend
  {
    string GetActiveWindowTitle();
    Task RegisterAsync();
    Task UnregisterAsync();
    Task RegisterOrUnregisterAsync();
    Bitmap? GetAppIcon(string path);
    void OpenSettings();

  }
}
