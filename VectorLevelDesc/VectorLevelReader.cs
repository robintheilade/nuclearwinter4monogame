using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

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
            VectorLevel.LevelDesc levelDesc = new VectorLevel.LevelDesc();

            levelDesc.Title         = _input.ReadString();
            levelDesc.Description   = _input.ReadString();

            levelDesc.MapWidth      = _input.ReadUInt32();
            levelDesc.MapHeight     = _input.ReadUInt32();
            
            //------------------------------------------------------------------
            // Read Entities
            UInt16 entityCount = _input.ReadUInt16();

            for( int entityIndex = 0; entityIndex < entityCount; entityIndex++ )
            {
                VectorLevel.Entities.EntityType entityType = (VectorLevel.Entities.EntityType)_input.ReadChar();

                VectorLevel.Entities.Entity entity;

                switch( entityType )
                {
                    case VectorLevel.Entities.EntityType.Group:
                        entity = ReadGroup( _input, levelDesc );
                        break;
                    case VectorLevel.Entities.EntityType.Marker:
                        entity = ReadMarker( _input, levelDesc );
                        break;
                    case VectorLevel.Entities.EntityType.Path:
                        entity = ReadPath( _input, levelDesc );
                        break;
                    case VectorLevel.Entities.EntityType.Text:
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
        internal VectorLevel.Entities.Group ReadGroup( ContentReader _input, TRead _levelDesc )
        {
            string strGroupName = _input.ReadString();
            string strParentName = _input.ReadString();
            VectorLevel.Entities.Group group = new VectorLevel.Entities.Group( strGroupName, _levelDesc.Entities[ strParentName ] as VectorLevel.Entities.Group );

            return group;
        }

        //----------------------------------------------------------------------
        internal VectorLevel.Entities.Marker ReadMarker( ContentReader _input, TRead _levelDesc )
        {
            string strMarkerName    = _input.ReadString();
            string strParentName    = _input.ReadString();
            string strMarkerType    = _input.ReadString();
            Vector2 vPosition       = _input.ReadVector2();
            Vector2 vSize           = _input.ReadVector2();
            float fAngle            = _input.ReadSingle();
            Vector2 vScale          = _input.ReadVector2();
            Color color             = _input.ReadColor();
            
            VectorLevel.Entities.Marker marker = new VectorLevel.Entities.Marker( strMarkerName, _levelDesc.Entities[ strParentName ] as VectorLevel.Entities.Group, strMarkerType, vPosition, vSize, fAngle, vScale, color );
            return marker;
        }


        //----------------------------------------------------------------------
        internal VectorLevel.Entities.Text ReadText( ContentReader _input, TRead _levelDesc )
        {
            VectorLevel.Entities.Text text = new VectorLevel.Entities.Text( _input.ReadString(), _levelDesc.Entities[ _input.ReadString() ] as VectorLevel.Entities.Group, _input.ReadVector2(), _input.ReadSingle() );

            UInt16 uiTextSpanCount = _input.ReadUInt16();

            for( UInt16 uiTextSpanIndex = 0; uiTextSpanIndex < uiTextSpanCount; uiTextSpanIndex++ )
            {
                text.TextSpans.Add( new VectorLevel.Entities.TextSpan( _input.ReadString() ) );
            }

            return text;
        }

        //----------------------------------------------------------------------
        internal VectorLevel.Entities.Path ReadPath( ContentReader _input, TRead _levelDesc )
        {
            VectorLevel.Entities.Path path = new VectorLevel.Entities.Path( _input.ReadString(), _levelDesc.Entities[ _input.ReadString() ] as VectorLevel.Entities.Group );

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

            // Subpaths
            UInt16 subpathCount = _input.ReadUInt16();
            
            for( UInt16 subpathIndex = 0; subpathIndex < subpathCount; subpathIndex++ )
            {
                VectorLevel.Entities.Subpath subpath = new VectorLevel.Entities.Subpath();

                subpath.IsClosed = _input.ReadBoolean();
                UInt16 nodeCount = _input.ReadUInt16();

                for( UInt16 nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++ )
                {
                    VectorLevel.Entities.PathNode node = new VectorLevel.Entities.PathNode();
                    
                    node.Type = (VectorLevel.Entities.PathNode.NodeType)_input.ReadChar();
                    node.Position = _input.ReadVector2();
                    node.ControlPoint1 = _input.ReadVector2();
                    node.ControlPoint2 = _input.ReadVector2();

                    subpath.PathNodes.Add( node );
                }

                path.Subpaths.Add( subpath );
            }

            return path;
        }
    }
}
