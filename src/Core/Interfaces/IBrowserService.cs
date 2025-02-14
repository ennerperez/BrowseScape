using System.Threading.Tasks;

namespace BrowseScape.Core.Interfaces
{
  public interface IBrowserService
  {
    Task<int> LaunchAsync(string url, string windowTitle = "");

  }
}
