using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Avalonia;
using Avalonia.Controls;
using BrowseScape.Core.Interfaces;

namespace BrowseScape.Core.Natives
{
  
  [SupportedOSPlatform("windows")]
  public class Windows : IBackend
  {
    [StructLayout(LayoutKind.Sequential)]
    private struct RTL_OSVERSIONINFOEX
    {
      internal uint dwOSVersionInfoSize;
      internal uint dwMajorVersion;
      internal uint dwMinorVersion;
      internal uint dwBuildNumber;
      internal uint dwPlatformId;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
      internal string szCSDVersion;
    }
    
    [DllImport("ntdll")]
    private static extern int RtlGetVersion(ref RTL_OSVERSIONINFOEX lpVersionInformation);
    
    private void FixWindowFrameOnWin10(Window w)
    {
      switch (w.WindowState)
      {
        case WindowState.Maximized:
        case WindowState.FullScreen:
          w.SystemDecorations = SystemDecorations.Full;
          break;
        case WindowState.Normal:
          w.SystemDecorations = SystemDecorations.BorderOnly;
          break;
      }
    }
    public void SetupApp(AppBuilder builder)
    {
      // Fix drop shadow issue on Windows 10
      var v = new RTL_OSVERSIONINFOEX { dwOSVersionInfoSize = (uint)Marshal.SizeOf<RTL_OSVERSIONINFOEX>() };
      if (RtlGetVersion(ref v) != 0 || (v.dwMajorVersion >= 10 && v.dwBuildNumber >= 22000))
      {
        return;
      }
      Window.WindowStateProperty.Changed.AddClassHandler<Window>((w, _) => FixWindowFrameOnWin10(w));
      Control.LoadedEvent.AddClassHandler<Window>((w, _) => FixWindowFrameOnWin10(w));
    }

  }
}
