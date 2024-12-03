using Avalonia;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Avalonia.Media.Fonts;
using BrowseScape.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace BrowseScape
{
  internal static partial class Program
  {

    #region Metadata

    internal static string Product => Assembly.GetAssembly(typeof(Program)).Product();
    internal static string DisplayVersion { get; private set; }
    internal static string Name { get; private set; }
    internal static Version Version { get; private set; }
    internal static string Commit { get; private set; }
    internal static string Tag { get; private set; }
    internal static string Environment { get; private set; }

    [GeneratedRegex(@"v?\=?((?:[0-9]{1,}\.{0,}){1,})\-?(.*)?\+(.*)?", RegexOptions.Compiled)]
    private static partial Regex VersionRegex();

    private static void ReadMetadata()
    {
      Name = Assembly.GetAssembly(typeof(Program)).Product();

      var informationalVersion = Assembly.GetAssembly(typeof(Program)).InformationalVersion();
      var versionMatch = VersionRegex().Match(informationalVersion);
      if (versionMatch.Success)
      {
        Version = Version.Parse(versionMatch.Groups[1].Value);
        Tag = versionMatch.Groups[2].Value;
        Commit = versionMatch.Groups[3].Value?.Substring(0, 7);
        DisplayVersion = string.Join("-", Version.ToString(3), Tag);
      }
      Environment = System.Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
    }

    #endregion

    internal static IBackend Backend { get; private set; }
    internal static ILogger Logger { get; private set; }

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
      var builder = BuildAvaloniaApp(args).InitializeAvaloniaApp(args);

      if (args.Length == 0)
      {
        return;
      }

      builder.StartWithClassicDesktopLifetime(args);
    }

    internal static bool IsRunning { get; private set; }
    private static AppBuilder InitializeAvaloniaApp(this AppBuilder appBuilder, string[] args)
    {
      AppDomain.CurrentDomain.UnhandledException += AppDomainUnhandledException;

      Configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddIniFile("Config.ini")
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
        .Enrich.WithProperty("ApplicationName", Product);

      // Initialize Logger
      Logger = Log.Logger = loggerConfiguration
        .WriteTo.Trace()
        .CreateLogger();

      IsRunning = true;

      return appBuilder;

    }
    private static void AppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
      var ex = (Exception)e.ExceptionObject;
      Logger?.Fatal(ex, "{Message}", ex.Message);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp(string[] args)
    {
      ReadMetadata();

      var builder = AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .WithInterFont()
        .LogToTrace();

      builder.ConfigureFonts(manager =>
      {
        var monospace = new EmbeddedFontCollection(
          new Uri($"fonts:{Name}", UriKind.Absolute),
          new Uri($"avares://{Name}/Resources/Fonts", UriKind.Absolute));
        manager.AddFontCollection(monospace);
      });

      Backend = IBackend.GetBackend();
      Backend.SetupApp(builder);

      return builder;
    }
  }
}
