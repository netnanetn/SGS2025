using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SGS2025Client.Shared
{
    public partial class CameraHostView : ContentView
    {
        public IntPtr Hwnd { get; internal set; } = IntPtr.Zero;
    }
}
