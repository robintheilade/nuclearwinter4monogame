using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;

namespace NuclearWinter.UI
{
    public class Slider : Widget
    {
        bool mbIsHovered;
        bool mbIsPressed;

        // FIXME: There should be a IntSlider/FloatSlider or a Discrete/Continuous setting for the Slider
        public int MinValue;
        public int MaxValue;
        public int Step = 1;

        int miValue;
        public int Value
        {
            get
            {
                return miValue;
            }

            set
            {
                miValue = (int)MathHelper.Clamp(value, MinValue, MaxValue);
                miValue -= miValue % Step;

                mTooltip.Text = miValue.ToString();
            }
        }

        public Action ChangeHandler;

        Tooltip mTooltip;

        // Style
        public Texture2D Frame;
        public Texture2D HandleFrame;
        public Texture2D HandleDownFrame;
        public Texture2D HandleHoverOverlay;
        public Texture2D HandleFocusOverlay;

        //----------------------------------------------------------------------
        public Slider(Screen screen, int min, int max, int initialValue, int step)
        : base(screen)
        {
            Frame = Screen.Style.SliderFrame;
            HandleFrame = Screen.Style.ButtonFrame;
            HandleDownFrame = Screen.Style.ButtonDownFrame;
            HandleHoverOverlay = Screen.Style.ButtonHoverOverlay;
            HandleFocusOverlay = Screen.Style.ButtonFocusOverlay;

            Debug.Assert(min < max);

            mTooltip = new Tooltip(Screen, "");

            MinValue = min;
            MaxValue = max;
            Value = initialValue;
            Step = step;

            UpdateContentSize();
        }

        //----------------------------------------------------------------------
        protected internal override void UpdateContentSize()
        {
            // No content size
        }

        //----------------------------------------------------------------------
        public override void DoLayout(Rectangle rectangle)
        {
            base.DoLayout(rectangle);
            HitBox = LayoutRect;
        }

        //----------------------------------------------------------------------
        public override void OnMouseEnter(Point hitPoint)
        {
            mbIsHovered = true;
        }

        public override void OnMouseOut(Point hitPoint)
        {
            mbIsHovered = false;
            mTooltip.EnableDisplayTimer = false;
        }

        //----------------------------------------------------------------------
        protected internal override bool OnMouseDown(Point hitPoint, int button)
        {
            if (button != Screen.Game.InputMgr.PrimaryMouseButton) return false;

            Screen.Focus(this);
            mbIsPressed = true;
            mTooltip.DisplayNow();

            int iWidth = LayoutRect.Width - Screen.Style.SliderHandleSize;
            int iX = hitPoint.X - LayoutRect.X - Screen.Style.SliderHandleSize / 2;
            float fProgress = (float)iX / iWidth;

            Value = MinValue + (int)Math.Floor(fProgress * (MaxValue - MinValue) / Step + 0.5f) * Step;

            if (ChangeHandler != null) ChangeHandler();

            return true;
        }

        public override void OnMouseMove(Point hitPoint)
        {
            if (mbIsPressed)
            {
                int iWidth = LayoutRect.Width - Screen.Style.SliderHandleSize;
                int iX = hitPoint.X - LayoutRect.X - Screen.Style.SliderHandleSize / 2;
                float fProgress = (float)iX / iWidth;

                int iValue = MinValue + (int)Math.Floor(fProgress * (MaxValue - MinValue) / Step + 0.5f) * Step;

                if (iValue != miValue)
                {
                    Value = iValue;
                    if (ChangeHandler != null) ChangeHandler();
                }
            }
        }

        protected internal override void OnMouseUp(Point hitPoint, int button)
        {
            if (button != Screen.Game.InputMgr.PrimaryMouseButton) return;

            mbIsPressed = false;
        }

        //----------------------------------------------------------------------
        protected internal override void OnPadMove(Direction direction)
        {
            if (direction == Direction.Left)
            {
                Value -= Step;
                if (ChangeHandler != null) ChangeHandler();
            }
            else
            if (direction == Direction.Right)
            {
                Value += Step;
                if (ChangeHandler != null) ChangeHandler();
            }
            else
            {
                base.OnPadMove(direction);
            }
        }

        //----------------------------------------------------------------------
        public override void Update(float elapsedTime)
        {
            mTooltip.EnableDisplayTimer = mbIsHovered;
            mTooltip.Update(elapsedTime);
        }

        public override void Draw()
        {
            Rectangle rect = new Rectangle(LayoutRect.X, LayoutRect.Center.Y - Screen.Style.SliderHandleSize / 2, LayoutRect.Width, Screen.Style.SliderHandleSize);

            Screen.DrawBox(Frame, rect, Screen.Style.SliderFrameCornerSize, Color.White);

            int handleX = rect.X + (int)((rect.Width - Screen.Style.SliderHandleSize) * (float)(Value - MinValue) / (MaxValue - MinValue));

            Screen.DrawBox((!mbIsPressed) ? HandleFrame : HandleDownFrame, new Rectangle(handleX, rect.Y, Screen.Style.SliderHandleSize, Screen.Style.SliderHandleSize), Screen.Style.ButtonCornerSize, Color.White);
            if (Screen.IsActive && mbIsHovered && !mbIsPressed)
            {
                Screen.DrawBox(HandleHoverOverlay, new Rectangle(handleX, rect.Y, Screen.Style.SliderHandleSize, Screen.Style.SliderHandleSize), Screen.Style.ButtonCornerSize, Color.White);
            }

            if (Screen.IsActive && HasFocus && !mbIsPressed)
            {
                Screen.DrawBox(HandleFocusOverlay, new Rectangle(handleX, rect.Y, Screen.Style.SliderHandleSize, Screen.Style.SliderHandleSize), Screen.Style.ButtonCornerSize, Color.White);
            }
        }

        //----------------------------------------------------------------------
        protected internal override void DrawHovered()
        {
            mTooltip.Draw();
        }
    }
}
