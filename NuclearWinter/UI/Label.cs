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
        UIFont mFont;
        public UIFont       Font
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
        string          mstrDisplayedText;
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
                mstrDisplayedText = value;
                UpdateContentSize();
            }
        }

        Anchor  mAnchor;
        public Anchor   Anchor
        {
            get { return mAnchor; }
            set { mAnchor = value; UpdateContentSize(); }
        }

        bool    mbWrapText;
        public bool WrapText
        {
            get { return mbWrapText; }
            set { mbWrapText = value; UpdateContentSize(); }
        }

        public Color            Color;
        public Color            OutlineColor;
        public float            OutlineRadius;

        //----------------------------------------------------------------------
        public Label( Screen _screen, string _strText, Anchor _anchor, Color _color )
        : base( _screen )
        {
            mstrText    = _strText;
            mstrDisplayedText = mstrText;
            mFont       = _screen.Style.MediumFont;
            mPadding    = new Box( 10 );
            mAnchor     = _anchor;

            Color           = _color;
            OutlineRadius   = Screen.Style.BlurRadius;
            OutlineColor    = _color * 0.5f;

            UpdateContentSize();
        }

        public Label( Screen _screen, string _strText, Color _color )
        : this( _screen, _strText, Anchor.Center, _color )
        {

        }

        public Label( Screen _screen, string _strText = "", Anchor _anchor = Anchor.Center )
        : this( _screen, _strText, _anchor, _screen.Style.DefaultTextColor )
        {

        }

        //----------------------------------------------------------------------
        public override Widget GetFirstFocusableDescendant( Direction _direction )
        {
            return null;
        }

        //----------------------------------------------------------------------
        internal override void UpdateContentSize()
        {
            ContentWidth = (int)Font.MeasureString( Text ).X + Padding.Left + Padding.Right;
            ContentHeight = (int)( Font.LineSpacing * 0.9f ) + Padding.Top + Padding.Bottom;

            if( mbWrapText )
            {
                ContentWidth = (int)Font.MeasureString( Text ).X + Padding.Left + Padding.Right;
                mlstrWrappedText = null;
                DoWrapText();
            }

            base.UpdateContentSize();
        }

        void DoWrapText()
        {
            if( mbWrapText )
            {
                if( LayoutRect.Width > 0 )
                {
                    // Wrap text
                    mlstrWrappedText = Screen.Game.WrapText( Font, Text, LayoutRect.Width - Padding.Horizontal );
                    ContentWidth = LayoutRect.Width;
                    ContentHeight = (int)( Font.LineSpacing * mlstrWrappedText.Count ) + Padding.Top + Padding.Bottom;
                }
                else
                if( mlstrWrappedText == null )
                {
                    mlstrWrappedText = new List<string>();
                    mlstrWrappedText.Add( mstrText );
                }
            }
            else
            if( Text != "" )
            {
                // Ellipsize
                mstrDisplayedText = Text;

                int iWidth = ContentWidth;
                int iOffset = Text.Length;
                while( iWidth > LayoutRect.Width )
                {
                    iOffset--;
                    mstrDisplayedText = Text.Substring( 0, iOffset ) + "...";
                    if( iOffset == 0 ) break;

                    iWidth = (int)Font.MeasureString( mstrDisplayedText ).X + Padding.Left + Padding.Right;
                }
            }
        }

        //----------------------------------------------------------------------
        internal override void DoLayout( Rectangle _rect )
        {
            if( LayoutRect == _rect ) return;

            bool bWrapTextNeeded = ( LayoutRect.Width != _rect.Width || LayoutRect.Height != _rect.Height );
            LayoutRect = _rect;

            if( bWrapTextNeeded )
            {
                DoWrapText();
            }

            Point pCenter = LayoutRect.Center;

            int iTop = WrapText ? ( LayoutRect.Y + Padding.Top ) : ( pCenter.Y - ContentHeight / 2 + Padding.Top );

            switch( Anchor )
            {
                case UI.Anchor.Start:
                    mpTextPosition = new Point(
                        LayoutRect.X + Padding.Left,
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
                        LayoutRect.Right - Padding.Right     - ContentWidth,
                        iTop
                    );
                    break;
            }
        }

        //----------------------------------------------------------------------
        internal override void Draw()
        {
            if( WrapText )
            {
                for( int i = 0; i < mlstrWrappedText.Count; i++ )
                {
                    float fX = mpTextPosition.X;
                    if( Anchor == UI.Anchor.Center )
                    {
                        fX += ContentWidth / 2 - Padding.Left - mFont.MeasureString( mlstrWrappedText[i] ).X / 2f;
                    }

                    Screen.Game.DrawBlurredText( OutlineRadius, mFont, mlstrWrappedText[i], new Vector2( (int)fX, mpTextPosition.Y + (int)( Font.LineSpacing * i ) + Font.YOffset ), Color, OutlineColor );
                }
            }
            else
            {
                Screen.Game.DrawBlurredText( OutlineRadius, mFont, mstrDisplayedText, new Vector2( mpTextPosition.X, mpTextPosition.Y + Font.YOffset ), Color, OutlineColor );
            }
        }
    }
}
