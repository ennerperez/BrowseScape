using System;
using System.Reflection;
using System.Runtime.InteropServices;
using MonoMac.CoreGraphics;
using MonoMac.Foundation;

namespace BrowseScape.Core.Natives.MacOS.Interop
{
  internal static class QuartzCore
  {

    internal class kCGWindowBounds
    {
      internal uint Height;
      internal uint Width;
      internal uint X;
      internal uint Y;
      public override string ToString() => $"{{{Width}, {Height}, {X}, {Y}}}";
    }
    
    internal class kCGWindow
    {
      internal uint kCGWindowAlpha;
      internal kCGWindowBounds kCGWindowBounds;
      internal uint kCGWindowIsOnscreen;
      internal int kCGWindowLayer;
      internal uint kCGWindowMemoryUsage;
      internal uint kCGWindowNumber;
      internal string kCGWindowOwnerName;
      internal uint kCGWindowOwnerPID;
      internal uint kCGWindowSharingState;
      internal uint kCGWindowStoreType;

      public override string ToString() => $"{kCGWindowOwnerName} ({kCGWindowOwnerPID})";

      internal void Read(NSObject source)
      {
        var props = this.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var prop in props)
        {
          var key = new NSString(prop.Name);
          var value = source.ValueForKey(key);
          if (prop.FieldType == typeof(kCGWindowBounds))
          {
            var innerProps = prop.FieldType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            var innerVal = new kCGWindowBounds();
            foreach (var iprop in innerProps)
            {
              var ikey = new NSString(iprop.Name);
              var ival = value.ValueForKey(ikey);
              if (iprop.FieldType == typeof(string))
              {
                iprop.SetValue(innerVal, ival.Description);
              }
              else if (iprop.FieldType == typeof(uint))
              {
                iprop.SetValue(innerVal, uint.Parse(ival.Description));
              }
              else if (iprop.FieldType == typeof(int))
              {
                iprop.SetValue(innerVal, int.Parse(ival.Description));
              }
            }
            prop.SetValue(this, innerVal);
          }
          else if (prop.FieldType == typeof(string))
          {
            prop.SetValue(this, value.Description);
          }
          else if (prop.FieldType == typeof(uint))
          {
            prop.SetValue(this, uint.Parse(value.Description));
          }
          else if (prop.FieldType == typeof(int))
          {
            prop.SetValue(this, int.Parse(value.Description));
          }
          
        }
      }
      
    }
    
    [DllImport(@"/System/Library/Frameworks/QuartzCore.framework/QuartzCore")]
    internal static extern IntPtr CGWindowListCopyWindowInfo(CGWindowListOption option, uint relativeToWindow);
  }
}
