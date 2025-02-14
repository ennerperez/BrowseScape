using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using BrowseScape.Core;
using BrowseScape.Core.Interfaces;
using Castle.Core.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit.Microsoft.DependencyInjection;
using Xunit.Microsoft.DependencyInjection.Abstracts;
using OS=System.Runtime.OperatingSystemExtensions;

namespace BrowseScape.UnitTests.Fixtures
{
  public class TestProjectFixture : TestBedFixture
  {
    protected override void AddServices(IServiceCollection services, IConfiguration configuration)
    {
      var assemblyPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
      configuration = new ConfigurationBuilder()
        .SetBasePath(assemblyPath ?? Directory.GetCurrentDirectory())
        .AddIniFile("Config.ini")
        .AddIniFile($"Config.{OS.GetName()}.ini", true)
        .AddEnvironmentVariables()
        .Build();
      
      services.AddSingleton(configuration);
      services.AddSingleton(Substitute.For<ILogger>());
      services.AddSingleton(Substitute.For<INotificationService>());
      services.AddCore();
    }
    protected override IEnumerable<TestAppSettings> GetTestAppSettings()
    {
      yield return new TestAppSettings { Filename = "appsettings.json", IsOptional = true };
    }
    protected override ValueTask DisposeAsyncCore() => new ValueTask();
    
    // protected override void AddUserSecrets(IConfigurationBuilder configurationBuilder) 
    //   => configurationBuilder.AddUserSecrets<TestProjectFixture>();
  }
}
