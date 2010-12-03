using System;
using System.Collections.Generic;

using System.Text;

namespace VectorLevel.LevelPack
{

    public class LevelInfo
    {
        //----------------------------------------------------------------------
        public LevelInfo()
        {

        }

        //----------------------------------------------------------------------
        public LevelInfo( string _strFilepath, string _strTitle, Difficulty _difficulty )
        {
            Filepath            = _strFilepath;
            Title               = _strTitle;
            LevelDifficulty     = _difficulty;
        }

        //----------------------------------------------------------------------
        public string       Filepath            = "";
        public string       Title               = "";
        public Difficulty   LevelDifficulty     = Difficulty.Easy;

        public enum Difficulty
        {
            Easy,
            Medium,
            Hard
        }
    }
}
