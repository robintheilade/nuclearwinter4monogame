using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using NuclearWinter.Xna;

#if !FNA
using OSKey = System.Windows.Forms.Keys;
#endif

namespace NuclearWinter.UI
{
    /*
     * A Screen handles passing on user input, updating and drawing
     * a bunch of widgets
     */
    public class Screen
    {
        //----------------------------------------------------------------------
        public NuclearGame Game { get; private set; }
        public bool IsActive;

        public Style Style { get; private set; }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public Rectangle Bounds { get { return new Rectangle(0, 0, Width, Height); } }

        public Group Root { get; private set; }

        public Widget FocusedWidget { get; private set; }
        bool mbHasActivatedFocusedWidget;

        Widget mClickedWidget;
        int miClickedWidgetMouseButton;

        public Widget HoveredWidget { get; private set; }
        Point mPreviousMouseHitPoint;

        int miIgnoreClickFrames;

        //----------------------------------------------------------------------
        public Screen(NuclearWinter.NuclearGame game, Style style, int width, int height)
        {
            Game = game;
            Style = style;
            Width = width;
            Height = height;

            Root = new Group(this);
        }

        //----------------------------------------------------------------------
        public void Resize(int width, int height)
        {
            Width = width;
            Height = height;
            Root.DoLayout(Bounds);

            // This will prevent accidental clicks when maximizing the window
            miIgnoreClickFrames = 3;
        }

        //----------------------------------------------------------------------
        public void HandleInput()
        {
            //------------------------------------------------------------------
            // Make sure we don't hold references to orphaned widgets
            if (FocusedWidget != null && FocusedWidget.IsOrphan)
            {
                FocusedWidget = null;
                mbHasActivatedFocusedWidget = false;
            }

            if (HoveredWidget != null && HoveredWidget.IsOrphan)
            {
                Game.SetCursor(MouseCursor.Default);
                HoveredWidget = null;
            }

            if (mClickedWidget != null && mClickedWidget.IsOrphan) mClickedWidget = null;

            //------------------------------------------------------------------
            Point mouseHitPoint = new Point(
                (int)(Game.InputMgr.MouseState.X / Resolution.ScaleFactor),
                (int)((Game.InputMgr.MouseState.Y - Game.GraphicsDevice.Viewport.Y) / Resolution.ScaleFactor)
            );

            if (!IsActive)
            {
                if (HoveredWidget != null)
                {
                    HoveredWidget.OnMouseOut(mouseHitPoint);
                    HoveredWidget = null;
                }

                if (mClickedWidget != null)
                {
                    mClickedWidget.OnMouseUp(mouseHitPoint, miClickedWidgetMouseButton);
                    mClickedWidget = null;
                }

                return;
            }

            //------------------------------------------------------------------
            // Mouse buttons
            bool bHasMouseEvent = false;

            if (miIgnoreClickFrames == 0)
            {
                for (int iButton = 0; iButton < 3; iButton++)
                {
                    if (Game.InputMgr.WasMouseButtonJustPressed(iButton))
                    {
                        bHasMouseEvent = true;

                        if (mClickedWidget == null)
                        {
                            miClickedWidgetMouseButton = iButton;

                            Widget hitWidget = null;

                            if (FocusedWidget != null)
                            {
                                hitWidget = FocusedWidget.HitTest(mouseHitPoint);
                            }

                            if (hitWidget == null)
                            {
                                hitWidget = Root.HitTest(mouseHitPoint);
                            }

                            while (hitWidget != null && !hitWidget.OnMouseDown(mouseHitPoint, iButton))
                            {
                                hitWidget = hitWidget.Parent;
                            }

                            mClickedWidget = hitWidget;
                        }
                    }
                }

                if (Game.InputMgr.WasMouseJustDoubleClicked())
                {
                    bHasMouseEvent = true;

                    Widget widget = FocusedWidget == null ? null : FocusedWidget.HitTest(mouseHitPoint);
                    if (widget != null)
                    {
                        bool bPressed;

                        switch (Game.InputMgr.PrimaryMouseButton)
                        {
                            case 0:
                                bPressed = Game.InputMgr.MouseState.LeftButton == ButtonState.Pressed;
                                break;
                            case 2:
                                bPressed = Game.InputMgr.MouseState.RightButton == ButtonState.Pressed;
                                break;
                            default:
                                throw new NotSupportedException();
                        }

                        if (bPressed)
                        {
                            mClickedWidget = widget;
                            miClickedWidgetMouseButton = Game.InputMgr.PrimaryMouseButton;
                        }

                        if (widget.OnMouseDoubleClick(mouseHitPoint))
                        {
                            miIgnoreClickFrames = 3;
                        }
                    }
                }
            }
            else
            {
                miIgnoreClickFrames--;
            }

            for (int iButton = 0; iButton < 3; iButton++)
            {
                if (Game.InputMgr.WasMouseButtonJustReleased(iButton))
                {
                    bHasMouseEvent = true;

                    if (mClickedWidget != null && iButton == miClickedWidgetMouseButton)
                    {
                        mClickedWidget.OnMouseUp(mouseHitPoint, iButton);
                        mClickedWidget = null;
                    }
                }
            }

            if (!bHasMouseEvent)
            {
                if (mouseHitPoint != mPreviousMouseHitPoint)
                {
                    if (mClickedWidget == null)
                    {
                        // old code only checked focused widget for mouse hit which meant you would not get
                        // Widget.OnMouseEnter and Widget.OnMMouseOut events on other widgets than the focussed
                        var hoveredWidget = Root.HitTest(mouseHitPoint);
                        if (hoveredWidget != null && hoveredWidget == HoveredWidget)
                        {
                            HoveredWidget.OnMouseMove(mouseHitPoint);
                        }
                        else
                        {
                            // cache previously hoveed widget and set the new hovered widget
                            // before call OnMouseOut or else any handler that checks for
                            // Widget.IsHovered will get an old result
                            var previousHoveredWidget = this.HoveredWidget;
                            this.HoveredWidget = hoveredWidget;

                            if (previousHoveredWidget != null)
                            {
                                previousHoveredWidget.OnMouseOut(mouseHitPoint);
                            }

                            if (HoveredWidget != null)
                            {
                                HoveredWidget.OnMouseEnter(mouseHitPoint);
                            }
                        }
                    }
                    else
                    {
                        mClickedWidget.OnMouseMove(mouseHitPoint);
                    }
                }
            }

            // Mouse wheel
            int iWheelDelta = Game.InputMgr.GetMouseWheelDelta();
            if (iWheelDelta != 0)
            {
                Widget hoveredWidget = (FocusedWidget == null ? null : FocusedWidget.HitTest(mouseHitPoint)) ?? Root.HitTest(mouseHitPoint);

                if (hoveredWidget != null)
                {
                    hoveredWidget.OnMouseWheel(mouseHitPoint, iWheelDelta);
                }
            }

            mPreviousMouseHitPoint = mouseHitPoint;

            //------------------------------------------------------------------
            // Keyboard
            if (FocusedWidget != null)
            {
                foreach (char character in Game.InputMgr.EnteredText)
                {
                    FocusedWidget.OnTextEntered(character);
                }

                if (Game.InputMgr.JustPressedOSKeys.Contains(OSKey.Enter) || Game.InputMgr.JustPressedOSKeys.Contains(OSKey.Return) || Game.InputMgr.JustPressedOSKeys.Contains(OSKey.Space))
                {
                    if (FocusedWidget.OnActivateDown())
                    {
                        mbHasActivatedFocusedWidget = true;
                    }
                }
                else
                if (Game.InputMgr.JustReleasedOSKeys.Contains(OSKey.Enter) || Game.InputMgr.JustReleasedOSKeys.Contains(OSKey.Return) || Game.InputMgr.JustReleasedOSKeys.Contains(OSKey.Space))
                {
                    if (mbHasActivatedFocusedWidget)
                    {
                        FocusedWidget.OnActivateUp();
                        mbHasActivatedFocusedWidget = false;
                    }
                }

                if (Game.InputMgr.WasKeyJustPressed(Keys.Left)) FocusedWidget.OnPadMove(UI.Direction.Left);
                if (Game.InputMgr.WasKeyJustPressed(Keys.Right)) FocusedWidget.OnPadMove(UI.Direction.Right);
                if (Game.InputMgr.WasKeyJustPressed(Keys.Up)) FocusedWidget.OnPadMove(UI.Direction.Up);
                if (Game.InputMgr.WasKeyJustPressed(Keys.Down)) FocusedWidget.OnPadMove(UI.Direction.Down);

                foreach (Keys key in Game.InputMgr.JustPressedKeys)
                {
                    FocusedWidget.OnKeyPress(key);
                }

                foreach (var key in Game.InputMgr.JustPressedOSKeys)
                {
                    FocusedWidget.OnOSKeyPress(key);
                }
            }
        }

        //----------------------------------------------------------------------
        public void Update(float elapsedTime)
        {
            Root.DoLayout(new Rectangle(0, 0, Width, Height));
            Root.Update(elapsedTime);
        }

        //----------------------------------------------------------------------
        public void Focus(Widget widget)
        {
            Debug.Assert(widget.Screen == this);

            mbHasActivatedFocusedWidget = false;
            if (FocusedWidget != null && FocusedWidget != widget)
            {
                FocusedWidget.OnBlur();
            }

            if (FocusedWidget != widget)
            {
                FocusedWidget = widget;
                FocusedWidget.OnFocus();
            }
        }

        //----------------------------------------------------------------------
        public void DrawBox(Texture2D texture, Rectangle extents, int cornerSize, Color color)
        {
            // Corners
            Game.SpriteBatch.Draw(texture, new Rectangle(extents.Left, extents.Top, cornerSize, cornerSize), new Rectangle(0, 0, cornerSize, cornerSize), color);
            Game.SpriteBatch.Draw(texture, new Rectangle(extents.Right - cornerSize, extents.Top, cornerSize, cornerSize), new Rectangle(texture.Width - cornerSize, 0, cornerSize, cornerSize), color);
            Game.SpriteBatch.Draw(texture, new Rectangle(extents.Left, extents.Bottom - cornerSize, cornerSize, cornerSize), new Rectangle(0, texture.Height - cornerSize, cornerSize, cornerSize), color);
            Game.SpriteBatch.Draw(texture, new Rectangle(extents.Right - cornerSize, extents.Bottom - cornerSize, cornerSize, cornerSize), new Rectangle(texture.Width - cornerSize, texture.Height - cornerSize, cornerSize, cornerSize), color);

            // Content
            Game.SpriteBatch.Draw(texture, new Rectangle(extents.Left + cornerSize, extents.Top + cornerSize, extents.Width - cornerSize * 2, extents.Height - cornerSize * 2), new Rectangle(cornerSize, cornerSize, texture.Width - cornerSize * 2, texture.Height - cornerSize * 2), color);

            // Border top / bottom
            Game.SpriteBatch.Draw(texture, new Rectangle(extents.Left + cornerSize, extents.Top, extents.Width - cornerSize * 2, cornerSize), new Rectangle(cornerSize, 0, texture.Width - cornerSize * 2, cornerSize), color);
            Game.SpriteBatch.Draw(texture, new Rectangle(extents.Left + cornerSize, extents.Bottom - cornerSize, extents.Width - cornerSize * 2, cornerSize), new Rectangle(cornerSize, texture.Height - cornerSize, texture.Width - cornerSize * 2, cornerSize), color);

            // Border left / right
            Game.SpriteBatch.Draw(texture, new Rectangle(extents.Left, extents.Top + cornerSize, cornerSize, extents.Height - cornerSize * 2), new Rectangle(0, cornerSize, cornerSize, texture.Height - cornerSize * 2), color);
            Game.SpriteBatch.Draw(texture, new Rectangle(extents.Right - cornerSize, extents.Top + cornerSize, cornerSize, extents.Height - cornerSize * 2), new Rectangle(texture.Width - cornerSize, cornerSize, cornerSize, texture.Height - cornerSize * 2), color);
        }

        //----------------------------------------------------------------------
        Stack<Rectangle> mlScissorRects = new Stack<Rectangle>();

        Rectangle TransformRect(Rectangle rectangle, Matrix matrix)
        {
            Vector2 vMin = new Vector2(rectangle.X, rectangle.Y);
            Vector2 vMax = new Vector2(rectangle.Right, rectangle.Bottom);
            vMin = Vector2.Transform(vMin, matrix);
            vMax = Vector2.Transform(vMax, matrix);
            Rectangle bounds = new Rectangle(
                (int)vMin.X,
                (int)vMin.Y,
                (int)(vMax.X - vMin.X),
                (int)(vMax.Y - vMin.Y)
                );

            return bounds;
        }

        public void PushScissorRectangle(Rectangle scissorRectangle)
        {
            Rectangle rect = TransformRect(scissorRectangle, Game.SpriteMatrix);
            rect.Offset(0, NuclearWinter.Resolution.Viewport.Y);
            Rectangle parentRect = ScissorRectangle;

            Rectangle newRect = parentRect.Clip(rect).Clip(Game.GraphicsDevice.Viewport.Bounds);

            SuspendBatch();
            mlScissorRects.Push(newRect);
            ResumeBatch();
        }

        public void PopScissorRectangle()
        {
            SuspendBatch();
            mlScissorRects.Pop();
            ResumeBatch();
        }

        public Rectangle ScissorRectangle
        {
            get { return mlScissorRects.Count > 0 ? mlScissorRects.Peek() : Game.GraphicsDevice.Viewport.Bounds; }
        }

        //----------------------------------------------------------------------
        public void Draw()
        {
            Game.SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Game.SpriteMatrix);

            Root.Draw();

            if (FocusedWidget != null && IsActive)
            {
                FocusedWidget.DrawFocused();
            }

            if (HoveredWidget != null && IsActive)
            {
                HoveredWidget.DrawHovered();
            }

            Game.SpriteBatch.End();

            Debug.Assert(mlScissorRects.Count == 0, "Unbalanced calls to PushScissorRectangles");
        }

        //----------------------------------------------------------------------
        public void SuspendBatch()
        {
            Game.SpriteBatch.End();
            Game.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
        }

        //----------------------------------------------------------------------
        public void ResumeBatch()
        {
            Game.GraphicsDevice.RasterizerState = Game.ScissorRasterizerState;

            if (mlScissorRects.Count > 0)
            {
                var rect = mlScissorRects.Peek();
                if (rect.Width > 0 && rect.Height > 0)
                {
                    Game.GraphicsDevice.ScissorRectangle = rect;
                }

                Game.SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, Game.ScissorRasterizerState, null, Game.SpriteMatrix);
            }
            else
            {
                Game.SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Game.SpriteMatrix);
            }
        }
    }
}
