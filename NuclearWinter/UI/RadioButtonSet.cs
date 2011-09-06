using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using NuclearWinter.Animation;
using Microsoft.Xna.Framework.Graphics;

namespace NuclearWinter.UI
{
    public class RadioButtonSet: Widget
    {
        //----------------------------------------------------------------------
        public struct RadioButtonSetStyle
        {
            //------------------------------------------------------------------
            public int                      CornerSize;

            public Color                    TextColor;
            public Color                    TextDownColor;

            public Texture2D                ButtonFrameLeft;
            public Texture2D                ButtonFrameMiddle;
            public Texture2D                ButtonFrameRight;

            public Texture2D                ButtonFrameLeftDown;
            public Texture2D                ButtonFrameMiddleDown;
            public Texture2D                ButtonFrameRightDown;

            public Texture2D                ButtonFrameLeftFocus;
            public Texture2D                ButtonFrameLeftDownFocus;
            public Texture2D                ButtonFrameMiddleFocus;
            public Texture2D                ButtonFrameMiddleDownFocus;
            public Texture2D                ButtonFrameRightFocus;
            public Texture2D                ButtonFrameRightDownFocus;

            //------------------------------------------------------------------
            public RadioButtonSetStyle(
                int         _iCornerSize,

                Color       _textColor,
                Color       _textDownColor,

                Texture2D   _buttonFrameLeft,
                Texture2D   _buttonFrameMiddle,
                Texture2D   _buttonFrameRight,

                Texture2D   _buttonFrameLeftDown,
                Texture2D   _buttonFrameMiddleDown,
                Texture2D   _buttonFrameRightDown,

                Texture2D   _buttonFrameLeftFocus,
                Texture2D   _buttonFrameLeftDownFocus,
                Texture2D   _buttonFrameMiddleFocus,
                Texture2D   _buttonFrameMiddleDownFocus,
                Texture2D   _buttonFrameRightFocus,
                Texture2D   _buttonFrameRightDownFocus
            )
            {
                CornerSize              = _iCornerSize;

                TextColor               = _textColor;
                TextDownColor           = _textDownColor;

                ButtonFrameLeft         = _buttonFrameLeft;
                ButtonFrameMiddle       = _buttonFrameMiddle;
                ButtonFrameRight        = _buttonFrameRight;

                ButtonFrameLeftDown     = _buttonFrameLeftDown;
                ButtonFrameMiddleDown   = _buttonFrameMiddleDown;
                ButtonFrameRightDown    = _buttonFrameRightDown;

                ButtonFrameLeftFocus        = _buttonFrameLeftFocus;
                ButtonFrameLeftDownFocus    = _buttonFrameLeftDownFocus;
                ButtonFrameMiddleFocus      = _buttonFrameMiddleFocus;
                ButtonFrameMiddleDownFocus  = _buttonFrameMiddleDownFocus;
                ButtonFrameRightFocus       = _buttonFrameRightFocus;
                ButtonFrameRightDownFocus   = _buttonFrameRightDownFocus;
           }
        }

        //----------------------------------------------------------------------
        List<Button>                    mlButtons;

        int                             miHoveredButton;
        bool                            mbIsPressed;

        RadioButtonSetStyle             mStyle;
        public RadioButtonSetStyle Style
        {
            get { return mStyle; }
            set {
                mStyle = value;

                int i = 0;
                foreach( Button button in mlButtons )
                {
                    button.Parent = this;
                    button.TextColor = ( SelectedButtonIndex == i ) ? mStyle.TextDownColor : mStyle.TextColor;
                    button.Padding = new Box( 0, 0, 0, 0 );

                    button.Style.FrameDown  = Style.ButtonFrameMiddleDown;
                    button.ClickHandler     = ButtonClicked;

                    i++;
                }

                Button firstButton = mlButtons.First();
                firstButton.Style.FrameDown = Style.ButtonFrameLeftDown;
                firstButton.Padding = new Box( 0, 0, 0, 5 );

                Button lastButton = mlButtons.Last();
                lastButton.Style.FrameDown = Style.ButtonFrameRightDown;
                lastButton.Padding = new Box( 0, 5, 0, 0 );
            }
        }

        public Action<RadioButtonSet>   ClickHandler;
        int                             miSelectedButtonIndex = 0;
        public int                      SelectedButtonIndex
        {
            get {
                return miSelectedButtonIndex;
            }

            set {
                miSelectedButtonIndex = value;

                for( int iButton = 0; iButton < mlButtons.Count; iButton++ )
                {
                    Button button = mlButtons[iButton];

                    button.Style.CornerSize     = Style.CornerSize;

                    if( iButton == miSelectedButtonIndex )
                    {
                        button.TextColor            = mStyle.TextDownColor;
                        if( iButton == 0 )
                        {
                            button.Style.Frame          = Style.ButtonFrameLeftDown;
                            button.Style.FrameFocus     = Style.ButtonFrameLeftDownFocus;
                        }
                        else
                        if( iButton == mlButtons.Count - 1 )
                        {
                            button.Style.Frame          = Style.ButtonFrameRightDown;
                            button.Style.FrameFocus     = Style.ButtonFrameRightDownFocus;
                        }
                        else
                        {
                            button.Style.Frame          = Style.ButtonFrameMiddleDown;
                            button.Style.FrameFocus     = Style.ButtonFrameMiddleDownFocus;
                        }
                    }
                    else
                    {
                        button.TextColor            = mStyle.TextColor;

                        if( iButton == 0 )
                        {
                            button.Style.Frame          = Style.ButtonFrameLeft;
                            button.Style.FrameFocus     = Style.ButtonFrameLeftFocus;
                        }
                        else
                        if( iButton == mlButtons.Count - 1 )
                        {
                            button.Style.Frame          = Style.ButtonFrameRight;
                            button.Style.FrameFocus     = Style.ButtonFrameRightFocus;
                        }
                        else
                        {
                            button.Style.Frame          = Style.ButtonFrameMiddle;
                            button.Style.FrameFocus     = Style.ButtonFrameMiddleFocus;
                        }
                    }
                }
            }
        }

        //----------------------------------------------------------------------
        public override Widget GetFirstFocusableDescendant( Direction _direction )
        {
            switch( _direction )
            {
                case Direction.Left:
                    return mlButtons[ mlButtons.Count - 1 ];
                default:
                    return mlButtons[ 0 ];
            }
        }

        //----------------------------------------------------------------------
        public override Widget GetSibling( Direction _direction, Widget _child )
        {
            int iIndex = mlButtons.IndexOf( (Button)_child );

            switch( _direction )
            {
                case Direction.Left:
                    if( iIndex > 0 )
                    {
                        return mlButtons[iIndex - 1];
                    }
                    break;
                case Direction.Right:
                    if( iIndex < mlButtons.Count - 1 )
                    {
                        return mlButtons[iIndex + 1];
                    }
                    break;
            }

            return base.GetSibling( _direction, this );
        }

        //----------------------------------------------------------------------
        public RadioButtonSet( Screen _screen, List<Button> _lButtons, int _iInitialButtonIndex )
        : base( _screen )
        {
            mlButtons = _lButtons;

            Style = new RadioButtonSetStyle(
                Screen.Style.ButtonCornerSize,
                
                Color.White * 0.6f,
                Color.White,
                Screen.Style.ButtonFrameLeft,
                Screen.Style.ButtonFrameMiddle,
                Screen.Style.ButtonFrameRight,

                Screen.Style.ButtonFrameLeftDown,
                Screen.Style.ButtonFrameMiddleDown,
                Screen.Style.ButtonFrameRightDown,

                Screen.Style.ButtonFrameLeftFocus,
                Screen.Style.ButtonFrameLeftDownFocus,
                Screen.Style.ButtonFrameMiddleFocus,
                Screen.Style.ButtonFrameMiddleDownFocus,
                Screen.Style.ButtonFrameRightFocus,
                Screen.Style.ButtonFrameRightDownFocus
            );

            SelectedButtonIndex = _iInitialButtonIndex;

            UpdateContentSize();
        }

        public RadioButtonSet( Screen _screen, List<Button> _lButtons )
        : this( _screen, _lButtons, 0 )
        {
        }

        //----------------------------------------------------------------------
        public RadioButtonSet( Screen _screen, RadioButtonSetStyle _style, List<Button> _lButtons, int _iInitialButtonIndex )
        : base( _screen )
        {
            mlButtons = _lButtons;

            Style = _style;

            SelectedButtonIndex = _iInitialButtonIndex;

            UpdateContentSize();
        }

        public RadioButtonSet( Screen _screen, RadioButtonSetStyle _style, List<Button> _lButtons )
        : this( _screen, _style, _lButtons, 0 )
        {
        }

        //----------------------------------------------------------------------
        internal override void UpdateContentSize()
        {
            ContentWidth    = Padding.Horizontal;
            ContentHeight   = 0;
            foreach( Button button in mlButtons )
            {
                ContentWidth += button.ContentWidth;
                ContentHeight = Math.Max( ContentHeight, button.ContentHeight );
            }

            ContentHeight += Padding.Vertical;

            base.UpdateContentSize();
        }

        //----------------------------------------------------------------------
        internal override void DoLayout( Rectangle _rect )
        {
            Position = _rect.Location;
            Size = new Point( _rect.Width, _rect.Height );

            Point pCenter = new Point( Position.X + Size.X / 2, Position.Y + Size.Y / 2 );

            int iHeight = Size.Y;

            HitBox = new Rectangle(
                pCenter.X - ContentWidth / 2,
                pCenter.Y - iHeight / 2,
                ContentWidth,
                iHeight
            );

            int iButton = 0;
            int iButtonX = 0;
            foreach( Button button in mlButtons )
            {
                button.DoLayout( new Rectangle(
                    HitBox.X + iButtonX, pCenter.Y - iHeight / 2,
                    button.ContentWidth, iHeight
                ) );

                iButtonX += button.ContentWidth;
                iButton++;
            }
        }
        
        //----------------------------------------------------------------------
        internal override void OnMouseEnter( Point _hitPoint )
        {
            base.OnMouseEnter( _hitPoint );
            UpdateHoveredButton( _hitPoint );

            mlButtons[miHoveredButton].OnMouseEnter( _hitPoint );
        }

        internal override void OnMouseOut( Point _hitPoint )
        {
            base.OnMouseOut( _hitPoint );

            mlButtons[miHoveredButton].OnMouseOut( _hitPoint );
        }

        internal override void OnMouseMove(Point _hitPoint)
        {
            base.OnMouseMove(_hitPoint);

            if( ! mbIsPressed )
            {
                int iPreviousHoveredButton = miHoveredButton;
                UpdateHoveredButton( _hitPoint );

                if( iPreviousHoveredButton != miHoveredButton )
                {
                    mlButtons[iPreviousHoveredButton].OnMouseOut( _hitPoint );
                }
                mlButtons[miHoveredButton].OnMouseEnter( _hitPoint );
            }
            else
            {
                mlButtons[miHoveredButton].OnMouseMove( _hitPoint );
            }
        }

        void UpdateHoveredButton( Point _hitPoint )
        {
            int iButton = 0;
            foreach( Button button in mlButtons )
            {
                if( button.HitTest( _hitPoint ) != null )
                {
                    miHoveredButton = iButton;
                    break;
                }

                iButton++;
            }
        }

        //----------------------------------------------------------------------
        internal override void OnMouseDown( Point _hitPoint )
        {
            base.OnMouseDown( _hitPoint );

            mbIsPressed = true;
            mlButtons[miHoveredButton].OnMouseDown( _hitPoint );
        }

        internal override void OnMouseUp( Point _hitPoint )
        {
            mbIsPressed = false;
            mlButtons[miHoveredButton].OnMouseUp( _hitPoint );
        }

        public void ButtonClicked( Button _button )
        {
            SelectedButtonIndex = mlButtons.IndexOf( _button );
            if( ClickHandler != null )
            {
                ClickHandler( this );
            }
        }

        //----------------------------------------------------------------------
        internal override void Draw()
        {
            foreach( Button button in mlButtons )
            {
                button.Draw();
            }
        }
    }
}
