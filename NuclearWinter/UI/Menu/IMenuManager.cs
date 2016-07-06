using Microsoft.Xna.Framework.Content;

namespace NuclearWinter.UI
{
    //--------------------------------------------------------------------------
    public interface IMenuManager
    {
        //----------------------------------------------------------------------
        ContentManager Content { get; }

        //----------------------------------------------------------------------
        Screen MenuScreen { get; }
        Screen PopupScreen { get; }

        void PushPopup(Panel popup);
        void PopPopup(Panel popup);
    }
}
