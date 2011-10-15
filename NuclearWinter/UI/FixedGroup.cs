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

        public bool                 AutoSize = false;

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
        internal override void UpdateContentSize()
        {
            if( AutoSize )
            {
                //ContentWidth = 0;
                ContentHeight = 0;

                foreach( FixedWidget fixedWidget in mlChildren )
                {
                    //ContentWidth    = Math.Max( ContentWidth, fixedWidget.LayoutRect.Right );
                    int iHeight = 0;
                    if( fixedWidget.ChildBox.Top.HasValue )
                    {
                        if( fixedWidget.ChildBox.Bottom.HasValue )
                        {
                            iHeight = fixedWidget.ChildBox.Top.Value + fixedWidget.Child.ContentHeight + fixedWidget.ChildBox.Bottom.Value;
                        }
                        else
                        {
                            iHeight = fixedWidget.ChildBox.Top.Value + fixedWidget.ChildBox.Height;
                        }
                    }

                    ContentHeight = Math.Max( ContentHeight, iHeight );
                }
            }

            base.UpdateContentSize();
        }

        //----------------------------------------------------------------------
        internal override void Update( float _fElapsedTime )
        {
            foreach( FixedWidget fixedWidget in mlChildren )
            {
                fixedWidget.Update( _fElapsedTime );
            }

            base.Update( _fElapsedTime );
        }

        //----------------------------------------------------------------------
        internal override void DoLayout( Rectangle _rect )
        {
            foreach( FixedWidget fixedWidget in mlChildren )
            {
                fixedWidget.DoLayout( _rect );
            }
            
            UpdateContentSize();

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
