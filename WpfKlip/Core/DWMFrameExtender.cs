#region License block
/*
Copyright (c) 2009,2015 Ilya Khaprov <ilya.khaprov@publitechs.com>
Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Windows.Media;
using WpfKlip.Core.Win.Structs;
using WpfKlip.Core.Win;

namespace WpfKlip.Core
{
    public class DWMFrameExtender
    {
        public static void FullGlass(Window w)
        {
            if (Environment.OSVersion.Version.Major > 5)
            {
                var mainWindowPtr = new WindowInteropHelper(w).Handle;
                try
                {
                    System.Windows.Application.Current.MainWindow.Background = new SolidColorBrush(Colors.Transparent);
             
                    HwndSource mainWindowSrc = HwndSource.FromHwnd(mainWindowPtr);
                    mainWindowSrc.CompositionTarget.BackgroundColor = Color.FromArgb(0, 0, 0, 0);

                    // Get System Dpi
                    System.Drawing.Graphics desktop = System.Drawing.Graphics.FromHwnd(mainWindowPtr);
                    float DesktopDpiX = desktop.DpiX;
                    float DesktopDpiY = desktop.DpiY;

                    // Set Margins
                    MARGINS margins = new MARGINS();

                    // Extend glass frame into client area
                    // Note that the default desktop Dpi is 96dpi. The  margins are
                    // adjusted for the system Dpi.
                    margins.cxLeftWidth = Convert.ToInt32(1000 * (DesktopDpiX / 96));
                    margins.cxRightWidth = Convert.ToInt32(1000 * (DesktopDpiX / 96));
                    margins.cyTopHeight = Convert.ToInt32(1000 * (DesktopDpiX / 96));
                    margins.cyBottomHeight = Convert.ToInt32(1000 * (DesktopDpiX / 96));

                    int hr = DwmApi.DwmExtendFrameIntoClientArea(mainWindowSrc.Handle, ref margins);
                    //
                    if (hr < 0)
                    {
                        //DwmExtendFrameIntoClientArea Failed
                    }
                }
                // If not Vista, paint background white.
                catch (DllNotFoundException)
                {
                    System.Windows.Application.Current.MainWindow.Background = SystemColors.ControlBrush;
                }
            }
        }

        internal static void RemoveGlass(MainWindow w)
        {
            if (Environment.OSVersion.Version.Major > 5)
            {
                var mainWindowPtr = new WindowInteropHelper(w).Handle;
                try
                {
                    System.Windows.Application.Current.MainWindow.Background = new SolidColorBrush(Colors.Transparent);

                    HwndSource mainWindowSrc = HwndSource.FromHwnd(mainWindowPtr);
                    mainWindowSrc.CompositionTarget.BackgroundColor = Colors.White;

                    // Get System Dpi
                    System.Drawing.Graphics desktop = System.Drawing.Graphics.FromHwnd(mainWindowPtr);
                    float DesktopDpiX = desktop.DpiX;
                    float DesktopDpiY = desktop.DpiY;

                    // Set Margins
                    MARGINS margins = new MARGINS();

                    // Extend glass frame into client area
                    // Note that the default desktop Dpi is 96dpi. The  margins are
                    // adjusted for the system Dpi.
                    margins.cxLeftWidth = 0;
                    margins.cxRightWidth = 0;
                    margins.cyTopHeight =0;
                    margins.cyBottomHeight = 0;

                    int hr = DwmApi.DwmExtendFrameIntoClientArea(mainWindowSrc.Handle, ref margins);
                    //
                    if (hr < 0)
                    {
                        //DwmExtendFrameIntoClientArea Failed
                    }
                }
                // If not Vista, paint background white.
                catch (DllNotFoundException)
                {
                    System.Windows.Application.Current.MainWindow.Background = SystemColors.ControlBrush;
                }
            }
        }
    }
}
