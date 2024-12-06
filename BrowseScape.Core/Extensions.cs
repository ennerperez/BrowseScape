using System;
using BrowseScape.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BrowseScape.Core
{
  public static class Extensions
  {
    public static IServiceCollection AddCore(this IServiceCollection serviceCollection)
    {
      if (OperatingSystem.IsWindows())
      {
        serviceCollection.AddSingleton<IBackend, Natives.Windows.Backend>();
      }
      else if (OperatingSystem.IsMacOS())
      {
        serviceCollection.AddSingleton<IBackend, Natives.MacOS.Backend>();
        serviceCollection.AddSingleton<IBrowserService, Natives.MacOS.Services.BrowserService>();
        serviceCollection.AddSingleton<IDefaultBrowserService, Natives.MacOS.Services.DefaultBrowserService>();
      }
      else if (OperatingSystem.IsLinux())
      {
        serviceCollection.AddSingleton<IBackend, Natives.Linux.Backend>();
      }
      return serviceCollection;
    }
  }
}
