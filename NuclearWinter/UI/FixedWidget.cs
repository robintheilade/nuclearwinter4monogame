using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace NuclearWinter.UI
{
    //--------------------------------------------------------------------------
    public class FixedWidget: Widget
    {
        //----------------------------------------------------------------------
        Widget mChild;
        public Widget       Child {
            get { return mChild; }
            set {
                if( mChild != null )
                {
                    Debug.Assert( mChild.Parent == this );
                    mChild.Parent = null;
                }

                mChild = value;
                if( mChild != null )
                {
                    Debug.Assert( mChild.Parent == null );
                    mChild.Parent = this;
                }

                UpdateContentSize();
            }
        }

        //----------------------------------------------------------------------
        public AnchoredRect     ChildBox;
        public Rectangle        ChildRectangle      { get; private set; }

        Anchor                  mContentAnchor;

        //----------------------------------------------------------------------
        FixedWidget( Screen _screen, Widget _child, AnchoredRect _box, Anchor _contentAnchor )
        : base( _screen )
        {
            Child           = _child;
            ChildBox        = _box;
            mContentAnchor  = _contentAnchor;
        }

        public FixedWidget( Screen _screen, AnchoredRect _box, Anchor _contentAnchor = Anchor.Center )
        : this( _screen, null, _box, _contentAnchor )
        {

        }

        public FixedWidget( Widget _widget, AnchoredRect _box, Anchor _contentAnchor = Anchor.Center )
        : this( _widget.Screen, _widget, _box, _contentAnchor )
        {

        }

        //----------------------------------------------------------------------
        public override Widget GetFirstFocusableDescendant( Direction _direction )
        {
            return ( Child != null ) ? Child.GetFirstFocusableDescendant( _direction ) : null;
        }

        //----------------------------------------------------------------------
        internal override void UpdateContentSize()
        {
            if( mChild != null )
            {
                ContentWidth    = mChild.ContentWidth;
                ContentHeight   = mChild.ContentHeight;
            }
            else
            {
                ContentWidth = 0;
                ContentHeight = 0;
            }

            base.UpdateContentSize();
        }

        //----------------------------------------------------------------------
        internal override void DoLayout( Rectangle _rect )
        {
            ChildRectangle = _rect;

            Position = ChildRectangle.Location;
            Size = new Point( ChildRectangle.Width, ChildRectangle.Height );

            if( mChild == null )
            {
                return;
            }

            switch( mContentAnchor )
            {
                case Anchor.Start:
                    Child.DoLayout( new Rectangle( Position.X, Position.Y, Child.ContentWidth, ChildRectangle.Height ) );
                    break;
                case Anchor.Center:
                case Anchor.Fill: // FIXME: Implement Fill anchor behavior
                    Child.DoLayout( new Rectangle( Position.X, Position.Y, Size.X, Size.Y ) );
                    break;
                case Anchor.End:
                    Child.DoLayout( new Rectangle( Position.X + Size.X - Child.ContentWidth, Position.Y, Child.ContentWidth, Size.Y ) );
                    break;
            }

            Debug.Assert( mChild.ContentWidth <= Size.X && mChild.ContentHeight <= Size.Y, "Child is too big for its parent" );
        }

        //----------------------------------------------------------------------
        public override Widget HitTest( Point _point )
        {
            if( Child != null )
            {
                return Child.HitTest( _point );
            }
            else
            {
                return base.HitTest( _point );
            }
        }

        internal override void OnMouseDown( Point _hitPoint, int _iButton )
        {
            if( Child != null )
            {
                Child.OnMouseDown( _hitPoint, _iButton );
            }
            else
            {
                base.OnMouseDown( _hitPoint, _iButton );
            }
        }

        internal override void OnMouseUp( Point _hitPoint, int _iButton )
        {
            if( Child != null )
            {
                Child.OnMouseUp( _hitPoint, _iButton );
            }
            else
            {
                base.OnMouseUp( _hitPoint, _iButton );
            }
        }

        internal override void OnMouseEnter( Point _hitPoint )
        {
            if( Child != null )
            {
                Child.OnMouseEnter( _hitPoint );
            }
            else
            {
                base.OnMouseEnter( _hitPoint );
            }
        }

        internal override void OnMouseOut( Point _hitPoint )
        {
            if( Child != null )
            {
                Child.OnMouseOut( _hitPoint );
            }
            else
            {
                base.OnMouseOut( _hitPoint );
            }
        }

        internal override void OnMouseMove( Point _hitPoint )
        {
            if( Child != null )
            {
                Child.OnMouseMove( _hitPoint );
            }
            else
            {
                base.OnMouseMove( _hitPoint );
            }
        }

        internal override bool OnPadButton( Buttons _button, bool _bIsDown )
        {
            if( Child != null )
            {
                return Child.OnPadButton( _button, _bIsDown );
            }
            else
            {
                return base.OnPadButton( _button, _bIsDown );
            }
        }

        internal override bool Update( float _fElapsedTime )
        {
            if( Child != null )
            {
                return Child.Update( _fElapsedTime );
            }
            else
            {
                return base.Update( _fElapsedTime );
            }
        }

        //----------------------------------------------------------------------
        internal override void Draw()
        {
            if( Child != null )
            {
                Child.Draw();
            }
        }
    }
}
