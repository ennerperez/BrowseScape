using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BrowseScape.Core.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BrowseScape.Runner.Services
{
  public class Worker : BackgroundService
  {
    private readonly IBackend _backend;
    private readonly IBrowserService _browserService;
    private readonly ILogger<Worker> _logger;
    public Worker(IBackend backend, IBrowserService browserService, ILogger<Worker> logger)
    {
      _backend = backend;
      _browserService = browserService;
      _logger = logger;
    }

    private static readonly string[] s_registerCommand = ["--register", "-r"];
    private static readonly string[] s_unregisterCommand = ["--unregister", "-u"];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      while (!stoppingToken.IsCancellationRequested)
      {
        var queue = new Queue<string>(File.ReadLines(Program.QueueFile));
        if (queue.Count > 0)
        {
          var cmd = queue.Dequeue();
          if (!string.IsNullOrWhiteSpace(cmd))
          {
            if (s_registerCommand.Contains(cmd, StringComparer.OrdinalIgnoreCase))
            {
              await _backend.RegisterAsync();
            }
            else if (s_unregisterCommand.Contains(cmd, StringComparer.OrdinalIgnoreCase))
            {
              await _backend.UnregisterAsync();
            }
            else
            {
              await _browserService.LaunchAsync(cmd.Trim());
            }
          }
          File.WriteAllLines(Program.QueueFile, queue.ToArray());
        }
        await Task.Delay(1000, stoppingToken);
      }
    }
  }
}
