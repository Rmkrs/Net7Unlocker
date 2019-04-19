namespace Net7MultiClientUnlocker.Framework.Win32
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SystemHandleInformation
    {
        public int ProcessID;

        public byte ObjectTypeNumber;

        public byte Flags;

        public ushort Handle;

        public int ObjectPointer;

        public UInt32 GrantedAccess;
    }
}