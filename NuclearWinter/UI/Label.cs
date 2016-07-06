using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

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
        public Action<Label>    ClickHandler;

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

        public bool     Underline;

        public bool     HasEllipsis { get { return mstrDisplayedText != mstrText; } }

        //----------------------------------------------------------------------
        string          mstrText;
        string          mstrDisplayedText;

        UIFont          mFont;
        Anchor          mAnchor;

        bool            mbWrapText;
        List<Tuple<string,bool>> mlWrappedText;

        Point           mpTextPosition;

        int             miEllipsizedTextWidth;

        //----------------------------------------------------------------------
        public Label( Screen screen, string text, Anchor anchor, Color color )
        : base( screen )
        {
            mstrText            = text;
            mstrDisplayedText   = mstrText;
            mFont               = screen.Style.MediumFont;
            mPadding            = Screen.Style.LabelPadding;
            mAnchor             = anchor;

            Color               = color;
            OutlineRadius       = 0;
            OutlineColor        = color * 0.5f;

            UpdateContentSize();
        }

        public Label( Screen screen, string text, Color color )
        : this( screen, text, Anchor.Center, color )
        {

        }

        public Label( Screen screen, string text = "", Anchor anchor = Anchor.Center )
        : this( screen, text, anchor, screen.Style.DefaultTextColor )
        {

        }

        //----------------------------------------------------------------------
        public override Widget GetFirstFocusableDescendant( Direction direction )
        {
            return ClickHandler != null ? this : null;
        }

        //----------------------------------------------------------------------
        protected internal override void UpdateContentSize()
        {
            ContentWidth = (int)Font.MeasureString( Text ).X + Padding.Horizontal;

            if( mbWrapText )
            {
                mlWrappedText = null;
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
                    mlWrappedText = Screen.Game.WrapText( Font, Text, LayoutRect.Width - Padding.Horizontal );
                    ContentWidth = LayoutRect.Width;
                    ContentHeight = (int)( Font.LineSpacing * mlWrappedText.Count ) + Padding.Vertical;
                }
                else
                if( mlWrappedText == null )
                {
                    mlWrappedText = new List<Tuple<string,bool>>();
                    mlWrappedText.Add( new Tuple<string,bool>( mstrText, true ) );
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
        public override Widget HitTest( Point point )
        {
            return ClickHandler != null ? base.HitTest( point ) : null;
        }

        public override void OnMouseEnter( Point hitPoint )
        {
            if( ClickHandler != null )
            {
                Screen.Game.SetCursor( MouseCursor.Hand );
            }
        }

        public override void OnMouseOut( Point hitPoint )
        {
            if( ClickHandler != null )
            {
                Screen.Game.SetCursor( MouseCursor.Default );
            }
        }

        protected internal override bool OnMouseDown( Point hitPoint, int button )
        {
            return ClickHandler != null && button == Screen.Game.InputMgr.PrimaryMouseButton;
        }

        protected internal override void OnMouseUp(Point hitPoint, int button)
        {
            if( button != Screen.Game.InputMgr.PrimaryMouseButton ) return;

            OnActivateUp();
        }

        //----------------------------------------------------------------------
        protected internal override void OnActivateUp()
        {
            if( ClickHandler != null )
            {
                ClickHandler( this );
            }
        }

        //----------------------------------------------------------------------
        public override void DoLayout( Rectangle rectangle )
        {
            Rectangle previousLayoutRect = LayoutRect;
            base.DoLayout( rectangle );

            bool bTextLayoutNeeded = ( LayoutRect.Width != previousLayoutRect.Width || LayoutRect.Height != previousLayoutRect.Height );

            if( bTextLayoutNeeded )
            {
                DoTextLayout();
            }

            Point pCenter = LayoutRect.Center;

            int iTop = WrapText ? ( LayoutRect.Y ) : ( pCenter.Y - ContentHeight / 2 );
            int iLeft;
            int iActualWidth = ( ContentWidth > LayoutRect.Width ? miEllipsizedTextWidth : ContentWidth );

            switch( Anchor )
            {
                case UI.Anchor.Start:
                    iLeft = LayoutRect.X;
                    mpTextPosition = new Point( iLeft + Padding.Left, iTop + Padding.Top );
                    break;
                case UI.Anchor.Center:
                    iLeft = pCenter.X - iActualWidth / 2;
                    mpTextPosition = new Point( iLeft + Padding.Left, iTop + Padding.Top );
                    break;
                case UI.Anchor.End:
                    iLeft = LayoutRect.Right - iActualWidth;
                    mpTextPosition = new Point( iLeft + Padding.Left, iTop + Padding.Top );
                    break;
                default:
                    throw new NotSupportedException();
            }

            HitBox = new Rectangle( iLeft, iTop, iActualWidth, ContentHeight );
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            DrawWithOffset( Point.Zero );
        }

        //----------------------------------------------------------------------
        public void DrawWithOffset( Point offset )
        {
            if( WrapText )
            {
                for( int i = 0; i < mlWrappedText.Count; i++ )
                {
                    float fX = mpTextPosition.X + offset.X;
                    float fTextWidth = mFont.MeasureString( mlWrappedText[i].Item1 ).X;
                    if( Anchor == UI.Anchor.Center )
                    {
                        fX += ContentWidth / 2 - Padding.Left - fTextWidth / 2f;
                    }

                    Screen.Game.DrawBlurredText( OutlineRadius, mFont, mlWrappedText[i].Item1, new Vector2( (int)fX, mpTextPosition.Y + (int)( Font.LineSpacing * i ) + Font.YOffset + offset.Y ), Color, OutlineColor );

                    if( Underline )
                    {
                        var vBottomLeft = new Vector2( fX, mpTextPosition.Y + (int)( Font.LineSpacing * (i+1) ) + Font.YOffset + offset.Y );
                        Screen.Game.DrawLine( vBottomLeft, vBottomLeft + new Vector2( fTextWidth, 0 ), Color );
                    }
                }
            }
            else
            {
                Screen.Game.DrawBlurredText( OutlineRadius, mFont, mstrDisplayedText, new Vector2( mpTextPosition.X + offset.X, mpTextPosition.Y + Font.YOffset + offset.Y ), Color, OutlineColor );

                if( Underline )
                {
                    var vBottomLeft = new Vector2( mpTextPosition.X + offset.X, mpTextPosition.Y + Font.YOffset + offset.Y + Font.LineSpacing );
                    Screen.Game.DrawLine( vBottomLeft, vBottomLeft + new Vector2( miEllipsizedTextWidth - Padding.Horizontal, 0 ), Color );
                }
            }
        }

        protected internal override void DrawFocused()
        {
            if( ClickHandler != null )
            {
                Screen.DrawBox( Screen.Style.ButtonFocusOverlay, LayoutRect, Screen.Style.ButtonCornerSize, Color.White );
            }
        }
    }
}
