using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
#if GAMERSERVICES
using Microsoft.Xna.Framework.GamerServices;
#endif
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace NuclearWinter
{
    /// <summary>
    /// Describes a screen mode
    /// </summary>
    public struct ScreenMode
    {
        //----------------------------------------------------------------------
        public ScreenMode( int _iWidth, int _iHeight, bool _bFullscreen )
        {
            Width           = _iWidth;
            Height          = _iHeight;
            Fullscreen      = _bFullscreen;
        }

        //----------------------------------------------------------------------
        public override string ToString()
        {
            return Width.ToString() + "x" + Height.ToString();
        }

        //----------------------------------------------------------------------
        public int      Width;            ///< Width
        public int      Height;           ///< Height
        public bool     Fullscreen;       ///< Is the Mode fullscreen?
    }

    //--------------------------------------------------------------------------
    /// <summary>
    /// Static class to manage game resolution and scaling
    /// </summary>
    public sealed class Resolution
    {
        static Resolution()
        {
            InternalMode = new ScreenMode( 1920, 1080, true );

            SortedScreenModes = new List<ScreenMode>();
            SortedScreenModes.Add( new ScreenMode( 800, 450, true ) );
            SortedScreenModes.Add( new ScreenMode( 1024, 768, true ) );
            SortedScreenModes.Add( new ScreenMode( 1280, 720, true ) );
            SortedScreenModes.Add( new ScreenMode( 1280, 800, true ) );
            SortedScreenModes.Add( new ScreenMode( 1440, 900, true ) );
            SortedScreenModes.Add( new ScreenMode( 1680, 1050, true ) );
            SortedScreenModes.Add( new ScreenMode( 1920, 1080, true ) );
            SortedScreenModes.Add( new ScreenMode( 1920, 1200, true ) );
        }

        //----------------------------------------------------------------------
        // Initialize the best Resolution available
        public static int Initialize( GraphicsDeviceManager _graphics )
        {
            List<ScreenMode> lReversedScreenModes = new List<ScreenMode>( SortedScreenModes );
            lReversedScreenModes.Reverse();

            int i = 0;
            foreach( ScreenMode mode in lReversedScreenModes )
            {
                if( SetScreenMode( _graphics, mode ) )
                {
                    return lReversedScreenModes.Count - ( i + 1 );
                }
                i++;
            }

            return 0;
        }

        //----------------------------------------------------------------------
        // Initialize the Resolution using the specified ScreenMode
        public static void Initialize( GraphicsDeviceManager _graphics, ScreenMode _mode )
        {
            InternalMode = new ScreenMode( 1920, 1080, true );
            SetScreenMode( _graphics, _mode );
        }

        //----------------------------------------------------------------------
        // Set the specified screen mode
        public static bool SetScreenMode( GraphicsDeviceManager _graphics, ScreenMode _mode )
        {
            if( _mode.Fullscreen )
            {
                // Fullscreen
                foreach( DisplayMode displayMode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes )
                {
                    if( (_mode.Width == displayMode.Width)
                    &&  (_mode.Height == displayMode.Height) )
                    {
                        ApplyScreenMode( _graphics, _mode );
                        return true;
                    }
                }
            }
            else
            {
                if( (_mode.Width <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width)
                &&  (_mode.Height <= GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height) )
                {
                    ApplyScreenMode( _graphics, _mode );
                    return true;
                }
            }

            return false;
        }

        //----------------------------------------------------------------------
        // Apply the specified ScreenMode
        private static void ApplyScreenMode( GraphicsDeviceManager _graphics, ScreenMode _mode )
        {
            Mode = _mode;
            _graphics.PreferredBackBufferWidth  = Mode.Width;
            _graphics.PreferredBackBufferHeight = Mode.Height;
            _graphics.PreferMultiSampling       = true;
            _graphics.IsFullScreen              = Mode.Fullscreen;
            _graphics.ApplyChanges();

            Scale = Matrix.CreateScale( (float)( Mode.Width * 9 / 16 ) / (float)InternalMode.Height );

            DefaultViewport = _graphics.GraphicsDevice.Viewport;

            mViewport        = new Viewport();
            mViewport.X      = 0;
            mViewport.Y      = (Mode.Height - (Mode.Width * 9 / 16 )) / 2;
            mViewport.Width  = Mode.Width;
            mViewport.Height = Mode.Width * 9 / 16;
            _graphics.GraphicsDevice.Viewport = mViewport;
        }
        
        //----------------------------------------------------------------------
        public static List<ScreenMode> SortedScreenModes;

        public static ScreenMode    Mode { get; private set; }                  // Actual mode
        public static ScreenMode    InternalMode { get; private set; }          // Internal mode

        public static Viewport      DefaultViewport { get; private set; }       // The full viewport

        private static Viewport     mViewport;                                  // The 16/9 viewport (with black borders)
        public static Viewport      Viewport { get { return mViewport; } }

        public static Matrix        Scale { get; private set; }

    }
}
