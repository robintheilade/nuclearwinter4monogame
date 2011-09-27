using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NuclearWinter.UI
{
    public class TextArea: Widget
    {
        Label           mLabel;

        public UIFont Font
        {
            get { return mLabel.Font; }
            set { mLabel.Font = value; }
        }

        public string Text {
            get { return mLabel.Text; }

            set {
                mLabel.Text = value;
                ScrollMax = Math.Max( 0, mLabel.ContentHeight - mLabel.Padding.Vertical - ( Size.Y - 20 ) );
            }
        }

        int             miScrollOffset;
        public int      ScrollOffset
        {
            get { return miScrollOffset; }
            set { miScrollOffset = (int)MathHelper.Clamp( value, 0f, ScrollMax ); }
        }

        public int      ScrollMax { get; private set; }

        int                         miScrollbarHeight;
        int                         miScrollbarOffset;

        //----------------------------------------------------------------------
        public TextArea( Screen _screen )
        : base( _screen )
        {
            mLabel = new Label( _screen );
            mLabel.Anchor = Anchor.Start;
            mLabel.Color = Screen.Style.DefaultTextColor;
            mLabel.WrapText = true;
        }

        //----------------------------------------------------------------------
        internal override void UpdateContentSize()
        {
            ContentWidth    = Padding.Left + Padding.Right;
            ContentHeight   = Padding.Top + Padding.Bottom;

            base.UpdateContentSize();
        }

        //----------------------------------------------------------------------
        internal override void DoLayout( Rectangle _rect )
        {
            Position = _rect.Location;
            Size = new Point( _rect.Width, _rect.Height );
            HitBox = _rect;

            mLabel.DoLayout( new Rectangle( Position.X + 10, Position.Y + 10 - miScrollOffset, Size.X - 20, Size.Y - 20 ) );

            ScrollMax = Math.Max( 0, mLabel.ContentHeight - mLabel.Padding.Vertical - ( Size.Y - 20 ) );
            miScrollOffset = (int)MathHelper.Min( miScrollOffset, ScrollMax );

            if( ScrollMax > 0 )
            {
                miScrollbarHeight = (int)( ( Size.Y - 20 ) / ( (float)mLabel.ContentHeight / ( Size.Y - 20 ) ) );
                miScrollbarOffset = (int)( (float)miScrollOffset / ScrollMax * (float)( Size.Y - 20 - miScrollbarHeight ) );
            }
        }

        //----------------------------------------------------------------------
        internal override void OnMouseWheel( Point _hitPoint, int _iDelta )
        {
            ScrollOffset = (int)( miScrollOffset - ( _iDelta / 120 * 3 * ( mLabel.Font.LineSpacing ) ) );
        }

        //----------------------------------------------------------------------
        internal override void Draw()
        {
            Screen.DrawBox( Screen.Style.ListFrame, new Rectangle( Position.X, Position.Y, Size.X, Size.Y ), 30, Color.White );

            Screen.PushScissorRectangle( new Rectangle( Position.X + 10, Position.Y + 10, Size.X - 20, Size.Y - 20 ) );
            mLabel.Draw();
            Screen.PopScissorRectangle();

            if( ScrollMax > 0 )
            {
                Screen.DrawBox( Screen.Style.VerticalScrollbar, new Rectangle( Position.X + Size.X - 5 - Screen.Style.VerticalScrollbar.Width / 2, Position.Y + 10 + miScrollbarOffset, Screen.Style.VerticalScrollbar.Width, miScrollbarHeight ), Screen.Style.VerticalScrollbarCornerSize, Color.White );
            }
        }

    }
}
