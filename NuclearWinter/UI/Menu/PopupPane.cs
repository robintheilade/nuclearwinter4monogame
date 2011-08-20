using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NuclearWinter.UI
{
    public abstract class PopupPane<T>: Pane where T:IMenuManager
    {
        public T                    Manager { get; private set; }

        //----------------------------------------------------------------------
        public PopupPane( T _manager )
        {
            Manager     = _manager;
            FixedGroup  = new NuclearWinter.UI.FixedGroup( Manager.PopupScreen );
        }
    }
}
