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
using System.Diagnostics;
using WpfKlip.Properties;
using WpfKlip.Core.Win;
using System.ComponentModel;
using System.Collections.Specialized;
using System.IO;
using WpfKlip.Core;

namespace WpfKlip
{
    /// <summary>
    /// Interaction logic for ExceptionsList.xaml
    /// TODO: cleanup
    /// </summary>
    public partial class ExclusionsList : UserControl
    {
        public ExclusionsList()
        {
            InitializeComponent();
            DefaultActionSelector.SelectedIndex = Settings.Default.DefaultExAction;
            DefaultActionSelector.SelectionChanged +=DefaultActionSelector_SelectedIndexChanged;
            refreshProcessList();
        }

        bool refreshing_list = false;
        private void refreshProcessList()
        {
            refreshing_list = true;
            Actionslist.ItemsSource = ExclusionslistController.ItemsSource;
            refreshing_list = false;
        }

        private void RefreshProcessListButton_Click(object sender, RoutedEventArgs e)
        {
            refreshProcessList();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!refreshing_list)
            {
                ExclusionslistController.Change((sender as Control).Tag);
            }
        }

        private void DefaultActionSelector_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            Settings.Default.DefaultExAction = (sender as ComboBox).SelectedIndex;
            Settings.Default.Save();
            refreshProcessList();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Exceptions.Clear();
            Settings.Default.Save();

            refreshProcessList();
        }
    }
}
