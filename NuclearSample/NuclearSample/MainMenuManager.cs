using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NuclearUI = NuclearWinter.UI;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace NuclearSample
{
    //--------------------------------------------------------------------------
    internal class MainMenuManager: NuclearUI.MenuManager<NuclearSampleGame>
    {
        NuclearUI.Splitter      mSplitter;
        NuclearUI.Panel         mDemoPanel;

        //----------------------------------------------------------------------
        public MainMenuManager( NuclearSampleGame _game, ContentManager _content )
        : base( _game, _game.UIStyle, _content )
        {
            // Splitter
            mSplitter = new NuclearUI.Splitter( MenuScreen, NuclearUI.Direction.Left );
            mSplitter.AnchoredRect = NuclearUI.AnchoredRect.CreateFull( 10 );
            MenuScreen.Root.AddChild( mSplitter );
            mSplitter.Collapsable = true;

            mSplitter.FirstPaneMinSize = 200;

            // Demo list
            NuclearUI.BoxGroup demosBoxGroup = new NuclearUI.BoxGroup( MenuScreen, NuclearUI.Orientation.Vertical, 0, NuclearUI.Anchor.Start );
            mSplitter.FirstPane = demosBoxGroup;

            mDemoPanel = new NuclearUI.Panel( MenuScreen, Content.Load<Texture2D>( "Sprites/UI/Panel04" ), MenuScreen.Style.PanelCornerSize );

            Demos.BasicDemoPane basicDemoPane = new Demos.BasicDemoPane( this );
            mSplitter.SecondPane = mDemoPanel;

            mDemoPanel.AddChild( basicDemoPane );

            demosBoxGroup.AddChild( CreateDemoButton( "Basic", basicDemoPane ), true );
            demosBoxGroup.AddChild( CreateDemoButton( "Notebook", new Demos.NotebookPane( this ) ), true );
            demosBoxGroup.AddChild( CreateDemoButton( "Text Area", new Demos.TextAreaPane( this ) ), true );
            demosBoxGroup.AddChild( CreateDemoButton( "Custom Viewport", new Demos.CustomViewportPane( this ) ), true );
        }

        //----------------------------------------------------------------------
        NuclearUI.Button CreateDemoButton( string _strDemoName, NuclearUI.ManagerPane<MainMenuManager> _demoPane )
        {
            NuclearUI.Button demoPaneButton = new NuclearUI.Button( MenuScreen, _strDemoName );
            demoPaneButton.ClickHandler = delegate {
                mDemoPanel.Clear();
                mDemoPanel.AddChild( _demoPane );
            };

            return demoPaneButton;
        }
    }
}
