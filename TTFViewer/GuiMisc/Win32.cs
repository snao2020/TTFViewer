using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace HookSysMenu
{
    public class Win32Exception : Exception
    {
        public Win32Exception()
        {
        }

        public Win32Exception(string message)
            : base(message)
        {
        }
    }


    public class Win32
    {
        // Common

        // DWORD WINAPI GetCurrentThreadId();
        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();

        //BOOL WINAPI GetWindowRect(HWND hWnd, LPRECT lpRect);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr hwnd, ref RECT lpRect);

        //BOOL WINAPI PtInRect(CONST RECT* lprc, POINT pt);
        [DllImport("user32.dll")]
        public static extern bool PtInRect(ref RECT lprc, POINT pt);

        /*
        #define LOWORD(l)        ((WORD)(((DWORD_PTR)(l)) & 0xffff))
        #define HIWORD(l)        ((WORD)((((DWORD_PTR)(l)) >> 16) & 0xffff))
        #define GET_X_LPARAM(lp) ((int)(short)LOWORD(lp))
        #define GET_Y_LPARAM(lp) ((int)(short)HIWORD(lp))
        */
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;

            public POINT(IntPtr lParam)
            {
                unchecked
                {
                    x = (short)((ulong)lParam & 0xFFFF);
                    y = (short)(((ulong)lParam >> 16) & 0xFFFF);
                }
            }
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            int left;
            int top;
            int right;
            int bottom;
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct CREATESTRUCT
        {
            public IntPtr lpCreateParams;
            public IntPtr hInstance;
            public IntPtr hMenu;
            public IntPtr hwndParent;
            public int cy;
            public int cx;
            public int y;
            public int x;
            public uint style;
            public IntPtr lpszName;
            public IntPtr lpszClass;
            public uint dwExStyle;
        };


        //BOOL GetGUIThreadInfo(DWORD idThread, LPGUITHREADINFO lpgui);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetGUIThreadInfo(uint idThread, ref GUITHREADINFO lpgui);


        [StructLayout(LayoutKind.Sequential)]
        public struct GUITHREADINFO
        {
            public uint cbSize;
            public uint flags;
            public IntPtr hwndActive;
            public IntPtr hwndFocus;
            public IntPtr hwndCapture;
            public IntPtr hwndMenuOwner;
            public IntPtr hwndMoveSize;
            public IntPtr hwndCaret;
            public RECT rcCaret;
        }


        // Message

        //LRESULT SendMessage(HWND hWnd, UINT Msg, WPARAM wParam, LPARAM lParam);
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hwnd, uint message, UIntPtr wParam, IntPtr lParam);

        //BOOL PostMessage(HWND hWnd, UINT Msg, WPARAM wParam, LPARAM lParam);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool PostMessage(IntPtr hwnd, uint message, UIntPtr wParam, IntPtr lParam);

        //DWORD GetMessagePos(VOID);
        [DllImport("user32.dll")]
        public static extern uint GetMessagePos();

        // VOID mouse_event(DWORD dwFlags, DWORD dx, DWORD dy, 
        //                  DWORD dwData, ULONG_PTR dwExtraInfo);
        [SuppressMessage("Microsoft.Design", "IDE1006", Justification = "Rule violation aceppted due blah blah..")]
        [DllImport("user32.dll")]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

        //UINT RegisterWindowMessage(LPCTSTR lpString);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern uint RegisterWindowMessage(string lpString);


        [StructLayout(LayoutKind.Sequential)]
        public struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public UIntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public POINT pt;
        }


        public const uint WM_CREATE = 0x0001;
        public const uint WM_DESTROY = 0x0002;
        public const uint WM_NCHITTEST = 0x0084;
        public const uint WM_KEYDOWN = 0x0100;
        public const uint WM_CHAR = 0x0102;
        public const uint WM_SYSCOMMAND = 0x0112;
        public const uint WM_INITMENUPOPUP = 0x0117;
        public const uint WM_MENUSELECT = 0x011F;
        public const uint MN_GETHMENU = 0x01E1;
        public const uint WM_MOUSEMOVE = 0x0200;
        public const uint WM_LBUTTONDOWN = 0x0201;
        public const uint WM_RBUTTONDOWN = 0x0204;
        public const uint WM_ENTERMENULOOP = 0x0211;
        public const uint WM_EXITMENULOOP = 0x0212;

        public const uint SC_RESTORE = 0xF120;
        public const uint SC_MOVE = 0xF010;
        public const uint SC_SIZE = 0xF000;
        public const uint SC_MINIMIZE = 0xF020;
        public const uint SC_MAXIMIZE = 0xF030;
        public const uint SC_CLOSE = 0xF060;

        public const uint HTCAPTION = 2;
        public const uint HTMINBUTTON = 8;

        public const uint VK_RETURN = 0x0D;
        public const uint VK_ESCAPE = 0x1B;
        public const uint VK_LEFT = 0x25;
        public const uint VK_UP = 0x26;
        public const uint VK_RIGHT = 0x27;
        public const uint VK_DOWN = 0x28;

        public const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        public const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        public const uint MOUSEEVENTF_ABSOLUTE = 0x8000;


        // WindowsHook

        //HOOK WINAPI SetWindowsHookEx(
        //      int idHook, HOOKPROC lpfn, HINSTANCE hMod, DWORD dwThreadId);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(
            int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        //BOOL WINAPI UnhookWindowsHookEx(HHOOK hhk);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        //LRESULT WINAPI CallNextHookEx(
        //    HHOOK hhk, int nCode, WPARAM wParam, LPARAM lParam);
        [DllImport("user32.dll")]
        public static extern IntPtr CallNextHookEx(
            IntPtr hhk, int nCode, UIntPtr wParam, IntPtr lParam);


        [StructLayout(LayoutKind.Sequential)]
        public struct CWPSTRUCT
        {
            public IntPtr lParam;
            public UIntPtr wParam;
            public uint message;
            public IntPtr hwnd;
        }


        public const int WH_MSGFILTER      = -1;
        public const int WH_CALLWNDPROC    = 4;


        //LRESULT CALLBACK CallWndProc(int nCode, WPARAM wParam, LPARAM lParam);
        public delegate IntPtr HookProc(int nCode, UIntPtr wParam, IntPtr lParam);


        // menu

        //HMENU GetSystemMenu(HWND hWnd, BOOL bRevert);
        [DllImport("user32.dll")]
        public static extern IntPtr GetSystemMenu(IntPtr hwnd, bool bRevert);

        //BOOL SetMenuDefaultItem(HMENU hMenu, UINT uItem, UINT fByPos);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetMenuDefaultItem(IntPtr hMenu, uint uItem, bool fByPos);

        //int GetMenuItemCount(HMENU hMenu);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetMenuItemCount(IntPtr hmenu);

        //BOOL EndMenu(VOID);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool EndMenu();

        //int MenuItemFromPoint(HWND hWnd, HMENU hMenu, POINT ptScreen);
        [DllImport("user32.dll")]
        public static extern int MenuItemFromPoint(IntPtr hwnd, IntPtr hmenu, POINT ptScreen);

        //BOOL GetMenuItemInfo(HMENU hMenu, UINT uItem, BOOL fByPosition, LPMENUITEMINFO lpmii);
        [DllImport("user32.dll", EntryPoint = "GetMenuItemInfo", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool GetMenuItemInfo(IntPtr hMenu, uint uItem, bool fByPosition, ref MENUITEMINFO menuItemInfo);


        public const uint MF_BYCOMMAND = 0x00000000;
        public const uint MF_BYPOSITION = 0x00000400;

        public const uint MFT_STRING = 0x00000000;
        public const uint MFT_BITMAP = 0x00000004;
        public const uint MFT_MENUBARBREAK = 0x00000020;
        public const uint MFT_MENUBREAK = 0x00000040;
        public const uint MFT_OWNERDRAW = 0x00000100;
        public const uint MFT_RADIOCHECK = 0x00000200;
        public const uint MFT_SEPARATOR = 0x00000800;
        public const uint MFT_RIGHTORDER = 0x00002000;
        public const uint MFT_RIGHTJUSTIFY = 0x00004000;


        public const uint MFS_GRAYED = 0x00000003;
        public const uint MFS_DISABLED = 0x00000003;
        public const uint MFS_CHECKED = 0x00000008;
        public const uint MFS_HILITE = 0x00000080;
        public const uint MFS_ENABLED = 0x00000000;
        public const uint MFS_UNCHECKED = 0x00000000;
        public const uint MFS_UNHILITE = 0x00000000;
        public const uint MFS_DEFAULT = 0x00001000;

        public const uint MIIM_STATE = 0x00000001;
        public const uint MIIM_ID = 0x00000002;
        public const uint MIIM_SUBMENU = 0x00000004;
        public const uint MIIM_CHECKMARKS = 0x00000008;
        public const uint MIIM_TYPE = 0x00000010;
        public const uint MIIM_DATA = 0x00000020;
        public const uint MIIM_STRING = 0x00000040;
        public const uint MIIM_BITMAP = 0x00000080;
        public const uint MIIM_FTYPE = 0x00000100;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct MENUITEMINFO
        {
            public uint cbSize;
            public uint fMask;
            public uint fType;
            public uint fState;
            public uint wID;
            public IntPtr hSubMenu;
            public IntPtr hbmpChecked;
            public IntPtr hbmpUnchecked;
            public IntPtr dwItemData;
            public string dwTypeData;
            public uint cch;
            public IntPtr hbmpItem;
        }
    }
}
