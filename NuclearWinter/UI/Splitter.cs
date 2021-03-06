﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;

namespace NuclearWinter.UI
{
    //---------------------------------------------------------------------------
    public class Splitter : Widget
    {
        //-----------------------------------------------------------------------
        public Widget FirstPane
        {
            get { return mFirstPane; }
            set
            {
                if (mFirstPane != null)
                {
                    Debug.Assert(mFirstPane.Parent == this);

                    mFirstPane.Parent = null;
                }

                mFirstPane = value;

                if (mFirstPane != null)
                {
                    Debug.Assert(mFirstPane.Parent == null);

                    mFirstPane.Parent = this;
                }
            }
        }

        public Widget SecondPane
        {
            get { return mSecondPane; }
            set
            {
                if (mSecondPane != null)
                {
                    Debug.Assert(mSecondPane.Parent == this);

                    mSecondPane.Parent = null;
                }

                mSecondPane = value;

                if (mSecondPane != null)
                {
                    Debug.Assert(mSecondPane.Parent == null);

                    mSecondPane.Parent = this;
                }
            }
        }

        public int FirstPaneMinSize = 100;
        public int SecondPaneMinSize = 100;

        public bool Enabled = true;
        public int SplitterOffset;
        public bool Collapsable;

        public bool InvertDrawOrder;

        public Texture2D SplitterFrame;
        public int SplitterFrameCornerSize;
        public int SplitterSize;

        public Texture2D HandleTex;

        //-----------------------------------------------------------------------
        Widget mFirstPane;
        Widget mSecondPane;

        bool mbCollapsed;
        Animation.AnimatedValue mCollapseAnim;
        bool mbDisplayFirstPane;
        bool mbDisplaySecondPane;

        // NOTE: Splitter is using a Direction instead of an Orientation so
        // it know from which side the offset is computed
        Direction mDirection;

        bool mbIsDragging;
        int miDragOffset;
        bool mbIsHovered;

        //-----------------------------------------------------------------------
        public Splitter(Screen screen, Direction direction, bool collapsable = false)
        : base(screen)
        {
            mDirection = direction;

            SplitterFrame = Screen.Style.SplitterFrame;
            SplitterFrameCornerSize = Screen.Style.SplitterFrameCornerSize;
            SplitterSize = Screen.Style.SplitterSize;

            Collapsable = collapsable;
            HandleTex = Collapsable ? Screen.Style.SplitterCollapseArrow : Screen.Style.SplitterDragHandle;
            mCollapseAnim = new Animation.SmoothValue(0f, 1f, 0.2f);
        }

        //-----------------------------------------------------------------------
        public void ToggleCollapse()
        {
            if (Collapsable) mbCollapsed = !mbCollapsed;
        }

        //-----------------------------------------------------------------------
        public override void Update(float elapsedTime)
        {
            if (mbCollapsed)
            {
                mCollapseAnim.Direction = Animation.AnimationDirection.Forward;
                mCollapseAnim.Update(elapsedTime);
            }
            else
            {
                mCollapseAnim.Direction = Animation.AnimationDirection.Backward;
                mCollapseAnim.Update(elapsedTime);
            }

            if (mFirstPane != null)
            {
                mFirstPane.Update(elapsedTime);
            }

            if (mSecondPane != null)
            {
                mSecondPane.Update(elapsedTime);
            }
        }

        //-----------------------------------------------------------------------
        public override void DoLayout(Rectangle rectangle)
        {
            base.DoLayout(rectangle);

            bool bHidePane = mbCollapsed || mCollapseAnim.CurrentValue != 0f;

            mbDisplayFirstPane = (!bHidePane || (mDirection != Direction.Left && mDirection != Direction.Up));
            mbDisplaySecondPane = (!bHidePane || (mDirection != Direction.Right && mDirection != Direction.Down));

            if (bHidePane)
            {
                int iCollapseOffset = (int)((1f - mCollapseAnim.CurrentValue) * SplitterOffset);

                switch (mDirection)
                {
                    case Direction.Left:
                        {
                            mSecondPane.DoLayout(new Rectangle(LayoutRect.Left + iCollapseOffset, LayoutRect.Top, LayoutRect.Width - iCollapseOffset, LayoutRect.Height));

                            HitBox = new Rectangle(
                                LayoutRect.Left + iCollapseOffset - SplitterSize / 2,
                                LayoutRect.Top,
                                SplitterSize,
                                LayoutRect.Height);
                            break;
                        }
                    case Direction.Up:
                        {
                            mSecondPane.DoLayout(LayoutRect);

                            HitBox = new Rectangle(
                                LayoutRect.Top,
                                LayoutRect.Top + iCollapseOffset - SplitterSize / 2,
                                LayoutRect.Width,
                                SplitterSize
                                );
                            break;
                        }
                    case Direction.Right:
                        {
                            mFirstPane.DoLayout(LayoutRect);

                            HitBox = new Rectangle(
                                LayoutRect.Right + iCollapseOffset - SplitterSize / 2,
                                LayoutRect.Top,
                                SplitterSize,
                                LayoutRect.Height);
                            break;
                        }
                    case Direction.Down:
                        {
                            mFirstPane.DoLayout(LayoutRect);

                            HitBox = new Rectangle(
                                LayoutRect.Left,
                                LayoutRect.Bottom + iCollapseOffset - SplitterSize / 2,
                                SplitterSize,
                                LayoutRect.Height);
                            break;
                        }
                }
            }
            else
            {
                switch (mDirection)
                {
                    case Direction.Left:
                        {
                            if (LayoutRect.Width >= FirstPaneMinSize + SecondPaneMinSize)
                            {
                                SplitterOffset = (int)MathHelper.Clamp(SplitterOffset, FirstPaneMinSize, LayoutRect.Width - SecondPaneMinSize);
                            }

                            HitBox = new Rectangle(
                                LayoutRect.Left + SplitterOffset - SplitterSize / 2,
                                LayoutRect.Top,
                                SplitterSize,
                                LayoutRect.Height);

                            if (mFirstPane != null)
                            {
                                mFirstPane.DoLayout(new Rectangle(LayoutRect.Left, LayoutRect.Top, SplitterOffset, LayoutRect.Height));
                            }

                            if (mSecondPane != null)
                            {
                                mSecondPane.DoLayout(new Rectangle(LayoutRect.Left + SplitterOffset, LayoutRect.Top, LayoutRect.Width - SplitterOffset, LayoutRect.Height));
                            }
                            break;
                        }
                    case Direction.Right:
                        {
                            if (LayoutRect.Width >= FirstPaneMinSize + SecondPaneMinSize)
                            {
                                SplitterOffset = (int)MathHelper.Clamp(SplitterOffset, SecondPaneMinSize, LayoutRect.Width - FirstPaneMinSize);
                            }

                            HitBox = new Rectangle(
                                LayoutRect.Right - SplitterOffset - SplitterSize / 2,
                                LayoutRect.Top,
                                SplitterSize,
                                LayoutRect.Height);

                            if (mFirstPane != null)
                            {
                                mFirstPane.DoLayout(new Rectangle(LayoutRect.Left, LayoutRect.Top, LayoutRect.Width - SplitterOffset, LayoutRect.Height));
                            }

                            if (mSecondPane != null)
                            {
                                mSecondPane.DoLayout(new Rectangle(LayoutRect.Right - SplitterOffset, LayoutRect.Top, SplitterOffset, LayoutRect.Height));
                            }
                            break;
                        }
                    case Direction.Up:
                        {
                            if (LayoutRect.Height >= FirstPaneMinSize + SecondPaneMinSize)
                            {
                                SplitterOffset = (int)MathHelper.Clamp(SplitterOffset, FirstPaneMinSize, LayoutRect.Height - SecondPaneMinSize);
                            }

                            HitBox = new Rectangle(
                                LayoutRect.Left,
                                LayoutRect.Top + SplitterOffset - SplitterSize / 2,
                                LayoutRect.Width,
                                SplitterSize);

                            if (mFirstPane != null)
                            {
                                mFirstPane.DoLayout(new Rectangle(LayoutRect.Left, LayoutRect.Top, LayoutRect.Width, SplitterOffset));
                            }

                            if (mSecondPane != null)
                            {
                                mSecondPane.DoLayout(new Rectangle(LayoutRect.Left, LayoutRect.Top + SplitterOffset, LayoutRect.Width, LayoutRect.Height - SplitterOffset));
                            }
                            break;
                        }
                    case Direction.Down:
                        {
                            if (LayoutRect.Height >= FirstPaneMinSize + SecondPaneMinSize)
                            {
                                SplitterOffset = (int)MathHelper.Clamp(SplitterOffset, SecondPaneMinSize, LayoutRect.Height - FirstPaneMinSize);
                            }

                            HitBox = new Rectangle(
                                LayoutRect.Left,
                                LayoutRect.Bottom - SplitterOffset - SplitterSize / 2,
                                LayoutRect.Width,
                                SplitterSize);

                            if (mFirstPane != null)
                            {
                                mFirstPane.DoLayout(new Rectangle(LayoutRect.Left, LayoutRect.Top, LayoutRect.Width, LayoutRect.Height - SplitterOffset));
                            }

                            if (mSecondPane != null)
                            {
                                mSecondPane.DoLayout(new Rectangle(LayoutRect.Left, LayoutRect.Bottom - SplitterOffset, LayoutRect.Width, SplitterOffset));
                            }
                            break;
                        }
                }
            }
        }

        public override Widget HitTest(Point point)
        {
            // The splitter itself
            if (HitBox.Contains(point))
            {
                return this;
            }

            // The panes
            if (mFirstPane != null && mbDisplayFirstPane)
            {
                Widget widget = mFirstPane.HitTest(point);
                if (widget != null)
                {
                    return widget;
                }
            }

            if (mSecondPane != null && mbDisplaySecondPane)
            {
                Widget widget = mSecondPane.HitTest(point);
                if (widget != null)
                {
                    return widget;
                }
            }

            return null;
        }

        public override void OnMouseEnter(Point hitPoint)
        {
            if (!Enabled) return;

            mbIsHovered = true;

            switch (mDirection)
            {
                case Direction.Left:
                case Direction.Right:
                    Screen.Game.SetCursor(Collapsable ? MouseCursor.Hand : MouseCursor.SizeWE);
                    break;
                case Direction.Up:
                case Direction.Down:
                    Screen.Game.SetCursor(Collapsable ? MouseCursor.Hand : MouseCursor.SizeNS);
                    break;
            }
        }

        public override void OnMouseOut(Point hitPoint)
        {
            mbIsHovered = false;
            Screen.Game.SetCursor(MouseCursor.Default);
        }

        public override void OnMouseMove(Point hitPoint)
        {
            if (!Enabled)
            {
                mbIsDragging = false;

                if (mbIsHovered)
                {
                    mbIsHovered = false;
                    Screen.Game.SetCursor(MouseCursor.Default);
                }
                return;
            }

            if (!Collapsable && mbIsDragging)
            {
                switch (mDirection)
                {
                    case Direction.Left:
                        SplitterOffset = miDragOffset + hitPoint.X;
                        break;
                    case Direction.Right:
                        SplitterOffset = miDragOffset - hitPoint.X;
                        break;
                    case Direction.Up:
                        SplitterOffset = miDragOffset + hitPoint.Y;
                        break;
                    case Direction.Down:
                        SplitterOffset = miDragOffset - hitPoint.Y;
                        break;
                }
            }
        }

        protected internal override bool OnMouseDown(Point hitPoint, int button)
        {
            if (button != Screen.Game.InputMgr.PrimaryMouseButton || !Enabled) return false;

            if (Collapsable)
            {
                mbIsDragging = true;
            }
            else
            {
                mbIsDragging = true;

                switch (mDirection)
                {
                    case Direction.Left:
                        miDragOffset = SplitterOffset - hitPoint.X;
                        break;
                    case Direction.Right:
                        miDragOffset = SplitterOffset + hitPoint.X;
                        break;
                    case Direction.Up:
                        miDragOffset = SplitterOffset - hitPoint.Y;
                        break;
                    case Direction.Down:
                        miDragOffset = SplitterOffset + hitPoint.Y;
                        break;
                }
            }

            Screen.Focus(this);

            return true;
        }

        protected internal override void OnMouseUp(Point hitPoint, int button)
        {
            if (Collapsable && Enabled)
            {
                mbCollapsed = !mbCollapsed;
            }

            mbIsDragging = false;
        }

        //-----------------------------------------------------------------------
        public override void Draw()
        {
            if (mbIsHovered)
            {
                Color handleColor = Color.White * (mbIsDragging ? 1f : 0.8f);

                Screen.DrawBox(SplitterFrame, HitBox, SplitterFrameCornerSize, handleColor);

                float fHandleAngle;
                switch (mDirection)
                {
                    case Direction.Left:
                        fHandleAngle = mbCollapsed ? 0f : MathHelper.Pi;
                        break;
                    case Direction.Right:
                        fHandleAngle = mbCollapsed ? MathHelper.Pi : 0f;
                        break;
                    case Direction.Up:
                        fHandleAngle = mbCollapsed ? MathHelper.PiOver2 : (3f * MathHelper.PiOver2);
                        break;
                    case Direction.Down:
                        fHandleAngle = mbCollapsed ? (3f * MathHelper.PiOver2) : MathHelper.PiOver2;
                        break;
                    default:
                        throw new NotSupportedException();
                }

                Screen.Game.SpriteBatch.Draw(HandleTex, new Vector2(HitBox.Center.X, HitBox.Center.Y), null, handleColor, fHandleAngle, new Vector2(HandleTex.Width / 2f, HandleTex.Height / 2f), 1f, SpriteEffects.None, 0f);
            }


            if (!InvertDrawOrder)
            {
                if (mFirstPane != null && mbDisplayFirstPane)
                {
                    mFirstPane.Draw();
                }

                if (mSecondPane != null && mbDisplaySecondPane)
                {
                    mSecondPane.Draw();
                }
            }
            else
            {
                if (mSecondPane != null && mbDisplaySecondPane)
                {
                    mSecondPane.Draw();
                }

                if (mFirstPane != null && mbDisplayFirstPane)
                {
                    mFirstPane.Draw();
                }
            }
        }
    }
}
