using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Diagnostics;

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

        //----------------------------------------------------------------------
        public AnchoredRect( int? _iLeft, int? _iTop, int? _iRight, int? _iBottom, int _iWidth, int _iHeight )
        {
            Left    = _iLeft;
            Top     = _iTop;

            Right   = _iRight;
            Bottom  = _iBottom;

            Width   = _iWidth;
            Height  = _iHeight;
        }

        //----------------------------------------------------------------------
        public static AnchoredRect CreateFixed( int _iLeft, int _iTop, int _iWidth, int _iHeight )
        {
            return new AnchoredRect( _iLeft, _iTop, null, null, _iWidth, _iHeight );
        }

        public static AnchoredRect CreateFixed( Rectangle _rect )
        {
            return new AnchoredRect( _rect.Left, _rect.Top, null, null, _rect.Width, _rect.Height );
        }

        public static AnchoredRect CreateFull( int _iValue )
        {
            return new AnchoredRect( _iValue, _iValue, _iValue, _iValue, 0, 0 );
        }

        public static AnchoredRect CreateFull( int _iLeft, int _iTop, int _iRight, int _iBottom )
        {
            return new AnchoredRect( _iLeft, _iTop, _iRight, _iBottom, 0, 0 );
        }

        public static AnchoredRect CreateLeftAnchored( int _iLeft, int _iTop, int _iBottom, int _iWidth )
        {
            return new AnchoredRect( _iLeft, _iTop, null, _iBottom, _iWidth, 0 );
        }

        public static AnchoredRect CreateRightAnchored( int _iRight, int _iTop, int _iBottom, int _iWidth )
        {
            return new AnchoredRect( null, _iTop, _iRight, _iBottom, _iWidth, 0 );
        }

        public static AnchoredRect CreateTopAnchored( int _iLeft, int _iTop, int _iRight, int _iHeight )
        {
            return new AnchoredRect( _iLeft, _iTop, _iRight, null, 0, _iHeight );
        }

        public static AnchoredRect CreateBottomAnchored( int _iLeft, int _iBottom, int _iRight, int _iHeight )
        {
            return new AnchoredRect( _iLeft, null, _iRight, _iBottom, 0, _iHeight );
        }

        public static AnchoredRect CreateBottomLeftAnchored( int _iLeft, int _iBottom, int _iWidth, int _iHeight )
        {
            return new AnchoredRect( _iLeft, null, null, _iBottom, _iWidth, _iHeight );
        }

        public static AnchoredRect CreateBottomRightAnchored( int _iRight, int _iBottom, int _iWidth, int _iHeight )
        {
            return new AnchoredRect( null, null, _iRight, _iBottom, _iWidth, _iHeight );
        }

        public static AnchoredRect CreateTopRightAnchored( int _iRight, int _iTop, int _iWidth, int _iHeight )
        {
            return new AnchoredRect( null, _iTop, _iRight, null, _iWidth, _iHeight );
        }

        public static AnchoredRect CreateTopLeftAnchored( int _iLeft, int _iTop, int _iWidth, int _iHeight )
        {
            return new AnchoredRect( _iLeft, _iTop, null, null, _iWidth, _iHeight );
        }

        public static AnchoredRect CreateCentered( int _iWidth, int _iHeight )
        {
            return new AnchoredRect( null, null, null, null, _iWidth, _iHeight );
        }
    }

    public struct Box
    {
        //----------------------------------------------------------------------
        public int Top;
        public int Right;
        public int Bottom;
        public int Left;

        public int Horizontal   { get { return Left + Right; } }
        public int Vertical     { get { return Top + Bottom; } }

        public Box( int _iTop, int _iRight, int _iBottom, int _iLeft )
        {
            Top     = _iTop;
            Right   = _iRight;
            Bottom  = _iBottom;
            Left    = _iLeft;
        }

        //----------------------------------------------------------------------
        public Box( int _iValue )
        {
            Left = Right = Top = Bottom = _iValue;
        }

        //----------------------------------------------------------------------
        public Box( int _iVertical, int _iHorizontal )
        {
            Top = Bottom = _iVertical;
            Left = Right = _iHorizontal;
        }
    }

    public abstract class Widget
    {
        //----------------------------------------------------------------------
        public Screen           Screen              { get; private set; }
        public Widget           Parent;

        public int              ContentWidth        { get; protected set; }
        public int              ContentHeight       { get; protected set; }

        // FIXME: Replace this with a rectangle?
        public Point            Position            { get; protected set; }
        public Point            Size                { get; protected set; }

        protected Box           mPadding;
        public Box              Padding
        {
            get { return mPadding; }

            set {
                mPadding = value;

                UpdateContentSize();
            }
        }

        public virtual Widget GetSibling( UI.Direction _direction, Widget _child )
        {
            if( Parent != null )
            {
                return Parent.GetSibling( _direction, this );
            }

            return null;
        }

        public virtual Widget GetFirstFocusableDescendant( UI.Direction _direction )
        {
            return this;
        }

        public bool HasFocus
        {
            get
            {
                return Screen.FocusedWidget == this;
            }
        }

        public Rectangle        HitBox              { get; protected set; }

        //----------------------------------------------------------------------
        public Widget( Screen _screen )
        {
            Screen = _screen;
        }

        internal abstract void  DoLayout( Rectangle _rect );

        //----------------------------------------------------------------------
        public virtual Widget   HitTest( Point _point )
        {
            if( HitBox.Contains( _point ) )
            {
                return this;
            }
            else
            {
                return null;
            }
        }

        internal virtual bool     Update( float _fElapsedTime ) { return false; }

        internal virtual void UpdateContentSize()
        {
            // Compute content size then call this!

            if( Parent != null )
            {
                Parent.UpdateContentSize();
            }
        }

        //----------------------------------------------------------------------
        // Events
        internal virtual void       OnMouseDown ( Point _hitPoint ) {}
        internal virtual void       OnMouseUp   ( Point _hitPoint ) {}
        internal virtual void       OnMouseDoubleClick( Point _hitPoint ) {}

        internal virtual void       OnMouseEnter( Point _hitPoint ) {}
        internal virtual void       OnMouseOut  ( Point _hitPoint ) {}
        internal virtual void       OnMouseMove ( Point _hitPoint ) {}

        internal virtual void       OnMouseWheel( Point _hitPoint, int _iDelta ) {}

        internal virtual void       OnKeyPress  ( Keys _key ) {}
        internal virtual void       OnTextEntered( char _char ) {}

        internal virtual void       OnActivateDown() {}
        internal virtual void       OnActivateUp() {}
        internal virtual bool       OnCancel( bool _bPressed ) { return false; } // return true to consume the event

        internal virtual void       OnFocus() {}
        internal virtual void       OnBlur() {}

        internal virtual bool       OnPadButton ( Buttons _button, bool _bIsDown ) { return false; }

        internal virtual void     OnPadMove( Direction _direction ) {
            Widget widget = GetSibling( _direction, this );

            if( widget != null )
            {
                Widget focusableWidget = widget.GetFirstFocusableDescendant( _direction );

                if( focusableWidget != null )
                {
                    Screen.Focus( focusableWidget );
                }
            }
        }

        //----------------------------------------------------------------------
        internal abstract void  Draw();
        internal virtual void   DrawFocused() {}
    }
}
