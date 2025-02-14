using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using BrowseScape.Core;
using BrowseScape.Core.Interfaces;
using BrowseScape.Runner.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using ILogger=Serilog.ILogger;
using OS=System.Runtime.OperatingSystemExtensions;

namespace BrowseScape.Runner
{

  public static partial class Program
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
      if (informationalVersion != null)
      {
        var versionMatch = VersionRegex().Match(informationalVersion);
        if (versionMatch.Success)
        {
          Metadata.Version = Version.Parse(versionMatch.Groups[1].Value);
          Metadata.Tag = versionMatch.Groups[2].Value;
          Metadata.Commit = versionMatch.Groups[3].Value.Substring(0, 7);
          Metadata.DisplayVersion = string.Join("-", Metadata.Version.ToString(3), Metadata.Tag);
        }
      }
      Metadata.Environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
    }

    #endregion

    public static ILogger Logger { get; private set; }

    public static IConfiguration Configuration { get; private set; }

    public static string QueueFile => Path.Combine(Path.GetTempPath(), $"{Metadata.Name ?? "BrowseScape"}.queue");

    [STAThread]
    public static void Main(string[] args)
    {
      ReadMetadata();

      File.AppendAllLines(QueueFile, new[] { string.Join(" ", args) });

      bool result;
      var mutex = new Mutex(true, Metadata.Name, out result);

      if (!result) { return; }

      // Initialize Logger
      var loggerConfiguration = new LoggerConfiguration()
        .WriteTo.Async(a =>
        {
          a.File(
            path: Path.Combine(OS.GetDataDir(), "Logs/.log"),
            rollingInterval: RollingInterval.Day,
            flushToDiskInterval: TimeSpan.FromSeconds(30),
            shared: true
          );
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

      System.Runtime.Exceptions.UnhandledException += (_, e) =>
      {
        var ex = e.ExceptionObject as Exception;
        Logger.Fatal(ex, "{Message}", ex?.Message);
      };

      HostApplicationBuilder builder;

      try
      {
        builder = BuildRunnerApp(args);
        IsRunning = true;

        var host = builder.Build();
        host.Run();

      }
      catch (Exception e)
      {
        Logger.Fatal(e, "{Message}", e.Message);
      }

      GC.KeepAlive(mutex);

    }
    private static HostApplicationBuilder BuildRunnerApp(string[] args)
    {
      var builder = Host.CreateApplicationBuilder(args);

      var assemblyPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
      Configuration = new ConfigurationBuilder()
        .SetBasePath(assemblyPath ?? Directory.GetCurrentDirectory())
        .AddIniFile("Config.ini")
        .AddIniFile($"Config.{OS.GetName()}.ini", true)
        .AddEnvironmentVariables()
        .AddCommandLine(args)
        .Build();

      // Register all the services needed for the application to run
      builder.Services.AddSingleton(Configuration);
      builder.Services.AddLogging(c => c.AddSerilog(Logger, true));

      // Core Services
      builder.Services.AddCore()
        .AddRunner();

      builder.Services.AddHostedService<Worker>();

      return builder;
    }
    internal static bool IsRunning { get; private set; }

  }
}
