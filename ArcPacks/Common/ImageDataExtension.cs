using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GameRes.Formats.Extension.Common
{
    public static class ImageDataExtension
    {
        public static byte[] GetBytes(this ImageData image)
        {
            var bitmap = image.Bitmap;
            int stride;
            if (PixelFormats.Bgr24 == bitmap.Format)
            {
                stride = (int)image.Width * 3;
            }
            else if (PixelFormats.Bgr32 == bitmap.Format)
            {
                stride = (int)(image.Width * 4);
            }
            else
            {
                stride = (int)(image.Width * 4);
                if (PixelFormats.Bgra32 != bitmap.Format)
                {
                    var converted_bitmap = new FormatConvertedBitmap();
                    converted_bitmap.BeginInit();
                    converted_bitmap.Source = image.Bitmap;
                    converted_bitmap.DestinationFormat = PixelFormats.Bgra32;
                    converted_bitmap.EndInit();
                    bitmap = converted_bitmap;
                }
            }

            var data = new byte[image.Height * stride];
            var row_data = new byte[stride];
            var rect = new Int32Rect(0, 0, (int)image.Width, 1);

            for (uint row = 0; row < image.Height; ++row)
            {
                bitmap.CopyPixels(rect, row_data, stride, 0);
                rect.Y++;
                row_data.CopyTo(data, row * stride);
            }



            return data;
        }

        public static byte[] GetBytes(this ImageData image, PixelFormat pixelFormat)
        {
            int stride = (int)(image.Width * pixelFormat.BitsPerPixel / 8 + 3) & ~3;

            var converted_bitmap = new FormatConvertedBitmap();
            converted_bitmap.BeginInit();
            converted_bitmap.Source = image.Bitmap;
            converted_bitmap.DestinationFormat = pixelFormat;
            converted_bitmap.EndInit();
            var bitmap = converted_bitmap;

            var data = new byte[image.Height * stride];
            var row_data = new byte[stride];
            var rect = new Int32Rect(0, 0, (int)image.Width, 1);

            for (uint row = 0; row < image.Height; ++row)
            {
                bitmap.CopyPixels(rect, row_data, stride, 0);
                rect.Y++;
                row_data.CopyTo(data, row * stride);
            }

            return data;
        }
    }
}