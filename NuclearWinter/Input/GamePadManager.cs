using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace NuclearWinter.Input
{
    public class GamePadManager: GameComponent
    {
        public const int                    siMaxInput              = 4;
        public const float                  sfStickThreshold        = 0.4f;

        public readonly GamePadState[]      GamePadStates;
        public readonly GamePadState[]      PreviousGamePadStates;

#if WINDOWS
        public MouseState                   MouseState;
        public MouseState                   PreviousMouseState;

        public LocalizedKeyboardState       KeyboardState;
        public LocalizedKeyboardState       PreviousKeyboardState;
#endif

        Buttons[]                           maLastPressedButtons;
        float[]                             mafRepeatTimers;
        bool[]                              mabRepeatButtons;
        const float                         sfButtonRepeatDelay     = 0.3f;
        const float                         sfButtonRepeatInterval  = 0.1f;

        List<Buttons>                       lButtons;

        //---------------------------------------------------------------------
        public GamePadManager( Game _game )
        : base ( _game )
        {
            GamePadStates           = new GamePadState[ siMaxInput ];
            PreviousGamePadStates   = new GamePadState[ siMaxInput ];

            maLastPressedButtons    = new Buttons[ siMaxInput ];
            mafRepeatTimers         = new float[ siMaxInput ];
            mabRepeatButtons        = new bool[ siMaxInput ];

            lButtons                = Utils.GetValues<Buttons>();
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

                if( key != Keys.None && KeyboardState.IsKeyDown( key ) && ! PreviousKeyboardState.IsKeyDown( key ) )
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
        public Keys GetKeyboardMapping( Buttons button )
        {
            switch( button )
            {
                case Buttons.A:
                    return Keys.Space;
                case Buttons.Start:
                    return Keys.Enter;
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
