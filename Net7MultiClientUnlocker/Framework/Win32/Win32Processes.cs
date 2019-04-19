namespace Net7MultiClientUnlocker.Framework.Win32
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    public class Win32Processes
    {
        private const int CnstSystemHandleInformation = 16;
        private const uint StatusInfoLengthMismatch = 0xc0000004;

        public static string GetObjectTypeName(SystemHandleInformation systemHandle, Process process)
        {
            var processHandle = Win32Api.OpenProcess(ProcessAccessFlags.All, false, process.Id);
            IntPtr pointerHandle;
            var basicInformation = new ObjectBasicInformation();
            var objObjectType = new ObjectTypeInformation();
            var informationLength = 0;

            if (!Win32Api.DuplicateHandle(
                    processHandle,
                    systemHandle.Handle,
                    Win32Api.GetCurrentProcess(),
                    out pointerHandle,
                    0,
                    false,
                    Win32Constants.DuplicateSameAccess))
            {
                return null;
            }

            var basicInformationPointer = Marshal.AllocHGlobal(Marshal.SizeOf(basicInformation));
            Win32Api.NtQueryObject(pointerHandle, (int)ObjectInformationClassType.ObjectBasicInformation, basicInformationPointer, Marshal.SizeOf(basicInformation), ref informationLength);
            basicInformation = (ObjectBasicInformation)Marshal.PtrToStructure(basicInformationPointer, basicInformation.GetType());
            Marshal.FreeHGlobal(basicInformationPointer);

            var basicInformationTypeInformationPointer = Marshal.AllocHGlobal(basicInformation.TypeInformationLength);
            informationLength = basicInformation.TypeInformationLength;
            while ((uint)Win32Api.NtQueryObject(pointerHandle, (int)ObjectInformationClassType.ObjectTypeInformation, basicInformationTypeInformationPointer, informationLength, ref informationLength) == Win32Constants.StatusInfoLengthMismatch)
            {
                Marshal.FreeHGlobal(basicInformationTypeInformationPointer);
                basicInformationTypeInformationPointer = Marshal.AllocHGlobal(informationLength);
            }

            objObjectType = (ObjectTypeInformation)Marshal.PtrToStructure(basicInformationTypeInformationPointer, objObjectType.GetType());
            var objectNameBuffer = Is64Bits() ? new IntPtr(Convert.ToInt64(objObjectType.Name.Buffer.ToString(), 10) >> 32) : objObjectType.Name.Buffer;

            var strObjectTypeName = Marshal.PtrToStringUni(objectNameBuffer, objObjectType.Name.Length >> 1);
            Marshal.FreeHGlobal(basicInformationTypeInformationPointer);
            return strObjectTypeName;
        }

        public static string GetObjectName(SystemHandleInformation systemHandle, Process process)
        {
            var processHandlePointer = Win32Api.OpenProcess(ProcessAccessFlags.All, false, process.Id);
            IntPtr objectBasicInformationPointer;
            var objBasic = new ObjectBasicInformation();
            var objObjectName = new ObjectNameInformation();
            var nameInformationLength = 0;

            if (!Win32Api.DuplicateHandle(
                    processHandlePointer,
                    systemHandle.Handle,
                    Win32Api.GetCurrentProcess(),
                    out objectBasicInformationPointer,
                    0,
                    false,
                    Win32Constants.DuplicateSameAccess))
            {
                return null;
            }

            var basicObjectPointer = Marshal.AllocHGlobal(Marshal.SizeOf(objBasic));
            Win32Api.NtQueryObject(objectBasicInformationPointer, (int)ObjectInformationClassType.ObjectBasicInformation, basicObjectPointer, Marshal.SizeOf(objBasic), ref nameInformationLength);
            objBasic = (ObjectBasicInformation)Marshal.PtrToStructure(basicObjectPointer, objBasic.GetType());
            Marshal.FreeHGlobal(basicObjectPointer);

            nameInformationLength = objBasic.NameInformationLength;

            var nameInformationLengthPointer = Marshal.AllocHGlobal(nameInformationLength);
            while ((uint)Win32Api.NtQueryObject(objectBasicInformationPointer, (int)ObjectInformationClassType.ObjectNameInformation, nameInformationLengthPointer, nameInformationLength, ref nameInformationLength) == Win32Constants.StatusInfoLengthMismatch)
            {
                Marshal.FreeHGlobal(nameInformationLengthPointer);
                nameInformationLengthPointer = Marshal.AllocHGlobal(nameInformationLength);
            }

            objObjectName = (ObjectNameInformation)Marshal.PtrToStructure(nameInformationLengthPointer, objObjectName.GetType());

            var objectNameBufferPointer = Is64Bits() ? new IntPtr(Convert.ToInt64(objObjectName.Name.Buffer.ToString(), 10) >> 32) : objObjectName.Name.Buffer;
            if (objectNameBufferPointer == IntPtr.Zero)
            {
                return null;
            }

            var objectNameBuffer = new byte[nameInformationLength];
            try
            {
                Marshal.Copy(objectNameBufferPointer, objectNameBuffer, 0, nameInformationLength);

                string strObjectName = Marshal.PtrToStringUni(Is64Bits() ? new IntPtr(objectNameBufferPointer.ToInt64()) : new IntPtr(objectNameBufferPointer.ToInt32()));
                return strObjectName;
            }
            catch (AccessViolationException)
            {
                return null;
            }
            finally
            {
                Marshal.FreeHGlobal(nameInformationLengthPointer);
                Win32Api.CloseHandle(objectBasicInformationPointer);
            }
        }

        public static List<SystemHandleInformation> GetHandles(Process process = null, string objectTypeName = null, string ojectName = null)
        {
            var handleInfoSize = 0x10000;
            var handlePointer = Marshal.AllocHGlobal(handleInfoSize);
            var handleSizeInfo = 0;

            while (Win32Api.NtQuerySystemInformation(CnstSystemHandleInformation, handlePointer, handleInfoSize, ref handleSizeInfo) == StatusInfoLengthMismatch)
            {
                handleInfoSize = handleSizeInfo;
                Marshal.FreeHGlobal(handlePointer);
                handlePointer = Marshal.AllocHGlobal(handleSizeInfo);
            }

            var handleSizeInfoBuffer = new byte[handleSizeInfo];
            Marshal.Copy(handlePointer, handleSizeInfoBuffer, 0, handleSizeInfo);

            long handleCount;
            IntPtr handleSizeInfoPointer;
            if (Is64Bits())
            {
                handleCount = Marshal.ReadInt64(handlePointer);
                handleSizeInfoPointer = new IntPtr(handlePointer.ToInt64() + 8);
            }
            else
            {
                handleCount = Marshal.ReadInt32(handlePointer);
                handleSizeInfoPointer = new IntPtr(handlePointer.ToInt32() + 4);
            }

            var lstHandles = new List<SystemHandleInformation>();

            for (long index = 0; index < handleCount; index++)
            {
                var systemHandleInformation = new SystemHandleInformation();
                if (Is64Bits())
                {
                    systemHandleInformation = (SystemHandleInformation)Marshal.PtrToStructure(handleSizeInfoPointer, systemHandleInformation.GetType());
                    handleSizeInfoPointer = new IntPtr(handleSizeInfoPointer.ToInt64() + Marshal.SizeOf(systemHandleInformation) + 8);
                }
                else
                {
                    handleSizeInfoPointer = new IntPtr(handleSizeInfoPointer.ToInt64() + Marshal.SizeOf(systemHandleInformation));
                    systemHandleInformation = (SystemHandleInformation)Marshal.PtrToStructure(handleSizeInfoPointer, systemHandleInformation.GetType());
                }

                if (process != null)
                {
                    if (systemHandleInformation.ProcessID != process.Id)
                    {
                        continue;
                    }
                }

                if (objectTypeName != null)
                {
                    var strObjectTypeName = GetObjectTypeName(systemHandleInformation, Process.GetProcessById(systemHandleInformation.ProcessID));
                    if (strObjectTypeName != objectTypeName)
                    {
                        continue;
                    }
                }

                if (ojectName != null)
                {
                    var strObjectName = GetObjectName(systemHandleInformation, Process.GetProcessById(systemHandleInformation.ProcessID));
                    if (strObjectName != ojectName)
                    {
                        continue;
                    }
                }

                var strObjectName2 = GetObjectName(systemHandleInformation, Process.GetProcessById(systemHandleInformation.ProcessID));
                if (strObjectName2 != null && (strObjectName2 == @"\Sessions\1\BaseNamedObjects\enb_mutex_lock" || strObjectName2.Length == 31))
                {
                    lstHandles.Add(systemHandleInformation);
                }
            }

            return lstHandles;
        }

        public static bool Is64Bits()
        {
            return Marshal.SizeOf(typeof(IntPtr)) == 8;
        }
    }
}
