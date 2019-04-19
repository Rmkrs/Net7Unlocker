namespace Net7MultiClientUnlocker.Framework.Win32
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct ObjectBasicInformation
    {
        public int Attributes;

        public int GrantedAccess;

        public int HandleCount;

        public int PointerCount;

        public int PagedPoolUsage;

        public int NonPagedPoolUsage;

        public int Reserved1;

        public int Reserved2;

        public int Reserved3;

        public int NameInformationLength;

        public int TypeInformationLength;

        public int SecurityDescriptorLength;

        public System.Runtime.InteropServices.ComTypes.FILETIME CreateTime;
    }
}