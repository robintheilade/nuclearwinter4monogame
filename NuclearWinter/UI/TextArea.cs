using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace NuclearWinter.UI
{
    public class TextArea: Widget
    {
        Label           mLabel;

        public string Text {
            get { return mLabel.Text; }

            set { mLabel.Text = value; }
        }

        //----------------------------------------------------------------------
        public TextArea( Screen _screen )
        : base( _screen )
        {
            mLabel = new Label( _screen );
            mLabel.Anchor = Anchor.Start;
            mLabel.Color = Screen.Style.ButtonTextColor;
            mLabel.WrapText = true;
        }

        //----------------------------------------------------------------------
        public override bool CanFocus { get { return true; } }

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

            mLabel.DoLayout( new Rectangle( Position.X + 10, Position.Y + 10, Size.X - 20, Size.Y - 20 ) );
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            Screen.DrawBox( Screen.Style.GridFrame, new Rectangle( Position.X, Position.Y, Size.X, Size.Y ), 30, Color.White );

            mLabel.Draw();
        }

    }
}
