﻿using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using System.Linq;
using System.Diagnostics;

#if !FNA
using OSKey = System.Windows.Forms.Keys;
#endif

namespace NuclearWinter.Input
{
    [Flags]
    public enum ShortcutKey
    {
        LeftCtrl = 1 << 0,
        RightCtrl = 1 << 1,

        LeftAlt = 1 << 2,
        RightAlt = 1 << 3,

        LeftWindows = 1 << 4,
        RightWindows = 1 << 5,
    }

    public class InputManager : GameComponent
    {
        public const int siMaxInput = 4;
        public const float sfStickThreshold = 0.4f;

        public readonly GamePadState[] GamePadStates;
        public readonly GamePadState[] PreviousGamePadStates;

        public MouseState MouseState { get; private set; }
        public MouseState PreviousMouseState { get; private set; }

        public int PrimaryMouseButton
        {
#if !FNA
            get { return System.Windows.Forms.SystemInformation.MouseButtonsSwapped ? 2 : 0; }
#else
            get { return 0; }
#endif
        }

        public int SecondaryMouseButton
        {
#if !FNA
            get { return System.Windows.Forms.SystemInformation.MouseButtonsSwapped ? 0 : 2; }
#else
            get { return 2; }
#endif
        }

        public LocalizedKeyboardState KeyboardState { get; private set; }
        public LocalizedKeyboardState PreviousKeyboardState { get; private set; }
        public PlayerIndex? KeyboardPlayerIndex { get; private set; }
        /*Keys                                        mLastKeyPressed;
        bool                                        mbRepeatKey;
        float                                       mfRepeatKeyTimer;*/
        public ShortcutKey ActiveShortcutKey;

        public List<char> EnteredText { get; private set; }
        public List<Keys> JustPressedKeys { get; private set; }

        public List<OSKey> JustPressedOSKeys { get; private set; }
        public List<OSKey> JustReleasedOSKeys { get; private set; }
#if !FNA
        WindowMessageFilter mMessageFilter;
#else
        float                                       mfTimeSinceLastClick;
        Point                                       mLastPrimaryClickPosition;
#endif
        bool mbDoubleClicked;
        bool mbDoubleClicking;

        Buttons[] maLastPressedButtons;
        float[] mafRepeatTimers;
        bool[] mabRepeatButtons;
        const float sfButtonRepeatDelay = 0.3f;
        const float sfButtonRepeatInterval = 0.1f;

        List<Buttons> lButtons;

        //---------------------------------------------------------------------
        public InputManager(Game game)
        : base(game)
        {
            GamePadStates = new GamePadState[siMaxInput];
            PreviousGamePadStates = new GamePadState[siMaxInput];

            maLastPressedButtons = new Buttons[siMaxInput];
            mafRepeatTimers = new float[siMaxInput];
            mabRepeatButtons = new bool[siMaxInput];

            lButtons = Utils.GetValues<Buttons>();


            ActiveShortcutKey = ShortcutKey.LeftCtrl | ShortcutKey.RightCtrl;
#if FNA
            if( SDL2.SDL.SDL_GetPlatform() == "Mac OS X" )
            {
                ActiveShortcutKey = ShortcutKey.LeftWindows | ShortcutKey.RightWindows;
            }
#endif

            EnteredText = new List<char>();
            JustPressedKeys = new List<Keys>();

            JustPressedOSKeys = new List<OSKey>();
            JustReleasedOSKeys = new List<OSKey>();

#if !FNA
            mMessageFilter = new WindowMessageFilter(Game.Window.Handle);
            mMessageFilter.CharacterHandler = delegate (char _char) { EnteredText.Add(_char); };
            mMessageFilter.KeyDownHandler = delegate (System.Windows.Forms.Keys _key) { JustPressedOSKeys.Add(_key); };
            mMessageFilter.KeyUpHandler = delegate (System.Windows.Forms.Keys _key) { JustReleasedOSKeys.Add(_key); };
            mMessageFilter.DoubleClickHandler = delegate { mbDoubleClicked = true; };
#else
            TextInputEXT.TextInput              += delegate( char _char ) { EnteredText.Add( _char ); };
            TextInputEXT.KeyDown                += delegate( SDL2.SDL.SDL_Keycode _key ) { JustPressedOSKeys.Add( (OSKey)_key ); };
            TextInputEXT.KeyUp                  += delegate( SDL2.SDL.SDL_Keycode _key ) { JustReleasedOSKeys.Add( (OSKey)_key ); };
#endif

            MouseState = Mouse.GetState();
            PreviousMouseState = MouseState;
        }

        //---------------------------------------------------------------------
        public override void Update(GameTime time)
        {
            float fElapsedTime = (float)time.ElapsedGameTime.TotalSeconds;

            for (int i = 0; i < siMaxInput; i++)
            {
                PreviousGamePadStates[i] = GamePadStates[i];
                GamePadStates[i] = GamePad.GetState((PlayerIndex)i);
            }

            if (!IsMouseCaptured)
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
                    MouseState.XButton2);
            }

            MouseState = Mouse.GetState();

            if (IsMouseCaptured)
            {
#if ! FNA
                Mouse.SetPosition(PreviousMouseState.X, PreviousMouseState.Y);
#else
                int x, y;
                SDL2.SDL.SDL_GetRelativeMouseState(out x, out y);

                MouseState = new MouseState(
                    x, y,
                    MouseState.ScrollWheelValue,
                    MouseState.LeftButton,
                    MouseState.MiddleButton,
                    MouseState.RightButton,
                    MouseState.XButton1,
                    MouseState.XButton2 );
#endif
            }

            PreviousKeyboardState = KeyboardState;
            KeyboardState = new LocalizedKeyboardState(Keyboard.GetState());

            KeyboardPlayerIndex = null;
            for (int i = 0; i < siMaxInput; i++)
            {
                if (!GamePadStates[i].IsConnected)
                {
                    KeyboardPlayerIndex = (PlayerIndex)i;
                    break;
                }
            }

            EnteredText.Clear();
            JustPressedOSKeys.Clear();
            JustReleasedOSKeys.Clear();

            Keys[] currentPressedKeys = KeyboardState.Native.GetPressedKeys();
            Keys[] previousPressedKeys = PreviousKeyboardState.Native.GetPressedKeys();

            JustPressedKeys = currentPressedKeys.Except(previousPressedKeys).ToList();

            mbDoubleClicked = false;

#if FNA
            if( WasMouseButtonJustPressed( PrimaryMouseButton ) )
            {
				// Can't use System.Windows.Forms on Mac so let's just hard-code some reasonable values
				float fDoubleClickTime = 0.5f; 	// System.Windows.Forms.SystemInformation.DoubleClickTime / 1000f;
				int iDoubleClickWidth = 4; 		// System.Windows.Forms.SystemInformation.DoubleClickSize.Width;
				int iDoubleClickHeight = 4; 	// System.Windows.Forms.SystemInformation.DoubleClickSize.Height;

                if( mfTimeSinceLastClick <= fDoubleClickTime )
                {
                    if( Math.Abs( MouseState.X - mLastPrimaryClickPosition.X ) <= iDoubleClickWidth
                       && Math.Abs( MouseState.Y - mLastPrimaryClickPosition.Y ) <= iDoubleClickHeight )
                    {
                        mbDoubleClicked = true;
                        // Suppress mouse down / up like the Windows event loop does
                        mbDoubleClicking = true;
                    }
                }

                mLastPrimaryClickPosition = new Point( MouseState.X, MouseState.Y );
                mfTimeSinceLastClick = 0f;
            }
            else
            {
                if( mbDoubleClicking && ! IsMouseButtonDown(PrimaryMouseButton) && ! WasMouseButtonDown(PrimaryMouseButton) )
                {
                    mbDoubleClicking = false;
                }

                mfTimeSinceLastClick += fElapsedTime;
            }
#endif

            for (int iGamePad = 0; iGamePad < siMaxInput; iGamePad++)
            {
                mabRepeatButtons[iGamePad] = false;

                foreach (Buttons button in lButtons)
                {
                    bool bButtonPressed;

                    switch (button)
                    {
                        // Override for left stick
                        case Buttons.LeftThumbstickLeft:
                            bButtonPressed = GamePadStates[iGamePad].ThumbSticks.Left.X < -sfStickThreshold && PreviousGamePadStates[iGamePad].ThumbSticks.Left.X > -sfStickThreshold;
                            break;
                        case Buttons.LeftThumbstickRight:
                            bButtonPressed = GamePadStates[iGamePad].ThumbSticks.Left.X > sfStickThreshold && PreviousGamePadStates[iGamePad].ThumbSticks.Left.X < sfStickThreshold;
                            break;
                        case Buttons.LeftThumbstickDown:
                            bButtonPressed = GamePadStates[iGamePad].ThumbSticks.Left.Y < -sfStickThreshold && PreviousGamePadStates[iGamePad].ThumbSticks.Left.Y > -sfStickThreshold;
                            break;
                        case Buttons.LeftThumbstickUp:
                            bButtonPressed = GamePadStates[iGamePad].ThumbSticks.Left.Y > sfStickThreshold && PreviousGamePadStates[iGamePad].ThumbSticks.Left.Y < sfStickThreshold;
                            break;

                        // Override for right stick
                        case Buttons.RightThumbstickLeft:
                            bButtonPressed = GamePadStates[iGamePad].ThumbSticks.Right.X < -sfStickThreshold && PreviousGamePadStates[iGamePad].ThumbSticks.Right.X > -sfStickThreshold;
                            break;
                        case Buttons.RightThumbstickRight:
                            bButtonPressed = GamePadStates[iGamePad].ThumbSticks.Right.X > sfStickThreshold && PreviousGamePadStates[iGamePad].ThumbSticks.Right.X < sfStickThreshold;
                            break;
                        case Buttons.RightThumbstickDown:
                            bButtonPressed = GamePadStates[iGamePad].ThumbSticks.Right.Y < -sfStickThreshold && PreviousGamePadStates[iGamePad].ThumbSticks.Right.Y > -sfStickThreshold;
                            break;
                        case Buttons.RightThumbstickUp:
                            bButtonPressed = GamePadStates[iGamePad].ThumbSticks.Right.Y > sfStickThreshold && PreviousGamePadStates[iGamePad].ThumbSticks.Right.Y < sfStickThreshold;
                            break;

                        // Default button behavior for the rest
                        default:
                            bButtonPressed = GamePadStates[iGamePad].IsButtonDown(button) && PreviousGamePadStates[iGamePad].IsButtonUp(button);
                            break;
                    }

                    if (!bButtonPressed)
                    {
                        Keys key = GetKeyboardMapping(button);
                        bButtonPressed = key != Keys.None && KeyboardState.IsKeyDown(key) && !PreviousKeyboardState.IsKeyDown(key);
                    }

                    if (bButtonPressed)
                    {
                        mafRepeatTimers[iGamePad] = 0f;
                        maLastPressedButtons[iGamePad] = button;
                        break;
                    }
                }

                if (maLastPressedButtons[iGamePad] != 0)
                {
                    bool bIsButtonStillDown = GamePadStates[iGamePad].IsButtonDown(maLastPressedButtons[iGamePad]);

                    if (!bIsButtonStillDown)
                    {
                        Keys key = GetKeyboardMapping(maLastPressedButtons[iGamePad]);
                        bIsButtonStillDown = key != Keys.None && KeyboardState.IsKeyDown(key);
                    }

                    if (bIsButtonStillDown)
                    {
                        float fRepeatValue = (mafRepeatTimers[iGamePad] - sfButtonRepeatDelay) % (sfButtonRepeatInterval);
                        float fNewRepeatValue = (mafRepeatTimers[iGamePad] + fElapsedTime - sfButtonRepeatDelay) % (sfButtonRepeatInterval);

                        if (mafRepeatTimers[iGamePad] < sfButtonRepeatDelay && mafRepeatTimers[iGamePad] + fElapsedTime >= sfButtonRepeatDelay)
                        {
                            mabRepeatButtons[iGamePad] = true;
                        }
                        else
                        if (mafRepeatTimers[iGamePad] > sfButtonRepeatDelay && fRepeatValue > fNewRepeatValue)
                        {
                            mabRepeatButtons[iGamePad] = true;
                        }

                        mafRepeatTimers[iGamePad] += fElapsedTime;
                    }
                    else
                    {
                        mafRepeatTimers[iGamePad] = 0f;
                        maLastPressedButtons[iGamePad] = 0;
                    }
                }
            }


        }

        //----------------------------------------------------------------------
        public bool IsMouseButtonDown(int button)
        {
            if (mbDoubleClicking) return false;

            switch (button)
            {
                case 0:
                    return MouseState.LeftButton == ButtonState.Pressed;
                case 1:
                    return MouseState.MiddleButton == ButtonState.Pressed;
                case 2:
                    return MouseState.RightButton == ButtonState.Pressed;
                case 3:
                    return MouseState.XButton1 == ButtonState.Pressed;
                case 4:
                    return MouseState.XButton2 == ButtonState.Pressed;
                case 5:
                    return (MouseState.ScrollWheelValue - PreviousMouseState.ScrollWheelValue) > 0;
                case 6:
                    return (MouseState.ScrollWheelValue - PreviousMouseState.ScrollWheelValue) < 0;
                default:
                    return false;
            }
        }


        //----------------------------------------------------------------------
        bool WasMouseButtonDown(int button)
        {
            if (mbDoubleClicking) return false;

            switch (button)
            {
                case 0:
                    return PreviousMouseState.LeftButton == ButtonState.Pressed;
                case 1:
                    return PreviousMouseState.MiddleButton == ButtonState.Pressed;
                case 2:
                    return PreviousMouseState.RightButton == ButtonState.Pressed;
                case 3:
                    return PreviousMouseState.XButton1 == ButtonState.Pressed;
                case 4:
                    return PreviousMouseState.XButton2 == ButtonState.Pressed;
                default:
                    return false;
            }
        }

        //----------------------------------------------------------------------
        public bool WasMouseButtonJustPressed(int button)
        {
            if (mbDoubleClicking) return false;

            switch (button)
            {
                case 0:
                    return MouseState.LeftButton == ButtonState.Pressed && PreviousMouseState.LeftButton == ButtonState.Released;
                case 1:
                    return MouseState.MiddleButton == ButtonState.Pressed && PreviousMouseState.MiddleButton == ButtonState.Released;
                case 2:
                    return MouseState.RightButton == ButtonState.Pressed && PreviousMouseState.RightButton == ButtonState.Released;
                case 3:
                    return MouseState.XButton1 == ButtonState.Pressed && PreviousMouseState.XButton1 == ButtonState.Released;
                case 4:
                    return MouseState.XButton2 == ButtonState.Pressed && PreviousMouseState.XButton2 == ButtonState.Released;
                case 5:
                    return (MouseState.ScrollWheelValue - PreviousMouseState.ScrollWheelValue) > 0;
                case 6:
                    return (MouseState.ScrollWheelValue - PreviousMouseState.ScrollWheelValue) < 0;
                default:
                    return false;
            }
        }

        //----------------------------------------------------------------------
        public bool WasMouseButtonJustReleased(int button, bool ignoreDoubleClick = false)
        {
            if (!ignoreDoubleClick && mbDoubleClicking) return false;

            switch (button)
            {
                case 0:
                    return MouseState.LeftButton == ButtonState.Released && PreviousMouseState.LeftButton == ButtonState.Pressed;
                case 1:
                    return MouseState.MiddleButton == ButtonState.Released && PreviousMouseState.MiddleButton == ButtonState.Pressed;
                case 2:
                    return MouseState.RightButton == ButtonState.Released && PreviousMouseState.RightButton == ButtonState.Pressed;
                case 3:
                    return MouseState.XButton1 == ButtonState.Released && PreviousMouseState.XButton1 == ButtonState.Pressed;
                case 4:
                    return MouseState.XButton2 == ButtonState.Released && PreviousMouseState.XButton2 == ButtonState.Pressed;
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
        public bool IsShortcutKeyDown()
        {
            return ((ActiveShortcutKey & ShortcutKey.LeftCtrl) == ShortcutKey.LeftCtrl && KeyboardState.Native.IsKeyDown(Keys.LeftControl) && !KeyboardState.Native.IsKeyDown(Keys.RightAlt))
                || ((ActiveShortcutKey & ShortcutKey.RightCtrl) == ShortcutKey.RightCtrl && KeyboardState.Native.IsKeyDown(Keys.RightControl))
                || ((ActiveShortcutKey & ShortcutKey.LeftAlt) == ShortcutKey.LeftAlt && KeyboardState.Native.IsKeyDown(Keys.LeftAlt))
                || ((ActiveShortcutKey & ShortcutKey.RightAlt) == ShortcutKey.RightAlt && KeyboardState.Native.IsKeyDown(Keys.RightAlt))
                || ((ActiveShortcutKey & ShortcutKey.LeftWindows) == ShortcutKey.LeftWindows && KeyboardState.Native.IsKeyDown(Keys.LeftWindows))
                || ((ActiveShortcutKey & ShortcutKey.RightWindows) == ShortcutKey.RightWindows && KeyboardState.Native.IsKeyDown(Keys.RightWindows));
        }

        //----------------------------------------------------------------------
        public bool WasKeyJustPressed(Keys key, bool native = false)
        {
            if (!native)
            {
                return KeyboardState.IsKeyDown(key) && !PreviousKeyboardState.IsKeyDown(key);
            }
            else
            {
                return KeyboardState.Native.IsKeyDown(key) && !PreviousKeyboardState.Native.IsKeyDown(key);
            }
        }

        //----------------------------------------------------------------------
        public bool WasKeyJustReleased(Keys key, bool native)
        {
            if (!native)
            {
                return KeyboardState.IsKeyUp(key) && !PreviousKeyboardState.IsKeyUp(key);
            }
            else
            {
                return KeyboardState.Native.IsKeyUp(key) && !PreviousKeyboardState.Native.IsKeyUp(key);
            }
        }


        //----------------------------------------------------------------------
        public int GetMouseWheelDelta()
        {
            return MouseState.ScrollWheelValue - PreviousMouseState.ScrollWheelValue;
        }

        //----------------------------------------------------------------------
        public bool WasButtonJustPressed(Buttons button, PlayerIndex controllingPlayer)
        {
            return WasButtonJustPressed(button, controllingPlayer, false);
        }

        //----------------------------------------------------------------------
        public bool WasButtonJustPressed(Buttons button, PlayerIndex controllingPlayer, bool repeat)
        {
            PlayerIndex discardedPlayerIndex;
            return WasButtonJustPressed(button, controllingPlayer, out discardedPlayerIndex, repeat);
        }

        //----------------------------------------------------------------------
        public bool WasButtonJustPressed(Buttons button, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
        {
            return WasButtonJustPressed(button, controllingPlayer, out playerIndex, false);
        }

        //----------------------------------------------------------------------
        public bool WasButtonJustPressed(Buttons button, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex, bool repeat)
        {
            if (controllingPlayer.HasValue)
            {
                playerIndex = controllingPlayer.Value;
                int iGamePad = (int)playerIndex;

                //--------------------------------------------------------------
                bool bButtonPressed;

                switch (button)
                {
                    // Override for left stick
                    case Buttons.LeftThumbstickLeft:
                        bButtonPressed = GamePadStates[iGamePad].ThumbSticks.Left.X < -sfStickThreshold && PreviousGamePadStates[iGamePad].ThumbSticks.Left.X > -sfStickThreshold;
                        break;
                    case Buttons.LeftThumbstickRight:
                        bButtonPressed = GamePadStates[iGamePad].ThumbSticks.Left.X > sfStickThreshold && PreviousGamePadStates[iGamePad].ThumbSticks.Left.X < sfStickThreshold;
                        break;
                    case Buttons.LeftThumbstickDown:
                        bButtonPressed = GamePadStates[iGamePad].ThumbSticks.Left.Y < -sfStickThreshold && PreviousGamePadStates[iGamePad].ThumbSticks.Left.Y > -sfStickThreshold;
                        break;
                    case Buttons.LeftThumbstickUp:
                        bButtonPressed = GamePadStates[iGamePad].ThumbSticks.Left.Y > sfStickThreshold && PreviousGamePadStates[iGamePad].ThumbSticks.Left.Y < sfStickThreshold;
                        break;

                    // Override for right stick
                    case Buttons.RightThumbstickLeft:
                        bButtonPressed = GamePadStates[iGamePad].ThumbSticks.Right.X < -sfStickThreshold && PreviousGamePadStates[iGamePad].ThumbSticks.Right.X > -sfStickThreshold;
                        break;
                    case Buttons.RightThumbstickRight:
                        bButtonPressed = GamePadStates[iGamePad].ThumbSticks.Right.X > sfStickThreshold && PreviousGamePadStates[iGamePad].ThumbSticks.Right.X < sfStickThreshold;
                        break;
                    case Buttons.RightThumbstickDown:
                        bButtonPressed = GamePadStates[iGamePad].ThumbSticks.Right.Y < -sfStickThreshold && PreviousGamePadStates[iGamePad].ThumbSticks.Right.Y > -sfStickThreshold;
                        break;
                    case Buttons.RightThumbstickUp:
                        bButtonPressed = GamePadStates[iGamePad].ThumbSticks.Right.Y > sfStickThreshold && PreviousGamePadStates[iGamePad].ThumbSticks.Right.Y < sfStickThreshold;
                        break;

                    // Default button behavior for the rest
                    default:
                        bButtonPressed = GamePadStates[iGamePad].IsButtonDown(button) && PreviousGamePadStates[iGamePad].IsButtonUp(button);
                        break;
                }

                if (!bButtonPressed && repeat)
                {
                    bButtonPressed = (mabRepeatButtons[(int)playerIndex] && button == maLastPressedButtons[(int)playerIndex]);
                }

                //--------------------------------------------------------------
                // Keyboard controls
                Keys key = GetKeyboardMapping(button);

                if (playerIndex == KeyboardPlayerIndex && key != Keys.None && KeyboardState.IsKeyDown(key) && !PreviousKeyboardState.IsKeyDown(key))
                {
                    bButtonPressed = true;
                }

                return bButtonPressed;
            }
            else
            {
                return WasButtonJustPressed(button, PlayerIndex.One, out playerIndex, repeat)
                    || WasButtonJustPressed(button, PlayerIndex.Two, out playerIndex, repeat)
                    || WasButtonJustPressed(button, PlayerIndex.Three, out playerIndex, repeat)
                    || WasButtonJustPressed(button, PlayerIndex.Four, out playerIndex, repeat);
            }
        }

        //----------------------------------------------------------------------
        public bool WasButtonJustReleased(Buttons button, PlayerIndex controllingPlayer)
        {
            PlayerIndex discardedPlayerIndex;
            return WasButtonJustReleased(button, controllingPlayer, out discardedPlayerIndex);
        }

        //----------------------------------------------------------------------
        public bool WasButtonJustReleased(Buttons button, PlayerIndex? controllingPlayer, out PlayerIndex playerIndex)
        {
            if (controllingPlayer.HasValue)
            {
                playerIndex = controllingPlayer.Value;
                int iGamePad = (int)playerIndex;

                //--------------------------------------------------------------
                bool bButtonPressed;

                switch (button)
                {
                    // Override for left stick
                    case Buttons.LeftThumbstickLeft:
                        bButtonPressed = PreviousGamePadStates[iGamePad].ThumbSticks.Left.X < -sfStickThreshold && GamePadStates[iGamePad].ThumbSticks.Left.X > -sfStickThreshold;
                        break;
                    case Buttons.LeftThumbstickRight:
                        bButtonPressed = PreviousGamePadStates[iGamePad].ThumbSticks.Left.X > sfStickThreshold && GamePadStates[iGamePad].ThumbSticks.Left.X < sfStickThreshold;
                        break;
                    case Buttons.LeftThumbstickDown:
                        bButtonPressed = PreviousGamePadStates[iGamePad].ThumbSticks.Left.Y < -sfStickThreshold && GamePadStates[iGamePad].ThumbSticks.Left.Y > -sfStickThreshold;
                        break;
                    case Buttons.LeftThumbstickUp:
                        bButtonPressed = PreviousGamePadStates[iGamePad].ThumbSticks.Left.Y > sfStickThreshold && GamePadStates[iGamePad].ThumbSticks.Left.Y < sfStickThreshold;
                        break;

                    // Override for right stick
                    case Buttons.RightThumbstickLeft:
                        bButtonPressed = PreviousGamePadStates[iGamePad].ThumbSticks.Right.X < -sfStickThreshold && GamePadStates[iGamePad].ThumbSticks.Right.X > -sfStickThreshold;
                        break;
                    case Buttons.RightThumbstickRight:
                        bButtonPressed = PreviousGamePadStates[iGamePad].ThumbSticks.Right.X > sfStickThreshold && GamePadStates[iGamePad].ThumbSticks.Right.X < sfStickThreshold;
                        break;
                    case Buttons.RightThumbstickDown:
                        bButtonPressed = PreviousGamePadStates[iGamePad].ThumbSticks.Right.Y < -sfStickThreshold && GamePadStates[iGamePad].ThumbSticks.Right.Y > -sfStickThreshold;
                        break;
                    case Buttons.RightThumbstickUp:
                        bButtonPressed = PreviousGamePadStates[iGamePad].ThumbSticks.Right.Y > sfStickThreshold && GamePadStates[iGamePad].ThumbSticks.Right.Y < sfStickThreshold;
                        break;

                    // Default button behavior for the rest
                    default:
                        bButtonPressed = PreviousGamePadStates[iGamePad].IsButtonDown(button) && GamePadStates[iGamePad].IsButtonUp(button);
                        break;
                }

                //--------------------------------------------------------------
                // Keyboard controls
                Keys key = GetKeyboardMapping(button);

                if (playerIndex == KeyboardPlayerIndex && key != Keys.None && PreviousKeyboardState.IsKeyDown(key) && !KeyboardState.IsKeyDown(key))
                {
                    bButtonPressed = true;
                }

                return bButtonPressed;
            }
            else
            {
                return WasButtonJustReleased(button, PlayerIndex.One, out playerIndex)
                    || WasButtonJustReleased(button, PlayerIndex.Two, out playerIndex)
                    || WasButtonJustReleased(button, PlayerIndex.Three, out playerIndex)
                    || WasButtonJustReleased(button, PlayerIndex.Four, out playerIndex);
            }
        }

        //----------------------------------------------------------------------
        public Keys GetKeyboardMapping(Buttons button)
        {
            switch (button)
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
        Point mSavedMousePosition;
        public bool IsMouseCaptured { get; private set; }

        //----------------------------------------------------------------------
        public void CaptureMouse()
        {
            var savedMouseState = Mouse.GetState();
            mSavedMousePosition = new Point(savedMouseState.X, savedMouseState.Y);

#if ! FNA
            Game.IsMouseVisible = false;
            Point mouseCenter = new Point(Game.Window.ClientBounds.Width / 2, Game.Window.ClientBounds.Height / 2);
            Mouse.SetPosition(mouseCenter.X, mouseCenter.Y);

            MouseState = Mouse.GetState();
            PreviousMouseState = MouseState;
#else
            SDL2.SDL.SDL_SetRelativeMouseMode(SDL2.SDL.SDL_bool.SDL_TRUE);
            int x, y;
            SDL2.SDL.SDL_GetRelativeMouseState(out x, out y);

            PreviousMouseState = new MouseState(
                0, 0, 
                PreviousMouseState.ScrollWheelValue,
                PreviousMouseState.LeftButton,
                PreviousMouseState.MiddleButton,
                PreviousMouseState.RightButton,
                PreviousMouseState.XButton1,
                PreviousMouseState.XButton2 );

            MouseState = new MouseState(
                0, 0, 
                MouseState.ScrollWheelValue,
                MouseState.LeftButton,
                MouseState.MiddleButton,
                MouseState.RightButton,
                MouseState.XButton1,
                MouseState.XButton2 );
#endif


            IsMouseCaptured = true;
        }

        //----------------------------------------------------------------------
        public void ReleaseMouse()
        {
            Debug.Assert(IsMouseCaptured);

#if FNA
            SDL2.SDL.SDL_SetRelativeMouseMode(SDL2.SDL.SDL_bool.SDL_FALSE);
#endif

            IsMouseCaptured = false;

            Game.IsMouseVisible = true;
            Mouse.SetPosition(mSavedMousePosition.X, mSavedMousePosition.Y);

            MouseState = Mouse.GetState();
            PreviousMouseState = MouseState;
        }
    }
}
