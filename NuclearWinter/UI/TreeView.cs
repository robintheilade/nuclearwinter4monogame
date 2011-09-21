using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NuclearWinter.Collections;
using System.Diagnostics;

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

        string mstrText = "";
        public string               Text
        {
            get { return mstrText; }
            set {
                mstrText = value;
                UpdateLabel();
            }
        }

        void UpdateLabel()
        {
            mLabel.Text = Collapsed ? string.Format( "{0} ({1})", mstrText, ContainedNodeCount ) : mstrText;
        }

        public object               Tag;

        public ObservableList<TreeViewNode>     Children            { get; private set; }
        public int                              ContainedNodeCount  { get; private set; }
        public int                              UncollapsedContainedNodeCount { get; private set; }

        public bool                 DisplayAsContainer;
        public bool                 Collapsed {
            get { return mbCollapsed; }
            set {
                mbCollapsed = value;
                UpdateLabel();
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
        public TreeViewNode( TreeView _treeView, string _strText, Texture2D _icon = null, object _tag = null )
        : base( _treeView.Screen )
        {
            mTreeView   = _treeView;
            Children    = new ObservableList<TreeViewNode>();

            Children.ListChanged += delegate( object _source, ObservableList<TreeViewNode>.ListChangedEventArgs _args )
            {
                _args.Item.Parent = _args.Added ? this : null;

                if( _args.Added )
                {
                    OnNodeAdded( 1 + _args.Item.ContainedNodeCount );
                }
                else
                {
                    OnNodeRemoved( 1 + _args.Item.ContainedNodeCount );
                }

                UpdateLabel();
                UpdateContentSize();
            };

            Children.ListCleared += delegate( object _source, EventArgs _args )
            {
                ContainedNodeCount = 0;
                UncollapsedContainedNodeCount = 0;
                UpdateLabel();
                UpdateContentSize();
            };

            mstrText = _strText;
            mLabel      = new Label( Screen, _strText, Anchor.Start, Screen.Style.DefaultTextColor );
            mImage      = new Image( Screen );
            mImage.Padding = new Box( 0, 5, 0, 10 );

            Icon = _icon;

            Tag = _tag;

            UpdateContentSize();
        }

        public TreeViewNode( TreeView _treeView, string _strText, object _tag )
        : this( _treeView, _strText, null, _tag )
        {

        }

        //----------------------------------------------------------------------
        internal override void UpdateContentSize()
        {
            ContentHeight = mTreeView.NodeHeight + mTreeView.NodeSpacing;
            if( Children.Count > 0 && ! mbCollapsed )
            {
                foreach( TreeViewNode child in Children )
                {
                    ContentHeight += child.ContentHeight;
                }
            }

            UncollapsedContainedNodeCount = 0;
            if( ! Collapsed )
            {
                foreach( TreeViewNode childNode in Children )
                {
                    UncollapsedContainedNodeCount += 1 + childNode.UncollapsedContainedNodeCount;
                }
            }

            if( Parent is TreeViewNode )
            {
                ((TreeViewNode)Parent).ChildSizeChanged();
            }

            base.UpdateContentSize();
        }

        //----------------------------------------------------------------------
        void ChildSizeChanged()
        {
            UpdateContentSize();
        }

        void OnNodeAdded( int _iAddedNodeCount )
        {
            ContainedNodeCount += _iAddedNodeCount;

            if( Parent is TreeViewNode )
            {
                ((TreeViewNode)Parent).OnNodeAdded( _iAddedNodeCount );
            }
        }

        void OnNodeRemoved( int _iRemovedNodeCount )
        {
            ContainedNodeCount -= _iRemovedNodeCount;

            if( Parent is TreeViewNode )
            {
                ((TreeViewNode)Parent).OnNodeRemoved( _iRemovedNodeCount );
            }
        }


        //----------------------------------------------------------------------
        internal override void DoLayout( Rectangle _rect )
        {
            Position = _rect.Location;
            Size = new Point( _rect.Width, _rect.Height );
            HitBox = _rect;

            int iX = Position.X;
            int iY = Position.Y + mTreeView.NodeHeight + mTreeView.NodeSpacing;
            foreach( TreeViewNode child in Children )
            {
                child.DoLayout( new Rectangle( iX + mTreeView.NodeBranchWidth, iY, Size.X - mTreeView.NodeBranchWidth, child.ContentHeight ) );
                iY += child.ContentHeight;
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
                if( ! mbIsLast )
                {
                    Screen.Game.SpriteBatch.Draw( Screen.Style.TreeViewBranch, new Vector2( Position.X - mTreeView.NodeBranchWidth, Position.Y ), Color.White );
                    Screen.Game.SpriteBatch.Draw( Screen.Style.TreeViewLine, new Rectangle( Position.X - mTreeView.NodeBranchWidth, Position.Y + mTreeView.NodeHeight + mTreeView.NodeSpacing, Screen.Style.TreeViewBranch.Width, ContentHeight - (mTreeView.NodeHeight + mTreeView.NodeSpacing) ), Color.White );
                }
                else
                {
                    Screen.Game.SpriteBatch.Draw( Screen.Style.TreeViewBranchLast, new Vector2( Position.X - mTreeView.NodeBranchWidth, Position.Y ), Color.White );
                }
            }

            Rectangle nodeRect = new Rectangle( Position.X, Position.Y, Size.X, mTreeView.NodeHeight );

            Texture2D frameTex;

            if( ! DisplayAsContainer )
            {
                frameTex = mTreeView.SelectedNode == this ? Screen.Style.GridBoxFrameSelected : Screen.Style.GridBoxFrame;
            }
            else
            {
                frameTex = mTreeView.SelectedNode == this ? Screen.Style.TreeViewContainerFrameSelected : Screen.Style.TreeViewContainerFrame;
            }

            if( frameTex != null )
            {
                Screen.DrawBox( frameTex, nodeRect, Screen.Style.GridBoxFrameCornerSize, Color.White );
            }

            if( Children.Count != 0 || DisplayAsContainer )
            {
                Texture2D tex = Children.Count == 0 ? Screen.Style.TreeViewBranchOpenEmpty : Screen.Style.TreeViewBranchOpen;

                if( Collapsed )
                {
                    tex = Screen.Style.TreeViewBranchClosed;
                }

                Screen.Game.SpriteBatch.Draw( tex, new Vector2( Position.X, Position.Y ), Color.White );

            }

            DrawNode( Position );

            if( mTreeView.HasFocus && mTreeView.FocusedNode == this )
            {
                if( mTreeView.SelectedNode != this )
                {
                    Screen.DrawBox( Screen.Style.GridBoxFrameFocus, nodeRect, Screen.Style.GridBoxFrameCornerSize, Color.White );
                }
                else
                if( Screen.Style.GridBoxFrameSelectedFocus != null )
                {
                    Screen.DrawBox( Screen.Style.GridBoxFrameSelectedFocus, nodeRect, Screen.Style.GridBoxFrameCornerSize, Color.White );
                }
            }

            if( mTreeView.HoveredNode == this )
            {
                if( mTreeView.SelectedNode != this )
                {
                    Screen.DrawBox( Screen.Style.GridBoxFrameHover, nodeRect, Screen.Style.GridBoxFrameCornerSize, Color.White );
                }
                else
                if( Screen.Style.GridBoxFrameSelectedHover != null )
                {
                    Screen.DrawBox( Screen.Style.GridBoxFrameSelectedHover, nodeRect, Screen.Style.GridBoxFrameCornerSize, Color.White );
                }
            }

            if( ! mbCollapsed )
            {
                foreach( TreeViewNode child in Children )
                {
                    child.Draw();
                }
            }
        }

        //----------------------------------------------------------------------
        internal void DrawNode( Point _position )
        {
            Rectangle nodeRect = new Rectangle( _position.X, _position.Y, Size.X, mTreeView.NodeHeight );

            int iLabelX = ( Children.Count > 0 || DisplayAsContainer ) ? mTreeView.NodeBranchWidth : 0;
            if( mImage.Texture != null )
            {
                mImage.DoLayout( new Rectangle( Position.X + iLabelX, Position.Y, mImage.ContentWidth, mTreeView.NodeHeight ) );
                mImage.Draw();
                iLabelX += mImage.ContentWidth;
            }

            mLabel.DoLayout( new Rectangle( _position.X + iLabelX, _position.Y, Size.X - iLabelX, mTreeView.NodeHeight ) );
            mLabel.Draw();
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
        public Action<TreeView>             ValidateHandler;
        public TreeViewNode                 SelectedNode    = null;

        //----------------------------------------------------------------------
        public List<Button>                 ActionButtons       { get; private set; }
        Button                              mHoveredActionButton;
        bool                                mbIsHoveredActionButtonDown;

        //----------------------------------------------------------------------
        public TreeViewNode                 HoveredNode     = null;
        internal TreeViewNode               FocusedNode     = null;

        //----------------------------------------------------------------------
        bool                                mbIsHovered;
        Point                               mHoverPoint;

        int                                 miScrollOffset;
        int                                 miScrollMax;

        //----------------------------------------------------------------------
        // Drag & drop
        public Func<TreeViewNode,TreeViewNode,bool>     DragNDropHandler;
        bool                                mbIsMouseDown;
        bool                                mbIsDragging;
        Point                               mMouseDownPoint;
        Point                               mMouseDragPoint;
        const int                           siDragTriggerDistance   = 10;

        //----------------------------------------------------------------------
        public TreeView( Screen _screen )
        : base( _screen )
        {
            Nodes = new List<TreeViewNode>();
            ActionButtons = new List<Button>();
        }

        //----------------------------------------------------------------------
        internal override void UpdateContentSize()
        {
            ContentWidth    = Padding.Left + Padding.Right;
            ContentHeight   = Padding.Top + Padding.Bottom;

            base.UpdateContentSize();
        }

        //----------------------------------------------------------------------
        internal override void DoLayout( Rectangle _rect )
        {
            Position = _rect.Location;
            Size = new Point( _rect.Width, _rect.Height );
            HitBox = _rect;

            int iX = Position.X + 10;
            int iY = Position.Y + 10;
            int iHeight = 0;
            foreach( TreeViewNode node in Nodes )
            {
                node.DoLayout( new Rectangle( iX, iY + iHeight - miScrollOffset, Size.X - 20, node.ContentHeight ) );
                iHeight += node.ContentHeight;
            }

            if( HoveredNode != null )
            {
                int iButtonX = 0;
                foreach( Button button in ActionButtons.Reverse<Button>() )
                {
                    button.DoLayout( new Rectangle(
                        Position.X + Size.X - 20 - iButtonX - button.ContentWidth,
                        HoveredNode.Position.Y + NodeHeight / 2 - button.ContentHeight / 2,
                        button.ContentWidth, button.ContentHeight )
                    );

                    iButtonX += button.ContentWidth;
                }
            }

            miScrollMax = Math.Max( 0, ( iHeight ) - ( Size.Y - 20 ) );
        }

        //----------------------------------------------------------------------
        internal override void OnMouseEnter( Point _hitPoint )
        {
            mbIsHovered = true;
            mHoverPoint = _hitPoint;
            UpdateHoveredNode();
        }

        internal override void OnMouseMove( Point _hitPoint )
        {
            if( mbIsMouseDown && FocusedNode != null )
            {
                mbIsDragging = DragNDropHandler != null && (
                        Math.Abs( _hitPoint.Y - mMouseDownPoint.Y ) > siDragTriggerDistance
                    ||  Math.Abs( _hitPoint.X - mMouseDownPoint.X ) > siDragTriggerDistance );
                mMouseDragPoint = _hitPoint;

                if( mMouseDragPoint.Y > Position.Y + Size.Y - 20
                ||  mMouseDragPoint.Y < Position.Y + 20 )
                {
                    Screen.AddWidgetToUpdateList( this );
                }
            }

            mHoverPoint = _hitPoint;
            UpdateHoveredNode();
        }

        internal override void OnMouseOut( Point _hitPoint )
        {
            mbIsHovered = false;
            UpdateHoveredNode();
        }

        void UpdateHoveredNode()
        {
            HoveredNode = null;
            if( mbIsHovered )
            {
                int iNodeIndex = ( mHoverPoint.Y - ( Position.Y + 10 ) + miScrollOffset ) / ( NodeHeight + NodeSpacing );

                HoveredNode = FindHoveredNode( Nodes, iNodeIndex, 0 );

                if( ! mbIsDragging && HoveredNode != null )
                {
                    if( mHoveredActionButton != null )
                    {
                        if( mHoveredActionButton.HitTest( mHoverPoint ) != null )
                        {
                            mHoveredActionButton.OnMouseMove( mHoverPoint );
                        }
                        else
                        {
                            mHoveredActionButton.OnMouseOut( mHoverPoint );

                            if( mbIsHoveredActionButtonDown )
                            {
                                mHoveredActionButton.ResetPressState();
                                mbIsHoveredActionButtonDown = false;
                            }

                            mHoveredActionButton = null;
                        }
                    }

                    if( mHoveredActionButton == null )
                    {
                        mbIsHoveredActionButtonDown = false;

                        foreach( Button button in ActionButtons )
                        {
                            if( button.HitTest( mHoverPoint ) != null )
                            {
                                mHoveredActionButton = button;
                                mHoveredActionButton.OnMouseEnter( mHoverPoint );
                                break;
                            }
                        }
                    }
                }
                else
                {
                    if( mbIsHoveredActionButtonDown )
                    {
                        mHoveredActionButton.ResetPressState();
                        mbIsHoveredActionButtonDown = false;
                    }
                    mHoveredActionButton = null;
                }
            }
        }

        TreeViewNode FindHoveredNode( IList<TreeViewNode> _children, int _iNodeIndex, int _iNodeOffset )
        {
            foreach( TreeViewNode node in _children )
            {
                if( _iNodeOffset == _iNodeIndex )
                {
                    return node;
                }
                else
                if( node.Collapsed )
                {
                    _iNodeOffset += 1;
                }
                else
                if( _iNodeOffset + node.UncollapsedContainedNodeCount >= _iNodeIndex )
                {
                    return FindHoveredNode( node.Children, _iNodeIndex, _iNodeOffset + 1 );
                }
                else
                {
                    _iNodeOffset += 1 + node.UncollapsedContainedNodeCount;
                }
            }

            return null;
        }

        //----------------------------------------------------------------------
        internal override void OnMouseDown( Point _hitPoint )
        {
            if( mHoveredActionButton != null )
            {
                mHoveredActionButton.OnActivateDown();
                mbIsHoveredActionButtonDown = true;
            }
            else
            {
                mbIsMouseDown = true;
                mMouseDownPoint = _hitPoint;

                Screen.Focus( this );
                FocusedNode = HoveredNode;
            }
        }

        internal override void OnMouseUp( Point _hitPoint )
        {
            mbIsMouseDown = false;

            if( mHoveredActionButton != null )
            {
                if( mbIsHoveredActionButtonDown )
                {
                    mHoveredActionButton.OnMouseUp( _hitPoint );
                    mbIsHoveredActionButtonDown = false;
                }
            }
            else
            if( mbIsDragging )
            {
                Debug.Assert( FocusedNode != null );

                if( HitBox.Contains( _hitPoint ) && HoveredNode != FocusedNode && DragNDropHandler != null )
                {
                    TreeViewNode ancestorNode = HoveredNode;

                    bool bIsCycle = false;
                    while( ancestorNode != null )
                    {
                        if( ancestorNode == FocusedNode )
                        {
                            bIsCycle = true;
                            break;
                        }

                        ancestorNode = (TreeViewNode)ancestorNode.Parent;
                    }

                    if( ! bIsCycle && DragNDropHandler(FocusedNode, HoveredNode ) )
                    {
                        if( FocusedNode.Parent != null )
                        {
                            ( (TreeViewNode)(FocusedNode.Parent) ).Children.Remove( FocusedNode );
                        }
                        else
                        {
                            Nodes.Remove( FocusedNode );
                        }

                        if( HoveredNode != null )
                        {
                            HoveredNode.Children.Add( FocusedNode );
                        }
                        else
                        {
                            Nodes.Add( FocusedNode );
                        }
                    }
                }
                else
                {
                    // Drag'n'Drop cancelled
                }

                mbIsDragging = false;
            }
            else
            if( HoveredNode != null && FocusedNode == HoveredNode )
            {
                if( ( HoveredNode.DisplayAsContainer || HoveredNode.Children.Count > 0 ) && _hitPoint.X < HoveredNode.Position.X + NodeBranchWidth )
                {
                    SelectedNode = null;
                    HoveredNode.Collapsed = ! HoveredNode.Collapsed;
                }
                else
                {
                    SelectedNode = HoveredNode;
                }
            }
        }

        //----------------------------------------------------------------------
        internal override void OnMouseDoubleClick( Point _hitPoint )
        {
            if( mHoveredActionButton == null && SelectedNode != null && ValidateHandler != null ) ValidateHandler( this );
        }

        //----------------------------------------------------------------------
        internal override void OnMouseWheel( Point _hitPoint, int _iDelta )
        {
            DoScroll( -_iDelta / 120 * 3 * ( NodeHeight + NodeSpacing ) );
        }

        void DoScroll( int _iDelta )
        {
            int iScrollChange = (int)MathHelper.Clamp( _iDelta, -miScrollOffset, Math.Max( 0, miScrollMax - miScrollOffset ) );
            miScrollOffset += iScrollChange;

            if( mbIsDragging )
            {
                mMouseDownPoint.Y -= iScrollChange;
            }

            UpdateHoveredNode();
        }

        const float sfScrollRepeatDelay = 0.3f;
        float mfScrollRepeatTimer = sfScrollRepeatDelay;
        internal override bool Update( float _fElapsedTime )
        {
            if( mbIsDragging )
            {
                if( mfScrollRepeatTimer >= sfScrollRepeatDelay )
                {
                    mfScrollRepeatTimer = 0f;

                    if( mMouseDragPoint.Y > Position.Y + Size.Y - 20 )
                    {
                        DoScroll( NodeHeight + NodeSpacing );
                    }
                    else
                    if( mMouseDragPoint.Y < Position.Y + 20 )
                    {
                        DoScroll( -( NodeHeight + NodeSpacing ) );
                    }
                    else
                    {
                        mfScrollRepeatTimer = sfScrollRepeatDelay;
                        return false;
                    }
                }
                
                mfScrollRepeatTimer += _fElapsedTime;

                return true;
            }

            mfScrollRepeatTimer = sfScrollRepeatDelay;
            return false;
        }

        //----------------------------------------------------------------------
        internal override void Draw()
        {
            Screen.DrawBox( Screen.Style.GridFrame, new Rectangle( Position.X, Position.Y, Size.X, Size.Y ), 30, Color.White );

            Screen.PushScissorRectangle( new Rectangle( Position.X + 10, Position.Y + 10, Size.X - 20, Size.Y - 20 ) );
            foreach( TreeViewNode node in Nodes )
            {
                node.Draw();
            }

            if( HoveredNode != null && ! mbIsDragging )
            {
                foreach( Button button in ActionButtons )
                {
                    button.Draw();
                }
            }

            Screen.PopScissorRectangle();
        }

        //----------------------------------------------------------------------
        internal override void DrawFocused()
        {
            if( mbIsDragging )
            {
                Debug.Assert( FocusedNode != null );

                FocusedNode.DrawNode( new Point(
                    FocusedNode.Position.X + mMouseDragPoint.X - mMouseDownPoint.X,
                    FocusedNode.Position.Y + mMouseDragPoint.Y - mMouseDownPoint.Y  ) );
            }
        }
    }
}
