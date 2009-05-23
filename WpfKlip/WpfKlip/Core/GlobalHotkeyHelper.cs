using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Windows;
using WpfKlip.Core.Win.Enums;
using WpfKlip.Core.Win;
namespace WpfKlip.Core
{
    public delegate void GlobalHotkeyHandler(int id);

    [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
    public class GlobalHotkeyHelper : NativeWindow
    {
        Form parent;

        public GlobalHotkeyHelper(Form parent)
        {
            parent.HandleCreated += new EventHandler(this.OnHandleCreated);
            parent.HandleDestroyed += new EventHandler(this.OnHandleDestroyed);
            this.parent = parent;
        }

        public GlobalHotkeyHelper(IntPtr handle)
        {
            AssignHandle(handle);
        }

        // Listen for the control's window creation and then hook into it.
        internal void OnHandleCreated(object sender, EventArgs e)
        {
            // Window is now created, assign handle to NativeWindow.
            AssignHandle(((Form)sender).Handle);
        }

        internal void OnHandleDestroyed(object sender, EventArgs e)
        {
            // Window was destroyed, release hook.
            ReleaseHandle();
        }

        public override void ReleaseHandle()
        {
            base.ReleaseHandle();
        }

        [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
        protected override void WndProc(ref Message m)
        {
            WindowMessages wm = (WindowMessages)m.Msg;
            switch (wm)
            {
                case WindowMessages.WM_HOTKEY:
                    ProcessHotKey(m.WParam.ToInt32());
                    break;
            }
            base.WndProc(ref m);
        }

        public event GlobalHotkeyHandler GlobalHotkeyFired;

        private void ProcessHotKey(int id)
        {
            if (GlobalHotkeyFired != null)
            {
                GlobalHotkeyFired(id);
            }
        }

        public bool RegisterHotKey(int id, KeyModifiers modifiers, VirtualKeys vk)
        {
            return User32.RegisterHotKey(this.Handle, id, (uint)modifiers, (int)vk);
        }
    }
}
