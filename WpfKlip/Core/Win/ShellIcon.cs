﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using WpfKlip.Core.Win.Structs;
using WpfKlip.Core.Win.Enums;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace WpfKlip.Core.Win
{

    using Bitmap = System.Drawing.Bitmap;
    /// <summary>
    /// Summary description for ShellIcon.  Get a small or large Icon with an easy C# function call
    /// that returns a 32x32 or 16x16 System.Drawing.Icon depending on which function you call
    /// either GetSmallIcon(string fileName) or GetLargeIcon(string fileName)
    /// </summary>
    public static class ShellIcon
    {
            public const uint SHGFI_ICON = 0x100;
            public const uint SHGFI_LARGEICON = 0x0; // 'Large icon
            public const uint SHGFI_SMALLICON = 0x1; // 'Small icon

        static ShellIcon()
        {

        }

        public static Icon GetSmallIcon(string fileName)
        {
            return GetIcon(fileName, Enums.GetFileIconFlags.SHGFI_SMALLICON | GetFileIconFlags.SHGFI_ICON);
        }

        public static Icon GetLargeIcon(string fileName)
        {
            return GetIcon(fileName, GetFileIconFlags.SHGFI_LARGEICON | GetFileIconFlags.SHGFI_ICON); ;
        }

        public static Icon GetXPIcon(string fileName)
        {
            return GetIcon(fileName, GetFileIconFlags.SHGFI_LARGEICON | GetFileIconFlags.SHGFI_ICON); ;
        }

        public static Icon GetVistaIcon(string fileName)
        {
            return GetIcon(fileName, GetFileIconFlags.SHGFI_LARGEICON | GetFileIconFlags.SHGFI_ICON); ;
        }


        private static Icon GetIcon(string fileName, GetFileIconFlags flags)
        {
            SHFILEINFO shinfo = new SHFILEINFO();
            IntPtr hImgSmall = Shell32.SHGetFileInfo(fileName, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), flags);

            Icon icon = (Icon)System.Drawing.Icon.FromHandle(shinfo.hIcon).Clone();
            User32.DestroyIcon(shinfo.hIcon);
            return icon;

        }

    }

    public static class IconEx
    {
        public static BitmapSource ToWpfBitmap(this Icon icon)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(icon.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
        }
        public static BitmapSource ToWpfBitmap(this Bitmap bitmap)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
        }
    }
}
