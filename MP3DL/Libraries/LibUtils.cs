using System;
using System.Drawing;

namespace MP3DL.Libraries
{
    internal class LibUtils
    {
        public static string ClearChars(string input)
        {
            string tmp = input;
            tmp = tmp.Replace('/', '-');
            tmp = tmp.Replace('|', ' ');
            tmp = tmp.Replace('\"', ' ');
            tmp = tmp.Replace('[', ' ');
            tmp = tmp.Replace(']', ' ');
            tmp = tmp.Replace('{', ' ');
            tmp = tmp.Replace('}', ' ');
            tmp = tmp.Replace('\'', ' ');
            tmp = tmp.Replace(',', ' ');
            tmp = tmp.Replace('.', ' ');
            tmp = tmp.Replace(':', ' ');
            tmp = tmp.Replace('?', ' ');
            return tmp;
        }
        public static string IsolateJPG(string LINK)
        {
            System.Diagnostics.Debug.WriteLine(LINK);
            try
            {
                int a = LINK.LastIndexOf(".jpg");
                return LINK.Substring(0, a + 4);
            }
            catch
            {
                return "";
            }
        }
        public static System.Drawing.Image CropToSquare(System.Drawing.Image originalImage, int offset)
        {
            int squaredimensions = Math.Min(originalImage.Height, originalImage.Width) - offset;
            Size squareSize = new Size(squaredimensions, squaredimensions);
            Bitmap squareImage = new Bitmap(squareSize.Width, squareSize.Height);
            using (Graphics graphics = Graphics.FromImage(squareImage))
            {
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                graphics.DrawImage(originalImage, 
                    (squareSize.Width / 2) - (originalImage.Width / 2), (squareSize.Height / 2) - (originalImage.Height / 2), originalImage.Width, originalImage.Height);
            }
            return squareImage;
        }
    }
}
