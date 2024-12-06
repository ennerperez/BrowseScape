using System.Runtime.InteropServices;
using System.Text;

namespace BrowseScape.Core.Natives.Windows.Interop
{
  internal static class User32
  {
    [DllImport("user32.dll")] internal static extern nint GetForegroundWindow();
    [DllImport("user32.dll")] internal static extern int GetWindowText(nint hWnd, StringBuilder text, int count);
  }
}
