using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using NuclearAnim = NuclearWinter.Animation;

namespace NuclearSample.GameStates
{
    //--------------------------------------------------------------------------
    internal class GameStateIntro: NuclearWinter.GameFlow.GameStateFadeTransition<NuclearSampleGame>
    {
        //----------------------------------------------------------------------
        NuclearAnim.LerpValue               mSwitchTimer;
        NuclearAnim.AnimatedValue           mMushroomAnim;
        NuclearAnim.AnimatedValue           mMushroomOpacityAnim;
        NuclearAnim.AnimatedValue           mLogoAnim;
        NuclearAnim.AnimatedValue           mTitleAnim;

        Texture2D                           mMushroomTex;
        Texture2D                           mTitleTex;
        Texture2D                           mLogoTex;

        Texture2D                           mSparklinLabsTex;

        Random                              mRandom;

        //----------------------------------------------------------------------
        public GameStateIntro( NuclearSampleGame _game )
        : base( _game )
        {

        }

        //----------------------------------------------------------------------
        public override void Start()
        {
            mRandom = new Random();

            mMushroomAnim = new NuclearAnim.SmoothValue( 1f, 0f, 0.3f );
            mMushroomOpacityAnim = new NuclearAnim.SmoothValue( 0f, 1f, 0.3f );
            mLogoAnim = new NuclearAnim.LerpValue( 0f, 1f, 0.3f, 0.3f );
            mTitleAnim = new NuclearAnim.SmoothValue( 0f, 10f, 3f, 0.1f );

            mMushroomTex = Content.Load<Texture2D>( "Sprites/Mushroom" );
            mTitleTex = Content.Load<Texture2D>( "Sprites/NuclearWinterTitle" );
            mLogoTex = Content.Load<Texture2D>( "Sprites/NuclearWinterLogo" );

            mSparklinLabsTex = Content.Load<Texture2D>( "Sprites/SparklinLabs" );

            mSwitchTimer = new NuclearAnim.LerpValue( 0f, 1f, 1f, 1.5f );

            base.Start();
        }

        //----------------------------------------------------------------------
        public override void Update( float _fElapsedTime )
        {
            mMushroomAnim.Update( _fElapsedTime );
            mMushroomOpacityAnim.Update( _fElapsedTime );
            mLogoAnim.Update( _fElapsedTime );
            mTitleAnim.Update( _fElapsedTime );
            mSwitchTimer.Update( _fElapsedTime );

            if( mSwitchTimer.IsOver && ! Game.GameStateMgr.IsSwitching )
            {
                Game.GameStateMgr.SwitchState( Game.MainMenu );
            }
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            Game.GraphicsDevice.Clear( new Color( 45, 51, 49 ) );

            Game.SpriteBatch.Begin( SpriteSortMode.Deferred, null, null, null, null, null, Game.SpriteMatrix );

            Vector2 vScreenCenter = new Vector2( Game.GraphicsDevice.Viewport.Width, Game.GraphicsDevice.Viewport.Height ) / 2f;

            Vector2 vMushroomOrigin = new Vector2( mMushroomTex.Width, mMushroomTex.Height ) / 2f;
            Vector2 vTitleOrigin    = new Vector2( mTitleTex.Width, mTitleTex.Height ) / 2f;
            Vector2 vLogoOrigin     = new Vector2( mLogoTex.Width, mLogoTex.Height ) / 2f;

            float fTitleOffsetAngle = (float)mRandom.NextDouble() * MathHelper.TwoPi;
            Vector2 vTitleOffset = new Vector2( (float)Math.Cos( fTitleOffsetAngle ), (float)Math.Sin( fTitleOffsetAngle ) ) * mTitleAnim.CurrentValue;

            Game.SpriteBatch.Draw( mMushroomTex,    vScreenCenter + new Vector2( -190 + 800f * (float)Math.Pow( mMushroomAnim.CurrentValue, 2 ), -340 ) + vMushroomOrigin,   null, Color.White * (float)Math.Pow( mMushroomOpacityAnim.CurrentValue, 3 ), 0f, vMushroomOrigin, Vector2.One, SpriteEffects.None, 0f  );
            Game.SpriteBatch.Draw( mTitleTex,       vScreenCenter + new Vector2( -210, -50 ) + vTitleOrigin + vTitleOffset,                     null, Color.White, 0f, vTitleOrigin, Vector2.One, SpriteEffects.None, 0f  );
            Game.SpriteBatch.Draw( mLogoTex,        vScreenCenter + new Vector2( -580, -80 ) + vLogoOrigin,                                     null, Color.White * mLogoAnim.CurrentValue, 0f, vLogoOrigin, Vector2.One * ( 2f - (float)Math.Pow( mLogoAnim.CurrentValue, 3 ) ), SpriteEffects.None, 0f  );

            Game.SpriteBatch.Draw( mSparklinLabsTex, new Vector2( Game.GraphicsDevice.Viewport.Width - mSparklinLabsTex.Width, Game.GraphicsDevice.Viewport.Height - mSparklinLabsTex.Height ), null, Color.White );

            Game.SpriteBatch.End();
        }
    }
}
