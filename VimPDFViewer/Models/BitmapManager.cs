using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Media.Imaging;

namespace VimPDFViewer.Models
{
    internal class BitmapManager
    {
        /// <summary>
        /// BitmapImageをBitmapに
        /// </summary>
        /// <param name="bitmapImage"></param>
        /// <returns></returns>
        internal static Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                Bitmap bitmap = new Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        /// <summary>
        /// BitmapをBitmapImageに
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        internal static BitmapImage Bitmap2BitmapImage(Bitmap bmp)
        {
            BitmapImage bitmapImage = new BitmapImage();
            using (MemoryStream memory = new MemoryStream())
            {
                bmp.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
            }
            return bitmapImage;
        }

        internal static BitmapImage imageCombine(BitmapImage[] bmpimg)
        {
            Bitmap[] bmp = new Bitmap[bmpimg.Length];
            for (int i = 0; i < bmpimg.Length; i++)
            {
                bmp[i] = BitmapImage2Bitmap(bmpimg[i]);
            }
            int dstWidth = 0, dstHeight = 0;
            for (int i = 0; i < bmp.Length; i++)
            {
                if (dstWidth < bmp[i].Width)
                {
                    dstWidth = bmp[i].Width;
                }
                dstHeight += bmp[i].Height;
            }

            var dst = new Bitmap(dstWidth, dstHeight);
            var dstRect = new Rectangle();
            using (var gs = Graphics.FromImage(dst))
            {
                for (int i = 0; i < bmp.Length; i++)
                {
                    dstRect.Width = bmp[i].Width;
                    dstRect.Height = bmp[i].Height;
                    gs.DrawImage(bmp[i], dstRect, 0, 0, bmp[i].Width, bmp[i].Height, GraphicsUnit.Pixel);
                    dstRect.Y = dstRect.Bottom;
                }
            }
            return Bitmap2BitmapImage(dst);
        }


    }
}
