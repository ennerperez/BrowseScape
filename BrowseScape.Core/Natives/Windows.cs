using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
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

    [DllImport("ntdll")] private static extern int RtlGetVersion(ref RTL_OSVERSIONINFOEX lpVersionInformation);
    
    [DllImport("user32.dll")] private static extern nint GetForegroundWindow();

    [DllImport("user32.dll")] private static extern int GetWindowText(nint hWnd, StringBuilder text, int count);

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


    public string GetActiveWindowTitle()
    {
      var result = string.Empty;
      const int NChars = 256;
      var buff = new StringBuilder(NChars);
      var handle = GetForegroundWindow();

      if (GetWindowText(handle, buff, NChars) > 0)
      {
        result = buff.ToString();
      }
      return result;
    }
    public Task RegisterAsync()
    {
      throw new System.NotImplementedException();
    }
    public Task UnregisterAsync()
    {
      throw new System.NotImplementedException();
    }

  }
}
