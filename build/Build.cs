using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Git;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;

partial class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    public readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;
    
    [Parameter("Environment to build - Default is 'Development' (local) or 'Production' (server)", Name = "environment")]
    public readonly Environment Environment = IsLocalBuild ? Environment.Development : Environment.Production;
    
    [Solution]
    public readonly Solution Solution;

    [GitRepository]
    public readonly GitRepository Repository;

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
        });

    Target Restore => _ => _
        .Executes(() =>
        {
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
        });

    DotNetVerbosity getDotNetVerbosity()
    {
      return Verbosity switch
      {
        Verbosity.Minimal => DotNetVerbosity.minimal,
        Verbosity.Verbose => DotNetVerbosity.detailed,
        Verbosity.Quiet => DotNetVerbosity.quiet,
        Verbosity.Normal => DotNetVerbosity.normal,
        _ => DotNetVerbosity.diagnostic
      };
    }

    [GeneratedRegex(@"v?\=?((?:[0-9]{1,}\.{0,}){1,})\-?(.*)", RegexOptions.Compiled)]
    private static partial Regex VersionRegex();

    [GeneratedRegex(@"\[assembly: AssemblyVersion\(.*\)\]", RegexOptions.Compiled)]
    private static partial Regex AssemblyVersionRegex();

    [GeneratedRegex(@"\[assembly: AssemblyFileVersion\(.*\)\]", RegexOptions.Compiled)]
    private static partial Regex AssemblyFileVersionRegex();

    [GeneratedRegex(@"\[assembly: AssemblyInformationalVersion\(.*\)\]", RegexOptions.Compiled)]
    private static partial Regex AssemblyInformationalVersionRegex();
  
    string GetReleaseNotes()
    {
      var gitOutput = GitTasks.Git("log -1 --pretty=%B");

      var releaseNotes = new List<string> { $"Environment: {Environment}", System.Environment.NewLine, "Release Notes:", System.Environment.NewLine };
      releaseNotes.AddRange(gitOutput.Where(x => !string.IsNullOrWhiteSpace(x.Text)).Select(x => x.Text).ToList());

      return string.Join(System.Environment.NewLine, releaseNotes);
    }
}
