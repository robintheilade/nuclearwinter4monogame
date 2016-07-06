using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace NuclearWinter.UI
{
    /*
     * A widget that draws a box with the specified texture & corner size
     * Can also contain stuff
     */
    public class Panel : Group
    {
        public Texture2D Texture;
        public int CornerSize;

        public bool DoClipping;

        bool mbEnableScrolling;
        public bool EnableScrolling
        {
            get { return mbEnableScrolling; }
            set
            {
                mbEnableScrolling = value;
                HasDynamicHeight = mbEnableScrolling;
            }
        }

        public Scrollbar Scrollbar { get; private set; }

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

        //----------------------------------------------------------------------
        public Panel(Screen screen, Texture2D texture, int cornerSize)
        : base(screen)
        {
            Texture = texture;
            CornerSize = cornerSize;
            Padding = new Box(CornerSize);

            Scrollbar = new Scrollbar(screen);
            Scrollbar.Parent = this;
        }

        //----------------------------------------------------------------------
        public override void Update(float elapsedTime)
        {
            if (EnableScrolling)
            {
                Scrollbar.Update(elapsedTime);
            }

            base.Update(elapsedTime);
        }

        //----------------------------------------------------------------------
        public override void DoLayout(Rectangle rectangle)
        {
            base.DoLayout(rectangle);

            if (EnableScrolling)
            {
                Scrollbar.DoLayout(LayoutRect, ContentHeight);
            }

            HitBox = LayoutRect;
        }

        protected override void LayoutChildren()
        {
            Rectangle childRectangle;

            if (EnableScrolling)
            {
                childRectangle = new Rectangle(LayoutRect.X + Padding.Left + Margin.Left, LayoutRect.Y + Padding.Top + Margin.Top - (int)Scrollbar.LerpOffset, LayoutRect.Width - Padding.Horizontal - Margin.Horizontal, LayoutRect.Height - Padding.Vertical - Margin.Vertical + Scrollbar.Max);
            }
            else
            {
                childRectangle = new Rectangle(LayoutRect.X + Padding.Left + Margin.Left, LayoutRect.Y + Padding.Top + Margin.Top, LayoutRect.Width - Padding.Horizontal - Margin.Horizontal, LayoutRect.Height - Padding.Vertical - Margin.Vertical);
            }

            foreach (Widget widget in mlChildren)
            {
                widget.DoLayout(childRectangle);
            }
        }

        public override Widget HitTest(Point point)
        {
            return Scrollbar.HitTest(point) ?? base.HitTest(point) ?? (HitBox.Contains(point) ? this : null);
        }

        //----------------------------------------------------------------------
        protected internal override void OnMouseWheel(Point hitPoint, int delta)
        {
            if ((delta < 0 && Scrollbar.Offset >= Scrollbar.Max)
             || (delta > 0 && Scrollbar.Offset <= 0))
            {
                base.OnMouseWheel(hitPoint, delta);
                return;
            }

            DoScroll(-delta * 50 / 120);
        }

        void DoScroll(int delta)
        {
            int iScrollChange = (int)MathHelper.Clamp(delta, -Scrollbar.Offset, Math.Max(0, Scrollbar.Max - Scrollbar.Offset));
            Scrollbar.Offset += iScrollChange;
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            if (Texture != null)
            {
                Screen.DrawBox(Texture, new Rectangle(LayoutRect.X + Margin.Left, LayoutRect.Y + Margin.Top, LayoutRect.Width - Margin.Horizontal, LayoutRect.Height - Margin.Vertical), CornerSize, Color.White);
            }

            if (DoClipping)
            {
                Screen.PushScissorRectangle(new Rectangle(LayoutRect.X + Padding.Left + Margin.Left, LayoutRect.Y + Padding.Top + Margin.Top, LayoutRect.Width - Padding.Horizontal - Margin.Horizontal, LayoutRect.Height - Padding.Vertical - Margin.Vertical));
            }

            base.Draw();

            if (DoClipping)
            {
                Screen.PopScissorRectangle();
            }

            if (EnableScrolling)
            {
                Scrollbar.Draw();
            }
        }
    }
}
