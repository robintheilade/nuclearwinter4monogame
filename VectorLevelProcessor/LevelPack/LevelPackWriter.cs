using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

using TWrite = VectorLevel.LevelPack.LevelPack;

namespace VectorLevelProcessor.LevelPack
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to write the specified data type into binary .xnb format.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    /// </summary>
    [ContentTypeWriter]
    public class LevelPackWriter : ContentTypeWriter<TWrite>
    {
        protected override void Write( ContentWriter _output, TWrite _levelPack )
        {
            _output.Write( (Int16)_levelPack.LevelGroups.Count );
            
            foreach( VectorLevel.LevelPack.LevelGroup levelGroup in _levelPack.LevelGroups )
            {
                _output.Write( levelGroup.Title );
                _output.Write( (Int16)levelGroup.Levels.Count );

                foreach( VectorLevel.LevelPack.LevelInfo levelInfo in levelGroup.Levels )
                {
                    _output.Write( levelInfo.Filepath );
                    _output.Write( levelInfo.Title );
                    _output.Write( levelInfo.Description );
                }
            }
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            // TODO: change this to the name of your ContentTypeReader
            // class which will be used to load this data.
            return typeof(VectorLevel.LevelPack.LevelPackReader).AssemblyQualifiedName;
        }
    }
}
