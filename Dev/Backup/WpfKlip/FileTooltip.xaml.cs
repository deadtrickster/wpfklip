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
using WpfKlip.Core;
using System.Windows.Controls.Primitives;
using System.IO;
using WpfKlip.Core.Win;
using System.Threading;

namespace WpfKlip
{

    using Bitmap = System.Drawing.Bitmap;
    /// <summary>
    /// Interaction logic for MyCustomTooltip.xaml
    /// </summary>
    public partial class FileTooltip : Popup
    {
        string[] files;
        public FileTooltip(string[] files)
        {
            #region control related
            InitializeComponent();
            MouseEnter += new MouseEventHandler(MyCustomTooltip_MouseEnter);
            MouseLeave += new MouseEventHandler(MyCustomTooltip_MouseLeave);
           // this.AllowsTransparency = true;
           // base.CoerceValue(HasDropShadowProperty);
           // base.SetValue(HasDropShadowProperty, true);
           
            this.SetResourceReference(Popup.PopupAnimationProperty, SystemParameters.ToolTipPopupAnimationKey);

            close_timer.Elapsed += new System.Timers.ElapsedEventHandler(close_timer_Elapsed);
            close_timer.Interval = 200;

            if (SystemParameters.DropShadow)
            {
                ToolTipService.SetHasDropShadow(this, true);
            }


            #endregion

            this.files = files;
           Thread thr = new Thread(PreapreFilesInfo);

            thr.Start();
            if (instance != null)
            {
                instance.IsOpen = false;
                instance.close_timer.Enabled = false;
            }

            this.Opened += new EventHandler(MyCustomTooltip_Opened);

            instance = this;
        }

        void MyCustomTooltip_Opened(object sender, EventArgs e)
        {
            if (instance != null && instance != this)
            {
                instance.IsOpen = false;
                instance.close_timer.Enabled = false;
            }
            instance = this;
            Focus();
            Fileslist.Focus();
        }

        static FileTooltip instance;

        #region control related
        bool inBusiness;

        public bool InBusiness
        {
            get { return inBusiness; }
            set { inBusiness = value; }
        }
        System.Timers.Timer close_timer = new System.Timers.Timer();
        void MyCustomTooltip_MouseLeave(object sender, MouseEventArgs e)
        {
            inBusiness = false;
            close_timer.Enabled = true;
            close_timer.Start();
        }

        void close_timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke((DispatcherInvoke)TryHideTooltip);
        }

        void TryHideTooltip()
        {
            close_timer.Enabled = false;
            if (!this.InBusiness)
            {
                this.IsOpen = false;
            }
        }

        void MyCustomTooltip_MouseEnter(object sender, MouseEventArgs e)
        {
            inBusiness = true;
        }
        #endregion

        List<_FileInfo> items;

        private void PreapreFilesInfo()
        {
            List<_FileInfo> ret = new List<_FileInfo>(files.Length);

            foreach (var file_path in files)
            {
                if (File.Exists(file_path) || Directory.Exists(file_path))
                {
                    ret.Add(new _FileInfo(file_path));
                }
            }
            items = ret;
            this.Dispatcher.Invoke((DispatcherInvoke)SetItemsSource);
        }

        private void SetItemsSource()
        {
            Fileslist.ItemsSource = items;
            preparingLabel.Visibility = Visibility.Collapsed;
            Fileslist.Visibility = Visibility.Visible;
        }

        private static ImageSource getThumb(string path)
        {
            System.Drawing.Bitmap image = ShellThumbnailsService.GetThumbOrIcon(path);

            if (image == null)
            {
                ///return ((ImageSource)new FileToIconConverter().GetImage(path, 128));
                ///
                image = ShellIcon.GetVistaIcon(path).ToBitmap();
            }
            return image.ToWpfBitmap();
        }

        public class _FileInfo
        {
            public _FileInfo(string path)
            {
                Name = System.IO.Path.GetFileName(path);
                Path = path;

                Thumb = getThumb(path);
                Thumb.Freeze();
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

            public ImageSource Thumb
            {
                get;
                set;
            }

            public string ToolTipInfo
            {
                get { return "Path: " + Path;/* +"\r\nSize: " + Size;*/ }
            }
        }
    }
}
