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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfKlip.Core;
using WpfKlip.Core.Win;
using System.Windows.Interop;
using WpfKlip.Properties;
using System.Diagnostics;

namespace WpfKlip
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += new RoutedEventHandler(MainWindow_Loaded);
            MouseDown += new MouseButtonEventHandler(MainWindow_MouseDown);
            CommandBindings.AddRange(Commands.CommandBindings);
            if (Environment.OSVersion.Version.Major > 5)
            {
                SettingsButton.ContextMenu = null; 
            }
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (Environment.OSVersion.Version.Major > 5)
            {
                MainWindow_Activated(null, null);
                Activated += new EventHandler(MainWindow_Activated);
            }
            else
            {
                ImproveXPLook();
            }

            InitTrayIcon();
        }
        System.Windows.Forms.NotifyIcon ni;
        private void InitTrayIcon()
        {
            ShowHideTrayIcon();
            Settings.Default.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Default_PropertyChanged);
        }

        void Default_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ShowTrayIcon")
            {
                ShowHideTrayIcon();
            }
        }

        private void ShowHideTrayIcon()
        {
            if (Settings.Default.ShowTrayIcon)
            {
                if (ni == null)
                {
                    ni = new System.Windows.Forms.NotifyIcon();
                    ni.Icon = new System.Drawing.Icon(System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\klipper.ico");
                    ni.DoubleClick +=
                        delegate(object sender, EventArgs args)
                        {
                            ToogleVisibility();
                        };
                }

                ni.Visible = true;
            }
            else if (ni != null)
            {
                ni.Visible = false;
            }
        }

        private void ImproveXPLook()
        {
            Background = new SolidColorBrush(SystemColors.WindowColor);
            this.WindowStyle = WindowStyle.SingleBorderWindow;
            this.ItemsBox.Margin = new Thickness(5);

            var mainWindowPtr = new WindowInteropHelper(this).Handle;

            int style = User32.GetWindowLong(mainWindowPtr, (int)WpfKlip.Core.Win.Enums.GWLIndex.GWL_STYLE);

            style = style & ~(int)WpfKlip.Core.Win.Enums.WindowStyle.WS_MINIMIZEBOX;
            style = style & ~(int)WpfKlip.Core.Win.Enums.WindowStyle.WS_MAXIMIZEBOX;
            style = style & ~(int)WpfKlip.Core.Win.Enums.WindowStyle.WS_CAPTION;
            style = style & ~(int)WpfKlip.Core.Win.Enums.WindowStyle.WS_SYSMENU;

            style = style | (int)WpfKlip.Core.Win.Enums.WindowStyle.WS_POPUP;

            User32.SetWindowLong(mainWindowPtr, Core.Win.Enums.GWLIndex.GWL_STYLE, style);
        }

        void MainWindow_Activated(object sender, EventArgs e)
        {
            DWMFrameExtender.FullGlass(this);
        }

        void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        public void ToogleVisibility()
        {
            switch (Visibility)
            {
                case Visibility.Hidden:
                    SetVisible();
                    break;
                case Visibility.Visible:
                    Visibility = Visibility.Hidden;
                    SettingsWindow.Singleton.Visibility = Visibility.Hidden;
                    break;
            }
        }

        public void SetVisible()
        {
            var scren = System.Windows.Forms.Screen.FromHandle(new WindowInteropHelper(this).Handle);
            var wa = scren.WorkingArea;

            // hmm if we at the end of screen?
            var top = System.Windows.Forms.Cursor.Position.Y - 10;
            var left = System.Windows.Forms.Cursor.Position.X - 10;

            if ((wa.Height - top) < Height) // should we use (wa.Left + wa.Width) instead of just wa.Width
            {
                top = top - (int)Height + 10;
            }

            if ((wa.Width - left) < Width)
            {
                left = left - (int)Width + 10;
            }

            this.Top = top;
            this.Left = left;
            ItemsBox.Focus();
            Visibility = Visibility.Visible;
        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CapturedItemsListController.Instance.RestoreClipboardChain();
        }
    }
}
