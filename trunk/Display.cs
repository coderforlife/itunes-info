//  iTunesInfo: Displays iTunes information and uses system keyboard shortcuts to control iTunes
//  Copyright (C) 2011  Jeffrey Bush <jeff@coderforlife.com>
//
//  This file is part of iTunesInfo
//
//  iTunesInfo is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  iTunesInfo is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with iTunesInfo. If not, see <http://www.gnu.org/licenses/>.


using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace iTunesInfo
{
    /// <summary>An enumeration of possible positions on the desktop</summary>
    enum DesktopPos
    {
        /// <summary>The position on the desktop that is closest to the clock of the taskbar</summary>
        NearClock,
        UpperLeft, UpperRight, LowerRight, LowerLeft
    }

    /// <summary>The position of the taskbar on the screen</summary>
    /// <remarks>Values forced to comply with APPBARDATA (http://msdn.microsoft.com/library/bb773184.aspx)</remarks>
    enum TaskbarPos : uint { Left = 0, Top, Right, Bottom }

    /// <summary>A rectangle object that is equivalent and compatible with the WinAPI RECT struct, used for numerous WinAPIs</summary>
    /// <remarks>http://msdn.microsoft.com/library/dd162897.aspx</remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT {
        public int left, top, right, bottom;
        public RECT(int left, int top, int right, int bottom) { this.left = left; this.top = top; this.right = right; this.bottom = bottom; }
        public override string ToString() { return "{left=" + left + ",top=" + top + ",right=" + right + ",bottom=" + bottom + "}"; }
    }

    /// <summary>A static utility class for interacting with the display / screen</summary>
    static class Display
    {

        #region Windows API

        /// <summary>Appbar Message Values, used with SHAppBarMessage</summary>
        /// <remarks>http://msdn.microsoft.com/library/bb762108.aspx</remarks>
        private enum ABM : uint
        {
            New = 0x00000000,
            Remove = 0x00000001,
            QueryPos = 0x00000002,
            SetPos = 0x00000003,
            GetState = 0x00000004,
            GetTaskbarPos = 0x00000005,
            Activate = 0x00000006,
            GetAutoHideBar = 0x00000007,
            SetAutoHideBar = 0x00000008,
            WindowPosChanged = 0x00000009,
            SetState = 0x0000000A,
        }

        /// <summary>Contains information about a system appbar message. This structure is used with the SHAppBarMessage function.</summary>
        /// <remarks>http://msdn.microsoft.com/library/bb773184.aspx</remarks>
        [StructLayout(LayoutKind.Sequential)]
        private struct APPBARDATA
        {
            /// <summary>The size of the structure, in bytes</summary>
            public uint cbSize;
            /// <summary>The handle to the appbar window</summary>
            public IntPtr hWnd;
            /// <summary>An application-defined message identifier. The application uses the specified identifier for notification messages that it sends to the appbar identified by the hWnd member. This member is used when sending the ABM_NEW message.</summary>
            public uint uCallbackMessage;
            /// <summary>A value that specifies an edge of the screen. This member is used when sending the ABM_GETAUTOHIDEBAR, ABM_QUERYPOS, ABM_SETAUTOHIDEBAR, and ABM_SETPOS messages.</summary>
            public TaskbarPos uEdge;
            /// <summary>A RECT structure to contain the bounding rectangle, in screen coordinates, of an appbar or the Windows taskbar. This member is used when sending the ABM_GETTASKBARPOS, ABM_QUERYPOS, and ABM_SETPOS messages.</summary>
            public RECT rc;
            /// <summary>A message-dependent value. This member is used with the ABM_SETAUTOHIDEBAR and ABM_SETSTATE messages.</summary>
            public int lParam;
        }

        /// <summary>Sends an appbar message to the system</summary>
        /// <remarks>http://msdn.microsoft.com/library/bb762108.aspx</remarks>
        /// <param name="dwMessage">Appbar message value to send, this parameter can be one of the values in the Display.ABM enum</param>
        /// <param name="pData">The appbar message data, the content of the structure on entry and on exit depends on the value set in the dwMessage parameter</param>
        /// <returns>This function returns a message-dependent value. For more information, see the Windows SDK documentation for the specific appbar message sent.</returns>
        [DllImport("shell32.dll", SetLastError = true)] private static extern IntPtr SHAppBarMessage(ABM dwMessage, [In] ref APPBARDATA pData);

        /// <summary>The class name of the Windows Taskbar</summary>
        private const string TaskbarClassName = "Shell_TrayWnd";

        /// <summary>Displays a window in its most recent size and position. This value is similar to SW_SHOWNORMAL, except that the window is not activated.</summary>
        /// <remarks>http://msdn.microsoft.com/library/ms633548.aspx</remarks>
        private const int SW_SHOWNOACTIVATE = 4;
        /// <summary>Does not activate the window. If this flag is not set, the window is activated and moved to the top of either the topmost or non-topmost group (depending on the setting of the hWndInsertAfter parameter).</summary>
        /// <remarks>http://msdn.microsoft.com/library/ms633545.aspx</remarks>
        private const uint SWP_NOACTIVATE = 0x0010;
        /// <summary>Places the window above all non-topmost windows. The window maintains its topmost position even when it is deactivated.</summary>
        /// <remarks>http://msdn.microsoft.com/library/ms633545.aspx</remarks>
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

        /// <summary>Retrieves a handle to the top-level window whose class name and window name match the specified strings.</summary>
        /// <remarks>http://msdn.microsoft.com/library/ms633499.aspx</remarks>
        /// <param name="lpClassName">The class name created by a previous call to the RegisterClass or RegisterClassEx function.
        /// If lpClassName points to a string, it specifies the window class name. The class name can be any name registered with RegisterClass or RegisterClassEx, or any of the predefined control-class names.
        /// If lpClassName is NULL, it finds any window whose title matches the lpWindowName parameter.</param>
        /// <param name="lpWindowName">The window name (the window's title). If this parameter is NULL, all window names match. </param>
        /// <returns>If the function succeeds, the return value is a handle to the window that has the specified class name and window name. If the function fails, the return value is NULL.</returns>
        [DllImport("user32.dll", SetLastError = true)] private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        /// <summary>Changes the size, position, and Z order of a child, pop-up, or top-level window</summary>
        /// <remarks>http://msdn.microsoft.com/library/ms633545.aspx</remarks>
        /// <param name="hWnd">A handle to the window</param>
        /// <param name="hWndInsertAfter">A handle to the window to precede the positioned window in the Z order</param>
        /// <param name="X">The new position of the left side of the window, in client coordinates</param>
        /// <param name="Y">The new position of the top of the window, in client coordinates</param>
        /// <param name="W">The new width of the window, in pixels</param>
        /// <param name="H">The new height of the window, in pixels</param>
        /// <param name="uFlags">The window sizing and positioning flags</param>
        /// <returns>If the function succeeds, the return value is nonzero</returns>
        [DllImport("user32.dll", SetLastError = true)] private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int W, int H, uint uFlags);
        /// <summary>Sets the specified window's show state</summary>
        /// <remarks>http://msdn.microsoft.com/library/ms633548.aspx</remarks>
        /// <param name="hWnd">A handle to the window</param>
        /// <param name="nCmdShow">Controls how the window is to be shown</param>
        /// <returns>If the window was previously visible, the return value is nonzero, otherwise zero</returns>
        [DllImport("user32.dll", SetLastError = true)] private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        #endregion

        /// <summary>Show the form as the topmost form, without activating it</summary>
        /// <param name="f">The form to show</param>
        public static void ShowInactiveTopmost(this Form f)
        {
            ShowWindow(f.Handle, SW_SHOWNOACTIVATE);
            if (!SetWindowPos(f.Handle, HWND_TOPMOST, f.Left, f.Top, f.Width, f.Height, SWP_NOACTIVATE))
                throw new Win32Exception();
        }

        /// <summary>Get the position of the Windows Taskbar</summary>
        /// <exception cref="Win32Exception">If either FindWindow or SHAppBarMessage fail</exception>
        /// <returns>The Windows Taskbar Position</returns>
        public static TaskbarPos GetTaskbarPosition()
        {
            // Fill in the APPDARDATA struct, only providing the handle to the Windows Taskbar 'window' which is obtained using FindWindow
            APPBARDATA data = new APPBARDATA
            {
                cbSize = (uint)Marshal.SizeOf(typeof(APPBARDATA)),
                hWnd = FindWindow(TaskbarClassName, null),
            };
            
            // Check that the hWnd is good and send the GetTaskbarPos message to the taskbar
            if (data.hWnd == IntPtr.Zero || SHAppBarMessage(ABM.GetTaskbarPos, ref data) == IntPtr.Zero)
                throw new Win32Exception();

            // The edge that the taskbar lines on
            return data.uEdge;
        }

        /// <summary>Get the position of the clock, using the position of the taskbar</summary>
        /// <returns>The corner that has the clock (taskbar notification icons, etc)</returns>
        public static DesktopPos GetClockPosition()
        {
            switch (GetTaskbarPosition()) {
                case TaskbarPos.Top: return DesktopPos.UpperRight; // The taskbar is on the top, so clock is in upper right
                case TaskbarPos.Left: return DesktopPos.LowerLeft; // The taskbar is on the left, so clock in in the lower left
                default: // Bottom, Right, Unknown
                    return DesktopPos.LowerRight; // otherwise it is in the lower right corner
            }
        }
    }
}
