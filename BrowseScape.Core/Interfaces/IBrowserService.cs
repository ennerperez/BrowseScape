using System.Threading.Tasks;

namespace BrowseScape.Core.Interfaces
{
  public interface IBrowserService
  {
    Task LaunchAsync(string url, string windowTitle);

  }
}
