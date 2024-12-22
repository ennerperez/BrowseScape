using Avalonia.Media.Imaging;

namespace BrowseScape.Core.Interfaces
{
  public interface IBrowser
  {
    Bitmap Icon { get; set; }
    string Id { get; set; }
  }
}
