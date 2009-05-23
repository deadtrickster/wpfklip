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
            Activated += new EventHandler(MainWindow_Activated);
            MouseDown += new MouseButtonEventHandler(MainWindow_MouseDown);
            Closed += new EventHandler(MainWindow_Closed);
            CommandBindings.AddRange(Commands.CommandBindings);
        }

        void MainWindow_Activated(object sender, EventArgs e)
        {
            DWMFrameExtender.FullGlass(this);
        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
      //      throw new NotImplementedException();
        }

        void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        internal void SetVisible()
        {
            this.Top = System.Windows.Forms.Cursor.Position.Y - 10;
            this.Left = System.Windows.Forms.Cursor.Position.X - 10;
            Visibility = Visibility.Visible;
        }
    }
}
