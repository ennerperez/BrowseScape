namespace BrowseScape.Core.Interfaces
{
  public interface IBrowser
  {
    string Id { get; set; }
    string Name { get; set; }
    string Icon { get; set; }
    bool IsInstalled { get; set; }
  }
}
