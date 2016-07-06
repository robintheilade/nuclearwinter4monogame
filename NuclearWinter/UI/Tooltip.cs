using Microsoft.Xna.Framework;
using System;

namespace NuclearWinter.UI
{
    public class Tooltip : Widget
    {
        //----------------------------------------------------------------------
        public string Text;

        //----------------------------------------------------------------------
        public bool EnableDisplayTimer
        {
            get { return mbEnableDisplayTimer; }
            set
            {
                mbEnableDisplayTimer = value;

                if (!mbEnableDisplayTimer)
                {
                    mfTooltipTimer = 0f;
                }
            }
        }

        //----------------------------------------------------------------------
        float mfTooltipTimer;
        const float sfTooltipDelay = 0.6f;
        bool mbEnableDisplayTimer;

        //----------------------------------------------------------------------
        public Tooltip(Screen screen, string text)
        : base(screen)
        {
            Text = text;
            Padding = Screen.Style.TooltipPadding;
        }

        //----------------------------------------------------------------------
        public void DisplayNow()
        {
            mfTooltipTimer = sfTooltipDelay;
        }

        //----------------------------------------------------------------------
        public void ResetTimer()
        {
            mfTooltipTimer = 0f;
        }

        //----------------------------------------------------------------------
        public override void Update(float elapsedTime)
        {
            if (EnableDisplayTimer)
            {
                mfTooltipTimer += elapsedTime;
            }
            base.Update(elapsedTime);
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            if (mfTooltipTimer < sfTooltipDelay || string.IsNullOrEmpty(Text)) return;

            UIFont font = Screen.Style.MediumFont;

            Vector2 vSize = font.MeasureString(Text);
            int iWidth = (int)vSize.X;
            int iHeight = (int)vSize.Y;

            Point topLeft = new Point(
                Math.Min(Screen.Game.InputMgr.MouseState.X, Screen.Width - iWidth - Padding.Horizontal),
                Math.Min(Screen.Game.InputMgr.MouseState.Y + 20, Screen.Height - iHeight - Padding.Vertical));

            Screen.DrawBox(Screen.Style.TooltipFrame, new Rectangle(topLeft.X, topLeft.Y, iWidth + Padding.Horizontal, iHeight + Padding.Vertical), Screen.Style.TooltipCornerSize, Color.White);
            Screen.Game.SpriteBatch.DrawString(font, Text, new Vector2(topLeft.X + Padding.Left, topLeft.Y + Padding.Top + font.YOffset), Screen.Style.TooltipTextColor);
        }

        //----------------------------------------------------------------------
        // NOTE: Hack to allow external update
        public void DoUpdate(float elapsedTime)
        {
            Update(elapsedTime);
        }

        //----------------------------------------------------------------------
        // NOTE: Hack to allow external draw
        public void DoDraw()
        {
            Draw();
        }
    }
}
