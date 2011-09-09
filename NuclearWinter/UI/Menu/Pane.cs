using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NuclearWinter.UI
{
    public abstract class Pane
    {
        public NuclearWinter.UI.FixedGroup       FixedGroup          { get; protected set; }

        //----------------------------------------------------------------------
        public Pane()
        {
        }

        public abstract void Open();
        public abstract void Close();

        public virtual void Focus()
        {
            FixedGroup.Screen.Focus( FixedGroup.GetFirstFocusableDescendant( NuclearWinter.UI.Direction.Down ) );
        }
    }
}
