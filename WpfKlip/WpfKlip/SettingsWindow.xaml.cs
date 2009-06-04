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
using System.Windows.Shapes;
using WpfKlip.Properties;
using System.Diagnostics;
using System.Windows.Interop;

namespace WpfKlip
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();

            Closing += new System.ComponentModel.CancelEventHandler(Optional_Closing);
            Application.Current.Exit+=new ExitEventHandler(Current_Exit);
        }
        bool cclose = true;
        void Current_Exit(object sender, ExitEventArgs e)
        {
            cclose = false;
            Settings.Default.Save();
            Close();
        }

        void Optional_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = cclose ;
            Visibility = Visibility.Hidden;
        }

        static SettingsWindow _singleBack;
        public static SettingsWindow Singleton
        {
            get
            {
                if (_singleBack == null)
                {
                    _singleBack = new SettingsWindow();
                }
                return _singleBack;
            }
        }

        private void label10_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("http://dead-trickster.com");
        }

        private void label11_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("http://12m3.deviantart.com/");
        }

        internal static void ShowOnce()
        {
            var scren = System.Windows.Forms.Screen.FromHandle(new WindowInteropHelper(Singleton).Handle);
            var wa = scren.WorkingArea;
            var mw = Application.Current.MainWindow;
            if ((wa.Left + wa.Width) < (mw.Left + 450 + 235))
            {
                Singleton.Left = mw.Left - 450 - 15;
            }
            else
            {
                Singleton.Left = mw.Left + mw.Width + 15;
            }
            Singleton.Top = Application.Current.MainWindow.Top;
            // hmm if we at the end of screen?
            Singleton.Show();
            Singleton.Activate();
        }
    }
}
