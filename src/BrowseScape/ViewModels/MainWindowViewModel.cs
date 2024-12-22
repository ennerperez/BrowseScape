using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using BrowseScape.Core.Interfaces;
using BrowseScape.Core.Models;
using Bitmap = Avalonia.Media.Imaging.Bitmap;

namespace BrowseScape.ViewModels
{
  public class MainWindowViewModel
  {
    public MainWindowViewModel()
    {
        Browsers = new ObservableCollection<Browser>();
        var data = File.ReadAllBytes("D:\\Sources\\Repos\\BrowseScape\\src\\BrowseScape\\Assets\\App.ico");
        Browsers.Add(new Browser()
        {
          Icon = new Bitmap(new MemoryStream(data)),
          Id = "Avalonia",
          Name = "Avalonia",
          IsInstalled = true
        });
    }
    public ObservableCollection<Browser> Browsers { get; set; }
  }

}
