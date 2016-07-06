using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace NuclearWinter.UI
{
    public class SpinningWheel : Image
    {
        float mfAngle;
        float mfFadeTimer;

        const float sfFadeDelay = 0.2f;
        const float sfFadeDuration = 0.4f;
        public bool FadeIn = true;

        //----------------------------------------------------------------------
        public SpinningWheel(Screen screen, Texture2D texture)
        : base(screen, texture)
        {
        }

        //----------------------------------------------------------------------
        public override void Update(float elapsedTime)
        {
            mfAngle += elapsedTime * 3f;

            mfFadeTimer = Math.Min(sfFadeDuration + sfFadeDelay, mfFadeTimer + elapsedTime);
        }

        public void Reset()
        {
            mfAngle = 0f;
            mfFadeTimer = 0f;
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            if (mTexture == null) return;

            Vector2 vOrigin = new Vector2(mTexture.Width / 2f, mTexture.Height / 2f);

            Color fadedColor = Color;

            if (FadeIn)
            {
                fadedColor *= (float)Math.Pow(Math.Max(0f, mfFadeTimer - sfFadeDelay) / sfFadeDuration, 2);
            }

            if (!mbStretch)
            {
                Screen.Game.SpriteBatch.Draw(mTexture, new Vector2(LayoutRect.Center.X - ContentWidth / 2 + Padding.Left, LayoutRect.Center.Y - ContentHeight / 2 + Padding.Top) + vOrigin, null, fadedColor, mfAngle, vOrigin, 1f, SpriteEffects.None, 0f);
            }
            else
            {
                Screen.Game.SpriteBatch.Draw(mTexture, new Rectangle(LayoutRect.X + Padding.Left + (int)vOrigin.X, LayoutRect.Y + Padding.Top + (int)vOrigin.Y, LayoutRect.Width - Padding.Horizontal, LayoutRect.Height - Padding.Vertical), null, fadedColor, mfAngle, new Vector2(mTexture.Width / 2f, mTexture.Height / 2f), SpriteEffects.None, 0f);
            }
        }
    }
}
