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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfKlip.Properties;
using WpfKlip.Core.Win;
using WpfKlip.Core.Win.Enums;
using WpfKlip.Core.Win.Structs;
using System.Runtime.InteropServices;
using System.IO;
using System.Windows.Interop;
using System.Diagnostics;

namespace WpfKlip.Core
{
    class CapturedItemsListController
    {
        enum MouseCommand
        {
            none,
            copy,
            paste,
            remove
        }

        enum ClickType
        {
            Click,
            MidClick,
            RightClick
        }

        static CapturedItemsListController instance;

        internal static CapturedItemsListController Instance
        {
            get { return instance; } 
        }

        public static void Create(MainWindow window)
        {
            instance = new CapturedItemsListController(window);
        }

        IntPtr activeWindow;
        IntPtr mainWindowPtr;
        System.Windows.Forms.Form f;
        ShellHook sh;
        ClipboardHelper ch;
        GlobalHotkeyHelper gh;
        private MainWindow mainWindow;

        private CapturedItemsListController(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            this.mainWindowPtr = new System.Windows.Interop.WindowInteropHelper(mainWindow).Handle;

            f = new System.Windows.Forms.Form();
            sh = new ShellHook(f.Handle);
            sh.WindowActivated += sh_WindowActivated;
            sh.RudeAppActivated += sh_WindowActivated;
            ch = new ClipboardHelper(f.Handle);
            ch.ClipboardTextGrabbed += ch_ClipboardTextGrabbed;

            gh = new GlobalHotkeyHelper(f.Handle);
            gh.GlobalHotkeyFired += new GlobalHotkeyHandler(gh_GlobalHotkeyFired);

            gh.RegisterHotKey(666, KeyModifiers.Alt | KeyModifiers.Control | KeyModifiers.Shift, VirtualKeys.VK_Z);
            gh.RegisterHotKey(667, KeyModifiers.Alt | KeyModifiers.Control | KeyModifiers.Shift, VirtualKeys.VK_1);
            gh.RegisterHotKey(668, KeyModifiers.Alt | KeyModifiers.Control | KeyModifiers.Shift, VirtualKeys.VK_2);
            gh.RegisterHotKey(669, KeyModifiers.Alt | KeyModifiers.Control | KeyModifiers.Shift, VirtualKeys.VK_3);
            gh.RegisterHotKey(670, KeyModifiers.Alt | KeyModifiers.Control | KeyModifiers.Shift, VirtualKeys.VK_4);
            gh.RegisterHotKey(671, KeyModifiers.Alt | KeyModifiers.Control | KeyModifiers.Shift, VirtualKeys.VK_5);
            gh.RegisterHotKey(672, KeyModifiers.Alt | KeyModifiers.Control | KeyModifiers.Shift, VirtualKeys.VK_6);
            gh.RegisterHotKey(673, KeyModifiers.Alt | KeyModifiers.Control | KeyModifiers.Shift, VirtualKeys.VK_7);
            gh.RegisterHotKey(674, KeyModifiers.Alt | KeyModifiers.Control | KeyModifiers.Shift, VirtualKeys.VK_8);
            gh.RegisterHotKey(675, KeyModifiers.Alt | KeyModifiers.Control | KeyModifiers.Shift, VirtualKeys.VK_9);
            gh.RegisterHotKey(676, KeyModifiers.Alt | KeyModifiers.Control | KeyModifiers.Shift, VirtualKeys.VK_0);
        }

        void sh_WindowActivated(ShellHook sender, IntPtr hWnd)
        {
            if (hWnd != mainWindowPtr)
            {
                activeWindow = hWnd;
            }
        }

        void ch_ClipboardTextGrabbed(string text)
        {

            if (ExclusionslistController.Accept(activeWindow))
            {

                var ItemsBox = mainWindow.ItemsBox;
                for (int i = 0; i < ItemsBox.Items.Count; i++)
                {
                    object item = ItemsBox.Items[i];
                    if (((item as ListBoxItem).Tag as String) == text)
                    {
                        ItemsBox.Items.RemoveAt(i);
                        ItemsBox.Items.Insert(0, item);
                        return;
                    }
                }

                ListBoxItem n = new ListBoxItem();
                n.AddHandler(Mouse.MouseDownEvent, new MouseButtonEventHandler(HandleClick), true);
                n.Content = CreatePreviewTitleString(text);
                n.Tag = text;
                n.MinHeight = 25;
                n.ToolTip = CreatePreviewBallonString(text);
                ItemsBox.Items.Insert(0, n);


                Console.WriteLine("\r\n\r\n");

                for (int i = 0; i < ItemsBox.Items.Count; i++)
                {
                    Console.WriteLine("{0}:{1}", i, (ItemsBox.Items[i] as ListBoxItem).Tag);
                }
            }
        }

        void n_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }


        void gh_GlobalHotkeyFired(int id)
        {
            if (id == 666)
            {
                ToogleVisibility();
                return;
            }

            int itemindex = id - 667;

            if (itemindex < mainWindow.ItemsBox.Items.Count)
                DoMouseCommand((ListBoxItem)mainWindow.ItemsBox.Items[itemindex], GetCommandForClick((ClickType)Settings.Default.ItemHotkeyActAs));

        }

        private void ToogleVisibility()
        {
            switch (mainWindow.Visibility)
            {
                case Visibility.Hidden:
                    mainWindow.SetVisible();
                    break;
                case Visibility.Visible:
                    mainWindow.Visibility = Visibility.Hidden;
                    break;
            }
        }

        internal void ClearList()
        {
            mainWindow.ItemsBox.Items.Clear();
        }

        #region new item processing
        private static string CreatePreviewTitleString(string str)
        {
            int len = Settings.Default.TitleLength;
            return CreatePreviewText(str, len);
        }

        private static string CreatePreviewBallonString(string str)
        {
            int len = Settings.Default.BallonLength;
            return CreatePreviewText(str, len);
        }

        private static string CreatePreviewText(string str, int len)
        {
            str = str.Replace("\r", "").Replace("\n", "");

            int index = 0;
            StringReader sr = new StringReader(str);
            StringBuilder sb = new StringBuilder();
            int ich = sr.Read();
            bool prevspace = false;
            while (ich != -1 && index < len)
            {
                char ch = (char)ich;
                if (Char.IsWhiteSpace(ch))
                {
                    if (!prevspace)
                    {
                        prevspace = true;
                        sb.Append(ch);
                        index++;
                    }
                }
                else
                {
                    prevspace = false;
                    sb.Append(ch);
                    index++;
                }
                ich = sr.Read();
            }

            return sb.ToString();
        }

        #endregion

        #region mouse events


        private void HandleClick(object obj, MouseButtonEventArgs e)
        {
            ListBoxItem sender = obj as ListBoxItem;
            MouseCommand command = MouseCommand.none;
            ClickType click = ClickType.Click;
            if (e.ClickCount == 1)
                if (e.ChangedButton == MouseButton.Left)
                {
                    click = ClickType.Click;
                }
                else if (e.ChangedButton == MouseButton.Right)
                {
                    click = ClickType.RightClick;
                }
                else
                {
                    click = ClickType.MidClick;
                }
            else
            {
                return;
            }

            command = GetCommandForClick(click);

            DoMouseCommand(sender, command);

            bool? hide;
            switch (click)
            {
                case ClickType.Click:
                    hide = Settings.Default.ClickHide;
                    break;
                case ClickType.MidClick:
                    hide = Settings.Default.MidClickHide;
                    break;
                case ClickType.RightClick:
                    hide = Settings.Default.RightClickHide;
                    break;
                default:
                    throw new Exception();
            }

            bool? rev = HideRevert();
            if (rev.HasValue && hide.HasValue)
            {
                if ((hide.Value && !rev.Value) | (!hide.Value && rev.Value))
                {
                    mainWindow.Visibility = Visibility.Hidden;
                }
            }
            else
            {
                if (hide.Value)
                {
                    mainWindow.Visibility = Visibility.Hidden;
                }

            }
        }

        private static MouseCommand GetCommandForClick(ClickType click)
        {
            MouseCommand command = MouseCommand.none;
            switch (click)
            {
                case ClickType.Click:
                    command = (MouseCommand)Settings.Default.Click;
                    break;
                case ClickType.MidClick:
                    command = (MouseCommand)Settings.Default.MidClick;
                    break;
                case ClickType.RightClick:
                    command = (MouseCommand)Settings.Default.RightClick;
                    break;
            }
            return command;
        }

        private bool? HideRevert()
        {
            switch (Settings.Default.RevertHide)
            {
                case 0:
                    return default(bool?);
                case 1:
                    return Keyboard.PrimaryDevice.IsKeyDown(Key.LeftAlt) || Keyboard.PrimaryDevice.IsKeyDown(Key.RightAlt);
                case 2:
                    return Keyboard.PrimaryDevice.IsKeyDown(Key.LeftShift) || Keyboard.PrimaryDevice.IsKeyDown(Key.RightShift);
                case 3:
                    return Keyboard.PrimaryDevice.IsKeyDown(Key.LeftCtrl) || Keyboard.PrimaryDevice.IsKeyDown(Key.RightCtrl);
                case 4:
                    return Keyboard.PrimaryDevice.IsKeyDown(Key.System);
                default:
                    throw new Exception();
            }
        }

        private void DoMouseCommand(ListBoxItem listBoxItem, MouseCommand command)
        {
            switch (command)
            {
                case MouseCommand.copy:
                    Copy(listBoxItem);
                    break;
                case MouseCommand.paste:

                    User32.SetForegroundWindow(activeWindow);
                    Copy(listBoxItem);
                    const int CtrlV = 22;  // ASCII for Ctrl+V.
                    press((int)System.Windows.Forms.Keys.ControlKey);
                    press((int)System.Windows.Forms.Keys.V);
                    release((int)System.Windows.Forms.Keys.V);
                   // User32.SendMessage(activeWindow, (int)WindowMessages.WM_CHAR, CtrlV, 0);
                    release((int)System.Windows.Forms.Keys.ControlKey);
                    User32.SetFocus(activeWindow);
                    break;
                case MouseCommand.remove:
                    mainWindow.ItemsBox.Items.Remove(listBoxItem);
                    break;
            }
            // handleClip = true;

        }

        private void Copy(ListBoxItem listBoxItem)
        {
            try
            {
                System.Windows.Forms.Clipboard.SetText((string)listBoxItem.Tag);
            }
            catch
            {
                System.Windows.MessageBox.Show("something going wrong");
            }
        }

        void press(int scanCode)
        {
            sendKey((ushort)scanCode, true);
        }

        void release(int scanCode)
        {
            sendKey((ushort)scanCode, false);
        }

        const uint INPUT_KEYBOARD = 1;
        const int KEY_EXTENDED = 0x0001;
        const uint KEY_UP = 0x0002;
        const uint KEY_SCANCODE = 0x0008;

        private void sendKey(ushort scanCode, bool press)
        {
            KEYBOARD_INPUT[] input = new KEYBOARD_INPUT[1];
            input[0] = new KEYBOARD_INPUT();
            input[0].type = INPUT_KEYBOARD;
            // input[0].flags = KEY_SCANCODE;

            if ((scanCode & 0xFF00) == 0xE000)
            { // extended key? 
                input[0].flags |= KEY_EXTENDED;
            }

            if (press)
            { // press? 
                input[0].vk = (ushort)(scanCode & 0xFF);
            }
            else
            { // release? 
                input[0].vk = scanCode;
                input[0].flags |= KEY_UP;
            }

            uint result = User32.SendInput(1, input, Marshal.SizeOf(input[0]));

            if (result != 1)
            {
                throw new Exception("Could not send key: " + scanCode);
            }
        }


        public System.Windows.Controls.Control SettingsPanel
        {
            get { return null; }
        }
        #endregion

        internal void End()
        {
            f.Close();
            f.Dispose();
        }

        internal void RestoreClipboardChain()
        {
            ch.RestoreClipboardChain();
        }
    }
}
