using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using WpfKlip.Core.Win.Structs;
using WpfKlip.Core.Win.Enums;

namespace WpfKlip.Core.Win
{
    class Shell32
    {
        [DllImport("shell32.dll")]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, GetFileIconFlags uFlags);
    }
}
