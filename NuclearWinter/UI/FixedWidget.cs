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
            if( mChild == null )
            {
                return;
            }

            Rectangle childRectangle;

            // Horizontal
            if( ChildBox.Left.HasValue )
            {
                childRectangle.X = _rect.Left + ChildBox.Left.Value;
                if( ChildBox.Right.HasValue )
                {
                    // Horizontally anchored
                    childRectangle.Width = ( _rect.Right - ChildBox.Right.Value ) - childRectangle.X;
                }
                else
                {
                    // Left-anchored
                    childRectangle.Width = ChildBox.Width;
                }
            }
            else
            {
                childRectangle.Width = ChildBox.Width;

                if( ChildBox.Right.HasValue )
                {
                    // Right-anchored
                    childRectangle.X = ( _rect.Right - ChildBox.Right.Value ) - childRectangle.Width;
                }
                else
                {
                    // Centered
                    childRectangle.X = _rect.Center.X - childRectangle.Width / 2;
                }
            }

            // Vertical
            if( ChildBox.Top.HasValue )
            {
                childRectangle.Y = _rect.Top + ChildBox.Top.Value;
                if( ChildBox.Bottom.HasValue )
                {
                    // Horizontally anchored
                    childRectangle.Height = ( _rect.Bottom - ChildBox.Bottom.Value ) - childRectangle.Y;
                }
                else
                {
                    // Top-anchored
                    childRectangle.Height = ChildBox.Height;
                }
            }
            else
            {
                childRectangle.Height = ChildBox.Height;

                if( ChildBox.Bottom.HasValue )
                {
                    // Bottom-anchored
                    childRectangle.Y = ( _rect.Bottom - ChildBox.Bottom.Value ) - childRectangle.Height;
                }
                else
                {
                    // Centered
                    childRectangle.Y = _rect.Center.Y - childRectangle.Height / 2;
                }
            }

            LayoutRect = childRectangle;

            switch( mContentAnchor )
            {
                case Anchor.Start:
                    Child.DoLayout( new Rectangle( LayoutRect.X, LayoutRect.Y, Math.Min( LayoutRect.Width, Child.ContentWidth ), LayoutRect.Height ) );
                    break;
                case Anchor.Center:
                case Anchor.Fill: // FIXME: Implement Fill anchor behavior
                    Child.DoLayout( LayoutRect );
                    break;
                case Anchor.End:
                    Child.DoLayout( new Rectangle( LayoutRect.Right - Child.ContentWidth, LayoutRect.Y, Child.ContentWidth, LayoutRect.Height ) );
                    break;
            }

            Debug.Assert( mChild.ContentWidth <= LayoutRect.Width && mChild.ContentHeight <= LayoutRect.Height, "Child is too big for its parent" );
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

        internal override void Update( float _fElapsedTime )
        {
            if( Child != null )
            {
                Child.Update( _fElapsedTime );
            }
            else
            {
                base.Update( _fElapsedTime );
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
