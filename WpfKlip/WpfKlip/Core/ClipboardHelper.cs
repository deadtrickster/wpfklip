#region License block
/*
Copyright (c) 2009 Khaprov Ilja (http://dead-trickster.com)
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
using System.Windows.Forms;
using WpfKlip.Core.Win;
using WpfKlip.Core.Win.Enums;
using System.Runtime.InteropServices;

namespace WpfKlip.Core
{
    public delegate void ClipboardTextGrabbedEventHandler(string text);
    [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
    public class ClipboardHelper : NativeWindow
    {
        Form parent;
        IntPtr hwndNextViewer;
        bool m_bCallingSetClipboardViewer;


        private void RegisterClipboardListener()
        {
            m_bCallingSetClipboardViewer = true;
            hwndNextViewer = User32.SetClipboardViewer(this.Handle);
            m_bCallingSetClipboardViewer = false;
        }

        public ClipboardHelper(Form parent)
        {
            parent.HandleCreated += new EventHandler(this.OnHandleCreated);
            parent.HandleDestroyed += new EventHandler(this.OnHandleDestroyed);
            this.parent = parent;
        }

        public ClipboardHelper(IntPtr handle)
        {
            AssignHandle(handle);
            RegisterClipboardListener();
        }

        // Listen for the control's window creation and then hook into it.
        internal void OnHandleCreated(object sender, EventArgs e)
        {
            // Window is now created, assign handle to NativeWindow.
            AssignHandle(((Form)sender).Handle);
            RegisterClipboardListener();
        }

        internal void OnHandleDestroyed(object sender, EventArgs e)
        {
            // Window was destroyed, release hook.
            ReleaseHandle();
        }

        public override void ReleaseHandle()
        {
            User32.ChangeClipboardChain(this.Handle, hwndNextViewer);
            base.ReleaseHandle();
        }

        [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
        protected override void WndProc(ref Message m)
        {
            WindowMessages wm = (WindowMessages)m.Msg;
            switch (wm)
            {
                case WindowMessages.WM_DRAWCLIPBOARD:
                    if (hwndNextViewer != null)
                        User32.SendMessage(hwndNextViewer, (int)WindowMessages.WM_CHANGECBCHAIN, m.WParam, m.LParam);
                    if (!m_bCallingSetClipboardViewer)
                        GrabClipboard();
                    break;
                case WindowMessages.WM_CHANGECBCHAIN:
                    // If the next window is closing, repair the chain. 

                    if ((IntPtr)m.WParam == hwndNextViewer)
                        hwndNextViewer = (IntPtr)m.LParam;

                    // Otherwise, pass the message to the next link. 

                    else if (hwndNextViewer != null)
                        User32.SendMessage(hwndNextViewer, (int)WindowMessages.WM_CHANGECBCHAIN, m.WParam, m.LParam);

                    break;
            }
            base.WndProc(ref m);
        }


        public event ClipboardTextGrabbedEventHandler ClipboardTextGrabbed;

        private void GrabClipboard()
        {
            try
            {
                if (System.Windows.Forms.Clipboard.ContainsText())
                {
                    var str = System.Windows.Forms.Clipboard.GetText();

                    if (ClipboardTextGrabbed != null)
                    {
                        ClipboardTextGrabbed(str);
                    }
                }
            }
            catch (COMException)
            {
                System.Windows.Forms.MessageBox.Show("Unable to retrive text from clipboard: COM exception :-(.");
            }
        }
    }
}
