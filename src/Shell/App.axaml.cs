using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Svg.Skia;
using BrowseScape.Core;
using BrowseScape.Core.Interfaces;
using BrowseScape.Core.Models;
using BrowseScape.Runner;
using BrowseScape.Shell.ViewModels;
using BrowseScape.Shell.Views;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using ILogger=Serilog.ILogger;
using OS=System.Runtime.OperatingSystemExtensions;

namespace BrowseScape.Shell
{
  public partial class App : Application
  {

    #region Metadata

    [GeneratedRegex(@"v?\=?((?:[0-9]{1,}\.{0,}){1,})\-?(.*)?\+(.*)?", RegexOptions.Compiled)]
    private static partial Regex VersionRegex();

    private static void ReadMetadata()
    {
      Metadata.Name = Assembly.GetAssembly(typeof(App)).Product();
      Metadata.Description = Assembly.GetAssembly(typeof(App)).Description();
      Metadata.Assembly = Assembly.GetAssembly(typeof(App))?.Location;

      var informationalVersion = Assembly.GetAssembly(typeof(App)).InformationalVersion();
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

    public static ILogger? Logger { get; private set; }
    public static ServiceProvider? Services { get; private set; }
    public static IConfiguration? Configuration { get; private set; }

    public static bool IsSingleViewLifetime =>
      Environment.GetCommandLineArgs()
        .Any(a => a == "--fbdev" || a == "--drm");

    [STAThread]
    public static async Task Main(string[] args)
    {
      ReadMetadata();

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

      AppBuilder? builder;

      try
      {

        builder = BuildAvaloniaApp(args);
        IsRunning = true;

        if (args.Length != 0)
        {
          var results = await builder.StartWithOpenerLifetime(args);
          if (!results.Any())
          {
            builder.StartWithClassicDesktopLifetime(args);
          }
        }
        else
        {
          builder.StartWithClassicDesktopLifetime(args);
        }
      }
      catch (Exception e)
      {
        Logger.Fatal(e, "{Message}", e.Message);
      }
    }

    internal static bool IsRunning { get; private set; }

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp(string[] args)
    {
      GC.KeepAlive(typeof(SvgImageExtension).Assembly);
      GC.KeepAlive(typeof(Avalonia.Svg.Skia.Svg).Assembly);
      var builder = AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .WithInterFont()
        // .ConfigureFonts(manager =>
        // {
        //   var monospace = new EmbeddedFontCollection(
        //     new Uri($"fonts:{Metadata.Name}", UriKind.Absolute),
        //     new Uri($"avares://{Metadata.Name}/Assets/Fonts", UriKind.Absolute));
        //   manager.AddFontCollection(monospace);
        // })
        .UseSkia()
        .LogToTrace();

      var assemblyPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
      Configuration = new ConfigurationBuilder()
        .SetBasePath(assemblyPath ?? Directory.GetCurrentDirectory())
        .AddIniFile("Config.ini")
        .AddIniFile($"Config.{OS.GetName()}.ini", true)
        .AddEnvironmentVariables()
        .AddCommandLine(args)
        .Build();

      // Register all the services needed for the application to run
      var collection = new ServiceCollection();
      collection.AddSingleton(Configuration);
      collection.AddLogging(c => c.AddSerilog(Logger, true));

      // Core Services
      collection.AddCore()
        .AddRunner()
        .AddShell(builder);

      // Creates a ServiceProvider containing services from the provided IServiceCollection
      Services = collection.BuildServiceProvider();

      // OS Setup
      builder.SetupApp();

      return builder;
    }
    public override void Initialize()
    {
      AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
      try
      {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
          var configs = Services?.GetService<IConfiguration>();
          var viewModel = new MainWindowViewModel { Browsers = [] };

          var browsers = new Dictionary<string, Browser>();
          if (configs != null)
          {
            configs.Bind("browsers", browsers);
          }
          foreach (var browser in browsers)
          {
            browser.Value.Id = browser.Key;
            if (!string.IsNullOrWhiteSpace(browser.Value.Icon))
            {
              browser.Value.Icon = browser.Value.Icon;
            }
          }
          viewModel.Browsers = new ObservableCollection<Browser>(browsers.Values.Where(m => m.IsInstalled));
          viewModel.ItemTappedCommand = new RelayCommand<TappedEventArgs>(ItemTapped);

          // Line below is needed to remove Avalonia data validation.
          // Without this line you will get duplicate validations from both Avalonia and CT
          BindingPlugins.DataValidators.RemoveAt(0);
          desktop.MainWindow = new MainWindow
          {
            MinHeight = 120,
            MinWidth = viewModel.Browsers.Count * 120,
            Height = 120,
            Width = viewModel.Browsers.Count * 120,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            DataContext = viewModel
          };

          return;
        }
      }
      catch (Exception e)
      {
        Logger?.Fatal(e, "{Message}", "An error occured during initialization.");
      }

      base.OnFrameworkInitializationCompleted();
    }

    private void ItemTapped(TappedEventArgs? obj)
    {
      var source = obj?.Source;
    }
  }

  public static class AppBuilderExtensions
  {

    public static async void StartWithRegisterLifetime(this AppBuilder builder)
    {
      var dbs = App.Services?.GetService<IBackend>();
      if (dbs != null)
      {
        await dbs.RegisterAsync();
      }
    }
    public static async void StartWithUnregisterLifetime(this AppBuilder builder)
    {
      var dbs = App.Services?.GetService<IBackend>();
      if (dbs != null)
      {
        await dbs.UnregisterAsync();
      }
    }
    public static async Task<int[]> StartWithOpenerLifetime(this AppBuilder builder, string[] args)
    {
      var ids = new List<int>();
      var bs = App.Services?.GetService<IBrowserService>();
      if (bs != null)
      {
        foreach (var arg in args)
        {
          var r = await bs.LaunchAsync(arg.Trim());
          if (r != 0) ids.Add(r);
        }
      }
      return ids.ToArray();
    }
  }

}
