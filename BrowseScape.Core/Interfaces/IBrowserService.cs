using System.Threading.Tasks;

namespace BrowseScape.Core.Interfaces
{
  public interface IBrowserService
  {
    Task<bool> LaunchAsync(string url);

  }
}
