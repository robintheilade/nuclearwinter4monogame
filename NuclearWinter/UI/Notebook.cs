using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

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

        public override bool CanFocus { get { return true; } }

        //----------------------------------------------------------------------
        public FixedGroup       PageGroup        { get; private set; }

        //----------------------------------------------------------------------
        public NotebookTab( Notebook _notebook, string _strText, Texture2D _iconTex )
        : base( _notebook.Screen )
        {
            mNotebook       = _notebook;

            mLabel          = new Label( Screen );
            mIcon           = new Image( Screen, _iconTex );
            mIcon.Padding   = new Box( 10, 0, 10, 20 );

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

            ContentHeight   = Math.Max( mIcon.ContentHeight, mLabel.ContentHeight ) + Padding.Top + Padding.Bottom;
        }

        //----------------------------------------------------------------------
        public override void DoLayout( Rectangle _rect )
        {
            Position = _rect.Location;
            Size = new Point( _rect.Width, _rect.Height );

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
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            Screen.DrawBox( mNotebook.Style.Tab, new Rectangle( Position.X, Position.Y, Size.X, Size.Y ), mNotebook.Style.TabCornerSize, Color.White );

            mLabel.Draw();
            mIcon.Draw();
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

        const int                   TabHeight = 50;

        /*bool                mbIsMouseInTabs;
        bool                mbIsTabPressed;*/

        //----------------------------------------------------------------------
        public override bool CanFocus { get { return true; } }

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

        /*
        //----------------------------------------------------------------------
        bool IsInTabs( Point _hitPoint )
        {
            return _hitPoint.Y < Position.Y + TabHeight && _hitPoint.X >= Position.X + 20 && _hitPoint.X < Position.X  + 20 + mTabs.ContentWidth;
        }
        */

        public override void OnMouseEnter( Point _hitPoint )
        {
            base.OnMouseEnter( _hitPoint );

            /*
            if( IsInTabs( _hitPoint ) )
            {
                mbIsMouseInTabs = true;
                mTabs.OnMouseEnter( _hitPoint );
            }
            else
            {
                mbIsMouseInTabs = false;
                // ActivePage.OnMouseEnter( _hitPoint );
            }
            */
        }

        public override void OnMouseOut( Point _hitPoint )
        {
            base.OnMouseOut( _hitPoint );
            
            /*
            if( mbIsMouseInTabs )
            {
                mTabs.OnMouseOut( _hitPoint );
            }
            else
            {
                // ActivePage.OnMouseOut( _hitPoint );
            }*/
        }

        public override void OnMouseMove( Point _hitPoint )
        {
            base.OnMouseMove( _hitPoint );

            /*if( IsInTabs( _hitPoint ) || mbIsTabPressed )
            {
                if( ! mbIsMouseInTabs )
                {
                    // ActivePage.OnMouseOut( _hitPoint );
                    mTabs.OnMouseEnter( _hitPoint );
                }
                else
                {
                    mTabs.OnMouseMove( _hitPoint );
                }

                mbIsMouseInTabs = true;
            }
            else
            {
                if( mbIsMouseInTabs )
                {
                    mTabs.OnMouseOut( _hitPoint );
                    // ActivePage.OnMouseEnter( _hitPoint );
                }
                else
                {
                    // ActivePage.OnMouseMove( _hitPoint );
                }

                mbIsMouseInTabs = false;
            }*/
        }

        //----------------------------------------------------------------------
        public override void OnMouseDown( Point _hitPoint )
        {
            base.OnMouseDown( _hitPoint );

            /*if( mbIsMouseInTabs )
            {
                mbIsTabPressed = true;
                mTabs.OnMouseDown( _hitPoint );
            }*/
        }

        public override void OnMouseUp( Point _hitPoint )
        {
            /*if( mbIsTabPressed )
            {
                mbIsTabPressed = false;
                mTabs.OnMouseUp( _hitPoint );
            }*/
        }

        public void TabClicked( RadioButtonSet _tabs )
        {
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
