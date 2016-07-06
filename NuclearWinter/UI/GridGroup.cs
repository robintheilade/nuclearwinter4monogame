using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NuclearWinter.UI
{
    /*
     * GridGroup allows packing widgets in a grid
     * It takes care of positioning each child widget properly
     */
    public class GridGroup : Group
    {
        //----------------------------------------------------------------------
        bool mbExpand;
        int miSpacing; // FIXME: Not taken into account
        Widget[,] maTiles;
        Dictionary<Widget, Point> maWidgetLocations;

        //----------------------------------------------------------------------
        public GridGroup(Screen screen, int columns, int rows, bool expand, int spacing)
        : base(screen)
        {
            mbExpand = expand;
            miSpacing = spacing;

            maTiles = new Widget[columns, rows];
            maWidgetLocations = new Dictionary<Widget, Point>();
        }

        public override void AddChild(Widget widget, int index)
        {
            throw new NotSupportedException();
        }

        public void AddChildAt(Widget child, int column, int row)
        {
            Debug.Assert(!maWidgetLocations.ContainsKey(child));
            Debug.Assert(child.Parent == null);
            Debug.Assert(child.Screen == Screen);

            child.Parent = this;

            maTiles[column, row] = child;
            maWidgetLocations[child] = new Point(column, row);
            mlChildren.Add(child);
        }

        public override void RemoveChild(Widget widget)
        {
            Debug.Assert(widget.Parent == this);

            widget.Parent = null;

            Point widgetLocation = maWidgetLocations[widget];
            maWidgetLocations.Remove(widget);

            maTiles[widgetLocation.X, widgetLocation.Y] = null;
            mlChildren.Remove(widget);
        }

        public override Widget GetFirstFocusableDescendant(Direction direction)
        {
            switch (direction)
            {
                case Direction.Left:
                    for (int i = maTiles.GetLength(0) - 1; i >= 0; i--)
                    {
                        for (int j = 0; j < maTiles.GetLength(1); j++)
                        {
                            Widget widget = maTiles[i, j];
                            if (widget != null)
                            {
                                Widget focusableWidget = widget.GetFirstFocusableDescendant(direction);
                                if (focusableWidget != null)
                                {
                                    return focusableWidget;
                                }
                            }
                        }
                    }
                    break;
                case Direction.Right:
                    for (int i = 0; i < maTiles.GetLength(0); i++)
                    {
                        for (int j = 0; j < maTiles.GetLength(1); j++)
                        {
                            Widget widget = maTiles[i, j];
                            if (widget != null)
                            {
                                Widget focusableWidget = widget.GetFirstFocusableDescendant(direction);
                                if (focusableWidget != null)
                                {
                                    return focusableWidget;
                                }
                            }
                        }
                    }
                    break;
                case Direction.Up:
                    for (int j = maTiles.GetLength(1) - 1; j >= 0; j--)
                    {
                        for (int i = 0; i < maTiles.GetLength(0); i++)
                        {
                            Widget widget = maTiles[i, j];
                            if (widget != null)
                            {
                                Widget focusableWidget = widget.GetFirstFocusableDescendant(direction);
                                if (focusableWidget != null)
                                {
                                    return focusableWidget;
                                }
                            }
                        }
                    }
                    break;
                case Direction.Down:
                    for (int j = 0; j < maTiles.GetLength(1); j++)
                    {
                        for (int i = 0; i < maTiles.GetLength(0); i++)
                        {
                            Widget widget = maTiles[i, j];
                            if (widget != null)
                            {
                                Widget focusableWidget = widget.GetFirstFocusableDescendant(direction);
                                if (focusableWidget != null)
                                {
                                    return focusableWidget;
                                }
                            }
                        }
                    }
                    break;
            }

            return null;
        }

        public override Widget GetSibling(Direction direction, Widget child)
        {
            Widget tileChild = null;
            Point childLocation = maWidgetLocations[child];
            int iOffset = 0;

            do
            {
                iOffset++;

                switch (direction)
                {
                    case Direction.Left:
                        if (childLocation.X - iOffset < 0) return base.GetSibling(direction, child);
                        tileChild = maTiles[childLocation.X - iOffset, childLocation.Y];
                        break;
                    case Direction.Right:
                        if (childLocation.X + iOffset >= maTiles.GetLength(0)) return base.GetSibling(direction, child);
                        tileChild = maTiles[childLocation.X + iOffset, childLocation.Y];
                        break;
                    case Direction.Up:
                        if (childLocation.Y - iOffset < 0) return base.GetSibling(direction, child);
                        tileChild = maTiles[childLocation.X, childLocation.Y - iOffset];
                        break;
                    case Direction.Down:
                        if (childLocation.Y + iOffset >= maTiles.GetLength(1)) return base.GetSibling(direction, child);
                        tileChild = maTiles[childLocation.X, childLocation.Y + iOffset];
                        break;
                }
            }
            while (tileChild == null);

            if (tileChild != null)
            {
                return tileChild;
            }
            else
            {
                return base.GetSibling(direction, this);
            }
        }

        //----------------------------------------------------------------------
        public override void DoLayout(Rectangle rectangle)
        {
            base.DoLayout(rectangle);
        }

        protected override void LayoutChildren()
        {
            if (!mbExpand)
            {
                int iColumnCount = maTiles.GetLength(0);
                int iRowCount = maTiles.GetLength(1);

                Point widgetSize = new Point(
                    (LayoutRect.Width - Padding.Horizontal) / iColumnCount,
                    (LayoutRect.Height - Padding.Vertical) / iRowCount);

                foreach (KeyValuePair<Widget, Point> kvpChild in maWidgetLocations)
                {
                    Point widgetPosition = new Point(
                        LayoutRect.X + Padding.Left + widgetSize.X * kvpChild.Value.X,
                        LayoutRect.Y + Padding.Top + widgetSize.Y * kvpChild.Value.Y);

                    kvpChild.Key.DoLayout(new Rectangle(widgetPosition.X, widgetPosition.Y, widgetSize.X, widgetSize.Y));
                }
            }
        }
    }
}
