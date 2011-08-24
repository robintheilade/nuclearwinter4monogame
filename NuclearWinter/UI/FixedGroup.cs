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
        public AnchoredRect             ChildBox;
        public Rectangle        ChildRectangle;

        Anchor                  mContentAnchor;

        //----------------------------------------------------------------------
        FixedWidget( Screen _screen, Widget _child, AnchoredRect _box, Anchor _contentAnchor )
        : base( _screen )
        {
            Child           = _child;
            ChildBox        = _box;
            mContentAnchor  = _contentAnchor;
        }

        public FixedWidget( Screen _screen, AnchoredRect _box, Anchor _contentAnchor )
        : this( _screen, null, _box, _contentAnchor )
        {

        }

        public FixedWidget( Screen _screen, AnchoredRect _box )
        : this( _screen, null, _box, Anchor.Center )
        {

        }

        public FixedWidget( Widget _widget, AnchoredRect _box, Anchor _contentAnchor )
        : this( _widget.Screen, _widget, _box, _contentAnchor )
        {

        }

        public FixedWidget( Widget _widget, AnchoredRect _box )
        : this( _widget.Screen, _widget, _box, Anchor.Center )
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
        public override void DoLayout( Rectangle _rect )
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
        public override void DoLayout( Rectangle _rect )
        {
            foreach( FixedWidget fixedWidget in mlChildren )
            {
                Rectangle childRectangle;

                // Horizontal
                if( fixedWidget.ChildBox.Left.HasValue )
                {
                    childRectangle.X = _rect.Left + fixedWidget.ChildBox.Left.Value;
                    if( fixedWidget.ChildBox.Right.HasValue )
                    {
                        // Horizontally anchored
                        childRectangle.Width = ( _rect.Right - fixedWidget.ChildBox.Right.Value ) - childRectangle.X;
                    }
                    else
                    {
                        // Left-anchored
                        childRectangle.Width = fixedWidget.ChildBox.Width;
                    }
                }
                else
                {
                    childRectangle.Width = fixedWidget.ChildBox.Width;

                    if( fixedWidget.ChildBox.Right.HasValue )
                    {
                        // Right-anchored
                        childRectangle.X = ( _rect.Right - fixedWidget.ChildBox.Right.Value ) - childRectangle.Width;
                    }
                    else
                    {
                        // Centered
                        childRectangle.X = _rect.Center.X - childRectangle.Width / 2;
                    }
                }

                // Vertical
                if( fixedWidget.ChildBox.Top.HasValue )
                {
                    childRectangle.Y = _rect.Top + fixedWidget.ChildBox.Top.Value;
                    if( fixedWidget.ChildBox.Bottom.HasValue )
                    {
                        // Horizontally anchored
                        childRectangle.Height = ( _rect.Bottom - fixedWidget.ChildBox.Bottom.Value ) - childRectangle.Y;
                    }
                    else
                    {
                        // Top-anchored
                        childRectangle.Height = fixedWidget.ChildBox.Height;
                    }
                }
                else
                {
                    childRectangle.Height = fixedWidget.ChildBox.Height;

                    if( fixedWidget.ChildBox.Bottom.HasValue )
                    {
                        // Bottom-anchored
                        childRectangle.Y = ( _rect.Bottom - fixedWidget.ChildBox.Bottom.Value ) - childRectangle.Height;
                    }
                    else
                    {
                        // Centered
                        childRectangle.Y = _rect.Center.Y - childRectangle.Height / 2;
                    }
                }

                fixedWidget.DoLayout( childRectangle );
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
