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
        public Texture2D            Icon;
        public string               Text {
            get { return mLabel.Text; }
            set { mLabel.Text = value; }
        }

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
        public TreeViewNode( TreeView _treeView, string _strText, Texture2D _icon )
        : base( _treeView.Screen )
        {
            mTreeView   = _treeView;
            Children    = new ObservableList<TreeViewNode>();

            Children.ListChanged += delegate( object _source, ObservableList<TreeViewNode>.ListChangedEventArgs _args ) { _args.Item.Parent = this; UpdateContentSize(); };

            mLabel      = new Label( Screen, _strText, Anchor.Start, Screen.Style.ButtonTextColor );
            mImage      = new Image( Screen, _icon );

            UpdateContentSize();
        }

        public TreeViewNode( TreeView _treeView, string _strText )
        : this( _treeView, _strText, null )
        {
        }

        //----------------------------------------------------------------------
        protected override void UpdateContentSize()
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
        public override void DoLayout( Rectangle _rect )
        {
            Position = _rect.Location;
            Size = new Point( _rect.Width, _rect.Height );
            HitBox = _rect;
            
            if( Children.Count > 0 || DisplayAsContainer )
            {
                mLabel.DoLayout( new Rectangle( Position.X + mTreeView.NodeBranchWidth, Position.Y, Size.X - mTreeView.NodeBranchWidth, mTreeView.NodeHeight ) );
            }
            else
            {
                mLabel.DoLayout( new Rectangle( Position.X, Position.Y, Size.X, mTreeView.NodeHeight ) );
            }

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
        public override void Draw()
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

            mImage.Draw();
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
        protected override void UpdateContentSize()
        {
            ContentWidth    = Padding.Left + Padding.Right;
            ContentHeight   = Padding.Top + Padding.Bottom;
        }

        //----------------------------------------------------------------------
        public override void DoLayout( Rectangle _rect )
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
        public override void Draw()
        {
            Screen.DrawBox( Screen.Style.GridFrame, new Rectangle( Position.X, Position.Y, Size.X, Size.Y ), 30, Color.White );

            foreach( TreeViewNode node in Nodes )
            {
                node.Draw();
            }
        }

    }
}
