using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NuclearWinter.UI
{
    public class CustomRenderer: Widget
    {
        public Action           DrawHandler;

        //----------------------------------------------------------------------
        public CustomRenderer( Screen _screen )
        : base( _screen )
        {
        }

        //----------------------------------------------------------------------
        internal override void DoLayout( Rectangle _rect )
        {
            Position = _rect.Location;
            Size = new Point( _rect.Width, _rect.Height );
        }

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
