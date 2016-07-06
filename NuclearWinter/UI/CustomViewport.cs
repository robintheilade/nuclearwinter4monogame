using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NuclearWinter.UI
{
    public abstract class CustomViewport : Widget
    {
        //----------------------------------------------------------------------
        public CustomViewport(Screen screen)
        : base(screen)
        {
        }

        //----------------------------------------------------------------------
        public override void DoLayout(Rectangle rectangle)
        {
            base.DoLayout(rectangle);
            HitBox = LayoutRect;
        }

        //----------------------------------------------------------------------
        Viewport mPreviousViewport;

        protected internal virtual void BeginDraw()
        {
            Screen.SuspendBatch();
            mPreviousViewport = Screen.Game.GraphicsDevice.Viewport;

            Viewport viewport = new Viewport(LayoutRect);
            Screen.Game.GraphicsDevice.Viewport = viewport;
        }

        //----------------------------------------------------------------------
        protected internal virtual void EndDraw()
        {
            Screen.Game.GraphicsDevice.Viewport = mPreviousViewport;
            Screen.ResumeBatch();
        }
    }
}
