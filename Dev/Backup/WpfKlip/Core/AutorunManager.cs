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
using Microsoft.Win32;
using System.Reflection;
using System.Windows.Shapes;
using System.Diagnostics;
using WpfKlip.Properties;
using System.Security.AccessControl;

namespace WPFKlip.Core
{
    /// <summary>
    /// NOTE: if application will be installed on development machine value of this valuename pair will be changed to 
    /// path to dev bin.
    /// </summary>
    public class RegistryAutorunManager
    {
        static RegistryAutorunManager()
        {
            
        }

        static void Default_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "startup")
            {
                isUserAutorunDefined = Settings.Default.startup;
            }
        }

        static void Default_SettingsLoaded(object sender, System.Configuration.SettingsLoadedEventArgs e)
        {
            isUserAutorunDefined = Settings.Default.startup;
        }

        public static void InitEvents()
        {
            Settings.Default.SettingsLoaded += new System.Configuration.SettingsLoadedEventHandler(Default_SettingsLoaded);
            Settings.Default.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Default_PropertyChanged);
        }

        static RegistryKey user = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.CreateSubKey | RegistryRights.ReadKey | RegistryRights.SetValue);

        static string entryname = Assembly.GetEntryAssembly().GetName().Name;
        static string executablepath = Assembly.GetEntryAssembly().Location;

        private static string GetNameOfValue(RegistryKey key, string executablepath)
        {
            foreach (var item in key.GetValueNames())
            {
                object val = user.GetValue(item, null);
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