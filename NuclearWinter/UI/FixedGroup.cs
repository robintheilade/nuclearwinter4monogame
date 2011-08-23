using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using NuclearWinter;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

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
        public Rectangle        ChildRectangle;

        public Box              ChildBox;
        public BoxAnchor        ChildBoxAnchor;

        Anchor                  mContentAnchor;

        //----------------------------------------------------------------------
        FixedWidget( Screen _screen, Widget _child, Rectangle _rect, Anchor _contentAnchor )
        : base( _screen )
        {
            Child           = _child;
            
            ChildRectangle  = _rect;

            ChildBox        = new Box(0);
            ChildBoxAnchor  = BoxAnchor.None;

            mContentAnchor  = _contentAnchor;
        }

        //----------------------------------------------------------------------
        public FixedWidget( Screen _screen, Rectangle _rect, Anchor _contentAnchor )
        : this( _screen, null, _rect, _contentAnchor )
        {

        }

        public FixedWidget( Screen _screen, Rectangle _rect )
        : this( _screen, null, _rect, Anchor.Center )
        {

        }

        //----------------------------------------------------------------------
        public FixedWidget( Widget _child, Rectangle _rect, Anchor _contentAnchor )
        : this( _child.Screen, _child, _rect, _contentAnchor )
        {
        }

        public FixedWidget( Widget _child, Rectangle _rect )
        : this( _child.Screen, _child, _rect, Anchor.Center )
        {
        }

        //----------------------------------------------------------------------
        FixedWidget( Screen _screen, Widget _child, Box _box, BoxAnchor _anchor, Anchor _contentAnchor )
        : base( _screen )
        {
            Child           = _child;

            ChildRectangle  = Rectangle.Empty;

            ChildBox        = _box;
            ChildBoxAnchor  = _anchor;

            mContentAnchor  = _contentAnchor;
        }

        //----------------------------------------------------------------------
        public FixedWidget( Widget _child, Box _box, BoxAnchor _anchor, Anchor _contentAnchor )
        : this( _child.Screen, _child, _box, _anchor, _contentAnchor )
        {

        }

        public FixedWidget( Widget _child, Box _box, BoxAnchor _anchor )
        : this( _child.Screen, _child, _box, _anchor, Anchor.Center )
        {
        }

        //----------------------------------------------------------------------
        public FixedWidget( Screen _screen, Box _box, BoxAnchor _anchor, Anchor _contentAnchor )
        : this( _screen, null, _box, _anchor, _contentAnchor )
        {

        }

        public FixedWidget( Screen _screen, Box _box, BoxAnchor _anchor )
        : this( _screen, null, _box, _anchor, Anchor.Center )
        {
        }

        //----------------------------------------------------------------------
        public override Widget GetFirstFocusableDescendant( Direction _direction )
        {
            return ( Child != null ) ? Child.GetFirstFocusableDescendant( _direction ) : null;
        }

        public override bool CanFocus { get { return false; } }

        //----------------------------------------------------------------------
        protected override void UpdateContentSize()
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
        }

        //----------------------------------------------------------------------
        public override void DoLayout( Rectangle? _rect )
        {
            if( _rect.HasValue )
            {
                ChildRectangle = _rect.Value;
            }

            Position = ChildRectangle.Location;
            Size = new Point( ChildRectangle.Width, ChildRectangle.Height );

            switch( mContentAnchor )
            {
                case Anchor.Start:
                    Child.DoLayout( new Rectangle( Position.X, Position.Y, Child.ContentWidth, ChildRectangle.Height ) );
                    break;
                case Anchor.Center:
                    Child.DoLayout( new Rectangle( Position.X, Position.Y, Size.X, Size.Y ) );
                    break;
                case Anchor.End:
                    Child.DoLayout( new Rectangle( Position.X, Position.Y, Size.X - Child.ContentWidth, Size.Y ) );
                    break;
            }
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

        public override void OnMouseDown( Point _hitPoint )
        {
            if( Child != null )
            {
                Child.OnMouseDown( _hitPoint );
            }
            else
            {
                base.OnMouseDown( _hitPoint );
            }
        }

        public override void OnMouseUp( Point _hitPoint )
        {
            if( Child != null )
            {
                Child.OnMouseUp( _hitPoint );
            }
            else
            {
                base.OnMouseUp( _hitPoint );
            }
        }

        public override void OnMouseEnter( Point _hitPoint )
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

        public override void OnMouseOut( Point _hitPoint )
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

        public override void OnMouseMove( Point _hitPoint )
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

        public override bool OnPadButton( Buttons _button, bool _bIsDown )
        {
            if( Child != null )
            {
                return Child.OnPadButton( _button, _bIsDown );
            }
            else
            {
                return base.OnPadButton(_button, _bIsDown);
            }
        }

        public override bool Update( float _fElapsedTime )
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
        public override void Draw()
        {
            if( Child != null )
            {
                Child.Draw();
            }
        }
    }

    //--------------------------------------------------------------------------
    public class FixedGroup: Widget
    {
        //----------------------------------------------------------------------
        List<FixedWidget>           mlChildren;

        //----------------------------------------------------------------------
        public void Clear()
        {
            mlChildren.RemoveAll( delegate(FixedWidget _widget) { _widget.Parent = null; return true; } );
        }

        //----------------------------------------------------------------------
        public void AddChild( FixedWidget _fixedWidget )
        {
            Debug.Assert( _fixedWidget.Parent == null );

            _fixedWidget.Parent = this;
            mlChildren.Add( _fixedWidget );
        }

        //----------------------------------------------------------------------
        public void RemoveChild( FixedWidget _fixedWidget )
        {
            Debug.Assert( _fixedWidget.Parent == this );

            _fixedWidget.Parent = null;
            mlChildren.Remove( _fixedWidget );
        }

        //----------------------------------------------------------------------
        public override Widget GetFirstFocusableDescendant( Direction _direction )
        {
            FixedWidget firstChild = null;
            Widget focusableDescendant = null;

            foreach( FixedWidget child in mlChildren )
            {
                switch( _direction )
                {
                    case Direction.Up:
                        if( ( firstChild == null || child.ChildRectangle.Bottom > firstChild.ChildRectangle.Bottom ) )
                        {
                            Widget childFocusableWidget = child.GetFirstFocusableDescendant( _direction );
                            if( childFocusableWidget != null )
                            {
                                firstChild = child;
                                focusableDescendant = childFocusableWidget;
                            }
                        }
                        break;
                    case Direction.Down:
                        if( firstChild == null || child.ChildRectangle.Top < firstChild.ChildRectangle.Top )
                        {
                            Widget childFocusableWidget = child.GetFirstFocusableDescendant( _direction );
                            if( childFocusableWidget != null )
                            {
                                firstChild = child;
                                focusableDescendant = childFocusableWidget;
                            }
                        }
                        break;
                    case Direction.Left:
                        if( firstChild == null || child.ChildRectangle.Right > firstChild.ChildRectangle.Right )
                        {
                            Widget childFocusableWidget = child.GetFirstFocusableDescendant( _direction );
                            if( childFocusableWidget != null )
                            {
                                firstChild = child;
                                focusableDescendant = childFocusableWidget;
                            }
                        }
                        break;
                    case Direction.Right:
                        if( firstChild == null || child.ChildRectangle.Left < firstChild.ChildRectangle.Left )
                        {
                            Widget childFocusableWidget = child.GetFirstFocusableDescendant( _direction );
                            if( childFocusableWidget != null )
                            {
                                firstChild = child;
                                focusableDescendant = childFocusableWidget;
                            }
                        }
                        break;
                }
            }

            return focusableDescendant;
        }

        //----------------------------------------------------------------------
        public override Widget GetSibling( Direction _direction, Widget _child )
        {
            FixedWidget nearestSibling = null;
            Widget focusableSibling = null;

            FixedWidget fixedChild = (FixedWidget)_child;

            foreach( FixedWidget child in mlChildren )
            {
                if( child == _child ) continue;

                switch( _direction )
                {
                    case Direction.Up:
                        if( child.ChildRectangle.Bottom <= fixedChild.ChildRectangle.Center.Y && ( nearestSibling == null || child.ChildRectangle.Bottom > nearestSibling.ChildRectangle.Bottom ) )
                        {
                            Widget childFocusableWidget = child.GetFirstFocusableDescendant( _direction );
                            if( childFocusableWidget != null )
                            {
                                nearestSibling = child;
                                focusableSibling = childFocusableWidget;
                            }
                        }
                        break;
                    case Direction.Down:
                        if( child.ChildRectangle.Top >= fixedChild.ChildRectangle.Center.Y && ( nearestSibling == null || child.ChildRectangle.Top < nearestSibling.ChildRectangle.Top ) )
                        {
                            Widget childFocusableWidget = child.GetFirstFocusableDescendant( _direction );
                            if( childFocusableWidget != null )
                            {
                                nearestSibling = child;
                                focusableSibling = childFocusableWidget;
                            }
                        }
                        break;
                    case Direction.Left:
                        if( child.ChildRectangle.Right <= fixedChild.ChildRectangle.Center.X && ( nearestSibling == null || child.ChildRectangle.Right > nearestSibling.ChildRectangle.Right ) )
                        {
                            Widget childFocusableWidget = child.GetFirstFocusableDescendant( _direction );
                            if( childFocusableWidget != null )
                            {
                                nearestSibling = child;
                                focusableSibling = childFocusableWidget;
                            }
                        }
                        break;
                    case Direction.Right:
                        if( child.ChildRectangle.Left >= fixedChild.ChildRectangle.Center.X && ( nearestSibling == null || child.ChildRectangle.Left < nearestSibling.ChildRectangle.Left ) )
                        {
                            Widget childFocusableWidget = child.GetFirstFocusableDescendant( _direction );
                            if( childFocusableWidget != null )
                            {
                                nearestSibling = child;
                                focusableSibling = childFocusableWidget;
                            }
                        }
                        break;
                }
            }

            if( focusableSibling == null )
            {
                return base.GetSibling( _direction, this );
            }

            return focusableSibling;
        }

        public override bool CanFocus { get { return false; } }

        //----------------------------------------------------------------------
        public FixedGroup( Screen _screen )
        : base( _screen )
        {
            mlChildren = new List<FixedWidget>();
        }

        //----------------------------------------------------------------------
        protected override void UpdateContentSize()
        {
        }

        //----------------------------------------------------------------------
        public override void DoLayout( Rectangle? _rect )
        {
            Debug.Assert( _rect != null );

            foreach( FixedWidget fixedWidget in mlChildren )
            {

                // FIXME: Handle all cases and fix existing ones
                switch( fixedWidget.ChildBoxAnchor )
                {
                    case BoxAnchor.None:
                        fixedWidget.DoLayout( null );
                        break;
                    case BoxAnchor.Right: {
                        int iX = _rect.Value.Right - fixedWidget.ContentWidth - fixedWidget.ChildBox.Right;
                        int iY = _rect.Value.Top + fixedWidget.ChildBox.Top;
                        fixedWidget.DoLayout( new Rectangle(
                            iX,
                            iY,
                            _rect.Value.Right - iX,
                            _rect.Value.Bottom - fixedWidget.ChildBox.Bottom - iY
                            )
                        );
                        break;
                    }
                    case BoxAnchor.BottomRight: {
                        int iX = _rect.Value.Right - fixedWidget.ContentWidth;
                        int iY = _rect.Value.Bottom - fixedWidget.ContentHeight - fixedWidget.ChildBox.Bottom;
                        fixedWidget.DoLayout( new Rectangle(
                            iX,
                            iY,
                            _rect.Value.Right - fixedWidget.ChildBox.Right - iX,
                            _rect.Value.Bottom - iY
                            )
                        );
                        break;
                    }
                    case BoxAnchor.BottomLeft: {
                        int iX = _rect.Value.Left + fixedWidget.ChildBox.Left;
                        int iY = _rect.Value.Bottom - fixedWidget.ContentHeight - fixedWidget.ChildBox.Bottom;
                        fixedWidget.DoLayout( new Rectangle(
                            iX,
                            iY,
                            _rect.Value.Right - fixedWidget.ChildBox.Right - iX,
                            _rect.Value.Bottom - iY
                            )
                        );
                        break;
                    }
                    case BoxAnchor.Vertical:
                    case BoxAnchor.Vertical | BoxAnchor.Left: {
                        int iX = _rect.Value.Left + fixedWidget.ChildBox.Left;
                        int iY = _rect.Value.Top + fixedWidget.ChildBox.Top;

                        fixedWidget.DoLayout( new Rectangle(
                            iX,
                            iY,
                            _rect.Value.Right - fixedWidget.ChildBox.Right - iX,
                            _rect.Value.Bottom - fixedWidget.ChildBox.Bottom - iY
                            )
                        );
                        break;
                        break;
                    }
                    case BoxAnchor.Full: {
                        int iX = _rect.Value.Left + fixedWidget.ChildBox.Left;
                        int iY = _rect.Value.Top + fixedWidget.ChildBox.Top;

                        fixedWidget.DoLayout( new Rectangle(
                            iX,
                            iY,
                            _rect.Value.Right - fixedWidget.ChildBox.Right - iX,
                            _rect.Value.Bottom - fixedWidget.ChildBox.Bottom - iY
                            )
                        );
                        break;
                    }
                }
            }
            
            HitBox = Resolution.InternalMode.Rectangle;
        }

        //----------------------------------------------------------------------
        public override Widget HitTest( Point _point )
        {
            if( HitBox.Contains( _point ) )
            {
                Widget hitWidget;

                for( int iChild = mlChildren.Count - 1; iChild >= 0; iChild-- )
                {
                    FixedWidget fixedChild = mlChildren[iChild];
                    if( ( hitWidget = fixedChild.Child.HitTest( _point ) ) != null )
                    {
                        return hitWidget;
                    }
                }
            }

            return null;
        }

        //----------------------------------------------------------------------
        public override bool OnPadButton( Buttons _button, bool _bIsDown )
        {
            foreach( FixedWidget child in mlChildren )
            {
                if( child.Child.OnPadButton( _button, _bIsDown ) )
                {
                    return true;
                }
            }

            return false;
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            foreach( FixedWidget child in mlChildren )
            {
                child.Child.Draw();
            }
        }
    }
}
