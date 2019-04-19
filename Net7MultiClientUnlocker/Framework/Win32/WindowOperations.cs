namespace Net7MultiClientUnlocker.Framework.Win32
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Text;

    public static class WindowOperations
    {
        public static bool FlashWindowEx(IntPtr windowHandle, UInt32 repeat)
        {
            var flashWindowInfo = new FlashWindowInfo();
            flashWindowInfo.Size = Convert.ToUInt32(Marshal.SizeOf(flashWindowInfo));
            flashWindowInfo.WindowHandle = windowHandle;
            flashWindowInfo.Flags = Win32Constants.FlashWindowAll;
            flashWindowInfo.Count = repeat; // UInt32.MaxValue;
            flashWindowInfo.Timeout = 0;

            return Win32Api.FlashWindowEx(ref flashWindowInfo);
        }

        public static Rect GetWindowRectangle(IntPtr windowHandle)
        {
            var rectangle = new Rect();
            Win32Api.GetWindowRect(windowHandle, ref rectangle);
            return rectangle;
        }

        public static bool MoveClientWindow(IntPtr windowHandle, Rect rectangle)
        {
            // return MoveWindow(WindowHandle, rectangle.Left, rectangle.Top, rectangle.Right - rectangle.Left, rectangle.Bottom - rectangle.Top, true);
            return Win32Api.SetWindowPos(windowHandle, 0, rectangle.Left, rectangle.Top, -1, -1, Win32Constants.SWP_NOSIZE | Win32Constants.SWP_NOZORDER | Win32Constants.SWP_FRAMECHANGED);
        }

        public static bool MoveClientWindow(IntPtr windowHandle, int left, int top)
        {
            return Win32Api.SetWindowPos(windowHandle, 0, left, top, 0, 0, Win32Constants.SWP_NOSIZE | Win32Constants.SWP_NOZORDER | Win32Constants.SWP_FRAMECHANGED);
        }

        public static void AcceptTos(IntPtr windowHandle)
        {
            var buttonHandle = Win32Api.FindWindowEx(windowHandle, IntPtr.Zero, "Button", "I Agree");
            Win32Api.SendMessage(buttonHandle, Win32Constants.BM_CLICK, IntPtr.Zero, IntPtr.Zero);
        }

        public static string GetWindowContent(IntPtr windowHandle)
        {
            var length = Win32Api.GetWindowTextLength(windowHandle);
            var sb = new StringBuilder(length + 1);
            Win32Api.GetWindowText(windowHandle, sb, sb.Capacity);
            return sb.ToString();
        }

        public static void SendKeysToClientProcess(IntPtr windowHandle, string keys)
        {
            Win32Api.SendMessage(windowHandle, Win32Constants.WM_KEYDOWN, 0, keys);
        }

        public static IntPtr SetFocus(IntPtr windowHandle)
        {
            return Win32Api.SetFocus(windowHandle);
        }

        public static void PostEscapeMessageToClientProcess(IntPtr windowHandle)
        {
            // SendMessage(windowHandle, BM_CLICK, IntPtr.Zero, IntPtr.Zero);
            Win32Api.PostMessage(windowHandle, Win32Constants.WM_KEYDOWN, (IntPtr)Win32Constants.VK_ESCAPE, 1);
        }

        public static void PostString(IntPtr windowHandle, string text)
        {
            var tmpText = text.ToCharArray();
            foreach (char c in tmpText)
            {
                Win32Api.PostMessage(windowHandle, Win32Constants.WM_CHAR, (IntPtr)c, 0);
            }
        }

        public static void EnableResize(IntPtr windowHandle)
        {
            const int Style = (int)
                              (Win32Constants.WS_CAPTION
                               | Win32Constants.WS_VISIBLE
                               | Win32Constants.WS_CLIPSIBLINGS
                               | Win32Constants.WS_CLIPCHILDREN
                               | Win32Constants.WS_SYSMENU
                               | Win32Constants.WS_THICKFRAME
                               | Win32Constants.WS_OVERLAPPED
                               | Win32Constants.WS_MINIMIZEBOX
                               | Win32Constants.WS_MAXIMIZEBOX
                               | Win32Constants.WS_EX_LEFT
                               | Win32Constants.WS_EX_LTRREADING
                               | Win32Constants.WS_EX_RIGHTSCROLLBAR
                               | Win32Constants.WS_EX_WINDOWEDGE
                               | Win32Constants.WS_THICKFRAME
                               | Win32Constants.WS_MAXIMIZEBOX
                               | Win32Constants.WS_MAXIMIZE
                               | Win32Constants.WS_EX_CONTROLPARENT
                               | Win32Constants.WS_EX_APPWINDOW 
                               | Win32Constants.DS_3DLOOK
                               | Win32Constants.DS_MODALFRAME); // Adds the sizing border

            Win32Api.SetWindowLong(windowHandle, Win32Constants.GWL_STYLE, Style);
            Win32Api.SetWindowPos(windowHandle, -1, 0, 0, 0, 0, Win32Constants.SWP_NOACTIVATE | Win32Constants.SWP_FRAMECHANGED | Win32Constants.SWP_NOMOVE | Win32Constants.SWP_NOSIZE | Win32Constants.SWP_NOZORDER | Win32Constants.SWP_NOREPOSITION | Win32Constants.SWP_NOREDRAW);
        }

        public static string GetWindowText(IntPtr hwnd)
        {
            var size = Win32Api.GetWindowTextLength(hwnd);
            if (size <= 0)
            {
                return String.Empty;
            }

            var builder = new StringBuilder(size + 1);
            Win32Api.GetWindowText(hwnd, builder, builder.Capacity);
            return builder.ToString();
        }

        public static bool SetWindowText(IntPtr hwnd, string text)
        {
            return Win32Api.SetWindowText(hwnd, text);
        }

        public static IEnumerable<IntPtr> FindWindows(Win32Api.EnumWindowsProc filter)
        {
            var windows = new List<IntPtr>();
            Win32Api.EnumWindows(
                delegate(IntPtr wnd, IntPtr param)
                    {
                        if (filter(wnd, param))
                        {
                            // only add the windows that pass the filter
                            windows.Add(wnd);
                        }

                        // but return true here so that we iterate all windows
                        return true;
                    }, 
                IntPtr.Zero);
            return windows;
        }

        public static IEnumerable<IntPtr> EnumerateProcessWindowHandles(int processId)
        {
            var handles = new List<IntPtr>();

            foreach (ProcessThread thread in Process.GetProcessById(processId).Threads)
            {
                Win32Api.EnumThreadWindows(thread.Id, (hWnd, lParam) => { handles.Add(hWnd); return true; }, IntPtr.Zero);
            }

            return handles;
        }

        public static bool IswindowVisible(IntPtr hwnd)
        {
            return Win32Api.IsWindowVisible(hwnd);
        }

        public static IEnumerable<IntPtr> FindWindowsWithText(string titleText)
        {
            return FindWindows((wnd, param) => GetWindowText(wnd).Contains(titleText));
        }

        public static IntPtr FindWindowWithTextForProccessId(string titleText, int processId)
        {
            var windowsWithTitle = FindWindowsWithText(titleText);
            foreach (var windowHandle in windowsWithTitle)
            {
                uint windowProcessId;
                Win32Api.GetWindowThreadProcessId(windowHandle, out windowProcessId);
                if (windowProcessId == processId)
                {
                    return windowHandle;
                }
            }

            return IntPtr.Zero;
        }

        public static IntPtr FindENBWindow(int processId)
        {
            return FindWindowWithTextForProccessId("Earth & Beyond", processId);
        }

        public static bool IsTOSWindowDisplayed(int processId)
        {
            var windowHandle = FindENBWindow(processId);
            if (windowHandle == IntPtr.Zero)
            {
                return false;
            }

            var buttonHandle = Win32Api.FindWindowEx(windowHandle, IntPtr.Zero, "Button", "I Agree");
            return buttonHandle != IntPtr.Zero;
        }

        public static long GetWindowStyles(IntPtr windowHandle)
        {
            return Win32Api.GetWindowLong(windowHandle, -21);
        }

        public static void LauncherPlayButton(IntPtr windowHandle)
        {
            var buttonHandle = Win32Api.FindWindowEx(windowHandle, IntPtr.Zero, "WindowsForms10.BUTTON.app.0.2004eee", "&Play");
            Win32Api.SendMessage(buttonHandle, Win32Constants.BM_CLICK, IntPtr.Zero, IntPtr.Zero);
        }

        public static bool IsLaucherPlayButtonDisplayed(IntPtr windowHandle)
        {
            var buttonHandle = Win32Api.FindWindowEx(windowHandle, IntPtr.Zero, "WindowsForms10.BUTTON.app.0.2004eee", "&Play");
            return buttonHandle != IntPtr.Zero;
        }
    }
}
