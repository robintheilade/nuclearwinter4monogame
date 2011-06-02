﻿using System;
using System.Collections.Generic;

using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VectorLevel
{
    //--------------------------------------------------------------------------
    /// <summary>
    /// A static block that can be rendered 
    /// </summary>
    public class BlockRenderer
    {
        //----------------------------------------------------------------------
        public BlockRenderer( LevelRenderer _levelRenderer, Entities.Path _path )
        {
            LevelRenderer   = _levelRenderer;
            Path            = _path;

            ComputeAABB();
        }

        //----------------------------------------------------------------------
        void ComputeAABB()
        {
            AABB = new NuclearWinter.AABB( Path.Subpaths[0].Vertices[0], Path.Subpaths[0].Vertices[0] );

            int j = Path.Subpaths[0].Vertices.Count - 1;
            for( int i = 0; i < Path.Subpaths[0].Vertices.Count; i++ )
            {
                if( Path.Subpaths[0].Vertices[i].X < AABB.Min.X )
                {
                    AABB.Min.X = Path.Subpaths[0].Vertices[i].X;
                }

                if( Path.Subpaths[0].Vertices[i].X > AABB.Max.X )
                {
                    AABB.Max.X = Path.Subpaths[0].Vertices[i].X;
                }

                if( Path.Subpaths[0].Vertices[i].Y < AABB.Min.Y )
                {
                    AABB.Min.Y = Path.Subpaths[0].Vertices[i].Y;
                }

                if( Path.Subpaths[0].Vertices[i].Y > AABB.Max.Y )
                {
                    AABB.Max.Y = Path.Subpaths[0].Vertices[i].Y;
                }
            }
        }

        //----------------------------------------------------------------------
        public void InitRender( float _fDepth, float _fScale )
        {
            int iFillVerticesCount      = Path.MeshVertices.Length;
            int iFillPrimitivesCount    = Path.MeshIndices.Length / 3;

            int iStrokeVerticesCount    = (Path.Subpaths[0].Vertices.Count + 1) * 2;
            int iStrokePrimitivesCount  = ( Path.Subpaths[0].Vertices.Count + (Path.Subpaths[0].IsClosed ? 0 : -1) ) * 2;

            if( Path.Subpaths[0].IsClosed )
            {
                //--------------------------------------------------------------
			    // Create Fill VertexBuffer
			    VertexPositionColorTexture[] aFillVertices = new VertexPositionColorTexture[ iFillVerticesCount ];

	            for (int i = 0; i < Path.MeshVertices.Length; i++)
                {
	                aFillVertices[ i ] = new VertexPositionColorTexture( new Vector3( Path.MeshVertices[i], _fDepth ), Path.FillColor, Path.MeshVertices[i] / 200f / _fScale );
                }

                mFillVertexBuffer = new VertexBuffer(
                    LevelRenderer.Game.GraphicsDevice,
                    VertexPositionColorTexture.VertexDeclaration,
                    aFillVertices.Length,
                    BufferUsage.WriteOnly );
                mFillVertexBuffer.SetData( aFillVertices );

                //--------------------------------------------------------------
			    // Create Fill IndexBuffer
			    short[] aFillIndices = new short[ iFillPrimitivesCount * 3 ];

                for (int i = 0; i < Path.MeshIndices.Length; i++)
                {
                    aFillIndices[ i ] = (short)Path.MeshIndices[i];
                }

			    mFillIndexBuffer = new IndexBuffer( LevelRenderer.Game.GraphicsDevice, IndexElementSize.SixteenBits, aFillIndices.Length, BufferUsage.WriteOnly );
			    mFillIndexBuffer.SetData( aFillIndices );
            }

            //------------------------------------------------------------------
            // Create Stroke VertexBuffer
            VertexPositionColorTexture[] aStrokeVertices = new VertexPositionColorTexture[ iStrokeVerticesCount ];

            int iPathNodeIndex = 0;
            Vector2 vNextNodeOutOffset = Vector2.Zero;
            Vector2 vNextNodeInOffset = Vector2.Zero;
            ComputeOffsets( Path.Subpaths[0].NodeIndices[ 0 ], out vNextNodeOutOffset, out vNextNodeInOffset );

            Vector2 vPreviousNodeOutOffset = Vector2.Zero;
            Vector2 vPreviousNodeInOffset = Vector2.Zero;
            
            float fU = 0f;
	        for (int i = 0; i < Path.Subpaths[0].Vertices.Count; i++)
            {
                if( iPathNodeIndex < Path.Subpaths[0].NodeIndices.Count && i >= Path.Subpaths[0].NodeIndices[ iPathNodeIndex % Path.Subpaths[0].NodeIndices.Count ] )
                {
                    iPathNodeIndex++;
                    
                    vPreviousNodeOutOffset = vNextNodeOutOffset;
                    vPreviousNodeInOffset = vNextNodeInOffset;

                    ComputeOffsets( Path.Subpaths[0].NodeIndices[ iPathNodeIndex % Path.Subpaths[0].NodeIndices.Count ], out vNextNodeOutOffset, out vNextNodeInOffset );
                }

                Vector2 vOutOffset;
                Vector2 vInOffset;
                ComputeOffsets( i, out vOutOffset, out vInOffset );

                //--------------------------------------------------------------
                // Fix overlapping strokes

                // This is done by checking for intersection with the next and previous path node's stroke vertices
                float fDistanceBefore = (vInOffset - vOutOffset).Length();
                float fOutV = 0f;
                float fInV = 1f;
                {
                    Vector2 A = vInOffset;
                    Vector2 B = vOutOffset;

                    Vector2 C = vNextNodeInOffset;
                    Vector2 D = vNextNodeOutOffset;

                    Vector2 E = B - A;
                    Vector2 F = D - C;
                    Vector2 P = new Vector2( -E.Y, E.X );
                    float h = ( Vector2.Dot( (A - C), P ) / Vector2.Dot( F, P ) );

                    if( 0f < h && h < 1f )
                    {
                        if( 0.5f < h )
                        {
                            vOutOffset = C + F * h;
                            fOutV = 1f - (vInOffset - vOutOffset).Length() / fDistanceBefore;
                        }
                        else
                        {
                            vInOffset = C + F * h;
                            fInV = (vInOffset - vOutOffset).Length() / fDistanceBefore;
                        }
                    }
                }

                {
                    Vector2 A = vInOffset;
                    Vector2 B = vOutOffset;

                    Vector2 C = vPreviousNodeInOffset;
                    Vector2 D = vPreviousNodeOutOffset;

                    Vector2 E = B - A;
                    Vector2 F = D - C;
                    Vector2 P = new Vector2( -E.Y, E.X );
                    float h = ( Vector2.Dot( (A - C), P ) / Vector2.Dot( F, P ) );

                    if( 0f < h && h < 1f )
                    {
                        if( 0.5f < h )
                        {
                            vOutOffset = C + F * h;
                            fOutV = 1f - (vInOffset - vOutOffset).Length() / fDistanceBefore;
                        }
                        else
                        {
                            vInOffset = C + F * h;
                            fInV = (vInOffset - vOutOffset).Length() / fDistanceBefore;
                        }
                    }
                }

                //--------------------------------------------------------------
                int nextI = (i + 1) % Path.Subpaths[0].Vertices.Count;
                Vector2 vDir = ( Path.Subpaths[0].Vertices[ nextI ] - Path.Subpaths[0].Vertices[ i ] );
                vDir.Normalize();
                Vector2 vTrucOut = vOutOffset - Path.Subpaths[0].Vertices[ i ];
                Vector2 vTrucIn = vInOffset - Path.Subpaths[0].Vertices[ i ];
                float fOutU = fU; // + Vector2.Dot( vTrucOut, vDir ) / 72f;
                float fInU = fU; // + Vector2.Dot( vTrucIn, vDir  ) / 72f;
                aStrokeVertices[ i * 2 ] = new VertexPositionColorTexture( new Vector3( vOutOffset, _fDepth ), Path.StrokeColor, new Vector2( fOutU, fOutV ) );
                aStrokeVertices[ i * 2 + 1 ] = new VertexPositionColorTexture( new Vector3( vInOffset, _fDepth ), Path.StrokeColor, new Vector2( fInU, fInV ) );

                fU += (Path.Subpaths[0].Vertices[ i ] - Path.Subpaths[0].Vertices[ nextI ]).Length() / 72f;
            }

            aStrokeVertices[ Path.Subpaths[0].Vertices.Count * 2 ] = 
                new VertexPositionColorTexture( aStrokeVertices[ 0 ].Position, Path.StrokeColor, new Vector2( fU, aStrokeVertices[ 0 ].TextureCoordinate.Y ) );
            aStrokeVertices[ Path.Subpaths[0].Vertices.Count * 2 + 1 ] =
                new VertexPositionColorTexture( aStrokeVertices[ 1 ].Position, Path.StrokeColor, new Vector2( fU, aStrokeVertices[ 1 ].TextureCoordinate.Y ) );

            mStrokeVertexBuffer = new VertexBuffer( LevelRenderer.Game.GraphicsDevice, VertexPositionColorTexture.VertexDeclaration, aStrokeVertices.Length, BufferUsage.WriteOnly );
            mStrokeVertexBuffer.SetData( aStrokeVertices );

            //------------------------------------------------------------------
            // Create Stroke IndexBuffer
			short[] aStrokeIndices = new short[ iStrokePrimitivesCount * 3 ];
            
	        for (int i = 0; i < Path.Subpaths[0].Vertices.Count + ( Path.Subpaths[0].IsClosed ? 0 : -1 ); i++)
            {
                int nextI = i + 1;

                int iOutVertex = i * 2;
                int iOutNextVertex = nextI * 2;

                int iInVertex = i * 2 + 1;
                int iInNextVertex = nextI * 2 + 1;

                aStrokeIndices[ i * 2 * 3 ] = (short)iOutNextVertex;
                aStrokeIndices[ i * 2 * 3 + 1 ] = (short)iInVertex;
                aStrokeIndices[ i * 2 * 3 + 2 ] = (short)iOutVertex;

                aStrokeIndices[ i * 2 * 3 + 3 ] = (short)iInNextVertex;
                aStrokeIndices[ i * 2 * 3 + 4 ] = (short)iInVertex;
                aStrokeIndices[ i * 2 * 3 + 5 ] = (short)iOutNextVertex;
            }

            mStrokeIndexBuffer = new IndexBuffer( LevelRenderer.Game.GraphicsDevice, IndexElementSize.SixteenBits, aStrokeIndices.Length, BufferUsage.WriteOnly );
            mStrokeIndexBuffer.SetData( aStrokeIndices );
        }

        //----------------------------------------------------------------------
        // Compute inside and outside offsets for stroke rendering
        private void ComputeOffsets( int _iVertex, out Vector2 _vOutOffset, out Vector2 _vInOffset )
        {
            int i = _iVertex;
            int prevI = (i > 0) ? (i - 1) : (Path.Subpaths[0].Vertices.Count - 1);
            int nextI = (i + 1) % Path.Subpaths[0].Vertices.Count;

            // Direction vector from previous vertex to current vertex
            Vector2 vNormPrev = Path.Subpaths[0].Vertices[ prevI ] - Path.Subpaths[0].Vertices[ i ];
            vNormPrev.Normalize();

            // Direction vector from next vertex to current vertex
            Vector2 vNormNext = Path.Subpaths[0].Vertices[ i ] - Path.Subpaths[0].Vertices[ nextI ];
            vNormNext.Normalize();

            //------------------------------------------------------------------
            // Compute outside offset

            // Offset the previous and next directions on the ouside
            Vector2 vOutPrev = new Vector2( -vNormPrev.Y, vNormPrev.X) * Path.StrokeWidth / 2f;
            Vector2 vOutNext = new Vector2(-vNormNext.Y, vNormNext.X) * Path.StrokeWidth / 2f;
            
            // Compute offset vertices
            Vector2 vOutPrevP1 = Path.Subpaths[0].Vertices[ prevI ] + vOutPrev;
            Vector2 vOutPrevP2 = Path.Subpaths[0].Vertices[ i ] + vOutPrev;
            
            Vector2 vOutNextP1 = Path.Subpaths[0].Vertices[ nextI ] + vOutNext;
            Vector2 vOutNextP2 = Path.Subpaths[0].Vertices[ i ] + vOutNext;

            // Find interesection between the two offset vertices
            float fOutAPrev = vOutPrevP2.Y - vOutPrevP1.Y;
            float fOutBPrev = vOutPrevP1.X - vOutPrevP2.X;
            float fOutCPrev = fOutAPrev * vOutPrevP1.X + fOutBPrev * vOutPrevP1.Y;

            float fOutANext = vOutNextP2.Y - vOutNextP1.Y;
            float fOutBNext = vOutNextP1.X - vOutNextP2.X;
            float fOutCNext = fOutANext * vOutNextP1.X + fOutBNext * vOutNextP1.Y;
            
            // We've got our new outside offset vertex
            Vector2 vOutOffset;

            float fOutDet = fOutAPrev * fOutBNext - fOutANext * fOutBPrev;
            if( Math.Abs( fOutDet ) <= 0.01f )
            {
                // Parallel
                vOutOffset = Path.Subpaths[0].Vertices[ i ] + vOutPrev; // or vOutNext, they are the same!
            }
            else
            {
                vOutOffset = new Vector2( (fOutBNext * fOutCPrev - fOutBPrev * fOutCNext) / fOutDet, (fOutAPrev * fOutCNext - fOutANext * fOutCPrev) / fOutDet );
            }

            _vOutOffset = vOutOffset;

            //------------------------------------------------------------------
            // Compute inside offset

            // Offset the previous and next directions on the inside
            Vector2 vInPrev = new Vector2(vNormPrev.Y, -vNormPrev.X) * Path.StrokeWidth / 2f;
            Vector2 vInNext = new Vector2(vNormNext.Y, -vNormNext.X) * Path.StrokeWidth / 2f;

            // Compute offset vertices
            Vector2 vInPrevP1 = Path.Subpaths[0].Vertices[ prevI ] + vInPrev;
            Vector2 vInPrevP2 = Path.Subpaths[0].Vertices[ i ] + vInPrev;
            
            Vector2 vInNextP1 = Path.Subpaths[0].Vertices[ nextI ] + vInNext;
            Vector2 vInNextP2 = Path.Subpaths[0].Vertices[ i ] + vInNext;

            // Find interesection between the two offset vertices
            float fInAPrev = vInPrevP2.Y - vInPrevP1.Y;
            float fInBPrev = vInPrevP1.X - vInPrevP2.X;
            float fInCPrev = fInAPrev * vInPrevP1.X + fInBPrev * vInPrevP1.Y;
            
            float vInANext = vInNextP2.Y - vInNextP1.Y;
            float vInBNext = vInNextP1.X - vInNextP2.X;
            float vInCNext = vInANext * vInNextP1.X + vInBNext * vInNextP1.Y;
            
            // We've got our new inside offset vertex
            Vector2 vInOffset;

            float in_det = fInAPrev * vInBNext - vInANext * fInBPrev;
            if( Math.Abs( in_det ) <= 0.01 )
            {
                // Parallel
                vInOffset = Path.Subpaths[0].Vertices[ i ] + vInPrev; // or vInNext, they are the same!
            }
            else
            {
                vInOffset = new Vector2( (vInBNext * fInCPrev - fInBPrev * vInCNext) / in_det, (fInAPrev * vInCNext - vInANext * fInCPrev) / in_det );
            }

            _vInOffset = vInOffset;
        }

        //----------------------------------------------------------------------
        public void DrawFill()
        {
            if( ! Path.Subpaths[0].IsClosed )
            {
                return;
            }

            GraphicsDevice graphicsDevice = LevelRenderer.Game.GraphicsDevice;

            graphicsDevice.Indices = mFillIndexBuffer;
            graphicsDevice.SetVertexBuffer( mFillVertexBuffer );
            
            graphicsDevice.DrawIndexedPrimitives(
                PrimitiveType.TriangleList,
                0, 0,
                Path.MeshVertices.Length,
                0,
                Path.MeshIndices.Length / 3 );
        }

        //----------------------------------------------------------------------
        public void DrawStroke()
        {
            int iStrokePrimitivesCount  = ( Path.Subpaths[0].Vertices.Count + (Path.Subpaths[0].IsClosed ? 0 : -1) ) * 2;

            GraphicsDevice graphicsDevice = LevelRenderer.Game.GraphicsDevice;

            graphicsDevice.Indices = mStrokeIndexBuffer;
            graphicsDevice.SetVertexBuffer( mStrokeVertexBuffer );
            graphicsDevice.DrawIndexedPrimitives(
                PrimitiveType.TriangleList,
                0, 0,
                (Path.Subpaths[0].Vertices.Count + 1) * 2,
                0,
                iStrokePrimitivesCount );
        }

        #region Shadow rendering

        //----------------------------------------------------------------------
        public void SetupShadowRendering()
        {
            BackFacing          = new bool[ Path.Subpaths[0].Vertices.Count ];
            StartingVertices    = new int[ Path.Subpaths[0].Vertices.Count ];
            ShadowVertices      = new VertexPositionColor[ Path.Subpaths[0].Vertices.Count * 2 ];
        }

        //----------------------------------------------------------------------
        void DrawShadowStrip( Light _light, int _startingIndex, int _iBufferIndex, int _iShadowVertexCount )
        {
            // Create a triangle strip that has the shape of the shadow
            int currentIndex = _startingIndex;
            int svCount = 0;
            while( svCount != _iShadowVertexCount )
            {
                Vector3 vertexPos = new Vector3( Path.Subpaths[0].Vertices[currentIndex], 0f );
                Vector3 lightPos = new Vector3( _light.Position, 0 );
                
                // One vertex on the hull
                ShadowVertices[ _iBufferIndex + svCount ] = new VertexPositionColor();
                ShadowVertices[ _iBufferIndex + svCount ].Color = Color.Transparent;
                ShadowVertices[ _iBufferIndex + svCount ].Position = vertexPos;

                // One extruded by the light direction
                ShadowVertices[ _iBufferIndex + svCount + 1] = new VertexPositionColor();
                ShadowVertices[ _iBufferIndex + svCount + 1].Color = Color.Transparent;
                Vector3 L2P = vertexPos - lightPos;
                L2P.Normalize();
                ShadowVertices[ _iBufferIndex + svCount + 1].Position = lightPos + L2P * 100000f;
                
                svCount += 2;
                currentIndex = (currentIndex + 1) % Path.Subpaths[0].Vertices.Count;
            }
        }

        //----------------------------------------------------------------------
        public void DrawShadow( Light _light )
        {
            // Quick AABB test to avoid drawing shadows that won't affect the light texture
            if( ! AABB.Contains( _light.Position, _light.Range ) )
            {
                return;
            }

            // Compute light facing for each edge
            int vertexCount = Path.Subpaths[0].Vertices.Count;

            Vector2 N = new Vector2();
            
            for( int i = 0; i < vertexCount; i++ )
            {
                Vector2 firstVertex = Path.Subpaths[0].Vertices[i];
                int secondIndex = ( i + 1 ) % vertexCount;
                Vector2 secondVertex = Path.Subpaths[0].Vertices[secondIndex];

                Vector2 middle = (firstVertex + secondVertex) / 2;
                
                Vector2 L = _light.Position - middle;

                N.X = - (secondVertex.Y - firstVertex.Y);
                N.Y = secondVertex.X - firstVertex.X;
                
                BackFacing[i] = ! ( Vector2.Dot(N, L) > 0 );
            }

            // Find all vertices that start a new shadow strip
            NbStartingVertices = 0;
            for( int i = 0; i < vertexCount; i++ )
            {
                int currentEdge = i;
                int nextEdge = (i + 1) % vertexCount;

                if( !BackFacing[currentEdge] && BackFacing[nextEdge])
                {
                    StartingVertices[ NbStartingVertices ] = nextEdge;
                    NbStartingVertices++;
                }
            }

            // Render each shadow strip
            int iVertexIndex = 0;
            for( int iStartingVertex = 0; iStartingVertex < NbStartingVertices; iStartingVertex++ )
            {
                int startingVertexIndex = StartingVertices[ iStartingVertex ];

                for( int i = 0; i < vertexCount; i++ )
                {
                    int currentEdge = ( startingVertexIndex + i ) % vertexCount;
                    int nextEdge = (currentEdge + 1) % vertexCount;

                    if( BackFacing[currentEdge] && !BackFacing[nextEdge])
                    {
                        int iShadowVertexCount;
                        if( nextEdge > startingVertexIndex )
                        {
                            iShadowVertexCount = nextEdge - startingVertexIndex + 1;
                        }
                        else
                        {
                            iShadowVertexCount = Path.Subpaths[0].Vertices.Count + 1 - startingVertexIndex + nextEdge;
                        }

                        DrawShadowStrip( _light, startingVertexIndex, iVertexIndex, iShadowVertexCount * 2 );
                        LevelRenderer.Game.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>( PrimitiveType.TriangleStrip, ShadowVertices, iVertexIndex, ( iShadowVertexCount - 1 ) * 2 );
                        iVertexIndex += iShadowVertexCount * 2;
                        break;
                    }
                }
            }
        }

        #endregion

        //----------------------------------------------------------------------
        public Entities.Path            Path;
        public NuclearWinter.AABB       AABB;

        //----------------------------------------------------------------------
        // Rendering
        LevelRenderer                   LevelRenderer;

        VertexBuffer                    mFillVertexBuffer;
		IndexBuffer                     mFillIndexBuffer;
        VertexBuffer                    mStrokeVertexBuffer;
        IndexBuffer                     mStrokeIndexBuffer;

        // Shadow rendering
        public bool[]                   BackFacing;
        public int[]                    StartingVertices;
        public int                      NbStartingVertices;
        public VertexPositionColor[]    ShadowVertices;
    }
}