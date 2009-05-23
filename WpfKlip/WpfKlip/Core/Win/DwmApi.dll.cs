using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using WpfKlip.Core.Win.Structs;

namespace WpfKlip.Core.Win
{
	static public class  DwmApi
	{
        [DllImport("DwmApi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(
            IntPtr hwnd,
            ref MARGINS pMarInset);
	}
}
