using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using BrowseScape.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BrowseScape
{
  public partial class App : Application
  {
    public override void Initialize()
    {
      AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {

      try
      {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
          if (desktop.Args is { Length: 0 })
          {
            desktop.TryShutdown();
            return;
          }
          if (desktop.Args != null && new[] { "--register", "-r" }.Contains(desktop.Args[0], StringComparer.OrdinalIgnoreCase))
          {
            var dbs = Program.Services.GetService<IDefaultBrowserService>();
            await dbs.RegisterAsync();
          }
          else if (desktop.Args != null && new[] { "--unregister", "-u" }.Contains(desktop.Args[0], StringComparer.OrdinalIgnoreCase))
          {
            var dbs = Program.Services.GetService<IDefaultBrowserService>();
            await dbs.UnregisterAsync();
          } 
          desktop.MainWindow = new MainWindow();
        }
        base.OnFrameworkInitializationCompleted();
      }
      catch (Exception e)
      {
        Program.Logger.Error(e, "An error occured during initialization.");
      }
    }
  }
}
