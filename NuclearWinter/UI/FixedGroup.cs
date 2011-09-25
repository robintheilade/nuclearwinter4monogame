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
    public class FixedGroup: Widget
    {
        //----------------------------------------------------------------------
        List<FixedWidget>           mlChildren;

        public int Width {
            get { return ContentWidth; }
            set { ContentWidth = value; }
        }

        public int Height {
            get { return ContentHeight; }
            set { ContentHeight = value; }
        }

        //----------------------------------------------------------------------
        public FixedGroup( Screen _screen )
        : base( _screen )
        {
            mlChildren = new List<FixedWidget>();
        }

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

        //----------------------------------------------------------------------
        internal override void UpdateContentSize()
        {
        }

        //----------------------------------------------------------------------
        internal override void DoLayout( Rectangle _rect )
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
                    if( fixedChild.Child != null && ( hitWidget = fixedChild.Child.HitTest( _point ) ) != null )
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
            foreach( FixedWidget child in mlChildren )
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
            foreach( FixedWidget child in mlChildren )
            {
                child.Draw();
            }
        }
    }
}
