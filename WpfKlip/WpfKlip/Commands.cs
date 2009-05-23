using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using WpfKlip.Core;

namespace WpfKlip
{
    public static class Commands
    {
        public static RoutedUICommand ExitCommand = new RoutedUICommand();
        public static RoutedUICommand ClearCommand = new RoutedUICommand();
        public static RoutedUICommand SettingsCommand = new RoutedUICommand();

        static void ExitCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            CapturedItemsListController.Instance.End();
            App.Current.Shutdown();
        }

        static void ClearCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            CapturedItemsListController.Instance.ClearList();
        }

        static void SettingsCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            SettingsWindow.ShowOnce();
        }

        static CommandBindingCollection _CommandBindings;

        public static CommandBindingCollection CommandBindings
        {
            get { return Commands._CommandBindings; }
            set { Commands._CommandBindings = value; }
        }

        static Commands()
        {
            _CommandBindings = new CommandBindingCollection();
            _CommandBindings.Add(new CommandBinding(ExitCommand, ExitCommandExecuted));
            _CommandBindings.Add(new CommandBinding(ClearCommand, ClearCommandExecuted));
            _CommandBindings.Add(new CommandBinding(SettingsCommand, SettingsCommandExecuted));
        }
    }
}
