using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
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
            desktop.Shutdown(0);
            return;
          }
          if (desktop.Args != null && new[] { "--register", "-r" }.Contains(desktop.Args[0], StringComparer.OrdinalIgnoreCase))
          {
            var dbs = Program.Services.GetService<IBackend>();
            await dbs.RegisterAsync();
            desktop.Shutdown(0);
            return;
          }
          else if (desktop.Args != null && new[] { "--unregister", "-u" }.Contains(desktop.Args[0], StringComparer.OrdinalIgnoreCase))
          {
            var dbs = Program.Services.GetService<IBackend>();
            await dbs.UnregisterAsync();
            desktop.Shutdown(0);
            return;
          }
          else if (desktop.Args != null)
          {
            var bs = Program.Services.GetService<IBrowserService>();
            foreach (var arg in desktop.Args)
            {
              await bs.LaunchAsync(arg.Trim());
            }
            desktop.Shutdown(0);
            return;
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
