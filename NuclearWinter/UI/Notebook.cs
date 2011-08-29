using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using Microsoft.Xna.Framework.Input;

namespace NuclearWinter.UI
{
    //--------------------------------------------------------------------------
    public class NotebookTab: Widget
    {
        //----------------------------------------------------------------------
        Notebook                mNotebook;

        //----------------------------------------------------------------------
        Label                   mLabel;
        Image                   mIcon;
        Button                  mCloseButton;

        public bool             Active { get { return mNotebook.Tabs[mNotebook.ActiveTabIndex] == this; } }
        bool                    mbIsHovered;
        bool                    mbIsPressed;

        //----------------------------------------------------------------------
        bool                    mbClosable;
        public bool             Closable
        {
            get { return mbClosable; }
            set { mbClosable = value; UpdateContentSize(); }
        }

        //----------------------------------------------------------------------
        public string           Text
        {
            get
            {
                return mLabel.Text;
            }
            
            set
            {
                mLabel.Text = value;
                mLabel.Padding = mLabel.Text != "" ? new Box( 10, 20, 10, 20 ) : new Box( 10, 20, 10, 0 );
                UpdateContentSize();
            }
        }

        public Texture2D        Icon
        {
            get {
                return mIcon.Texture;
            }

            set
            {
                mIcon.Texture = value;
                UpdateContentSize();
            }
        }

        public Color TextColor
        {
            get { return mLabel.Color; }
            set { mLabel.Color = value; }
        }

        //----------------------------------------------------------------------
        public FixedGroup       PageGroup        { get; private set; }

        //----------------------------------------------------------------------
        public NotebookTab( Notebook _notebook, string _strText, Texture2D _iconTex )
        : base( _notebook.Screen )
        {
            mNotebook       = _notebook;

            mLabel          = new Label( Screen, "", Screen.Style.ButtonTextColor );
            mIcon           = new Image( Screen, _iconTex );
            mIcon.Padding   = new Box( 10, 0, 10, 20 );

            mCloseButton    = new Button( Screen, new Button.ButtonStyle( 5, null, null, Screen.Style.NotebookTabCloseHover, Screen.Style.NotebookTabCloseDown, null, 0, 0 ), "", Screen.Style.NotebookTabClose, Anchor.Center );
            mCloseButton.ClickHandler = delegate {
                mNotebook.Tabs.Remove( this );
            };

            Text            = _strText;

            PageGroup       = new FixedGroup( Screen );
        }

        //----------------------------------------------------------------------
        protected override void UpdateContentSize()
        {
            if( mIcon.Texture != null )
            {
                ContentWidth    = mIcon.ContentWidth + mLabel.ContentWidth + Padding.Left + Padding.Right;
            }
            else
            {
                ContentWidth    = mLabel.ContentWidth + Padding.Left + Padding.Right;
            }

            if( Closable )
            {
                ContentWidth += Screen.Style.NotebookTabClose.Width;
            }

            ContentHeight   = Math.Max( mIcon.ContentHeight, mLabel.ContentHeight ) + Padding.Top + Padding.Bottom;
        }

        //----------------------------------------------------------------------
        public override void DoLayout( Rectangle _rect )
        {
            Position = _rect.Location;
            Size = new Point( _rect.Width, _rect.Height );

            HitBox = _rect;

            Point pCenter = new Point( Position.X + Size.X / 2, Position.Y + Size.Y / 2 );

            if( mIcon.Texture != null )
            {
                mIcon.Position = new Point( Position.X + Padding.Left, pCenter.Y - mIcon.ContentHeight / 2 );
            }

            mLabel.DoLayout(
                new Rectangle(
                    Position.X + Padding.Left + ( mIcon.Texture != null ? mIcon.ContentWidth : 0 ), pCenter.Y - mLabel.ContentHeight / 2,
                    mLabel.ContentWidth, mLabel.ContentHeight
                )
            );

            if( Closable )
            {
                mCloseButton.DoLayout( new Rectangle(
                    Position.X + Size.X - 10 - Screen.Style.NotebookTabClose.Width,
                    Position.Y + Size.Y / 2 - Screen.Style.NotebookTabClose.Height / 2,
                    mCloseButton.ContentWidth, mCloseButton.ContentHeight )
                );
            }
        }

        //----------------------------------------------------------------------
        public override Widget HitTest( Point _point )
        {
            return mCloseButton.HitTest( _point ) ?? base.HitTest( _point );
        }

        public override void OnMouseDown( Point _hitPoint )
        {
            Screen.Focus( this );
            //OnActivateDown();
        }

        public override void OnMouseUp( Point _hitPoint )
        {
            if( _hitPoint.Y < mNotebook.Position.Y + mNotebook.TabHeight /* && IsInTab */ )
            {
                if( _hitPoint.X > Position.X && _hitPoint.X < Position.X + Size.X )
                {
                    OnActivateUp();
                }
            }
            /*else
            {
                ResetPressState();
            }*/
        }

        public override void OnMouseEnter( Point _hitPoint )
        {
            mbIsHovered = true;
        }

        public override void OnMouseOut( Point _hitPoint )
        {
            mbIsHovered = false;
        }

        public override void OnMouseMove( Point _hitPoint )
        {
        }

        public override void OnPadMove( Direction _direction )
        {
            int iTabIndex = mNotebook.Tabs.IndexOf( this );

            if( _direction == Direction.Left && iTabIndex > 0 )
            {
                NotebookTab tab = mNotebook.Tabs[iTabIndex - 1];
                Screen.Focus( tab );
            }
            else
            if( _direction == Direction.Right && iTabIndex < mNotebook.Tabs.Count - 1 )
            {
                NotebookTab tab = mNotebook.Tabs[iTabIndex  + 1];
                Screen.Focus( tab );
            }
            else
            {
                base.OnPadMove( _direction );
            }
        }

        public override void OnActivateUp()
        {
            mNotebook.SetActiveTab( this );
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            bool bIsActive = Active;

            Screen.DrawBox( bIsActive ? mNotebook.Style.ActiveTab : mNotebook.Style.Tab, new Rectangle( Position.X, Position.Y, Size.X, Size.Y ), mNotebook.Style.TabCornerSize, Color.White );

            if( mbIsHovered && ! bIsActive ) // && ! mbIsPressed && mPressedAnim.IsOver )
            {
                if( Screen.IsActive )
                {
                    Screen.DrawBox( Screen.Style.ButtonFrameHover, new Rectangle( Position.X, Position.Y, Size.X, Size.Y ), mNotebook.Style.TabCornerSize, Color.White );
                }
            }

            if( Screen.IsActive && HasFocus )
            {
                Screen.DrawBox( bIsActive ? mNotebook.Style.ActiveTabFocus : mNotebook.Style.TabFocus, new Rectangle( Position.X, Position.Y, Size.X, Size.Y ), mNotebook.Style.TabCornerSize, Color.White );
            }

            mLabel.Draw();
            mIcon.Draw();

            if( Closable )
            {
                mCloseButton.Draw();
            }
        }
    }

    //--------------------------------------------------------------------------
    public class Notebook: Widget
    {
        //----------------------------------------------------------------------
        public struct NotebookStyle
        {
            public NotebookStyle( int _iTabCornerSize, Texture2D _tab, Texture2D _tabFocus, Texture2D _activeTab, Texture2D _activeTabFocus )
            {
                TabCornerSize   = _iTabCornerSize;
                Tab             = _tab;
                TabFocus        = _tabFocus;
                ActiveTab       = _activeTab;
                ActiveTabFocus  = _activeTabFocus;
            }

            public int              TabCornerSize;

            public Texture2D        Tab;
            public Texture2D        TabFocus;
            public Texture2D        ActiveTab;
            public Texture2D        ActiveTabFocus;
        }

        //----------------------------------------------------------------------
        public NotebookStyle        Style;

        Panel                       mPanel;

        public List<NotebookTab>    Tabs            { get; private set; }
        public int                  ActiveTabIndex  { get; private set; }

        public int                  TabHeight = 50;

        //----------------------------------------------------------------------
        public Notebook( Screen _screen )
        : base( _screen )
        {
            Style = new NotebookStyle(
                Screen.Style.NotebookTabCornerSize,
                Screen.Style.NotebookTab,
                Screen.Style.NotebookTabFocus,
                Screen.Style.NotebookActiveTab,
                Screen.Style.NotebookActiveTabFocus
            );

            mPanel = new Panel( Screen, Screen.Style.Panel, Screen.Style.PanelCornerSize );
            Tabs = new List<NotebookTab>();
        }

        //----------------------------------------------------------------------
        protected override void UpdateContentSize()
        {
            ContentWidth    = 0;
            ContentHeight   = 0;
        }

        //----------------------------------------------------------------------
        public override void DoLayout( Rectangle _rect )
        {
            Position    = _rect.Location;
            Size        = new Point( _rect.Width, _rect.Height );

            HitBox = new Rectangle(
                Position.X,
                Position.Y,
                Size.X,
                Size.Y
            );

            ActiveTabIndex = Math.Min( Tabs.Count - 1, ActiveTabIndex );
            NotebookTab activeTab = Tabs[ActiveTabIndex];

            Rectangle contentRect = new Rectangle( Position.X, Position.Y + ( TabHeight - 10 ), Size.X, Size.Y - ( TabHeight - 10 ) );

            mPanel.DoLayout( contentRect );

            int iTabX = 0;
            foreach( NotebookTab tab in Tabs )
            {
                int iTabWidth = tab.ContentWidth;

                Rectangle tabRect = new Rectangle(
                    Position.X + 20 + iTabX,
                    Position.Y,
                    iTabWidth,
                    TabHeight
                    );

                tab.DoLayout( tabRect );

                iTabX += iTabWidth;
            }

            activeTab.PageGroup.DoLayout( contentRect );
        }

        //----------------------------------------------------------------------
        public void SetActiveTab( NotebookTab _tab )
        {
            Debug.Assert( Tabs.Contains( _tab ) );

            ActiveTabIndex = Tabs.IndexOf( _tab );
        }

        //----------------------------------------------------------------------
        public override Widget HitTest( Point _point )
        {
            if( _point.Y < Position.Y + TabHeight )
            {
                if( _point.X < Position.X + 20 ) return null;

                int iTabX = 0;
                int iTab = 0;

                foreach( NotebookTab tab in Tabs )
                {
                    int iTabWidth = tab.ContentWidth;

                    if( _point.X - Position.X - 20 < iTabX + iTabWidth )
                    {
                        return Tabs[ iTab ].HitTest( _point );
                    }

                    iTabX += iTabWidth;
                    iTab++;
                }

                return null;
            }
            else
            {
                return Tabs[ActiveTabIndex].PageGroup.HitTest( _point );
            }
        }

        public override bool OnPadButton( Buttons _button, bool _bIsDown )
        {
            return Tabs[ActiveTabIndex].OnPadButton( _button, _bIsDown );
        }

        public override bool Update( float _fElapsedTime )
        {
            return Tabs[ActiveTabIndex].Update( _fElapsedTime );
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            mPanel.Draw();

            foreach( NotebookTab tab in Tabs )
            {
                tab.Draw();
            }

            Tabs[ActiveTabIndex].PageGroup.Draw();
        }
    }
}
