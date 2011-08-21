using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace NuclearWinter.UI
{
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

        public Point            Position;
        public Point            Size;

        protected Box           mPadding;
        public Box              Padding
        {
            get { return mPadding; }

            set {
                mPadding = value;

                UpdateContentSize();
            }
        }

        public abstract bool CanFocus { get; }
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
            if( CanFocus )
            {
                return this;
            }
            else
            {
                return null;
            }
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

        public abstract void    DoLayout( Rectangle? _rect );

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

        public virtual bool     OnPadButton ( Buttons _button, bool _bIsDown ) { return false; }

        public virtual bool     Update( float _fElapsedTime ) { return false; }
        protected abstract void UpdateContentSize();

        //----------------------------------------------------------------------
        // Events
        public virtual void     OnMouseDown ( Point _hitPoint ) {}
        public virtual void     OnMouseUp   ( Point _hitPoint ) {}

        public virtual void     OnMouseEnter( Point _hitPoint ) {}
        public virtual void     OnMouseOut  ( Point _hitPoint ) {}
        public virtual void     OnMouseMove ( Point _hitPoint ) {}

        public virtual void     OnMouseWheel( int _iDelta ) {}

        public virtual void     OnKeyPress  ( Keys _key ) {}
        public virtual void     OnTextEntered( char _char ) {}

        public virtual void     OnActivateDown() {}
        public virtual void     OnActivateUp() {}
        public virtual bool     OnCancel( bool _bPressed ) { return false; } // return true to consume the event

        public virtual void     OnFocus() {}
        public virtual void     OnBlur() {}

        public virtual void     OnPadMove( Direction _direction ) {
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
        public abstract void    Draw();
        public virtual void     DrawFocused() {}
    }
}
