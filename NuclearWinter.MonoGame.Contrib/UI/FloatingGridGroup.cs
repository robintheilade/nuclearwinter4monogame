using Microsoft.Xna.Framework;
using NuclearWinter.UI;
using System;
using System.Diagnostics;
using System.Linq;

namespace NuclearWinter.Contrib.UI
{
    public class FloatingGridGroup : Group
    {
        private bool isScrollbarEnabled;

        public bool IsScrollbarEnabled
        {
            get
            {
                return this.isScrollbarEnabled;
            }
            set
            {
                this.isScrollbarEnabled
                    = this.HasDynamicHeight
                    = value
                    ;
            }
        }

        public Scrollbar Scrollbar
        {
            [DebuggerStepThrough]
            get;
            [DebuggerStepThrough]
            private set;
        }

        public FloatingGridGroup(Screen screen) : base(screen)
        {
            this.Scrollbar = new Scrollbar(screen)
            {
                ID = "FloatingGridGroup.Scrollbar",
                Parent = this,
            };
        }

        public override void DoLayout(Rectangle rectangle)
        {
            base.DoLayout(rectangle);

            if (this.IsScrollbarEnabled)
            {
                this.Scrollbar.DoLayout(this.LayoutRect, this.ContentHeight);
            }

            HitBox = LayoutRect;
        }

        protected override void LayoutChildren()
        {
            var max = 0;
            var imageWidth = 32;

            for (var childIndex = 0; childIndex < this.mlChildren.Count; childIndex++)
            {
                var child = this.mlChildren[childIndex];
                var childWidth = imageWidth + child.Padding.Horizontal;
                var columns = this.LayoutRect.Width / childWidth;
                var childRect = child.LayoutRect;
                childRect.Width = childRect.Height = childWidth;
                childRect.X = ((childIndex % columns) * childRect.Width) + this.LayoutRect.X;
                childRect.Y = ((childIndex / columns) * childRect.Width) - ((int)this.Scrollbar.LerpOffset);
                //childRect.Y = ((childIndex / columns) * childRect.Width) + this.LayoutRect.Y - ((int)this.Scrollbar.LerpOffset);
                child.AnchoredRect = AnchoredRect.CreateFixed(childRect.X, childRect.Y, childRect.Width, childRect.Height);
                child.DoLayout(new Rectangle(0, this.LayoutRect.Y, imageWidth, imageWidth));
                max = Math.Max(max, childRect.Bottom);
            }

            // max is the max scrollable height available not just visible
            //this.Scrollbar.Max = max - this.LayoutRect.Y; // WHY IS THIS IMPORTANT???? this is necessary when contentheight is too high
        }

        protected override void UpdateContentSize()
        {
            var min = this.mlChildren.Min(c => c.AnchoredRect.Top.Value);
            var max = this.mlChildren.Max(c => c.AnchoredRect.Top.Value + c.AnchoredRect.Height);
            this.ContentHeight = max - min;
        }

        public override void Update(float elapsedTime)
        {
            if (this.IsScrollbarEnabled)
            {
                this.Scrollbar.Update(elapsedTime);
            }

            base.Update(elapsedTime);
        }

        public override void Draw()
        {
            //this.Screen.DoScissorRectangle(this.LayoutRect, base.Draw);
            base.Draw();

            if (this.IsScrollbarEnabled)
            {
                this.Scrollbar.Draw();
            }
        }

        protected override void OnMouseWheel(Point hitPoint, int delta)
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

        public override Widget HitTest(Point point)
        {
            return Scrollbar.HitTest(point) ?? base.HitTest(point) ?? (HitBox.Contains(point) ? this : null);
        }
    }
}
