namespace Net7MultiClientUnlocker.Framework.Win32
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct FlashWindowInfo
    {
        public UInt32 Size;

        public IntPtr WindowHandle;

        public UInt32 Flags;

        public UInt32 Count;

        public UInt32 Timeout;
    }
}