using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using BrowseScape.Core.Interfaces;
using BrowseScape.Core.Models;
using BrowseScape.ViewModels;
using BrowseScape.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BrowseScape
{
  public partial class App : Application
  {
    public override void Initialize()
    {
      AvaloniaXamlLoader.Load(this);
    }

    private static readonly string[] s_registerCommand = ["--register", "-r"];
    private static readonly string[] s_unregisterCommand = ["--unregister", "-u"];

    public override async void OnFrameworkInitializationCompleted()
    {
      try
      {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
          if (desktop.Args != null && desktop.Args.Length != 0 && s_registerCommand.Contains(desktop.Args[0], StringComparer.OrdinalIgnoreCase))
          {
            var dbs = Program.Services.GetService<IBackend>();
            await dbs.RegisterAsync();
            desktop.Shutdown(0);
            return;
          }
          else if (desktop.Args != null && desktop.Args.Length != 0 && s_unregisterCommand.Contains(desktop.Args[0], StringComparer.OrdinalIgnoreCase))
          {
            var dbs = Program.Services.GetService<IBackend>();
            await dbs.UnregisterAsync();
            desktop.Shutdown(0);
            return;
          }
          else if (desktop.Args != null && desktop.Args.Length != 0)
          {
            var bs = Program.Services.GetService<IBrowserService>();
            foreach (var arg in desktop.Args)
            {
              await bs.LaunchAsync(arg.Trim());
            }
            desktop.Shutdown(0);
            return;
          }

          var backend = Program.Services.GetService<IBackend>();
          var configs = Program.Services.GetService<IConfiguration>();
          var viewModel = new MainWindowViewModel { Browsers = [] };

          var browsers = new Dictionary<string, string>();
          configs.Bind("browsers", browsers);
          var items = browsers.Select(m =>
            new Browser
            {
              Id = m.Key, IsInstalled = File.Exists(Environment.ExpandEnvironmentVariables(m.Value)),
              //Icon = backend.GetAppIcon(m.Value)
            }
          ).Where(m => m.IsInstalled);
          viewModel.Browsers = new ObservableCollection<Browser>(items);

          // Line below is needed to remove Avalonia data validation.
          // Without this line you will get duplicate validations from both Avalonia and CT
          BindingPlugins.DataValidators.RemoveAt(0);
          desktop.MainWindow = new MainWindow { DataContext = viewModel };
        }
      }
      catch (Exception e)
      {
        Program.Logger.Error(e, "An error occured during initialization.");
      }
      
      base.OnFrameworkInitializationCompleted();
    }
  }
}
