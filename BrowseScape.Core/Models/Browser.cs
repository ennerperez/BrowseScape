using System;
using System.IO;
using BrowseScape.Core.Interfaces;

namespace BrowseScape.Core.Models
{
  public class Browser : IBrowser
  {
    public string Id { get; set; }
    public string Name { get; set; }
    public string Args { get; set; }
    public string Icon { get; set; }
    public bool IsInstalled => File.Exists(Environment.ExpandEnvironmentVariables(Path));
    public string Path { get; set; }

  }
}
