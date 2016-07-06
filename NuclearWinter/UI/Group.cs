using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NuclearWinter.UI
{
    /*
     * A widget to lay out a bunch of child widgets
     */
    public class Group : Widget
    {
        protected List<Widget> mlChildren;

        public virtual void Clear()
        {
            mlChildren.RemoveAll(delegate (Widget _widget) { _widget.Parent = null; return true; });
            UpdateContentSize();
        }

        public void AddChild(Widget widget)
        {
            AddChild(widget, mlChildren.Count);
        }

        public virtual void AddChild(Widget widget, int index)
        {
            Debug.Assert(widget.Parent == null);
            Debug.Assert(widget.Screen == Screen);

            widget.Parent = this;
            mlChildren.Insert(index, widget);
            UpdateContentSize();
        }

        public Widget GetChild(int index)
        {
            return mlChildren[index];
        }

        public virtual void RemoveChild(Widget widget)
        {
            Debug.Assert(widget.Parent == this);

            widget.Parent = null;
            mlChildren.Remove(widget);
            UpdateContentSize();
        }

        //----------------------------------------------------------------------
        public bool HasDynamicHeight = false;

        public int Width
        {
            get { return ContentWidth; }
            set { ContentWidth = value; }
        }

        public int Height
        {
            get { return ContentHeight; }
            set { ContentHeight = value; }
        }

        //----------------------------------------------------------------------
        public Group(Screen screen)
        : base(screen)
        {
            mlChildren = new List<Widget>();
        }

        //----------------------------------------------------------------------
        protected internal override void UpdateContentSize()
        {
            if (HasDynamicHeight)
            {
                //ContentWidth = 0;
                ContentHeight = 0;

                foreach (Widget widget in mlChildren)
                {
                    //ContentWidth    = Math.Max( ContentWidth, fixedWidget.LayoutRect.Right );
                    int iHeight = 0;
                    if (widget.AnchoredRect.Top.HasValue)
                    {
                        if (widget.AnchoredRect.Bottom.HasValue)
                        {
                            iHeight = widget.AnchoredRect.Top.Value + widget.ContentHeight + widget.AnchoredRect.Bottom.Value;
                        }
                        else
                        {
                            iHeight = widget.AnchoredRect.Top.Value + widget.AnchoredRect.Height;
                        }
                    }

                    ContentHeight = Math.Max(ContentHeight, iHeight + Padding.Vertical);
                }
            }

            base.UpdateContentSize();
        }

        //----------------------------------------------------------------------
        public override void Update(float elapsedTime)
        {
            foreach (Widget widget in mlChildren)
            {
                widget.Update(elapsedTime);
            }

            base.Update(elapsedTime);
        }

        //----------------------------------------------------------------------
        public override void DoLayout(Rectangle rectangle)
        {
            base.DoLayout(rectangle);

            LayoutChildren();
            UpdateContentSize();

            HitBox = LayoutRect;
        }

        //----------------------------------------------------------------------
        protected virtual void LayoutChildren()
        {
            foreach (Widget widget in mlChildren)
            {
                widget.DoLayout(LayoutRect);
            }
        }

        //----------------------------------------------------------------------
        public override Widget GetFirstFocusableDescendant(Direction direction)
        {
            Widget firstChild = null;
            Widget focusableDescendant = null;

            foreach (Widget child in mlChildren)
            {
                switch (direction)
                {
                    case Direction.Up:
                        if ((firstChild == null || child.LayoutRect.Bottom > firstChild.LayoutRect.Bottom))
                        {
                            Widget childFocusableWidget = child.GetFirstFocusableDescendant(direction);
                            if (childFocusableWidget != null)
                            {
                                firstChild = child;
                                focusableDescendant = childFocusableWidget;
                            }
                        }
                        break;
                    case Direction.Down:
                        if (firstChild == null || child.LayoutRect.Top < firstChild.LayoutRect.Top)
                        {
                            Widget childFocusableWidget = child.GetFirstFocusableDescendant(direction);
                            if (childFocusableWidget != null)
                            {
                                firstChild = child;
                                focusableDescendant = childFocusableWidget;
                            }
                        }
                        break;
                    case Direction.Left:
                        if (firstChild == null || child.LayoutRect.Right > firstChild.LayoutRect.Right)
                        {
                            Widget childFocusableWidget = child.GetFirstFocusableDescendant(direction);
                            if (childFocusableWidget != null)
                            {
                                firstChild = child;
                                focusableDescendant = childFocusableWidget;
                            }
                        }
                        break;
                    case Direction.Right:
                        if (firstChild == null || child.LayoutRect.Left < firstChild.LayoutRect.Left)
                        {
                            Widget childFocusableWidget = child.GetFirstFocusableDescendant(direction);
                            if (childFocusableWidget != null)
                            {
                                firstChild = child;
                                focusableDescendant = childFocusableWidget;
                            }
                        }
                        break;
                }
            }

            return focusableDescendant;
        }

        //----------------------------------------------------------------------
        public override Widget GetSibling(Direction direction, Widget fixedChild)
        {
            Widget nearestSibling = null;
            Widget focusableSibling = null;

            foreach (Widget child in mlChildren)
            {
                if (child == fixedChild) continue;

                switch (direction)
                {
                    case Direction.Up:
                        if (child.LayoutRect.Bottom <= fixedChild.LayoutRect.Center.Y && (nearestSibling == null || child.LayoutRect.Bottom > nearestSibling.LayoutRect.Bottom))
                        {
                            Widget childFocusableWidget = child.GetFirstFocusableDescendant(direction);
                            if (childFocusableWidget != null)
                            {
                                nearestSibling = child;
                                focusableSibling = childFocusableWidget;
                            }
                        }
                        break;
                    case Direction.Down:
                        if (child.LayoutRect.Top >= fixedChild.LayoutRect.Center.Y && (nearestSibling == null || child.LayoutRect.Top < nearestSibling.LayoutRect.Top))
                        {
                            Widget childFocusableWidget = child.GetFirstFocusableDescendant(direction);
                            if (childFocusableWidget != null)
                            {
                                nearestSibling = child;
                                focusableSibling = childFocusableWidget;
                            }
                        }
                        break;
                    case Direction.Left:
                        if (child.LayoutRect.Right <= fixedChild.LayoutRect.Center.X && (nearestSibling == null || child.LayoutRect.Right > nearestSibling.LayoutRect.Right))
                        {
                            Widget childFocusableWidget = child.GetFirstFocusableDescendant(direction);
                            if (childFocusableWidget != null)
                            {
                                nearestSibling = child;
                                focusableSibling = childFocusableWidget;
                            }
                        }
                        break;
                    case Direction.Right:
                        if (child.LayoutRect.Left >= fixedChild.LayoutRect.Center.X && (nearestSibling == null || child.LayoutRect.Left < nearestSibling.LayoutRect.Left))
                        {
                            Widget childFocusableWidget = child.GetFirstFocusableDescendant(direction);
                            if (childFocusableWidget != null)
                            {
                                nearestSibling = child;
                                focusableSibling = childFocusableWidget;
                            }
                        }
                        break;
                }
            }

            if (focusableSibling == null)
            {
                return base.GetSibling(direction, this);
            }

            return focusableSibling;
        }

        //----------------------------------------------------------------------
        public override Widget HitTest(Point point)
        {
            if (HitBox.Contains(point))
            {
                Widget hitWidget;

                for (int iChild = mlChildren.Count - 1; iChild >= 0; iChild--)
                {
                    Widget child = mlChildren[iChild];

                    if ((hitWidget = child.HitTest(point)) != null)
                    {
                        return hitWidget;
                    }
                }
            }

            return null;
        }

        //----------------------------------------------------------------------
        protected internal override bool OnPadButton(Buttons button, bool isDown)
        {
            foreach (Widget child in mlChildren)
            {
                if (child.OnPadButton(button, isDown))
                {
                    return true;
                }
            }

            return false;
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            foreach (Widget child in mlChildren)
            {
                child.Draw();
            }
        }
    }
}
