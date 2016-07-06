using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NuclearWinter.Collections;
using System;
using System.Diagnostics;
using System.Linq;

namespace NuclearWinter.UI
{
    //--------------------------------------------------------------------------
    public class NotebookTab : Widget
    {
        //----------------------------------------------------------------------
        public Group PageGroup { get; private set; }
        public object Tag;

        public bool IsActive { get { return mNotebook.Tabs[mNotebook.ActiveTabIndex] == this; } }

        bool mbIsUnread;
        public bool IsUnread
        {
            get { return mbIsUnread; }
            set
            {
                mbIsUnread = value;
                if (OnUnreadStatusChanged != null)
                {
                    OnUnreadStatusChanged(mbIsUnread);
                }
            }
        }

        public Action<bool> OnUnreadStatusChanged;

        public bool IsPinned
        {
            get { return mbIsPinned; }
            set { mbIsPinned = value; UpdateContentSize(); }
        }

        public bool IsClosable
        {
            get { return mNotebook.HasClosableTabs && !IsPinned; }
        }

        //----------------------------------------------------------------------
        public string Text
        {
            get { return mLabel.Text; }

            set
            {
                mLabel.Text = value;
                mTooltip.Text = value;

                UpdatePaddings();
                UpdateContentSize();
            }
        }

        //----------------------------------------------------------------------
        public Color TextColor
        {
            get { return mLabel.Color; }
            set { mLabel.Color = value; }
        }

        //----------------------------------------------------------------------
        public Texture2D Icon
        {
            get { return mIcon.Texture; }

            set
            {
                mIcon.Texture = value;
                UpdatePaddings();
                UpdateContentSize();
            }
        }

        //----------------------------------------------------------------------
        Notebook mNotebook;
        Tooltip mTooltip;

        //----------------------------------------------------------------------
        Label mLabel;
        Image mIcon;

        Button mCloseButton;
        bool mbIsPinned;

        internal int DragOffset;

        //----------------------------------------------------------------------
        void UpdatePaddings()
        {
            int iHorizontal = mNotebook.Style.HorizontalTabPadding;
            int iVertical = mNotebook.Style.VerticalTabPadding;

            if (mIcon.Texture != null)
            {
                mIcon.Padding = mLabel.Text != "" ? new Box(iVertical, 0, iVertical, iHorizontal / 2) : new Box(iVertical, 0, iVertical, iHorizontal);
                mLabel.Padding = mLabel.Text != "" ? new Box(iVertical, iHorizontal / 2, iVertical, iHorizontal / 2) : new Box(iVertical, iHorizontal, iVertical, 0);
            }
            else
            {
                mLabel.Padding = new Box(iVertical, iHorizontal);
            }
        }

        //----------------------------------------------------------------------
        public NotebookTab(Notebook notebook, string text, Texture2D iconTexture)
        : base(notebook.Screen)
        {
            mNotebook = notebook;
            Parent = notebook;

            mLabel = new Label(Screen, "", Anchor.Start, Screen.Style.DefaultTextColor);
            mIcon = new Image(Screen, iconTexture);

            mTooltip = new Tooltip(Screen, "");

            mCloseButton = new Button(Screen, new Button.ButtonStyle(5, null, null, mNotebook.Style.TabCloseHover, mNotebook.Style.TabCloseDown, null, 0, 0), "", mNotebook.Style.TabClose, Anchor.Center);
            mCloseButton.Parent = this;
            mCloseButton.Padding = new Box(0);
            mCloseButton.ClickHandler = delegate
            {
                mNotebook.Tabs.Remove(this);

                Screen.Focus(mNotebook);

                if (mNotebook.TabClosedHandler != null)
                {
                    mNotebook.TabClosedHandler(this);
                }
            };

            Text = text;

            PageGroup = new Group(Screen);
            PageGroup.Parent = this;
        }

        //----------------------------------------------------------------------
        protected internal override void UpdateContentSize()
        {
            if (mIcon.Texture != null)
            {
                ContentWidth = mIcon.ContentWidth + mLabel.ContentWidth + Padding.Horizontal;
            }
            else
            {
                ContentWidth = mLabel.ContentWidth + Padding.Horizontal;
            }

            if (IsClosable)
            {
                ContentWidth += mCloseButton.ContentWidth;
            }

            ContentHeight = Math.Max(mIcon.ContentHeight, mLabel.ContentHeight) + Padding.Top + Padding.Bottom;

            base.UpdateContentSize();
        }

        //----------------------------------------------------------------------
        public override void Update(float elapsedTime)
        {
            mCloseButton.Update(elapsedTime);
            PageGroup.Update(elapsedTime);

            mTooltip.EnableDisplayTimer = mNotebook.HoveredTab == this && mNotebook.DraggedTab != this;
            mTooltip.Update(elapsedTime);
        }

        //----------------------------------------------------------------------
        public override void DoLayout(Rectangle rectangle)
        {
            base.DoLayout(rectangle);

            HitBox = rectangle;

            Point pCenter = LayoutRect.Center;

            if (mIcon.Texture != null)
            {
                mIcon.DoLayout(new Rectangle(LayoutRect.X + Padding.Left, pCenter.Y - mIcon.ContentHeight / 2, mIcon.ContentWidth, mIcon.ContentHeight));
            }

            int iLabelWidth = LayoutRect.Width - Padding.Horizontal - (mIcon.Texture != null ? mIcon.ContentWidth : 0) - (IsClosable ? mCloseButton.ContentWidth : 0);

            mLabel.DoLayout(
                new Rectangle(
                    LayoutRect.X + Padding.Left + (mIcon.Texture != null ? mIcon.ContentWidth : 0), pCenter.Y - mLabel.ContentHeight / 2,
                    iLabelWidth, mLabel.ContentHeight
                )
            );

            if (IsClosable)
            {
                mCloseButton.DoLayout(new Rectangle(
                    LayoutRect.Right - 10 - mCloseButton.ContentWidth,
                    pCenter.Y - mNotebook.Style.TabClose.Height / 2,
                    mCloseButton.ContentWidth, mCloseButton.ContentHeight)
                );
            }
        }

        //----------------------------------------------------------------------
        public override Widget HitTest(Point point)
        {
            return mCloseButton.HitTest(point) ?? base.HitTest(point);
        }

        protected internal override bool OnMouseDown(Point hitPoint, int button)
        {
            if (button == Screen.Game.InputMgr.PrimaryMouseButton)
            {
                Screen.Focus(this);

                if (IsClosable)
                {
                    mNotebook.DraggedTab = this;
                    DragOffset = hitPoint.X - LayoutRect.X;
                }
            }
            else
            if (button == 1)
            {
                Screen.Focus(this);
            }

            return true;
        }

        protected internal override void OnMouseUp(Point hitPoint, int button)
        {
            if (button == Screen.Game.InputMgr.PrimaryMouseButton)
            {
                if (mNotebook.DraggedTab == this)
                {
                    mNotebook.DropTab();
                    DragOffset = 0;
                }

                if (hitPoint.Y < mNotebook.LayoutRect.Y + mNotebook.Style.TabHeight /* && IsInTab */ )
                {
                    if (hitPoint.X > LayoutRect.X && hitPoint.X < LayoutRect.Right)
                    {
                        OnActivateUp();
                    }
                }
            }
            else
            if (button == 1)
            {
                if (IsClosable && HitBox.Contains(hitPoint))
                {
                    Close();
                    Screen.Focus(mNotebook);

                    if (mNotebook.TabClosedHandler != null)
                    {
                        mNotebook.TabClosedHandler(this);
                    }
                }
            }
        }

        public override void OnMouseEnter(Point hitPoint)
        {
            mNotebook.HoveredTab = this;
        }

        public override void OnMouseOut(Point hitPoint)
        {
            if (mNotebook.HoveredTab == this)
            {
                mNotebook.HoveredTab = null;
            }

            mTooltip.EnableDisplayTimer = false;
        }

        public override void OnMouseMove(Point hitPoint)
        {
            if (mNotebook.DraggedTab == this)
            {
                if (mNotebook.Tabs[mNotebook.ActiveTabIndex] != this)
                {
                    mNotebook.SetActiveTab(this);
                }
            }
        }

        protected internal override void OnPadMove(Direction direction)
        {
            int iTabIndex = mNotebook.Tabs.IndexOf(this);

            if (direction == Direction.Left && iTabIndex > 0)
            {
                NotebookTab tab = mNotebook.Tabs[iTabIndex - 1];
                Screen.Focus(tab);
            }
            else
            if (direction == Direction.Right && iTabIndex < mNotebook.Tabs.Count - 1)
            {
                NotebookTab tab = mNotebook.Tabs[iTabIndex + 1];
                Screen.Focus(tab);
            }
            else
            {
                base.OnPadMove(direction);
            }
        }

        protected internal override void OnActivateUp()
        {
            mNotebook.SetActiveTab(this);
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            if (mNotebook.DraggedTab != this)
            {
                DrawTab();
            }
        }

        void DrawTab()
        {
            bool bIsActive = IsActive;

            Screen.DrawBox(bIsActive ? mNotebook.Style.ActiveTab : mNotebook.Style.Tab, LayoutRect, mNotebook.Style.TabCornerSize, Color.White);

            if (mNotebook.HoveredTab == this && !bIsActive)
            {
                if (Screen.IsActive)
                {
                    Screen.DrawBox(Screen.Style.ButtonHoverOverlay, LayoutRect, mNotebook.Style.TabCornerSize, Color.White);
                }
            }

            if (IsUnread)
            {
                Screen.DrawBox(mNotebook.Style.UnreadTabMarker, LayoutRect, mNotebook.Style.TabCornerSize, Color.White);
            }

            if (Screen.IsActive && HasFocus)
            {
                Screen.DrawBox(bIsActive ? mNotebook.Style.ActiveTabFocus : mNotebook.Style.TabFocus, LayoutRect, mNotebook.Style.TabCornerSize, Color.White);
            }

            mLabel.Draw();
            mIcon.Draw();

            if (IsClosable)
            {
                mCloseButton.Draw();
            }
        }

        //----------------------------------------------------------------------
        protected internal override void DrawHovered()
        {
            if (!mLabel.HasEllipsis) return;

            mTooltip.Draw();
        }

        //----------------------------------------------------------------------
        protected internal override void DrawFocused()
        {
            if (mNotebook.DraggedTab == this)
            {
                DrawTab();
            }
        }

        //----------------------------------------------------------------------
        public void Close(bool triggerCloseHandler = false)
        {
            mNotebook.Tabs.Remove(this);

            if (HasFocus)
            {
                Screen.Focus(mNotebook);
            }

            if (triggerCloseHandler && mNotebook.TabClosedHandler != null)
            {
                mNotebook.TabClosedHandler(this);
            }

        }
    }

    //--------------------------------------------------------------------------
    public class Notebook : Widget
    {
        //----------------------------------------------------------------------
        public struct NotebookStyle
        {
            public NotebookStyle(
                int tabCornerSize, int tabHeight, int tabBarPanelVerticalOffset,
                Texture2D tab = null, Texture2D tabFocus = null, Texture2D activeTab = null, Texture2D activeTabFocus = null, Texture2D unreadTabMarker = null,
                int horizontalTabPadding = 20,
                int verticalTabPadding = 10,
                int tabBarLeftOffset = 20,
                int tabBarRightOffset = 20,
                int unpinnedTabWidth = 250,
                Texture2D tabClose = null, Texture2D tabCloseHover = null, Texture2D tabCloseDown = null)
            {
                TabCornerSize = tabCornerSize;
                TabHeight = tabHeight;
                TabBarPanelVerticalOffset = tabBarPanelVerticalOffset;

                Tab = tab;
                TabFocus = tabFocus;
                ActiveTab = activeTab;
                ActiveTabFocus = activeTabFocus;
                UnreadTabMarker = unreadTabMarker;

                HorizontalTabPadding = horizontalTabPadding;
                VerticalTabPadding = verticalTabPadding;

                TabBarLeftOffset = tabBarLeftOffset;
                TabBarRightOffset = tabBarRightOffset;
                UnpinnedTabWidth = unpinnedTabWidth;

                TabClose = tabClose;
                TabCloseHover = tabCloseHover;
                TabCloseDown = tabCloseDown;
            }

            public int TabCornerSize;
            public int TabHeight;
            public int TabBarPanelVerticalOffset;

            public Texture2D Tab;
            public Texture2D TabFocus;
            public Texture2D ActiveTab;
            public Texture2D ActiveTabFocus;
            public Texture2D UnreadTabMarker;

            public int HorizontalTabPadding;
            public int VerticalTabPadding;

            public int TabBarLeftOffset;
            public int TabBarRightOffset;
            public int UnpinnedTabWidth;

            public Texture2D TabClose;
            public Texture2D TabCloseHover;
            public Texture2D TabCloseDown;
        }

        //----------------------------------------------------------------------
        public NotebookStyle Style;

        public ObservableList<NotebookTab> Tabs { get; private set; }

        public bool HasClosableTabs
        {
            get { return mbHasClosableTabs; }
            set
            {
                mbHasClosableTabs = value;

                foreach (NotebookTab tab in Tabs)
                {
                    tab.UpdateContentSize();
                }
            }
        }

        public Action<NotebookTab> TabClosedHandler;

        public int ActiveTabIndex { get; private set; }

        //----------------------------------------------------------------------
        int miUnpinnedTabWidth;

        internal NotebookTab HoveredTab;
        internal NotebookTab DraggedTab;
        int miDraggedTabTargetIndex = -1;

        //----------------------------------------------------------------------
        Panel mPanel;
        bool mbHasClosableTabs;

        //----------------------------------------------------------------------
        public Notebook(Screen screen)
        : base(screen)
        {
            Style = Screen.Style.NotebookStyle;

            mPanel = new Panel(Screen, Screen.Style.Panel, Screen.Style.PanelCornerSize);

            Tabs = new ObservableList<NotebookTab>();

            Tabs.ListChanged += delegate (object _source, ObservableList<NotebookTab>.ListChangedEventArgs _args)
            {
                if (!_args.Added)
                {
                    if (DraggedTab == _args.Item)
                    {
                        DraggedTab = null;
                    }

                    if (HoveredTab == _args.Item)
                    {
                        HoveredTab = null;
                    }
                }

                if (Tabs.Count > 0)
                {
                    ActiveTabIndex = Math.Min(Tabs.Count - 1, ActiveTabIndex);
                    Tabs[ActiveTabIndex].IsUnread = false;
                }
            };
        }

        //----------------------------------------------------------------------
        protected internal override void UpdateContentSize()
        {
            // No content size
        }

        //----------------------------------------------------------------------
        public override void DoLayout(Rectangle rectangle)
        {
            base.DoLayout(rectangle);
            HitBox = LayoutRect;

            Rectangle contentRect = new Rectangle(LayoutRect.X, LayoutRect.Y + (Style.TabHeight - Style.TabBarPanelVerticalOffset), LayoutRect.Width, LayoutRect.Height - (Style.TabHeight - Style.TabBarPanelVerticalOffset));

            mPanel.DoLayout(contentRect);

            int iTabBarStartX = LayoutRect.Left + Style.TabBarLeftOffset;
            int iTabBarEndX = LayoutRect.Right - Style.TabBarRightOffset;

            int iTabBarWidth = iTabBarEndX - iTabBarStartX;

            int iUnpinnedTabsCount = Tabs.Count(tab => !tab.IsPinned);

            int iPinnedTabsWidth = Tabs.Sum(tab => tab.IsPinned ? tab.ContentWidth : 0);
            int iUnpinnedTabsWidth = Style.UnpinnedTabWidth * iUnpinnedTabsCount;

            miUnpinnedTabWidth = Style.UnpinnedTabWidth;

            if (iPinnedTabsWidth + iUnpinnedTabsWidth > iTabBarWidth && iUnpinnedTabsCount > 0)
            {
                miUnpinnedTabWidth = (iTabBarWidth - iPinnedTabsWidth) / iUnpinnedTabsCount;
            }

            int iDraggedTabX = 0;
            if (DraggedTab != null)
            {
                iDraggedTabX = Math.Min(iTabBarEndX - miUnpinnedTabWidth, Math.Max(iTabBarStartX, Screen.Game.InputMgr.MouseState.X - DraggedTab.DragOffset));
            }

            int iTabX = 0;
            int iTabIndex = 0;
            bool bDraggedTabInserted = DraggedTab == null;
            miDraggedTabTargetIndex = Tabs.Count - 1;
            foreach (NotebookTab tab in Tabs)
            {
                if (tab == DraggedTab) continue;

                int iTabWidth = tab.IsPinned ? tab.ContentWidth : miUnpinnedTabWidth;

                if (!tab.IsPinned && !bDraggedTabInserted && iDraggedTabX - iTabBarStartX < iTabX + iTabWidth / 2)
                {
                    miDraggedTabTargetIndex = iTabIndex;
                    iTabX += miUnpinnedTabWidth;
                    bDraggedTabInserted = true;
                }

                Rectangle tabRect = new Rectangle(
                    iTabBarStartX + iTabX,
                    LayoutRect.Y,
                    iTabWidth,
                    Style.TabHeight
                    );

                tab.DoLayout(tabRect);

                iTabX += iTabWidth;
                iTabIndex++;
            }

            if (DraggedTab != null)
            {
                Rectangle tabRect = new Rectangle(
                    iDraggedTabX,
                    LayoutRect.Y,
                    miUnpinnedTabWidth,
                    Style.TabHeight
                    );

                DraggedTab.DoLayout(tabRect);
            }

            Tabs[ActiveTabIndex].PageGroup.DoLayout(contentRect);
        }

        //----------------------------------------------------------------------
        public void SetActiveTab(NotebookTab tab)
        {
            Debug.Assert(Tabs.Contains(tab));

            ActiveTabIndex = Tabs.IndexOf(tab);
            tab.IsUnread = false;
        }

        //----------------------------------------------------------------------
        internal void DropTab()
        {
            NotebookTab droppedTab = DraggedTab;
            int iOldIndex = Tabs.IndexOf(droppedTab);

            if (miDraggedTabTargetIndex != iOldIndex)
            {
                Tabs.RemoveAt(iOldIndex);
                Tabs.Insert(miDraggedTabTargetIndex, droppedTab);
                ActiveTabIndex = miDraggedTabTargetIndex;
            }

            DraggedTab = null;
            miDraggedTabTargetIndex = -1;
        }

        //----------------------------------------------------------------------
        public override Widget HitTest(Point point)
        {
            if (point.Y < LayoutRect.Y + Style.TabHeight)
            {
                int iTabBarStartX = LayoutRect.Left + Style.TabBarLeftOffset;

                if (point.X < iTabBarStartX) return null;

                int iTabX = 0;
                int iTab = 0;

                foreach (NotebookTab tab in Tabs)
                {
                    int iTabWidth = tab.IsPinned ? tab.ContentWidth : miUnpinnedTabWidth;

                    if (point.X - iTabBarStartX < iTabX + iTabWidth)
                    {
                        return Tabs[iTab].HitTest(point);
                    }

                    iTabX += iTabWidth;
                    iTab++;
                }

                return null;
            }
            else
            {
                return Tabs[ActiveTabIndex].PageGroup.HitTest(point);
            }
        }

        protected internal override bool OnPadButton(Buttons button, bool isDown)
        {
            return Tabs[ActiveTabIndex].OnPadButton(button, isDown);
        }

        //----------------------------------------------------------------------
        protected internal override void OnKeyPress(Keys key)
        {
            bool bShortcutKey = Screen.Game.InputMgr.IsShortcutKeyDown();

            bool bCtrl = Screen.Game.InputMgr.KeyboardState.IsKeyDown(Keys.LeftControl, true) || Screen.Game.InputMgr.KeyboardState.IsKeyDown(Keys.RightControl, true);
            bool bShift = Screen.Game.InputMgr.KeyboardState.IsKeyDown(Keys.LeftShift, true) || Screen.Game.InputMgr.KeyboardState.IsKeyDown(Keys.RightShift, true);

            var activeTab = Tabs[ActiveTabIndex];
            if (bShortcutKey && key == Keys.W && activeTab.IsClosable)
            {
                activeTab.Close();
                Screen.Focus(this);

                if (TabClosedHandler != null)
                {
                    TabClosedHandler(activeTab);
                }
            }
            else if (bCtrl && key == Keys.Tab)
            {
                if (bShift)
                {
                    int iActiveTabIndex = ActiveTabIndex - 1;
                    if (iActiveTabIndex == -1) iActiveTabIndex = Tabs.Count - 1;
                    ActiveTabIndex = iActiveTabIndex;
                }
                else
                {
                    ActiveTabIndex = (ActiveTabIndex + 1) % Tabs.Count;
                }

                Tabs[ActiveTabIndex].IsUnread = false;

                Screen.Focus(Tabs[ActiveTabIndex]);
            }
            else
            {
                base.OnKeyPress(key);
            }
        }

        //----------------------------------------------------------------------
        public override void Update(float elapsedTime)
        {
            foreach (NotebookTab tab in Tabs)
            {
                tab.Update(elapsedTime);
            }
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            mPanel.Draw();

            foreach (NotebookTab tab in Tabs)
            {
                tab.Draw();
            }

            Tabs[ActiveTabIndex].PageGroup.Draw();
        }
    }
}
