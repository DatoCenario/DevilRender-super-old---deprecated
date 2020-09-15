using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Numerics;

namespace DevilRender
{
    public static class GraphicsExtensions
    {
        public static Point ToPoint(this Vector2 v)
        {
            return new Point((int)v.X, (int)v.Y);
        }
        public static bool InBitmap(this Bitmap bitmap, int x, int y)
        {
            return x >= 0 && x < bitmap.Width && y >= 0 && y < bitmap.Height;
        }

        public static Bitmap ResizeWithoutSmooth(Bitmap bitmap, Size newSize)
        {
            var resized = new Bitmap(newSize.Width, newSize.Height);
            float deltaWidth, deltaHeight; Bitmap minimal, other;
            if (newSize.Width * newSize.Height < bitmap.Width * bitmap.Height)
            {
                minimal = resized;
                other = bitmap;
            }
            else
            {
                minimal = bitmap;
                other = resized;
            }
            deltaWidth = (other.Width / minimal.Width);
            deltaHeight = (other.Height / minimal.Height);
            bool resizedMinimal = minimal == resized;
            var gr = Graphics.FromImage(resized);
            gr.Clear(Color.Black);
            for (int i = 0; i < minimal.Width; i++)
            {
                for (int g = 0; g < minimal.Height; g++)
                {
                    int di = (int)(i * deltaWidth);
                    int dg = (int)(g * deltaHeight);
                    if (resizedMinimal)
                    {
                        resized.SetPixel(i, g, bitmap.GetPixel(di, dg));
                    }
                    else
                    {
                        gr.FillRectangle(new SolidBrush(bitmap.GetPixel(i, g)),
                            new Rectangle(di, dg, (int)deltaWidth, (int)deltaHeight));
                    }
                }
            }
            return resized;
        }

        public static Bitmap ConcatTwoImages(Bitmap b1, Bitmap b2)
        {
            var concat = new Bitmap(b1.Width, b1.Height + b2.Height);
            var gr = Graphics.FromImage(concat);
            gr.DrawImage(b1, 0, 0);
            gr.DrawRectangle(Pens.White, new Rectangle(0, 0, b1.Width, b1.Height));
            gr.DrawImage(b2, 0, b1.Height);
            return concat;
        }
    }
}
