using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NuclearWinter.Collections;
using System.Diagnostics;
using Microsoft.Xna.Framework.Input;

#if !MONOGAME
using OSKey = System.Windows.Forms.Keys;
#elif !MONOMAC
using OSKey = OpenTK.Input.Key;
#else
using OSKey = MonoMac.AppKit.NSKey;
#endif

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
                mLabel.Padding = mImage.Texture != null ? new Box( 0, mTreeView.Style.NodeHorizontalPadding, 0, 0 ) : new Box( 0, mTreeView.Style.NodeHorizontalPadding );
            }
        }

        string mstrText = "";
        bool mbIsLabelDirty = false;

        public string               Text
        {
            get { return mstrText; }
            set {
                mstrText = value;
                mbIsLabelDirty = true;
            }
        }

        void UpdateLabel()
        {
            mLabel.Text = Collapsed ? string.Format( "{0} ({1})", mstrText, ContainedNodeCount ) : mstrText;
        }

        public List<TreeViewIndicator>          Indicators { get; private set; }

        public object                           Tag;

        public ObservableList<TreeViewNode>     Children            { get; private set; }
        public int                              ContainedNodeCount  { get; private set; }
        public int                              UncollapsedContainedNodeCount { get; private set; }

        public bool                             DisplayAsContainer;
        public bool                             Collapsed {
            get { return mbCollapsed; }
            set {
                mbCollapsed = value;
                mbIsLabelDirty = true;
                UpdateContentSize();
            }
        }

        public CheckBoxState                   CheckBoxState;

        //----------------------------------------------------------------------
        TreeView                                mTreeView;

        Label                                   mLabel;
        Image                                   mImage;

        bool                                    mbCollapsed;
        bool                                    mbIsLast;

        int                                     miIndicatorAndActionButtonsWidth;

        //----------------------------------------------------------------------
        public TreeViewNode( TreeView _treeView, string _strText, Texture2D _icon = null, object _tag = null )
        : base( _treeView.Screen )
        {
            mTreeView   = _treeView;
            Indicators = new List<TreeViewIndicator>();

            Children    = new ObservableList<TreeViewNode>();
            Children.ListChanged += OnChildrenListChanged;
            Children.ListCleared += OnChildrenListCleared;

            mstrText = _strText;
            mLabel      = new Label( Screen, _strText, Anchor.Start, Screen.Style.DefaultTextColor );
            mImage      = new Image( Screen );
            mImage.Padding = new Box( 0, mTreeView.Style.NodeHorizontalPadding / 2, 0, mTreeView.Style.NodeHorizontalPadding );

            Icon = _icon;
            Tag = _tag;

            UpdateContentSize();
        }

        public TreeViewNode( TreeView _treeView, string _strText, object _tag )
        : this( _treeView, _strText, null, _tag )
        {

        }

        //----------------------------------------------------------------------
        public void ReplaceChildren( List<TreeViewNode> _lChildren )
        {
            // Clear previous nodes
            foreach( TreeViewNode node in Children )
            {
                node.Parent = null;
            }

            Children = new ObservableList<TreeViewNode>( _lChildren );
            Children.ListChanged += OnChildrenListChanged;
            Children.ListCleared += OnChildrenListCleared;

            if( Parent is TreeViewNode )
            {
                ((TreeViewNode)Parent).OnNodeRemoved( ContainedNodeCount );
            }

            ContainedNodeCount = Children.Count;
            foreach( TreeViewNode node in Children )
            {
                node.Parent = this;
                ContainedNodeCount += node.ContainedNodeCount;
            }

            if( Parent is TreeViewNode )
            {
                ((TreeViewNode)Parent).OnNodeAdded( ContainedNodeCount );
            }

            mbIsLabelDirty = true;
            UpdateContentSize();
        }

        //----------------------------------------------------------------------
        void OnChildrenListChanged( object _source, ObservableList<TreeViewNode>.ListChangedEventArgs _args )
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

            mbIsLabelDirty = true;

            if( mTreeView.LayoutSuspended ) return;

            UpdateContentSize();
        }

        //----------------------------------------------------------------------
        void OnChildrenListCleared( object _source, EventArgs _args )
        {
            ContainedNodeCount = 0;
            UncollapsedContainedNodeCount = 0;

            mbIsLabelDirty = true;
            UpdateContentSize();
        }

        //----------------------------------------------------------------------
        protected internal override void UpdateContentSize()
        {
            ComputeContentSize();
            base.UpdateContentSize();
        }

        void ComputeContentSize()
        {
            ContentHeight = mTreeView.Style.NodeHeight + mTreeView.Style.NodeSpacing;
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
        }

        //----------------------------------------------------------------------
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
        public override void DoLayout( Rectangle _rect )
        {
            base.DoLayout( _rect );
            HitBox = LayoutRect;

            miIndicatorAndActionButtonsWidth = 0;

            if( mbIsLabelDirty )
            {
                UpdateLabel();
                mbIsLabelDirty = false;
            }

            if( mTreeView.HoveredNode == this && ! mTreeView.IsDragging )
            {
                miIndicatorAndActionButtonsWidth += mTreeView.ActionButtons.Sum( delegate( Button _button ) { return _button.ContentWidth; } );
            }

            // Indicators
            foreach( TreeViewIndicator indicator in Indicators )
            {
                miIndicatorAndActionButtonsWidth += indicator.ContentWidth + mTreeView.Style.NodeHorizontalPadding;
                indicator.DoLayout( new Rectangle( LayoutRect.Right - miIndicatorAndActionButtonsWidth, LayoutRect.Y + mTreeView.Style.IndicatorVerticalPadding, indicator.ContentWidth, mTreeView.Style.NodeHeight - mTreeView.Style.IndicatorVerticalPadding * 2 ) );
            }

            // Child nodes
            int iX = LayoutRect.X;
            int iY = LayoutRect.Y + mTreeView.Style.NodeHeight + mTreeView.Style.NodeSpacing;
            foreach( TreeViewNode child in Children )
            {
                child.DoLayout( new Rectangle( iX + mTreeView.Style.NodeBranchWidth, iY, LayoutRect.Width - mTreeView.Style.NodeBranchWidth, child.ContentHeight ) );
                iY += child.ContentHeight;
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
            if( ! LayoutRect.Intersects( Screen.ScissorRectangle ) ) return;

            if( Parent != null )
            {
                if( ! mbIsLast )
                {
                    Screen.Game.SpriteBatch.Draw( mTreeView.Style.NodeBranch, new Vector2( LayoutRect.X - mTreeView.Style.NodeBranchWidth, LayoutRect.Y ), Color.White );
                    Screen.Game.SpriteBatch.Draw( mTreeView.Style.NodeBranchLine, new Rectangle( LayoutRect.X - mTreeView.Style.NodeBranchWidth, LayoutRect.Y + mTreeView.Style.NodeHeight + mTreeView.Style.NodeSpacing, mTreeView.Style.NodeBranch.Width, ContentHeight - (mTreeView.Style.NodeHeight + mTreeView.Style.NodeSpacing) ), Color.White );
                }
                else
                {
                    Screen.Game.SpriteBatch.Draw( mTreeView.Style.NodeBranchLast, new Vector2( LayoutRect.X - mTreeView.Style.NodeBranchWidth, LayoutRect.Y ), Color.White );
                }
            }

            Rectangle nodeRect = new Rectangle( LayoutRect.X, LayoutRect.Y, LayoutRect.Width, mTreeView.Style.NodeHeight );

            Texture2D frameTex;

            if( ! DisplayAsContainer )
            {
                frameTex = mTreeView.SelectedNode == this ? mTreeView.Style.SelectedNodeFrame : mTreeView.Style.NodeFrame;
            }
            else
            {
                frameTex = mTreeView.SelectedNode == this ? mTreeView.Style.SelectedContainerNodeFrame : mTreeView.Style.ContainerNodeFrame;
            }

            if( frameTex != null )
            {
                Screen.DrawBox( frameTex, nodeRect, mTreeView.Style.NodeFrameCornerSize, Color.White );
            }

            if( Children.Count != 0 || DisplayAsContainer )
            {
                Texture2D tex = Children.Count == 0 ? mTreeView.Style.NodeBranchOpenEmpty : mTreeView.Style.NodeBranchOpen;

                if( Collapsed )
                {
                    tex = mTreeView.Style.NodeBranchClosed;
                }

                Screen.Game.SpriteBatch.Draw( tex, new Vector2( LayoutRect.X, LayoutRect.Y ), Color.White );

            }

            if( mTreeView.HasCheckBoxes )
            {
                Rectangle checkBoxRect = new Rectangle(
                    nodeRect.X + Screen.Style.CheckBoxPadding.Left + ( ( Children.Count > 0 || DisplayAsContainer ) ? mTreeView.Style.NodeBranchWidth : 0 ),
                    nodeRect.Y + Screen.Style.CheckBoxPadding.Top,
                    nodeRect.Height - Screen.Style.CheckBoxPadding.Horizontal,
                    nodeRect.Height - Screen.Style.CheckBoxPadding.Vertical );

                Screen.DrawBox( Screen.Style.CheckBoxFrame, checkBoxRect, Screen.Style.CheckBoxFrameCornerSize, Color.White );

                if( mTreeView.HoveredNode == this && mTreeView.IsHoveringNodeCheckBox() && ! mTreeView.IsDragging )
                {
                    Screen.DrawBox( Screen.Style.CheckBoxFrameHover, checkBoxRect, Screen.Style.CheckBoxFrameCornerSize, Color.White );
                }

                Texture2D tex;
                
                switch( CheckBoxState )
                {
                    case UI.CheckBoxState.Checked:
                        tex = Screen.Style.CheckBoxChecked;
                        break;
                    case UI.CheckBoxState.Unchecked:
                        tex = Screen.Style.CheckBoxUnchecked;
                        break;
                    case UI.CheckBoxState.Inconsistent:
                        tex = Screen.Style.CheckBoxInconsistent;
                        break;
                    default:
                        throw new NotSupportedException();
                }

                Screen.Game.SpriteBatch.Draw( tex, new Vector2( checkBoxRect.Center.X, checkBoxRect.Center.Y ), null, Color.White, 0f, new Vector2( tex.Width, tex.Height ) / 2f, 1f, SpriteEffects.None, 1f );
            }

            DrawNode( LayoutRect.Location, miIndicatorAndActionButtonsWidth );

            foreach( TreeViewIndicator indicator in Indicators )
            {
                indicator.Draw();
            }

            if( mTreeView.HasFocus && mTreeView.FocusedNode == this )
            {
                if( mTreeView.SelectedNode != this )
                {
                    Screen.DrawBox( mTreeView.Style.NodeFocusOverlay, nodeRect, mTreeView.Style.NodeFrameCornerSize, Color.White );
                }
                else
                if( mTreeView.Style.SelectedNodeFocusOverlay != null )
                {
                    Screen.DrawBox( mTreeView.Style.SelectedNodeFocusOverlay, nodeRect, mTreeView.Style.NodeFrameCornerSize, Color.White );
                }
            }

            if( mTreeView.HoveredNode == this && ! mTreeView.IsHoveringNodeCheckBox() && ( ! mTreeView.IsDragging || mTreeView.InsertMode == TreeView.NodeInsertMode.Over ) )
            {
                if( mTreeView.SelectedNode != this )
                {
                    Screen.DrawBox( mTreeView.Style.NodeHoverOverlay, nodeRect, mTreeView.Style.NodeFrameCornerSize, Color.White );
                }
                else
                if( mTreeView.Style.SelectedNodeHoverOverlay != null )
                {
                    Screen.DrawBox( mTreeView.Style.SelectedNodeHoverOverlay, nodeRect, mTreeView.Style.NodeFrameCornerSize, Color.White );
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
        internal void DrawNode( Point _position, int _iRight )
        {
            int iLabelX = 0;
            if( Children.Count > 0 || DisplayAsContainer )
            {
                iLabelX += mTreeView.Style.NodeBranchWidth;
            }
            
            if( mTreeView.HasCheckBoxes )
            {
                iLabelX += mTreeView.Style.NodeHeight + mTreeView.Style.NodeSpacing;
            }

            if( mImage.Texture != null )
            {
                mImage.DoLayout( new Rectangle( _position.X + iLabelX, _position.Y, mImage.ContentWidth, mTreeView.Style.NodeHeight ) );
                mImage.Draw();
                iLabelX += mImage.ContentWidth;
            }

            mLabel.DoLayout( new Rectangle( _position.X + iLabelX, _position.Y, LayoutRect.Width - iLabelX - _iRight, mTreeView.Style.NodeHeight ) );
            mLabel.Draw();
        }

        internal void ResumeLayout()
        {
            foreach( var node in Children )
            {
                node.ResumeLayout();
            }

            ComputeContentSize();
        }
    }

    //--------------------------------------------------------------------------
    public class TreeViewIndicator: Widget
    {
        public Texture2D            Frame;
        public int                  FrameCornerSize;

        public object               Tag;

        public Texture2D            Icon
        {
            get { return mImage.Texture; }
            set {
                mImage.Texture = value;
                UpdatePaddings();
            }
        }

        public string Text
        {
            get { return mLabel.Text; }
            set {
                mLabel.Text = value;
                UpdatePaddings();
            }
        }

        Label                       mLabel;
        Image                       mImage;

        void UpdatePaddings()
        {
            mImage.Padding = mLabel.Text != "" ? new Box( 0, 0, 0, 10 ) : new Box(0);
            mLabel.Padding = mImage.Texture != null ? new Box( 5, 5, 5, 0 ) : new Box( 5 );

            UpdateContentSize();
        }

        //----------------------------------------------------------------------
        public TreeViewIndicator( Screen _screen, string _strText, Texture2D _iconTex=null, object _tag=null )
        : base( _screen )
        {
            mLabel = new Label( Screen, _strText );
            mLabel.Font = Screen.Style.SmallFont;
            mLabel.Parent = this;

            mImage = new Image( _screen, _iconTex );

            UpdatePaddings();

            Tag = _tag;

            UpdateContentSize();
        }

        //----------------------------------------------------------------------
        protected internal override void UpdateContentSize()
        {
            ContentWidth = ( mImage.Texture != null ? mImage.ContentWidth : 0 ) + ( mLabel.Text != "" ? mLabel.ContentWidth : 0 );

            // Not calling base.UpdateContentSize();
        }

        public override void DoLayout( Rectangle _rect )
        {
            base.DoLayout( _rect );

            int iLabelX = 0;
            if( mImage.Texture != null )
            {
                mImage.DoLayout( new Rectangle( _rect.X, _rect.Y, mImage.ContentWidth, _rect.Height ) );
                iLabelX += mImage.ContentWidth;
            }

            mLabel.DoLayout( new Rectangle( _rect.X + iLabelX, _rect.Y, _rect.Width - iLabelX, _rect.Height ) );
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            if( Frame != null )
            {
                Screen.DrawBox( Frame, LayoutRect, FrameCornerSize, Color.White );
            }

            if( mImage.Texture != null )
            {
                mImage.Draw();
            }

            mLabel.Draw();
        }
    }

    //--------------------------------------------------------------------------
    public class TreeView: Widget
    {
        //----------------------------------------------------------------------
        public struct TreeViewStyle
        {
            public TreeViewStyle(
                int _iNodeHeight, int _iNodeSpacing=0, int _iNodeHorizontalPadding=10, int _iIndicatorVerticalPadding=10,
                int _iTreeViewFrameCornerSize=10,
                int _iNodeFrameCornerSize=10,
                int _iNodeBranchWidth=25,

                Texture2D _treeViewFrame=null,

                Texture2D _nodeFrame=null,
                Texture2D _nodeHoverOverlay=null,
                Texture2D _nodeFocusOverlay=null,
                Texture2D _selectedNodeFrame=null,
                Texture2D _selectedNodeFocusOverlay=null,
                Texture2D _selectedNodeHoverOverlay=null,
                Texture2D _containerNodeFrame=null,
                Texture2D _selectedContainerNodeFrame=null,
                
                Texture2D _nodeBranchClosed=null,
                Texture2D _nodeBranchOpen=null,
                Texture2D _nodeBranchOpenEmpty=null,
                Texture2D _nodeBranch=null,
                Texture2D _nodeBranchLast=null,
                Texture2D _nodeBranchLine=null
                )
            {
                TreeViewFrameCornerSize = _iTreeViewFrameCornerSize;
                TreeViewFrame = _treeViewFrame;

                NodeHeight = _iNodeHeight;
                NodeSpacing = _iNodeSpacing;
                NodeHorizontalPadding = _iNodeHorizontalPadding;
                IndicatorVerticalPadding = _iIndicatorVerticalPadding;
                NodeBranchWidth = _iNodeBranchWidth;

                NodeFrameCornerSize         = _iNodeFrameCornerSize;

                NodeFrame                   = _nodeFrame;
                NodeHoverOverlay            = _nodeHoverOverlay;
                NodeFocusOverlay            = _nodeFocusOverlay;
                SelectedNodeFrame           = _selectedNodeFrame;
                SelectedNodeHoverOverlay    = _selectedNodeHoverOverlay;
                SelectedNodeFocusOverlay    = _selectedNodeFocusOverlay;

                ContainerNodeFrame          = _containerNodeFrame;
                SelectedContainerNodeFrame  = _selectedContainerNodeFrame;

                NodeBranchClosed            = _nodeBranchClosed;
                NodeBranchOpen              = _nodeBranchOpen;
                NodeBranchOpenEmpty         = _nodeBranchOpenEmpty;
                NodeBranch                  = _nodeBranch;
                NodeBranchLast              = _nodeBranchLast;
                NodeBranchLine              = _nodeBranchLine;
            }

            public int              TreeViewFrameCornerSize;
            public Texture2D        TreeViewFrame;

            public int              NodeHeight;
            public int              NodeSpacing;
            public int              NodeHorizontalPadding;
            public int              IndicatorVerticalPadding;

            public int              NodeFrameCornerSize;
            public Texture2D        NodeFrame;
            public Texture2D        NodeHoverOverlay;
            public Texture2D        NodeFocusOverlay;
            public Texture2D        SelectedNodeFrame;
            public Texture2D        SelectedNodeFocusOverlay;
            public Texture2D        SelectedNodeHoverOverlay;

            public Texture2D        ContainerNodeFrame;
            public Texture2D        SelectedContainerNodeFrame;

            public int              NodeBranchWidth;
            public Texture2D        NodeBranchClosed;
            public Texture2D        NodeBranchOpen;
            public Texture2D        NodeBranchOpenEmpty;
            public Texture2D        NodeBranch;
            public Texture2D        NodeBranchLast;
            public Texture2D        NodeBranchLine;
        }

        public TreeViewStyle                Style;

        //----------------------------------------------------------------------
        public ObservableList<TreeViewNode> Nodes               { get; private set; }

        internal bool                       LayoutSuspended     { get; private set; }
        public void SuspendLayout()
        {
            LayoutSuspended = true;
        }

        public void ResumeLayout()
        {
            LayoutSuspended = false;

            foreach( var node in Nodes )
            {
                node.ResumeLayout();
            }
        }

        //----------------------------------------------------------------------
        public Action<TreeView>             ValidateHandler;
        public Action<TreeView>             SelectHandler;
        public Action<TreeView>             HoverHandler;
        public Action<TreeView>             RemoveHandler;

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
        public readonly ObservableList<Button>  ActionButtons;
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
        public Func<TreeViewNode,CheckBoxState,bool>
                                            NodeCheckStateChangedHandler;

        //----------------------------------------------------------------------
        // Drag & drop
        public Func<TreeViewNode,TreeViewNode,int,bool>
                                            DragNDropHandler;
        bool                                mbIsMouseDown;
        internal bool                       IsDragging { get; private set; }
        Point                               mMouseDownPoint;
        Point                               mMouseDragPoint;

        //----------------------------------------------------------------------
        public TreeView( Screen _screen )
        : base( _screen )
        {
            Style = Screen.Style.TreeViewStyle;
            Padding = Screen.Style.TreeViewPadding;

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
                        IsDragging = false;
                    }
                }
            };

            Scrollbar = new UI.Scrollbar( _screen );
            Scrollbar.Parent = this;

            ActionButtons = new ObservableList<Button>();

            ActionButtons.ListCleared += delegate {
                mHoveredActionButton = null;
            };

            ActionButtons.ListChanged += delegate {
                if( mHoveredActionButton != null && ! ActionButtons.Contains( mHoveredActionButton ) )
                {
                    mHoveredActionButton = null;
                }
            };
        }

        //----------------------------------------------------------------------
        protected internal override void UpdateContentSize()
        {
            ContentWidth    = Padding.Horizontal;
            ContentHeight   = Padding.Vertical;

            base.UpdateContentSize();
        }
        
        //----------------------------------------------------------------------
        public override void DoLayout( Rectangle _rect )
        {
            base.DoLayout( _rect );
            HitBox = new Rectangle( LayoutRect.X + Padding.Left, LayoutRect.Y + Padding.Top, LayoutRect.Width - Padding.Horizontal, LayoutRect.Height - Padding.Vertical );

            int iX = LayoutRect.X + Padding.Left;
            int iY = LayoutRect.Y + Padding.Top;
            int iHeight = 0;
            foreach( TreeViewNode node in Nodes )
            {
                node.DoLayout( new Rectangle( iX, iY + iHeight - (int)Scrollbar.LerpOffset, LayoutRect.Width - Padding.Horizontal, node.ContentHeight ) );
                iHeight += node.ContentHeight;
            }

            if( HoveredNode != null && ! IsDragging )
            {
                int iButtonX = 0;
                foreach( Button button in ActionButtons.Reverse<Button>() )
                {
                    button.DoLayout( new Rectangle(
                        LayoutRect.Right - Padding.Right - Style.NodeHorizontalPadding - iButtonX - button.ContentWidth,
                        HoveredNode.LayoutRect.Y + Style.NodeHeight / 2 - button.ContentHeight / 2,
                        button.ContentWidth, button.ContentHeight )
                    );

                    iButtonX += button.ContentWidth;
                }
            }

            ContentHeight = Padding.Vertical + iHeight - Style.NodeSpacing;
            Scrollbar.DoLayout( LayoutRect, ContentHeight );
        }

        //----------------------------------------------------------------------
        public override Widget HitTest( Point _point )
        {
            return Scrollbar.HitTest( _point ) ?? base.HitTest( _point );
        }

        public override void OnMouseEnter( Point _hitPoint )
        {
            mbIsHovered = true;
            mHoverPoint = _hitPoint;
            UpdateHoveredNode();
        }

        public override void OnMouseMove( Point _hitPoint )
        {
            if( mbIsMouseDown && FocusedNode != null )
            {
                IsDragging = DragNDropHandler != null && (
                        Math.Abs( _hitPoint.Y - ( mMouseDownPoint.Y + (int)Scrollbar.LerpOffset ) ) > MouseDragTriggerDistance
                    ||  Math.Abs( _hitPoint.X - mMouseDownPoint.X ) > MouseDragTriggerDistance );
                mMouseDragPoint = _hitPoint;
            }

            mbIsHovered = HitBox.Contains( _hitPoint );

            mHoverPoint = _hitPoint;
            UpdateHoveredNode();
        }

        public override void OnMouseOut( Point _hitPoint )
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
                int iNodeY = ( mHoverPoint.Y - ( LayoutRect.Y + Padding.Top ) + (int)Scrollbar.LerpOffset );

                int iOffset = iNodeY % ( Style.NodeHeight + Style.NodeSpacing );

                if( iOffset < ( Style.NodeHeight + Style.NodeSpacing ) / 4 )
                {
                    InsertMode = NodeInsertMode.Before;
                }
                else
                if( iOffset > ( Style.NodeHeight + Style.NodeSpacing ) * 3 / 4 )
                {
                    InsertMode = NodeInsertMode.After;
                }
                else
                {
                    InsertMode = NodeInsertMode.Over;
                }

                int iNodeIndex = iNodeY / ( Style.NodeHeight + Style.NodeSpacing );

                HoveredNode = FindHoveredNode( Nodes, iNodeIndex, 0 );

                if( HoveredNode == null )
                {
                    InsertMode = iNodeY < ( Style.NodeHeight + Style.NodeSpacing ) / 4 ? NodeInsertMode.Before : NodeInsertMode.After;
                }
                else
                if( HoverHandler != null && oldHoveredNode != HoveredNode )
                {
                    HoverHandler( this );
                }

                if( oldHoveredNode != HoveredNode )
                {
                    if( mHoveredActionButton != null )
                    {
                        if( mbIsHoveredActionButtonDown )
                        {
                            mHoveredActionButton.ResetPressState();
                            mbIsHoveredActionButtonDown = false;
                        }
                        mHoveredActionButton = null;
                    }
                }

                if( ! IsDragging && HoveredNode != null )
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
            else
            if( mHoveredActionButton != null )
            {
                if( mbIsHoveredActionButtonDown )
                {
                    mHoveredActionButton.ResetPressState();
                    mbIsHoveredActionButtonDown = false;
                }
                mHoveredActionButton = null;
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
        protected internal override bool OnMouseDown( Point _hitPoint, int _iButton )
        {
            if( _iButton != Screen.Game.InputMgr.PrimaryMouseButton ) return false;

            if( mHoveredActionButton != null )
            {
                mHoveredActionButton.OnActivateDown();
                mbIsHoveredActionButtonDown = true;
            }
            else
            {
                mbIsMouseDown = true;
                mMouseDownPoint = new Point( _hitPoint.X, _hitPoint.Y + (int)Scrollbar.LerpOffset );

                Screen.Focus( this );
                FocusedNode = HoveredNode;
            }

            return true;
        }

        protected internal override void OnMouseUp( Point _hitPoint, int _iButton )
        {
            if( _iButton != Screen.Game.InputMgr.PrimaryMouseButton ) return;

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
            if( IsDragging )
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

                IsDragging = false;
                mfScrollRepeatTimer = sfScrollRepeatDelay;
            }
            else
            {
                SelectHoveredNode( true );
            }
        }

        //----------------------------------------------------------------------
        protected internal override bool OnMouseDoubleClick( Point _hitPoint )
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

        //----------------------------------------------------------------------
        protected internal override void OnOSKeyPress( OSKey _key )
        {
            switch( _key )
            {
                case OSKey.Home:
                    Scrollbar.Offset = 0;
                    break;
                case OSKey.End:
                    Scrollbar.Offset = Scrollbar.Max;
                    break;
                case OSKey.PageUp:
                    Scrollbar.Offset -= LayoutRect.Height;
                    break;
                case OSKey.PageDown:
                    Scrollbar.Offset += LayoutRect.Height;
                    break;
#if !MONOMAC
                case OSKey.Enter:
#else
                case OSKey.Return:
#endif
                    if( ValidateHandler != null && SelectedNode != null )
                    {
                        ValidateHandler( this );
                    }
                    break;
                case OSKey.Delete:
                    if( RemoveHandler != null )
                    {
                        RemoveHandler(this);
                    }
                    break;
            }
        }

        //----------------------------------------------------------------------
        internal bool IsHoveringNodeCheckBox()
        {
            bool bBranch = ( HoveredNode.DisplayAsContainer || HoveredNode.Children.Count > 0 );
            int x = HoveredNode.LayoutRect.X + ( bBranch ? Style.NodeBranchWidth : 0 );
            return HasCheckBoxes && mHoverPoint.X >= x && mHoverPoint.X < x + Style.NodeHeight + Style.NodeSpacing;
        }

        void SelectHoveredNode( bool _bDoCollapse )
        {
            if( HoveredNode != null && FocusedNode == HoveredNode )
            {
                bool bBranch = ( HoveredNode.DisplayAsContainer || HoveredNode.Children.Count > 0 );

                if( HasCheckBoxes && IsHoveringNodeCheckBox() )
                {
                    CheckBoxState newState = ( HoveredNode.CheckBoxState == CheckBoxState.Checked ) ? CheckBoxState.Unchecked : CheckBoxState.Checked;
                    if( NodeCheckStateChangedHandler == null || NodeCheckStateChangedHandler( HoveredNode, newState ) )
                    {
                        HoveredNode.CheckBoxState = newState;
                    }
                }
                else
                {
                    if( bBranch && mHoverPoint.X < HoveredNode.LayoutRect.X + Style.NodeBranchWidth )
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
            }
            else
            {
                SelectedNode = null;
                if( SelectHandler != null ) SelectHandler( this );
            }
        }

        //----------------------------------------------------------------------
        protected internal override void OnMouseWheel( Point _hitPoint, int _iDelta )
        {
            DoScroll( -_iDelta / 120 * 3 * ( Style.NodeHeight + Style.NodeSpacing ) );
        }

        //----------------------------------------------------------------------
        void DoScroll( int _iDelta )
        {
            int iScrollChange = (int)MathHelper.Clamp( _iDelta, -Scrollbar.Offset, Math.Max( 0, Scrollbar.Max - Scrollbar.Offset ) );
            Scrollbar.Offset += iScrollChange;

            UpdateHoveredNode();
        }

        //----------------------------------------------------------------------
        public override void Update( float _fElapsedTime )
        {
            foreach( Button actionButton in ActionButtons )
            {
                actionButton.Update( _fElapsedTime );
            }

            if( IsDragging )
            {
                if( mfScrollRepeatTimer >= sfScrollRepeatDelay )
                {
                    mfScrollRepeatTimer = 0f;

                    if( mMouseDragPoint.Y > LayoutRect.Bottom - 20 )
                    {
                        DoScroll( Style.NodeHeight + Style.NodeSpacing );
                    }
                    else
                    if( mMouseDragPoint.Y < LayoutRect.Y + 20 )
                    {
                        DoScroll( -( Style.NodeHeight + Style.NodeSpacing ) );
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
        protected internal override void OnPadMove( Direction _direction )
        {
            ObservableList<TreeViewNode> lNodes = null;
            TreeViewNode parentNode = null;
            int iIndex = -1;

            if( FocusedNode != null )
            {
                parentNode = ( FocusedNode.Parent != null ) ? ( (TreeViewNode)FocusedNode.Parent ) : null;
                lNodes = ( parentNode != null ) ? parentNode.Children : Nodes;
                iIndex = lNodes.IndexOf( FocusedNode );
            }

            if( _direction == Direction.Up )
            {
                if( lNodes == null ) return;

                if( iIndex > 0 )
                {
                    TreeViewNode node = lNodes[ iIndex - 1 ];

                    while( node.Children.Count > 0 && ! node.Collapsed )
                    {
                        node = node.Children[ node.Children.Count - 1 ];
                    }

                    FocusedNode = node;
                }
                else
                if( parentNode != null )
                {
                    FocusedNode = parentNode;
                }
            }
            else
            if( _direction == Direction.Down )
            {
                if( lNodes == null ) return;

                if( FocusedNode.Children.Count > 0 && ! FocusedNode.Collapsed )
                {
                    FocusedNode = FocusedNode.Children[0];
                }
                else
                {
                    TreeViewNode node = FocusedNode;

                    while( true )
                    {
                        if( iIndex < lNodes.Count - 1 )
                        {
                            FocusedNode = lNodes[ iIndex + 1 ];
                            break;
                        }
                        else
                        if( parentNode != null )
                        {
                            node = parentNode;
                            parentNode = ( node.Parent != null ) ? ( (TreeViewNode)node.Parent ) : null;
                            lNodes = ( parentNode != null ) ? parentNode.Children : Nodes;
                            iIndex = lNodes.IndexOf( node );
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            else
            {
                base.OnPadMove( _direction );
            }
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            Screen.DrawBox( Style.TreeViewFrame, LayoutRect, Style.TreeViewFrameCornerSize, Color.White );

            Rectangle paddedRect = new Rectangle( LayoutRect.X + Padding.Left, LayoutRect.Y + Padding.Top, LayoutRect.Width - Padding.Horizontal, LayoutRect.Height - Padding.Vertical );

            Screen.PushScissorRectangle( paddedRect );
            foreach( TreeViewNode node in Nodes )
            {
                node.Draw();
            }

            if( HoveredNode != null && ! IsDragging )
            {
                foreach( Button button in ActionButtons )
                {
                    button.Draw();
                }
            }

            if( IsDragging )
            {
                if( HoveredNode != null )
                {
                    if( InsertMode != NodeInsertMode.Over )
                    {
                        int iX = HoveredNode.LayoutRect.X;
                        int iWidth = HoveredNode.LayoutRect.Width;
                        int iY = HoveredNode.LayoutRect.Y - ( Style.NodeSpacing + Screen.Style.ListRowInsertMarker.Height ) / 2 + ( InsertMode == NodeInsertMode.After ? Style.NodeHeight + Style.NodeSpacing : 0 );

                        if( InsertMode == NodeInsertMode.After && ( HoveredNode.DisplayAsContainer || HoveredNode.Children.Count > 0 ) && ! HoveredNode.Collapsed )
                        {
                            iX += Style.NodeBranchWidth;
                            iWidth -= Style.NodeBranchWidth;
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
                    int iY = paddedRect.Y - ( Style.NodeSpacing + Screen.Style.ListRowInsertMarker.Height ) / 2 + ( InsertMode == NodeInsertMode.After ? ( ContentHeight - (int)Scrollbar.LerpOffset ) : 0 );

                    Rectangle markerRect = new Rectangle( iX, iY, iWidth, Screen.Style.ListRowInsertMarker.Height );
                    Screen.DrawBox( Screen.Style.ListRowInsertMarker, markerRect, Screen.Style.ListRowInsertMarkerCornerSize, Color.White );
                }
            }

            Screen.PopScissorRectangle();

            Scrollbar.Draw();
        }

        //----------------------------------------------------------------------
        protected internal override void DrawHovered()
        {
            if( mHoveredActionButton != null )
            {
                mHoveredActionButton.DrawHovered();
            }
        }

        //----------------------------------------------------------------------
        protected internal override void DrawFocused()
        {
            if( IsDragging )
            {
                Debug.Assert( FocusedNode != null );

                FocusedNode.DrawNode( new Point(
                    FocusedNode.LayoutRect.X + mMouseDragPoint.X - mMouseDownPoint.X,
                    FocusedNode.LayoutRect.Y + mMouseDragPoint.Y - mMouseDownPoint.Y + (int)Scrollbar.LerpOffset ), 0 );
            }
        }
    }
}
