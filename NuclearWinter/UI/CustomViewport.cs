using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace NuclearWinter.UI
{
    public interface ICustomViewportHandler
    {
        void OnMouseEnter( Point _hitPoint );
        void OnMouseMove( Point _hitPoint );
        void OnMouseOut( Point _hitPoint );

        void OnMouseDown( Point _hitPoint, int _iButton );
        void OnMouseUp( Point _hitPoint, int _iButton );

        void OnMouseWheel( Point _hitPoint, int _iDelta );

        void OnKeyPress( Keys _key );

        void OnBlur();

        void Update(float _fElapsedTime);
        void Draw();
        void DrawHovered();
    }

    public class CustomViewport: Widget
    {
        public ICustomViewportHandler       EventHandler;

        //----------------------------------------------------------------------
        public CustomViewport( Screen _screen )
        : base( _screen )
        {
        }

        //----------------------------------------------------------------------
        protected internal override void DoLayout( Rectangle _rect )
        {
            base.DoLayout( _rect );
            HitBox = LayoutRect;
        }

        Point TransformPoint( Point _point )
        {
            return new Point( _point.X - LayoutRect.X, _point.Y - LayoutRect.Y );
        }

        protected internal override void OnPadMove( Direction _direction ) { }
        protected internal override void OnKeyPress( Keys _key )                          { if( EventHandler != null ) EventHandler.OnKeyPress( _key );  }

        protected internal override void OnMouseEnter( Point _hitPoint )                  { if( EventHandler != null ) EventHandler.OnMouseEnter( TransformPoint( _hitPoint ) );  }
        protected internal override void OnMouseMove( Point _hitPoint )                   { if( EventHandler != null ) EventHandler.OnMouseMove( TransformPoint( _hitPoint ) );  }
        protected internal override void OnMouseOut( Point _hitPoint )                    { if( EventHandler != null ) EventHandler.OnMouseOut( TransformPoint( _hitPoint ) );  }

        protected internal override void OnMouseDown( Point _hitPoint, int _iButton )     { Screen.Focus( this ); if( EventHandler != null ) EventHandler.OnMouseDown( TransformPoint( _hitPoint ), _iButton );  }
        protected internal override void OnMouseUp( Point _hitPoint, int _iButton )       { if( EventHandler != null ) EventHandler.OnMouseUp( TransformPoint( _hitPoint ), _iButton );  }

        protected internal override void OnMouseWheel( Point _hitPoint, int _iDelta )     { if( EventHandler != null ) EventHandler.OnMouseWheel( TransformPoint( _hitPoint ), _iDelta ); }

        protected internal override void OnBlur()                                         { if( EventHandler != null ) EventHandler.OnBlur(); }

        protected internal override void Update( float _fElapsedTime )
        {
            if( EventHandler != null )
            {
                EventHandler.Update( _fElapsedTime );
            }
        }

        //----------------------------------------------------------------------
        protected internal override void Draw()
        {
            if( EventHandler != null )
            {
                Screen.SuspendBatch();

                Viewport previousViewport = Screen.Game.GraphicsDevice.Viewport;
                Viewport viewport = new Viewport( LayoutRect );
                Screen.Game.GraphicsDevice.Viewport = viewport;
                EventHandler.Draw();

                Screen.Game.GraphicsDevice.Viewport = previousViewport;

                Screen.ResumeBatch();
            }
        }

        protected internal override void DrawHovered()
        {
            if( EventHandler != null )
            {
                EventHandler.DrawHovered();
            }
        }
    }
}
