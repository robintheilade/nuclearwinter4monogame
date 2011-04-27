using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VectorUI.Widgets
{
    public abstract class Widget
    {
        //----------------------------------------------------------------------
        public Widget( UISheet _uiSheet )
        {
            UISheet = _uiSheet;
        }

        //----------------------------------------------------------------------
        public abstract void Update( float _fElapsedTime );
        public abstract void Draw();

        //----------------------------------------------------------------------
        public UISheet UISheet;
    }
}
