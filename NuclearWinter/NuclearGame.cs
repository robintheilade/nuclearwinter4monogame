using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NuclearWinter
{
    public class NuclearGame: Game
    {
        //----------------------------------------------------------------------
        public NuclearGame()
        {
            Graphics = new GraphicsDeviceManager(this);
        }

        //----------------------------------------------------------------------
        protected override void Initialize()
        {
            SpriteBatch = new SpriteBatch( GraphicsDevice );

            GameStateMgr = new GameFlow.GameStateMgr( this );
            Components.Add( GameStateMgr );

#if WINDOWS || XBOX
            GamePadMgr                = new Input.GamePadManager( this );
            Components.Add( GamePadMgr );
#endif

#if WINDOWS_PHONE
            TouchMgr                = new Input.TouchManager( this );
            Components.Add( TouchMgr );
#endif

            base.Initialize();
        }

        //----------------------------------------------------------------------
        public virtual string GetUIString( string _strId ) { return _strId; }

        //----------------------------------------------------------------------
        public GraphicsDeviceManager                        Graphics;
        public SpriteBatch                                  SpriteBatch;

        //----------------------------------------------------------------------
        // Game States
        public GameFlow.GameStateMgr                        GameStateMgr        { get; private set; }

#if WINDOWS || XBOX
        public Input.GamePadManager                         GamePadMgr          { get; private set; }
#endif

#if WINDOWS_PHONE
        public Input.TouchManager                           TouchMgr            { get; private set; }
#endif
    }
}
