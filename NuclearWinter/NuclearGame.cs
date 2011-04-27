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
        public NuclearGame()
        {
            Graphics = new GraphicsDeviceManager(this);
        }

        protected override void Initialize()
        {
            SpriteBatch = new SpriteBatch( GraphicsDevice );

            GameStateMgr = new NuclearWinter.GameFlow.GameStateMgr( this );
            Components.Add( GameStateMgr );

            base.Initialize();
        }

        //----------------------------------------------------------------------
        public GraphicsDeviceManager                        Graphics;
        public SpriteBatch                                  SpriteBatch;

        //----------------------------------------------------------------------
        // Game States
        public NuclearWinter.GameFlow.GameStateMgr          GameStateMgr        { get; private set; }
    }
}
