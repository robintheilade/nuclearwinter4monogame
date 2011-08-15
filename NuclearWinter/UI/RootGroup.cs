using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NuclearWinter.UI
{
    public class RootGroup: Group
    {
        public override bool CanFocus { get { return false; } }

        //----------------------------------------------------------------------
        public RootGroup( Screen _screen )
        : base( _screen )
        {
        }
    }
}
