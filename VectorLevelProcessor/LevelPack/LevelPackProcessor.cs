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

using VectorLevel.LevelPack;

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

            _context.Logger.LogImportantMessage( "Output directory is: " + _context.OutputDirectory );
            _context.Logger.LogImportantMessage( "Intermediate directory is: " + _context.IntermediateDirectory );

            while( reader.Read() )
            {
                if( reader.IsStartElement( "LevelPack" )) 
                {
                    levelPack.Title = reader.GetAttribute( "Title" );
                }

                if( reader.IsStartElement( "Level" ) && reader.IsEmptyElement )
                {
                    string strLevelFilepath         = reader.GetAttribute( "Path" );
                    string strLevelTitle            = reader.GetAttribute( "Title" );
                    LevelInfo.Difficulty difficulty =  (LevelInfo.Difficulty)Enum.Parse( typeof(LevelInfo.Difficulty), reader.GetAttribute( "Difficulty" ), true );

                    levelPack.Levels.Add(
                        new VectorLevel.LevelPack.LevelInfo(
                            strLevelFilepath,
                            strLevelTitle,
                            difficulty )
                        );
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
