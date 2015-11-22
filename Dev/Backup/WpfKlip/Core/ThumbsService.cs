using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace WpfKlip.Core
{
    public class ThumbsService
    {
        public static System.Drawing.Bitmap GetIcon(string path)
        {
            if (File.Exists(path))
            {
                
            }
            else if (Directory.Exists(path))
            {

            }
            else
            {
                return Properties.Resources.file_not_found_icon;
            }
            throw new NotImplementedException();
        }

        public static System.Drawing.Bitmap GetThumbnailOrIcon(string path)
        {
            throw new NotImplementedException();
        }
    }
}
