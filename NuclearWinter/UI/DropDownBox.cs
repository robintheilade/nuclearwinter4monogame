using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NuclearWinter.Animation;
using NuclearWinter.Collections;

namespace NuclearWinter.UI
{

    public class DropDownItem
    {
        //----------------------------------------------------------------------
        public string               Text {
            get { return mLabel.Text; }
            set {
                mLabel.Text = value;

                if( DropDownBox != null && DropDownBox.SelectedItem == this )
                {
                    DropDownBox.UpdateLabelText();
                }
            }
        }

        internal DropDownBox        DropDownBox;

        Label                       mLabel;
        public object               Tag;

        //----------------------------------------------------------------------
        public DropDownItem( Screen _screen, string _strText, object _tag=null )
        {
            mLabel = new Label( _screen, _strText, Anchor.Start );
            Tag     = _tag;
        }

        internal void DoLayout( Rectangle _rect )
        {
            mLabel.DoLayout( _rect );
        }

        internal void Draw()
        {
            mLabel.Draw();
        }
    }

    public class DropDownBox: Widget
    {
        //----------------------------------------------------------------------
        int miSelectedItemIndex;
        public int                      SelectedItemIndex {
            get { return miSelectedItemIndex; }
            set
            {
                miSelectedItemIndex = value;
                if( Items.Count > 0 )
                {
                    UpdateLabelText();
                }
            }
        }

        internal void UpdateLabelText()
        {
            mCurrentItemLabel.Text = Items[miSelectedItemIndex].Text;
        }

        public DropDownItem             SelectedItem            { get { return SelectedItemIndex != -1 ? Items[ SelectedItemIndex ] : null; } }
        public bool                     IsOpen                  { get; private set; }
        public Action<DropDownBox>      ChangeHandler;

        //----------------------------------------------------------------------
        public Texture2D                ButtonFrame             { get; set; }
        public Texture2D                ButtonFrameDown         { get; set; }
        public Texture2D                ButtonFrameHover        { get; set; }
        public Texture2D                ButtonFramePressed      { get; set; }

        //----------------------------------------------------------------------
        public ObservableList<DropDownItem>
                                        Items           { get; private set; }

        bool                            mbIsHovered;
        int                             miHoveredItemIndex;
        Point                           mHoverPoint;

        AnimatedValue                   mPressedAnim;
        bool                            mbIsPressed;

        Rectangle                       mDropDownHitBox;
        const int                       siMaxLineDisplayed = 5;

        int                             miScrollItemOffset;
        public Scrollbar                mScrollbar;

        public Box                      TextPadding;

        Label                           mCurrentItemLabel;

        //----------------------------------------------------------------------
        int ScrollItemOffset {
            get { return miScrollItemOffset; }
            set {
                miScrollItemOffset = value;
                mScrollbar.Offset = miScrollItemOffset * ( Screen.Style.MediumFont.LineSpacing + Padding.Vertical );
            }
        }

        //----------------------------------------------------------------------
        public DropDownBox( Screen _screen, List<DropDownItem> _lItems, int _iInitialValueIndex )
        : base( _screen )
        {
            mCurrentItemLabel = new Label( Screen, _anchor: Anchor.Start );

            Items = new ObservableList<DropDownItem>( _lItems );

            Items.ListChanged += delegate( object _source, ObservableList<DropDownItem>.ListChangedEventArgs _args )
            {
                if( _args.Added )
                {
                    _args.Item.DropDownBox = this;
                }

                if( SelectedItemIndex == -1 )
                {
                    if( _args.Added )
                    {
                        SelectedItemIndex = _args.Index;
                    }
                }
                else
                if( _args.Index <= SelectedItemIndex )
                {
                    SelectedItemIndex = Math.Min( Items.Count - 1, Math.Max( 0, SelectedItemIndex + ( _args.Added ? 1 : -1 ) ) );
                }
            };
            
            Items.ListCleared += delegate( object _source, EventArgs _args )
            {
                SelectedItemIndex = -1;
            };

            SelectedItemIndex = _iInitialValueIndex;
            mScrollbar = new Scrollbar( this );
            ScrollItemOffset = Math.Max( 0, Math.Min( SelectedItemIndex, Items.Count - siMaxLineDisplayed ) );
            mScrollbar.LerpOffset = mScrollbar.Offset;

            Padding = new Box( 10 );
            TextPadding = new Box( 5 );

            mPressedAnim    = new SmoothValue( 1f, 0f, 0.2f );
            mPressedAnim.SetTime( mPressedAnim.Duration );

            ButtonFrame         = Screen.Style.ButtonFrame;
            ButtonFrameDown     = Screen.Style.ButtonFrameDown;
            ButtonFrameHover    = Screen.Style.ButtonHover;
            ButtonFramePressed  = Screen.Style.ButtonPress;

            UpdateContentSize();
        }

        //----------------------------------------------------------------------
        internal override void UpdateContentSize()
        {
            UIFont uiFont = Screen.Style.MediumFont;

            int iMaxWidth = 0;
            foreach( DropDownItem _item in Items )
            {
                iMaxWidth = Math.Max( iMaxWidth, (int)uiFont.MeasureString( _item.Text ).X );
            }

            ContentWidth    = iMaxWidth + Padding.Horizontal + TextPadding.Horizontal + Screen.Style.DropDownArrow.Width;
            ContentHeight   = (int)( uiFont.LineSpacing * 0.9f ) + Padding.Vertical + TextPadding.Vertical;

            base.UpdateContentSize();
        }

        //----------------------------------------------------------------------
        internal override void DoLayout( Rectangle _rect )
        {
            base.DoLayout( _rect );
            HitBox = LayoutRect;

            mDropDownHitBox = new Rectangle(
                HitBox.Left, HitBox.Bottom,
                HitBox.Width, Math.Min( siMaxLineDisplayed, Items.Count ) * ( Screen.Style.MediumFont.LineSpacing + TextPadding.Vertical ) + Padding.Vertical );
            
            mScrollbar.DoLayout( mDropDownHitBox, Items.Count * ( Screen.Style.MediumFont.LineSpacing + TextPadding.Vertical ) );

            mCurrentItemLabel.DoLayout( new Rectangle( LayoutRect.X + TextPadding.Left, LayoutRect.Top + TextPadding.Top, LayoutRect.Width - TextPadding.Horizontal - Screen.Style.DropDownArrow.Width, LayoutRect.Height - TextPadding.Vertical ) );

            if( IsOpen )
            {
                int iLinesDisplayed = Math.Min( siMaxLineDisplayed, Items.Count );

                int iMaxIndex = Math.Min( Items.Count - 1, ScrollItemOffset + iLinesDisplayed - 1 );
                for( int iIndex = ScrollItemOffset; iIndex <= iMaxIndex; iIndex++ )
                {
                    Items[iIndex].DoLayout( new Rectangle( LayoutRect.X + TextPadding.Left, LayoutRect.Bottom + ( Screen.Style.MediumFont.LineSpacing + TextPadding.Vertical ) * ( iIndex - ScrollItemOffset ) + TextPadding.Top, LayoutRect.Width - TextPadding.Horizontal, Screen.Style.MediumFont.LineSpacing + TextPadding.Vertical + 10 ) );
                }
            }
        }

        //----------------------------------------------------------------------
        internal override void Update( float _fElapsedTime )
        {
            if( ! mPressedAnim.IsOver )
            {
                mPressedAnim.Update( _fElapsedTime );
            }

            mScrollbar.Update( _fElapsedTime );
        }

        //----------------------------------------------------------------------
        internal override void OnMouseEnter( Point _hitPoint )
        {
            mbIsHovered = true;
        }

        internal override void OnMouseOut( Point _hitPoint )
        {
            mbIsHovered = false;
        }

        //----------------------------------------------------------------------
        internal override void OnMouseDown( Point _hitPoint, int _iButton )
        {
            if( _iButton != Screen.Game.InputMgr.PrimaryMouseButton ) return;

            Screen.Focus( this );
            
            if( IsOpen && mDropDownHitBox.Contains( _hitPoint ) )
            {
            }
            else
            {
                miHoveredItemIndex = SelectedItemIndex;

                if( miHoveredItemIndex < ScrollItemOffset )
                {
                    ScrollItemOffset = miHoveredItemIndex;
                }
                else
                if( miHoveredItemIndex >= ScrollItemOffset + siMaxLineDisplayed )
                {
                    ScrollItemOffset = Math.Min( miHoveredItemIndex - siMaxLineDisplayed + 1, Items.Count - siMaxLineDisplayed );
                }

                mScrollbar.LerpOffset = mScrollbar.Offset;

                IsOpen = ! IsOpen;
                mPressedAnim.SetTime( 0f );
            }
        }

        internal override void OnMouseUp( Point _hitPoint, int _iButton )
        {
            if( _iButton != Screen.Game.InputMgr.PrimaryMouseButton ) return;

            if( IsOpen && mDropDownHitBox.Contains( _hitPoint ) )
            {
                mHoverPoint = _hitPoint;
                UpdateHoveredItem();
                
                mPressedAnim.SetTime( 1f );
                IsOpen = false;
                mbIsPressed = false;

                if( miHoveredItemIndex != -1 )
                {
                    SelectedItemIndex = miHoveredItemIndex;
                    if( ChangeHandler != null ) ChangeHandler( this );
                }
            }
            else
            if( HitTest( _hitPoint ) == this )
            {
                OnClick();
            }
            else
            {
                mPressedAnim.SetTime( 1f );
                IsOpen = false;
                mbIsPressed = false;
            }
        }

        internal override void OnMouseMove( Point _hitPoint )
        {
            if( IsOpen && mDropDownHitBox.Contains( _hitPoint ) )
            {
                mHoverPoint = _hitPoint;
                UpdateHoveredItem();
            }
            else
            {
                base.OnMouseMove( _hitPoint );
            }
        }

        internal override void OnMouseWheel( Point _hitPoint, int _iDelta )
        {
            int iNewScrollOffset = (int)MathHelper.Clamp( ScrollItemOffset - _iDelta / 120 * 3, 0, Math.Max( 0, Items.Count - siMaxLineDisplayed ) );
            miHoveredItemIndex += iNewScrollOffset - ScrollItemOffset;
            ScrollItemOffset = iNewScrollOffset;
        }

        void UpdateHoveredItem()
        {
            miHoveredItemIndex = (int)( ( mHoverPoint.Y - ( LayoutRect.Bottom + Padding.Top ) ) / ( Screen.Style.MediumFont.LineSpacing + TextPadding.Vertical ) ) + ScrollItemOffset;

            if( miHoveredItemIndex >= Items.Count )
            {
                miHoveredItemIndex = -1;
            }
        }

        //----------------------------------------------------------------------
        void OnClick()
        {
            mPressedAnim.SetTime( 0f );
        }

        //----------------------------------------------------------------------
        internal override void OnActivateDown()
        {
            if( IsOpen )
            {
            }
            else
            {
                miHoveredItemIndex = SelectedItemIndex;

                if( miHoveredItemIndex < ScrollItemOffset )
                {
                    ScrollItemOffset = miHoveredItemIndex;
                }
                else
                if( miHoveredItemIndex >= ScrollItemOffset + siMaxLineDisplayed )
                {
                    ScrollItemOffset = Math.Min( miHoveredItemIndex - siMaxLineDisplayed + 1, Items.Count - siMaxLineDisplayed );
                }

                mbIsPressed = true;
                mPressedAnim.SetTime( 0f );
            }
        }

        internal override void OnActivateUp()
        {
            if( IsOpen )
            {
                SelectedItemIndex = miHoveredItemIndex;
                if( ChangeHandler != null ) ChangeHandler( this );

                mPressedAnim.SetTime( 1f );
                IsOpen = false;
                mbIsPressed = false;
            }
            else
            {
                IsOpen = true;
            }
        }

        internal override bool OnCancel( bool _bPressed )
        {
            if( IsOpen )
            {
                if( ! _bPressed ) OnBlur();
                return true;
            }
            else
            {
                return false;
            }
        }

        //----------------------------------------------------------------------
        internal override void OnBlur()
        {
            mPressedAnim.SetTime( 1f );
            IsOpen = false;
            mbIsPressed = false;
        }

        //----------------------------------------------------------------------
        internal override void OnPadMove( Direction _direction )
        {
            if( ! IsOpen )
            {
                base.OnPadMove( _direction );
                return;
            }

            if( _direction == Direction.Up )
            {
                miHoveredItemIndex = Math.Max( 0, miHoveredItemIndex - 1 );

                if( miHoveredItemIndex < ScrollItemOffset )
                {
                    ScrollItemOffset = miHoveredItemIndex;
                }
            }
            else
            if( _direction == Direction.Down )
            {
                miHoveredItemIndex = Math.Min( Items.Count - 1, miHoveredItemIndex + 1 );

                if( miHoveredItemIndex >= ScrollItemOffset + siMaxLineDisplayed )
                {
                    ScrollItemOffset = Math.Min( miHoveredItemIndex - siMaxLineDisplayed + 1, Items.Count - siMaxLineDisplayed );
                }
            }
        }

        //----------------------------------------------------------------------
        internal override void Draw()
        {
            Screen.DrawBox( (!IsOpen && !mbIsPressed) ? ButtonFrame  : ButtonFrameDown, LayoutRect, Screen.Style.ButtonCornerSize, Color.White );

            if( mbIsHovered && ! IsOpen && mPressedAnim.IsOver )
            {
                if( Screen.IsActive )
                {
                    Screen.DrawBox( ButtonFrameHover,      LayoutRect, Screen.Style.ButtonCornerSize, Color.White );
                }
            }
            else
            {
                Screen.DrawBox( ButtonFramePressed,    LayoutRect, Screen.Style.ButtonCornerSize, Color.White * mPressedAnim.CurrentValue );
            }

            if( Screen.IsActive && HasFocus && ! IsOpen )
            {
                Screen.DrawBox( Screen.Style.ButtonFocus, LayoutRect, Screen.Style.ButtonCornerSize, Color.White );
            }

            Screen.Game.SpriteBatch.Draw( Screen.Style.DropDownArrow,
                new Vector2( LayoutRect.Right - Padding.Right - TextPadding.Right - Screen.Style.DropDownArrow.Width, LayoutRect.Center.Y - Screen.Style.DropDownArrow.Height / 2 ),
                Color.White
            );

            mCurrentItemLabel.Draw();
        }

        //----------------------------------------------------------------------
        public override Widget HitTest( Point _point )
        {
            if( HasFocus && IsOpen )
            {
                return this;
            }

            if( HitBox.Contains( _point ) )
            {
                return this;
            }
            else
            {
                return null;
            }
        }

        //----------------------------------------------------------------------
        internal override void DrawFocused()
        {
            if( IsOpen )
            {
                int iLinesDisplayed = Math.Min( siMaxLineDisplayed, Items.Count );

                Screen.DrawBox( Screen.Style.ListFrame, new Rectangle( LayoutRect.X, LayoutRect.Bottom, LayoutRect.Width, iLinesDisplayed * ( Screen.Style.MediumFont.LineSpacing + TextPadding.Vertical ) + Padding.Vertical ), Screen.Style.ButtonCornerSize, Color.White );

                int iMaxIndex = Math.Min( Items.Count - 1, ScrollItemOffset + iLinesDisplayed - 1 );
                for( int iIndex = ScrollItemOffset; iIndex <= iMaxIndex; iIndex++ )
                {
                    if( Screen.IsActive && miHoveredItemIndex == iIndex )
                    {
                        Screen.DrawBox( Screen.Style.GridBoxFrameHover, new Rectangle( LayoutRect.X + TextPadding.Left, LayoutRect.Bottom + ( Screen.Style.MediumFont.LineSpacing + TextPadding.Vertical ) * ( iIndex - ScrollItemOffset ) + TextPadding.Top, LayoutRect.Width - TextPadding.Horizontal, Screen.Style.MediumFont.LineSpacing + TextPadding.Vertical + 10 ), 10, Color.White );
                    }

                    Items[iIndex].Draw();
                }

                mScrollbar.Draw();
            }
        }

    }
}
