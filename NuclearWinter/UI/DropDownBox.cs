using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NuclearWinter.Animation;
using NuclearWinter.Collections;
using System;
using System.Collections.Generic;

namespace NuclearWinter.UI
{

    public class DropDownItem
    {
        //----------------------------------------------------------------------
        public string Text
        {
            get { return mLabel.Text; }
            set
            {
                mLabel.Text = value;

                if (DropDownBox != null && DropDownBox.SelectedItem == this)
                {
                    DropDownBox.UpdateLabelText();
                }
            }
        }

        internal DropDownBox DropDownBox;

        Label mLabel;
        public object Tag;

        //----------------------------------------------------------------------
        public DropDownItem(Screen screen, string text, object tag = null)
        {
            mLabel = new Label(screen, text, Anchor.Start);
            Tag = tag;
        }

        internal void DoLayout(Rectangle rectangle)
        {
            mLabel.DoLayout(rectangle);
        }

        internal void Draw()
        {
            mLabel.Draw();
        }
    }

    public class DropDownBox : Widget
    {
        //----------------------------------------------------------------------
        int miSelectedItemIndex;
        public int SelectedItemIndex
        {
            get { return miSelectedItemIndex; }
            set
            {
                miSelectedItemIndex = value;
                if (Items.Count > 0)
                {
                    UpdateLabelText();
                }
            }
        }

        internal void UpdateLabelText()
        {
            mCurrentItemLabel.Text = Items[miSelectedItemIndex].Text;
        }

        public DropDownItem SelectedItem { get { return SelectedItemIndex != -1 ? Items[SelectedItemIndex] : null; } }
        public bool IsOpen { get; private set; }
        public Action<DropDownBox> ChangeHandler;

        //----------------------------------------------------------------------
        public Texture2D ButtonFrame { get; set; }
        public Texture2D ButtonFrameDown { get; set; }
        public Texture2D ButtonFrameHover { get; set; }
        public Texture2D ButtonFramePressed { get; set; }

        //----------------------------------------------------------------------
        public ObservableList<DropDownItem>
                                        Items
        { get; private set; }

        bool mbIsHovered;
        int miHoveredItemIndex;
        Point mHoverPoint;

        AnimatedValue mPressedAnim;
        bool mbIsPressed;

        Rectangle mDropDownHitBox;
        const int siMaxLineDisplayed = 10;

        int miScrollItemOffset;
        public Scrollbar mScrollbar;

        public Box TextPadding;

        Label mCurrentItemLabel;

        //----------------------------------------------------------------------
        int ScrollItemOffset
        {
            get { return miScrollItemOffset; }
            set
            {
                miScrollItemOffset = value;
                mScrollbar.Offset = miScrollItemOffset * (Screen.Style.MediumFont.LineSpacing + Padding.Vertical);
            }
        }

        //----------------------------------------------------------------------
        public DropDownBox(Screen screen, List<DropDownItem> items, int initialValueIndex)
        : base(screen)
        {
            mCurrentItemLabel = new Label(Screen, anchor: Anchor.Start);

            Items = new ObservableList<DropDownItem>(items);

            Items.ListChanged += delegate (object _source, ObservableList<DropDownItem>.ListChangedEventArgs _args)
            {
                if (_args.Added)
                {
                    _args.Item.DropDownBox = this;
                }

                if (SelectedItemIndex == -1)
                {
                    if (_args.Added)
                    {
                        SelectedItemIndex = _args.Index;
                    }
                }
                else
                if (_args.Index <= SelectedItemIndex)
                {
                    SelectedItemIndex = Math.Min(Items.Count - 1, Math.Max(0, SelectedItemIndex + (_args.Added ? 1 : -1)));
                }
            };

            Items.ListCleared += delegate (object _source, EventArgs _args)
            {
                SelectedItemIndex = -1;
            };

            SelectedItemIndex = initialValueIndex;
            mScrollbar = new Scrollbar(screen);
            mScrollbar.Parent = this;

            ScrollItemOffset = Math.Max(0, Math.Min(SelectedItemIndex, Items.Count - siMaxLineDisplayed));
            mScrollbar.LerpOffset = mScrollbar.Offset;

            Padding = Screen.Style.DropDownBoxPadding;
            TextPadding = Screen.Style.DropDownBoxTextPadding;

            mPressedAnim = new SmoothValue(1f, 0f, 0.2f);
            mPressedAnim.SetTime(mPressedAnim.Duration);

            ButtonFrame = Screen.Style.ButtonFrame;
            ButtonFrameDown = Screen.Style.ButtonDownFrame;
            ButtonFrameHover = Screen.Style.ButtonHoverOverlay;
            ButtonFramePressed = Screen.Style.ButtonDownOverlay;

            UpdateContentSize();
        }

        //----------------------------------------------------------------------
        protected internal override void UpdateContentSize()
        {
            UIFont uiFont = Screen.Style.MediumFont;

            int iMaxWidth = 0;
            foreach (DropDownItem _item in Items)
            {
                iMaxWidth = Math.Max(iMaxWidth, (int)uiFont.MeasureString(_item.Text).X);
            }

            ContentWidth = iMaxWidth + Padding.Horizontal + TextPadding.Horizontal + Screen.Style.DropDownArrow.Width;
            ContentHeight = (int)(uiFont.LineSpacing * 0.9f) + Padding.Vertical + TextPadding.Vertical;

            base.UpdateContentSize();
        }

        //----------------------------------------------------------------------
        public override void DoLayout(Rectangle rectangle)
        {
            base.DoLayout(rectangle);
            HitBox = LayoutRect;

            mDropDownHitBox = new Rectangle(
                HitBox.Left, HitBox.Bottom,
                HitBox.Width, Math.Min(siMaxLineDisplayed, Items.Count) * (Screen.Style.MediumFont.LineSpacing + TextPadding.Vertical) + Padding.Vertical);

            mScrollbar.DoLayout(mDropDownHitBox, Items.Count * (Screen.Style.MediumFont.LineSpacing + TextPadding.Vertical));

            mCurrentItemLabel.DoLayout(new Rectangle(LayoutRect.X + TextPadding.Left, LayoutRect.Top + TextPadding.Top, LayoutRect.Width - TextPadding.Horizontal - Screen.Style.DropDownArrow.Width, LayoutRect.Height - TextPadding.Vertical));

            if (IsOpen)
            {
                int iLinesDisplayed = Math.Min(siMaxLineDisplayed, Items.Count);

                int iMaxIndex = Math.Min(Items.Count - 1, ScrollItemOffset + iLinesDisplayed - 1);
                for (int iIndex = ScrollItemOffset; iIndex <= iMaxIndex; iIndex++)
                {
                    Items[iIndex].DoLayout(new Rectangle(LayoutRect.X + TextPadding.Left, LayoutRect.Bottom + (Screen.Style.MediumFont.LineSpacing + TextPadding.Vertical) * (iIndex - ScrollItemOffset) + TextPadding.Top, LayoutRect.Width - TextPadding.Horizontal, Screen.Style.MediumFont.LineSpacing + TextPadding.Vertical + 10));
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

            mScrollbar.Update(elapsedTime);
        }

        //----------------------------------------------------------------------
        public override void OnMouseEnter(Point hitPoint)
        {
            mbIsHovered = true;
        }

        public override void OnMouseOut(Point hitPoint)
        {
            mbIsHovered = false;
        }

        //----------------------------------------------------------------------
        protected internal override bool OnMouseDown(Point hitPoint, int button)
        {
            if (button != Screen.Game.InputMgr.PrimaryMouseButton) return false;

            Screen.Focus(this);

            if (IsOpen && mDropDownHitBox.Contains(hitPoint))
            {
            }
            else
            {
                miHoveredItemIndex = SelectedItemIndex;

                if (miHoveredItemIndex < ScrollItemOffset)
                {
                    ScrollItemOffset = miHoveredItemIndex;
                }
                else
                if (miHoveredItemIndex >= ScrollItemOffset + siMaxLineDisplayed)
                {
                    ScrollItemOffset = Math.Min(miHoveredItemIndex - siMaxLineDisplayed + 1, Items.Count - siMaxLineDisplayed);
                }

                mScrollbar.LerpOffset = mScrollbar.Offset;

                IsOpen = !IsOpen;
                mPressedAnim.SetTime(0f);
            }

            return true;
        }

        protected internal override void OnMouseUp(Point hitPoint, int button)
        {
            if (button != Screen.Game.InputMgr.PrimaryMouseButton) return;

            if (IsOpen && mDropDownHitBox.Contains(hitPoint))
            {
                mHoverPoint = hitPoint;
                UpdateHoveredItem();

                mPressedAnim.SetTime(1f);
                IsOpen = false;
                mbIsPressed = false;

                if (miHoveredItemIndex != -1)
                {
                    SelectedItemIndex = miHoveredItemIndex;
                    if (ChangeHandler != null) ChangeHandler(this);
                }
            }
            else
            if (HitTest(hitPoint) == this)
            {
                OnClick();
            }
            else
            {
                mPressedAnim.SetTime(1f);
                IsOpen = false;
                mbIsPressed = false;
            }
        }

        public override void OnMouseMove(Point hitPoint)
        {
            if (IsOpen && mDropDownHitBox.Contains(hitPoint))
            {
                mHoverPoint = hitPoint;
                UpdateHoveredItem();
            }
            else
            {
                base.OnMouseMove(hitPoint);
            }
        }

        protected internal override void OnMouseWheel(Point hitPoint, int delta)
        {
            if (IsOpen)
            {
                int iNewScrollOffset = (int)MathHelper.Clamp(ScrollItemOffset - delta * 3 / 120, 0, Math.Max(0, Items.Count - siMaxLineDisplayed));
                miHoveredItemIndex += iNewScrollOffset - ScrollItemOffset;
                ScrollItemOffset = iNewScrollOffset;
            }
            else
            {
                base.OnMouseWheel(hitPoint, delta);
            }
        }

        void UpdateHoveredItem()
        {
            miHoveredItemIndex = (int)((mHoverPoint.Y - (LayoutRect.Bottom + Padding.Top)) / (Screen.Style.MediumFont.LineSpacing + TextPadding.Vertical)) + ScrollItemOffset;

            if (miHoveredItemIndex >= Items.Count)
            {
                miHoveredItemIndex = -1;
            }
        }

        //----------------------------------------------------------------------
        void OnClick()
        {
            mPressedAnim.SetTime(0f);
        }

        //----------------------------------------------------------------------
        protected internal override bool OnActivateDown()
        {
            if (IsOpen)
            {
            }
            else
            {
                miHoveredItemIndex = SelectedItemIndex;

                if (miHoveredItemIndex < ScrollItemOffset)
                {
                    ScrollItemOffset = miHoveredItemIndex;
                }
                else
                if (miHoveredItemIndex >= ScrollItemOffset + siMaxLineDisplayed)
                {
                    ScrollItemOffset = Math.Min(miHoveredItemIndex - siMaxLineDisplayed + 1, Items.Count - siMaxLineDisplayed);
                }

                mbIsPressed = true;
                mPressedAnim.SetTime(0f);
            }

            return true;
        }

        protected internal override void OnActivateUp()
        {
            if (IsOpen)
            {
                if (miHoveredItemIndex != -1)
                {
                    SelectedItemIndex = miHoveredItemIndex;
                    if (ChangeHandler != null) ChangeHandler(this);
                }

                mPressedAnim.SetTime(1f);
                IsOpen = false;
                mbIsPressed = false;
            }
            else
            {
                IsOpen = true;
            }
        }

        protected internal override bool OnCancel(bool pressed)
        {
            if (IsOpen)
            {
                if (!pressed) OnBlur();
                return true;
            }
            else
            {
                return false;
            }
        }

        //----------------------------------------------------------------------
        protected internal override void OnBlur()
        {
            mPressedAnim.SetTime(1f);
            IsOpen = false;
            mbIsPressed = false;
        }

        //----------------------------------------------------------------------
        protected internal override void OnPadMove(Direction direction)
        {
            if (!IsOpen)
            {
                base.OnPadMove(direction);
                return;
            }

            if (direction == Direction.Up)
            {
                miHoveredItemIndex = Math.Max(0, miHoveredItemIndex - 1);

                if (miHoveredItemIndex < ScrollItemOffset)
                {
                    ScrollItemOffset = miHoveredItemIndex;
                }
            }
            else if (direction == Direction.Down)
            {
                miHoveredItemIndex = Math.Min(Items.Count - 1, miHoveredItemIndex + 1);

                if (miHoveredItemIndex >= ScrollItemOffset + siMaxLineDisplayed)
                {
                    ScrollItemOffset = Math.Min(miHoveredItemIndex - siMaxLineDisplayed + 1, Items.Count - siMaxLineDisplayed);
                }
            }
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            DrawWithOffset(Point.Zero);
        }

        public void DrawWithOffset(Point offset)
        {
            Rectangle rect = LayoutRect;
            rect.Offset(offset);

            if (ButtonFrame != null)
            {
                Screen.DrawBox((!IsOpen && !mbIsPressed) ? ButtonFrame : ButtonFrameDown, rect, Screen.Style.ButtonCornerSize, Color.White);
            }

            if (mbIsHovered && !IsOpen && mPressedAnim.IsOver)
            {
                if (Screen.IsActive)
                {
                    Screen.DrawBox(ButtonFrameHover, rect, Screen.Style.ButtonCornerSize, Color.White);
                }
            }
            else
            {
                Screen.DrawBox(ButtonFramePressed, rect, Screen.Style.ButtonCornerSize, Color.White * mPressedAnim.CurrentValue);
            }

            if (Screen.IsActive && HasFocus && !IsOpen)
            {
                Screen.DrawBox(Screen.Style.ButtonFocusOverlay, rect, Screen.Style.ButtonCornerSize, Color.White);
            }

            Screen.Game.SpriteBatch.Draw(Screen.Style.DropDownArrow,
                new Vector2(rect.Right - Padding.Right - TextPadding.Right - Screen.Style.DropDownArrow.Width, rect.Center.Y - Screen.Style.DropDownArrow.Height / 2),
                Color.White
            );

            mCurrentItemLabel.DrawWithOffset(offset);
        }

        //----------------------------------------------------------------------
        public override Widget HitTest(Point point)
        {
            if (HasFocus && IsOpen)
            {
                return /*mScrollbar.HitTest( _point ) ??*/ this;
            }

            return base.HitTest(point);
        }

        //----------------------------------------------------------------------
        protected internal override void DrawFocused()
        {
            if (IsOpen)
            {
                int iLinesDisplayed = Math.Min(siMaxLineDisplayed, Items.Count);

                var rect = new Rectangle(LayoutRect.X, LayoutRect.Bottom, LayoutRect.Width, iLinesDisplayed * (Screen.Style.MediumFont.LineSpacing + TextPadding.Vertical) + Padding.Vertical);
                Screen.DrawBox(Screen.Style.ListViewStyle.ListViewFrame, rect, Screen.Style.ListViewStyle.ListViewFrameCornerSize, Color.White);

                int iMaxIndex = Math.Min(Items.Count - 1, ScrollItemOffset + iLinesDisplayed - 1);
                for (int iIndex = ScrollItemOffset; iIndex <= iMaxIndex; iIndex++)
                {
                    if (Screen.IsActive && miHoveredItemIndex == iIndex)
                    {
                        Screen.DrawBox(Screen.Style.DropDownBoxEntryHoverOverlay, new Rectangle(LayoutRect.X + TextPadding.Left, LayoutRect.Bottom + (Screen.Style.MediumFont.LineSpacing + TextPadding.Vertical) * (iIndex - ScrollItemOffset) + TextPadding.Top, LayoutRect.Width - TextPadding.Horizontal, Screen.Style.MediumFont.LineSpacing + TextPadding.Vertical + 10), 10, Color.White);
                    }

                    Items[iIndex].Draw();
                }

                mScrollbar.Draw();
            }
        }

    }
}
