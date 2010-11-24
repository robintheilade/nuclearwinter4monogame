using System;
using System.Collections.Generic;

using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VectorLevel.Entities
{
    //--------------------------------------------------------------------------
    /// <summary>
    /// A path is made of one or more subpaths and some associated properties
    /// 
    /// Paths can be made into static or dynamic blocks to be rendered
    /// and used for physics
    /// 
    /// They can also be used as a level design feature, for instance
    /// moving a platform along a path or handling camera flow
    /// 
    /// Paths can be open or closed
    /// </summary>
    public class Path: Entity
    {
        //----------------------------------------------------------------------
        public Path( string _strName, Group _parent )
        : base( _strName, EntityType.Path, _parent )
        {
            Subpaths = new List<Subpath>();
            
            FillColor       = new Color();
            StrokeColor     = new Color();
            StrokeWidth     = 0f;
        }

        //----------------------------------------------------------------------
        public void Flatten()
        {
            //------------------------------------------------------------------
            // Flatten all subpaths
            foreach( Subpath subpath in Subpaths )
            {
                subpath.Flatten();
            }
            
            //------------------------------------------------------------------
            // Ensure proper winding order
            if( Subpaths.Count == 1 && Subpaths[0].IsClosed )
            {
                // There's only one contour, ensure it has proper winding order
                if( Triangulator.Triangulator.DetermineWindingOrder( Subpaths[0].Vertices.ToArray() ) != Triangulator.WindingOrder.CounterClockwise )
                {
                    for( int i = 0; i < Subpaths[0].NodeIndices.Count; i++ )
                    {
                        if( Subpaths[0].NodeIndices[i] != 0 )
                        {
                            Subpaths[0].NodeIndices[i] = Subpaths[0].Vertices.Count - Subpaths[0].NodeIndices[i];
                        }
                    }

                    Subpaths[0].NodeIndices.Reverse( 1, Subpaths[0].NodeIndices.Count - 1 );
                    Subpaths[0].Vertices.Reverse( 1, Subpaths[0].Vertices.Count - 1 );

                    //Subpaths[0].Vertices = new List<Vector2>( Triangulator.Triangulator.EnsureWindingOrder( Subpaths[0].Vertices.ToArray(), Triangulator.WindingOrder.CounterClockwise ) );
                }
            }

            //------------------------------------------------------------------
            // Cut holes
            Vector2[] avVertices = Subpaths[0].Vertices.ToArray();
            /*
            for( int i = 1; i < Subpaths.Count; i++ )
            {
                avVertices = Triangulator.Triangulator.CutHoleInShape( avVertices, Subpaths[i].Vertices.ToArray() );
            }
            */

            //------------------------------------------------------------------
            // Do actual triangulation
            Triangulator.Triangulator.Triangulate( avVertices, Triangulator.WindingOrder.CounterClockwise, out MeshVertices, out MeshIndices );
        }

        //----------------------------------------------------------------------
        public List<Subpath>            Subpaths;

        public Color                    FillColor;
        public Color                    StrokeColor;
        public float                    StrokeWidth;

        // Flattened path data
        public Vector2[]                MeshVertices;
        public int[]                    MeshIndices;
    }

    //--------------------------------------------------------------------------
    public class Subpath
    {
        //----------------------------------------------------------------------
        public Subpath()
        {
            PathNodes   = new List<PathNode>();
            IsClosed    = false;
        }

        //----------------------------------------------------------------------
        /// <summary>
        /// Compute the point at the specified offset on a cubic Bézier curve
        /// </summary>
        /// <param name="vStart">Start point</param>
        /// <param name="vEnd">End point</param>
        /// <param name="vControlPoint1">First control point</param>
        /// <param name="vControlPoint2">Second control point</param>
        /// <param name="fOffset">Offset on the curve (0f - 1f)</param>
        /// <returns>The point's position</returns>
        private Vector2 ComputeCubicBezierPoint( ref Vector2 vStart, ref Vector2 vEnd, ref Vector2 vControlPoint1, ref Vector2 vControlPoint2, float fOffset )
        {
            float fOneMinusOffset = 1f - fOffset;
            Vector2 preVertex = vStart * fOneMinusOffset + vControlPoint1 * fOffset;
            Vector2 middleVertex = vControlPoint1 * fOneMinusOffset + vControlPoint2 * fOffset;
            Vector2 postVertex = vControlPoint2 * fOneMinusOffset + vEnd * fOffset;

            Vector2 leftVertex = preVertex * fOneMinusOffset + middleVertex * fOffset;
            Vector2 rightVertex = middleVertex * fOneMinusOffset + postVertex * fOffset;
            Vector2 newVertex = leftVertex * fOneMinusOffset + rightVertex * fOffset;

            return newVertex;
        }

        //----------------------------------------------------------------------
        /// <summary>
        /// Compute the list of points needed to approximate a cubic Bézier curve
        /// </summary>
        /// <param name="vStart">Start point</param>
        /// <param name="vEnd">End point</param>
        /// <param name="vControlPoint1">First control point</param>
        /// <param name="vControlPoint2">Second control point</param>
        /// <param name="fOffset">Offset on the curve (0f - 1f)</param>
        /// <returns>The point list</returns>
        private List<Vector2> ComputeCubicBezier( ref Vector2 vStart, ref Vector2 vEnd, ref Vector2  vControlPoint1, ref Vector2 vControlPoint2, float fLeftOffset, ref Vector2 vLeft, float fRightOffset, ref Vector2 vRight )
        {
            List<Vector2> lvPoints = new List<Vector2>();
            
            float fOffset = ( fLeftOffset + fRightOffset ) / 2f;
            Vector2 vNewPoint = ComputeCubicBezierPoint( ref vStart, ref vEnd, ref vControlPoint1, ref vControlPoint2, fOffset );

            if( Vector2.Distance( vNewPoint, ( vLeft + vRight ) / 2f) <= 2f )
            {
                return lvPoints;
            }

            lvPoints.AddRange( ComputeCubicBezier( ref vStart, ref vEnd, ref vControlPoint1, ref vControlPoint2, fLeftOffset, ref vLeft, fOffset, ref vNewPoint ) );
            lvPoints.Add( vNewPoint );
            lvPoints.AddRange( ComputeCubicBezier( ref vStart, ref vEnd, ref vControlPoint1, ref vControlPoint2, fOffset, ref vNewPoint, fRightOffset, ref vRight ) );

            return lvPoints;
        }

        //----------------------------------------------------------------------
        /// <summary>
        /// Compute the list of points needed to approximate a cubic Bézier curve
        /// </summary>
        /// <param name="vStart">Start point</param>
        /// <param name="vEnd">End point</param>
        /// <param name="vControlPoint1">First control point</param>
        /// <param name="vControlPoint2">Second control point</param>
        /// <param name="fOffset">Offset on the curve (0f - 1f)</param>
        /// <returns>The point list</returns>
        private List<Vector2> ComputeCubicBezier( Vector2 vStart, Vector2 vEnd, Vector2 vControlPoint1, Vector2 vControlPoint2 )
        {
            return ComputeCubicBezier( ref vStart, ref vEnd, ref vControlPoint1, ref vControlPoint2, 0f, ref vStart, 1f, ref vEnd );
        }

        //----------------------------------------------------------------------
        public void Flatten()
        {
            Vertices = new List<Vector2>();
            NodeIndices = new List<int>();

            foreach( PathNode node in PathNodes )
            {
                switch( node.Type )
                {
                    case PathNode.NodeType.MoveTo:
                    case PathNode.NodeType.LineTo:
                        Vertices.Add( node.Position );
                        break;
                    case PathNode.NodeType.CurveTo:
                        Vertices.AddRange( ComputeCubicBezier( Vertices[ Vertices.Count - 1 ], node.Position, node.ControlPoint1, node.ControlPoint2 ) );
                        Vertices.Add( node.Position );
                        break;
                }

                NodeIndices.Add( Vertices.Count - 1 );
            }
            
            if( (Vertices[0] - Vertices[ Vertices.Count - 1 ]).Length() < 0.1f )
            {
                // Remove duplicated vertex
                Vertices.RemoveAt( Vertices.Count - 1 );
                NodeIndices.RemoveAt( NodeIndices.Count - 1 );
            }
        }

        //----------------------------------------------------------------------
        public bool                 IsClosed;
        public List<PathNode>       PathNodes   { get; private set; }
        public List<Vector2>        Vertices;
        public List<int>            NodeIndices;
    }

    //--------------------------------------------------------------------------
    public struct PathNode
    {
        //----------------------------------------------------------------------
        public PathNode( NodeType _type, Vector2 _vPosition )
        {
            Type            = _type;
            Position        = _vPosition;
            ControlPoint1   = Vector2.Zero;
            ControlPoint2   = Vector2.Zero;
        }

        //----------------------------------------------------------------------
        public PathNode( NodeType _type, Vector2 _vPosition, Vector2 _vControlPoint1, Vector2 _vControlPoint2 )
        {
            Type            = _type;
            Position        = _vPosition;
            ControlPoint1   = _vControlPoint1;
            ControlPoint2   = _vControlPoint2;
        }

        //----------------------------------------------------------------------
        public enum NodeType
        {
            MoveTo,
            LineTo,
            CurveTo,
        };

        //----------------------------------------------------------------------
        public NodeType     Type;
        public Vector2      Position;
        public Vector2      ControlPoint1;
        public Vector2      ControlPoint2;
    }
}
