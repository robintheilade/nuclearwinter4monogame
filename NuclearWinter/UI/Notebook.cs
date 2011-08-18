using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NuclearWinter.UI
{
    //--------------------------------------------------------------------------
    public class Notebook: Widget
    {
        //----------------------------------------------------------------------
        RadioButtonSet.RadioButtonSetStyle  mTabStyle;

        RadioButtonSet      mTabs;
        Panel               mPanel;

        const int           TabHeight = 50;
        bool                mbIsMouseInTabs;
        bool                mbIsTabPressed;

        //----------------------------------------------------------------------
        public override bool CanFocus { get { return true; } }

        //----------------------------------------------------------------------
        public Notebook( Screen _screen, List<Button> _lButtons )
        : base( _screen )
        {
            mTabStyle = new RadioButtonSet.RadioButtonSetStyle(
                _screen.Style.NotebookTabCornerSize,

                _screen.Style.NotebookTab,
                _screen.Style.NotebookTab,
                _screen.Style.NotebookTab,

                _screen.Style.NotebookActiveTab,
                _screen.Style.NotebookActiveTab,
                _screen.Style.NotebookActiveTab,

                _screen.Style.ButtonFrameFocused,
                _screen.Style.ButtonFrameFocused // FIXME: FocusedActiveTab
            );

            mTabs = new RadioButtonSet( Screen, mTabStyle, _lButtons );
            mTabs.Expand = true;
            mTabs.ClickHandler = TabClicked;
            mPanel = new Panel( Screen, Screen.Style.Panel, Screen.Style.PanelCornerSize );
        }

        //----------------------------------------------------------------------
        protected override void UpdateContentSize()
        {
            ContentWidth    = mTabs.ContentWidth;
            ContentHeight   = mTabs.ContentHeight;
        }

        //----------------------------------------------------------------------
        public override void DoLayout( Rectangle? _rect )
        {
            if( _rect.HasValue )
            {
                Position    = _rect.Value.Location;
                Size        = new Point( _rect.Value.Width, _rect.Value.Height );
            }

            HitBox = new Rectangle(
                Position.X,
                Position.Y,
                Size.X,
                Size.Y
            );

            mTabs.DoLayout( new Rectangle( Position.X + 20, Position.Y, mTabs.ContentWidth, TabHeight ) );
            mPanel.DoLayout( new Rectangle( Position.X, Position.Y + ( TabHeight - 10 ), Size.X, Size.Y - ( TabHeight - 10 ) ) );
        }

        //----------------------------------------------------------------------
        public override void OnMouseEnter( Point _hitPoint )
        {
            base.OnMouseEnter( _hitPoint );

            if( _hitPoint.Y < Position.Y + TabHeight )
            {
                mbIsMouseInTabs = true;
                mTabs.OnMouseEnter( _hitPoint );
            }
            else
            {
                mbIsMouseInTabs = false;
                // ActivePage.OnMouseEnter( _hitPoint );
            }
        }

        public override void OnMouseOut( Point _hitPoint )
        {
            base.OnMouseOut( _hitPoint );
            
            if( mbIsMouseInTabs )
            {
                mTabs.OnMouseOut( _hitPoint );
            }
            else
            {
                // ActivePage.OnMouseOut( _hitPoint );
            }
        }

        public override void OnMouseMove( Point _hitPoint )
        {
            base.OnMouseMove( _hitPoint );

            if( _hitPoint.Y < Position.Y + TabHeight || mbIsTabPressed )
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
            }
        }

        //----------------------------------------------------------------------
        public override void OnMouseDown( Point _hitPoint )
        {
            base.OnMouseDown( _hitPoint );

            if( mbIsMouseInTabs )
            {
                mbIsTabPressed = true;
                mTabs.OnMouseDown( _hitPoint );
            }
        }

        public override void OnMouseUp( Point _hitPoint )
        {
            if( mbIsTabPressed )
            {
                mbIsTabPressed = false;
                mTabs.OnMouseUp( _hitPoint );
            }
        }

        public void TabClicked( RadioButtonSet _tabs )
        {
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            mPanel.Draw();
            mTabs.Draw();
        }
    }
}
