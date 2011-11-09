using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace NuclearWinter.UI
{
    public abstract class PopupPane<T>: Pane where T:IMenuManager
    {
        public T                    Manager { get; private set; }

        public Point Size {
            get { return mSize; }
            set {
                mSize = value;
                mPanelContainer.ChildBox.Width = mSize.X;
                mPanelContainer.ChildBox.Height = mSize.Y;

                mPageContainer.ChildBox.Width = mSize.X;
                mPageContainer.ChildBox.Height = mSize.Y;
            }
        }

        Point                       mSize               = new Point( 800, 450 );
        FixedWidget                 mPanelContainer;
        protected FixedWidget       mPageContainer;

        //----------------------------------------------------------------------
        public PopupPane( T _manager )
        {
            Manager     = _manager;
            FixedGroup  = new NuclearWinter.UI.FixedGroup( Manager.PopupScreen );

            Panel panel = new Panel( FixedGroup.Screen, FixedGroup.Screen.Style.PopupFrame, FixedGroup.Screen.Style.PopupFrameCornerSize );

            mPanelContainer = new FixedWidget( panel, AnchoredRect.CreateCentered( mSize.X, mSize.Y ) );
            FixedGroup.AddChild( mPanelContainer );

            mPageContainer = new FixedWidget( FixedGroup.Screen, AnchoredRect.CreateCentered( mSize.X, mSize.Y ) );
            FixedGroup.AddChild( mPageContainer );
        }

        //----------------------------------------------------------------------
        public void Open( int _iWidth, int _iHeight )
        {
            Size = new Point( _iWidth, _iHeight );
            Open();
        }
    }
}
