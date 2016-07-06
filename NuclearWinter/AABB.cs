using Microsoft.Xna.Framework;
using System;

namespace NuclearWinter
{
    public struct AABB
    {
        //----------------------------------------------------------------------
        public Vector2 Min;
        public Vector2 Max;

        //----------------------------------------------------------------------
        public AABB(Vector2 min, Vector2 max)
        {
            Min = min;
            Max = max;
        }

        //----------------------------------------------------------------------
        public void Extend(Vector2 point)
        {
            Min.X = Math.Min(point.X, Min.X);
            Min.Y = Math.Min(point.Y, Min.Y);

            Max.X = Math.Max(point.X, Max.X);
            Max.Y = Math.Max(point.Y, Max.Y);
        }

        //----------------------------------------------------------------------
        public bool Intersects(AABB aabb)
        {
            return
                ((Min.X <= aabb.Max.X) || (Max.X >= aabb.Min.X))
                &&
                ((Min.Y <= aabb.Max.Y) || (Max.Y >= aabb.Min.Y));
        }

        //----------------------------------------------------------------------
        public bool Contains(AABB aabb)
        {
            return
                (Min.X <= aabb.Min.X && Max.X >= aabb.Max.X)
                &&
                (Min.Y <= aabb.Min.Y && Max.Y >= aabb.Max.Y);
        }

        //----------------------------------------------------------------------
        public bool Contains(Vector2 point)
        {
            return
                (Min.X <= point.X && Max.X >= point.X)
                &&
                (Min.Y <= point.Y && Max.Y >= point.Y);
        }

        //----------------------------------------------------------------------
        public bool Contains(Vector2 point, float axisDistance)
        {
            return
                (point.X >= Min.X - axisDistance)
            && (point.X <= Max.X + axisDistance)
            && (point.Y >= Min.Y - axisDistance)
            && (point.Y <= Max.Y + axisDistance);
        }

        //----------------------------------------------------------------------
        public float Width
        {
            get
            {
                return Math.Abs(Max.X - Min.X);
            }
        }

        //----------------------------------------------------------------------
        public float Height
        {
            get
            {
                return Math.Abs(Max.Y - Min.Y);
            }
        }

        public Vector2 Center
        {
            get
            {
                return (Min + Max) / 2f;
            }
        }
    }
}
