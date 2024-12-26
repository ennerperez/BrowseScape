using System.Collections.ObjectModel;
using Avalonia.Input;
using BrowseScape.Core.Models;
using CommunityToolkit.Mvvm.Input;

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
            Name = "Avalonia"
          });
        }
    }
    public ObservableCollection<Browser> Browsers { get; set; }
    
    public RelayCommand<TappedEventArgs> ItemTappedCommand { get; set; }
  }

}
