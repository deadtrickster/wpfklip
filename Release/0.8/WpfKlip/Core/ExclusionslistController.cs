using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WpfKlip.Core.Win;
using WpfKlip.Properties;
using System.Diagnostics;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.IO;

namespace WpfKlip.Core
{
    class ExclusionslistController
    {
        private static string  _CurrentProcessFileName = Process.GetCurrentProcess().MainModule.FileName;
        public static IEnumerable ItemsSource
        {
            get {
                return CombineItems(Process.GetProcesses(), Settings.Default.Exceptions);
            }

        }

        static System.Collections.IEnumerable CombineItems(Process[] processes, System.Collections.Specialized.StringCollection stringCollection)
        {
            List<DistilledProcess> ret = new List<DistilledProcess>(processes.Length);

            // known items
            if (Settings.Default.Exceptions == null)
                Settings.Default.Exceptions = new StringCollection();
            for (int i = 0; i < Settings.Default.Exceptions.Count; i++)
            {
                string[] Exceptions = Settings.Default.Exceptions[i].Split(new string[] { "%%%" }, StringSplitOptions.None);
                string path = Exceptions[0];
                string name = Exceptions[1];
                ret.Add(DistilledProcess.FromRule(path, name));
            }

            //list processes
            var currentProcess = Process.GetCurrentProcess();
            List<Process> distinctImages = new List<Process>(processes.Length);
            IEqualityComparer<Process> comparer = new DistinctHelper();
            for (int i = 0; i < processes.Length; i++)
            {
                try
                {
                    if (processes[i].MainModule.FileName == _CurrentProcessFileName)
                        continue;

                    var exist = ret.FirstOrDefault(dp => dp.ExecutablePath == processes[i].MainModule.FileName);
                    if (exist == null)
                    {

                        if (distinctImages.FirstOrDefault(p => p.MainModule.FileName == processes[i].MainModule.FileName) == null)
                            distinctImages.Add(processes[i]);
                    }
                    else
                    {
                        exist.Update(processes[i]);
                    }
                }
                catch (Win32Exception)
                {
                }
            }

            for (int i = 0; i < distinctImages.Count; i++)
            {
                try
                {
                    ret.Add(DistilledProcess.FromProcess(distinctImages[i]));
                }
                catch (Win32Exception)
                {
                }
                catch (InvalidOperationException)
                {

                }
            }

            return ret;
        }

        public static bool Accept(IntPtr fromWindow)
        {
            if (Settings.Default.Exceptions == null || Settings.Default.Exceptions.Count == 0)
                return Settings.Default.DefaultExAction == 0 ? true : false;

            if (fromWindow == IntPtr.Zero)
                return true;
            else
            {
                string path = CapturedItemsListController.Instance.ActiveProcess.MainModule.FileName;

                for (int i = 0; i < Settings.Default.Exceptions.Count; i++)
                {
                    if (Settings.Default.Exceptions[i].StartsWith(path))
                    {
                        if (Settings.Default.DefaultExAction == 0)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }


            if (Settings.Default.DefaultExAction == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        internal static void Change(object tag)
        {
            DistilledProcess dp = tag as DistilledProcess;
            if (dp.Action != Settings.Default.DefaultExAction)
            {
                if (!Settings.Default.Exceptions.Contains(dp.ExecutablePath + "%%%" + dp.ProcessName))
                {
                    Settings.Default.Exceptions.Add(dp.ExecutablePath + "%%%" + dp.ProcessName);
                    Settings.Default.Save();
                }
            }
            else
            {
                Settings.Default.Exceptions.Remove(dp.ExecutablePath + "%%%" + dp.ProcessName);
                Settings.Default.Save();
            }
        }
    }

    class DistilledProcess
    {
        public string ProcessName
        {
            get { return name; }
        }

        public string ExecutablePath
        {
            get { return path; }
        }

        public string Info
        {
            get { return info; }
            set { info = value; }
        }

        public BitmapSource Icon
        {
            get { return icon; }
        }


        public int Action
        {
            get { return _action; }
            set { _action = value; }
        }

        private string name;
        private string path;
        private BitmapSource icon;
        private string info;
        int _action;

        private DistilledProcess(string name, string path, string info, BitmapSource icon, int action)
        {
            this.name = name;
            this.path = path;
            this.icon = icon;
            _action = action;
            this.info = info;
        }

        public static DistilledProcess FromRule(string path, string name)
        {
            if (File.Exists(path))
            {
                FileInfo file = new FileInfo(path);
                return new DistilledProcess(name, path, path, ShellIcon.GetLargeIcon(path).ToWpfBitmap(), Settings.Default.DefaultExAction == 0 ? 1 : 0);
            }
            else
            {
                return null;
            }
        }

        public static DistilledProcess FromProcess(Process process)
        {
            try // hilarios exceptions under xp
            {
                return new DistilledProcess(ConstructProcessLabel(process.MainModule.FileVersionInfo.FileDescription, process.ProcessName), process.MainModule.FileName, ConstructProcessInfo(process), ShellIcon.GetLargeIcon(process.MainModule.FileName).ToWpfBitmap(), Settings.Default.DefaultExAction);
            }
            catch
            {
                try
                {
                    return new DistilledProcess(ConstructProcessLabel(process.MainWindowTitle, process.ProcessName), process.MainModule.FileName, ConstructProcessInfo(process), ShellIcon.GetLargeIcon(process.MainModule.FileName).ToWpfBitmap(), Settings.Default.DefaultExAction);
                }
                catch
                {
                    return new DistilledProcess(ConstructProcessLabel(process.MainWindowTitle, process.ProcessName), process.MainModule.FileName, ConstructProcessInfo(process), null, Settings.Default.DefaultExAction);
                }
            }
        }

        static string ConstructProcessLabel(string windowTitle, string processName)
        {
            if (windowTitle == null)
            {
                return processName;
            }
            if (windowTitle.Length == 0)
                return processName;
            if (windowTitle.Length <= 25)
                return windowTitle;
            else
            {
                return windowTitle.Substring(0, 25) + "...";
            }
        }

        static string ConstructProcessInfo(Process process)
        {
            /*
                       StringBuilder sb = new StringBuilder();
                       if (process.MainWindowTitle.Length != 0)
                       {
                           sb.AppendLine("Main Window Title: " + process.MainWindowTitle);
                       }
                       sb.Append("Executable Path: " + process.MainModule.FileName);
                       return sb.ToString();*/
            return process.MainModule.FileName;
        }

        internal void Update(Process process)
        {
            name = ConstructProcessLabel(process.MainModule.FileVersionInfo.FileDescription, process.ProcessName);
        }
    }

    /// <summary>
    /// two processes considered to be equal if their MainModules located at the same path
    /// </summary>
    class DistinctHelper : IEqualityComparer<Process>
    {
        public bool Equals(Process x, Process y)
        {
            return x.MainModule.FileName == y.MainModule.FileName;
        }

        public int GetHashCode(Process obj)
        {
            return obj.MainModule.FileName.GetHashCode();
        }
    }
}
