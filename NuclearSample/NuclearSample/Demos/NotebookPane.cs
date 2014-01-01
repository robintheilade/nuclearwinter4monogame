using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NuclearUI = NuclearWinter.UI;

namespace NuclearSample.Demos
{
    class NotebookPane: NuclearUI.ManagerPane<MainMenuManager>
    {
        //----------------------------------------------------------------------
        public NotebookPane( MainMenuManager _manager )
        : base( _manager )
        {
            //------------------------------------------------------------------
            var notebook = new NuclearUI.Notebook( Manager.MenuScreen );
            AddChild( notebook );

            notebook.HasClosableTabs  = true;

            var homeTab = new NuclearUI.NotebookTab( notebook, "Home", null );
            homeTab.IsPinned = true;
            notebook.Tabs.Add( homeTab );

            var createTab = new NuclearUI.Button( Manager.MenuScreen, "Create tab" );
            createTab.AnchoredRect = NuclearUI.AnchoredRect.CreateFull( 10 );

            int iTabCounter = 0;
            createTab.ClickHandler = delegate {
                var tab = new NuclearUI.NotebookTab( notebook, string.Format( "Tab {0}", ++iTabCounter ), null );

                notebook.Tabs.Add( tab );
            };

            homeTab.PageGroup.AddChild( createTab );
        }

        //----------------------------------------------------------------------
    }
}
