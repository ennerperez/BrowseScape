using System.Threading.Tasks;

namespace BrowseScape.Core.Interfaces
{
  public interface IDefaultBrowserService
  {
    Task RegisterAsync();
    Task UnregisterAsync();
    Task RegisterOrUnregisterAsync();
  }
}
