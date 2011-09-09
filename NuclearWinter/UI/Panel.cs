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
     */
    public class Panel: Widget
    {
        public Texture2D        Texture;
        public int              CornerSize;

        //----------------------------------------------------------------------
        public Panel( Screen _screen, Texture2D _texture, int _iCornerSize )
        : base( _screen )
        {
            Texture     = _texture;
            CornerSize  = _iCornerSize;
        }

        //----------------------------------------------------------------------
        public override Widget GetFirstFocusableDescendant( Direction _direction )
        {
            return null;
        }

        //----------------------------------------------------------------------
        internal override void UpdateContentSize()
        {
        }

        //----------------------------------------------------------------------
        internal override void DoLayout( Rectangle _rect )
        {
            Position = _rect.Location;
            Size = new Point( _rect.Width, _rect.Height );

            HitBox = new Rectangle( Position.X, Position.Y, Size.X, Size.Y );
        }

        //----------------------------------------------------------------------
        internal override void Draw()
        {
            Screen.DrawBox( Texture, new Rectangle( Position.X, Position.Y, Size.X, Size.Y ), CornerSize, Color.White );
        }
    }
}
