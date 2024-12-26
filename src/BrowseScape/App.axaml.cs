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
using Avalonia.Controls.Notifications;
using Avalonia.Data.Core.Plugins;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Svg.Skia;
using BrowseScape.Core;
using BrowseScape.Core.Interfaces;
using BrowseScape.Core.Models;
using BrowseScape.ViewModels;
using BrowseScape.Views;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using ILogger=Serilog.ILogger;
using OS=System.Runtime.OperatingSystemExtensions;

namespace BrowseScape
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

    internal static ILogger Logger { get; private set; }
    internal static ServiceProvider Services { get; private set; }
    internal static IConfiguration Configuration { get; private set; }

    public static bool IsSingleViewLifetime =>
      Environment.GetCommandLineArgs()
        .Any(a => a == "--fbdev" || a == "--drm");

    [STAThread]
    public static void Main(string[] args)
    {

      ReadMetadata();

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
            path: Path.Combine(OS.GetDataDir(), "Logs/.log"),
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

      IsRunning = true;

      AppDomain.CurrentDomain.UnhandledException += (_, e) =>
      {
        var ex = e.ExceptionObject as Exception;
        Logger?.Fatal(ex, "{Message}", ex?.Message);
      };

      TaskScheduler.UnobservedTaskException += (_, e) =>
      {
        var ex = e.Exception as Exception;
        Logger?.Fatal(ex, "{Message}", ex?.Message);
        e.SetObserved();
      };

      try
      {
        BuildAvaloniaApp()
          .StartWithClassicDesktopLifetime(args);
      }
      catch (Exception e)
      {
        Logger.Fatal(e, "{Message}", e.Message);
      }
    }

    internal static bool IsRunning { get; private set; }

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
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

      if (OperatingSystem.IsLinux())
      {
        Core.Natives.Linux.Backend.SetupApp(builder);
      }
      else if (OperatingSystem.IsMacOS())
      {
        Core.Natives.MacOS.Backend.SetupApp(builder);
      }
      else if (OperatingSystem.IsWindows())
      {
        Core.Natives.Windows.Backend.SetupApp(builder);
      }

      return builder;
    }
    public override void Initialize()
    {
      AvaloniaXamlLoader.Load(this);
    }

    private static readonly string[] s_registerCommand = ["--register", "-r"];
    private static readonly string[] s_unregisterCommand = ["--unregister", "-u"];

    public override async void OnFrameworkInitializationCompleted()
    {
      try
      {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
          if ((await TryLaunchedAsRegister(desktop))) return;
          if ((await TryLaunchedAsUnregister(desktop))) return;
          if ((await TryLaunchedAsOpener(desktop))) return;
          await TryLaunchedAsNormal(desktop);
        }
      }
      catch (Exception e)
      {
        Logger?.Fatal(e, "{Message}", "An error occured during initialization.");
      }

      base.OnFrameworkInitializationCompleted();
    }

    private async Task<bool> TryLaunchedAsRegister(IClassicDesktopStyleApplicationLifetime desktop)
    {
      if (desktop.Args != null && desktop.Args.Length != 0 && s_registerCommand.Contains(desktop.Args[0], StringComparer.OrdinalIgnoreCase))
      {
        var dbs = Services.GetService<IBackend>();
        await dbs.RegisterAsync();
        desktop.Shutdown(0);
        return true;
      }
      return false;
    }
    private async Task<bool> TryLaunchedAsUnregister(IClassicDesktopStyleApplicationLifetime desktop)
    {
      if (desktop.Args != null && desktop.Args.Length != 0 && s_unregisterCommand.Contains(desktop.Args[0], StringComparer.OrdinalIgnoreCase))
      {
        var dbs = Services.GetService<IBackend>();
        await dbs.UnregisterAsync();
        desktop.Shutdown(0);
        return true;
      }
      return false;
    }
    private async Task<bool> TryLaunchedAsOpener(IClassicDesktopStyleApplicationLifetime desktop)
    {
      if (desktop.Args != null && desktop.Args.Length != 0)
      {
        var bs = Services.GetService<IBrowserService>();
        foreach (var arg in desktop.Args)
        {
          await bs.LaunchAsync(arg.Trim());
        }
        desktop.Shutdown(0);
        return true;
      }
      return false;
    }
    private Task TryLaunchedAsNormal(IClassicDesktopStyleApplicationLifetime desktop)
    {
      var configs = Services.GetService<IConfiguration>();
      var viewModel = new MainWindowViewModel { Browsers = [] };

      var browsers = new Dictionary<string, Browser>();
      configs.Bind("browsers", browsers);
      foreach (var browser in browsers)
      {
        browser.Value.Id = browser.Key;
        if (!string.IsNullOrWhiteSpace(browser.Value.Icon))
        {
          browser.Value.Icon = browser.Value.Icon;
        }
      }
      viewModel.Browsers = new ObservableCollection<Browser>(browsers.Values.Where(m=> m.IsInstalled));
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

      return Task.CompletedTask;
    }
    private void ItemTapped(TappedEventArgs obj)
    {
      var source = obj.Source;
    }
  }

}
