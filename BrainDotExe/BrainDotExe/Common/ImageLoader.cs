using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using BrainDotExe.Draw;
using BrainDotExe.Properties;
using BrainDotExe.Util;
using EloBuddy;

namespace BrainDotExe.Common
{
    public class ImageLoader
    {
        public static Bitmap Load(string championName)
        {
            var cachedPath = GetCachedPath(championName);
            if (File.Exists(cachedPath))
            {
                return ChangeOpacity(new Bitmap(cachedPath));
            }
            var bitmap = Resources.ResourceManager.GetObject(championName) as Bitmap;
            if (bitmap == null)
            {
                return ChangeOpacity(CreateFinalImage(Resources.Default));
            }
            var finalBitmap = CreateFinalImage(bitmap);
            finalBitmap.Save(cachedPath);
            return ChangeOpacity(finalBitmap);
        }

        private static string GetCachedPath(string championName)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CU");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            path = Path.Combine(path, Game.Version);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return Path.Combine(path, championName + ".png");
        }

        private static Bitmap CreateFinalImage(Bitmap srcBitmap)
        {
            var img = new Bitmap(srcBitmap.Width, srcBitmap.Width);
            var cropRect = new Rectangle(0, 0, srcBitmap.Width, srcBitmap.Width);

            using (Bitmap sourceImage = srcBitmap)
            {
                using (Bitmap croppedImage = sourceImage.Clone(cropRect, sourceImage.PixelFormat))
                {
                    using (var tb = new TextureBrush(croppedImage))
                    {
                        using (var g = Graphics.FromImage(img))
                        {
                            g.FillEllipse(tb, 0, 0, srcBitmap.Width, srcBitmap.Width);
                            var p = new Pen(Color.DarkRed, 2) { Alignment = PenAlignment.Inset };
                            g.DrawEllipse(p, 0, 0, srcBitmap.Width, srcBitmap.Width);
                        }
                    }
                }
            }
            srcBitmap.Dispose();
            return img;
        }

        private static Bitmap ChangeOpacity(Image img)
        {
            var bmp = new Bitmap(img.Width, img.Height);
            var graphics = Graphics.FromImage(bmp);
            var colormatrix = new ColorMatrix { Matrix33 = 15 };
            var imgAttribute = new ImageAttributes();
            imgAttribute.SetColorMatrix(colormatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            graphics.DrawImage(
                img, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel,
                imgAttribute);
            graphics.Dispose();
            img.Dispose();
            return bmp;
        }
    }
}
