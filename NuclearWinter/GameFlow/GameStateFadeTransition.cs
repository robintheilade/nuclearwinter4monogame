using Microsoft.Xna.Framework;
using System;

namespace NuclearWinter.GameFlow
{
    public abstract class GameStateFadeTransition<T> : GameState<T> where T : NuclearGame
    {
        static float sfTransitionDuration = 0.3f;
        float mfTransition;

        //----------------------------------------------------------------------
        public GameStateFadeTransition(T game)
        : base(game)
        {
        }

        //----------------------------------------------------------------------
        public override bool UpdateFadeIn(float time)
        {
            bool bFadeInDone = (mfTransition >= sfTransitionDuration);
            mfTransition = Math.Min(mfTransition + time, sfTransitionDuration);

            Update(time);

            return bFadeInDone;
        }

        //----------------------------------------------------------------------
        public override bool UpdateFadeOut(float time)
        {
            bool bFadeOutDone = (mfTransition <= 0f);
            mfTransition = Math.Max(mfTransition - time, 0f);

            Update(time);

            return bFadeOutDone;
        }

        //----------------------------------------------------------------------
        public override void DrawFadeIn()
        {
            Draw(mfTransition / sfTransitionDuration);
        }

        //----------------------------------------------------------------------
        public override void DrawFadeOut()
        {
            Draw(mfTransition / sfTransitionDuration);
        }

        //----------------------------------------------------------------------
        public void Draw(float transition)
        {
            Draw();

            Color fadeColor = Color.Black * (1f - transition);

            Game.SpriteBatch.Begin();

            // this.Game.Form.Width/Height does not work with fullscreen
            // this.Game.ClientSize.Width/Height does not work with fullscreen
            // this.Game.Graphics.PreferredBackBufferWidth/Height is unreliable not just in fullscreen
            Game.SpriteBatch.Draw(Game.WhitePixelTex, new Rectangle(0, 0, this.Game.GraphicsDevice.DisplayMode.Width + 1, this.Game.GraphicsDevice.DisplayMode.Height + 1), fadeColor);

            Game.SpriteBatch.End();
        }
    }
}
