using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NuclearWinter.UI
{
    public abstract class ManagerPane<T> where T:IMenuManager
    {
        public T                    Manager         { get; private set; }
        public Group                FixedGroup      { get; private set; }

        //----------------------------------------------------------------------
        public ManagerPane( T _manager )
        {
            Manager     = _manager;
            FixedGroup  =  new Group( _manager.MenuScreen );
        }
    }
}
