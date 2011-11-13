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

        Group mPopupGroup;
        public Group            PopupGroup
        {
            get { return mPopupGroup; }
            set {
                if( mPopupGroup != null )
                {
                    mPopupScreen.Root.RemoveChild( mPopupGroup );
                }

                mPopupGroup = value;

                if( mPopupGroup != null )
                {
                    mPopupScreen.Root.AddChild( mPopupGroup );
                }
            }
        }

        public MessagePopup MessagePopup        { get; private set; }

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
            PopupScreen.Root.AddChild( fade );

            //------------------------------------------------------------------
            MessagePopup        = new MessagePopup( this );
        }

        //----------------------------------------------------------------------
        public virtual void Update( float _fElapsedTime )
        {
            MenuScreen.IsActive     = Game.IsActive && ( Game.GameStateMgr == null || ! Game.GameStateMgr.IsSwitching ) && PopupGroup == null;
            PopupScreen.IsActive    = Game.IsActive && ( Game.GameStateMgr == null || ! Game.GameStateMgr.IsSwitching ) && PopupGroup != null;

            MenuScreen.HandleInput();
            if( PopupGroup != null )
            {
                PopupScreen.HandleInput();
            }

            MenuScreen.Update( _fElapsedTime );
            if( PopupGroup != null )
            {
                PopupScreen.Update( _fElapsedTime );
            }
        }

        //----------------------------------------------------------------------
        public void Draw()
        {
            MenuScreen.Draw();

            if( PopupGroup != null )
            {
                PopupScreen.Draw();
            }
        }
    }
}
