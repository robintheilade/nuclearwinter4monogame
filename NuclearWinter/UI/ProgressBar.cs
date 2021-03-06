﻿using Microsoft.Xna.Framework;
using System;

namespace NuclearWinter.UI
{
    //--------------------------------------------------------------------------
    public class ProgressBar : Widget
    {
        //----------------------------------------------------------------------
        public int Value
        {
            get { return miValue; }
            set { miValue = value; mfLerpValue = miValue; }
        }

        public int Max;

        //----------------------------------------------------------------------
        int miValue;
        float mfLerpValue;

        //----------------------------------------------------------------------
        public void SetProgress(int value)
        {
            miValue = value;
        }

        //----------------------------------------------------------------------
        public ProgressBar(Screen screen, int value = 0, int max = 100)
        : base(screen)
        {
            Value = value;
            Max = max;
        }

        //----------------------------------------------------------------------
        public override void Update(float elapsedTime)
        {
            float fLerpAmount = Math.Min(1f, elapsedTime * NuclearGame.LerpMultiplier);

            mfLerpValue = MathHelper.Lerp(mfLerpValue, Value, fLerpAmount);
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            Screen.DrawBox(Screen.Style.ProgressBarFrame, LayoutRect, Screen.Style.ProgressBarFrameCornerSize, Color.White);

            if (Value > 0)
            {
                Rectangle progressRect = new Rectangle(LayoutRect.X, LayoutRect.Y, Screen.Style.ProgressBar.Width / 2 + (int)((LayoutRect.Width - Screen.Style.ProgressBar.Width / 2) * mfLerpValue / Max), LayoutRect.Height);
                Screen.DrawBox(Screen.Style.ProgressBar, progressRect, Screen.Style.ProgressBarCornerSize, Color.White);
            }
        }
    }
}
