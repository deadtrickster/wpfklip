using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace WpfKlip.Core
{
    /// <summary>
    /// http://www.codeproject.com/Messages/2913691/Comparing-one-image-to-many-others-speeded-up.aspx
    /// </summary>
    public sealed class ImageHash
    {
        private static SHA256Managed _shaM;
        private static SHA256Managed shaM
        {
            get
            {
                if (_shaM == null)
                    _shaM = new SHA256Managed();

                return _shaM;
            }
        }

        private static System.Drawing.ImageConverter _imageConverter;
        private static System.Drawing.ImageConverter imageConverter
        {
            get
            {
                if (_imageConverter == null)
                    _imageConverter = new System.Drawing.ImageConverter();

                return _imageConverter;
            }
        }

        public System.Drawing.Image Image { get; private set; }

        private byte[] _Hash;
        public byte[] Hash
        {
            get
            {
                if (_Hash == null)
                {
                    _Hash = (byte[])imageConverter.ConvertTo(Image, typeof(byte[]));
                    _Hash = shaM.ComputeHash(_Hash);
                }

                return _Hash;
            }
        }

        public ImageHash(System.Drawing.Image image)
        {
            this.Image = image;
        }


        public enum CompareResult
        {
            ciCompareOk,
            ciPixelMismatch,
            ciSizeMismatch
        };

        public static CompareResult Compare(ImageHash img1, ImageHash img2)
        {
            CompareResult cr = CompareResult.ciCompareOk;

            //Test to see if we have the same size of image

            if (img1.Image.Size != img2.Image.Size)
            {
                cr = CompareResult.ciSizeMismatch;
            }
            else
            {
                for (int i = 0; i < img1.Hash.Length && i < img2.Hash.Length
                                  && cr == CompareResult.ciCompareOk; i++)
                {
                    if (img1.Hash[i] != img2.Hash[i])
                        cr = CompareResult.ciPixelMismatch;
                }
            }
            return cr;
        }
    }
}
