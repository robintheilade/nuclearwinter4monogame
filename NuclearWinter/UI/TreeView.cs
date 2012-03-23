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

        public CheckBoxStatus       CheckBoxStatus;

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
                    if( _args.Item == mTreeView.SelectedNode )
                    {
                        mTreeView.SelectedNode = null;
                    }

                    if( _args.Item == mTreeView.HoveredNode )
                    {
                        mTreeView.UpdateHoveredNode();
                    }

                    if( _args.Item == mTreeView.FocusedNode )
                    {
                        mTreeView.FocusedNode = null;
                    }

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
            base.DoLayout( _rect );
            HitBox = LayoutRect;

            int iX = LayoutRect.X;
            int iY = LayoutRect.Y + mTreeView.NodeHeight + mTreeView.NodeSpacing;
            foreach( TreeViewNode child in Children )
            {
                child.DoLayout( new Rectangle( iX + mTreeView.NodeBranchWidth, iY, LayoutRect.Width - mTreeView.NodeBranchWidth, child.ContentHeight ) );
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
            if( ! LayoutRect.Intersects( Screen.ScissorRectangle ) ) return;

            if( Parent != null )
            {
                if( ! mbIsLast )
                {
                    Screen.Game.SpriteBatch.Draw( Screen.Style.TreeViewBranch, new Vector2( LayoutRect.X - mTreeView.NodeBranchWidth, LayoutRect.Y ), Color.White );
                    Screen.Game.SpriteBatch.Draw( Screen.Style.TreeViewLine, new Rectangle( LayoutRect.X - mTreeView.NodeBranchWidth, LayoutRect.Y + mTreeView.NodeHeight + mTreeView.NodeSpacing, Screen.Style.TreeViewBranch.Width, ContentHeight - (mTreeView.NodeHeight + mTreeView.NodeSpacing) ), Color.White );
                }
                else
                {
                    Screen.Game.SpriteBatch.Draw( Screen.Style.TreeViewBranchLast, new Vector2( LayoutRect.X - mTreeView.NodeBranchWidth, LayoutRect.Y ), Color.White );
                }
            }

            Rectangle nodeRect = new Rectangle( LayoutRect.X, LayoutRect.Y, LayoutRect.Width, mTreeView.NodeHeight );

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

                Screen.Game.SpriteBatch.Draw( tex, new Vector2( LayoutRect.X, LayoutRect.Y ), Color.White );

            }

            if( mTreeView.HasCheckBoxes )
            {
                Rectangle checkBoxRect = new Rectangle( nodeRect.X + ( ( Children.Count > 0 || DisplayAsContainer ) ? mTreeView.NodeBranchWidth : 0 ), nodeRect.Y, nodeRect.Height, nodeRect.Height );

                Screen.DrawBox( Screen.Style.TreeViewCheckBoxFrame, checkBoxRect, Screen.Style.TreeViewCheckBoxFrameCornerSize, Color.White );

                if( mTreeView.HoveredNode == this && mTreeView.IsHoveringNodeCheckBox() )
                {
                    Screen.DrawBox( Screen.Style.TreeViewCheckBoxFrameHover, checkBoxRect, Screen.Style.GridBoxFrameCornerSize, Color.White );
                }

                Texture2D tex;
                
                switch( CheckBoxStatus )
                {
                    case UI.CheckBoxStatus.Checked:
                        tex = Screen.Style.TreeViewCheckBoxChecked;
                        break;
                    case UI.CheckBoxStatus.Unchecked:
                        tex = Screen.Style.TreeViewCheckBoxUnchecked;
                        break;
                    case UI.CheckBoxStatus.Inconsistent:
                        tex = Screen.Style.TreeViewCheckBoxInconsistent;
                        break;
                    default:
                        throw new NotSupportedException();
                }

                Screen.Game.SpriteBatch.Draw( tex, new Vector2( checkBoxRect.Center.X, checkBoxRect.Center.Y ), null, Color.White, 0f, new Vector2( tex.Width, tex.Height ) / 2f, 1f, SpriteEffects.None, 1f );
            }

            DrawNode( LayoutRect.Location );

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

            if( mTreeView.HoveredNode == this && ! mTreeView.IsHoveringNodeCheckBox() && mTreeView.InsertMode == TreeView.NodeInsertMode.Over )
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
            Rectangle nodeRect = new Rectangle( _position.X, _position.Y, LayoutRect.Width, mTreeView.NodeHeight );

            int iLabelX = 0;
            if( Children.Count > 0 || DisplayAsContainer )
            {
                iLabelX += mTreeView.NodeBranchWidth;
            }
            
            if( mTreeView.HasCheckBoxes )
            {
                iLabelX += mTreeView.NodeHeight + mTreeView.NodeSpacing;
            }

            if( mImage.Texture != null )
            {
                mImage.DoLayout( new Rectangle( LayoutRect.X + iLabelX, LayoutRect.Y, mImage.ContentWidth, mTreeView.NodeHeight ) );
                mImage.Draw();
                iLabelX += mImage.ContentWidth;
            }

            mLabel.DoLayout( new Rectangle( _position.X + iLabelX, _position.Y, LayoutRect.Width - iLabelX, mTreeView.NodeHeight ) );
            mLabel.Draw();
        }
    }

    //--------------------------------------------------------------------------
    public class TreeView: Widget
    {
        //----------------------------------------------------------------------
        public ObservableList<TreeViewNode> Nodes               { get; private set; }

        public int                          NodeHeight      = 40;
        public int                          NodeSpacing     = 0;
        public int                          NodeBranchWidth = 25;

        //----------------------------------------------------------------------
        public Action<TreeView>             ValidateHandler;
        public Action<TreeView>             SelectHandler;

        TreeViewNode                        mSelectedNode = null;
        public TreeViewNode SelectedNode
        {
            get { return mSelectedNode; }
            set {
                mSelectedNode = value;

                if( mSelectedNode != null )
                {
                    // FIXME: Scroll to selected node
                    // Computing the selected node's position at this point is complicated at best
                }
            }
        }

        //----------------------------------------------------------------------
        public List<Button>                 ActionButtons       { get; private set; }
        Button                              mHoveredActionButton;
        bool                                mbIsHoveredActionButtonDown;

        //----------------------------------------------------------------------
        public TreeViewNode                 HoveredNode     = null;

        internal enum NodeInsertMode
        {
            Before,
            Over,
            After
        }

        internal NodeInsertMode             InsertMode;

        internal TreeViewNode               FocusedNode     = null;

        //----------------------------------------------------------------------
        bool                                mbIsHovered;
        Point                               mHoverPoint;

        //----------------------------------------------------------------------
        public Scrollbar                    Scrollbar       { get; private set; }
        const float                         sfScrollRepeatDelay = 0.3f;
        float                               mfScrollRepeatTimer = sfScrollRepeatDelay;

        //----------------------------------------------------------------------
        // Checkboxes
        public bool                         HasCheckBoxes;
        public bool                         CheckBoxCascading;
        public Action<TreeViewNode,CheckBoxStatus>
                                            NodeCheckStatusChangedHandler;

        //----------------------------------------------------------------------
        // Drag & drop
        public Func<TreeViewNode,TreeViewNode,int,bool>
                                            DragNDropHandler;
        bool                                mbIsMouseDown;
        bool                                mbIsDragging;
        Point                               mMouseDownPoint;
        Point                               mMouseDragPoint;
        const int                           siDragTriggerDistance   = 10;

        //----------------------------------------------------------------------
        public TreeView( Screen _screen )
        : base( _screen )
        {
            Nodes = new ObservableList<TreeViewNode>();

            Nodes.ListCleared += delegate {
                SelectedNode = null;
                HoveredNode = null;
                FocusedNode = null;
                mHoveredActionButton = null;
                mbIsHoveredActionButtonDown = false;
            };

            Nodes.ListChanged += delegate( object _source, ObservableList<TreeViewNode>.ListChangedEventArgs _args )
            {
                if( ! _args.Added )
                {
                    if( _args.Item == SelectedNode )
                    {
                        SelectedNode = null;
                    }

                    if( _args.Item == HoveredNode )
                    {
                        UpdateHoveredNode();
                    }

                    if( _args.Item == FocusedNode )
                    {
                        FocusedNode = null;
                        mbIsDragging = false;
                    }
                }
            };

            Scrollbar = new UI.Scrollbar( this );

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
            base.DoLayout( _rect );
            HitBox = new Rectangle( LayoutRect.X + 10, LayoutRect.Y + 10, LayoutRect.Width - 20, LayoutRect.Height - 20 );

            int iX = LayoutRect.X + 10;
            int iY = LayoutRect.Y + 10;
            int iHeight = 0;
            foreach( TreeViewNode node in Nodes )
            {
                node.DoLayout( new Rectangle( iX, iY + iHeight - (int)Scrollbar.LerpOffset, LayoutRect.Width - 20, node.ContentHeight ) );
                iHeight += node.ContentHeight;
            }

            if( HoveredNode != null )
            {
                int iButtonX = 0;
                foreach( Button button in ActionButtons.Reverse<Button>() )
                {
                    button.DoLayout( new Rectangle(
                        LayoutRect.Right - 20 - iButtonX - button.ContentWidth,
                        HoveredNode.LayoutRect.Y + NodeHeight / 2 - button.ContentHeight / 2,
                        button.ContentWidth, button.ContentHeight )
                    );

                    iButtonX += button.ContentWidth;
                }
            }

            ContentHeight = iHeight;
            Scrollbar.DoLayout( LayoutRect, ContentHeight + Padding.Vertical + 30 );
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
            }

            mbIsHovered = HitBox.Contains( _hitPoint );

            mHoverPoint = _hitPoint;
            UpdateHoveredNode();
        }

        internal override void OnMouseOut( Point _hitPoint )
        {
            mbIsHovered = false;
            UpdateHoveredNode();
        }

        internal void UpdateHoveredNode()
        {
            TreeViewNode oldHoveredNode = HoveredNode;
            HoveredNode = null;

            if( mbIsHovered )
            {
                int iNodeY = ( mHoverPoint.Y - ( LayoutRect.Y + 10 ) + (int)Scrollbar.LerpOffset );

                int iOffset = iNodeY % ( NodeHeight + NodeSpacing );

                if( iOffset < ( NodeHeight + NodeSpacing ) / 4 )
                {
                    InsertMode = NodeInsertMode.Before;
                }
                else
                if( iOffset > ( NodeHeight + NodeSpacing ) * 3 / 4 )
                {
                    InsertMode = NodeInsertMode.After;
                }
                else
                {
                    InsertMode = NodeInsertMode.Over;
                }

                int iNodeIndex = iNodeY / ( NodeHeight + NodeSpacing );

                HoveredNode = FindHoveredNode( Nodes, iNodeIndex, 0 );

                if( HoveredNode == null )
                {
                    InsertMode = iNodeY < ( NodeHeight + NodeSpacing ) / 4 ? NodeInsertMode.Before : NodeInsertMode.After;
                }

                if( oldHoveredNode != HoveredNode )
                {
                    mHoveredActionButton = null;
                    mbIsHoveredActionButtonDown = false;
                }

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
        internal override void OnMouseDown( Point _hitPoint, int _iButton )
        {
            if( _iButton != 0 ) return;

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

        internal override void OnMouseUp( Point _hitPoint, int _iButton )
        {
            if( _iButton != 0 ) return;

            mbIsMouseDown = false;

            if( mHoveredActionButton != null )
            {
                if( mbIsHoveredActionButtonDown )
                {
                    mHoveredActionButton.OnMouseUp( _hitPoint, _iButton );
                    mbIsHoveredActionButtonDown = false;
                }
            }
            else
            if( mbIsDragging )
            {
                Debug.Assert( FocusedNode != null );

                TreeViewNode draggedNode = FocusedNode;
                TreeViewNode currentParentNode = (TreeViewNode)draggedNode.Parent;

                int iIndex = ( HoveredNode != null ) ? HoveredNode.Children.Count : Nodes.Count;
                TreeViewNode targetParentNode = HoveredNode;

                if( HoveredNode != null )
                {
                    switch( InsertMode )
                    {
                        case NodeInsertMode.Before:
                            targetParentNode = (TreeViewNode)HoveredNode.Parent;
                            iIndex = ( targetParentNode != null ) ? targetParentNode.Children.IndexOf( HoveredNode ) : Nodes.IndexOf( HoveredNode );
                            break;
                        case NodeInsertMode.After:
                            if( ! HoveredNode.DisplayAsContainer && HoveredNode.Children.Count == 0 )
                            {
                                targetParentNode = (TreeViewNode)HoveredNode.Parent;
                                iIndex = 1 + ( targetParentNode != null ? targetParentNode.Children.IndexOf( HoveredNode ) : Nodes.IndexOf( HoveredNode ) );
                            }
                            else
                            {
                                iIndex = 0;
                            }
                            break;
                    }
                }

                if( HitBox.Contains( _hitPoint ) && targetParentNode != draggedNode && DragNDropHandler != null )
                {
                    TreeViewNode ancestorNode = targetParentNode;

                    bool bIsCycle = false;
                    while( ancestorNode != null )
                    {
                        if( ancestorNode == draggedNode )
                        {
                            bIsCycle = true;
                            break;
                        }

                        ancestorNode = (TreeViewNode)ancestorNode.Parent;
                    }

                    if( ! bIsCycle )
                    {
                        // Offset index if the node is moving inside the same parent
                        if( targetParentNode == currentParentNode )
                        {
                            if( currentParentNode != null )
                            {
                                if( currentParentNode.Children.IndexOf( draggedNode ) < iIndex )
                                {
                                    iIndex--;
                                }
                            }
                            else
                            {
                                if( Nodes.IndexOf( draggedNode ) < iIndex )
                                {
                                    iIndex--;
                                }
                            }
                        }

                        if( DragNDropHandler( draggedNode, targetParentNode, iIndex ) )
                        {

                            if( draggedNode.Parent != null )
                            {
                                ( (TreeViewNode)(draggedNode.Parent) ).Children.Remove( draggedNode );
                            }
                            else
                            {
                                Nodes.Remove( draggedNode );
                            }


                            if( targetParentNode != null )
                            {
                                targetParentNode.Children.Insert( iIndex, draggedNode );
                            }
                            else
                            {
                                Nodes.Insert( iIndex, draggedNode );
                            }

                            FocusedNode = draggedNode;
                        }
                    }
                }
                else
                {
                    // Drag'n'Drop cancelled
                }

                mbIsDragging = false;
                mfScrollRepeatTimer = sfScrollRepeatDelay;
            }
            else
            {
                SelectHoveredNode( true );
            }
        }

        //----------------------------------------------------------------------
        internal override bool OnMouseDoubleClick( Point _hitPoint )
        {
            if( mHoveredActionButton == null && ValidateHandler != null )
            {
                SelectHoveredNode( false );

                if( SelectedNode != null )
                {
                    ValidateHandler( this );
                    return true;
                }
            }

            return false;
        }

        internal bool IsHoveringNodeCheckBox()
        {
            bool bBranch = ( HoveredNode.DisplayAsContainer || HoveredNode.Children.Count > 0 );
            int x = HoveredNode.LayoutRect.X + ( bBranch ? NodeBranchWidth : 0 );
            return HasCheckBoxes && mHoverPoint.X >= x && mHoverPoint.X < x + NodeHeight + NodeSpacing;
        }

        void SelectHoveredNode( bool _bDoCollapse )
        {
            if( HoveredNode != null && FocusedNode == HoveredNode )
            {
                bool bBranch = ( HoveredNode.DisplayAsContainer || HoveredNode.Children.Count > 0 );

                if( HasCheckBoxes && IsHoveringNodeCheckBox() )
                {
                    CheckBoxStatus newStatus = ( HoveredNode.CheckBoxStatus == CheckBoxStatus.Checked ) ? CheckBoxStatus.Unchecked : CheckBoxStatus.Checked;
                    NodeCheckStatusChangedHandler( HoveredNode, newStatus );

                    HoveredNode.CheckBoxStatus = newStatus;
                }
                else
                if( bBranch && mHoverPoint.X < HoveredNode.LayoutRect.X + NodeBranchWidth )
                {
                    if( _bDoCollapse )
                    {
                        HoveredNode.Collapsed = ! HoveredNode.Collapsed;
                    }

                    SelectedNode = null;
                }
                else
                {
                    SelectedNode = HoveredNode;
                }

                if( SelectHandler != null ) SelectHandler( this );
            }
            else
            {
                SelectedNode = null;
                if( SelectHandler != null ) SelectHandler( this );
            }
        }

        //----------------------------------------------------------------------
        internal override void OnMouseWheel( Point _hitPoint, int _iDelta )
        {
            DoScroll( -_iDelta / 120 * 3 * ( NodeHeight + NodeSpacing ) );
        }

        void DoScroll( int _iDelta )
        {
            int iScrollChange = (int)MathHelper.Clamp( _iDelta, -Scrollbar.Offset, Math.Max( 0, Scrollbar.Max - Scrollbar.Offset ) );
            Scrollbar.Offset += iScrollChange;

            if( mbIsDragging )
            {
                mMouseDownPoint.Y -= iScrollChange;
            }

            UpdateHoveredNode();
        }

        internal override void Update( float _fElapsedTime )
        {
            foreach( Button actionButton in ActionButtons )
            {
                actionButton.Update( _fElapsedTime );
            }

            if( mbIsDragging )
            {
                if( mfScrollRepeatTimer >= sfScrollRepeatDelay )
                {
                    mfScrollRepeatTimer = 0f;

                    if( mMouseDragPoint.Y > LayoutRect.Bottom - 20 )
                    {
                        DoScroll( NodeHeight + NodeSpacing );
                    }
                    else
                    if( mMouseDragPoint.Y < LayoutRect.Y + 20 )
                    {
                        DoScroll( -( NodeHeight + NodeSpacing ) );
                    }
                    else
                    {
                        mfScrollRepeatTimer = sfScrollRepeatDelay;
                    }
                }
                
                mfScrollRepeatTimer += _fElapsedTime;
            }

            bool bIsScrolling = Math.Abs( Scrollbar.LerpOffset - Scrollbar.Offset ) > 1f;
            Scrollbar.Update( _fElapsedTime );

            if( bIsScrolling )
            {
                UpdateHoveredNode();
            }
        }

        //----------------------------------------------------------------------
        internal override void Draw()
        {
            Screen.DrawBox( Screen.Style.ListFrame, LayoutRect, 30, Color.White );

            Rectangle paddedRect = new Rectangle( LayoutRect.X + 10, LayoutRect.Y + 10, LayoutRect.Width - 20, LayoutRect.Height - 20 );

            Screen.PushScissorRectangle( paddedRect );
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

            if( mbIsDragging )
            {
                if( HoveredNode != null )
                {
                    if( InsertMode != NodeInsertMode.Over )
                    {
                        int iX = HoveredNode.LayoutRect.X;
                        int iWidth = HoveredNode.LayoutRect.Width;
                        int iY = HoveredNode.LayoutRect.Y - ( NodeSpacing + Screen.Style.ListRowInsertMarker.Height ) / 2 + ( InsertMode == NodeInsertMode.After ? NodeHeight + NodeSpacing : 0 );

                        if( InsertMode == NodeInsertMode.After && ( HoveredNode.DisplayAsContainer || HoveredNode.Children.Count > 0 ) && ! HoveredNode.Collapsed )
                        {
                            iX += NodeBranchWidth;
                            iWidth -= NodeBranchWidth;
                        }

                        Rectangle markerRect = new Rectangle( iX, iY, iWidth, Screen.Style.ListRowInsertMarker.Height );
                        Screen.DrawBox( Screen.Style.ListRowInsertMarker, markerRect, Screen.Style.ListRowInsertMarkerCornerSize, Color.White );
                    }
                }
                else
                if( mbIsHovered )
                {
                    int iX = paddedRect.X;
                    int iWidth = paddedRect.Width;
                    int iY = paddedRect.Y - ( NodeSpacing + Screen.Style.ListRowInsertMarker.Height ) / 2 + ( InsertMode == NodeInsertMode.After ? ( ContentHeight - (int)Scrollbar.LerpOffset ) : 0 );

                    Rectangle markerRect = new Rectangle( iX, iY, iWidth, Screen.Style.ListRowInsertMarker.Height );
                    Screen.DrawBox( Screen.Style.ListRowInsertMarker, markerRect, Screen.Style.ListRowInsertMarkerCornerSize, Color.White );
                }
            }

            Screen.PopScissorRectangle();

            Scrollbar.Draw();
        }

        //----------------------------------------------------------------------
        internal override void DrawHovered()
        {
            if( mHoveredActionButton != null )
            {
                mHoveredActionButton.DrawHovered();
            }
        }

        //----------------------------------------------------------------------
        internal override void DrawFocused()
        {
            if( mbIsDragging )
            {
                Debug.Assert( FocusedNode != null );

                FocusedNode.DrawNode( new Point(
                    FocusedNode.LayoutRect.X + mMouseDragPoint.X - mMouseDownPoint.X,
                    FocusedNode.LayoutRect.Y + mMouseDragPoint.Y - mMouseDownPoint.Y  ) );
            }
        }
    }
}
