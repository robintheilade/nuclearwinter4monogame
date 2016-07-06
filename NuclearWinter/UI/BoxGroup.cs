using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NuclearWinter.UI
{
    /*
     * BoxGroup allows packing widgets in one direction
     * It takes care of positioning each child widget properly
     */
    public class BoxGroup : Group
    {
        //----------------------------------------------------------------------
        Orientation mOrientation;
        int miSpacing;

        List<bool> mlExpandedChildren;

        Anchor mContentAnchor;

        //----------------------------------------------------------------------
        public BoxGroup(Screen screen, Orientation orientation, int spacing, Anchor contentAnchor = Anchor.Center)
        : base(screen)
        {
            mlExpandedChildren = new List<bool>();

            mOrientation = orientation;
            miSpacing = spacing;
            mContentAnchor = contentAnchor;
        }

        //----------------------------------------------------------------------
        public void AddChild(Widget widget, int index, bool expand)
        {
            Debug.Assert(widget.Parent == null);
            Debug.Assert(widget.Screen == Screen);

            widget.Parent = this;
            mlExpandedChildren.Insert(index, expand);
            mlChildren.Insert(index, widget);
            UpdateContentSize();
        }

        public override void AddChild(Widget widget, int index)
        {
            AddChild(widget, index, false);
        }

        public void AddChild(Widget widget, bool expand)
        {
            AddChild(widget, mlChildren.Count, expand);
        }

        public override void RemoveChild(Widget widget)
        {
            Debug.Assert(widget.Parent == this);

            widget.Parent = null;

            mlExpandedChildren.RemoveAt(mlChildren.IndexOf(widget));

            mlChildren.Remove(widget);
            UpdateContentSize();
        }

        public override void Clear()
        {
            base.Clear();
            mlExpandedChildren.Clear();
        }

        //----------------------------------------------------------------------
        public override Widget GetFirstFocusableDescendant(Direction direction)
        {
            if (mlChildren.Count == 0) return null;

            if (mOrientation == Orientation.Vertical)
            {
                return mlChildren[0].GetFirstFocusableDescendant(direction);
            }
            else
            {
                return mlChildren[0].GetFirstFocusableDescendant(direction);
            }
        }

        //----------------------------------------------------------------------
        public override Widget GetSibling(Direction direction, Widget child)
        {
            int iIndex = mlChildren.IndexOf(child);

            if (mOrientation == Orientation.Horizontal)
            {
                if (direction == Direction.Right)
                {
                    if (iIndex < mlChildren.Count - 1)
                    {
                        return mlChildren[iIndex + 1];
                    }
                }
                else
                if (direction == Direction.Left)
                {
                    if (iIndex > 0)
                    {
                        return mlChildren[iIndex - 1];
                    }
                }
            }
            else
            {
                if (direction == Direction.Down)
                {
                    if (iIndex < mlChildren.Count - 1)
                    {
                        return mlChildren[iIndex + 1];
                    }
                }
                else
                if (direction == Direction.Up)
                {
                    if (iIndex > 0)
                    {
                        return mlChildren[iIndex - 1];
                    }
                }
            }

            return base.GetSibling(direction, this);
        }

        //----------------------------------------------------------------------
        protected internal override void UpdateContentSize()
        {
            if (mOrientation == Orientation.Horizontal)
            {
                ContentWidth = Padding.Horizontal;
                ContentHeight = 0;
                foreach (Widget child in mlChildren)
                {
                    ContentWidth += child.AnchoredRect.HasWidth ? child.AnchoredRect.Width : child.ContentWidth;
                    ContentHeight = Math.Max(ContentHeight, child.ContentHeight);
                }

                ContentHeight += Padding.Vertical;

                if (mlChildren.Count > 1)
                {
                    ContentWidth += miSpacing * (mlChildren.Count - 1);
                }
            }
            else
            {
                ContentHeight = Padding.Vertical;
                ContentWidth = 0;
                foreach (Widget child in mlChildren)
                {
                    ContentHeight += child.AnchoredRect.HasHeight ? child.AnchoredRect.Height : child.ContentHeight;
                    ContentWidth = Math.Max(ContentWidth, child.ContentWidth);
                }

                ContentWidth += Padding.Horizontal;
                if (mlChildren.Count > 1)
                {
                    ContentHeight += miSpacing * (mlChildren.Count - 1);
                }
            }

            base.UpdateContentSize();
        }

        //----------------------------------------------------------------------
        public override void DoLayout(Rectangle rectangle)
        {
            base.DoLayout(rectangle);
        }

        protected override void LayoutChildren()
        {
            int iWidth = LayoutRect.Width - Padding.Horizontal;
            int iHeight = LayoutRect.Height - Padding.Vertical;

            int iUnexpandedSize = 0;
            int iExpandedChildrenCount = 0;
            for (int iIndex = 0; iIndex < mlChildren.Count; iIndex++)
            {
                if (!mlExpandedChildren[iIndex])
                {
                    iUnexpandedSize += (mOrientation == Orientation.Horizontal) ?
                        (mlChildren[iIndex].AnchoredRect.HasWidth ? mlChildren[iIndex].AnchoredRect.Width : mlChildren[iIndex].ContentWidth) :
                        (mlChildren[iIndex].AnchoredRect.HasHeight ? mlChildren[iIndex].AnchoredRect.Height : mlChildren[iIndex].ContentHeight);
                }
                else
                {
                    iExpandedChildrenCount++;
                }
            }

            iUnexpandedSize += (mlChildren.Count - 1) * miSpacing;

            float fExpandedWidgetSize = 0f;
            if (iExpandedChildrenCount > 0)
            {
                fExpandedWidgetSize = (((mOrientation == Orientation.Horizontal) ? iWidth : iHeight) - iUnexpandedSize) / (float)iExpandedChildrenCount;
            }

            int iActualSize = iExpandedChildrenCount > 0 ? ((mOrientation == Orientation.Horizontal) ? iWidth : iHeight) : iUnexpandedSize;

            Point pWidgetPosition;

            switch (mOrientation)
            {
                case Orientation.Horizontal:
                    switch (mContentAnchor)
                    {
                        case Anchor.Start:
                            pWidgetPosition = new Point(LayoutRect.Left + Padding.Left, LayoutRect.Top + Padding.Top);
                            break;
                        case Anchor.Center:
                            pWidgetPosition = new Point(LayoutRect.Center.X - iActualSize / 2, LayoutRect.Top + Padding.Top);
                            break;
                        case Anchor.End:
                            pWidgetPosition = new Point(LayoutRect.Right - Padding.Right - iActualSize, LayoutRect.Top + Padding.Top);
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                    break;
                case Orientation.Vertical:
                    switch (mContentAnchor)
                    {
                        case Anchor.Start:
                            pWidgetPosition = new Point(LayoutRect.Left + Padding.Left, LayoutRect.Top + Padding.Top);
                            break;
                        case Anchor.Center:
                            pWidgetPosition = new Point(LayoutRect.X + Padding.Left, LayoutRect.Center.Y - iActualSize / 2);
                            break;
                        case Anchor.End:
                            pWidgetPosition = new Point(LayoutRect.X + Padding.Left, LayoutRect.Bottom - Padding.Bottom - iActualSize);
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }

            float fOffset = 0;
            for (int iIndex = 0; iIndex < mlChildren.Count; iIndex++)
            {
                Widget widget = mlChildren[iIndex];

                int iWidgetSize = (mOrientation == Orientation.Horizontal) ?
                    (mlChildren[iIndex].AnchoredRect.HasWidth ? mlChildren[iIndex].AnchoredRect.Width : mlChildren[iIndex].ContentWidth) :
                    (mlChildren[iIndex].AnchoredRect.HasHeight ? mlChildren[iIndex].AnchoredRect.Height : mlChildren[iIndex].ContentHeight);

                if (mlExpandedChildren[iIndex])
                {
                    if (iIndex < mlChildren.Count - 1)
                    {
                        iWidgetSize = (int)Math.Floor(fExpandedWidgetSize + fOffset - Math.Floor(fOffset));
                    }
                    else
                    {
                        iWidgetSize = (int)(((mOrientation == Orientation.Horizontal) ? iWidth : iHeight) - Math.Floor(fOffset));
                    }
                    fOffset += fExpandedWidgetSize + miSpacing;
                }
                else
                {
                    fOffset += iWidgetSize + miSpacing;
                }

                widget.DoLayout(new Rectangle(pWidgetPosition.X, pWidgetPosition.Y, mOrientation == Orientation.Horizontal ? iWidgetSize : iWidth, mOrientation == Orientation.Horizontal ? iHeight : iWidgetSize));

                switch (mOrientation)
                {
                    case Orientation.Horizontal:
                        pWidgetPosition.X += iWidgetSize + miSpacing;
                        break;
                    case Orientation.Vertical:
                        pWidgetPosition.Y += iWidgetSize + miSpacing;
                        break;
                }
            }
        }
    }
}
