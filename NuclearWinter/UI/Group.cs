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
        protected override void UpdateContentSize()
        {
        }

        //----------------------------------------------------------------------
        public override void DoLayout( Rectangle _rect )
        {
            foreach( Widget widget in mlChildren )
            {
                widget.DoLayout( _rect );
            }

            HitBox = Resolution.InternalMode.Rectangle;
        }

        //----------------------------------------------------------------------
        public override Widget GetFirstFocusableDescendant( Direction _direction )
        {
            foreach( Widget child in mlChildren )
            {
                Widget focusableWidget = child.GetFirstFocusableDescendant( _direction );
                if( focusableWidget != null )
                {
                    return focusableWidget;
                }
            }

            return null;
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

        public override bool OnPadButton( Buttons _button, bool _bIsDown )
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
        public override void Draw()
        {
            foreach( Widget child in mlChildren )
            {
                child.Draw();
            }
        }
    }
}
