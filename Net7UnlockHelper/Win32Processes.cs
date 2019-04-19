﻿namespace Net7UnlockHelper
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    public class Win32Processes
    {
        private const int CnstSystemHandleInformation = 16;
        private const uint StatusInfoLengthMismatch = 0xc0000004;

        public static string GetObjectTypeName(Win32API.SYSTEM_HANDLE_INFORMATION systemHandle, Process process)
        {
            var processHandle = Win32API.OpenProcess(Win32API.ProcessAccessFlags.All, false, process.Id);
            // ReSharper disable once InlineOutVariableDeclaration
            IntPtr pointerHandle;
            var basicInformation = new Win32API.OBJECT_BASIC_INFORMATION();
            var objObjectType = new Win32API.OBJECT_TYPE_INFORMATION();
            var informationLength = 0;

            if (!Win32API.DuplicateHandle(
                    processHandle,
                    systemHandle.Handle,
                    Win32API.GetCurrentProcess(),
                    out pointerHandle,
                    0,
                    false,
                    Win32API.DUPLICATE_SAME_ACCESS))
            {
                return null;
            }

            var basicInformationPointer = Marshal.AllocHGlobal(Marshal.SizeOf(basicInformation));
            Win32API.NtQueryObject(pointerHandle, (int)Win32API.ObjectInformationClass.ObjectBasicInformation, basicInformationPointer, Marshal.SizeOf(basicInformation), ref informationLength);
            basicInformation = (Win32API.OBJECT_BASIC_INFORMATION)Marshal.PtrToStructure(basicInformationPointer, basicInformation.GetType());
            Marshal.FreeHGlobal(basicInformationPointer);

            var basicInformationTypeInformationPointer = Marshal.AllocHGlobal(basicInformation.TypeInformationLength);
            informationLength = basicInformation.TypeInformationLength;
            while ((uint)Win32API.NtQueryObject(pointerHandle, (int)Win32API.ObjectInformationClass.ObjectTypeInformation, basicInformationTypeInformationPointer, informationLength, ref informationLength) == Win32API.STATUS_INFO_LENGTH_MISMATCH)
            {
                Marshal.FreeHGlobal(basicInformationTypeInformationPointer);
                basicInformationTypeInformationPointer = Marshal.AllocHGlobal(informationLength);
            }

            objObjectType = (Win32API.OBJECT_TYPE_INFORMATION)Marshal.PtrToStructure(basicInformationTypeInformationPointer, objObjectType.GetType());
            var objectNameBuffer = Is64Bits() ? new IntPtr(Convert.ToInt64(objObjectType.Name.Buffer.ToString(), 10) >> 32) : objObjectType.Name.Buffer;

            var strObjectTypeName = Marshal.PtrToStringUni(objectNameBuffer, objObjectType.Name.Length >> 1);
            Marshal.FreeHGlobal(basicInformationTypeInformationPointer);
            return strObjectTypeName;
        }

        public static string GetObjectName(Win32API.SYSTEM_HANDLE_INFORMATION systemHandle, Process process)
        {
            var processHandlePointer = Win32API.OpenProcess(Win32API.ProcessAccessFlags.All, false, process.Id);
            // ReSharper disable once InlineOutVariableDeclaration
            IntPtr objectBasicInformationPointer;
            var objBasic = new Win32API.OBJECT_BASIC_INFORMATION();
            var objObjectName = new Win32API.OBJECT_NAME_INFORMATION();
            var nameInformationLength = 0;

            if (!Win32API.DuplicateHandle(
                    processHandlePointer,
                    systemHandle.Handle,
                    Win32API.GetCurrentProcess(),
                    out objectBasicInformationPointer,
                    0,
                    false,
                    Win32API.DUPLICATE_SAME_ACCESS))
            {
                return null;
            }

            var basicObjectPointer = Marshal.AllocHGlobal(Marshal.SizeOf(objBasic));
            Win32API.NtQueryObject(objectBasicInformationPointer, (int)Win32API.ObjectInformationClass.ObjectBasicInformation, basicObjectPointer, Marshal.SizeOf(objBasic), ref nameInformationLength);
            objBasic = (Win32API.OBJECT_BASIC_INFORMATION)Marshal.PtrToStructure(basicObjectPointer, objBasic.GetType());
            Marshal.FreeHGlobal(basicObjectPointer);

            nameInformationLength = objBasic.NameInformationLength;

            var nameInformationLengthPointer = Marshal.AllocHGlobal(nameInformationLength);
            while ((uint)Win32API.NtQueryObject(objectBasicInformationPointer, (int)Win32API.ObjectInformationClass.ObjectNameInformation, nameInformationLengthPointer, nameInformationLength, ref nameInformationLength) == Win32API.STATUS_INFO_LENGTH_MISMATCH)
            {
                Marshal.FreeHGlobal(nameInformationLengthPointer);
                nameInformationLengthPointer = Marshal.AllocHGlobal(nameInformationLength);
            }

            objObjectName = (Win32API.OBJECT_NAME_INFORMATION)Marshal.PtrToStructure(nameInformationLengthPointer, objObjectName.GetType());

            var objectNameBufferPointer = Is64Bits() ? new IntPtr(Convert.ToInt64(objObjectName.Name.Buffer.ToString(), 10) >> 32) : objObjectName.Name.Buffer;

            if (objectNameBufferPointer == IntPtr.Zero)
            {
                return null;
            }

            var objectNameBuffer = new byte[nameInformationLength];
            try
            {
                Marshal.Copy(objectNameBufferPointer, objectNameBuffer, 0, nameInformationLength);

                var strObjectName = Marshal.PtrToStringUni(Is64Bits() ? new IntPtr(objectNameBufferPointer.ToInt64()) : new IntPtr(objectNameBufferPointer.ToInt32()));
                return strObjectName;
            }
            catch (AccessViolationException)
            {
                return null;
            }
            finally
            {
                Marshal.FreeHGlobal(nameInformationLengthPointer);
                Win32API.CloseHandle(objectBasicInformationPointer);
            }
        }

        public static List<Win32API.SYSTEM_HANDLE_INFORMATION> GetHandles(Process process = null, string objectTypeName = null, string ojectName = null)
        {
            var handleInfoSize = 0x10000;
            var handlePointer = Marshal.AllocHGlobal(handleInfoSize);
            var handleSizeInfo = 0;

            while (Win32API.NtQuerySystemInformation(CnstSystemHandleInformation, handlePointer, handleInfoSize, ref handleSizeInfo) == StatusInfoLengthMismatch)
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

            var lstHandles = new List<Win32API.SYSTEM_HANDLE_INFORMATION>();

            for (long index = 0; index < handleCount; index++)
            {
                var systemHandleInformation = new Win32API.SYSTEM_HANDLE_INFORMATION();
                if (Is64Bits())
                {
                    systemHandleInformation = (Win32API.SYSTEM_HANDLE_INFORMATION)Marshal.PtrToStructure(handleSizeInfoPointer, systemHandleInformation.GetType());
                    handleSizeInfoPointer = new IntPtr(handleSizeInfoPointer.ToInt64() + Marshal.SizeOf(systemHandleInformation) + 8);
                }
                else
                {
                    handleSizeInfoPointer = new IntPtr(handleSizeInfoPointer.ToInt64() + Marshal.SizeOf(systemHandleInformation));
                    systemHandleInformation = (Win32API.SYSTEM_HANDLE_INFORMATION)Marshal.PtrToStructure(handleSizeInfoPointer, systemHandleInformation.GetType());
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
