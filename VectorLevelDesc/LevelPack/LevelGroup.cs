using System;
using System.Collections.Generic;

using System.Text;

namespace VectorLevel.LevelPack
{
    public class LevelGroup
    {
        //----------------------------------------------------------------------
        public LevelGroup( string _strTitle )
        {
            Title = _strTitle;
        }

        //----------------------------------------------------------------------
        public string               Title = "";
        public List<LevelInfo>      Levels = new List<LevelInfo>();
    }
}
