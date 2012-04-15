using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace NuclearWinter.UI
{
    //--------------------------------------------------------------------------
    /// <summary>
    /// A Label to display some text
    /// </summary>
    public class Label: Widget
    {
        //----------------------------------------------------------------------
        public string Text
        {
            get { return mstrText; }
            
            set
            {
                mstrText = value;
                mstrDisplayedText = value;
                UpdateContentSize();
            }
        }

        //----------------------------------------------------------------------
        public UIFont Font
        {
            get { return mFont; }
            
            set
            {
                mFont = value;
                UpdateContentSize();
            }
        }

        //----------------------------------------------------------------------
        public Anchor Anchor
        {
            get { return mAnchor; }
            set { mAnchor = value; UpdateContentSize(); }
        }

        //----------------------------------------------------------------------
        public bool WrapText
        {
            get { return mbWrapText; }
            set { mbWrapText = value; UpdateContentSize(); }
        }

        //----------------------------------------------------------------------
        public Color    Color;
        public Color    OutlineColor;
        public float    OutlineRadius;

        public bool     HasEllipsis { get { return mstrDisplayedText != mstrText; } }

        //----------------------------------------------------------------------
        string          mstrText;
        string          mstrDisplayedText;

        UIFont          mFont;
        Anchor          mAnchor;

        bool            mbWrapText;
        List<string>    mlstrWrappedText;

        Point           mpTextPosition;

        int             miEllipsizedTextWidth;

        //----------------------------------------------------------------------
        public Label( Screen _screen, string _strText, Anchor _anchor, Color _color )
        : base( _screen )
        {
            mstrText            = _strText;
            mstrDisplayedText   = mstrText;
            mFont               = _screen.Style.MediumFont;
            mPadding            = new Box( 10 );
            mAnchor             = _anchor;

            Color               = _color;
            OutlineRadius       = Screen.Style.BlurRadius;
            OutlineColor        = _color * 0.5f;

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
            ContentWidth = (int)Font.MeasureString( Text ).X + Padding.Horizontal;

            if( mbWrapText )
            {
                mlstrWrappedText = null;
            }
            else
            {
                ContentHeight = (int)( Font.LineSpacing * 0.9f ) + Padding.Vertical;
            }

            DoTextLayout();

            base.UpdateContentSize();
        }

        void DoTextLayout()
        {
            if( mbWrapText )
            {
                if( LayoutRect.Width > 0 )
                {
                    // Wrap text
                    mlstrWrappedText = Screen.Game.WrapText( Font, Text, LayoutRect.Width - Padding.Horizontal );
                    ContentWidth = LayoutRect.Width;
                    ContentHeight = (int)( Font.LineSpacing * mlstrWrappedText.Count ) + Padding.Vertical;
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

                miEllipsizedTextWidth = ContentWidth;
                int iOffset = Text.Length;
                while( miEllipsizedTextWidth > LayoutRect.Width )
                {
                    iOffset--;
                    mstrDisplayedText = Text.Substring( 0, iOffset ) + "…";
                    if( iOffset == 0 ) break;

                    miEllipsizedTextWidth = (int)Font.MeasureString( mstrDisplayedText ).X + Padding.Horizontal;
                }
            }
        }

        //----------------------------------------------------------------------
        internal override void DoLayout( Rectangle _rect )
        {
            Rectangle previousLayoutRect = LayoutRect;
            base.DoLayout( _rect );

            bool bTextLayoutNeeded = ( LayoutRect.Width != previousLayoutRect.Width || LayoutRect.Height != previousLayoutRect.Height );

            if( bTextLayoutNeeded )
            {
                DoTextLayout();
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
                        pCenter.X - ( ContentWidth > LayoutRect.Width ? miEllipsizedTextWidth : ContentWidth ) / 2 + Padding.Left,
                        iTop
                    );
                    break;
                case UI.Anchor.End:
                    mpTextPosition = new Point(
                        LayoutRect.Right + Padding.Right - ( ContentWidth > LayoutRect.Width ? miEllipsizedTextWidth : ContentWidth ),
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
