using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic.ApplicationServices;
using WpfKlip.Properties;
using System.Windows.Interop;
using System.Windows.Forms;
using WPFKlip.Core;

namespace WpfKlip
{
    class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            SingleInstanceManager manager = new SingleInstanceManager();
            RegistryAutorunManager.InitEvents();
            manager.Run(args);
        }
    }

    public class SingleInstanceManager : WindowsFormsApplicationBase
    {
        App app;

        public SingleInstanceManager()
        {
            this.IsSingleInstance = true;
        }



        protected override bool OnStartup(Microsoft.VisualBasic.ApplicationServices.StartupEventArgs e)
        {
            app = new App();
            app.InitializeComponent();

            MainWindow mainWindow = new MainWindow();

            app.Run(mainWindow);
            return false;
        }

        protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
        {
            base.OnStartupNextInstance(eventArgs);
            if (Settings.Default.FocusMainInstance)
            {
                ((MainWindow)app.MainWindow).SetVisible();
            }
        }
    }
}
