using System.Threading.Tasks;

namespace BrowseScape.Core.Interfaces
{
  public interface IBackend
  {
    string GetActiveWindowTitle();
    Task RegisterAsync();
    Task UnregisterAsync();
    Task RegisterOrUnregisterAsync();
    byte[] GetAppIcon(string path);
    void OpenSettings();

  }
}
