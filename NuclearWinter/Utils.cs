using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace NuclearWinter
{
    public class Utils
    {
        //----------------------------------------------------------------------
        // Source: http://thedeadpixelsociety.com/2012/01/hex-colors-in-xna/
        public static Color ColorFromHex(string value)
        {
            if (value.StartsWith("#")) value = value.Substring(1);

            uint hex = uint.Parse(value, System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture);

            Color color = Color.White;
            if (value.Length == 8)
            {
                color.R = (byte)(hex >> 24);
                color.G = (byte)(hex >> 16);
                color.B = (byte)(hex >> 8);
                color.A = (byte)(hex);
            }
            else
            if (value.Length == 6)
            {
                color.R = (byte)(hex >> 16);
                color.G = (byte)(hex >> 8);
                color.B = (byte)(hex);
            }
            else
            {
                throw new InvalidOperationException("Invald hex representation of an ARGB or RGB color value.");
            }

            return color;
        }

        //----------------------------------------------------------------------
        // There is no Enum.GetValues() on the Xbox 360
        // See http://forums.xna.com/forums/p/1610/157478.aspx
        public static List<T> GetValues<T>()
        {
            Type currentEnum = typeof(T);
            List<T> resultSet = new List<T>();
            if (currentEnum.IsEnum)
            {
                FieldInfo[] fields = currentEnum.GetFields(BindingFlags.Static | BindingFlags.Public);
                foreach (FieldInfo field in fields)
                    resultSet.Add((T)field.GetValue(null));
            }

            return resultSet;
        }

        //----------------------------------------------------------------------
        public static bool SegmentIntersect(ref Vector2 a, ref Vector2 b, ref Vector2 c, ref Vector2 d, out Vector2 intersectionPoint)
        {
            float fCoeff1;
            float fCoeff2;
            return SegmentIntersect(ref a, ref b, ref c, ref d, out intersectionPoint, out fCoeff1, out fCoeff2);
        }

        //----------------------------------------------------------------------
        // Based on implementation found in Farseer Physics 2
        public static bool SegmentIntersect(ref Vector2 a, ref Vector2 b, ref Vector2 c, ref Vector2 d, out Vector2 intersectionPoint, out float coeff1, out float coeff2)
        {
            intersectionPoint = new Vector2();
            coeff1 = 0f;
            coeff2 = 0f;

            float fA = d.Y - c.Y;
            float fB = b.X - a.X;
            float fC = d.X - c.X;
            float fD = b.Y - a.Y;

            // Denominator to solution of linear system
            float fDenom = (fA * fB) - (fC * fD);

            // If denominator is 0, then lines are parallel
            if (!(fDenom >= -sfEpsilon && fDenom <= sfEpsilon))
            {
                float fE = a.Y - c.Y;
                float fF = a.X - c.X;
                float fOneOverDenom = 1f / fDenom;

                // Numerator of first equation
                float fUA = (fC * fE) - (fA * fF);
                fUA *= fOneOverDenom;

                // Check if intersection point of the two lines is on line segment 1
                if (fUA >= 0f && fUA <= 1f)
                {
                    // Numerator of second equation
                    float fUB = (fB * fE) - (fD * fF);
                    fUB *= fOneOverDenom;

                    // Check if intersection point of the two lines is on line segment 2
                    // means the line segments intersect, since we know it is on
                    // segment 1 as well.
                    if (fUB >= 0f && fUB <= 1f)
                    {
                        // Check if they are coincident (no collision in this case)
                        if (fUA != 0f && fUB != 0f)
                        {
                            // There is an intersection
                            intersectionPoint.X = a.X + fUA * fB;
                            intersectionPoint.Y = a.Y + fUA * fD;

                            coeff1 = fUA;
                            coeff2 = fUB;

                            return true;
                        }
                    }
                }
            }

            return false;
        }

        //----------------------------------------------------------------------
        private const float sfEpsilon = .00001f;
    }
}