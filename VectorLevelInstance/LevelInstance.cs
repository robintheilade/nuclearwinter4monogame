using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace VectorLevel
{
    public class LevelInstance
    {
        //----------------------------------------------------------------------
        public LevelInstance( uint _mapWidth, uint _mapHeight )
        {
            MapWidth            = _mapWidth;
            MapHeight           = _mapHeight;
            AmbientLightColor   = new Color( 192, 192, 192 );
        }
        
        //----------------------------------------------------------------------
        /// <summary>
        /// Initialize Level geometry rendering
        /// </summary>
        /// <param name="_graphicsDevice">Graphics device to use for rendering</param>
        /// <param name="_spriteBatch">Spritebatch to use for rendering</param>
        /// <param name="_serviceProvider">Service provider to use for content loading</param>
        public void InitRender( GraphicsDevice _graphicsDevice, SpriteBatch _spriteBatch, IServiceProvider _serviceProvider )
        {
            GraphicsDevice              = _graphicsDevice;
			VertexDeclaration           = VertexPositionColorTexture.VertexDeclaration;
            
            //------------------------------------------------------------------
            // Setup lightmap
            PresentationParameters pp = _graphicsDevice.PresentationParameters;
            mLightMap = new RenderTarget2D( _graphicsDevice, pp.BackBufferWidth / 4, pp.BackBufferHeight / 4, false, SurfaceFormat.Color, DepthFormat.None );

            mSpriteBatch = _spriteBatch;
            mContent = new ContentManager( _serviceProvider, "Content" );
            mFullAlphaTex = mContent.Load<Texture2D>( "BlackPixel" );

            //------------------------------------------------------------------
            mavBorderVertices = new VertexPositionColor[ 8 * 3 ];

            for( int i = 0; i < mavBorderVertices.Length; i++ )
            {
                mavBorderVertices[ i ].Color = Color.Black;
            }
            
            // Top
            mavBorderVertices[0].Position = new Vector3( -sfBorderWidth, -sfBorderWidth, 0f );
            mavBorderVertices[1].Position = new Vector3( MapWidth + sfBorderWidth, -sfBorderWidth, 0f );
            mavBorderVertices[2].Position = new Vector3( -sfBorderWidth, 0f, 0f );
            
            mavBorderVertices[3].Position = new Vector3( MapWidth + sfBorderWidth, -sfBorderWidth, 0f );
            mavBorderVertices[4].Position = new Vector3( MapWidth + sfBorderWidth, 0f, 0f );
            mavBorderVertices[5].Position = new Vector3( -sfBorderWidth, 0f, 0f );
            
            // Right
            mavBorderVertices[6].Position = new Vector3( MapWidth, 0f, 0f );
            mavBorderVertices[7].Position = new Vector3( MapWidth + sfBorderWidth, 0f, 0f );
            mavBorderVertices[8].Position = new Vector3( MapWidth, MapHeight, 0f );
            
            mavBorderVertices[9].Position = new Vector3( MapWidth + sfBorderWidth, 0f, 0f );
            mavBorderVertices[10].Position = new Vector3( MapWidth + sfBorderWidth, MapHeight, 0f );
            mavBorderVertices[11].Position = new Vector3( MapWidth, MapHeight, 0f );

            // Bottom
            mavBorderVertices[12].Position = new Vector3( -sfBorderWidth, MapHeight, 0f );
            mavBorderVertices[13].Position = new Vector3( MapWidth + sfBorderWidth, MapHeight, 0f );
            mavBorderVertices[14].Position = new Vector3( MapWidth + sfBorderWidth, MapHeight + sfBorderWidth, 0f );
            
            mavBorderVertices[15].Position = new Vector3( -sfBorderWidth, MapHeight, 0f );
            mavBorderVertices[16].Position = new Vector3( MapWidth + sfBorderWidth, MapHeight + sfBorderWidth, 0f );
            mavBorderVertices[17].Position = new Vector3( -sfBorderWidth, MapHeight + sfBorderWidth, 0f );

            // Left
            mavBorderVertices[18].Position = new Vector3( -sfBorderWidth, 0f, 0f );
            mavBorderVertices[19].Position = new Vector3( 0f, 0f, 0f );
            mavBorderVertices[20].Position = new Vector3( 0f, MapHeight, 0f );
            
            mavBorderVertices[21].Position = new Vector3( 0f, 0f, 0f );
            mavBorderVertices[22].Position = new Vector3( 0f, MapHeight, 0f );
            mavBorderVertices[23].Position = new Vector3( -sfBorderWidth, MapHeight, 0f );
        }

        //----------------------------------------------------------------------
        public void DrawLevelBorders()
        {
            GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, mavBorderVertices, 0, 8 );
        }

        //----------------------------------------------------------------------
        /// <summary>
        /// Prepare light map for shadow rendering
        /// </summary>
        public void PrepareLightMap()
        {
            mSavedViewport = GraphicsDevice.Viewport;
            GraphicsDevice.SetRenderTarget( mLightMap );
            //GraphicsDevice.Viewport = mSavedViewport;

            GraphicsDevice.Clear( AmbientLightColor );
        }
        
        //----------------------------------------------------------------------
        /// <summary>
        /// Restore rendering settings
        /// </summary>
        public void EndLightMap()
        {
            ClearAlphaToOne();
            GraphicsDevice.SetRenderTarget( null );
            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.Viewport = mSavedViewport;
        }

        //----------------------------------------------------------------------
        public void ClearAlphaToOne()
        {
            mSpriteBatch.Begin( SpriteSortMode.Immediate, BlendState.Opaque );

            BlendState blendState = new BlendState();
            blendState.AlphaDestinationBlend    = Blend.One;
            blendState.AlphaSourceBlend         = Blend.One;
            blendState.AlphaBlendFunction       = BlendFunction.Max;
            blendState.ColorWriteChannels       = ColorWriteChannels.Alpha;
            blendState.ColorWriteChannels1       = ColorWriteChannels.Alpha;
            blendState.ColorWriteChannels2       = ColorWriteChannels.Alpha;
            blendState.ColorWriteChannels3       = ColorWriteChannels.Alpha;

            GraphicsDevice.BlendState           = blendState;

            mSpriteBatch.Draw( mFullAlphaTex, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White );
            mSpriteBatch.End();

            GraphicsDevice.BlendState = BlendState.AlphaBlend;
        }
        
        //----------------------------------------------------------------------
        public uint                 MapWidth;
        public uint                 MapHeight;

        //----------------------------------------------------------------------
        // Rendering
		public VertexDeclaration    VertexDeclaration   { get; private set; }
        internal GraphicsDevice     GraphicsDevice      { get; private set; }

        internal VertexBuffer       FillVertexBuffer    { get; private set; }
		internal IndexBuffer        FillIndexBuffer     { get; private set; }
        internal VertexBuffer       StrokeVertexBuffer  { get; private set; }
        internal IndexBuffer        StrokeIndexBuffer   { get; private set; }

        ContentManager              mContent;
        SpriteBatch                 mSpriteBatch;

        Viewport                    mSavedViewport;
        public RenderTarget2D       mLightMap;
        Texture2D                   mFullAlphaTex;
        public Color                AmbientLightColor;

        static float                sfBorderWidth = 1000f;
        public float                PhysicsRatio;
        VertexPositionColor[]       mavBorderVertices;
    }
}
