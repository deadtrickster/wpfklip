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
            Singleton.Top = Application.Current.MainWindow.Top;
            Singleton.Left = Application.Current.MainWindow.Left + Application.Current.MainWindow.Width + 15; // hmm if we at the end of screen?
            Singleton.Show(); 
            Singleton.Activate();
        }
    }
}
