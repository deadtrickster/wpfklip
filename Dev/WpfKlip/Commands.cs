#region License block
/*
Copyright (c) 2009 Khaprov Ilya (http://dead-trickster.com)
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
