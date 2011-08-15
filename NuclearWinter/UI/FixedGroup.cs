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
        Widget mChild;

        public Widget       Child {
            get { return mChild; }
            set {
                if( mChild != null )
                {
                    Debug.Assert( mChild.Parent == this );
                    mChild.Parent = null;
                }

                Debug.Assert( value.Parent == null );
                mChild = value;
                mChild.Parent = this;
            }
        }

        public Rectangle    ChildRectangle;

        Anchor              mAnchor;

        public override Widget GetFirstFocusableDescendant(Direction _direction)
        {
            return Child.GetFirstFocusableDescendant( _direction );
        }

        public override bool CanFocus
        {
            get { return false; }
        }
        //public override bool CanFocus { get { return Child != null && Child.CanFocus; } }

        public FixedWidget( Widget _child, Rectangle _rect, Anchor _anchor )
        : base( _child.Screen )
        {
            Child           = _child;
            ChildRectangle  = _rect;
            mAnchor         = _anchor;
        }

        public FixedWidget( Widget _child, Rectangle _rect )
        : this( _child, _rect, Anchor.Center )
        {
        }


        protected override void UpdateContentSize()
        {
        }

        public override void DoLayout( Rectangle? _rect )
        {
            if( _rect.HasValue )
            {
                ChildRectangle = _rect.Value;
            }

            Position = ChildRectangle.Location;
            Size = new Point( ChildRectangle.Width, ChildRectangle.Height );

            switch( mAnchor )
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

        public override void Draw()
        {
            Child.Draw();
        }
    }

    //--------------------------------------------------------------------------
    public class FixedGroup: Widget
    {
        List<FixedWidget>           mlChildren;

        public void Clear()
        {
            mlChildren.RemoveAll( delegate(FixedWidget _widget) { _widget.Parent = null; return true; } );
        }

        public void AddChild( FixedWidget _fixedWidget )
        {
            Debug.Assert( _fixedWidget.Parent == null );

            _fixedWidget.Parent = this;
            mlChildren.Add( _fixedWidget );
        }

        public void RemoveChild( FixedWidget _fixedWidget )
        {
            Debug.Assert( _fixedWidget.Parent == this );

            _fixedWidget.Parent = null;
            mlChildren.Remove( _fixedWidget );
        }

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
            foreach( FixedWidget fixedWidget in mlChildren )
            {
                fixedWidget.DoLayout( fixedWidget.ChildRectangle );
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
