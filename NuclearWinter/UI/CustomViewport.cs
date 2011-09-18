using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NuclearWinter.UI
{
    public interface CustomViewportEventHandler
    {
        void OnMouseEnter( Point _hitPoint );
        void OnMouseMove( Point _hitPoint );
        void OnMouseOut( Point _hitPoint );

        void OnMouseDown( Point _hitPoint );
        void OnMouseUp( Point _hitPoint );
    }

    public class CustomViewport: Widget
    {
        public Action                       DrawHandler;
        public CustomViewportEventHandler   EventHandler;

        //----------------------------------------------------------------------
        public CustomViewport( Screen _screen )
        : base( _screen )
        {
        }

        //----------------------------------------------------------------------
        internal override void DoLayout( Rectangle _rect )
        {
            Position = _rect.Location;
            Size = new Point( _rect.Width, _rect.Height );

            HitBox = new Rectangle( Position.X, Position.Y, Size.X, Size.Y );
        }

        internal override void OnMouseEnter(Point _hitPoint) { if( EventHandler != null ) EventHandler.OnMouseEnter( _hitPoint );  }
        internal override void OnMouseMove(Point _hitPoint) { if( EventHandler != null ) EventHandler.OnMouseMove( _hitPoint );  }
        internal override void OnMouseOut(Point _hitPoint) { if( EventHandler != null ) EventHandler.OnMouseOut( _hitPoint );  }

        internal override void OnMouseDown(Point _hitPoint) { if( EventHandler != null ) EventHandler.OnMouseDown( _hitPoint );  }
        internal override void OnMouseUp(Point _hitPoint) { if( EventHandler != null ) EventHandler.OnMouseUp( _hitPoint );  }

        //----------------------------------------------------------------------
        internal override void Draw()
        {
            if( DrawHandler != null )
            {
                Screen.SuspendBatch();

                Viewport previousViewport = Screen.Game.GraphicsDevice.Viewport;
                Viewport viewport = new Viewport( Position.X, Position.Y, Size.X, Size.Y );
                Screen.Game.GraphicsDevice.Viewport = viewport;
                DrawHandler();

                Screen.Game.GraphicsDevice.Viewport = previousViewport;

                Screen.ResumeBatch();
            }

            
        }
    }
}
