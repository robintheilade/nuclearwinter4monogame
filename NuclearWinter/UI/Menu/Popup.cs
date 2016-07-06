using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace NuclearWinter.UI
{
    public abstract class Popup<T> : Panel where T : IMenuManager
    {
        public T Manager { get; private set; }
        public readonly Point DefaultSize = new Point(800, 450);

        //----------------------------------------------------------------------
        public Popup(T manager)
        : base(manager.PopupScreen, manager.PopupScreen.Style.PopupFrame, manager.PopupScreen.Style.PopupFrameCornerSize)
        {
            Manager = manager;
            AnchoredRect = AnchoredRect.CreateCentered(DefaultSize.X, DefaultSize.Y);
        }

        //----------------------------------------------------------------------
        /// <summary>
        /// Close should be called when programmatically closing a popup
        /// </summary>
        public virtual void Close()
        {
            Manager.PopPopup(this);
        }

        //----------------------------------------------------------------------
        /// <summary>
        /// Dismiss should be called whenever the user dismisses the popup (with the Escape key or a Cancel / Back buttons)
        /// </summary>
        protected virtual void Dismiss()
        {
            Close();
        }

        //----------------------------------------------------------------------
        protected internal override void OnKeyPress(Keys key)
        {
            if (key == Keys.Escape)
            {
                Dismiss();
            }
            else
            {
                base.OnKeyPress(key);
            }
        }
    }
}
