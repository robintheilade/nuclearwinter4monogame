using System;
using System.Collections.Generic;
using System.Diagnostics;

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
    public class LevelRenderer
    {
        //----------------------------------------------------------------------
        public LevelRenderer( Game _game, uint _uiMapWidth, uint _uiMapHeight )
        {
            Game                = _game;

            MapWidth            = _uiMapWidth;
            MapHeight           = _uiMapHeight;

            Content            = new ContentManager( _game.Services, "Content" );
            FullAlphaTex       = Content.Load<Texture2D>( "BlackPixel" );

            //------------------------------------------------------------------
            // Level borders
            SetupLevelBorder( sfBorderWidth );

            //------------------------------------------------------------------
            // Render batching
            mavBatchVertices    = new Vector2[4];
            mavSpriteQuad       = new VertexPositionColorTexture[4];

            //------------------------------------------------------------------
            // Effect
			Effect = new BasicEffect( Game.GraphicsDevice );
			Effect.World = Matrix.Identity;
			Effect.VertexColorEnabled = true;
			Effect.DiffuseColor = new Vector3( 1f );
        }

        //----------------------------------------------------------------------
        public void SetupLevelBorder( float _fBorderWidth )
        {
            mavBorderVertices = new VertexPositionColor[ 8 * 3 ];

            for( int i = 0; i < mavBorderVertices.Length; i++ )
            {
                mavBorderVertices[ i ].Color = Color.Black;
            }
            
            // Top
            mavBorderVertices[0].Position   = new Vector3( -_fBorderWidth,              -_fBorderWidth,             0f );
            mavBorderVertices[1].Position   = new Vector3( MapWidth + _fBorderWidth,    -_fBorderWidth,             0f );
            mavBorderVertices[2].Position   = new Vector3( -_fBorderWidth,              0f,                         0f );
            
            mavBorderVertices[3].Position   = new Vector3( MapWidth + _fBorderWidth,    -_fBorderWidth,             0f );
            mavBorderVertices[4].Position   = new Vector3( MapWidth + _fBorderWidth,    0f,                         0f );
            mavBorderVertices[5].Position   = new Vector3( -_fBorderWidth,              0f,                         0f );
            
            // Right
            mavBorderVertices[6].Position   = new Vector3( MapWidth,                    0f,                         0f );
            mavBorderVertices[7].Position   = new Vector3( MapWidth + _fBorderWidth,    0f,                         0f );
            mavBorderVertices[8].Position   = new Vector3( MapWidth,                    MapHeight,                  0f );
            
            mavBorderVertices[9].Position   = new Vector3( MapWidth + _fBorderWidth,    0f,                         0f );
            mavBorderVertices[10].Position  = new Vector3( MapWidth + _fBorderWidth,    MapHeight,                  0f );
            mavBorderVertices[11].Position  = new Vector3( MapWidth,                    MapHeight,                  0f );

            // Bottom
            mavBorderVertices[12].Position  = new Vector3( -_fBorderWidth,              MapHeight,                  0f );
            mavBorderVertices[13].Position  = new Vector3( MapWidth + _fBorderWidth,    MapHeight,                  0f );
            mavBorderVertices[14].Position  = new Vector3( MapWidth + _fBorderWidth,    MapHeight + _fBorderWidth,  0f );
            
            mavBorderVertices[15].Position  = new Vector3( -_fBorderWidth,              MapHeight,                  0f );
            mavBorderVertices[16].Position  = new Vector3( MapWidth + _fBorderWidth,    MapHeight + _fBorderWidth,  0f );
            mavBorderVertices[17].Position  = new Vector3( -_fBorderWidth,              MapHeight + _fBorderWidth,  0f );

            // Left
            mavBorderVertices[18].Position  = new Vector3( -_fBorderWidth,              0f,                         0f );
            mavBorderVertices[19].Position  = new Vector3( 0f,                          0f,                         0f );
            mavBorderVertices[20].Position  = new Vector3( 0f,                          MapHeight,                  0f );
            
            mavBorderVertices[21].Position  = new Vector3( 0f,                          0f, 0f );
            mavBorderVertices[22].Position  = new Vector3( 0f,                          MapHeight,                  0f );
            mavBorderVertices[23].Position  = new Vector3( -_fBorderWidth,              MapHeight,                  0f );
        }

        //----------------------------------------------------------------------
        public void DrawLevelBorders()
        {
            Game.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(
                PrimitiveType.TriangleList,
                mavBorderVertices,
                0,
                mavBorderVertices.Length / 3 );
        }
        
        //----------------------------------------------------------------------
        public void DrawSprite( Texture2D _texture, Vector2 _vPosition, Color _color, float _fRotation, Vector2 _vOrigin, float _fScale )
        {
            DrawSprite( _texture, _vPosition, Rectangle.Empty, _color, _fRotation, _vOrigin, new Vector2( _fScale ) );
        }

        //----------------------------------------------------------------------
        public void DrawSprite( Texture2D _texture, Vector2 _vPosition, Color _color, float _fRotation, Vector2 _vOrigin, Vector2 _vScale )
        {
            DrawSprite( _texture, _vPosition, Rectangle.Empty, _color, _fRotation, _vOrigin, _vScale );
        }

        //----------------------------------------------------------------------
        public void DrawSprite( Texture2D _texture, Vector2 _vPosition, Rectangle _sourceRectangle, Color _color, float _fRotation, Vector2 _vOrigin, float _fScale )
        {
            DrawSprite( _texture, _vPosition, _sourceRectangle, _color, _fRotation, _vOrigin, new Vector2( _fScale ) );
        }

        //----------------------------------------------------------------------
        public void DrawSprite( Texture2D _texture, Vector2 _vPosition, Rectangle _sourceRectangle, Color _color, float _fRotation, Vector2 _vOrigin, Vector2 _vScale )
        {
            Effect.Texture = _texture;
            Effect.CurrentTechnique.Passes[0].Apply();
            
            if( _sourceRectangle == Rectangle.Empty )
            {
                _sourceRectangle = new Rectangle( 0, 0, _texture.Width, _texture.Height );
            }

            float fWidth = _sourceRectangle.Width;
            float fHeight = _sourceRectangle.Height;

            mavBatchVertices[0] = ( -_vOrigin )                                   * _vScale;
            mavBatchVertices[1] = ( -_vOrigin + new Vector2( fWidth, 0f ) )       * _vScale;
            mavBatchVertices[2] = ( -_vOrigin + new Vector2( 0f, fHeight ) )      * _vScale;
            mavBatchVertices[3] = ( -_vOrigin + new Vector2( fWidth, fHeight ) )  * _vScale;
            
            Matrix rotationMatrix = new Matrix();
            Matrix.CreateRotationZ( _fRotation, out rotationMatrix );
            Vector2.Transform( mavBatchVertices, ref rotationMatrix, mavBatchVertices );

            mavSpriteQuad[0].Position = new Vector3( _vPosition + mavBatchVertices[0], 0f );
            mavSpriteQuad[0].TextureCoordinate = new Vector2( (float)_sourceRectangle.Left / _texture.Width, (float)_sourceRectangle.Top / _texture.Height );
            mavSpriteQuad[0].Color = _color;

            mavSpriteQuad[1].Position = new Vector3( _vPosition + mavBatchVertices[1], 0f );
            mavSpriteQuad[1].TextureCoordinate = new Vector2( (float)_sourceRectangle.Right / _texture.Width, (float)_sourceRectangle.Top / _texture.Height );
            mavSpriteQuad[1].Color = _color;

            mavSpriteQuad[2].Position = new Vector3( _vPosition + mavBatchVertices[2], 0f );
            mavSpriteQuad[2].TextureCoordinate = new Vector2( (float)_sourceRectangle.Left / _texture.Width, (float)_sourceRectangle.Bottom / _texture.Height );
            mavSpriteQuad[2].Color = _color;

            mavSpriteQuad[3].Position = new Vector3( _vPosition + mavBatchVertices[3], 0f );
            mavSpriteQuad[3].TextureCoordinate = new Vector2( (float)_sourceRectangle.Right / _texture.Width, (float)_sourceRectangle.Bottom / _texture.Height );
            mavSpriteQuad[3].Color = _color;

            Game.GraphicsDevice.DrawUserPrimitives<VertexPositionColorTexture>(PrimitiveType.TriangleStrip, mavSpriteQuad, 0, 2 );
        }

        //----------------------------------------------------------------------
        public void BatchStartSprite( Texture2D _texture )
        {
            mBatchSpriteTexture = _texture;
            BatchedSpritesCount = 0;   
        }

        //----------------------------------------------------------------------
        public void BatchDrawPrepared( Texture2D _texture, VertexBuffer _vertexBuffer )
        {
            Effect.Texture = _texture;
            Effect.CurrentTechnique.Passes[0].Apply();

            Game.GraphicsDevice.SetVertexBuffer( _vertexBuffer );
            Game.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, _vertexBuffer.VertexCount / 3 );
        }

        //----------------------------------------------------------------------
        public void BatchDrawSprite( Vector2 _vPosition, Color _color, float _fRotation, Vector2 _vOrigin, float _fScale )
        {
            BatchDrawSprite( _vPosition, Rectangle.Empty, _color, _fRotation, _vOrigin, new Vector2( _fScale ) );
        }

        //----------------------------------------------------------------------
        public void BatchDrawSprite( Vector2 _vPosition, Rectangle _sourceRectangle, Color _color, float _fRotation, Vector2 _vOrigin, float _fScale )
        {
            BatchDrawSprite( _vPosition, _sourceRectangle, _color, _fRotation, _vOrigin, new Vector2( _fScale ) );
        }

        //----------------------------------------------------------------------
        public void BatchDrawSprite( Vector2 _vPosition, Rectangle _sourceRectangle, Color _color, float _fRotation, Vector2 _vOrigin, Vector2 _vScale )
        {
            if( BatchedSpritesCount >= MaxBatchedSpritesCount )
            {
                Debug.Assert( false, "Too many sprites have been batched" );
                return;
            }

            if( _sourceRectangle == Rectangle.Empty )
            {
                _sourceRectangle = new Rectangle( 0, 0, mBatchSpriteTexture.Width, mBatchSpriteTexture.Height );
            }

            float fWidth = _sourceRectangle.Width;
            float fHeight = _sourceRectangle.Height;

            mavBatchVertices[0] = ( -_vOrigin )                                   * _vScale;
            mavBatchVertices[1] = ( -_vOrigin + new Vector2( fWidth, 0f ) )       * _vScale;
            mavBatchVertices[2] = ( -_vOrigin + new Vector2( 0f, fHeight ) )      * _vScale;
            mavBatchVertices[3] = ( -_vOrigin + new Vector2( fWidth, fHeight ) )  * _vScale;
            
            Matrix rotationMatrix = new Matrix();
            Matrix.CreateRotationZ( _fRotation, out rotationMatrix );
            Vector2.Transform( mavBatchVertices, ref rotationMatrix, mavBatchVertices );
            
            // First triangle
            BatchSpriteVertices[ 6 * BatchedSpritesCount + 0 ].Position = new Vector3( _vPosition + mavBatchVertices[0], 0f );
            BatchSpriteVertices[ 6 * BatchedSpritesCount + 0 ].TextureCoordinate = new Vector2( (float)_sourceRectangle.Left / mBatchSpriteTexture.Width, (float)_sourceRectangle.Top / mBatchSpriteTexture.Height );
            BatchSpriteVertices[ 6 * BatchedSpritesCount + 0 ].Color = _color;

            BatchSpriteVertices[ 6 * BatchedSpritesCount + 1 ].Position = new Vector3( _vPosition + mavBatchVertices[1], 0f );
            BatchSpriteVertices[ 6 * BatchedSpritesCount + 1 ].TextureCoordinate = new Vector2( (float)_sourceRectangle.Right / mBatchSpriteTexture.Width, (float)_sourceRectangle.Top / mBatchSpriteTexture.Height );
            BatchSpriteVertices[ 6 * BatchedSpritesCount + 1 ].Color = _color;

            BatchSpriteVertices[ 6 * BatchedSpritesCount + 2 ].Position = new Vector3( _vPosition + mavBatchVertices[2], 0f );
            BatchSpriteVertices[ 6 * BatchedSpritesCount + 2 ].TextureCoordinate = new Vector2( (float)_sourceRectangle.Left / mBatchSpriteTexture.Width, (float)_sourceRectangle.Bottom / mBatchSpriteTexture.Height );
            BatchSpriteVertices[ 6 * BatchedSpritesCount + 2 ].Color = _color;

            // Second triangle
            BatchSpriteVertices[ 6 * BatchedSpritesCount + 4 ].Position = new Vector3( _vPosition + mavBatchVertices[1], 0f );
            BatchSpriteVertices[ 6 * BatchedSpritesCount + 4 ].TextureCoordinate = new Vector2( (float)_sourceRectangle.Right / mBatchSpriteTexture.Width, (float)_sourceRectangle.Top / mBatchSpriteTexture.Height );
            BatchSpriteVertices[ 6 * BatchedSpritesCount + 4 ].Color = _color;

            BatchSpriteVertices[ 6 * BatchedSpritesCount + 3 ].Position = new Vector3( _vPosition + mavBatchVertices[2], 0f );
            BatchSpriteVertices[ 6 * BatchedSpritesCount + 3 ].TextureCoordinate = new Vector2( (float)_sourceRectangle.Left / mBatchSpriteTexture.Width, (float)_sourceRectangle.Bottom / mBatchSpriteTexture.Height );
            BatchSpriteVertices[ 6 * BatchedSpritesCount + 3 ].Color = _color;

            BatchSpriteVertices[ 6 * BatchedSpritesCount + 5 ].Position = new Vector3( _vPosition + mavBatchVertices[3], 0f );
            BatchSpriteVertices[ 6 * BatchedSpritesCount + 5 ].TextureCoordinate = new Vector2( (float)_sourceRectangle.Right / mBatchSpriteTexture.Width, (float)_sourceRectangle.Bottom / mBatchSpriteTexture.Height );
            BatchSpriteVertices[ 6 * BatchedSpritesCount + 5 ].Color = _color;

            BatchedSpritesCount++;
        }

        //----------------------------------------------------------------------
        public void BatchEndSprite()
        {
            if( BatchedSpritesCount > 0 )
            {
                Effect.Texture = mBatchSpriteTexture;
                Effect.CurrentTechnique.Passes[0].Apply();
                Game.GraphicsDevice.DrawUserPrimitives<VertexPositionColorTexture>(PrimitiveType.TriangleList, BatchSpriteVertices, 0, BatchedSpritesCount * 2 );
            }
        }

        //----------------------------------------------------------------------
        public Game                 Game;
        public ContentManager       Content;
        public BasicEffect          Effect;

        public uint                 MapWidth;
        public uint                 MapHeight;

        public Texture2D            FullAlphaTex;

        //----------------------------------------------------------------------
        // Border
        static float                sfBorderWidth = 1000f;
        VertexPositionColor[]       mavBorderVertices;

        //----------------------------------------------------------------------
        // Batching
        Texture2D                               mBatchSpriteTexture;
        Vector2[]                               mavBatchVertices;
        VertexPositionColorTexture[]            mavSpriteQuad;

        const int                               MaxBatchedSpritesCount = 300;
        public int                              BatchedSpritesCount;
        public VertexPositionColorTexture[]     BatchSpriteVertices = new VertexPositionColorTexture[ MaxBatchedSpritesCount * 6 ];
    }
}
