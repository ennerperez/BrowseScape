using System.Collections.ObjectModel;
using BrowseScape.Core.Models;

namespace BrowseScape.ViewModels
{
  public class MainWindowViewModel
  {
    public MainWindowViewModel()
    {
        Browsers = new ObservableCollection<Browser>();
        for (var i = 0; i < 5; i++)
        {
          Browsers.Add(new Browser()
          {
            Icon = "/Assets/Images/Browsers/Chronium.svg",
            Id = "Avalonia",
            Name = "Avalonia",
            IsInstalled = true
          });
        }
    }
    public ObservableCollection<Browser> Browsers { get; set; }
  }

}
