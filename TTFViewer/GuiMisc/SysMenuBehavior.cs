using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Interop;


namespace HookSysMenu
{
    class SysMenuBehavior : Behavior<Window>
    {
        MessageHook MessageHook = new MessageHook();

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Loaded += Loaded;
            AssociatedObject.Closed += Closed;
        }


        protected override void OnDetaching()
        {
            AssociatedObject.Closed -= Closed;
            AssociatedObject.Loaded -= Loaded;

            base.OnDetaching();
        }


        private void Loaded(object sender, RoutedEventArgs e)
        {
            AssociatedObject.LostMouseCapture += LostMouseCapture;
            AssociatedObject.PreviewKeyUp += PreviewKeyUp;

            MessageHook.Hook();

            // set sysmenu writable.
            IntPtr hwnd = new WindowInteropHelper(AssociatedObject).Handle;
            Win32.GetSystemMenu(hwnd, false);
        }


        private void Closed(object sender, EventArgs e)
        {
            MessageHook.Unhook();

            AssociatedObject.PreviewKeyUp -= PreviewKeyUp;
            AssociatedObject.LostMouseCapture -= LostMouseCapture;
        }


        private void LostMouseCapture(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.OriginalSource is MenuBase)
            {
                uint flags = 0;
                if (e.LeftButton == MouseButtonState.Pressed)
                    flags = Win32.MOUSEEVENTF_LEFTDOWN | Win32.MOUSEEVENTF_ABSOLUTE;
                else if (e.RightButton == MouseButtonState.Pressed)
                    flags = Win32.MOUSEEVENTF_RIGHTDOWN | Win32.MOUSEEVENTF_ABSOLUTE;

                if (flags != 0)
                {
                    IntPtr hwnd = new WindowInteropHelper(AssociatedObject).Handle;
                    uint pos = Win32.GetMessagePos();
                    int ht = (int)Win32.SendMessage(hwnd, Win32.WM_NCHITTEST, UIntPtr.Zero, (IntPtr)pos);
                    if (ht >= Win32.HTCAPTION)
                    {
                        int x = (short)(pos & 0xFFFF);
                        int y = (short)((pos >> 16) & 0xFFFF);
                        Win32.mouse_event(flags, (uint)x, (uint)y, 0, UIntPtr.Zero);
                    }
                }
            }
        }


        private void PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.System)
            {
                var menu = FindDescendent<Menu>(AssociatedObject, (i => i.IsMainMenu && HasEnableItem(i)));
                if (menu == null)
                    e.Handled = true;
            }
        }

        static T FindDescendent<T>(DependencyObject parent, Func<T, bool> cond) where T : DependencyObject
        {
            T result = null;
            if (parent != null)
            {
                foreach (var item in LogicalTreeHelper.GetChildren(parent))
                {
                    if (item is T t && cond(t))
                        return t;
                    else
                    {
                        T ret = FindDescendent(item as DependencyObject, cond);
                        if (ret != null)
                            return ret;
                    }
                }
            }
            return result;
        }


        static bool HasEnableItem(Menu menu)
        {
            foreach (var i in menu.Items)
            {
                if (i is UIElement fe && fe.IsEnabled)
                    return true;
            }
            return false;
        }
    }

    class MessageHook
    {
        IntPtr HHook;
        Win32.HookProc HookProc;

        MsgFilter MsgFilter = new MsgFilter();

        public void Hook()
        {
            if (MenuHelper.RegPostKeyDown == 0)
                throw new Win32Exception("Win32API RegisterWindowMessage is failed.");                

            if (HHook == IntPtr.Zero)
            {
                HookProc = new Win32.HookProc(MessageHookProc);
                uint threadId = Win32.GetCurrentThreadId();
                HHook = Win32.SetWindowsHookEx(Win32.WH_CALLWNDPROC, HookProc, IntPtr.Zero, threadId);
            }
        }


        public void Unhook()
        {
            if (HHook != IntPtr.Zero)
            {
                Win32.UnhookWindowsHookEx(HHook);
                HHook = IntPtr.Zero;
                HookProc = null;
            }
        }


        private IntPtr MessageHookProc(int nCode, UIntPtr wp, IntPtr lp)
        {
            if (lp != IntPtr.Zero)
            {
                Win32.CWPSTRUCT cwp = (Win32.CWPSTRUCT)Marshal.PtrToStructure(lp, typeof(Win32.CWPSTRUCT));
                switch (cwp.message)
                {
                    case Win32.WM_CREATE: WmCreate(cwp.hwnd, cwp.wParam, cwp.lParam); break;
                    case Win32.WM_DESTROY: WmDestroy(cwp.hwnd, cwp.wParam, cwp.lParam); break;
                    case Win32.WM_ENTERMENULOOP: WmEnterMenuLoop(cwp.hwnd, cwp.wParam, cwp.lParam); break;
                    case Win32.WM_EXITMENULOOP: WmExitMenuLoop(cwp.hwnd, cwp.wParam, cwp.lParam); break;
                    case Win32.WM_INITMENUPOPUP: WmInitMenuPopup(cwp.hwnd, cwp.wParam, cwp.lParam); break;
                }
            }
            return Win32.CallNextHookEx(HHook, nCode, wp, lp);
        }


        private void WmCreate(IntPtr hwnd, UIntPtr wParam, IntPtr lParam)
        {
            MsgFilter.AddPopupMenu(hwnd, wParam, lParam);
        }

        private void WmDestroy(IntPtr hwnd, UIntPtr wParam, IntPtr lParam)
        {
            MsgFilter.RemovePopupMenu(hwnd, wParam, lParam);
        }

        private void WmEnterMenuLoop(IntPtr hwnd, UIntPtr wParam, IntPtr lParam)
        {
            MsgFilter.Hook(hwnd, wParam, lParam);
        }

        private void WmExitMenuLoop(IntPtr hwnd, UIntPtr wParam, IntPtr lParam)
        {
            MsgFilter.Unhook(hwnd, wParam, lParam);
        }

        private void WmInitMenuPopup(IntPtr hwnd, UIntPtr wParam, IntPtr lParam)
        {
            if (((uint)lParam & 0xFFFF0000) != 0) // SystemMenu
            {
                IntPtr hmenu = new IntPtr((long)(ulong)wParam);
                int ht = (int)Win32.SendMessage(hwnd, Win32.WM_NCHITTEST, UIntPtr.Zero, (IntPtr)Win32.GetMessagePos());
                if (ht == Win32.HTCAPTION || ht == Win32.HTMINBUTTON)
                {
                    HwndSource hwndSource = HwndSource.FromHwnd(hwnd);
                    if (hwndSource.RootVisual is Window window)
                    {
                        if (window.WindowState == WindowState.Maximized
                            || window.WindowState == WindowState.Minimized)
                        {
                            Win32.SetMenuDefaultItem(hmenu, Win32.SC_RESTORE, false);
                        }
                        else
                            Win32.SetMenuDefaultItem(hmenu, Win32.SC_MAXIMIZE, false);
                    }
                }
            }
        }
    }


    class MsgFilter
    {
        IntPtr hHook;
        Win32.HookProc HookProc;

        List<PopupMenuItem> List = new List<PopupMenuItem>();

        public void Hook(IntPtr hwnd, UIntPtr wParam, IntPtr lParam)
        {
            if (hHook == IntPtr.Zero)
            {
                HookProc = new Win32.HookProc(MsgFilterProc);
                uint threadId = Win32.GetCurrentThreadId();
                hHook = Win32.SetWindowsHookEx(Win32.WH_MSGFILTER, HookProc, IntPtr.Zero, threadId);
            }
        }

        public void Unhook(IntPtr hwnd, UIntPtr wParam, IntPtr lParam)
        {
            List.Clear();            
            if (hHook != IntPtr.Zero)
            {
                Win32.UnhookWindowsHookEx(hHook);
                hHook = IntPtr.Zero;
                HookProc = null;
            }            
        }


        public void AddPopupMenu(IntPtr hwnd, UIntPtr wParam, IntPtr lParam)
        {
            Win32.CREATESTRUCT cs = (Win32.CREATESTRUCT)Marshal.PtrToStructure(lParam, typeof(Win32.CREATESTRUCT));
            if((uint)cs.lpszClass == 0x8000) // 0x8000 PopupMenuWindow class
                List.Insert(0, new PopupMenuItem(hwnd));
        }


        public void RemovePopupMenu(IntPtr hwnd, UIntPtr wParam, IntPtr lParam)
        {
            int index = 0;
            foreach(PopupMenuItem item in List)
            {
                if (item.HasHWnd(hwnd))
                    break;
                else
                    index++;
            }
            if(index < List.Count)
            {
                List.RemoveRange(0, index + 1);
            }
        }


        private IntPtr MsgFilterProc(int nCode, UIntPtr wp, IntPtr lp)
        {
            bool cancel = false;
            if (lp != IntPtr.Zero)
            {
                Win32.MSG msg = (Win32.MSG)Marshal.PtrToStructure(lp, typeof(Win32.MSG));
                if (msg.message == MenuHelper.RegPostKeyDown)
                    cancel = WmRegPostKeyDown(ref msg);
                else
                {
                    switch (msg.message)
                    {
                        case Win32.WM_MENUSELECT: cancel = WmMenuSelect(ref msg); break;
                        case Win32.WM_KEYDOWN: cancel = WmKeyDown(ref msg); break;
                        case Win32.WM_CHAR: cancel = WmChar(ref msg); break;
                        case Win32.WM_MOUSEMOVE: cancel = WmMouseMove(ref msg); break;
                        case Win32.WM_LBUTTONDOWN: cancel = WmMouseButtonDown(ref msg); break;
                        case Win32.WM_RBUTTONDOWN: cancel = WmMouseButtonDown(ref msg); break;
                    }
                }
            }
            return cancel ? (IntPtr)1 : Win32.CallNextHookEx(hHook, nCode, wp, lp);
        }


        bool WmMenuSelect(ref Win32.MSG msg)
        {
            bool cancel = false;

            //IntPtr hwnd = msg.hwnd;
            IntPtr hmenu = msg.lParam;
            //int item = HIWORD(pMsg->wParam) & MF_POPUP ? 0L : (int)LOWORD(pMsg->wParam);
            //HMENU hmenuPopup = HIWORD(pMsg->wParam) & MF_POPUP ? GetSubMenu((HMENU)pMsg->lParam, LOWORD(pMsg->wParam)) : 0L;
            //uint flags = (UINT)((short)HIWORD(pMsg->wParam) == -1 ? 0xFFFFFFFF : HIWORD(pMsg->wParam));

            PopupMenuItem pmi = List.Find(i=>i.HasHMenu(hmenu));
            if (pmi != null)
                cancel = pmi.WmMenuSelect(ref msg);

            return cancel;
        }


        bool WmKeyDown(ref Win32.MSG msg)
        {
            bool cancel = false;

            IntPtr hwnd = msg.hwnd;
            uint vk = (uint)msg.wParam;
            //int cRepeat = (int)(short)LOWORD(pMsg->lParam);
            //UINT flags = (UINT)HIWORD(pMsg->lParam);

            if (vk == Win32.VK_ESCAPE && List.Count == 1)
            {
                cancel = true;
                Win32.EndMenu();
            }
            else
            {
                PopupMenuItem pmi = GetTop();
                if (pmi != null)
                    cancel = pmi.WmKeyDown(ref msg);
            }
            return cancel;
        }


        bool WmChar(ref Win32.MSG msg)
        {
            bool cancel = false;

            PopupMenuItem pmi = GetTop();
            if (pmi != null)
                cancel = pmi.WmChar(ref msg);

            return cancel;
        }


        bool WmRegPostKeyDown(ref Win32.MSG msg)
        {
            bool cancel = false;

            PopupMenuItem pmi = GetTop();
            if (pmi != null)
                cancel = pmi.WmRegPostKeyDown(ref msg);

            return cancel;
        }


        bool WmMouseMove(ref Win32.MSG msg)
        {
            bool cancel = false;

            Win32.POINT ptScreen = new Win32.POINT(msg.lParam);

            PopupMenuItem pmi = List.Find(i => i.HitTest(ptScreen));
            if (pmi != null)
                cancel = pmi.WmMouseMove(ref msg);

            return cancel;
        }


        bool WmMouseButtonDown(ref Win32.MSG msg)
        {
            bool cancel = false;

            Win32.POINT ptScreen = new Win32.POINT(msg.lParam);

            PopupMenuItem pmi = List.Find(i => i.HitTest(ptScreen));
            if (pmi != null)
                cancel = pmi.WmMouseButtonDown(ref msg);

            return cancel;
        }


        PopupMenuItem GetTop()
        {
            return List.Count > 0 ? List[0] : null;
        }
    }


    class PopupMenuItem
    {
        IntPtr HWndPopup;
        bool FirstMessageDone;
        int MouseSelection;
        int KeyboardSelection;

        public PopupMenuItem(IntPtr hwndPopup)
        {
            HWndPopup = hwndPopup;
        }


        public bool HasHWnd(IntPtr hwnd)
        {
            return hwnd == HWndPopup;
        }


        public bool HasHMenu(IntPtr hmenu)
        {
            return hmenu == MenuHelper.GetHMenu(HWndPopup);
        }


        public bool HitTest(Win32.POINT ptScreen)
        {
            Win32.RECT rc = new Win32.RECT();
            Win32.GetWindowRect(HWndPopup, ref rc);
            return Win32.PtInRect(ref rc, ptScreen);
        }


        public bool WmMenuSelect(ref Win32.MSG msg)
        {
            bool cancel = false;

            IntPtr hwnd = msg.hwnd;
            IntPtr hmenu = msg.lParam;
            //int item = HIWORD(pMsg->wParam) & MF_POPUP ? 0L : (int)LOWORD(pMsg->wParam);
            //HMENU hmenuPopup = HIWORD(pMsg->wParam) & MF_POPUP ? GetSubMenu((HMENU)pMsg->lParam, LOWORD(pMsg->wParam)) : 0L;
            uint flags = ((uint)msg.wParam >> 16) & 0xFFFF;

            if (flags == 0xFFFF)
                flags = uint.MaxValue;

            if (!FirstMessageDone)
            {
                FirstMessageDone = true;

                int pos = MenuHelper.GetHilitePos(hmenu);
                if (pos >= 0)
                    KeyboardSelection = pos;

                if (flags != uint.MaxValue && (flags & Win32.MFS_GRAYED) != 0)
                {
                    if (MenuHelper.GetFirstEnabled(hmenu) < 0)
                    {
                        cancel = true;
                        MenuHelper.ShowSelect(HWndPopup, -1);
                    }
                    else
                        Win32.PostMessage(hwnd, Win32.WM_KEYDOWN, (UIntPtr)Win32.VK_DOWN, IntPtr.Zero);
                }
            }
            return cancel;
        }


        public bool WmKeyDown(ref Win32.MSG msg)
        {
            bool cancel = false;

            IntPtr hwnd = msg.hwnd;
            uint vk = (uint)msg.wParam;
            //int cRepeat = (int)(short)LOWORD(pMsg->lParam);
            //UINT flags = (UINT)HIWORD(pMsg->lParam);

            FirstMessageDone = true;

            IntPtr hmenu = MenuHelper.GetHMenu(HWndPopup);
            int sel;
            switch (vk)
            {
                case Win32.VK_RETURN:
                    sel = MenuHelper.GetHilitePos(hmenu);
                    if (sel < 0)
                        cancel = true;
                    break;

                case Win32.VK_UP:
                case Win32.VK_DOWN:
                    if (MenuHelper.GetFirstEnabled(hmenu) < 0)
                        cancel = true;
                    else
                    {
                        int pos = KeyboardSelection;

                        // menuitem at pos may not have MF_HILITE
                        if (pos >= 0)
                            MenuHelper.ShowSelect(HWndPopup, pos);

                        Win32.PostMessage(hwnd, MenuHelper.RegPostKeyDown, (UIntPtr)vk, hmenu);
                    }
                    break;
            }

            return cancel;
        }


        public bool WmChar(ref Win32.MSG msg)
        {
            bool cancel = false;

            IntPtr hmenu = MenuHelper.GetHMenu(HWndPopup);
            int pos = MenuHelper.GetMnemonicItem(hmenu, (char)msg.wParam);
            uint state = MenuHelper.GetState(hmenu, pos);
            if (state != uint.MaxValue && (state & Win32.MFS_GRAYED) != 0)
                cancel = true;

            return cancel;
        }


        public bool WmRegPostKeyDown(ref Win32.MSG msg)
        {
            bool cancel = false;

            uint vk = (uint)msg.wParam;
            IntPtr hmenu = msg.lParam;

            if (vk == Win32.VK_UP || vk == Win32.VK_DOWN)
            {
                if (hmenu == MenuHelper.GetHMenu(HWndPopup))
                {
                    int pos = MenuHelper.GetHilitePos(hmenu);
                    if (pos >= 0)
                    {
                        KeyboardSelection = pos;

                        uint state = MenuHelper.GetState(hmenu, pos);
                        if (state != uint.MaxValue && (state & Win32.MFS_GRAYED) != 0)
                            Win32.PostMessage(msg.hwnd, Win32.WM_KEYDOWN, (UIntPtr)vk, IntPtr.Zero);
                    }
                }
            }
            return cancel;
        }


        public bool WmMouseMove(ref Win32.MSG msg)
        {
            bool cancel = false;

            //IntPtr hwnd = msg.hwnd;
            //int x = GET_X_LPARAM(pMsg->lParam);
            //int y = GET_Y_LPARAM(pMsg->lParam);
            //UINT keyFlags = (UINT)pMsg->wParam;

            FirstMessageDone = true;

            IntPtr hmenu = MenuHelper.GetHMenu(HWndPopup);
            Win32.POINT ptScreen = new Win32.POINT(msg.lParam);
            int mouseSel = MenuHelper.HitTest(HWndPopup, ptScreen);
            uint state = MenuHelper.GetState(hmenu, mouseSel);
            if (state != uint.MaxValue && (state & Win32.MFS_GRAYED) != 0)
                mouseSel = -1;

            int oldMouseSel = MouseSelection;
            int keyboardSel = KeyboardSelection;

            if (mouseSel != oldMouseSel)
            {
                MouseSelection = mouseSel;
                if (mouseSel >= 0)
                {
                    if (mouseSel != keyboardSel)
                        KeyboardSelection = mouseSel;
                }
                else
                {
                    cancel = true;
                    if (oldMouseSel == keyboardSel)
                        MenuHelper.ShowSelect(HWndPopup, -1);
                }
            }
            else if (mouseSel != keyboardSel || mouseSel < 0)
                cancel = true;

            return cancel;
        }


        public bool WmMouseButtonDown(ref Win32.MSG msg)
        {
            bool cancel = false;

            FirstMessageDone = true;

            IntPtr hmenu = MenuHelper.GetHMenu(HWndPopup);
            Win32.POINT ptScreen = new Win32.POINT(msg.lParam);
            int pos = MenuHelper.HitTest(HWndPopup, ptScreen);
            uint state = MenuHelper.GetState(hmenu, pos);
            if (state != uint.MaxValue && (state & Win32.MFS_GRAYED) != 0)
                cancel = true;

            return cancel;
        }
    }


    static class MenuHelper
    {
        static uint InternalMessage;
        public static uint RegPostKeyDown
        {
            get
            {
                if (InternalMessage == 0)
                    InternalMessage = Win32.RegisterWindowMessage("MenuHookPostKeyDownMessage");
                return InternalMessage;
            }
        }


        public static uint GetGuiThreadFlags()
        {
            var gti = new Win32.GUITHREADINFO()
            {
                cbSize = (uint)Marshal.SizeOf<Win32.GUITHREADINFO>(),
            };

            if (Win32.GetGUIThreadInfo(Win32.GetCurrentThreadId(), ref gti))
                return gti.flags;
            else
                return uint.MaxValue;
        }


        public static uint GetState(IntPtr hmenu, int pos)
        {
            uint result = uint.MaxValue;
            if (pos >= 0)
            {
                var mii = new Win32.MENUITEMINFO()
                {
                    cbSize = (uint)Marshal.SizeOf<Win32.MENUITEMINFO>(),
                    fMask = Win32.MIIM_STATE,
                };
                if (Win32.GetMenuItemInfo(hmenu, (uint)pos, true, ref mii))
                    result = mii.fState;                
            }
            return result;
        }


        static uint mn_selectitem = 0x1E5;

        public static void ShowSelect(IntPtr hwnd, int pos)
        {
            Win32.SendMessage(hwnd, mn_selectitem, (UIntPtr)(uint)pos, IntPtr.Zero);
        }


        public static IntPtr GetHMenu(IntPtr hwnd)
        {
            return Win32.SendMessage(hwnd, Win32.MN_GETHMENU, UIntPtr.Zero, IntPtr.Zero);
        }


        public static int HitTest(IntPtr hwnd, Win32.POINT ptScreen)
        {
            IntPtr hmenu = GetHMenu(hwnd);
            return Win32.MenuItemFromPoint(hwnd, hmenu, ptScreen);            
        }

        public static int GetHilitePos(IntPtr hmenu)
        {
            int result = -1;
            if (hmenu != IntPtr.Zero)
            {
                int count = Win32.GetMenuItemCount(hmenu);
                for (int i = 0; result < 0 && i < count; i++)
                {
                    uint state = GetState(hmenu, i);
                    if (state != uint.MaxValue && (state & Win32.MFS_HILITE) != 0)
                        result = i;
                }
            }
            return result;
        }



        public static int GetFirstEnabled(IntPtr hmenu)
        {
            int result = -1;
            if (hmenu != IntPtr.Zero)
            {
                int count = Win32.GetMenuItemCount(hmenu);
                for (int i = 0; result < 0 && i < count; i++)
                {
                    uint state = GetState(hmenu, i);
                    if (state != uint.MaxValue && (state & Win32.MFS_GRAYED) == 0)
                        result = i;
                }
            }
            return result;
        }


        public static int GetMnemonicItem(IntPtr hmenu, char c)
        {
            int result = -1;
            if (hmenu != null && c != '\0')
            {
                c = char.ToUpper(c);

                int count = Win32.GetMenuItemCount(hmenu);
                for (int i = 0; result < 0 && i < count; i++)
                {
                    var mii = new Win32.MENUITEMINFO()
                    {
                        cbSize = (uint)Marshal.SizeOf<Win32.MENUITEMINFO>(),
                        fMask = Win32.MIIM_STRING,
                        dwTypeData = null,
                        cch = 0,
                    };
                    if (Win32.GetMenuItemInfo(hmenu, (uint)i, true, ref mii))
                    {
                        mii.dwTypeData = new string(new char[mii.cch + 1]);
                        mii.cch++;
                        if (Win32.GetMenuItemInfo(hmenu, (uint)i, true, ref mii))
                        {
                            int index = mii.dwTypeData.IndexOf('&');
                            if (0 <= index && index + 1 < mii.dwTypeData.Length 
                                && char.ToUpper(mii.dwTypeData[index + 1]) == c)
                                result = i;
                        }
                    }
                }
            }
            return result;
        }


    }
}

