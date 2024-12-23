using System;
using BrowseScape.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BrowseScape.Core
{
  public static class Extensions
  {
    public static IServiceCollection AddCore(this IServiceCollection serviceCollection)
    {
      serviceCollection.AddSingleton<IBrowserService, Services.BrowserService>();
      if (OperatingSystem.IsWindows())
      {
        serviceCollection.AddSingleton<IBackend, Natives.Windows.Backend>();
      }
      else if (OperatingSystem.IsMacOS())
      {
        serviceCollection.AddSingleton<IBackend, Natives.MacOS.Backend>();
      }
      else if (OperatingSystem.IsLinux())
      {
        serviceCollection.AddSingleton<IBackend, Natives.Linux.Backend>();
      }
      return serviceCollection;
    }
  }
}
