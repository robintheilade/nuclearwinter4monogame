using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace NuclearWinter.UI
{
    /*
     * A Label to display some text
     */
    public class Label: Widget
    {
        SpriteFont mFont;
        public SpriteFont       Font
        {
            get { return mFont; }
            
            set
            {
                mFont = value;
                UpdateContentSize();
            }
        }

        List<string>    mlstrWrappedText;
        string          mstrText;
        Point           mpTextPosition;

        public string           Text
        {
            get
            {
                return mstrText;
            }
            
            set
            {
                mstrText = value;
                UpdateContentSize();
            }
        }

        Anchor  mAnchor;
        public Anchor   Anchor
        {
            get { return mAnchor; }
            set { mAnchor = value; DoLayout( null ); }
        }

        bool    mbWrapText;
        public bool WrapText
        {
            get { return mbWrapText; }
            set { mbWrapText = value; UpdateContentSize(); DoLayout( null ); }
        }

        public override bool CanFocus { get { return false; } }


        public Color            Color;

        //----------------------------------------------------------------------
        public Label( Screen _screen, string _strText, Anchor _anchor, Color _color )
        : base( _screen )
        {
            mstrText    = _strText;
            mFont       = _screen.Style.MediumFont;
            mPadding    = new Box( 10 );
            mAnchor     = _anchor;

            Color       = _color;

            UpdateContentSize();
        }

        public Label( Screen _screen, string _strText, Color _color )
        : this( _screen, _strText, Anchor.Center, _color )
        {

        }

        public Label( Screen _screen, string _strText, Anchor _anchor )
        : this( _screen, _strText, _anchor, Color.White )
        {

        }

        public Label( Screen _screen, string _strText )
        : this( _screen, _strText, Anchor.Center, Color.White )
        {
        }

        public Label( Screen _screen )
        : this( _screen, "", Anchor.Center, Color.White )
        {
        }

        //----------------------------------------------------------------------
        protected override void UpdateContentSize()
        {
            ContentWidth = (int)Font.MeasureString( Text ).X + Padding.Left + Padding.Right;
            ContentHeight = (int)( Font.LineSpacing * 0.9f ) + Padding.Top + Padding.Bottom;

            if( mbWrapText )
            {
                mlstrWrappedText = null;
                DoWrapText();
            }
        }

        void DoWrapText()
        {
            if( ContentWidth > Size.X && Size.X > 0 )
            {
                // Wrap text
                mlstrWrappedText = Screen.Game.WrapText( Font, Text, Size.X - Padding.Horizontal );
                ContentWidth = Size.X;
                ContentHeight = (int)( Font.LineSpacing * 0.9f * mlstrWrappedText.Count ) + Padding.Top + Padding.Bottom;
            }
            else
            if( mlstrWrappedText == null )
            {
                mlstrWrappedText = new List<string>();
                mlstrWrappedText.Add( mstrText );
            }
        }

        //----------------------------------------------------------------------
        public override void DoLayout( Rectangle? _rect )
        {
            if( _rect.HasValue )
            {
                Position = _rect.Value.Location;
                Size = new Point( _rect.Value.Width, _rect.Value.Height );

                DoWrapText();
            }

            Point pCenter = new Point( Position.X + Size.X / 2, Position.Y + Size.Y / 2 );

            int iTop = WrapText ? ( Position.Y + Padding.Top ) : ( pCenter.Y - ContentHeight / 2 + Padding.Top );

            switch( Anchor )
            {
                case UI.Anchor.Start:
                    mpTextPosition = new Point(
                        Position.X + Padding.Left,
                        iTop
                    );
                    break;
                case UI.Anchor.Center:
                    mpTextPosition = new Point(
                        pCenter.X - ContentWidth / 2 + Padding.Left,
                        iTop
                    );
                    break;
                case UI.Anchor.End:
                    mpTextPosition = new Point(
                        Position.X + Size.X - Padding.Right - ContentWidth,
                        iTop
                    );
                    break;
            }
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            if( WrapText )
            {
                for( int i = 0; i < mlstrWrappedText.Count; i++ )
                {
                    Screen.Game.DrawBlurredText( mFont, mlstrWrappedText[i], new Vector2( mpTextPosition.X, mpTextPosition.Y + (int)( Font.LineSpacing * 0.9f * i ) ), Color );
                }
            }
            else
            {
                Screen.Game.DrawBlurredText( mFont, Text, new Vector2( mpTextPosition.X, mpTextPosition.Y ), Color );
            }
        }
    }
}
