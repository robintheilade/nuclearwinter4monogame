using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace NuclearWinter.UI
{
    /*
     * A widget that draws a box with the specified texture & corner size
     * Can also contain stuff
     */
    public class Panel: FixedGroup
    {
        public Texture2D        Texture;
        public int              CornerSize;

        //----------------------------------------------------------------------
        public Panel( Screen _screen, Texture2D _texture, int _iCornerSize )
        : base( _screen )
        {
            Texture     = _texture;
            CornerSize  = _iCornerSize;
            Padding     = new Box( CornerSize );
        }

        //----------------------------------------------------------------------
        internal override void DoLayout( Rectangle _rect )
        {
            Position = _rect.Location;
            Size = new Point( _rect.Width, _rect.Height );

            base.DoLayout( new Rectangle( _rect.X + Padding.Left, _rect.Y + Padding.Right, _rect.Width - Padding.Horizontal, _rect.Height - Padding.Vertical ) );
        }

        //----------------------------------------------------------------------
        internal override void Draw()
        {
            Screen.DrawBox( Texture, new Rectangle( Position.X, Position.Y, Size.X, Size.Y ), CornerSize, Color.White );

            base.Draw();
        }
    }
}
