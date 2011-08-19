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

            public Texture2D                ButtonFrameLeft;
            public Texture2D                ButtonFrameMiddle;
            public Texture2D                ButtonFrameRight;

            public Texture2D                ButtonFrameLeftDown;
            public Texture2D                ButtonFrameMiddleDown;
            public Texture2D                ButtonFrameRightDown;

            public Texture2D                ButtonFrameFocused;
            public Texture2D                ButtonFrameDownFocused;

            //------------------------------------------------------------------
            public RadioButtonSetStyle(
                int         _iCornerSize,

                Texture2D   _buttonFrameLeft,
                Texture2D   _buttonFrameMiddle,
                Texture2D   _buttonFrameRight,

                Texture2D   _buttonFrameLeftDown,
                Texture2D   _buttonFrameMiddleDown,
                Texture2D   _buttonFrameRightDown,

                Texture2D   _buttonFrameFocused,
                Texture2D   _buttonFrameDownFocused
            )
            {
                CornerSize              = _iCornerSize;
                ButtonFrameLeft         = _buttonFrameLeft;
                ButtonFrameMiddle       = _buttonFrameMiddle;
                ButtonFrameRight        = _buttonFrameRight;

                ButtonFrameLeftDown     = _buttonFrameLeftDown;
                ButtonFrameMiddleDown   = _buttonFrameMiddleDown;
                ButtonFrameRightDown    = _buttonFrameRightDown;

                ButtonFrameFocused      = _buttonFrameFocused;
                ButtonFrameDownFocused  = _buttonFrameDownFocused;
           }
        }

        //----------------------------------------------------------------------
        List<Button>                    mlButtons;

        public bool                     Expand;

        int                             miHoveredButton;
        bool                            mbIsPressed;

        RadioButtonSetStyle             mStyle;
        public RadioButtonSetStyle Style
        {
            get { return mStyle; }
            set {
                mStyle = value;

                foreach( Button button in mlButtons )
                {
                    button.Parent = this;
                    button.Padding = new Box( 0, 0, 0, 0 );

                    button.Style.FrameDown  = Style.ButtonFrameMiddleDown;
                    button.ClickHandler     = ButtonClicked;
                }

                Button firstButton = mlButtons.First();
                firstButton.Style.FrameDown = Style.ButtonFrameLeftDown;

                Button lastButton = mlButtons.Last();
                lastButton.Style.FrameDown = Style.ButtonFrameRightDown;
            }
        }

        public override bool            CanFocus { get { return false; } }
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
                        button.Style.FrameFocused   = Style.ButtonFrameDownFocused;
                        if( iButton == 0 )
                        {
                            button.Style.Frame          = Style.ButtonFrameLeftDown;
                        }
                        else
                        if( iButton == mlButtons.Count - 1 )
                        {
                            button.Style.Frame          = Style.ButtonFrameRightDown;
                        }
                        else
                        {
                            button.Style.Frame          = Style.ButtonFrameMiddleDown;
                        }
                    }
                    else
                    {
                        button.Style.FrameFocused   = Style.ButtonFrameFocused;

                        if( iButton == 0 )
                        {
                            button.Style.Frame          = Style.ButtonFrameLeft;
                        }
                        else
                        if( iButton == mlButtons.Count - 1 )
                        {
                            button.Style.Frame      = Style.ButtonFrameRight;
                        }
                        else
                        {
                            button.Style.Frame      = Style.ButtonFrameMiddle;
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
                Screen.Style.ButtonFrameLeft,
                Screen.Style.ButtonFrameMiddle,
                Screen.Style.ButtonFrameRight,

                Screen.Style.ButtonFrameLeft,
                Screen.Style.ButtonFrameMiddle,
                Screen.Style.ButtonFrameRight,

                Screen.Style.ButtonFrameFocus,
                Screen.Style.ButtonFrameDownFocus
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
        protected override void UpdateContentSize()
        {
            ContentWidth    = Padding.Horizontal;
            ContentHeight   = 0;
            foreach( Button button in mlButtons )
            {
                ContentWidth += button.ContentWidth;
                ContentHeight = Math.Max( ContentHeight, button.ContentHeight );
            }

            ContentHeight += Padding.Vertical;
        }

        //----------------------------------------------------------------------
        public override void DoLayout( Rectangle? _rect )
        {
            if( _rect.HasValue )
            {
                Position = _rect.Value.Location;
                Size = new Point( _rect.Value.Width, _rect.Value.Height );
            }

            Point pCenter = new Point( Position.X + Size.X / 2, Position.Y + Size.Y / 2 );

            int iHeight = Expand ? Size.Y : ContentHeight;

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
        public override void OnMouseEnter( Point _hitPoint )
        {
            base.OnMouseEnter( _hitPoint );
            UpdateHoveredButton( _hitPoint );

            mlButtons[miHoveredButton].OnMouseEnter( _hitPoint );
        }

        public override void OnMouseOut( Point _hitPoint )
        {
            base.OnMouseOut( _hitPoint );

            mlButtons[miHoveredButton].OnMouseOut( _hitPoint );
        }

        public override void OnMouseMove(Point _hitPoint)
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
        public override void OnMouseDown( Point _hitPoint )
        {
            base.OnMouseDown( _hitPoint );

            mbIsPressed = true;
            mlButtons[miHoveredButton].OnMouseDown( _hitPoint );
        }

        public override void OnMouseUp( Point _hitPoint )
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
        public override void Draw()
        {
            foreach( Button button in mlButtons )
            {
                button.Draw();
            }
        }
    }
}
