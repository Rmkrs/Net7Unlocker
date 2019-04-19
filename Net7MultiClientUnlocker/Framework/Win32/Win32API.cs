namespace Net7MultiClientUnlocker.Framework.Win32
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    public static class Win32Api
    {
        public delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lparam);

        public delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);

        [DllImport("ntdll.dll")]
        public static extern int NtQueryObject(IntPtr objectHandle, int objectInformationClass, IntPtr objectInformation, int objectInformationLength, ref int returnLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint QueryDosDevice(string deviceName, StringBuilder targetPath, int maximumBufferLength);

        [DllImport("ntdll.dll")]
        public static extern uint NtQuerySystemInformation(int systemInformationClass, IntPtr systemInformation, int systemInformationLength, ref int returnLength);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr OpenMutex(UInt32 desiredAccess, bool inheritHandle, string name);

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(ProcessAccessFlags desiredAccess, [MarshalAs(UnmanagedType.Bool)] bool inheritHandle, int processId);
        
        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr objectHandle);
        
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DuplicateHandle(IntPtr sourceProcessHandle, ushort sourceHandle, IntPtr targetProcessHandle, out IntPtr targetHandle, uint desiredAccess, [MarshalAs(UnmanagedType.Bool)] bool inheritHandle, uint options);
        
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetCurrentProcess();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FlashWindowEx(ref FlashWindowInfo pwfi);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hwnd, ref Rect rect);

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern bool SetWindowPos(IntPtr hwnd, int hwndInsertAfter, int x, int y, int cx, int cy, UInt32 flags);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool MoveWindow(IntPtr hwnd, int x, int y, int width, int height, bool repaint);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hwnd, UInt32 msg, IntPtr wparam, IntPtr lparam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hwnd, StringBuilder lpstring, int maxCount);

        [DllImport("user32.dll")]
        public static extern bool SetWindowText(IntPtr hwnd, string text);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetWindowTextLength(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hwnd);

        [DllImport("User32.dll")]
        public static extern int SendMessage(IntPtr hwnd, uint msg, uint wparam, string lparam);

        [DllImport("User32.Dll")]
        public static extern Int32 PostMessage(IntPtr hwnd, uint msg, IntPtr wparam, int lparam);
        
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int SetWindowLong(IntPtr hwnd, int index, int newLong);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lparam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint processId);

        [DllImport("user32.dll")]
        public static extern IntPtr SetFocus(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern bool EnumThreadWindows(int threadId, Win32Api.EnumThreadDelegate threadDelegate, IntPtr lparam);
    }
}
