using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interactivity;
using System.Windows.Interop;


namespace GuiMisc
{
    public static class WindowPlacementHelper
    {
        public const uint WPF_SETMINPOSITION = 0x0001;
        public const uint WPF_RESTORETOMAXIMIZED = 0x0002;
        public const uint WPF_ASYNCWINDOWPLACEMENT = 0x0004;

        public const uint SW_HIDE = 0;
        public const uint SW_SHOWNORMAL = 1;
        public const uint SW_NORMAL = 1;
        public const uint SW_SHOWMINIMIZED = 2;
        public const uint SW_SHOWMAXIMIZED = 3;
        public const uint SW_MAXIMIZE = 3;
        public const uint SW_SHOWNOACTIVATE = 4;
        public const uint SW_SHOW = 5;
        public const uint SW_MINIMIZE = 6;
        public const uint SW_SHOWMINNOACTIVE = 7;
        public const uint SW_SHOWNA = 8;
        public const uint SW_RESTORE = 9;
        public const uint SW_SHOWDEFAULT = 10;
        public const uint SW_FORCEMINIMIZE = 11;
        public const uint SW_MAX = 11;


        [DllImport("User32.dll", EntryPoint="GetWindowPlacement")]
        static extern bool NativeGetWindowPlacement(IntPtr hWnd, [In][Out] WindowPlacement lpwndpl);

        [DllImport("User32.dll", EntryPoint ="SetWindowPlacement")]
        static extern bool NativeSetWindowPlacement(IntPtr hWnd, WindowPlacement lpwndpl);


        public static WindowPlacement GetWindowPlacement(this Window window)
        {
            WindowPlacement result = null;
            
            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            if (hwnd != null)
            {
                var w = new WindowPlacement
                {
                    length = (uint)Marshal.SizeOf(typeof(WindowPlacement))
                };
                if (NativeGetWindowPlacement(hwnd, w))
                    result = w;
            }
            return result;
        }


        public static bool SetWindowPlacement(this Window window, WindowPlacement wp)
        {
            bool result = false;

            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            if (wp != null && hwnd != null)
            {
                if (wp.showCmd == SW_HIDE || wp.showCmd == SW_SHOWMINIMIZED
                    || wp.showCmd == SW_MINIMIZE || wp.showCmd == SW_SHOWMINNOACTIVE)
                {
                    wp.showCmd = SW_SHOWNORMAL;
                }
                result = NativeSetWindowPlacement(hwnd, wp);
            }
            return result;
        }
    }


    [Serializable]
    [SettingsSerializeAs(SettingsSerializeAs.String)]
    [TypeConverter(typeof(WindowPlacementConverter))]
    [StructLayout(LayoutKind.Sequential)]
    public class WindowPlacement
    {
        public uint length;
        public uint flags;
        public uint showCmd;
        public POINT ptMinPosition;
        public POINT ptMaxPosition;
        public RECT rcNormalPosition;


        public override string ToString()
        {
            String str = string.Format("{0},{1},{2},{3},{4},{5}",
                                length, flags, showCmd, 
                                ptMinPosition.ToString(),
                                ptMaxPosition.ToString(),
                                rcNormalPosition.ToString());
            return str;
        }


        public static WindowPlacement FromString(string text)
        {
            WindowPlacement result = null;

            string[] array = text.Split(',');
            if(array.Length > 10)
            { 
                result = new WindowPlacement();

                uint.TryParse(array[0], out result.length);
                uint.TryParse(array[1], out result.flags);
                uint.TryParse(array[2], out result.showCmd);

                string str0 = string.Format("{0},{1}", array[3], array[4]);
                result.ptMinPosition = POINT.FromString(str0);

                string str1 = string.Format("{0},{1}", array[5], array[6]);
                result.ptMaxPosition = POINT.FromString(str1);

                string str2 = string.Format("{0},{1},{2},{3}", array[7], array[8], array[9], array[10]);
                result.rcNormalPosition = RECT.FromString(str2);
            }
            return result;
        }
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int x;
        public int y;


        public override string ToString()
        {
            return string.Format("{0},{1}", x, y);
        }


        public static POINT FromString(string text)
        {
            var result = new POINT();

            string[] array = text.Split(',');
            if (array.Length > 1)
            {
                int.TryParse(array[0], out result.x);
                int.TryParse(array[1], out result.y);
            }
            return result;
        }
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;


        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3}", left, top, right, bottom);
        }


        public static RECT FromString(string text)
        {
            var result = new RECT();

            string[] array = text.Split(',');
            if (array.Length > 3)
            {
                int.TryParse(array[0], out result.left);
                int.TryParse(array[1], out result.top);
                int.TryParse(array[2], out result.right);
                int.TryParse(array[3], out result.bottom);
            }
            return result;
        }
    }


    class WindowPlacementConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            bool result;

            if (sourceType == typeof(string))
                result = true;
            else
                result = base.CanConvertFrom(context, sourceType);

            return result;
        }


        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            bool result;

            if (destinationType == typeof(string))
                result = true;
            else
                result = base.CanConvertTo(context, destinationType);

            return result;
        }


        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            object result;

            if(value is string source)
                result = WindowPlacement.FromString(source);
            else
                result = base.ConvertFrom(context, culture, value);

            return result;
        }


        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            object result;

            if (destinationType == typeof(string) && value is WindowPlacement)
                result = ((WindowPlacement)value).ToString();
            else
                result = base.ConvertTo(context, culture, value, destinationType);

            return result;
        }
    }
}
