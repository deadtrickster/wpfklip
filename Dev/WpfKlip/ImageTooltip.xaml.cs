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

namespace WpfKlip
{
    /// <summary>
    /// Interaction logic for ImageTooltip.xaml
    /// </summary>
    public partial class ImageTooltip : UserControl
    {
        public ImageTooltip(System.Windows.Media.Imaging.BitmapSource image)
        {
            InitializeComponent();
            ImageSource = image;
            InvalidateVisual();
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ImageSource != default(BitmapSource);
        }

        public BitmapSource ImageSource
        {
            get { return (BitmapSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(BitmapSource), typeof(ImageTooltip), new UIPropertyMetadata(default(BitmapSource)));
    }
}
