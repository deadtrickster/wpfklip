using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Controls;
using System.IO;
using WpfKlip.Properties;
using System.Windows.Input;
using WpfKlip.Core.Win.Structs;
using WpfKlip.Core.Win;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Security.Cryptography;
using System.Windows.Media.Effects;

namespace WpfKlip.Core
{
    public delegate void ItemCopiedEventHandler(ListBoxItem item, ClickType clickType);

    delegate void DispatcherInvoke();
    public class MouseCommand
    {
        public const int none = 0;
        public const int copy = 1;
        public const int paste = 2;
        public const int remove = 3;


        public static int GetCommandForClick(ClickType click)
        {
            int command = MouseCommand.none;
            switch (click)
            {
                case ClickType.Click:
                    command = Settings.Default.Click;
                    break;
                case ClickType.MidClick:
                    command = Settings.Default.MidClick;
                    break;
                case ClickType.RightClick:
                    command = Settings.Default.RightClick;
                    break;
            }
            return command;
        }
    }

    public enum ClickType
    {
        Click,
        MidClick,
        RightClick
    }

    internal abstract class DataEnabledListBoxItem : ListBoxItem
    {

        protected static bool VistaAndAbove = Environment.OSVersion.Version.Major > 5;

        protected CapturedItemsListController controller;

        protected DataEnabledListBoxItem(CapturedItemsListController controller)
        {
            this.controller = controller;
            this.AddHandler(System.Windows.Input.Mouse.MouseDownEvent, new MouseButtonEventHandler(HandleClick), true);
        }
        public void Copy()
        {
            try
            {
                CopyImpl();
            }
            catch
            {
                try
                {
                    CopyImpl();
                }
                catch(COMException/*Exception e*/)
                {
                    MessageBox.Show("An error occured while performing clipboard operation (copy)");
                }
            }
        }

        public abstract void CopyImpl();

        protected void HandleClick(object obj, MouseButtonEventArgs e)
        {
            ListBoxItem sender = obj as ListBoxItem;
            int command = MouseCommand.none;
            ClickType click = ClickType.Click;
            if (e.ClickCount == 1)
                if (e.ChangedButton == MouseButton.Left)
                {
                    click = ClickType.Click;
                }
                else if (e.ChangedButton == MouseButton.Right)
                {
                    click = ClickType.RightClick;
                }
                else
                {
                    click = ClickType.MidClick;
                }
            else
            {
                return;
            }

            command = MouseCommand.GetCommandForClick(click);

            DoMouseCommand(command);

            if (ItemClicked != null)
                ItemClicked(this, click);
        }

        public void DoMouseCommand(int command)
        {
            switch (command)
            {
                case MouseCommand.copy:
                    Copy();
                    break;
                case MouseCommand.paste:

                    User32.SetForegroundWindow(controller.ActiveWindow);
                    Copy();
                    press((int)System.Windows.Forms.Keys.ControlKey);
                    press((int)System.Windows.Forms.Keys.V);
                    release((int)System.Windows.Forms.Keys.V);
                    // User32.SendMessage(activeWindow, (int)WindowMessages.WM_CHAR, CtrlV, 0);
                    release((int)System.Windows.Forms.Keys.ControlKey);
                    User32.SetFocus(controller.ActiveWindow);
                    break;
                case MouseCommand.remove:
                    (this.Parent as ListBox).Items.Remove(this);
                    break;
            }
            // handleClip = true;

        }

        void press(int scanCode)
        {
            sendKey((ushort)scanCode, true);
        }

        void release(int scanCode)
        {
            sendKey((ushort)scanCode, false);
        }

        const uint INPUT_KEYBOARD = 1;
        const int KEY_EXTENDED = 0x0001;
        const uint KEY_UP = 0x0002;
        const uint KEY_SCANCODE = 0x0008;

        private void sendKey(ushort scanCode, bool press)
        {
            KEYBOARD_INPUT[] input = new KEYBOARD_INPUT[1];
            input[0] = new KEYBOARD_INPUT();
            input[0].type = INPUT_KEYBOARD;
            // input[0].flags = KEY_SCANCODE;

            if ((scanCode & 0xFF00) == 0xE000)
            { // extended key? 
                input[0].flags |= KEY_EXTENDED;
            }

            if (press)
            { // press? 
                input[0].vk = (ushort)(scanCode & 0xFF);
            }
            else
            { // release? 
                input[0].vk = scanCode;
                input[0].flags |= KEY_UP;
            }

            uint result = User32.SendInput(1, input, Marshal.SizeOf(input[0]));

            if (result != 1)
            {
                throw new Exception("Could not send key: " + scanCode);
            }
        }

        public System.Windows.Controls.Control SettingsPanel
        {
            get { return null; }
        }

        public event ItemCopiedEventHandler ItemClicked;

        protected string Title
        {
            set
            {
                TextBlock l = new TextBlock();
                l.TextWrapping = TextWrapping.NoWrap;
                l.Text = value;
                Content = l;
            }
        }
    }

    internal class TextDataLBI : DataEnabledListBoxItem
    {
        string text;
        public String Text
        {
            get { return text; }
        }

        string rtf;
        public String Rtf
        {
            get { return rtf; }
        }

        public TextDataLBI(IDataObject dataObject)
            :base(CapturedItemsListController.Instance)
        {
            this.text = Clipboard.GetText();

            if (dataObject.GetFormats().Contains(DataFormats.Rtf))
            {
                rtf = dataObject.GetData(DataFormats.Rtf) as string;
            }

            this.Title = CreatePreviewTitleString(text);
            this.Tag = text;
            this.MinHeight = 25;
            this.ToolTip = CreatePreviewBallonString(text);
        }

        #region new item processing
        private string CreatePreviewTitleString(string str)
        {
            str = str.Trim();
            return CreateTitleText(str, str.Length);
        }

        private static string CreatePreviewBallonString(string str)
        {
            int len = Settings.Default.BallonLength;
            return CreateBallonText(str, len);
        }

        private static string CreateBallonText(string str, int len)
        {
            int index = 0;
            StringReader sr = new StringReader(str);
            StringBuilder sb = new StringBuilder();
            int ich = sr.Read();
            bool prevspace = false;
            while (ich != -1 && index < len)
            {
                char ch = (char)ich;
                if (Char.IsWhiteSpace(ch))
                {
                    if (!prevspace)
                    {
                        prevspace = true;
                        sb.Append(ch);
                        index++;
                    }
                }
                else
                {
                    prevspace = false;
                    sb.Append(ch);
                    index++;
                }
                ich = sr.Read();
            }

            if (str.Length > len)
                sb.Append("...");

            return sb.ToString();
        }

        private static string CreateTitleText(string str, int len)
        {
            str = str.Replace("\r", " ").Replace("\n", " ");


            int index = 0;
            StringReader sr = new StringReader(str);
            StringBuilder sb = new StringBuilder();
            int ich = sr.Read();
            bool prevspace = false;
            while (ich != -1 && index < len)
            {
                char ch = (char)ich;
                if (Char.IsWhiteSpace(ch))
                {
                    if (!prevspace)
                    {
                        prevspace = true;
                        sb.Append(ch);
                        index++;
                    }
                }
                else
                {
                    prevspace = false;
                    sb.Append(ch);
                    index++;
                }
                ich = sr.Read();
            }

            return sb.ToString();
        }

        #endregion

        public override void CopyImpl()
        {
            if (rtf != null)
            {
                var dataObject = new DataObject();
                dataObject.SetData(DataFormats.Rtf, rtf);
                dataObject.SetText(text);
                Clipboard.SetDataObject(dataObject);
            }
            else
            {
                System.Windows.Clipboard.SetText(text);
            }
        }
    }

    internal class FileDropsLBI : DataEnabledListBoxItem
    {
        string[] files;

        public string[] Files
        {
            get { return files; }
            set { files = value; }
        }
        FileTooltip tooltip;
        public FileDropsLBI(string[] files)
            :base(CapturedItemsListController.Instance)
        {
            this.files = files;

            this.Title = CreateTitleString(files);
            this.Tag = files;
            this.MinHeight = 25;
            

            MouseLeave += new MouseEventHandler(FileDropsLBI_MouseLeave);
            MouseEnter += new MouseEventHandler(FileDropsLBI_MouseEnter);
            tooltip = new FileTooltip(files);
            //tooltip.Topmost = true;
            //tooltip.Background = new  LinearGradientBrush(Colors.White, Colors.Gray,90);


            close_timer.Elapsed += new System.Timers.ElapsedEventHandler(close_timer_Elapsed);
            close_timer.Interval = 200;

            open_timer.Elapsed += new System.Timers.ElapsedEventHandler(open_timer_Elapsed);
            open_timer.Interval = 300;
            //var tooltip = new FileDropFormatToolTip(files);
            //this.ToolTip = tooltip;
            //this.ToolTipClosing += new ToolTipEventHandler(FileDropsLBI_ToolTipClosing);
            //this.ToolTipOpening += new ToolTipEventHandler(FileDropsLBI_ToolTipOpening);
        }

        void open_timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke((DispatcherInvoke) ShowTooltip);
        }

        System.Timers.Timer open_timer = new System.Timers.Timer();
        void FileDropsLBI_MouseEnter(object sender, MouseEventArgs e)
        {
            if (tooltip.IsOpen)
            {
                tooltip.InBusiness = true;
            }
            else
            {
                close_timer.Enabled = false;
                close_timer.Stop();
                open_timer.Enabled = true;
                open_timer.Start();
            }
        }

        private void ShowTooltip()
        {

            var position = System.Windows.Forms.Cursor.Position;
            if (tooltip.Visibility == Visibility.Collapsed)
            {
                //tooltip.Top = position.Y + 20;
                // tooltip.Left = position.X + 20;
                tooltip.IsOpen = true;

                tooltip.Focus();
            }
            tooltip.InBusiness = true;
        }

        System.Timers.Timer close_timer = new System.Timers.Timer();

        void FileDropsLBI_MouseLeave(object sender, MouseEventArgs e)
        {
            open_timer.Enabled = false;
            open_timer.Stop();
            close_timer.Enabled = true;
            close_timer.Start();
            tooltip.InBusiness = false;
        }

        void close_timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke((DispatcherInvoke)TryHideTooltip);
        }

        void TryHideTooltip()
        {
            close_timer.Enabled = false;
            if (!tooltip.InBusiness)
            {
                tooltip.IsOpen = false;
            }
        }


        void FileDropsLBI_ToolTipOpening(object sender, ToolTipEventArgs e)
        {
           
        }

        void FileDropsLBI_ToolTipClosing(object sender, ToolTipEventArgs e)
        {
            e.Handled = true;
        }

        private string CreateTitleString(string[] files)
        {
            StringBuilder sb = new StringBuilder();
            if (files.Length > 1)
                sb.Append("FILES: ");
            else
                sb.Append("FILE: ");

            foreach (var file in files)
            {
                sb.Append(Path.GetFileName(file));
                sb.Append(' ');
            }

            return sb.ToString();
        }

        public override void CopyImpl()
        {
            System.Windows.Forms.Clipboard.SetData(System.Windows.Forms.DataFormats.FileDrop, files);
        }
    }

    internal class ImageLBI : DataEnabledListBoxItem
    {
        System.Windows.Media.Imaging.BitmapSource image;
        static int counter = 1;
        public ImageLBI(System.Drawing.Bitmap bitmap)
            :base(CapturedItemsListController.Instance)
        {

            System.Windows.Media.Imaging.BitmapSource image = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
              bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
              BitmapSizeOptions.FromEmptyOptions());

            this.image = image;
            Tag = new ImageHash(bitmap);

            this.MinHeight = 25;
            
            double width = image.Width;
            double height = image.Height;
            int scale_to = 200;

            Scale(ref width, ref height, scale_to);

            this.ToolTip = new Image { Source = image, Width = width, Height = height };

            this.Title = "IMAGE #" + counter++ + " (" + controller.ActiveProcess.ProcessName + ")";
        }

        private static void Scale(ref double width, ref double height, int scale_to)
        {
            double scale = width / height;
            if (scale > 1)
            {
                width = width > scale_to ? scale_to : width;
                height = width / scale;
            }
            else if (scale == 1)
            {
                width = height = width > scale_to ? scale_to : width;
            }
            else
            {
                height = height > scale_to ? scale_to : height;
                width = scale * height;
            }
        }

        public override void CopyImpl()
        {
            System.Windows.Forms.Clipboard.SetImage((Tag as ImageHash).Image);
        }
    }
}
