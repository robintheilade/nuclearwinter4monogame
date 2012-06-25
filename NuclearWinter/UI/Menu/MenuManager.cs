using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using NuclearWinter;
using System.Diagnostics;
using System.Collections.Generic;
using System;

namespace NuclearWinter.UI
{
    public abstract class MenuManager<T>: IMenuManager where T:NuclearGame
    {
        public T                        Game                    { get; private set; }
        public ContentManager           Content                 { get; private set; }

        //----------------------------------------------------------------------
        // Menu
        public Screen                   MenuScreen              { get; private set; }

        //----------------------------------------------------------------------
        // Popup
        public Screen                   PopupScreen             { get; private set; }
        public MessagePopup             MessagePopup            { get; private set; }

        public Stack<IPopup>            PopupStack              { get; private set; }
        NuclearWinter.UI.Image          mPopupFade;

        //----------------------------------------------------------------------
        public MenuManager( T _game, Style _style, ContentManager _content )
        {
            Game        = _game;
            Content     = _content;

            //------------------------------------------------------------------
            // Menu
            MenuScreen  = new NuclearWinter.UI.Screen( _game, _style, _game.GraphicsDevice.Viewport.Width, _game.GraphicsDevice.Viewport.Height );

            //------------------------------------------------------------------
            // Popup
            PopupScreen         = new NuclearWinter.UI.Screen( _game, _style, _game.GraphicsDevice.Viewport.Width, _game.GraphicsDevice.Viewport.Height );
            PopupStack          = new Stack<IPopup>();
            MessagePopup        = new MessagePopup( this );

            mPopupFade          = new NuclearWinter.UI.Image( PopupScreen, Game.WhitePixelTex, true );
            mPopupFade.Color    = PopupScreen.Style.PopupBackgroundFadeColor;
        }

        //----------------------------------------------------------------------
        public virtual void Update( float _fElapsedTime )
        {
            MenuScreen.IsActive     = Game.IsActive && ( Game.GameStateMgr == null || ! Game.GameStateMgr.IsSwitching ) && PopupStack.Count == 0;
            PopupScreen.IsActive    = Game.IsActive && ( Game.GameStateMgr == null || ! Game.GameStateMgr.IsSwitching ) && PopupStack.Count > 0;

            MenuScreen.HandleInput();
            if( PopupStack.Count > 0 )
            {
                PopupScreen.HandleInput();
            }

            MenuScreen.Update( _fElapsedTime );
            if( PopupStack.Count > 0 )
            {
                PopupScreen.Update( _fElapsedTime );
            }
        }

        //----------------------------------------------------------------------
        public void Draw()
        {
            MenuScreen.Draw();

            if( PopupStack.Count > 0 )
            {
                PopupScreen.Draw();
            }
        }

        //----------------------------------------------------------------------
        public void PushPopup( IPopup _popup )
        {
            if( PopupStack.Contains( _popup ) ) throw new InvalidOperationException( "Cannot push same popup twice" );

            PopupStack.Push( _popup );

            PopupScreen.Root.Clear();
            PopupScreen.Root.AddChild( mPopupFade );
            PopupScreen.Root.AddChild( _popup.Panel );
        }

        //----------------------------------------------------------------------
        // NOTE: This method takes the popup to remove to help ensure consistency
        public void PopPopup( IPopup _popup )
        {
            if( PopupStack.Count == 0 || PopupStack.Peek() != _popup ) throw new InvalidOperationException( "Cannot pop a popup if it isn't at the top of the stack" );

            PopupStack.Pop();

            PopupScreen.Root.Clear();

            if( PopupStack.Count > 0 )
            {
                PopupScreen.Root.AddChild( mPopupFade );
                PopupScreen.Root.AddChild( PopupStack.Peek().Panel );
            }
        }
    }
}
