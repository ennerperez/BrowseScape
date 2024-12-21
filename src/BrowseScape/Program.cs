using Avalonia;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Avalonia.Controls.Notifications;
using Avalonia.Media.Fonts;
using BrowseScape.Core;
using BrowseScape.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using OS = System.Runtime.OperatingSystemExtensions;

namespace BrowseScape
{
  internal static partial class Program
  {

    #region Metadata

    [GeneratedRegex(@"v?\=?((?:[0-9]{1,}\.{0,}){1,})\-?(.*)?\+(.*)?", RegexOptions.Compiled)]
    private static partial Regex VersionRegex();

    private static void ReadMetadata()
    {
      Metadata.Name = Assembly.GetAssembly(typeof(Program)).Product();
      Metadata.Description = Assembly.GetAssembly(typeof(Program)).Description();
      Metadata.Assembly = Assembly.GetAssembly(typeof(Program))?.Location;

      var informationalVersion = Assembly.GetAssembly(typeof(Program)).InformationalVersion();
      var versionMatch = VersionRegex().Match(informationalVersion);
      if (versionMatch.Success)
      {
        Metadata.Version = Version.Parse(versionMatch.Groups[1].Value);
        Metadata.Tag = versionMatch.Groups[2].Value;
        Metadata.Commit = versionMatch.Groups[3].Value?.Substring(0, 7);
        Metadata.DisplayVersion = string.Join("-", Metadata.Version.ToString(3), Metadata.Tag);
      }
      Metadata.Environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
    }

    #endregion

    internal static IBackend Backend { get; private set; }
    internal static ILogger Logger { get; private set; }
    internal static ServiceProvider Services { get; set; }
    internal static IConfiguration Configuration { get; private set; }

    public static bool IsSingleViewLifetime =>
      System.Environment.GetCommandLineArgs()
        .Any(a => a == "--fbdev" || a == "--drm");

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
      try
      {
        BuildAvaloniaApp()
          .InitializeAvaloniaApp(args)
          .StartWithClassicDesktopLifetime(args);
      }
      catch (Exception)
      {
        // ignore
      }
    }

    internal static bool IsRunning { get; private set; }
    private static AppBuilder InitializeAvaloniaApp(this AppBuilder appBuilder, string[] args)
    {
      AppDomain.CurrentDomain.UnhandledException += AppDomainUnhandledException;

      Configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddIniFile("Config.ini")
        .AddIniFile($"Config.{OS.GetName()}.ini", true)
        .AddEnvironmentVariables()
        .AddCommandLine(args)
        .Build();

      // Initialize Logger
      var loggerConfiguration = new LoggerConfiguration()
        .WriteTo.Async(a =>
        {
#if DEBUG
          a.Console();
          a.File(
            path: $"../../../Logs/.log",
            rollingInterval: RollingInterval.Day,
            flushToDiskInterval: TimeSpan.FromSeconds(30),
            shared: true
          );
#else
          a.File(
            path: $"%appdata%/../Local/{Name}/Logs/.log",
            rollingInterval: RollingInterval.Day,
            flushToDiskInterval: TimeSpan.FromSeconds(30),
            shared: true
          );
#endif

        })
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithProcessId()
        .Enrich.WithProcessName()
        .Enrich.WithThreadId()
        .Enrich.WithThreadName()
        .Enrich.WithProperty("ApplicationName", Metadata.Name);

      // Initialize Logger
      Logger = Log.Logger = loggerConfiguration
        .WriteTo.Trace()
        .CreateLogger();

      // Register all the services needed for the application to run
      var collection = new ServiceCollection();
      collection.AddSingleton(Configuration);
      collection.AddLogging(c => c.AddSerilog(Logger, true));
      
      collection.AddSingleton<INotificationManager, WindowNotificationManager>();
      
      // Core Services
      collection.AddCore();

      // Creates a ServiceProvider containing services from the provided IServiceCollection
      Services = collection.BuildServiceProvider();

      var backend = Services.GetService<IBackend>();
      backend?.SetupApp(appBuilder);

      IsRunning = true;

      return appBuilder;

    }
    private static void AppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
      var ex = (Exception)e.ExceptionObject;
      Logger?.Fatal(ex, "{Message}", ex.Message);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
    {
      ReadMetadata();

      var builder = AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .WithInterFont()
        .LogToTrace();

      builder.ConfigureFonts(manager =>
      {
        var monospace = new EmbeddedFontCollection(
          new Uri($"fonts:{Metadata.Name}", UriKind.Absolute),
          new Uri($"avares://{Metadata.Name}/Resources/Fonts", UriKind.Absolute));
        manager.AddFontCollection(monospace);
      });

      return builder;
    }
  }
}
