using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using BrowseScape.Core;
using Castle.Core.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit.Microsoft.DependencyInjection;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace BrowseScape.UnitTests.Fixtures
{
  public class TestProjectFixture : TestBedFixture
  {
    protected override void AddServices(IServiceCollection services, IConfiguration configuration)
    {
      configuration = new ConfigurationBuilder()
        .AddIniFile("Config.ini")
        .AddEnvironmentVariables()
        .Build();
      
      services.AddSingleton(configuration);
      services.AddSingleton(Substitute.For<ILogger>());
      services.AddSingleton(Substitute.For<INotificationManager>());
      services.AddCore();
    }
    protected override IEnumerable<TestAppSettings> GetTestAppSettings()
    {
      yield return new() { Filename = "appsettings.json", IsOptional = false };
    }
    protected override ValueTask DisposeAsyncCore() => new();
    
    // protected override void AddUserSecrets(IConfigurationBuilder configurationBuilder) 
    //   => configurationBuilder.AddUserSecrets<TestProjectFixture>();
  }
}
