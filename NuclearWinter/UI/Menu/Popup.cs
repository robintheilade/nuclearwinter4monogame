using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace NuclearWinter.UI
{
    public abstract class Popup<T> where T:IMenuManager
    {
        public T                    Manager { get; private set; }

        public readonly Point       DefaultSize = new Point( 800, 450 );
        public Panel                Panel { get; private set; }

        //----------------------------------------------------------------------
        public Popup( T _manager )
        {
            Manager     = _manager;

            Panel = new Panel( Manager.PopupScreen, Manager.PopupScreen.Style.PopupFrame, Manager.PopupScreen.Style.PopupFrameCornerSize );
            Panel.AnchoredRect = AnchoredRect.CreateCentered( DefaultSize.X, DefaultSize.Y );
        }

        public abstract void Open();
    }
}
