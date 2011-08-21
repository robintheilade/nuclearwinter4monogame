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

        public LocalizedKeyboardState               KeyboardState           { get; private set; }
        public LocalizedKeyboardState               PreviousKeyboardState   { get; private set; }
        public PlayerIndex?                         KeyboardPlayerIndex     { get; private set; }
        /*Keys                                        mLastKeyPressed;
        bool                                        mbRepeatKey;
        float                                       mfRepeatKeyTimer;*/

        public List<char>                           EnteredText             { get; private set; }
        public List<Keys>                           JustPressedKeys         { get; private set; }
        TextInput                                   mTextInput;
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

            mTextInput              = new TextInput( Game.Window.Handle );
            mTextInput.CharacterHandler = delegate( char _char ) { EnteredText.Add( _char ); };
            mTextInput.KeyDownHandler   = delegate( System.Windows.Forms.Keys _key ) { JustPressedKeys.Add( (Keys)_key ); };

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
            PreviousMouseState = MouseState;
            MouseState = Mouse.GetState();

            PreviousKeyboardState = KeyboardState;
            KeyboardState = new LocalizedKeyboardState( Keyboard.GetState() );

            /*
            Keys[] pressedKeys = KeyboardState.Native.GetPressedKeys();

            mbRepeatKey = false;
            bool bIsKeyStillDown = false;
            if( pressedKeys.Length > 0 )
            {
                if( pressedKeys[0] != mLastKeyPressed )
                {
                    mLastKeyPressed = pressedKeys[0];
                    mfRepeatKeyTimer = 0f;
                }
                else
                {
                    bIsKeyStillDown = true;
                }
            }
            else
            {
                mLastKeyPressed = Keys.None;
            }

            if( bIsKeyStillDown )
            {
                float fRepeatValue      = ( mfRepeatKeyTimer - sfButtonRepeatDelay ) % ( sfButtonRepeatInterval );
                float fNewRepeatValue   = ( mfRepeatKeyTimer + fElapsedTime - sfButtonRepeatDelay ) % ( sfButtonRepeatInterval );

                if( mfRepeatKeyTimer < sfButtonRepeatDelay && mfRepeatKeyTimer + fElapsedTime >= sfButtonRepeatDelay )
                {
                    mbRepeatKey = true;
                }
                else
                if( mfRepeatKeyTimer > sfButtonRepeatDelay && fRepeatValue > fNewRepeatValue )
                {
                    mbRepeatKey = true;
                }

                mfRepeatKeyTimer += fElapsedTime;
            }
            */

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
            JustPressedKeys.Clear();
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
        public bool WasKeyJustPressed( Keys _key )
        {
            return KeyboardState.IsKeyDown(_key) && ! PreviousKeyboardState.IsKeyDown(_key);
        }

        //----------------------------------------------------------------------
        public bool WasKeyJustReleased( Keys _key )
        {
            return KeyboardState.IsKeyUp(_key) && ! PreviousKeyboardState.IsKeyUp(_key);
        }

        /*
        public IEnumerable<Keys> GetJustPressedKeys()
        {

            Keys[] currentPressedKeys = KeyboardState.Native.GetPressedKeys();
            Keys[] previousPressedKeys = PreviousKeyboardState.Native.GetPressedKeys();
            
            List<Keys> keys = currentPressedKeys.Except( previousPressedKeys ).ToList();

            if( mbRepeatKey )
            {
                Debug.Assert( mLastKeyPressed != Keys.None );

                if( ! keys.Contains( mLastKeyPressed ) )
                {
                    keys.Add( mLastKeyPressed );
                }
            }

            return keys;
        }
        */

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


    }
}
