using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace VectorLevel
{
    //--------------------------------------------------------------------------
    public class LightMap
    {
        //----------------------------------------------------------------------
        public LightMap( LevelRenderer _levelRenderer, int _iLightMapSizeFactor, Color _ambientLightColor )
        {
            LevelRenderer           = _levelRenderer;

            LightMapSizeFactor  = _iLightMapSizeFactor;
            AmbientLightColor   = _ambientLightColor;

            mClearAlphaBlendState = new BlendState();
            mClearAlphaBlendState.AlphaDestinationBlend     = Blend.One;
            mClearAlphaBlendState.AlphaSourceBlend          = Blend.One;
            mClearAlphaBlendState.AlphaBlendFunction        = BlendFunction.Max;
            mClearAlphaBlendState.ColorWriteChannels        = ColorWriteChannels.Alpha;
            mClearAlphaBlendState.ColorWriteChannels1       = ColorWriteChannels.Alpha;
            mClearAlphaBlendState.ColorWriteChannels2       = ColorWriteChannels.Alpha;
            mClearAlphaBlendState.ColorWriteChannels3       = ColorWriteChannels.Alpha;

            PresentationParameters pp = LevelRenderer.Game.GraphicsDevice.PresentationParameters;
            LightMapTex = new RenderTarget2D(
                LevelRenderer.Game.GraphicsDevice,
                pp.BackBufferWidth / LightMapSizeFactor,
                pp.BackBufferHeight / LightMapSizeFactor,
                false,
                SurfaceFormat.Color,
                DepthFormat.None );
        }

        //----------------------------------------------------------------------
        /// Prepare light map for shadow rendering
        public void PrepareLightMap()
        {
            mSavedViewport = LevelRenderer.Game.GraphicsDevice.Viewport;
            LevelRenderer.Game.GraphicsDevice.SetRenderTarget( LightMapTex );
            LevelRenderer.Game.GraphicsDevice.Clear( AmbientLightColor );
        }
        
        //----------------------------------------------------------------------
        /// Restore rendering settings
        public void EndLightMap()
        {
            ClearAlphaToOne();
            LevelRenderer.Game.GraphicsDevice.SetRenderTarget( null );
            LevelRenderer.Game.GraphicsDevice.Clear( Color.Black );
            LevelRenderer.Game.GraphicsDevice.Viewport = mSavedViewport;
        }

        //----------------------------------------------------------------------
        public void ClearAlphaToOne()
        {
            LevelRenderer.Game.GraphicsDevice.BlendState = mClearAlphaBlendState;
            LevelRenderer.DrawSprite( LevelRenderer.FullAlphaTex,
                Vector2.Zero,
                Color.White,
                0f,
                Vector2.Zero,
                new Vector2( LevelRenderer.MapWidth, LevelRenderer.MapHeight )
            );

            LevelRenderer.Game.GraphicsDevice.BlendState = BlendState.AlphaBlend;
        }

        //----------------------------------------------------------------------
        public LevelRenderer        LevelRenderer               { get; private set; }

        public Color                AmbientLightColor;
        public int                  LightMapSizeFactor          { get; private set; }
        public RenderTarget2D       LightMapTex                 { get; private set; }

        //----------------------------------------------------------------------
        BlendState                  mClearAlphaBlendState;

        Viewport                    mSavedViewport;
    }
}
