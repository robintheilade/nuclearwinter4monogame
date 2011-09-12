using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using NuclearWinter;
using System.Diagnostics;

namespace NuclearWinter.UI
{
    public abstract class MenuManager<T>: IMenuManager where T:NuclearGame
    {
        public T                Game                    { get; private set; }

        ContentManager mContent;
        public ContentManager   Content                 { get { return mContent; } }

        //----------------------------------------------------------------------
        // Menu
        Screen mMenuScreen;
        public Screen           MenuScreen              { get { return mMenuScreen; } }

        //----------------------------------------------------------------------
        // Popup
        Screen mPopupScreen;
        public Screen           PopupScreen             { get { return mPopupScreen; } }
        public FixedWidget      PopupScreenPaneAnchor   { get; private set; }
        Pane                    mActivePopupPane;
        public MessagePopupPane MessagePopupPane        { get; private set; }

        //----------------------------------------------------------------------
        public MenuManager( T _game, Style _style, ContentManager _content )
        {
            Game        = _game;
            mContent    = _content;

            //------------------------------------------------------------------
            // Menu
            mMenuScreen  = new NuclearWinter.UI.Screen( _game, _style, _game.GraphicsDevice.Viewport.Width, _game.GraphicsDevice.Viewport.Height );

            //------------------------------------------------------------------
            // Popup
            mPopupScreen = new NuclearWinter.UI.Screen( _game, _style, _game.GraphicsDevice.Viewport.Width, _game.GraphicsDevice.Viewport.Height );

            NuclearWinter.UI.Image fade = new NuclearWinter.UI.Image( PopupScreen, Game.WhitePixelTex, true );
            fade.Color = mPopupScreen.Style.PopupBackgroundFadeColor;
            PopupScreen.Root.AddChild( new NuclearWinter.UI.FixedWidget( fade, AnchoredRect.CreateFull(0) ) );

            PopupScreenPaneAnchor   = new NuclearWinter.UI.FixedWidget( PopupScreen, AnchoredRect.CreateFull(0) );
            PopupScreen.Root.AddChild( PopupScreenPaneAnchor );

            //------------------------------------------------------------------
            MessagePopupPane        = new MessagePopupPane( this );
        }

        //----------------------------------------------------------------------
        public void DisplayPopup( Pane _popupPane )
        {
            Debug.Assert( mActivePopupPane == null );
            Debug.Assert( _popupPane.FixedGroup.Screen == PopupScreen );

            mActivePopupPane = _popupPane;
            PopupScreenPaneAnchor.Child = mActivePopupPane.FixedGroup;
        }

        //----------------------------------------------------------------------
        public void ClosePopup()
        {
            Debug.Assert( mActivePopupPane != null );
            PopupScreenPaneAnchor.Child = null;
            mActivePopupPane = null;
        }

        //----------------------------------------------------------------------
        public virtual void Update( float _fElapsedTime )
        {
            MenuScreen.IsActive     = Game.IsActive && ! Game.GameStateMgr.IsSwitching && mActivePopupPane == null;
            PopupScreen.IsActive    = Game.IsActive && ! Game.GameStateMgr.IsSwitching && mActivePopupPane != null;

            MenuScreen.HandleInput();
            if( mActivePopupPane != null )
            {
                PopupScreen.HandleInput();
            }

            MenuScreen.Update( _fElapsedTime );
            if( mActivePopupPane != null )
            {
                PopupScreen.Update( _fElapsedTime );
            }
        }

        //----------------------------------------------------------------------
        public void Draw()
        {
            MenuScreen.Draw();

            if( mActivePopupPane != null )
            {
                PopupScreen.Draw();
            }
        }
    }
}
