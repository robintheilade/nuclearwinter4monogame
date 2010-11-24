using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using System.Xml;
using System.Globalization;

// TODO: replace these with the processor input and output types.
using TInput = VectorLevelProcessor.XMLFile;
using TOutput = VectorLevel.LevelPack.LevelPack;

namespace VectorLevelProcessor.LevelPack
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to apply custom processing to content data, converting an object of
    /// type TInput to TOutput. The input and output types may be the same if
    /// the processor wishes to alter data without changing its type.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    /// </summary>
    [ContentProcessor(DisplayName = "Level Pack Processor")]
    public class LevelPackProcessor: ContentProcessor<TInput, TOutput>
    {
        //----------------------------------------------------------------------
        /// <summary>
        /// Process Level Pack
        /// </summary>
        /// <param name="_input"></param>
        /// <param name="_context"></param>
        /// <returns></returns>
        public override TOutput Process( TInput _input, ContentProcessorContext _context )
        {
            
            VectorLevel.LevelPack.LevelPack levelPack = new VectorLevel.LevelPack.LevelPack();

            System.Xml.XmlReader reader = _input.XmlReader;

            reader.Read();
            reader.ReadStartElement( "LevelPack" );

            _context.Logger.LogImportantMessage( "Output directory is: " + _context.OutputDirectory );
            _context.Logger.LogImportantMessage( "Intermediate directory is: " + _context.IntermediateDirectory );

            while( reader.Read() )
            {
                if( reader.IsStartElement( "Group" ) && reader.IsEmptyElement )
                {
                    string strGroupPath    = reader.GetAttribute( "Path" );
                    string strGroupTitle   = reader.GetAttribute( "Title" );

                    string strFullGroupPath = System.IO.Path.Combine( System.IO.Path.GetDirectoryName( _input.Filepath ), strGroupPath );

                    VectorLevel.LevelPack.LevelGroup levelGroup = new VectorLevel.LevelPack.LevelGroup( strGroupTitle );
                    
                    foreach( string strLevelFilepath in System.IO.Directory.GetFiles( strFullGroupPath, "*", System.IO.SearchOption.AllDirectories ) )
                    {
                        // Using BuildAndLoadAsset() instead of Convert() would allow to reuse XML intermediate files instead of rebuilding all levels each time
                        // BUT it implies that everything can be XML-deserializable (which is not currently the case)
                        _context.AddDependency( strLevelFilepath );
                        System.Xml.XmlReader levelReader = System.Xml.XmlReader.Create( strLevelFilepath );
                        VectorLevel.LevelDesc level = _context.Convert<XMLFile, VectorLevel.LevelDesc>( new XMLFile( System.IO.Path.GetFullPath( strLevelFilepath ), levelReader ), "VectorLevelProcessor" );

                        levelGroup.Levels.Add( new VectorLevel.LevelPack.LevelInfo( System.IO.Path.Combine( strGroupPath, System.IO.Path.GetFileNameWithoutExtension( strLevelFilepath ) ), level.Title, level.Description ) );
                    }

                    levelPack.LevelGroups.Add( levelGroup );
                }
                else
                if( reader.NodeType == XmlNodeType.EndElement )
                {
                    break;
                }
            }

            return levelPack;
        }
    }
}
