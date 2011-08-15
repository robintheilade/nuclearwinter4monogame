using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace NuclearWinter.UI
{
    //--------------------------------------------------------------------------
    public class WidgetProxy: Widget
    {
        Widget mChild;
        public Widget       Child {
            get {
                return mChild;
            }
            set {
                if( mChild != null ) mChild.Parent = null;
                mChild = value;
                if( mChild != null ) mChild.Parent = this;
                UpdateContentSize();
            }
        }
        
        public override bool CanFocus { get { return false; } }

        public override Widget GetFirstFocusableDescendant( Direction _direction )
        {
            return ( Child != null && Child.CanFocus ) ? Child : null;
        }

        public WidgetProxy( Screen _screen, Widget _child )
        : base( _screen )
        {
            Child       = _child;
            if( Child != null )
            {
                Child.Parent = this;
            }
        }

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

        public override void DoLayout( Rectangle? _rect )
        {
            if( Child != null )
            {
                Child.DoLayout( _rect );
            }
        }
        
        public override void Draw()
        {
            if( Child != null )
            {
                Child.Draw();
            }
        }

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
    }
}
