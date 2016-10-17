using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;

#if !FNA
using OSKey = System.Windows.Forms.Keys;
#endif

namespace NuclearWinter.UI
{
    public struct Box
    {
        //----------------------------------------------------------------------
        public int Top;
        public int Right;
        public int Bottom;
        public int Left;

        public int Horizontal { get { return Left + Right; } }
        public int Vertical { get { return Top + Bottom; } }

        public Box(int top, int right, int bottom, int left)
        {
            Top = top;
            Right = right;
            Bottom = bottom;
            Left = left;
        }

        //----------------------------------------------------------------------
        public Box(int value)
        {
            Left = Right = Top = Bottom = value;
        }

        //----------------------------------------------------------------------
        public Box(int vertical, int horizontal)
        {
            Top = Bottom = vertical;
            Left = Right = horizontal;
        }
    }

    public abstract class Widget
    {
        //----------------------------------------------------------------------
        public Screen Screen { get; private set; }
        public Widget Parent;

        /// <summary>
        /// Returns whether the widget is connected to its Screen's root
        /// </summary>
        public bool IsOrphan
        {
            get
            {
                Widget widget = this;
                while (widget.Parent != null && widget.Parent != Screen.Root) widget = widget.Parent;
                return widget.Parent == null;
            }
        }

        public int ContentWidth { get; protected set; }
        public int ContentHeight { get; protected set; }

        public AnchoredRect AnchoredRect;
        public Rectangle LayoutRect { get; protected set; }

        protected Box mPadding;
        public Box Padding
        {
            get { return mPadding; }
            set
            {
                mPadding = value;
                UpdateContentSize();
            }
        }

        public virtual Widget GetSibling(UI.Direction direction, Widget child)
        {
            if (Parent != null)
            {
                return Parent.GetSibling(direction, this);
            }

            return null;
        }

        public virtual Widget GetFirstFocusableDescendant(UI.Direction direction)
        {
            return this;
        }

        public bool HasFocus
        {
            get
            {
                return Screen.FocusedWidget == this;
            }
        }

        public bool IsHovered
        {
            get
            {
                return Screen.HoveredWidget == this;
            }
        }

        public Rectangle HitBox { get; protected set; }

        //----------------------------------------------------------------------
        public Widget(Screen screen, AnchoredRect anchoredRect)
        {
            Screen = screen;
            AnchoredRect = anchoredRect;
        }

        public Widget(Screen screen)
        : this(screen, AnchoredRect.Full)
        {
        }

        //----------------------------------------------------------------------
        public virtual void DoLayout(Rectangle rectangle)
        {
            Rectangle childRectangle;

            // Horizontal
            if (AnchoredRect.Left.HasValue)
            {
                childRectangle.X = rectangle.Left + AnchoredRect.Left.Value;
                if (AnchoredRect.Right.HasValue)
                {
                    // Horizontally anchored
                    childRectangle.Width = (rectangle.Right - AnchoredRect.Right.Value) - childRectangle.X;
                }
                else
                {
                    // Left-anchored
                    childRectangle.Width = AnchoredRect.Width;
                }
            }
            else
            {
                childRectangle.Width = AnchoredRect.Width;

                if (AnchoredRect.Right.HasValue)
                {
                    // Right-anchored
                    childRectangle.X = (rectangle.Right - AnchoredRect.Right.Value) - childRectangle.Width;
                }
                else
                {
                    // Centered
                    childRectangle.X = rectangle.Center.X - childRectangle.Width / 2;
                }
            }

            // Vertical
            if (AnchoredRect.Top.HasValue)
            {
                childRectangle.Y = rectangle.Top + AnchoredRect.Top.Value;
                if (AnchoredRect.Bottom.HasValue)
                {
                    // Horizontally anchored
                    childRectangle.Height = (rectangle.Bottom - AnchoredRect.Bottom.Value) - childRectangle.Y;
                }
                else
                {
                    // Top-anchored
                    childRectangle.Height = AnchoredRect.Height;
                }
            }
            else
            {
                childRectangle.Height = AnchoredRect.Height;

                if (AnchoredRect.Bottom.HasValue)
                {
                    // Bottom-anchored
                    childRectangle.Y = (rectangle.Bottom - AnchoredRect.Bottom.Value) - childRectangle.Height;
                }
                else
                {
                    // Centered
                    childRectangle.Y = rectangle.Center.Y - childRectangle.Height / 2;
                }
            }

            LayoutRect = childRectangle;
        }

        //----------------------------------------------------------------------
        public virtual Widget HitTest(Point point)
        {
            return HitBox.Contains(point) ? this : null;
        }

        //----------------------------------------------------------------------
        protected Point TransformPointScreenToWidget(Point point)
        {
            return new Point(point.X - LayoutRect.X, point.Y - LayoutRect.Y);
        }

        //----------------------------------------------------------------------
        public virtual void Update(float elapsedTime) { }

        protected internal virtual void UpdateContentSize()
        {
            // Compute content size then call this!

            if (Parent != null)
            {
                Parent.UpdateContentSize();
            }
        }

        //----------------------------------------------------------------------
        // Events

        // Called when a mouse button was pressed. Widget should return true if event was consumed
        // Otherwise it will bubble up to its parent
        protected internal virtual bool OnMouseDown(Point hitPoint, int button)
        {
            this.MouseDown?.Invoke(this, hitPoint, button);
            return true;
        }

        protected internal virtual void OnMouseUp(Point hitPoint, int button)
        {
            this.MouseUp?.Invoke(this, hitPoint, button);
        }

        protected internal virtual bool OnMouseDoubleClick(Point hitPoint)
        {
            this.MouseDoubleClick?.Invoke(this, hitPoint);
            return false;
        }

        public virtual void OnMouseEnter(Point hitPoint)
        {
            this.MouseEnter?.Invoke(this, hitPoint);
        }

        public virtual void OnMouseOut(Point hitPoint)
        {
            this.MouseOut?.Invoke(this, hitPoint);
        }

        public virtual void OnMouseMove(Point hitPoint)
        {
            this.MouseMove?.Invoke(this, hitPoint);
        }

        protected internal virtual void OnMouseWheel(Point hitPoint, int delta) { if (Parent != null) Parent.OnMouseWheel(hitPoint, delta); }

        public const int MouseDragTriggerDistance = 10;

        protected internal virtual void OnOSKeyPress(OSKey key)
        {
            // allow user to handle keys
            // if the user set the args.Handled to true no more processing will happen
            var handlers = this.KeyPress;
            if (handlers != null)
            {
                var args = new KeyPressEventArgs(key, this.Screen.Game.InputMgr.KeyboardState);
                foreach (EventHandler<KeyPressEventArgs> handler in handlers.GetInvocationList())
                {
                    handler(this, args);
                    if(args.Handled)
                    {
                        return;
                    }
                }
            }

            bool bCtrl = Screen.Game.InputMgr.KeyboardState.IsKeyDown(Keys.LeftControl, true) || Screen.Game.InputMgr.KeyboardState.IsKeyDown(Keys.RightControl, true);

            if (!bCtrl && key == OSKey.Tab)
            {
                List<Direction> directions = new List<Direction>();

                if (Screen.Game.InputMgr.KeyboardState.Native.IsKeyDown(Keys.LeftShift) || Screen.Game.InputMgr.KeyboardState.Native.IsKeyDown(Keys.RightShift))
                {
                    directions.Add(Direction.Left);
                    directions.Add(Direction.Up);
                }
                else
                {
                    directions.Add(Direction.Right);
                    directions.Add(Direction.Down);
                }

                foreach (Direction direction in directions)
                {
                    Widget widget = GetSibling(direction, this);

                    if (widget != null)
                    {
                        Widget focusableWidget = widget.GetFirstFocusableDescendant(direction);

                        if (focusableWidget != null)
                        {
                            Screen.Focus(focusableWidget);
                            break;
                        }
                    }
                }
            }
            else
            {
                if (Parent != null) Parent.OnOSKeyPress(key);
            }
        }

        protected internal virtual void OnKeyPress(Keys key) { if (Parent != null) Parent.OnKeyPress(key); }

        protected internal virtual void OnTextEntered(char @char) { }

        protected internal virtual bool OnActivateDown() { return false; }
        protected internal virtual void OnActivateUp() { }
        protected internal virtual bool OnCancel(bool pressed) { return false; } // return true to consume the event

        protected internal virtual void OnFocus() { }
        protected internal virtual void OnBlur() { }

        protected internal virtual bool OnPadButton(Buttons button, bool isDown) { return false; }

        protected internal virtual void OnPadMove(Direction direction)
        {
            Widget widget = GetSibling(direction, this);

            if (widget != null)
            {
                Widget focusableWidget = widget.GetFirstFocusableDescendant(direction);

                if (focusableWidget != null)
                {
                    Screen.Focus(focusableWidget);
                }
            }
        }

        //----------------------------------------------------------------------
        public abstract void Draw();
        protected internal virtual void DrawFocused() { }
        protected internal virtual void DrawHovered() { }

        public EventHandler<KeyPressEventArgs> KeyPress;
        public event Action<Widget, Point, int> MouseDown;
        public event Action<Widget, Point, int> MouseUp;
        public event Action<Widget, Point> MouseEnter;
        public event Action<Widget, Point> MouseOut;
        public event Action<Widget, Point> MouseMove;
        public event Action<Widget, Point> MouseDoubleClick;
    }
}
