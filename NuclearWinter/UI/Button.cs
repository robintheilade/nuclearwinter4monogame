using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NuclearWinter.Animation;
using System;

namespace NuclearWinter.UI
{
    /*
     * A clickable Button containing a Label and an optional Image icon
     */
    public class Button : Widget
    {
        //----------------------------------------------------------------------
        public struct ButtonStyle
        {
            //------------------------------------------------------------------
            public int CornerSize;
            public Texture2D Frame;
            public Texture2D DownFrame;
            public Texture2D HoverOverlay;
            public Texture2D DownOverlay;
            public Texture2D FocusOverlay;
            public int VerticalPadding;
            public int HorizontalPadding;

            //------------------------------------------------------------------
            public ButtonStyle(
                int _iCornerSize,
                Texture2D _buttonFrame,
                Texture2D _buttonFrameDown,
                Texture2D _buttonFrameHover,
                Texture2D _buttonFramePressed,
                Texture2D _buttonFrameFocused,
                int _iVerticalPadding,
                int _iHorizontalPadding
            )
            {
                CornerSize = _iCornerSize;
                Frame = _buttonFrame;
                DownFrame = _buttonFrameDown;
                HoverOverlay = _buttonFrameHover;
                DownOverlay = _buttonFramePressed;
                FocusOverlay = _buttonFrameFocused;

                VerticalPadding = _iVerticalPadding;
                HorizontalPadding = _iHorizontalPadding;
            }
        }

        public UIFont Font
        {
            get { return mLabel.Font; }
            set { mLabel.Font = value; UpdateContentSize(); }
        }

        //----------------------------------------------------------------------
        Label mLabel;
        Image mIcon;

        bool mbIsHovered;

        AnimatedValue mPressedAnim;
        bool mbIsPressed;

        protected Box mMargin;
        public Box Margin
        {
            get { return mMargin; }

            set
            {
                mMargin = value;
                UpdateContentSize();
            }
        }

        public string Text
        {
            get
            {
                return mLabel.Text;
            }

            set
            {
                mLabel.Text = value;
                mLabel.Padding = mLabel.Text != "" ? new Box(Style.VerticalPadding, Style.HorizontalPadding) : new Box(Style.VerticalPadding, Style.HorizontalPadding, Style.VerticalPadding, 0);
                UpdateContentSize();
            }
        }

        public Texture2D Icon
        {
            get
            {
                return mIcon.Texture;
            }

            set
            {
                mIcon.Texture = value;
                UpdateContentSize();
            }
        }

        public Color IconColor
        {
            get
            {
                return mIcon.Color;
            }

            set
            {
                mIcon.Color = value;
            }
        }


        Anchor mAnchor;
        public Anchor Anchor
        {
            get
            {
                return mAnchor;
            }

            set
            {
                mAnchor = value;
            }
        }

        public Color TextColor
        {
            get { return mLabel.Color; }
            set { mLabel.Color = value; }
        }

        public ButtonStyle Style;

        public Action<Button> ClickHandler;
        public object Tag;

        public string TooltipText
        {
            get { return mTooltip.Text; }
            set { mTooltip.Text = value; }
        }

        //----------------------------------------------------------------------
        Tooltip mTooltip;

        //----------------------------------------------------------------------
        public Button(Screen screen, ButtonStyle style, string text = "", Texture2D iconTex = null, Anchor anchor = Anchor.Center, string tooltipText = "", object tag = null)
        : base(screen)
        {
            Style = style;

            mPadding = new Box(0);
            mMargin = new Box(0);

            mLabel = new Label(screen);

            mIcon = new Image(screen);
            mIcon.Texture = iconTex;
            mIcon.Padding = new Box(Style.VerticalPadding, 0, Style.VerticalPadding, Style.HorizontalPadding);

            Text = text;
            TextColor = Screen.Style.DefaultTextColor;

            Anchor = anchor;

            mPressedAnim = new SmoothValue(1f, 0f, 0.2f);
            mPressedAnim.SetTime(mPressedAnim.Duration);

            mTooltip = new Tooltip(Screen, "");

            TooltipText = tooltipText;
            Tag = tag;

            UpdateContentSize();
        }

        //----------------------------------------------------------------------
        public Button(Screen screen, string text = "", Texture2D iconTex = null, Anchor anchor = Anchor.Center, string tooltipText = "", object tag = null)
        : this(screen, new ButtonStyle(
                screen.Style.ButtonCornerSize,
                screen.Style.ButtonFrame,
                screen.Style.ButtonDownFrame,
                screen.Style.ButtonHoverOverlay,
                screen.Style.ButtonDownOverlay,
                screen.Style.ButtonFocusOverlay,
                screen.Style.ButtonVerticalPadding,
                screen.Style.ButtonHorizontalPadding
            ), text, iconTex, anchor, tooltipText, tag)
        {
        }

        //----------------------------------------------------------------------
        protected internal override void UpdateContentSize()
        {
            ContentWidth = ((mIcon.Texture != null) ? mIcon.ContentWidth : 0) + mLabel.ContentWidth + Padding.Horizontal + mMargin.Horizontal;
            ContentHeight = Math.Max(mIcon.ContentHeight, mLabel.ContentHeight) + Padding.Vertical + mMargin.Vertical;

            base.UpdateContentSize();
        }

        //----------------------------------------------------------------------
        public override void DoLayout(Rectangle rectangle)
        {
            base.DoLayout(rectangle);
            HitBox = LayoutRect;
            Point pCenter = LayoutRect.Center;

            switch (mAnchor)
            {
                case UI.Anchor.Start:
                    if (mIcon.Texture != null)
                    {
                        mIcon.DoLayout(new Rectangle(LayoutRect.X + Padding.Left + Margin.Left, pCenter.Y - mIcon.ContentHeight / 2, mIcon.ContentWidth, mIcon.ContentHeight));
                    }

                    mLabel.DoLayout(
                        new Rectangle(
                            LayoutRect.X + Padding.Left + Margin.Left + (mIcon.Texture != null ? mIcon.ContentWidth : 0), pCenter.Y - mLabel.ContentHeight / 2,
                            mLabel.ContentWidth, mLabel.ContentHeight
                        )
                    );
                    break;
                case UI.Anchor.Center:
                    {
                        int iLabelWidth = mLabel.ContentWidth;
                        int iLabelOffset = ContentWidth;

                        if (mIcon.Texture != null)
                        {
                            mIcon.DoLayout(new Rectangle(pCenter.X - ContentWidth / 2 + Padding.Left + Margin.Left, pCenter.Y - mIcon.ContentHeight / 2, mIcon.ContentWidth, mIcon.ContentHeight));
                        }
                        else
                        {
                            iLabelWidth = Math.Min(mLabel.ContentWidth - mLabel.Padding.Horizontal, LayoutRect.Width) + mLabel.Padding.Horizontal;
                            iLabelOffset += iLabelWidth - mLabel.ContentWidth;
                        }

                        mLabel.DoLayout(

                            new Rectangle(
                                pCenter.X - iLabelOffset / 2 + Padding.Left + Margin.Left + (mIcon.Texture != null ? mIcon.ContentWidth : 0), pCenter.Y - mLabel.ContentHeight / 2,
                                iLabelWidth, mLabel.ContentHeight
                            )
                        );
                        break;
                    }
            }
        }

        //----------------------------------------------------------------------
        public override void Update(float elapsedTime)
        {
            if (!mPressedAnim.IsOver)
            {
                mPressedAnim.Update(elapsedTime);
            }

            mTooltip.EnableDisplayTimer = mbIsHovered;
            mTooltip.Update(elapsedTime);
        }

        //----------------------------------------------------------------------
        public override void OnMouseEnter(Point hitPoint)
        {
            base.OnMouseEnter(hitPoint);
            mbIsHovered = true;
        }

        public override void OnMouseOut(Point hitPoint)
        {
            base.OnMouseOut(hitPoint);
            mbIsHovered = false;
            mTooltip.EnableDisplayTimer = false;
        }

        //----------------------------------------------------------------------
        protected internal override bool OnMouseDown(Point hitPoint, int button)
        {
            if (button != Screen.Game.InputMgr.PrimaryMouseButton) return false;

            Screen.Focus(this);
            OnActivateDown();

            return true;
        }

        protected internal override void OnMouseUp(Point hitPoint, int button)
        {
            if (button != Screen.Game.InputMgr.PrimaryMouseButton) return;

            if (HitTest(hitPoint) == this)
            {
                OnActivateUp();
            }
            else
            {
                ResetPressState();
            }
        }

        //----------------------------------------------------------------------
        protected internal override bool OnActivateDown()
        {
            mbIsPressed = true;
            mPressedAnim.SetTime(0f);
            return true;
        }

        protected internal override void OnActivateUp()
        {
            mPressedAnim.SetTime(0f);
            mbIsPressed = false;
            if (ClickHandler != null) ClickHandler(this);
        }

        protected internal override void OnBlur()
        {
            ResetPressState();
        }

        //----------------------------------------------------------------------
        internal void ResetPressState()
        {
            mPressedAnim.SetTime(1f);
            mbIsPressed = false;
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            Texture2D frame = (!mbIsPressed) ? Style.Frame : Style.DownFrame;

            if (frame != null)
            {
                Screen.DrawBox(frame, LayoutRect, Style.CornerSize, Color.White);
            }

            Rectangle marginRect = new Rectangle(LayoutRect.X + Margin.Left, LayoutRect.Y + Margin.Top, LayoutRect.Width - Margin.Left - Margin.Right, LayoutRect.Height - Margin.Top - Margin.Bottom);

            if (mbIsHovered && !mbIsPressed && mPressedAnim.IsOver)
            {
                if (Screen.IsActive && Style.HoverOverlay != null)
                {
                    Screen.DrawBox(Style.HoverOverlay, marginRect, Style.CornerSize, Color.White);
                }
            }
            else
            if (mPressedAnim.CurrentValue > 0f)
            {
                if (Style.DownOverlay != null)
                {
                    Screen.DrawBox(Style.DownOverlay, marginRect, Style.CornerSize, Color.White * mPressedAnim.CurrentValue);
                }
            }

            if (Screen.IsActive && HasFocus && !mbIsPressed)
            {
                if (Style.FocusOverlay != null)
                {
                    Screen.DrawBox(Style.FocusOverlay, marginRect, Style.CornerSize, Color.White);
                }
            }

            mLabel.Draw();

            mIcon.Draw();
        }

        //----------------------------------------------------------------------
        protected internal override void DrawHovered()
        {
            mTooltip.Draw();
        }
    }
}
