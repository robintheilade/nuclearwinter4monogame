using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NuclearUI = NuclearWinter.UI;
using Microsoft.Xna.Framework.Content;

namespace NuclearSample.Menus
{
    //--------------------------------------------------------------------------
    internal class MainMenuManager: NuclearUI.MenuManager<NuclearSampleGame>
    {
        //----------------------------------------------------------------------
        public MainMenuManager( NuclearSampleGame _game, ContentManager _content )
        : base( _game, _game.UIStyle, _content )
        {
            // Menu group
            NuclearUI.BoxGroup menuGroup = new NuclearUI.BoxGroup( MenuScreen, NuclearUI.Orientation.Vertical, 20 );
            menuGroup.AnchoredRect = NuclearUI.AnchoredRect.CreateCentered( 200, 800 );
            MenuScreen.Root.AddChild( menuGroup );
            MenuScreen.Focus( menuGroup );

            // Start button
            NuclearUI.Button startButton = new NuclearUI.Button( MenuScreen, "Start" );
            menuGroup.AddChild( startButton );

            startButton.ClickHandler = delegate {

            };

            // Options button
            NuclearUI.Button optionsButton = new NuclearUI.Button( MenuScreen, "Options" );
            menuGroup.AddChild( optionsButton );

            optionsButton.ClickHandler = delegate {

            };

            // Quit button
            NuclearUI.Button quitButton = new NuclearUI.Button( MenuScreen, "Quit" );
            menuGroup.AddChild( quitButton );

            quitButton.ClickHandler = delegate {
                Game.Exit();
            };

        }

        //----------------------------------------------------------------------
        public override void Update( float _fElapsedTime )
        {
            base.Update( _fElapsedTime );
        }
    }
}
