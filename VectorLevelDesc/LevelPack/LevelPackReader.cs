using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

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

            Int16 iGroupCount = _input.ReadInt16();

            for( int i = 0; i < iGroupCount; i++ )
            {
                string levelGroupTitle = _input.ReadString();

                LevelGroup levelGroup = new LevelGroup( levelGroupTitle );

                Int16 iLevelCount = _input.ReadInt16();

                for( int levelIndex = 0; levelIndex < iLevelCount; levelIndex++ )
                {
                    string levelFilepath = _input.ReadString();
                    string levelTitle = _input.ReadString();
                    string levelDescription = _input.ReadString();

                    levelGroup.Levels.Add( new LevelInfo( levelFilepath, levelTitle, levelDescription ) );
                }

                levelPack.LevelGroups.Add( levelGroup );
            }
            
            return levelPack;
        }
    }
}
