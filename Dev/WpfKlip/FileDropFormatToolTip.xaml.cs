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
using System.IO;
using WpfKlip.Core.Win;

namespace WpfKlip
{
    /// <summary>
    /// Interaction logic for FileDropFormatToolTip.xaml
    /// </summary>
    public partial class FileDropFormatToolTip : Window
    {
        public FileDropFormatToolTip(string[] files)
        {
            InitializeComponent();
            Fileslist.ItemsSource = PreapreFilesInfo(files);
        }

        private System.Collections.IEnumerable PreapreFilesInfo(string[] files)
        {
            List<_FileInfo> ret = new List<_FileInfo>(files.Length);

            foreach (var file_path in files)
            {
                if (File.Exists(file_path))
                {
                    ret.Add(new _FileInfo(file_path));
                }
            }

            return ret;
        }

        public class _FileInfo
        {
            public _FileInfo(string path)
            {
                Name = System.IO.Path.GetFileName(path);
                Path = path;
                Thumb = ShellThumbnailsService.GetThumbOrIcon(path).ToWpfBitmap();
            }

            public string Name
            {
                get;
                set;
            }

            public string Path
            {
                get;
                set;
            }

            public int Size
            {
                get;
                set;
            }

            public BitmapSource Thumb
            {
                get;
                set;
            }

            public string ToolTipInfo
            {
                get { return "Path: " + Path + "\r\nSize: " + Size; }
            }
        }

    }
}
