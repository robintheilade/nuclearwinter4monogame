using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using VectorLevel;
using VectorLevel.Entities;

// TODO: replace this with the type you want to write out.
using TRead = VectorLevel.LevelDesc;

namespace VectorLevelProcessor
{
    //--------------------------------------------------------------------------
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to reader the specified data type from binary .xnb format.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    /// </summary>
    public class VectorLevelReader: ContentTypeReader<TRead>
    {

        //----------------------------------------------------------------------
        protected override TRead Read( ContentReader _input, TRead _levelDesc )
        {
            LevelDesc levelDesc = new LevelDesc();

            levelDesc.Title         = _input.ReadString();
            levelDesc.Description   = _input.ReadString();

            levelDesc.MapWidth      = _input.ReadUInt32();
            levelDesc.MapHeight     = _input.ReadUInt32();
            
            //------------------------------------------------------------------
            // Read Entities
            UInt16 entityCount = _input.ReadUInt16();

            for( int entityIndex = 0; entityIndex < entityCount; entityIndex++ )
            {
                EntityType entityType = (EntityType)_input.ReadChar();

                Entity entity;

                switch( entityType )
                {
                    case EntityType.Group:
                        entity = ReadGroup( _input, levelDesc );
                        break;
                    case EntityType.Marker:
                        entity = ReadMarker( _input, levelDesc );
                        break;
                    case EntityType.Path:
                        entity = ReadPath( _input, levelDesc );
                        break;
                    case EntityType.Text:
                        entity = ReadText( _input, levelDesc );
                        break;
                    default:
                        throw new Exception();
                }
                
                levelDesc.Entities[ entity.Name ] = entity;
                levelDesc.OrderedEntities.Add( entity );
            }

            return levelDesc;
        }

        //----------------------------------------------------------------------
        internal Group ReadGroup( ContentReader _input, TRead _levelDesc )
        {
            string strGroupName = _input.ReadString();
            string strParentName = _input.ReadString();
            GroupMode groupMode = _input.ReadObject<GroupMode>();
            Group group = new Group( strGroupName, _levelDesc.Entities[ strParentName ] as Group, groupMode );

            return group;
        }

        //----------------------------------------------------------------------
        internal Marker ReadMarker( ContentReader _input, TRead _levelDesc )
        {
            string strMarkerName        = _input.ReadString();
            string strParentName        = _input.ReadString();
            string strMarkerType        = _input.ReadString();
            string strMarkerFullPath    = _input.ReadString();
            Vector2 vPosition           = _input.ReadVector2();
            Vector2 vSize               = _input.ReadVector2();
            float fAngle                = _input.ReadSingle();
            Vector2 vScale              = _input.ReadVector2();
            Color color                 = _input.ReadColor();
            
            Marker marker = new Marker( strMarkerName, _levelDesc.Entities[ strParentName ] as Group, strMarkerType, strMarkerFullPath, vPosition, vSize, fAngle, vScale, color );
            return marker;
        }


        //----------------------------------------------------------------------
        internal Text ReadText( ContentReader _input, TRead _levelDesc )
        {
            Text text = new Text( _input.ReadString(), _levelDesc.Entities[ _input.ReadString() ] as Group, _input.ReadVector2(), _input.ReadSingle(), _input.ReadColor(), _input.ReadObject<TextAnchor>() );

            UInt16 uiTextSpanCount = _input.ReadUInt16();

            for( UInt16 uiTextSpanIndex = 0; uiTextSpanIndex < uiTextSpanCount; uiTextSpanIndex++ )
            {
                text.TextSpans.Add( new TextSpan( _input.ReadString() ) );
            }

            return text;
        }

        //----------------------------------------------------------------------
        internal Path ReadPath( ContentReader _input, TRead _levelDesc )
        {
            Path path = new Path( _input.ReadString(), _levelDesc.Entities[ _input.ReadString() ] as Group );

            // Fill Color
            UInt32 uiPackedFillColor    = _input.ReadUInt32();
            Color fillColor = new Color();
            fillColor.B = (byte) uiPackedFillColor;
            fillColor.G = (byte) (uiPackedFillColor >> 8);
            fillColor.R = (byte) (uiPackedFillColor >> 16);
            fillColor.A = (byte) (uiPackedFillColor >> 24);
            path.FillColor = fillColor;

            // Stroke color
            UInt32 uiPackedStrokeColor    = _input.ReadUInt32();
            Color StrokeColor = new Color();
            StrokeColor.B = (byte) uiPackedStrokeColor;
            StrokeColor.G = (byte) (uiPackedStrokeColor >> 8);
            StrokeColor.R = (byte) (uiPackedStrokeColor >> 16);
            StrokeColor.A = (byte) (uiPackedStrokeColor >> 24);
            path.StrokeColor = StrokeColor;

            // Stroke width
            path.StrokeWidth = _input.ReadSingle();


            // Connections
            path.ConnectionStart    = _input.ReadString();
            path.ConnectionEnd      = _input.ReadString();

            // Subpaths
            UInt16 subpathCount = _input.ReadUInt16();
            
            for( UInt16 subpathIndex = 0; subpathIndex < subpathCount; subpathIndex++ )
            {
                Subpath subpath = new Subpath();

                subpath.IsClosed = _input.ReadBoolean();
                UInt16 nodeCount = _input.ReadUInt16();

                for( UInt16 nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++ )
                {
                    PathNode node = new PathNode();
                    
                    node.Type = (PathNode.NodeType)_input.ReadChar();
                    node.Position = _input.ReadVector2();
                    node.ControlPoint1 = _input.ReadVector2();
                    node.ControlPoint2 = _input.ReadVector2();

                    subpath.PathNodes.Add( node );
                }

                subpath.Vertices = _input.ReadObject<List<Vector2>>();
                subpath.NodeIndices = _input.ReadObject<List<int>>();

                path.Subpaths.Add( subpath );
            }

            path.MeshVertices = _input.ReadObject<Vector2[]>();
            path.MeshIndices = _input.ReadObject<int[]>();

            return path;
        }
    }
}
