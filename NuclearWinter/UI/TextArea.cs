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
                ScrollMax = Math.Max( 0, mLabel.ContentHeight - mLabel.Padding.Vertical - ( LayoutRect.Height - 20 ) );
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
            LayoutRect = _rect;
            HitBox = LayoutRect;

            mLabel.DoLayout( new Rectangle( LayoutRect.X + 10, LayoutRect.Y + 10 - miScrollOffset, LayoutRect.Width - 20, LayoutRect.Height - 20 ) );

            ScrollMax = Math.Max( 0, mLabel.ContentHeight - mLabel.Padding.Vertical - ( LayoutRect.Height - 20 ) );
            miScrollOffset = (int)MathHelper.Min( miScrollOffset, ScrollMax );

            if( ScrollMax > 0 )
            {
                miScrollbarHeight = (int)( ( LayoutRect.Height - 20 ) / ( (float)mLabel.ContentHeight / ( LayoutRect.Height - 20 ) ) );
                miScrollbarOffset = (int)( (float)miScrollOffset / ScrollMax * (float)( LayoutRect.Height - 20 - miScrollbarHeight ) );
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
            Screen.DrawBox( Screen.Style.ListFrame, LayoutRect, 30, Color.White );

            Screen.PushScissorRectangle( new Rectangle( LayoutRect.X + 10, LayoutRect.Y + 10 - miScrollOffset, LayoutRect.Width - 20, LayoutRect.Height - 20 ) );
            mLabel.Draw();
            Screen.PopScissorRectangle();

            if( ScrollMax > 0 )
            {
                Screen.DrawBox( Screen.Style.VerticalScrollbar, new Rectangle( LayoutRect.Right - 5 - Screen.Style.VerticalScrollbar.Width / 2, LayoutRect.Y + 10 + miScrollbarOffset, Screen.Style.VerticalScrollbar.Width, miScrollbarHeight ), Screen.Style.VerticalScrollbarCornerSize, Color.White );
            }
        }

    }
}
