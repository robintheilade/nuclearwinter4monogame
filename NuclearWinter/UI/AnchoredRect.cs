
using Microsoft.Xna.Framework;

namespace NuclearWinter.UI
{
    public struct AnchoredRect
    {
        public int?     Left;
        public int?     Top;

        public int?     Right;
        public int?     Bottom;

        public int      Width;
        public int      Height;

        public static AnchoredRect Full
        {
            get {
                return new AnchoredRect( 0, 0, 0, 0, 0, 0 );
            }
        }

        public bool HasWidth
        {
            get { return ! Left.HasValue || ! Right.HasValue; }
        }

        public bool HasHeight
        {
            get { return ! Top.HasValue || ! Bottom.HasValue; }
        }

        //----------------------------------------------------------------------
        public AnchoredRect( int? left, int? top, int? right, int? bottom, int width, int height )
        {
            Left    = left;
            Top     = top;

            Right   = right;
            Bottom  = bottom;

            Width   = width;
            Height  = height;
        }

        //----------------------------------------------------------------------
        public static AnchoredRect CreateFixed( int left, int top, int width, int height )
        {
            return new AnchoredRect( left, top, null, null, width, height );
        }

        public static AnchoredRect CreateFixed( Rectangle rectangle )
        {
            return new AnchoredRect( rectangle.Left, rectangle.Top, null, null, rectangle.Width, rectangle.Height );
        }

        public static AnchoredRect CreateFull( int value )
        {
            return new AnchoredRect( value, value, value, value, 0, 0 );
        }

        public static AnchoredRect CreateFull( int left, int top, int right, int bottom )
        {
            return new AnchoredRect( left, top, right, bottom, 0, 0 );
        }

        public static AnchoredRect CreateLeftAnchored( int left, int top, int bottom, int width )
        {
            return new AnchoredRect( left, top, null, bottom, width, 0 );
        }

        public static AnchoredRect CreateRightAnchored( int right, int top, int bottom, int width )
        {
            return new AnchoredRect( null, top, right, bottom, width, 0 );
        }

        public static AnchoredRect CreateTopAnchored( int left, int top, int right, int height )
        {
            return new AnchoredRect( left, top, right, null, 0, height );
        }

        public static AnchoredRect CreateBottomAnchored( int left, int bottom, int right, int height )
        {
            return new AnchoredRect( left, null, right, bottom, 0, height );
        }

        public static AnchoredRect CreateBottomLeftAnchored( int left, int bottom, int width, int height )
        {
            return new AnchoredRect( left, null, null, bottom, width, height );
        }

        public static AnchoredRect CreateBottomRightAnchored( int right, int bottom, int width, int height )
        {
            return new AnchoredRect( null, null, right, bottom, width, height );
        }

        public static AnchoredRect CreateTopRightAnchored( int right, int top, int width, int height )
        {
            return new AnchoredRect( null, top, right, null, width, height );
        }

        public static AnchoredRect CreateTopLeftAnchored( int left, int top, int width, int height )
        {
            return new AnchoredRect( left, top, null, null, width, height );
        }

        public static AnchoredRect CreateCentered( int width, int height )
        {
            return new AnchoredRect( null, null, null, null, width, height );
        }
    }
}
