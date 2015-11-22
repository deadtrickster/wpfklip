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
        // Sends an appbar message to the system. 
        [DllImport("shell32.dll")]
        public static extern UInt32 SHAppBarMessage(
            UInt32 dwMessage,					// Appbar message value to send.
            ref APPBARDATA pData);				// Address of an APPBARDATA structure. The content of the structure 
        // depends on the value set in the dwMessage parameter. 


        [DllImport("shell32.dll")]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, GetFileIconFlags uFlags);
    }
}
