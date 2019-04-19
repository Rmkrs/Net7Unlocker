namespace Net7MultiClientUnlocker.Framework.Win32
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct GenericMapping
    {
        public int GenericRead;

        public int GenericWrite;

        public int GenericExecute;

        public int GenericAll;
    }
}