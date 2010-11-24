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
        public LevelInfo( string _strFilepath, string _strTitle, string _strDescription )
        {
            Filepath    = _strFilepath;
            Title       = _strTitle;
            Description = _strDescription;
        }

        //----------------------------------------------------------------------
        public string   Filepath;
        public string   Title;
        public string   Description;
    }
}
