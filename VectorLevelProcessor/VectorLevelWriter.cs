using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

using VectorLevel;
using VectorLevel.Entities;

using TWrite = VectorLevel.LevelDesc;

namespace VectorLevelProcessor
{
    //--------------------------------------------------------------------------
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to write the specified data type into binary .xnb format.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    /// </summary>
    [ContentTypeWriter]
    public class VectorLevelWriter : ContentTypeWriter<TWrite>
    {
        //----------------------------------------------------------------------
        protected override void Write( ContentWriter _output, TWrite _levelDesc )
        {
            _output.Write( _levelDesc.Title );
            _output.Write( _levelDesc.Description );
            _output.Write( _levelDesc.MapWidth );
            _output.Write( _levelDesc.MapHeight );
            
            //------------------------------------------------------------------
            // Write entities
            _output.Write( (UInt16)(_levelDesc.Entities.Count - 1) ); // -1 for the Root group
            
            foreach( Entity entity in _levelDesc.OrderedEntities )
            {
                if( entity == _levelDesc.Root )
                {
                    // Do not write the Root group
                    continue;
                }

                _output.Write( (char)entity.Type );
                _output.Write( entity.Name );
                _output.Write( entity.Parent.Name );
                
                switch( entity.Type )
                {
                    case EntityType.Group:
                        WriteGroup( _output, entity as Group );
                        break;
                    case EntityType.Marker:
                        WriteMarker( _output, entity as Marker );
                        break;
                    case EntityType.Path:
                        WritePath( _output, entity as Path );
                        break;
                    case EntityType.Text:
                        WriteText( _output, entity as Text );
                        break;
                    default:
                        throw new Exception();
                }
            }
        }

        //----------------------------------------------------------------------
        internal void WriteGroup( ContentWriter _output, Group _group )
        {
            _output.WriteObject<GroupMode>( _group.GroupMode );
        }

        //----------------------------------------------------------------------
        internal void WriteMarker( ContentWriter _output, Marker _marker )
        {
            _output.Write( _marker.MarkerType );
            _output.Write( _marker.MarkerFullPath );
            _output.Write( _marker.Position );
            _output.Write( _marker.Size );
            _output.Write( _marker.Angle );
            _output.Write( _marker.Scale );
            _output.Write( _marker.Color );
        }

        //----------------------------------------------------------------------
        internal void WriteText( ContentWriter _output, Text _text )
        {
            _output.Write( _text.Position );
            _output.Write( _text.Angle );
            _output.Write( _text.FillColor );
            _output.WriteObject<TextAnchor>( _text.Anchor );

            _output.Write( (UInt16)_text.TextSpans.Count );

            foreach( TextSpan textSpan in _text.TextSpans )
            {
                _output.Write( textSpan.Value );
            }
        }

        //----------------------------------------------------------------------
        internal void WritePath( ContentWriter _output, Path _path )
        {
            _path.Flatten();

            _output.Write( (UInt32) _path.FillColor.PackedValue );
            _output.Write( (UInt32) _path.StrokeColor.PackedValue );
            _output.Write( _path.StrokeWidth );

            // Subpaths
            _output.Write( (UInt16) _path.Subpaths.Count );

            foreach( Subpath subpath in _path.Subpaths )
            {
                _output.Write( subpath.IsClosed );

                _output.Write( (UInt16)subpath.PathNodes.Count );
                foreach( PathNode node in subpath.PathNodes )
                {
                    _output.Write( (char)node.Type );
                    _output.Write( node.Position );
                    _output.Write( node.ControlPoint1 );
                    _output.Write( node.ControlPoint2 );
                }

                _output.WriteObject<List<Vector2>>( subpath.Vertices );
                _output.WriteObject<List<int>>( subpath.NodeIndices );
            }

            _output.WriteObject<Vector2[]>( _path.MeshVertices );
            _output.WriteObject<int[]>( _path.MeshIndices );
        }

        //----------------------------------------------------------------------
        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            // TODO: change this to the name of your ContentTypeReader
            // class which will be used to load this data.
            return typeof(VectorLevelReader).AssemblyQualifiedName;
        }
    }
}
