using BrowseScape.Core.Interfaces;
using BrowseScape.Runner.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BrowseScape.Runner
{
  public static class Extensions
  {
    public static IServiceCollection AddRunner(this IServiceCollection serviceCollection)
    {
      serviceCollection.AddSingleton<INotificationService, NotificationService>();
      return serviceCollection;
    }
  }
}
