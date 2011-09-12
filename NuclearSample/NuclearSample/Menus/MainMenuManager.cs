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
            NuclearUI.BoxGroup menuGroup = new NuclearUI.BoxGroup( MenuScreen, NuclearUI.Orientation.Vertical, 20 );

            NuclearUI.Button startButton = new NuclearUI.Button( MenuScreen, "Start" );
            startButton.ClickHandler = delegate {

            };
            menuGroup.AddChild( startButton );

            NuclearUI.Button optionsButton = new NuclearUI.Button( MenuScreen, "Options" );
            optionsButton.ClickHandler = delegate {

            };
            menuGroup.AddChild( optionsButton );

            NuclearUI.Button quitButton = new NuclearUI.Button( MenuScreen, "Quit" );
            quitButton.ClickHandler = delegate {

            };

            menuGroup.AddChild( quitButton );

            MenuScreen.Root.AddChild( new NuclearUI.FixedWidget( menuGroup, NuclearUI.AnchoredRect.CreateCentered( 200, 800 ) ) );

            MenuScreen.Focus( menuGroup );
        }

        //----------------------------------------------------------------------
        public override void Update( float _fElapsedTime )
        {
            base.Update( _fElapsedTime );
        }
    }
}
