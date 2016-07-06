using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace NuclearWinter.UI
{
    //--------------------------------------------------------------------------
    public enum CheckBoxState
    {
        Unchecked,
        Checked,
        Inconsistent
    }

    //--------------------------------------------------------------------------
    public class CheckBox : Widget
    {
        //----------------------------------------------------------------------
        public string Text
        {
            get { return mLabel.Text; }
            set { mLabel.Text = value; }
        }

        public CheckBoxState CheckState;
        public Action<CheckBox, CheckBoxState> ChangeHandler;

        public Texture2D Frame;
        public int FrameCornerSize;

        public object Tag;

        //----------------------------------------------------------------------
        Label mLabel;
        bool mbIsHovered;
        Rectangle mCheckBoxRect;

        //----------------------------------------------------------------------
        public CheckBox(Screen screen, string text, object tag = null)
        : base(screen)
        {
            Frame = Screen.Style.CheckBoxFrame;
            FrameCornerSize = Screen.Style.CheckBoxFrameCornerSize;

            mLabel = new Label(Screen, text, Anchor.Start);
            mLabel.Padding = new Box(0, 0, 0, Screen.Style.CheckBoxLabelSpacing);

            Padding = Screen.Style.CheckBoxPadding;

            Tag = tag;

            UpdateContentSize();
        }

        //----------------------------------------------------------------------
        protected internal override void UpdateContentSize()
        {
            ContentWidth = Padding.Horizontal + Screen.Style.CheckBoxSize + mLabel.ContentWidth;

            base.UpdateContentSize();
        }

        //----------------------------------------------------------------------
        public override void DoLayout(Rectangle rectangle)
        {
            base.DoLayout(rectangle);

            mCheckBoxRect = new Rectangle(LayoutRect.X + Padding.Left, LayoutRect.Center.Y - Screen.Style.CheckBoxSize / 2, Screen.Style.CheckBoxSize, Screen.Style.CheckBoxSize);
            mLabel.DoLayout(new Rectangle(LayoutRect.X + Padding.Left + Screen.Style.CheckBoxSize, LayoutRect.Y + Padding.Top, LayoutRect.Width - Padding.Horizontal - Screen.Style.CheckBoxSize, LayoutRect.Height - Padding.Vertical));

            UpdateContentSize();
        }

        //----------------------------------------------------------------------
        public override Widget HitTest(Point point)
        {
            return (mCheckBoxRect.Contains(point) || mLabel.HitBox.Contains(point)) ? this : null;
        }

        //----------------------------------------------------------------------
        public override void OnMouseMove(Point hitPoint)
        {
            mbIsHovered = mCheckBoxRect.Contains(hitPoint) || mLabel.LayoutRect.Contains(hitPoint);
        }

        //----------------------------------------------------------------------
        public override void OnMouseOut(Point hitPoint)
        {
            mbIsHovered = false;
        }

        protected internal override void OnMouseUp(Point hitPoint, int button)
        {
            if (mbIsHovered)
            {
                OnActivateUp();
            }
        }

        protected internal override bool OnActivateDown()
        {
            return true;
        }

        protected internal override void OnActivateUp()
        {
            CheckBoxState newState = (CheckState == CheckBoxState.Checked) ? CheckBoxState.Unchecked : CheckBoxState.Checked;
            if (ChangeHandler != null) ChangeHandler(this, newState);
            CheckState = newState;
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            DrawWithOffset(Point.Zero);
        }

        //----------------------------------------------------------------------
        public void DrawWithOffset(Point offset)
        {
            var rect = mCheckBoxRect;
            rect.Offset(offset);
            Screen.DrawBox(Frame, rect, FrameCornerSize, Color.White);

            if (mbIsHovered)
            {
                Screen.DrawBox(Screen.Style.CheckBoxFrameHover, rect, FrameCornerSize, Color.White);
            }

            Texture2D tex;

            switch (CheckState)
            {
                case UI.CheckBoxState.Checked:
                    tex = Screen.Style.CheckBoxChecked;
                    break;
                case UI.CheckBoxState.Unchecked:
                    tex = Screen.Style.CheckBoxUnchecked;
                    break;
                case UI.CheckBoxState.Inconsistent:
                    tex = Screen.Style.CheckBoxInconsistent;
                    break;
                default:
                    throw new NotSupportedException();
            }

            Screen.Game.SpriteBatch.Draw(tex, new Vector2(rect.Center.X, rect.Center.Y), null, Color.White, 0f, new Vector2(tex.Width, tex.Height) / 2f, 1f, SpriteEffects.None, 1f);

            mLabel.DrawWithOffset(offset);
        }

        protected internal override void DrawFocused()
        {
            Screen.DrawBox(Screen.Style.ButtonFocusOverlay, mLabel.LayoutRect, Screen.Style.ButtonCornerSize, Color.White);
        }
    }
}
