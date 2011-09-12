using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace NuclearSample.GameStates
{
    //--------------------------------------------------------------------------
    internal class GameStateIntro: NuclearWinter.GameFlow.GameStateFadeTransition<NuclearSampleGame>
    {
        //----------------------------------------------------------------------
        Texture2D                                   mLogoTex;
        NuclearWinter.Animation.AnimatedValue       mLogoAnim;

        //----------------------------------------------------------------------
        public GameStateIntro( NuclearSampleGame _game )
        : base( _game )
        {

        }

        //----------------------------------------------------------------------
        public override void Start()
        {
            mLogoTex = Content.Load<Texture2D>( "Sprites/Logo" );
            mLogoAnim = new NuclearWinter.Animation.SmoothValue( 0.3f, 1f, 1.5f );

            base.Start();
        }

        //----------------------------------------------------------------------
        public override void Update( float _fElapsedTime )
        {
            mLogoAnim.Update( _fElapsedTime );

            if( mLogoAnim.IsOver && ! Game.GameStateMgr.IsSwitching )
            {
                Game.GameStateMgr.SwitchState( Game.MainMenu );
            }
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            Game.GraphicsDevice.Clear( Color.Black );

            Game.SpriteBatch.Begin( SpriteSortMode.Deferred, null, null, null, null, null, Game.SpriteMatrix );

            Vector2 vScreenCenter = new Vector2( Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height ) / 2f;
            Vector2 vLogoCenter = new Vector2( mLogoTex.Width, mLogoTex.Height ) /2f;

            Game.SpriteBatch.Draw( mLogoTex, vScreenCenter, null, Color.White * mLogoAnim.CurrentValue, 0f, vLogoCenter, mLogoAnim.CurrentValue, SpriteEffects.None, 0f );

            Game.SpriteBatch.End();
        }
    }
}
