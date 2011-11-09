using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using VectorLevel.LevelPack;

// TODO: replace this with the type you want to write out.
using TRead = VectorLevel.LevelPack.LevelPack;

namespace VectorLevel.LevelPack
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to reader the specified data type from binary .xnb format.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    /// </summary>
    public class LevelPackReader: ContentTypeReader<TRead>
    {
        protected override TRead Read( ContentReader _input, TRead _level )
        {
            LevelPack levelPack = new LevelPack();

            levelPack.Title = _input.ReadString();
            Int16 iLevelCount = _input.ReadInt16();

            for( int iLevelIndex = 0; iLevelIndex < iLevelCount; iLevelIndex++ )
            {
                string                  strLevelFilepath    = _input.ReadString();
                string                  strLevelTitle       = _input.ReadString();
                LevelInfo.Difficulty    levelDifficulty     = (LevelInfo.Difficulty)_input.ReadByte();

                levelPack.Levels.Add( new LevelInfo( strLevelFilepath, strLevelTitle, levelDifficulty ) );
                levelPack.LevelsByFilepath[ strLevelFilepath ] = levelPack.Levels[levelPack.Levels.Count - 1];
            }
            
            return levelPack;
        }
    }
}
