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
    /*
     * Abstract base class for all types of group
     */
    public abstract class Group: Widget
    {
        protected List<Widget>      mlChildren;

        public void Clear()
        {
            mlChildren.RemoveAll( delegate(Widget _widget) { _widget.Parent = null; return true; } );
            UpdateContentSize();
        }

        public void AddChild( Widget _widget )
        {
            AddChild( _widget, mlChildren.Count );
        }

        public virtual void AddChild( Widget _widget, int _iIndex )
        {
            Debug.Assert( _widget.Parent == null );

            _widget.Parent = this;
            mlChildren.Insert( _iIndex, _widget );
            UpdateContentSize();
        }

        public virtual void RemoveChild( Widget _widget )
        {
            Debug.Assert( _widget.Parent == this );

            _widget.Parent = null;
            mlChildren.Remove( _widget );
            UpdateContentSize();
        }

        //----------------------------------------------------------------------
        public Group( Screen _screen )
        : base( _screen )
        {
            mlChildren = new List<Widget>();
        }

        //----------------------------------------------------------------------
        internal override void Update( float _fElapsedTime )
        {
            foreach( Widget widget in mlChildren )
            {
                widget.Update( _fElapsedTime );
            }

            base.Update( _fElapsedTime );
        }

        //----------------------------------------------------------------------
        internal override void DoLayout( Rectangle _rect )
        {
            foreach( Widget widget in mlChildren )
            {
                widget.DoLayout( _rect );
            }

            UpdateContentSize();

            HitBox = Resolution.InternalMode.Rectangle;
        }

        //----------------------------------------------------------------------
        public override Widget GetFirstFocusableDescendant( Direction _direction )
        {
            Widget firstChild = null;
            Widget focusableDescendant = null;

            foreach( Widget child in mlChildren )
            {
                switch( _direction )
                {
                    case Direction.Up:
                        if( ( firstChild == null || child.LayoutRect.Bottom > firstChild.LayoutRect.Bottom ) )
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
                        if( firstChild == null || child.LayoutRect.Top < firstChild.LayoutRect.Top )
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
                        if( firstChild == null || child.LayoutRect.Right > firstChild.LayoutRect.Right )
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
                        if( firstChild == null || child.LayoutRect.Left < firstChild.LayoutRect.Left )
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
                        if( child.LayoutRect.Bottom <= fixedChild.LayoutRect.Center.Y && ( nearestSibling == null || child.LayoutRect.Bottom > nearestSibling.LayoutRect.Bottom ) )
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
                        if( child.LayoutRect.Top >= fixedChild.LayoutRect.Center.Y && ( nearestSibling == null || child.LayoutRect.Top < nearestSibling.LayoutRect.Top ) )
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
                        if( child.LayoutRect.Right <= fixedChild.LayoutRect.Center.X && ( nearestSibling == null || child.LayoutRect.Right > nearestSibling.LayoutRect.Right ) )
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
                        if( child.LayoutRect.Left >= fixedChild.LayoutRect.Center.X && ( nearestSibling == null || child.LayoutRect.Left < nearestSibling.LayoutRect.Left ) )
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

        //----------------------------------------------------------------------
        public override Widget HitTest( Point _point )
        {
            if( HitBox.Contains( _point ) )
            {
                Widget hitWidget;

                for( int iChild = mlChildren.Count - 1; iChild >= 0; iChild-- )
                {
                    Widget child = mlChildren[iChild];

                    if( ( hitWidget = child.HitTest( _point ) ) != null )
                    {
                        return hitWidget;
                    }
                }
            }

            return null;
        }

        //----------------------------------------------------------------------
        internal override bool OnPadButton( Buttons _button, bool _bIsDown )
        {
            foreach( Widget child in mlChildren )
            {
                if( child.OnPadButton( _button, _bIsDown ) )
                {
                    return true;
                }
            }

            return false;
        }

        //----------------------------------------------------------------------
        internal override void Draw()
        {
            foreach( Widget child in mlChildren )
            {
                child.Draw();
            }
        }
    }
}
