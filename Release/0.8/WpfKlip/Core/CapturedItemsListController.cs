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
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfKlip.Core
{

    public delegate Boolean LambdaComparer (object obj);

    class CapturedItemsListController
    {
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

        public IntPtr ActiveWindow
        {
            get { return activeWindow; }
        }

        Process activeProcess;

        public Process ActiveProcess
        {
            get { return activeProcess; }
        }

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
            ch.ClipboardGrabbed += ch_ClipboardGrabbed;
            ch.RegisterClipboardListener();

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

        void ch_ClipboardGrabbed(System.Windows.Forms.IDataObject dataObject)
        {
            IntPtr id;
            User32.GetWindowThreadProcessId(activeWindow, out id);
            activeProcess = Process.GetProcessById(id.ToInt32());

            if (ExclusionslistController.Accept(activeWindow))
            {

                var formats = dataObject.GetFormats();
                DataEnabledListBoxItem n = null;
                var ItemsBox = mainWindow.ItemsBox;
                if (formats.Contains(DataFormats.Text))
                {
                    var text = dataObject.GetData(DataFormats.Text);

                    if (CheckDuplicates(ItemsBox, (obj) => (text as string) == (obj as string)))
                        return;

                    n = new TextDataLBI(text as string);
                }
                else if (formats.Contains(DataFormats.FileDrop))
                {
                    var files = dataObject.GetData(DataFormats.FileDrop) as string[];

                    if (CheckDuplicates(ItemsBox, (obj) =>
                    {
                        string[] objasfiles = obj as string[];

                        if (objasfiles != null && objasfiles.Length == files.Length)
                        {

                            // we need this since order may nto be preserved
                            // e.g. when user selects file from l-to-r and 
                            // and then r-to-l
                            for (int i = 0; i < objasfiles.Length; i++)
                            {
                                if (!files.Contains(objasfiles[i]))
                                    return false;
                            }

                            return true;
                        }
                        return false;
                    }))
                        return;

                    n = new FileDropsLBI(files);
                }
                else if (System.Windows.Clipboard.ContainsImage())
                {

                    System.Windows.Forms.IDataObject clipboardData = System.Windows.Forms.Clipboard.GetDataObject();
                    System.Drawing.Bitmap bitmap = (System.Drawing.Bitmap)clipboardData.GetData(
                       System.Windows.Forms.DataFormats.Bitmap);

                    if (CheckDuplicates(ItemsBox, (obj) =>
                    {
                        var taghash = obj as ImageHash;
                        if (taghash == null)
                        {
                            return false;
                        }

                        else return ImageHash.Compare(taghash, new ImageHash(bitmap)) == ImageHash.CompareResult.ciCompareOk;
                    }))
                        return;

                    n = new ImageLBI(bitmap);
                }

                if (n != null)
                {
                    ItemsBox.Items.Insert(0, n);
                    n.ItemClicked += new ItemCopiedEventHandler(n_ItemClicked);
                }

                /* Console.WriteLine("\r\n\r\n");

                 for (int i = 0; i < ItemsBox.Items.Count; i++)
                 {
                     Console.WriteLine("{0}:{1}", i, (ItemsBox.Items[i] as ListBoxItem).Tag);
                 }*/
            }
        }

        private static bool CheckDuplicates(ListBox ItemsBox, LambdaComparer comparer)
        {
            for (int i = 0; i < ItemsBox.Items.Count; i++)
            {
                object item = ItemsBox.Items[i];
                if (comparer( (item as ListBoxItem).Tag )) // for certain data formats we must do equal not eq
                {
                    if (i != 0)
                    {
                        ItemsBox.Items.RemoveAt(i);
                        ItemsBox.Items.Insert(0, item);
                    }
                    return true;
                }
            }

            return false;
        }

        void sh_WindowActivated(ShellHook sender, IntPtr hWnd)
        {
            if (hWnd != mainWindowPtr)
            {
                activeWindow = hWnd;
            }
        }

        void n_ItemClicked(ListBoxItem item, ClickType clickType)
        {
            TryHide( clickType);
        }

        void gh_GlobalHotkeyFired(int id)
        {
            if (id == 666)
            {
                mainWindow.ToogleVisibility();
                return;
            }

            int itemindex = id - 667;

            if (itemindex < mainWindow.ItemsBox.Items.Count)
            {
                (mainWindow.ItemsBox.Items[itemindex] as DataEnabledListBoxItem).DoMouseCommand(MouseCommand.GetCommandForClick((ClickType)Settings.Default.ItemHotkeyActAs));
            }

        }

        internal void ClearList()
        {
            mainWindow.ItemsBox.Items.Clear();
        }

        #region mouse events

        private void TryHide(ClickType click)
        {
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
                    mainWindow.Visibility = System.Windows.Visibility.Hidden;
                }
            }
            else
            {
                if (hide.Value)
                {
                    mainWindow.Visibility = System.Windows.Visibility.Hidden;
                }
            }
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

        internal void StopClipboard()
        {
            ch.DontCatchAnything = true;
        }

        internal void StartClipboard()
        {
            ch.DontCatchAnything = false;
        }
    }
}
