using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;

namespace NuclearWinter.UI
{
    public abstract class MenuManager<T> : IMenuManager where T : NuclearGame
    {
        public T Game { get; private set; }
        public ContentManager Content { get; private set; }

        //----------------------------------------------------------------------
        // Menu
        public Screen MenuScreen { get; private set; }

        //----------------------------------------------------------------------
        // Popup
        public Screen PopupScreen { get; private set; }
        public MessagePopup MessagePopup { get; private set; }

        public IEnumerable<Panel> PopupStack { get { return (IEnumerable<Panel>)mPopupStack; } }
        public Panel TopMostPopup { get { return mPopupStack.Count > 0 ? mPopupStack.Peek() : null; } }

        Stack<Panel> mPopupStack;
        NuclearWinter.UI.Image mPopupFade;

        //----------------------------------------------------------------------
        public MenuManager(T game, Style style, ContentManager content)
        {
            Game = game;
            Content = content;

            //------------------------------------------------------------------
            // Menu
            MenuScreen = new NuclearWinter.UI.Screen(game, style, game.GraphicsDevice.Viewport.Width, game.GraphicsDevice.Viewport.Height);

            //------------------------------------------------------------------
            // Popup
            PopupScreen = new NuclearWinter.UI.Screen(game, style, game.GraphicsDevice.Viewport.Width, game.GraphicsDevice.Viewport.Height);
            mPopupStack = new Stack<Panel>();
            MessagePopup = new MessagePopup(this);

            mPopupFade = new NuclearWinter.UI.Image(PopupScreen, Game.WhitePixelTex, true);
            mPopupFade.Color = PopupScreen.Style.PopupBackgroundFadeColor;
        }

        //----------------------------------------------------------------------
        public virtual void Update(float elapsedTime)
        {
            MenuScreen.IsActive = Game.IsActive && (Game.GameStateMgr == null || !Game.GameStateMgr.IsSwitching) && mPopupStack.Count == 0;
            PopupScreen.IsActive = Game.IsActive && (Game.GameStateMgr == null || !Game.GameStateMgr.IsSwitching) && mPopupStack.Count > 0;

            MenuScreen.HandleInput();
            if (mPopupStack.Count > 0)
            {
                PopupScreen.HandleInput();
            }

            MenuScreen.Update(elapsedTime);
            if (mPopupStack.Count > 0)
            {
                PopupScreen.Update(elapsedTime);
            }
        }

        //----------------------------------------------------------------------
        public void Draw()
        {
            MenuScreen.Draw();

            if (mPopupStack.Count > 0)
            {
                PopupScreen.Draw();
            }
        }

        //----------------------------------------------------------------------
        public void PushPopup(Panel popup)
        {
            if (mPopupStack.Contains(popup)) throw new InvalidOperationException("Cannot push same popup twice");

            mPopupStack.Push(popup);

            PopupScreen.Root.Clear();
            PopupScreen.Root.AddChild(mPopupFade);
            PopupScreen.Root.AddChild((Panel)popup);
        }

        //----------------------------------------------------------------------
        // NOTE: This method takes the removed popup as an argument to help ensure consistency
        public void PopPopup(Panel popup)
        {
            if (mPopupStack.Count == 0 || mPopupStack.Peek() != popup) throw new InvalidOperationException("Cannot pop a popup if it isn't at the top of the stack");

            mPopupStack.Pop();

            PopupScreen.Root.Clear();

            if (mPopupStack.Count > 0)
            {
                PopupScreen.Root.AddChild(mPopupFade);

                var panel = (Panel)mPopupStack.Peek();
                PopupScreen.Root.AddChild(panel);
                PopupScreen.Focus(panel);
            }
        }
    }
}
