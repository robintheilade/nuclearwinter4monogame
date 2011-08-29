using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace NuclearWinter.UI
{
    public class TreeView: Widget
    {
        //----------------------------------------------------------------------
        public TreeView( Screen _screen )
        : base( _screen )
        {

        }

        //----------------------------------------------------------------------
        protected override void UpdateContentSize()
        {
            ContentWidth    = Padding.Left + Padding.Right;
            ContentHeight   = Padding.Top + Padding.Bottom;
        }

        //----------------------------------------------------------------------
        public override void DoLayout( Rectangle _rect )
        {
            Position = _rect.Location;
            Size = new Point( _rect.Width, _rect.Height );
            HitBox = _rect;
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            Screen.DrawBox( Screen.Style.GridFrame, new Rectangle( Position.X, Position.Y, Size.X, Size.Y ), 30, Color.White );
        }

    }
}
