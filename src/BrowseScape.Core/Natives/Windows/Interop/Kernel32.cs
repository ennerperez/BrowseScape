using System.Runtime.InteropServices;

namespace BrowseScape.Core.Natives.Windows.Interop
{
  public static class Kernel32
  {
    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    public static extern nint GetModuleHandle(string? lpModuleName);

    // ReSharper disable once InconsistentNaming
    private const uint ATTACH_PARENT_PROCESS = 0x0ffffffff;

    [DllImport("kernel32.dll")]
    private static extern bool AttachConsole(uint dwProcessId);

    public static void AttachToParentConsole() => AttachConsole(ATTACH_PARENT_PROCESS);
  }
}
