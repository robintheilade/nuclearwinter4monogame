using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NuclearWinter.UI
{
    public class RadioButtonSet : Widget
    {
        //----------------------------------------------------------------------
        public struct RadioButtonSetStyle
        {
            //------------------------------------------------------------------
            public int CornerSize;
            public int FrameOffset;

            public Color TextColor;
            public Color TextDownColor;

            public Texture2D ButtonFrameLeft;
            public Texture2D ButtonFrameMiddle;
            public Texture2D ButtonFrameRight;

            public Texture2D ButtonDownFrameLeft;
            public Texture2D ButtonDownFrameMiddle;
            public Texture2D ButtonDownFrameRight;

            //------------------------------------------------------------------
            public RadioButtonSetStyle(
                int cornerSize,
                int frameOffset,

                Color textColor,
                Color textDownColor,

                Texture2D buttonFrameLeft,
                Texture2D buttonFrameMiddle,
                Texture2D buttonFrameRight,

                Texture2D buttonDownFrameLeft,
                Texture2D buttonDownFrameMiddle,
                Texture2D buttonDownFrameRight
            )
            {
                CornerSize = cornerSize;
                FrameOffset = frameOffset;

                TextColor = textColor;
                TextDownColor = textDownColor;

                ButtonFrameLeft = buttonFrameLeft;
                ButtonFrameMiddle = buttonFrameMiddle;
                ButtonFrameRight = buttonFrameRight;

                ButtonDownFrameLeft = buttonDownFrameLeft;
                ButtonDownFrameMiddle = buttonDownFrameMiddle;
                ButtonDownFrameRight = buttonDownFrameRight;
            }
        }

        //----------------------------------------------------------------------
        List<Button> mlButtons;
        public IList<Button> Buttons { get { return mlButtons.AsReadOnly(); } }

        int miHoveredButton;
        bool mbIsPressed;

        RadioButtonSetStyle mStyle;
        public RadioButtonSetStyle Style
        {
            get { return mStyle; }
            set
            {
                mStyle = value;

                int i = 0;
                foreach (Button button in mlButtons)
                {
                    button.Parent = this;
                    button.TextColor = (SelectedButtonIndex == i) ? mStyle.TextDownColor : mStyle.TextColor;
                    button.Padding = new Box(0, mStyle.FrameOffset);
                    button.Margin = new Box(0, -mStyle.FrameOffset);

                    button.Style.DownFrame = Style.ButtonDownFrameMiddle;
                    button.ClickHandler = ButtonClicked;

                    i++;
                }

                Button firstButton = mlButtons.First();
                firstButton.Style.DownFrame = Style.ButtonDownFrameLeft;
                firstButton.Margin = new Box(0, -mStyle.FrameOffset, 0, 0);

                Button lastButton = mlButtons.Last();
                lastButton.Style.DownFrame = Style.ButtonDownFrameRight;
                lastButton.Margin = new Box(0, 0, 0, -mStyle.FrameOffset);
            }
        }

        public bool Expand;

        public Action<RadioButtonSet, int> ClickHandler;
        int miSelectedButtonIndex = 0;
        public int SelectedButtonIndex
        {
            get
            {
                return miSelectedButtonIndex;
            }

            set
            {
                miSelectedButtonIndex = value;

                for (int iButton = 0; iButton < mlButtons.Count; iButton++)
                {
                    Button button = mlButtons[iButton];

                    button.Style.CornerSize = Style.CornerSize;

                    if (iButton == miSelectedButtonIndex)
                    {
                        button.TextColor = mStyle.TextDownColor;
                        if (iButton == 0)
                        {
                            button.Style.Frame = Style.ButtonDownFrameLeft;
                        }
                        else
                        if (iButton == mlButtons.Count - 1)
                        {
                            button.Style.Frame = Style.ButtonDownFrameRight;
                        }
                        else
                        {
                            button.Style.Frame = Style.ButtonDownFrameMiddle;
                        }
                    }
                    else
                    {
                        button.TextColor = mStyle.TextColor;

                        if (iButton == 0)
                        {
                            button.Style.Frame = Style.ButtonFrameLeft;
                        }
                        else
                        if (iButton == mlButtons.Count - 1)
                        {
                            button.Style.Frame = Style.ButtonFrameRight;
                        }
                        else
                        {
                            button.Style.Frame = Style.ButtonFrameMiddle;
                        }
                    }
                }
            }
        }

        public Button SelectedButton
        {
            get { return mlButtons[miSelectedButtonIndex]; }
        }

        //----------------------------------------------------------------------
        public override Widget GetFirstFocusableDescendant(Direction direction)
        {
            switch (direction)
            {
                case Direction.Left:
                    return mlButtons[mlButtons.Count - 1];
                default:
                    return mlButtons[0];
            }
        }

        //----------------------------------------------------------------------
        public override Widget GetSibling(Direction direction, Widget child)
        {
            int iIndex = mlButtons.IndexOf((Button)child);

            switch (direction)
            {
                case Direction.Left:
                    if (iIndex > 0)
                    {
                        return mlButtons[iIndex - 1];
                    }
                    break;
                case Direction.Right:
                    if (iIndex < mlButtons.Count - 1)
                    {
                        return mlButtons[iIndex + 1];
                    }
                    break;
            }

            return base.GetSibling(direction, this);
        }

        //----------------------------------------------------------------------
        public RadioButtonSet(Screen screen, List<Button> buttons, int initialButtonIndex, bool expand = false)
        : base(screen)
        {
            mlButtons = buttons;

            Style = new RadioButtonSetStyle(
                Screen.Style.RadioButtonCornerSize,
                Screen.Style.RadioButtonFrameOffset,

                Color.White * 0.6f,
                Color.White,
                Screen.Style.ButtonFrameLeft,
                Screen.Style.ButtonFrameMiddle,
                Screen.Style.ButtonFrameRight,

                Screen.Style.ButtonDownFrameLeft,
                Screen.Style.ButtonDownFrameMiddle,
                Screen.Style.ButtonDownFrameRight
            );

            SelectedButtonIndex = initialButtonIndex;
            Expand = expand;

            UpdateContentSize();
        }

        public RadioButtonSet(Screen screen, List<Button> buttons, bool expand = false)
        : this(screen, buttons, 0, expand)
        {
        }

        //----------------------------------------------------------------------
        public RadioButtonSet(Screen screen, RadioButtonSetStyle style, List<Button> buttons, int initialButtonIndex)
        : base(screen)
        {
            mlButtons = buttons;

            Style = style;

            SelectedButtonIndex = initialButtonIndex;

            UpdateContentSize();
        }

        public RadioButtonSet(Screen screen, RadioButtonSetStyle style, List<Button> buttons)
        : this(screen, style, buttons, 0)
        {
        }

        //----------------------------------------------------------------------
        protected internal override void UpdateContentSize()
        {
            ContentWidth = Padding.Horizontal;
            ContentHeight = 0;
            foreach (Button button in mlButtons)
            {
                ContentWidth += button.ContentWidth;
                ContentHeight = Math.Max(ContentHeight, button.ContentHeight);
            }

            ContentHeight += Padding.Vertical;

            base.UpdateContentSize();
        }

        //----------------------------------------------------------------------
        public override void DoLayout(Rectangle rectangle)
        {
            base.DoLayout(rectangle);

            Point pCenter = LayoutRect.Center;

            int iHeight = LayoutRect.Height;

            HitBox = new Rectangle(
                pCenter.X - (Expand ? LayoutRect.Width : ContentWidth) / 2,
                pCenter.Y - iHeight / 2,
                Expand ? LayoutRect.Width : ContentWidth,
                iHeight
            );

            float fExpandedButtonWidth = (float)LayoutRect.Width / mlButtons.Count;

            int iButton = 0;
            int iButtonX = 0;

            float fOffset = 0f;

            foreach (Button button in mlButtons)
            {
                int iWidth = button.ContentWidth;

                if (Expand)
                {
                    if (iButton < mlButtons.Count - 1)
                    {
                        iWidth = (int)Math.Floor(fExpandedButtonWidth + fOffset - Math.Floor(fOffset));
                    }
                    else
                    {
                        iWidth = (int)(LayoutRect.Width - Math.Floor(fOffset));
                    }
                    fOffset += fExpandedButtonWidth;
                }
                else
                {
                    fOffset += iWidth;
                }

                button.DoLayout(new Rectangle(
                    HitBox.X + iButtonX, pCenter.Y - iHeight / 2,
                    iWidth, iHeight
                ));

                iButtonX += iWidth;
                iButton++;
            }
        }

        public override void Update(float elapsedTime)
        {
            foreach (Button button in mlButtons)
            {
                button.Update(elapsedTime);
            }
        }

        //----------------------------------------------------------------------
        public override void OnMouseEnter(Point hitPoint)
        {
            base.OnMouseEnter(hitPoint);
            UpdateHoveredButton(hitPoint);

            mlButtons[miHoveredButton].OnMouseEnter(hitPoint);
        }

        public override void OnMouseOut(Point hitPoint)
        {
            base.OnMouseOut(hitPoint);

            mlButtons[miHoveredButton].OnMouseOut(hitPoint);
        }

        public override void OnMouseMove(Point hitPoint)
        {
            base.OnMouseMove(hitPoint);

            if (!mbIsPressed)
            {
                int iPreviousHoveredButton = miHoveredButton;
                UpdateHoveredButton(hitPoint);

                if (iPreviousHoveredButton != miHoveredButton)
                {
                    mlButtons[iPreviousHoveredButton].OnMouseOut(hitPoint);
                }
                mlButtons[miHoveredButton].OnMouseEnter(hitPoint);
            }
            else
            {
                mlButtons[miHoveredButton].OnMouseMove(hitPoint);
            }
        }

        void UpdateHoveredButton(Point hitPoint)
        {
            int iButton = 0;
            foreach (Button button in mlButtons)
            {
                if (button.HitTest(hitPoint) != null)
                {
                    miHoveredButton = iButton;
                    break;
                }

                iButton++;
            }
        }

        //----------------------------------------------------------------------
        protected internal override bool OnMouseDown(Point hitPoint, int button)
        {
            if (button != Screen.Game.InputMgr.PrimaryMouseButton) return false;

            mbIsPressed = true;
            mlButtons[miHoveredButton].OnMouseDown(hitPoint, button);

            return true;
        }

        protected internal override void OnMouseUp(Point hitPoint, int button)
        {
            if (button != Screen.Game.InputMgr.PrimaryMouseButton) return;

            mbIsPressed = false;
            mlButtons[miHoveredButton].OnMouseUp(hitPoint, button);
        }

        internal void ButtonClicked(Button button)
        {
            int iSelectedButtonIndex = mlButtons.IndexOf(button);
            if (ClickHandler != null)
            {
                ClickHandler(this, iSelectedButtonIndex);
            }
            SelectedButtonIndex = iSelectedButtonIndex;
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            foreach (Button button in mlButtons)
            {
                button.Draw();
            }
        }

        protected internal override void DrawHovered()
        {
            mlButtons[miHoveredButton].DrawHovered();
        }
    }
}
