using Microsoft.Xna.Framework;
using System;

namespace NuclearWinter.Xna
{
    public static class XnaExtensions
    {
        public static Rectangle Clip(this Rectangle rectangle1, Rectangle rectangle2)
        {
            Rectangle rect = new Rectangle(
                Math.Max(rectangle1.X, rectangle2.X),
                Math.Max(rectangle1.Y, rectangle2.Y),
                0, 0);

            rect.Width = Math.Min(rectangle1.Right, rectangle2.Right) - rect.X;
            rect.Height = Math.Min(rectangle1.Bottom, rectangle2.Bottom) - rect.Y;

            if (rect.Width < 0)
            {
                rect.X += rect.Width;
                rect.Width = 0;
            }

            if (rect.Height < 0)
            {
                rect.Y += rect.Height;
                rect.Height = 0;
            }

            return rect;
        }

        public static Vector2 ToVector2(this Point _point)
        {
            return new Vector2(_point.X, _point.Y);
        }
    }
}
