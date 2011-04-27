using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using System.Xml;
using System.Globalization;

using VectorLevel;
using VectorLevel.Entities;

using TInput = VectorLevelProcessor.XMLFile;
using TOutput = VectorLevel.LevelDesc;

namespace VectorLevelProcessor
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to apply custom processing to content data, converting an object of
    /// type TInput to TOutput. The input and output types may be the same if
    /// the processor wishes to alter data without changing its type.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    /// </summary>
    [ContentProcessor(DisplayName = "Vector Level Processor")]
    public class VectorLevelProcessor: ContentProcessor<TInput, TOutput>
    {
        //----------------------------------------------------------------------
        /// <summary>
        /// Process SVG Level
        /// </summary>
        /// <param name="_input"></param>
        /// <param name="_context"></param>
        /// <returns></returns>
        public override TOutput Process( TInput _input, ContentProcessorContext _context )
        {
            //------------------------------------------------------------------
            // Create level desc
            mLevelDesc = new LevelDesc();
            mLevelDesc.Title = System.IO.Path.GetFileNameWithoutExtension( _input.Filepath ); // Fallback if no name is specified in RDF meta-data

            mMatrixStack = new Stack<Matrix>();
            mMatrixStack.Push( Matrix.Identity );

            mGroupStack = new Stack<Group>();
            mGroupStack.Push( mLevelDesc.Root );

            //------------------------------------------------------------------
            // Traverse the SVG document
            mXmlReader = _input.XmlReader;
            mXmlPathStack = new Stack<string>();

            while( mXmlReader.Read() )
            {
                switch( mXmlReader.NodeType )
                {
                    case XmlNodeType.Document:
                        break;
                    case XmlNodeType.Element:
                        StartElement();
                        break;
                    case XmlNodeType.EndElement:
                        EndElement();
                        break;
                    case XmlNodeType.Text:
                        Characters();
                        break;
                }
            }

            //------------------------------------------------------------------
            // Ensure we've got the expected matrix & xml stack depth
            if( mMatrixStack.Count != 1 )
            {
                throw new Exception( String.Format( "Matrix stack should have depth 1, found {0} instead", mMatrixStack.Count.ToString() ) );
            }

            if( mXmlPathStack.Count != 0 )
            {
                throw new Exception( String.Format( "XML path stack should have depth 0, found {0} instead", mXmlPathStack.Count.ToString() ) );
            }
            
            return mLevelDesc;
        }

        //----------------------------------------------------------------------
        /// <summary>
        /// Callback for XML element start
        /// </summary>
        /// <param name="mXmlReader"></param>
        private void StartElement()
        {
            mXmlPathStack.Push( mXmlReader.Name );
            
            //------------------------------------------------------------------
            // New Matrix to be used for enclosed elements
            Matrix newMatrix = Matrix.Identity * mMatrixStack.Peek();

            string strTransform = mXmlReader.GetAttribute( "transform" );
            if( null != strTransform )
            {
                newMatrix = ComputeMatrixFromTransform( strTransform ) * newMatrix; 
            }

            mMatrixStack.Push( newMatrix );
            
            //------------------------------------------------------------------
            if( mXmlPathStack.Count == 1 )
            {
                //--------------------------------------------------------------
                // SVG Root element
                if( mXmlPathStack.Peek() == "svg" )
                {
                    mLevelDesc.MapWidth = (UInt32)float.Parse( mXmlReader.GetAttribute( "width" ), CultureInfo.InvariantCulture );
                    mLevelDesc.MapHeight = (UInt32)float.Parse( mXmlReader.GetAttribute("height"), CultureInfo.InvariantCulture );
                }
            }
            
            else
            {
                //--------------------------------------------------------------
                // Group element
                if( mXmlPathStack.Peek() == "g" )
                {
                    Group group;

                    string strGroupMode = mXmlReader.GetAttribute( "groupmode", "http://www.inkscape.org/namespaces/inkscape" );
                    if( strGroupMode == "layer" )
                    {
                        //------------------------------------------------------
                        // Inkscape Layer, use the label rather than the id
                        string strLayerName = mXmlReader.GetAttribute( "label", "http://www.inkscape.org/namespaces/inkscape" );
                        group = new Group( strLayerName, mGroupStack.Peek(), GroupMode.Layer );
                    }
                    else
                    {
                        //------------------------------------------------------
                        // Any other kind of group
                        group = new VectorLevel.Entities.Group( mXmlReader.GetAttribute( "id" ), mGroupStack.Peek(), GroupMode.Group );
                    }

                    mGroupStack.Push( group );
                    mLevelDesc.Entities[ group.Name ] = group;
                    mLevelDesc.OrderedEntities.Add( group );
                }
                else

                //--------------------------------------------------------------
                // Path / Rect element
                if( mXmlPathStack.Peek() == "path" || mXmlPathStack.Peek() == "rect" )
                {
                    VectorLevel.Entities.Path path = ReadPath();
                    mLevelDesc.Entities[ path.Name ] = path;
                    mLevelDesc.OrderedEntities.Add( path );
                }
                else
                //--------------------------------------------------------------
                // Image element
                if( mXmlPathStack.Peek() == "image" )
                {
                    VectorLevel.Entities.Marker marker = ReadMarker();
                    mLevelDesc.Entities[ marker.Name ] = marker;
                    mLevelDesc.OrderedEntities.Add( marker );
                }
                else
                //--------------------------------------------------------------
                // Text element
                if( mXmlPathStack.Peek() == "text" )
                {
                    VectorLevel.Entities.Text text = ReadText();
                    mLevelDesc.Entities[ text.Name ] = text;
                    mLevelDesc.OrderedEntities.Add( text );
                }
            }

            if( mXmlReader.IsEmptyElement )
            {
                mMatrixStack.Pop();
                mXmlPathStack.Pop();

                if( mXmlReader.LocalName == "g" )
                {
                    mGroupStack.Pop();
                }
            }
        }

        //----------------------------------------------------------------------
        /// <summary>
        /// Callback for XML element end
        /// </summary>
        /// <param name="mXmlReader"></param>
        private void EndElement()
        {
            //------------------------------------------------------------------
            if( mXmlReader.LocalName == "title" && mXmlReader.NamespaceURI == "http://purl.org/dc/elements/1.1/" )
            {
                // Dublin Core - document title
                mLevelDesc.Title = mstrData;
            }
            else
            if( mXmlReader.LocalName == "description" && mXmlReader.NamespaceURI == "http://purl.org/dc/elements/1.1/" )
            {
                // Dublin Core - document description
                mLevelDesc.Description = mstrData;
            }
            else
            if( mXmlReader.LocalName == "g" )
            {
                mGroupStack.Pop();
            }
            else
            if( mXmlReader.LocalName == "tspan" )
            {
                if( mstrData != "" )
                {
                    VectorLevel.Entities.Text text = mLevelDesc.OrderedEntities[mLevelDesc.OrderedEntities.Count - 1] as VectorLevel.Entities.Text;
                    text.TextSpans.Add( new VectorLevel.Entities.TextSpan( mstrData ) );
                }
            }

            mXmlPathStack.Pop();
            mstrData = "";

            mMatrixStack.Pop();
        }

        //----------------------------------------------------------------------
        /// <summary>
        /// Callback for XML character data
        /// </summary>
        /// <param name="mXmlReader"></param>
        private void Characters()
        {
            mstrData += mXmlReader.Value;
        }

        //----------------------------------------------------------------------
        internal Matrix ComputeMatrixFromTransform( string _strTransform )
        {
            Matrix newMatrix = Matrix.Identity;
            string strData = "";

            TransformType? transformType = null;

            foreach( char cChar in _strTransform )
            {
                if( cChar == '(' )
                {
                    // Which transform is this?
                    switch( strData.ToLower() )
                    {
                        case "matrix":
                            transformType = TransformType.Matrix;
                            break;
                        case "translate":
                            transformType = TransformType.Translate;
                            break;
                        case "scale":
                            transformType = TransformType.Scale;
                            break;
                        case "rotate":
                            transformType = TransformType.Rotate;
                            break;
                        case "skewx":
                            transformType = TransformType.SkewX;
                            break;
                        case "skewy":
                            transformType = TransformType.SkewY;
                            break;
                    }

                    strData = "";
                }
                else
                if( cChar == ')' )
                {
                    if( ! transformType.HasValue )
                    {
                        throw new Exception( String.Format( "Error while parsing transform attribute: <{0} id=\"{1}\">", mXmlReader.Name, mXmlReader.GetAttribute( "id" ) ) );
                    }

                    string[] astrTokens = strData.Split( ", ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries );

                    switch( transformType )
                    {
                        case TransformType.Matrix:
                            if( astrTokens.Length != 6 ) throw new Exception( String.Format( "Matrix transform type expects 6 values, found {0}", astrTokens.Length ) );

                            Matrix transformMatrix = Matrix.Identity;

                            transformMatrix.M11 = float.Parse( astrTokens[0], CultureInfo.InvariantCulture );
                            transformMatrix.M21 = float.Parse( astrTokens[1], CultureInfo.InvariantCulture );
                            transformMatrix.M12 = float.Parse( astrTokens[2], CultureInfo.InvariantCulture );
                            transformMatrix.M22 = float.Parse( astrTokens[3], CultureInfo.InvariantCulture );
                            transformMatrix.M13 = float.Parse( astrTokens[4], CultureInfo.InvariantCulture );
                            transformMatrix.M23 = float.Parse( astrTokens[5], CultureInfo.InvariantCulture );

                            transformMatrix.M31 = 0;
                            transformMatrix.M32 = 0;
                            transformMatrix.M33 = 1;
                            
                            newMatrix = Matrix.CreateScale( new Vector3( 1f, -1f, 1f ) ) * transformMatrix * Matrix.CreateScale( new Vector3( 1f, -1f, 1f ) ) * newMatrix;
                            break;
                        case TransformType.Translate:
                            if( astrTokens.Length != 2 ) throw new Exception( String.Format( "Translate transform type expects 2 values, found {0}", astrTokens.Length ) );
                            
                            Vector3 vTranslation = new Vector3( float.Parse( astrTokens[0], CultureInfo.InvariantCulture ), float.Parse( astrTokens[1], CultureInfo.InvariantCulture ), 0 );
                            newMatrix = Matrix.CreateTranslation( vTranslation ) * newMatrix;
                            break;

                        case TransformType.Rotate:
                            if( astrTokens.Length != 1 ) throw new Exception( String.Format( "Rotate transform type expects 1 value, found {0}", astrTokens.Length ) );

                            float fAngle = float.Parse( astrTokens[0], CultureInfo.InvariantCulture );
                            newMatrix = Matrix.CreateRotationZ( MathHelper.ToRadians( fAngle ) ) * newMatrix;
                            break;

                        case TransformType.Scale:
                            if( astrTokens.Length != 2 ) throw new Exception( String.Format( "Scale transform type expects 2 values, found {0}", astrTokens.Length ) );
                            
                            Vector3 vScale = new Vector3( float.Parse( astrTokens[0], CultureInfo.InvariantCulture ), float.Parse( astrTokens[1], CultureInfo.InvariantCulture ), 0 );
                            newMatrix = Matrix.CreateScale( vScale ) * newMatrix;
                            break;
                        default:
                            throw new NotImplementedException( String.Format( "SVG Transform type \"{0}\" is not supported", transformType.ToString() ) );
                    }

                    transformType = null;
                    strData = "";
                }
                else
                {
                    strData += cChar;
                }
            }

            return newMatrix;
        }

        //----------------------------------------------------------------------
        internal VectorLevel.Entities.Text ReadText()
        {
            string strTextName      = mXmlReader.GetAttribute( "id" );
            
            Vector2 vPosition = new Vector2(
                float.Parse( mXmlReader.GetAttribute( "x" ), CultureInfo.InvariantCulture ),
                float.Parse( mXmlReader.GetAttribute( "y" ), CultureInfo.InvariantCulture ) );

            float fAngle = (float)Math.Atan2( mMatrixStack.Peek().M12 /* cos angle */, mMatrixStack.Peek().M11 /* sin angle */ );

            VectorLevel.Entities.Text text = new VectorLevel.Entities.Text( strTextName, mGroupStack.Peek(), Vector2.Transform( vPosition, mMatrixStack.Peek() ), fAngle );
            return text;
        }

        //----------------------------------------------------------------------
        internal VectorLevel.Entities.Marker ReadMarker()
        {
            string strMarkerName    = mXmlReader.GetAttribute( "id" );
            string strImagePath     = mXmlReader.GetAttribute( "href", "http://www.w3.org/1999/xlink" );
            string strMarkerType    = System.IO.Path.GetFileNameWithoutExtension( strImagePath );

            //------------------------------------------------------------------
            Vector2 vPosition = new Vector2(
                float.Parse( mXmlReader.GetAttribute( "x" ), CultureInfo.InvariantCulture ),
                float.Parse( mXmlReader.GetAttribute( "y" ), CultureInfo.InvariantCulture ) );

            Vector2 vSize = new Vector2(
                float.Parse( mXmlReader.GetAttribute( "width" ), CultureInfo.InvariantCulture ),
                float.Parse( mXmlReader.GetAttribute( "height" ), CultureInfo.InvariantCulture ) );

            float fAngle = (float)Math.Atan2( mMatrixStack.Peek().M12 /* cos angle */, mMatrixStack.Peek().M11 /* sin angle */ );
            Vector2 vScale = new Vector2(
                    (float)Math.Sqrt( Math.Pow( mMatrixStack.Peek().M11, 2 ) + Math.Pow( mMatrixStack.Peek().M12, 2 ) ),
                    (float)Math.Sqrt( Math.Pow( mMatrixStack.Peek().M21, 2 ) + Math.Pow( mMatrixStack.Peek().M22, 2 ) )
                );

            //------------------------------------------------------------------
            // Read style
            string strStyleAttr = mXmlReader.GetAttribute( "style" );

            Color fillColor = Color.White;
            Color strokeColor = new Color();
            float fStrokeWidth = 0f;
            ReadStyle( strStyleAttr, ref fillColor, ref strokeColor, ref fStrokeWidth );

            //------------------------------------------------------------------
            VectorLevel.Entities.Marker marker = new VectorLevel.Entities.Marker( strMarkerName, mGroupStack.Peek(), strMarkerType, Vector2.Transform( vPosition, mMatrixStack.Peek() ), vSize, fAngle, vScale, fillColor );

            return marker;
        }

        //----------------------------------------------------------------------
        internal void ReadStyle( string _strStyleAttr, ref Color _fillColor, ref Color _strokeColor, ref float _fStrokeWidth )
        {
            _fStrokeWidth = 0f;

            float fFillOpacity = 1f;
            float fStrokeOpacity = 1f;
            bool bHasStroke = false;

            if( _strStyleAttr != null )
            {
                string[] styles = _strStyleAttr.Split( ";".ToCharArray() );
                foreach( string style in styles )
                {
                    string[] keyValPair = style.Split( ":".ToCharArray() );
                    string key = keyValPair[0];
                    string value = keyValPair[1];

                    switch( key )
                    {
                    case "fill":
                        if( value[0] == '#' )
                        {
                            _fillColor = ReadHexColor( value );
                        }
                        else
                        if( value.ToLower() == "none" )
                        {
                            // FIXME: disable fill rendering altogether
                            _fillColor = Color.Transparent;
                            fFillOpacity = 0f;
                        }
                        break;
                    case "stroke":
                        if( value[0] == '#' )
                        {
                            _strokeColor = ReadHexColor( value );
                            bHasStroke = true;
                        }
                        break;
                    case "fill-opacity":
                        {
                        float fOpacity = float.Parse( value, CultureInfo.InvariantCulture );
                        fFillOpacity *= fOpacity;
                        break;
                        }
                    case "opacity":
                        {
                        float fOpacity = float.Parse( value, CultureInfo.InvariantCulture );
                        fStrokeOpacity *= fOpacity;
                        fFillOpacity = fOpacity;
                        break;
                        }
                    case "stroke-opacity":
                        {
                        float fOpacity = float.Parse( value, CultureInfo.InvariantCulture );
                        fStrokeOpacity *= fOpacity;
                        break;
                        }
                    case "stroke-width":
                        {
                        value = value.Replace("px", "");
                        _fStrokeWidth = float.Parse( value, CultureInfo.InvariantCulture );
                        break;
                        }
                    }
                }
            }

            _fillColor *= fFillOpacity;
            if( ! bHasStroke )
            {
                _fStrokeWidth = 0f;
            }
            _strokeColor *= fStrokeOpacity;
        }

        //----------------------------------------------------------------------
        internal VectorLevel.Entities.Path ReadPath()
        {
            VectorLevel.Entities.Path path = new VectorLevel.Entities.Path( mXmlReader.GetAttribute( "id" ), mGroupStack.Peek() );

            //------------------------------------------------------------------
            // Read style
            string strStyleAttr = mXmlReader.GetAttribute( "style" );
            ReadStyle( strStyleAttr, ref path.FillColor, ref path.StrokeColor, ref path.StrokeWidth );

            //------------------------------------------------------------------
            // Read contours
            if( mXmlPathStack.Peek() == "rect" )
            {
                float fX = float.Parse( mXmlReader.GetAttribute( "x" ), CultureInfo.InvariantCulture );
                float fY = float.Parse( mXmlReader.GetAttribute( "y" ), CultureInfo.InvariantCulture );

                float fWidth = float.Parse( mXmlReader.GetAttribute( "width" ), CultureInfo.InvariantCulture );
                float fHeight = float.Parse( mXmlReader.GetAttribute( "height" ), CultureInfo.InvariantCulture );

                VectorLevel.Entities.Subpath subpath = new VectorLevel.Entities.Subpath();

                subpath.PathNodes.Add( new VectorLevel.Entities.PathNode( VectorLevel.Entities.PathNode.NodeType.MoveTo, Vector2.Transform( new Vector2( fX, fY ), mMatrixStack.Peek() ) ) );
                subpath.PathNodes.Add( new VectorLevel.Entities.PathNode( VectorLevel.Entities.PathNode.NodeType.LineTo, Vector2.Transform( new Vector2( fX + fWidth, fY ), mMatrixStack.Peek() ) ) );
                subpath.PathNodes.Add( new VectorLevel.Entities.PathNode( VectorLevel.Entities.PathNode.NodeType.LineTo, Vector2.Transform( new Vector2( fX + fWidth, fY + fHeight ), mMatrixStack.Peek() ) ) );
                subpath.PathNodes.Add( new VectorLevel.Entities.PathNode( VectorLevel.Entities.PathNode.NodeType.LineTo, Vector2.Transform( new Vector2( fX, fY + fHeight ), mMatrixStack.Peek() ) ) );
                subpath.IsClosed = true;

                path.Subpaths.Add( subpath );
            }
            else
            {
                string strPathData = mXmlReader.GetAttribute( "d" );
                ParseSVGPathData( path, strPathData );
            }

            return path;
        }

        //----------------------------------------------------------------------
        /// <summary>
        /// Parse SVG Path data
        /// </summary>
        /// <param name="_strPathData"></param>
        /// <param name="_strPathID"></param>
        /// <returns></returns>
        internal void ParseSVGPathData( VectorLevel.Entities.Path _path, string _strPathData )
        {
            Vector2 vCurrentPosition = Vector2.Zero;
            
            string[] astrTokens = _strPathData.Split( " ,".ToCharArray() );
            string strImplicitCommand = "";

            VectorLevel.Entities.Subpath currentSubpath = null;

            int iTokenIndex = -1;
            while( iTokenIndex < astrTokens.Length - 1 )
            {
                iTokenIndex++;
                string strToken = astrTokens[ iTokenIndex ];

                //--------------------------------------------------------------
                // Start a new subpath
                if( ( strToken == "M" ) || ( strToken == "m" ) )
                {
                    currentSubpath = new VectorLevel.Entities.Subpath();
                    _path.Subpaths.Add( currentSubpath );

                    // Read vertex and add it to subpath
                    Vector2 vertex = ParseSVGPathVertex( astrTokens[ iTokenIndex + 1 ], astrTokens[ iTokenIndex + 2 ] );
                    iTokenIndex += 2;

                    if( strToken == "m" )
                    {
                        // Relative
                        vertex += vCurrentPosition;
                    }

                    currentSubpath.PathNodes.Add( new VectorLevel.Entities.PathNode( VectorLevel.Entities.PathNode.NodeType.MoveTo, Vector2.Transform( vertex, mMatrixStack.Peek() ) ) );

                    vCurrentPosition = vertex;
                    strImplicitCommand = (strToken == "M" ) ? "L" : "l";
                    continue;
                }
                else

                //--------------------------------------------------------------
                // Line to
                if( ( strToken == "L" ) || ( strToken == "l" ) )
                {
                    iTokenIndex++;
                    strImplicitCommand = strToken;
                }
                else

                //--------------------------------------------------------------
                // Close subpath
                if( ( strToken == "Z" ) || ( strToken == "z" ) )
                {
                    currentSubpath.IsClosed = true;

                    currentSubpath = null;
                    strImplicitCommand = "";
                    continue;
                }
                else

                //--------------------------------------------------------------
                // Horizontal / Vertical
                if( ( strToken == "H" ) || ( strToken == "h" ) || ( strToken == "V" ) || ( strToken == "v" ) )
                {
                    throw new NotSupportedException( "Horizontal and vertical commands are not supported (path \"" + _path.Name + "\")" );
                }
                else

                //--------------------------------------------------------------
                // Cubic Bézier curve
                if( ( strToken == "C" ) || ( strToken == "c" ) )
                {
                    iTokenIndex++;
                    strImplicitCommand = strToken;
                }
                else

                //--------------------------------------------------------------
                // Smooth cubic Bézier curve
                if( ( strToken == "S" ) || ( strToken == "s" ) )
                {
                    throw new NotSupportedException( "Smooth cubic Bézier curves are not supported (path \"" + _path.Name + "\")" );
                }
                else

                //--------------------------------------------------------------
                // Quadratic Bézier curve
                if( ( strToken == "Q" ) || ( strToken == "q" ) || ( strToken == "T" ) || ( strToken == "t" ) )
                {
                    throw new NotSupportedException( "Quadratic Bézier curves are not supported (path \"" + _path.Name + "\")" );
                }
                else

                //--------------------------------------------------------------
                // Elliptical arc
                if( ( strToken == "A" ) || ( strToken == "a" ) )
                {
                    throw new NotSupportedException( "Elliptical arcs are not supported (path \"" + _path.Name + "\")" );
                }
                
                //--------------------------------------------------------------
                // Actual Cubic Bézier curve parsing
                if( ( strImplicitCommand == "C" ) || (strImplicitCommand == "c" ) )
                {
                    Vector2 vControlPoint1 = ParseSVGPathVertex( astrTokens[ iTokenIndex ], astrTokens[ iTokenIndex + 1 ] );
                    Vector2 vControlPoint2 = ParseSVGPathVertex( astrTokens[ iTokenIndex + 2 ], astrTokens[ iTokenIndex + 3 ] );
                    Vector2 vertex = ParseSVGPathVertex( astrTokens[ iTokenIndex + 4 ], astrTokens[ iTokenIndex + 5 ] );
                    iTokenIndex += 5;
                    
                    if( strImplicitCommand == "c" )
                    {
                        // Relative position
                        vertex += vCurrentPosition;
                        vControlPoint1 += vCurrentPosition;
                        vControlPoint2 += vCurrentPosition;
                    }

                    currentSubpath.PathNodes.Add( new VectorLevel.Entities.PathNode(
                        VectorLevel.Entities.PathNode.NodeType.CurveTo,
                        Vector2.Transform( vertex, mMatrixStack.Peek() ),
                        Vector2.Transform( vControlPoint1, mMatrixStack.Peek() ),
                        Vector2.Transform( vControlPoint2, mMatrixStack.Peek() ) ) );

                    vCurrentPosition = vertex;
                }
                else
                if( ( strImplicitCommand == "L" ) || ( strImplicitCommand == "l" ) )
                {
                    // Read vertex and add it to contour
                    Vector2 vertex = ParseSVGPathVertex( astrTokens[ iTokenIndex ], astrTokens[ iTokenIndex + 1 ] );
                    iTokenIndex++;

                    if( strImplicitCommand == "l" )
                    {
                        // Relative
                        vertex += vCurrentPosition;
                    }

                    currentSubpath.PathNodes.Add( new VectorLevel.Entities.PathNode( VectorLevel.Entities.PathNode.NodeType.LineTo, Vector2.Transform( vertex, mMatrixStack.Peek() ) ) );

                    vCurrentPosition = vertex;
                }
            }
        }

        //----------------------------------------------------------------------
        /// <summary>
        /// Parse SVG Path data vertex
        /// </summary>
        /// <param name="_strData"></param>
        /// <returns></returns>
        private Vector2 ParseSVGPathVertex( string _strX, string _strY )
        {
            return new Vector2( float.Parse( _strX, CultureInfo.InvariantCulture ), float.Parse( _strY, CultureInfo.InvariantCulture ) );
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
        /// Compute the list of points needed to approximate a cubic Bézier curve on the specified interval
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
        /// <returns>The point list</returns>
        private List<Vector2> ComputeCubicBezier( ref Vector2 vStart, ref Vector2 vEnd, ref Vector2 vControlPoint1, ref Vector2 vControlPoint2 )
        {
            return ComputeCubicBezier( ref vStart, ref vEnd, ref vControlPoint1, ref vControlPoint2, 0f, ref vStart, 1f, ref vEnd );
        }

        //----------------------------------------------------------------------
        /// <summary>
        /// Read color from hex string
        /// </summary>
        private Color ReadHexColor( string _value )
        {
            // NOTE: Had to invert red & blue, why?
            byte R, G, B;
            R = byte.Parse( _value.Substring( 5, 2 ), NumberStyles.HexNumber, CultureInfo.InvariantCulture );
            G = byte.Parse( _value.Substring( 3, 2 ), NumberStyles.HexNumber, CultureInfo.InvariantCulture );
            B = byte.Parse( _value.Substring( 1, 2 ), NumberStyles.HexNumber, CultureInfo.InvariantCulture );
            
            return new Color( R, G, B );
        }

        //----------------------------------------------------------------------
        enum TransformType
        {
            Matrix
        ,   Translate
        ,   Scale
        ,   Rotate
        ,   SkewX
        ,   SkewY
        }

        //----------------------------------------------------------------------
        VectorLevel.LevelDesc   mLevelDesc;

        XmlReader                           mXmlReader;
        Stack<string>                       mXmlPathStack;
        Stack<VectorLevel.Entities.Group>   mGroupStack;
        Stack<Matrix>                       mMatrixStack;
        string                              mstrData = "";
        

    }
}