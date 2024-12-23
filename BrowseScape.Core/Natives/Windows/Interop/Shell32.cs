using System;
using System.Runtime.InteropServices;

namespace BrowseScape.Core.Natives.Windows.Interop
{
  internal static class Shell32
  {
    
    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    internal static extern unsafe IntPtr ExtractAssociatedIcon(HandleRef hInst, char* iconPath, ref int index);

  }
}
