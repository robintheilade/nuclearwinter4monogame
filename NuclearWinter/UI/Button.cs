using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using NuclearWinter.Animation;
using Microsoft.Xna.Framework.Input;

namespace NuclearWinter.UI
{
    /*
     * A clickable Button containing a Label and an optional Image icon
     */
    public class Button: Widget
    {
        //----------------------------------------------------------------------
        public struct ButtonStyle
        {
            //------------------------------------------------------------------
            public int              CornerSize;
            public Texture2D        Frame;
            public Texture2D        FrameDown;
            public Texture2D        FrameHover;
            public Texture2D        FramePressed;
            public Texture2D        FrameFocused;

            //------------------------------------------------------------------
            public ButtonStyle(
                int         _iCornerSize,
                Texture2D   _buttonFrame,
                Texture2D   _buttonFrameDown,
                Texture2D   _buttonFrameHover,
                Texture2D   _buttonFramePressed,
                Texture2D   _buttonFrameFocused
            )
            {
                CornerSize      = _iCornerSize;
                Frame           = _buttonFrame;
                FrameDown       = _buttonFrameDown;
                FrameHover      = _buttonFrameHover;
                FramePressed    = _buttonFramePressed;
                FrameFocused    = _buttonFrameFocused;
            }
        }

        //----------------------------------------------------------------------
        Label                   mLabel;
        Image                   mIcon;
        Buttons                 mBoundPadButton;

        bool                    mbIsHovered;

        AnimatedValue           mPressedAnim;
        bool                    mbIsPressed;

        public string           Text
        {
            get
            {
                return mLabel.Text;
            }
            
            set
            {
                mLabel.Text = value;
                mLabel.Padding = mLabel.Text != "" ? new Box( 10, 20, 10, 20 ) : new Box( 10, 20, 10, 0 );
                UpdateContentSize();
            }
        }

        public Texture2D        Icon
        {
            get {
                return mIcon.Texture;
            }

            set
            {
                mIcon.Texture = value;
                UpdateContentSize();
            }
        }

        Anchor mAnchor;
        public Anchor Anchor
        {
            get {
                return mAnchor;
            }

            set
            {
                mAnchor = value;
            }
        }

        public Color TextColor
        {
            get { return mLabel.Color; }
            set { mLabel.Color = value; }
        }

        public override bool CanFocus { get { return true; } }

        public ButtonStyle Style;

        public Action<Button>   ClickHandler;

        //----------------------------------------------------------------------
        public Button( Screen _screen, ButtonStyle _style, string _strText, Texture2D _iconTex, Anchor _anchor )
        : base( _screen )
        {
            mLabel          = new Label( _screen );

            mIcon           = new Image( _screen );
            mIcon.Texture   = _iconTex;
            mIcon.Padding   = new Box( 10, 0, 10, 20 );

            Text            = _strText;

            Anchor          = _anchor;

            mPressedAnim    = new SmoothValue( 1f, 0f, 0.2f );
            mPressedAnim.SetTime( mPressedAnim.Duration );

            TextColor           = Screen.Style.ButtonTextColor;

            Style = _style;

            UpdateContentSize();
        }

        //----------------------------------------------------------------------
        public Button( Screen _screen, string _strText, Texture2D _iconTex, Anchor _anchor )
        : this( _screen, new ButtonStyle(
                _screen.Style.ButtonCornerSize,
                _screen.Style.ButtonFrame,
                _screen.Style.ButtonFrameDown,
                _screen.Style.ButtonFrameHover,
                _screen.Style.ButtonFramePressed,
                _screen.Style.ButtonFrameFocus
            ), _strText, _iconTex, _anchor )
        {
        }

        //----------------------------------------------------------------------
        public Button( Screen _screen, string _strText, Texture2D _iconTex )
        : this( _screen, _strText, _iconTex, Anchor.Center )
        {

        }

        //----------------------------------------------------------------------
        public Button( Screen _screen, string _strText )
        : this( _screen, _strText, null, Anchor.Center )
        {
        }

        //----------------------------------------------------------------------
        public Button( Screen _screen )
        : this( _screen, "", null, Anchor.Center )
        {
        }

        //----------------------------------------------------------------------
        protected override void UpdateContentSize()
        {
            if( mIcon.Texture != null )
            {
                ContentWidth    = mIcon.ContentWidth + mLabel.ContentWidth + Padding.Left + Padding.Right;
            }
            else
            {
                ContentWidth    = mLabel.ContentWidth + Padding.Left + Padding.Right;
            }

            ContentHeight   = Math.Max( mIcon.ContentHeight, mLabel.ContentHeight ) + Padding.Top + Padding.Bottom;
        }


        //----------------------------------------------------------------------
        public override void DoLayout( Rectangle? _rect )
        {
            if( _rect.HasValue )
            {
                Position = _rect.Value.Location;
                Size = new Point( _rect.Value.Width, _rect.Value.Height );
            }

            HitBox = new Rectangle( Position.X, Position.Y, Size.X, Size.Y );
            Point pCenter = new Point( Position.X + Size.X / 2, Position.Y + Size.Y / 2 );

            switch( mAnchor )
            {
                case UI.Anchor.Start:
                    if( mIcon.Texture != null )
                    {
                        mIcon.Position = new Point( Position.X + Padding.Left, pCenter.Y - mIcon.ContentHeight / 2 );
                    }

                    mLabel.DoLayout(
                        new Rectangle(
                            Position.X + Padding.Left + ( mIcon.Texture != null ? mIcon.ContentWidth : 0 ), pCenter.Y - mLabel.ContentHeight / 2,
                            mLabel.ContentWidth, mLabel.ContentHeight
                        )
                    );
                    break;
                case UI.Anchor.Center:
                    if( mIcon.Texture != null )
                    {
                        mIcon.Position = new Point( pCenter.X - ContentWidth / 2 + Padding.Left, pCenter.Y - mIcon.ContentHeight / 2 );
                    }

                    mLabel.DoLayout(
                        new Rectangle(
                            pCenter.X - ContentWidth / 2 + Padding.Left + ( mIcon.Texture != null ? mIcon.ContentWidth : 0 ), pCenter.Y - mLabel.ContentHeight / 2,
                            mLabel.ContentWidth, mLabel.ContentHeight
                        )
                    );
                    break;
            }
        }

        //----------------------------------------------------------------------
        public override bool Update( float _fElapsedTime )
        {
            mPressedAnim.Update( _fElapsedTime );
            return ! mPressedAnim.IsOver;
        }

        //----------------------------------------------------------------------
        public override void OnMouseEnter( Point _hitPoint )
        {
            base.OnMouseEnter( _hitPoint );
            mbIsHovered = true;
        }

        public override void OnMouseOut( Point _hitPoint )
        {
            base.OnMouseOut( _hitPoint );
            mbIsHovered = false;
        }

        //----------------------------------------------------------------------
        public override void OnMouseDown( Point _hitPoint )
        {
            base.OnMouseDown( _hitPoint );

            Screen.Focus( this );
            OnActivateDown();
        }

        public override void OnMouseUp( Point _hitPoint )
        {
            if( HitTest( _hitPoint ) == this )
            {
                OnActivateUp();
            }
            else
            {
                ResetPressState();
            }
        }

        //----------------------------------------------------------------------
        public void BindPadButton( Buttons _button )
        {
            mBoundPadButton = _button;
        }

        //----------------------------------------------------------------------
        public override bool OnPadButton( Buttons _button, bool _bIsDown )
        {
            if( _button == mBoundPadButton )
            {
                if( _bIsDown )
                {
                    Screen.Focus( this );
                    OnActivateDown();
                }
                else
                {
                    OnActivateUp();
                }

                return true;
            }

            return false;
        }

        //----------------------------------------------------------------------
        public override void OnActivateDown()
        {
            mbIsPressed = true;
            mPressedAnim.SetTime( 0f );
        }

        public override void OnActivateUp()
        {
            mPressedAnim.SetTime( 0f );
            mbIsPressed = false;
            Screen.AddWidgetToUpdateList( this );
            if( ClickHandler != null ) ClickHandler( this );
        }

        public override void OnBlur()
        {
            ResetPressState();
        }

        //----------------------------------------------------------------------
        void ResetPressState()
        {
            mPressedAnim.SetTime( 1f );
            mbIsPressed = false;
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            Screen.DrawBox( (!mbIsPressed) ? Style.Frame : Style.FrameDown, new Rectangle( Position.X, Position.Y, Size.X, Size.Y ), Style.CornerSize, Color.White );

            if( mbIsHovered && ! mbIsPressed && mPressedAnim.IsOver )
            {
                Screen.DrawBox( Style.FrameHover,      new Rectangle( Position.X, Position.Y, Size.X, Size.Y ), Style.CornerSize, Color.White );
            }
            else
            if( mPressedAnim.CurrentValue > 0f )
            {
                Screen.DrawBox( Style.FramePressed,    new Rectangle( Position.X, Position.Y, Size.X, Size.Y ), Style.CornerSize, Color.White * mPressedAnim.CurrentValue );
            }

            if( HasFocus && ! mbIsPressed )
            {
                Screen.DrawBox( Style.FrameFocused, new Rectangle( Position.X, Position.Y, Size.X, Size.Y ), Style.CornerSize, Color.White );
            }

            mLabel.Draw();

            mIcon.Draw();
        }
    }
}
