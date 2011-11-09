using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using NuclearWinter;
using NuclearUI = NuclearWinter.UI;

namespace NuclearSample
{
    //--------------------------------------------------------------------------
    public class NuclearSampleGame : NuclearGame
    {
        //----------------------------------------------------------------------
        internal GameStates.GameStateIntro      Intro       { get; private set; }
        internal GameStates.GameStateMainMenu   MainMenu    { get; private set; }

        //----------------------------------------------------------------------
        internal NuclearUI.Style                UIStyle     { get; private set; }

        //----------------------------------------------------------------------
        public NuclearSampleGame()
        {
            Content.RootDirectory = "Content";

            Graphics.PreferredBackBufferWidth = 1280;
            Graphics.PreferredBackBufferHeight = 720;
        }

        //----------------------------------------------------------------------
        protected override void Initialize()
        {

            base.Initialize();
        }

        //----------------------------------------------------------------------
        protected override void LoadContent()
        {
            //------------------------------------------------------------------
            // UI Style
            UIStyle = new NuclearUI.Style();

            UIStyle.BlurRadius  = 1;
            UIStyle.SmallFont   = new NuclearUI.UIFont( Content.Load<SpriteFont>( "Fonts/SmallFont" ) );
            UIStyle.MediumFont  = new NuclearUI.UIFont( Content.Load<SpriteFont>( "Fonts/MediumFont" ) );
            UIStyle.BigFont     = new NuclearUI.UIFont( Content.Load<SpriteFont>( "Fonts/BigFont" ) );

            UIStyle.ButtonFrame     = Content.Load<Texture2D>( "Sprites/UI/ButtonFrame" );
            UIStyle.ButtonHover     = Content.Load<Texture2D>( "Sprites/UI/ButtonHover" );
            UIStyle.ButtonFocus     = Content.Load<Texture2D>( "Sprites/UI/ButtonFocus" );
            UIStyle.ButtonFrameDown = Content.Load<Texture2D>( "Sprites/UI/ButtonFrameDown" );

            //------------------------------------------------------------------
            // Game states
            Intro       = new GameStates.GameStateIntro( this );
            MainMenu    = new GameStates.GameStateMainMenu( this );
            GameStateMgr.SwitchState( Intro );

            base.LoadContent();
        }

        //----------------------------------------------------------------------
        protected override void Update( GameTime _time )
        {
            base.Update( _time );
        }

        //----------------------------------------------------------------------
        protected override void Draw( GameTime _time )
        {
            base.Draw( _time );
        }
    }
}
