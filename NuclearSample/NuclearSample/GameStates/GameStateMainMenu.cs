using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace NuclearSample.GameStates
{
    //--------------------------------------------------------------------------
    internal class GameStateMainMenu: NuclearWinter.GameFlow.GameStateFadeTransition<NuclearSampleGame>
    {
        //----------------------------------------------------------------------
        Menus.MainMenuManager       mMainMenuManager;

        //----------------------------------------------------------------------
        public GameStateMainMenu( NuclearSampleGame _game )
        : base( _game )
        {

        }

        //----------------------------------------------------------------------
        public override void Start()
        {
            mMainMenuManager = new Menus.MainMenuManager( Game, Content );
            Game.IsMouseVisible = true;

            base.Start();
        }

        public override void Stop()
        {
            mMainMenuManager = null;

            base.Stop();
        }

        //----------------------------------------------------------------------
        public override void Update( float _fElapsedTime )
        {
            mMainMenuManager.Update( _fElapsedTime );
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            Game.GraphicsDevice.Clear( Color.Black );

            mMainMenuManager.Draw();
        }
    }
}
