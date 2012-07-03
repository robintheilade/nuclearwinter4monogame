using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using System.Linq;
using System.Collections;
using System.Diagnostics;

namespace NuclearWinter.Input
{
    public class InputManager: GameComponent
    {
        public const int                            siMaxInput              = 4;
        public const float                          sfStickThreshold        = 0.4f;

        public readonly GamePadState[]              GamePadStates;
        public readonly GamePadState[]              PreviousGamePadStates;

#if WINDOWS
        public MouseState                           MouseState              { get; private set; }
        public MouseState                           PreviousMouseState      { get; private set; }

        public int PrimaryMouseButton
        {
            get { return System.Windows.Forms.SystemInformation.MouseButtonsSwapped ? 2 : 0; }
        }

        public int SecondaryMouseButton
        {
            get { return System.Windows.Forms.SystemInformation.MouseButtonsSwapped ? 0 : 2; }
        }

        public LocalizedKeyboardState               KeyboardState           { get; private set; }
        public LocalizedKeyboardState               PreviousKeyboardState   { get; private set; }
        public PlayerIndex?                         KeyboardPlayerIndex     { get; private set; }
        /*Keys                                        mLastKeyPressed;
        bool                                        mbRepeatKey;
        float                                       mfRepeatKeyTimer;*/

        public List<char>                           EnteredText             { get; private set; }
        public List<Keys>                           JustPressedKeys         { get; private set; }
        public List<System.Windows.Forms.Keys>      JustPressedWindowsKeys  { get; private set; }

        WindowMessageFilter                         mMessageFilter;
        bool                                        mbDoubleClicked;
#endif

        Buttons[]                                   maLastPressedButtons;
        float[]                                     mafRepeatTimers;
        bool[]                                      mabRepeatButtons;
        const float                                 sfButtonRepeatDelay     = 0.3f;
        const float                                 sfButtonRepeatInterval  = 0.1f;

        List<Buttons>                               lButtons;

        //---------------------------------------------------------------------
        public InputManager( Game _game )
        : base ( _game )
        {
            GamePadStates           = new GamePadState[ siMaxInput ];
            PreviousGamePadStates   = new GamePadState[ siMaxInput ];

            maLastPressedButtons    = new Buttons[ siMaxInput ];
            mafRepeatTimers         = new float[ siMaxInput ];
            mabRepeatButtons        = new bool[ siMaxInput ];

            lButtons                = Utils.GetValues<Buttons>();

#if WINDOWS
            EnteredText             = new List<char>();
            JustPressedKeys         = new List<Keys>();
            JustPressedWindowsKeys  = new List<System.Windows.Forms.Keys>();

            mMessageFilter              = new WindowMessageFilter( Game.Window.Handle );
            mMessageFilter.CharacterHandler = delegate( char _char ) { EnteredText.Add( _char ); };
            mMessageFilter.KeyDownHandler   = delegate( System.Windows.Forms.Keys _key ) { JustPressedWindowsKeys.Add( _key ); };
            mMessageFilter.DoubleClickHandler   = delegate { mbDoubleClicked = true; };

            MouseState = Mouse.GetState();
            PreviousMouseState = MouseState;
#endif
        }

        //---------------------------------------------------------------------
        public override void Update( GameTime _time )
        {
            float fElapsedTime = (float)_time.ElapsedGameTime.TotalSeconds;

            for( int i = 0; i < siMaxInput; i++ )
            {
                PreviousGamePadStates[ i ] = GamePadStates[ i ];
                GamePadStates[ i ] = GamePad.GetState( (PlayerIndex)i );
            }

#if WINDOWS
            if( ! IsMouseCaptured )
            {
                PreviousMouseState = MouseState;
            }
            else
            {
                PreviousMouseState = new MouseState(
                    PreviousMouseState.X, PreviousMouseState.Y, // Keep old mouse position
                    MouseState.ScrollWheelValue,
                    MouseState.LeftButton,
                    MouseState.MiddleButton,
                    MouseState.RightButton,
                    MouseState.XButton1,
                    MouseState.XButton2 );
            }

            MouseState = Mouse.GetState();

            if( IsMouseCaptured )
            {
                Mouse.SetPosition( PreviousMouseState.X, PreviousMouseState.Y );
            }


            PreviousKeyboardState = KeyboardState;
            KeyboardState = new LocalizedKeyboardState( Keyboard.GetState() );

            KeyboardPlayerIndex = null;
            for( int i = 0; i < siMaxInput; i++ )
            {
                if( ! GamePadStates[i].IsConnected )
                {
                    KeyboardPlayerIndex = (PlayerIndex)i;
                    break;
                }
            }

            EnteredText.Clear();
            JustPressedWindowsKeys.Clear();

            Keys[] currentPressedKeys = KeyboardState.Native.GetPressedKeys();
            Keys[] previousPressedKeys = PreviousKeyboardState.Native.GetPressedKeys();
            
            JustPressedKeys = currentPressedKeys.Except( previousPressedKeys ).ToList();

            mbDoubleClicked = false;
#endif

            for( int iGamePad = 0; iGamePad < siMaxInput; iGamePad++ )
            {
                mabRepeatButtons[ iGamePad ] = false;

                foreach( Buttons button in lButtons )
                {
                    bool bButtonPressed;

                    switch( button )
                    {
                        // Override for left stick
                        case Buttons.LeftThumbstickLeft:
                            bButtonPressed = GamePadStates[ iGamePad ].ThumbSticks.Left.X < -sfStickThreshold && PreviousGamePadStates[ iGamePad ].ThumbSticks.Left.X > -sfStickThreshold;
                            break;
                        case Buttons.LeftThumbstickRight:
                            bButtonPressed = GamePadStates[ iGamePad ].ThumbSticks.Left.X > sfStickThreshold && PreviousGamePadStates[ iGamePad ].ThumbSticks.Left.X < sfStickThreshold;
                            break;
                        case Buttons.LeftThumbstickDown:
                            bButtonPressed = GamePadStates[ iGamePad ].ThumbSticks.Left.Y < -sfStickThreshold && PreviousGamePadStates[ iGamePad ].ThumbSticks.Left.Y > -sfStickThreshold;
                            break;
                        case Buttons.LeftThumbstickUp:
                            bButtonPressed = GamePadStates[ iGamePad ].ThumbSticks.Left.Y > sfStickThreshold && PreviousGamePadStates[ iGamePad ].ThumbSticks.Left.Y < sfStickThreshold;
                            break;

                        // Override for right stick
                        case Buttons.RightThumbstickLeft:
                            bButtonPressed = GamePadStates[ iGamePad ].ThumbSticks.Right.X < -sfStickThreshold && PreviousGamePadStates[ iGamePad ].ThumbSticks.Right.X > -sfStickThreshold;
                            break;
                        case Buttons.RightThumbstickRight:
                            bButtonPressed = GamePadStates[ iGamePad ].ThumbSticks.Right.X > sfStickThreshold && PreviousGamePadStates[ iGamePad ].ThumbSticks.Right.X < sfStickThreshold;
                            break;
                        case Buttons.RightThumbstickDown:
                            bButtonPressed = GamePadStates[ iGamePad ].ThumbSticks.Right.Y < -sfStickThreshold && PreviousGamePadStates[ iGamePad ].ThumbSticks.Right.Y > -sfStickThreshold;
                            break;
                        case Buttons.RightThumbstickUp:
                            bButtonPressed = GamePadStates[ iGamePad ].ThumbSticks.Right.Y > sfStickThreshold && PreviousGamePadStates[ iGamePad ].ThumbSticks.Right.Y < sfStickThreshold;
                            break;
                        
                        // Default button behavior for the rest
                        default:
                            bButtonPressed = GamePadStates[ iGamePad ].IsButtonDown( button ) && PreviousGamePadStates[ iGamePad ].IsButtonUp( button );
                            break;
                    }

#if WINDOWS
                    if( ! bButtonPressed )
                    {
                        Keys key = GetKeyboardMapping( button );
                        bButtonPressed = key != Keys.None && KeyboardState.IsKeyDown( key ) && ! PreviousKeyboardState.IsKeyDown( key );
                    }
#endif

                    if( bButtonPressed )
                    {
                        mafRepeatTimers[ iGamePad ] = 0f;
                        maLastPressedButtons[ iGamePad ] = button;
                        break;
                    }
                }

                if( maLastPressedButtons[iGamePad] != 0 )
                {
                    bool bIsButtonStillDown = GamePadStates[ iGamePad ].IsButtonDown( maLastPressedButtons[ iGamePad ] );

#if WINDOWS
                    if( ! bIsButtonStillDown )
                    {
                        Keys key = GetKeyboardMapping( maLastPressedButtons[iGamePad] );
                        bIsButtonStillDown = key != Keys.None && KeyboardState.IsKeyDown( key );
                    }
#endif

                    if( bIsButtonStillDown )
                    {
                        float fRepeatValue      = ( mafRepeatTimers[ iGamePad ] - sfButtonRepeatDelay ) % ( sfButtonRepeatInterval );
                        float fNewRepeatValue   = ( mafRepeatTimers[ iGamePad ] + fElapsedTime - sfButtonRepeatDelay ) % ( sfButtonRepeatInterval );

                        if( mafRepeatTimers[ iGamePad ] < sfButtonRepeatDelay && mafRepeatTimers[ iGamePad ] + fElapsedTime >= sfButtonRepeatDelay )
                        {
                            mabRepeatButtons[ iGamePad ] = true;
                        }
                        else
                        if( mafRepeatTimers[ iGamePad ] > sfButtonRepeatDelay && fRepeatValue > fNewRepeatValue )
                        {
                            mabRepeatButtons[ iGamePad ] = true;
                        }

                        mafRepeatTimers[ iGamePad ] += fElapsedTime;
                    }
                    else
                    {
                        mafRepeatTimers[ iGamePad ] = 0f;
                        maLastPressedButtons[ iGamePad ] = 0;
                    }
                }
            }


        }
        
#if WINDOWS
        //----------------------------------------------------------------------
        public bool IsMouseButtonDown( int _iButton )
        {
            switch( _iButton )
            {
                case 0:
                    return MouseState.LeftButton    == ButtonState.Pressed;
                case 1:
                    return MouseState.MiddleButton  == ButtonState.Pressed;
                case 2:
                    return MouseState.RightButton   == ButtonState.Pressed;
                case 3:
                    return MouseState.XButton1      == ButtonState.Pressed;
                case 4:
                    return MouseState.XButton2      == ButtonState.Pressed;
                case 5:
                    return ( MouseState.ScrollWheelValue - PreviousMouseState.ScrollWheelValue ) > 0;
                case 6:
                    return ( MouseState.ScrollWheelValue - PreviousMouseState.ScrollWheelValue ) < 0;
                default:
                    return false;
            }
        }

        //----------------------------------------------------------------------
        public bool WasMouseButtonJustPressed( int _iButton )
        {
            switch( _iButton )
            {
                case 0:
                    return MouseState.LeftButton    == ButtonState.Pressed && PreviousMouseState.LeftButton     == ButtonState.Released;
                case 1:
                    return MouseState.MiddleButton  == ButtonState.Pressed && PreviousMouseState.MiddleButton   == ButtonState.Released;
                case 2:
                    return MouseState.RightButton   == ButtonState.Pressed && PreviousMouseState.RightButton    == ButtonState.Released;
                case 3:
                    return MouseState.XButton1      == ButtonState.Pressed && PreviousMouseState.XButton1       == ButtonState.Released;
                case 4:
                    return MouseState.XButton2      == ButtonState.Pressed && PreviousMouseState.XButton2       == ButtonState.Released;
                case 5:
                    return ( MouseState.ScrollWheelValue - PreviousMouseState.ScrollWheelValue ) > 0;
                case 6:
                    return ( MouseState.ScrollWheelValue - PreviousMouseState.ScrollWheelValue ) < 0;
                default:
                    return false;
            }
        }

        //----------------------------------------------------------------------
        public bool WasMouseButtonJustReleased( int _iButton )
        {
            switch( _iButton )
            {
                case 0:
                    return MouseState.LeftButton    == ButtonState.Released && PreviousMouseState.LeftButton     == ButtonState.Pressed;
                case 1:
                    return MouseState.MiddleButton  == ButtonState.Released && PreviousMouseState.MiddleButton   == ButtonState.Pressed;
                case 2:
                    return MouseState.RightButton   == ButtonState.Released && PreviousMouseState.RightButton    == ButtonState.Pressed;
                case 3:
                    return MouseState.XButton1      == ButtonState.Released && PreviousMouseState.XButton1       == ButtonState.Pressed;
                case 4:
                    return MouseState.XButton2      == ButtonState.Released && PreviousMouseState.XButton2       == ButtonState.Pressed;
                default:
                    return false;
            }
        }

        //----------------------------------------------------------------------
        public bool WasMouseJustDoubleClicked()
        {
            return mbDoubleClicked;
        }

        //----------------------------------------------------------------------
        public bool WasKeyJustPressed( Keys _key, bool _bNative=false )
        {
            if( ! _bNative )
            {
                return KeyboardState.IsKeyDown(_key) && ! PreviousKeyboardState.IsKeyDown(_key);
            }
            else
            {
                return KeyboardState.Native.IsKeyDown(_key) && ! PreviousKeyboardState.Native.IsKeyDown(_key);
            }
        }

        //----------------------------------------------------------------------
        public bool WasKeyJustReleased( Keys _key )
        {
            return KeyboardState.IsKeyUp(_key) && ! PreviousKeyboardState.IsKeyUp(_key);
        }


        //----------------------------------------------------------------------
        public int GetMouseWheelDelta()
        {
            return MouseState.ScrollWheelValue - PreviousMouseState.ScrollWheelValue;
        }
#endif

        //----------------------------------------------------------------------
        public bool WasButtonJustPressed( Buttons _button, PlayerIndex _controllingPlayer )
        {
            return WasButtonJustPressed( _button, _controllingPlayer, false );
        }

        //----------------------------------------------------------------------
        public bool WasButtonJustPressed( Buttons _button, PlayerIndex _controllingPlayer, bool _bRepeat )
        {
            PlayerIndex discardedPlayerIndex;
            return WasButtonJustPressed( _button, _controllingPlayer, out discardedPlayerIndex, _bRepeat );
        }

        //----------------------------------------------------------------------
        public bool WasButtonJustPressed( Buttons _button, PlayerIndex? _controllingPlayer, out PlayerIndex _playerIndex )
        {
            return WasButtonJustPressed( _button, _controllingPlayer, out _playerIndex, false );
        }

        //----------------------------------------------------------------------
        public bool WasButtonJustPressed( Buttons _button, PlayerIndex? _controllingPlayer, out PlayerIndex _playerIndex, bool _bRepeat )
        {
            if( _controllingPlayer.HasValue )
            {
                _playerIndex = _controllingPlayer.Value;
                int iGamePad = (int)_playerIndex;

                //--------------------------------------------------------------
                bool bButtonPressed;

                switch( _button )
                {
                    // Override for left stick
                    case Buttons.LeftThumbstickLeft:
                        bButtonPressed = GamePadStates[ iGamePad ].ThumbSticks.Left.X < -sfStickThreshold && PreviousGamePadStates[ iGamePad ].ThumbSticks.Left.X > -sfStickThreshold;
                        break;
                    case Buttons.LeftThumbstickRight:
                        bButtonPressed = GamePadStates[ iGamePad ].ThumbSticks.Left.X > sfStickThreshold && PreviousGamePadStates[ iGamePad ].ThumbSticks.Left.X < sfStickThreshold;
                        break;
                    case Buttons.LeftThumbstickDown:
                        bButtonPressed = GamePadStates[ iGamePad ].ThumbSticks.Left.Y < -sfStickThreshold && PreviousGamePadStates[ iGamePad ].ThumbSticks.Left.Y > -sfStickThreshold;
                        break;
                    case Buttons.LeftThumbstickUp:
                        bButtonPressed = GamePadStates[ iGamePad ].ThumbSticks.Left.Y > sfStickThreshold && PreviousGamePadStates[ iGamePad ].ThumbSticks.Left.Y < sfStickThreshold;
                        break;

                    // Override for right stick
                    case Buttons.RightThumbstickLeft:
                        bButtonPressed = GamePadStates[ iGamePad ].ThumbSticks.Right.X < -sfStickThreshold && PreviousGamePadStates[ iGamePad ].ThumbSticks.Right.X > -sfStickThreshold;
                        break;
                    case Buttons.RightThumbstickRight:
                        bButtonPressed = GamePadStates[ iGamePad ].ThumbSticks.Right.X > sfStickThreshold && PreviousGamePadStates[ iGamePad ].ThumbSticks.Right.X < sfStickThreshold;
                        break;
                    case Buttons.RightThumbstickDown:
                        bButtonPressed = GamePadStates[ iGamePad ].ThumbSticks.Right.Y < -sfStickThreshold && PreviousGamePadStates[ iGamePad ].ThumbSticks.Right.Y > -sfStickThreshold;
                        break;
                    case Buttons.RightThumbstickUp:
                        bButtonPressed = GamePadStates[ iGamePad ].ThumbSticks.Right.Y > sfStickThreshold && PreviousGamePadStates[ iGamePad ].ThumbSticks.Right.Y < sfStickThreshold;
                        break;
                    
                    // Default button behavior for the rest
                    default:
                        bButtonPressed = GamePadStates[ iGamePad ].IsButtonDown( _button ) && PreviousGamePadStates[ iGamePad ].IsButtonUp( _button );
                        break;
                }

                if( ! bButtonPressed && _bRepeat )
                {
                    bButtonPressed = ( mabRepeatButtons[ (int)_playerIndex ] && _button == maLastPressedButtons[ (int)_playerIndex ] );
                }
                
#if WINDOWS
                //--------------------------------------------------------------
                // Keyboard controls
                Keys key = GetKeyboardMapping( _button );

                if( _playerIndex == KeyboardPlayerIndex && key != Keys.None && KeyboardState.IsKeyDown( key ) && ! PreviousKeyboardState.IsKeyDown( key ) )
                {
                    bButtonPressed = true;
                }
#endif

                return bButtonPressed;
            }
            else
            {
                return  WasButtonJustPressed( _button, PlayerIndex.One, out _playerIndex, _bRepeat )
                    ||  WasButtonJustPressed( _button, PlayerIndex.Two, out _playerIndex, _bRepeat )
                    ||  WasButtonJustPressed( _button, PlayerIndex.Three, out _playerIndex, _bRepeat )
                    ||  WasButtonJustPressed( _button, PlayerIndex.Four, out _playerIndex, _bRepeat );
            }
        }

        //----------------------------------------------------------------------
        public bool WasButtonJustReleased( Buttons _button, PlayerIndex _controllingPlayer )
        {
            PlayerIndex discardedPlayerIndex;
            return WasButtonJustReleased( _button, _controllingPlayer, out discardedPlayerIndex );
        }

        //----------------------------------------------------------------------
        public bool WasButtonJustReleased( Buttons _button, PlayerIndex? _controllingPlayer, out PlayerIndex _playerIndex )
        {
            if( _controllingPlayer.HasValue )
            {
                _playerIndex = _controllingPlayer.Value;
                int iGamePad = (int)_playerIndex;

                //--------------------------------------------------------------
                bool bButtonPressed;

                switch( _button )
                {
                    // Override for left stick
                    case Buttons.LeftThumbstickLeft:
                        bButtonPressed = PreviousGamePadStates[ iGamePad ].ThumbSticks.Left.X < -sfStickThreshold && GamePadStates[ iGamePad ].ThumbSticks.Left.X > -sfStickThreshold;
                        break;
                    case Buttons.LeftThumbstickRight:
                        bButtonPressed = PreviousGamePadStates[ iGamePad ].ThumbSticks.Left.X > sfStickThreshold && GamePadStates[ iGamePad ].ThumbSticks.Left.X < sfStickThreshold;
                        break;
                    case Buttons.LeftThumbstickDown:
                        bButtonPressed = PreviousGamePadStates[ iGamePad ].ThumbSticks.Left.Y < -sfStickThreshold && GamePadStates[ iGamePad ].ThumbSticks.Left.Y > -sfStickThreshold;
                        break;
                    case Buttons.LeftThumbstickUp:
                        bButtonPressed = PreviousGamePadStates[ iGamePad ].ThumbSticks.Left.Y > sfStickThreshold && GamePadStates[ iGamePad ].ThumbSticks.Left.Y < sfStickThreshold;
                        break;

                    // Override for right stick
                    case Buttons.RightThumbstickLeft:
                        bButtonPressed = PreviousGamePadStates[ iGamePad ].ThumbSticks.Right.X < -sfStickThreshold && GamePadStates[ iGamePad ].ThumbSticks.Right.X > -sfStickThreshold;
                        break;
                    case Buttons.RightThumbstickRight:
                        bButtonPressed = PreviousGamePadStates[ iGamePad ].ThumbSticks.Right.X > sfStickThreshold && GamePadStates[ iGamePad ].ThumbSticks.Right.X < sfStickThreshold;
                        break;
                    case Buttons.RightThumbstickDown:
                        bButtonPressed = PreviousGamePadStates[ iGamePad ].ThumbSticks.Right.Y < -sfStickThreshold && GamePadStates[ iGamePad ].ThumbSticks.Right.Y > -sfStickThreshold;
                        break;
                    case Buttons.RightThumbstickUp:
                        bButtonPressed = PreviousGamePadStates[ iGamePad ].ThumbSticks.Right.Y > sfStickThreshold && GamePadStates[ iGamePad ].ThumbSticks.Right.Y < sfStickThreshold;
                        break;
                    
                    // Default button behavior for the rest
                    default:
                        bButtonPressed = PreviousGamePadStates[ iGamePad ].IsButtonDown( _button ) && GamePadStates[ iGamePad ].IsButtonUp( _button );
                        break;
                }
                
#if WINDOWS
                //--------------------------------------------------------------
                // Keyboard controls
                Keys key = GetKeyboardMapping( _button );

                if( _playerIndex == KeyboardPlayerIndex && key != Keys.None && PreviousKeyboardState.IsKeyDown( key ) && ! KeyboardState.IsKeyDown( key ) )
                {
                    bButtonPressed = true;
                }
#endif

                return bButtonPressed;
            }
            else
            {
                return  WasButtonJustReleased( _button, PlayerIndex.One, out _playerIndex )
                    ||  WasButtonJustReleased( _button, PlayerIndex.Two, out _playerIndex )
                    ||  WasButtonJustReleased( _button, PlayerIndex.Three, out _playerIndex )
                    ||  WasButtonJustReleased( _button, PlayerIndex.Four, out _playerIndex );
            }
        }

        //----------------------------------------------------------------------
        public Keys GetKeyboardMapping( Buttons button )
        {
            switch( button )
            {
                case Buttons.A:
                    return Keys.Space;
                // Keys.Enter is not a good match for the Start button
                /*case Buttons.Start:
                    return Keys.Enter;*/
                case Buttons.Back:
                    return Keys.Escape;
                case Buttons.LeftThumbstickLeft:
                    return Keys.Left;
                case Buttons.LeftThumbstickRight:
                    return Keys.Right;
                case Buttons.LeftThumbstickUp:
                    return Keys.Up;
                case Buttons.LeftThumbstickDown:
                    return Keys.Down;
            }

            return Keys.None;
        }

        //----------------------------------------------------------------------
        Point                   mSavedMousePosition;
        public bool             IsMouseCaptured     { get; private set; }

        //----------------------------------------------------------------------
        public void CaptureMouse()
        {
            mSavedMousePosition = new Point( Mouse.GetState().X, Mouse.GetState().Y );
            Game.IsMouseVisible = false;
            Point mouseCenter = new Point( Game.Window.ClientBounds.Width / 2, Game.Window.ClientBounds.Height / 2);
            Mouse.SetPosition( mouseCenter.X, mouseCenter.Y );

            MouseState = Mouse.GetState();
            PreviousMouseState = MouseState;

            IsMouseCaptured = true;
        }

        //----------------------------------------------------------------------
        public void ReleaseMouse()
        {
            Debug.Assert( IsMouseCaptured );

            IsMouseCaptured = false;

            Game.IsMouseVisible = true;
            Mouse.SetPosition( mSavedMousePosition.X, mSavedMousePosition.Y );

            MouseState = Mouse.GetState();
            PreviousMouseState = MouseState;
        }
    }
}
