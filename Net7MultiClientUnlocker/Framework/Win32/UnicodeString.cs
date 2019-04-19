namespace Net7MultiClientUnlocker.Framework.Win32
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct UnicodeString
    {
        public ushort Length;

        public ushort MaximumLength;

        public IntPtr Buffer;
    }
}