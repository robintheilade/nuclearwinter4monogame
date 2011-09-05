using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NuclearWinter.Collections;

namespace NuclearWinter.UI
{
    //--------------------------------------------------------------------------
    public class TreeViewNode: Widget
    {
        //----------------------------------------------------------------------
        public Texture2D            Icon
        {
            get { return mImage.Texture; }
            set {
                mImage.Texture = value;
                mLabel.Padding = mImage.Texture != null ? new Box( 10, 10, 10, 0 ) : new Box( 10 );
            }
        }

        public string               Text
        {
            get { return mLabel.Text; }
            set { mLabel.Text = value; }
        }

        public object               Tag;

        public ObservableList<TreeViewNode>     Children        { get; private set; }

        public bool                 DisplayAsContainer;
        public bool                 Collapsed {
            get { return mbCollapsed; }
            set {
                mbCollapsed = value;
                UpdateContentSize();
            }
        }

        //----------------------------------------------------------------------
        TreeView                    mTreeView;

        Label                       mLabel;
        Image                       mImage;

        bool                        mbCollapsed;
        bool                        mbIsLast;

        //----------------------------------------------------------------------
        public TreeViewNode( TreeView _treeView, string _strText, Texture2D _icon, object _tag )
        : base( _treeView.Screen )
        {
            mTreeView   = _treeView;
            Children    = new ObservableList<TreeViewNode>();

            Children.ListChanged += delegate( object _source, ObservableList<TreeViewNode>.ListChangedEventArgs _args ) { _args.Item.Parent = this; UpdateContentSize(); };

            mLabel      = new Label( Screen, _strText, Anchor.Start, Screen.Style.DefaultTextColor );
            mImage      = new Image( Screen );
            mImage.Padding = new Box( 0, 5, 0, 10 );

            Icon = _icon;

            UpdateContentSize();
        }

        public TreeViewNode( TreeView _treeView, string _strText, Texture2D _icon )
        : this( _treeView, _strText, _icon, null )
        {

        }

        public TreeViewNode( TreeView _treeView, string _strText, object _tag )
        : this( _treeView, _strText, null, _tag )
        {

        }

        public TreeViewNode( TreeView _treeView, string _strText )
        : this( _treeView, _strText, null, null )
        {
        }

        //----------------------------------------------------------------------
        internal override void UpdateContentSize()
        {
            ContentHeight = mTreeView.NodeHeight;
            if( Children.Count > 0 && ! mbCollapsed )
            {
                ContentHeight += mTreeView.NodeSpacing * (Children.Count + 1);
                foreach( TreeViewNode child in Children )
                {
                    ContentHeight += child.ContentHeight;
                }
            }

            if( Parent is TreeViewNode )
            {
                ((TreeViewNode)Parent).ChildSizeChanged();
            }
        }

        //----------------------------------------------------------------------
        void ChildSizeChanged()
        {
            UpdateContentSize();
        }

        //----------------------------------------------------------------------
        internal override void DoLayout( Rectangle _rect )
        {
            Position = _rect.Location;
            Size = new Point( _rect.Width, _rect.Height );
            HitBox = _rect;
            
            int iLabelX = ( Children.Count > 0 || DisplayAsContainer ) ? mTreeView.NodeBranchWidth : 0;

            if( mImage.Texture != null )
            {
                mImage.DoLayout( new Rectangle( Position.X + iLabelX, Position.Y, mImage.ContentWidth, mTreeView.NodeHeight ) );
                iLabelX += mImage.ContentWidth;
            }

            mLabel.DoLayout( new Rectangle( Position.X + iLabelX, Position.Y, Size.X - iLabelX, mTreeView.NodeHeight ) );

            int iX = Position.X;
            int iY = Position.Y + mTreeView.NodeHeight + mTreeView.NodeSpacing;
            foreach( TreeViewNode child in Children )
            {
                child.DoLayout( new Rectangle( iX + mTreeView.NodeBranchWidth, iY, Size.X - mTreeView.NodeBranchWidth, child.ContentHeight ) );
                iY += child.ContentHeight + mTreeView.NodeSpacing;
            }

            if( Parent != null )
            {
                TreeViewNode parent = ((TreeViewNode)Parent);
                mbIsLast = parent.Children[ parent.Children.Count - 1 ] == this;
            }
        }

        //----------------------------------------------------------------------
        internal override void Draw()
        {
            if( Parent != null )
            {
                Screen.Game.SpriteBatch.Draw( mbIsLast ? Screen.Style.TreeViewBranchLast : Screen.Style.TreeViewBranch, new Vector2( Position.X - mTreeView.NodeBranchWidth, Position.Y ), Color.White );
            }

            if( Children.Count == 0 && ! DisplayAsContainer )
            {
                Screen.DrawBox( Screen.Style.GridBoxFrame, new Rectangle( Position.X, Position.Y, Size.X, mTreeView.NodeHeight ), Screen.Style.GridBoxFrameCornerSize, Color.White );
            }
            else
            {
                Texture2D tex = Children.Count == 0 ? Screen.Style.TreeViewBranchOpenEmpty : Screen.Style.TreeViewBranchOpen;

                if( Collapsed )
                {
                    tex = Screen.Style.TreeViewBranchClosed;
                }

                Screen.Game.SpriteBatch.Draw( tex, new Vector2( Position.X, Position.Y ), Color.White );
            }

            if( mImage.Texture != null )
            {
                mImage.Draw();
            }

            mLabel.Draw();

            if( ! mbCollapsed )
            {
                foreach( TreeViewNode child in Children )
                {
                    child.Draw();
                }
            }
        }
    }

    //--------------------------------------------------------------------------
    public class TreeView: Widget
    {
        //----------------------------------------------------------------------
        public List<TreeViewNode>           Nodes               { get; private set; }

        public int                          NodeHeight      = 40;
        public int                          NodeSpacing     = 0;
        public int                          NodeBranchWidth = 25;

        //----------------------------------------------------------------------
        public TreeView( Screen _screen )
        : base( _screen )
        {
            Nodes = new List<TreeViewNode>();
        }

        //----------------------------------------------------------------------
        internal override void UpdateContentSize()
        {
            ContentWidth    = Padding.Left + Padding.Right;
            ContentHeight   = Padding.Top + Padding.Bottom;
        }

        //----------------------------------------------------------------------
        internal override void DoLayout( Rectangle _rect )
        {
            Position = _rect.Location;
            Size = new Point( _rect.Width, _rect.Height );
            HitBox = _rect;

            int iX = Position.X + 10;
            int iY = Position.Y + 10;
            foreach( TreeViewNode node in Nodes )
            {
                node.DoLayout( new Rectangle( iX, iY, Size.X - 20, node.ContentHeight ) );
                iY += node.ContentHeight;
            }
        }

        //----------------------------------------------------------------------
        internal override void Draw()
        {
            Screen.DrawBox( Screen.Style.GridFrame, new Rectangle( Position.X, Position.Y, Size.X, Size.Y ), 30, Color.White );

            foreach( TreeViewNode node in Nodes )
            {
                node.Draw();
            }
        }

    }
}
