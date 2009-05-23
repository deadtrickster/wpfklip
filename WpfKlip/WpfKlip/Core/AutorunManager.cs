using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Reflection;
using System.Windows.Shapes;
using System.Diagnostics;
using WpfKlip.Properties;
using System.Security.AccessControl;

namespace WPFKlip.Core
{
    public class RegistryAutorunManager
    {
        static RegistryAutorunManager()
        {
            
        }

        static void Default_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "startup")
            {
                isSystemAutorunDefined = Settings.Default.startup;
            }
        }

        static void Default_SettingsLoaded(object sender, System.Configuration.SettingsLoadedEventArgs e)
        {
            isSystemAutorunDefined = Settings.Default.startup;
        }

        public static void InitEvents()
        {
            Settings.Default.SettingsLoaded += new System.Configuration.SettingsLoadedEventHandler(Default_SettingsLoaded);
            Settings.Default.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Default_PropertyChanged);
        }

        static RegistryKey user = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.CreateSubKey | RegistryRights.ReadKey | RegistryRights.SetValue);
        static RegistryKey system = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);

        static string entryname = Assembly.GetEntryAssembly().GetName().Name;
        static string executablepath = Assembly.GetEntryAssembly().Location;
        public static bool isSystemAutorunDefined
        {
            get
            {
                foreach (var item in system.GetValueNames())
                {
                    object val = system.GetValue(item, null);
                    if (val.ToString() == executablepath)
                    {
                        return true;
                    }
                }

                return false;
            }
            set
            {

                string alreadyname = GetNameOfValue(system, executablepath);
                if (value)
                {
                    if (alreadyname == string.Empty)
                    {
                        system.SetValue(entryname, executablepath);
                        return;
                    }
                }
                else
                {
                    if (alreadyname != string.Empty)
                    {
                        system.DeleteValue(alreadyname);
                    }
                }
            }
        }

        private static string GetNameOfValue(RegistryKey key, string executablepath)
        {
            foreach (var item in key.GetValueNames())
            {
                object val = system.GetValue(item, null);
                if (val.ToString() == executablepath)
                {
                    return item;
                }
            }

            return string.Empty;
        }

        public static bool isUserAutorunDefined
        {
            get
            {
                foreach (var item in user.GetValueNames())
                {
                    object val = user.GetValue(item, null);
                    if (val.ToString() == executablepath)
                    {
                        return true;
                    }
                }

                return false;
            }
            set
            {

                string alreadyname = GetNameOfValue(user, executablepath);
                if (value)
                {
                    if (alreadyname == string.Empty)
                    {
                        user.SetValue(entryname, executablepath);
                        return;
                    }
                }
                else
                {
                    if (alreadyname == string.Empty)
                    {
                        user.DeleteValue(alreadyname);
                    }
                }
            }
        }
    }
}
