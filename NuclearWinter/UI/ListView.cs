using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using NuclearWinter.Animation;
using Microsoft.Xna.Framework.Input;

namespace NuclearWinter.UI
{
    //--------------------------------------------------------------------------
    // A ListViewColumn defines a column in a ListView
    public class ListViewColumn
    {
        //----------------------------------------------------------------------
        public Label        Label { get; private set; }
        public Image        Image { get; private set; }
        public int          Width;
        public Anchor       Anchor;

        //----------------------------------------------------------------------
        public ListViewColumn( ListView _listView, string _strText, int _iWidth, Anchor _anchor )
        {
            Width   = _iWidth;
            Label   = new UI.Label( _listView.Screen, _strText );
            Anchor  = _anchor;
        }

        public ListViewColumn( ListView _listView, Texture2D _tex, Color _color, int _iWidth, Anchor _anchor )
        {
            Width   = _iWidth;
            Image   = new Image( _listView.Screen, _tex );
            Image.Color = _color;
            Anchor  = _anchor;
        }
    }

    //--------------------------------------------------------------------------
    abstract public class ListViewCell
    {
        //----------------------------------------------------------------------
        protected ListView              mListView;

        public string                   Text;
        public Texture2D                Image;
        public Color                    Color;

        //----------------------------------------------------------------------
        public ListViewCell( ListView _view, string _strText, Texture2D _image )
        {
            mListView = _view;
            Text    = _strText;
            Image   = _image;
        }

        //----------------------------------------------------------------------
        internal abstract void DoLayout( ListViewColumn _col );
        internal abstract void Draw( Point _location );
    }

    //--------------------------------------------------------------------------
    public class ListViewTextCell: ListViewCell
    {
        string          mstrText;
        float           mfTextWidth;
        Vector2         mvTextOffset;

        //----------------------------------------------------------------------
        public ListViewTextCell( ListView _view, string _strText )
        : base( _view, _strText, null )
        {
            Color   = mListView.TextColor;
        }

        //----------------------------------------------------------------------
        internal override void DoLayout( ListViewColumn _col )
        {
            mstrText = Text;
            mfTextWidth = mListView.Screen.Style.MediumFont.MeasureString( mstrText ).X + 20 + mListView.ColSpacing;
            if( mstrText != "" && mfTextWidth > _col.Width )
            {
                int iOffset = mstrText.Length;

                while( mfTextWidth > _col.Width )
                {
                    iOffset--;
                    mstrText = Text.Substring( 0, iOffset ) + "…";
                    if( iOffset == 0 ) break;

                    mfTextWidth = mListView.Screen.Style.MediumFont.MeasureString( mstrText ).X + 20 + mListView.ColSpacing;
                }
            }

            mvTextOffset = Vector2.Zero;
            switch( _col.Anchor )
            {
                case Anchor.Start:
                    mvTextOffset.X += 10;
                    break;
                case Anchor.Center:
                    mvTextOffset.X += _col.Width / 2f - mfTextWidth / 2f;
                    break;
                case Anchor.End:
                    mvTextOffset.X += _col.Width - mfTextWidth - 10;
                    break;
            }
        }

        //----------------------------------------------------------------------
        internal override void Draw( Point _location )
        {
            Vector2 vTextPos = new Vector2( _location.X, _location.Y + 10 + mListView.RowHeight / 2 - ( mListView.Screen.Style.MediumFont.LineSpacing * 0.9f ) / 2f );
            vTextPos += mvTextOffset;
            vTextPos.Y += mListView.Screen.Style.MediumFont.YOffset;

            mListView.Screen.Game.SpriteBatch.DrawString( mListView.Screen.Style.MediumFont, mstrText, vTextPos, Color );
        }
    }

    //--------------------------------------------------------------------------
    public class ListViewImageCell: ListViewCell
    {
        Vector2     mvOffset;

        //----------------------------------------------------------------------
        public ListViewImageCell( ListView _view, Texture2D _image )
        : base( _view, null, _image )
        {
            Color   = Color.White;
        }

        //----------------------------------------------------------------------
        internal override void DoLayout( ListViewColumn _col )
        {
            mvOffset = Vector2.Zero;
            switch( _col.Anchor )
            {
                case Anchor.Start:
                    mvOffset.X += 10;
                    break;
                case Anchor.Center:
                    mvOffset.X += _col.Width / 2f - Image.Width / 2f;
                    break;
                case Anchor.End:
                    mvOffset.X += _col.Width - Image.Width - 10;
                    break;
            }
        }

        //----------------------------------------------------------------------
        internal override void Draw( Point _location )
        {
            Vector2 vImagePos = new Vector2( _location.X, _location.Y + 10 + mListView.RowHeight / 2 - Image.Height / 2f );
            vImagePos += mvOffset;

            mListView.Screen.Game.SpriteBatch.Draw( Image, vImagePos, Color );
        }
    }

    //--------------------------------------------------------------------------
    /*
     * A set of values for a row in a ListView
     */
    public class ListViewRow
    {
        //----------------------------------------------------------------------
        public ListViewCell[]           Cells           { get; private set; }
        public object                   Tag;

        //----------------------------------------------------------------------
        public ListViewRow( ListViewCell[] _aCells, object _tag )
        {
            Cells           = _aCells;
            Tag             = _tag;
        }
    }

    /*
     * A ListView displays data as a grid
     * Rows can be selected
     */
    public class ListView: Widget
    {
        //----------------------------------------------------------------------
        public struct ListViewStyle
        {
            public Texture2D        ListFrame;
            public Texture2D        FrameSelected;
            public Texture2D        FrameSelectedHover;
            public Texture2D        FrameSelectedFocus;
        }

        //----------------------------------------------------------------------
        public List<ListViewColumn> Columns             { get; private set; }
        public bool                 DisplayColumnHeaders    = true;
        public bool                 MergeColumns            = false;
        public bool                 SelectFocusedRow        = true;

        public List<ListViewRow>    Rows;
        public int                  RowHeight   = 60;
        public int                  RowSpacing  = 0;
        public int                  ColSpacing  = 0;

        public int                  SelectedRowIndex;
        public ListViewRow          SelectedRow         { get { return SelectedRowIndex != -1 ? Rows[ SelectedRowIndex ] : null; } }
        public Action<ListView>     SelectHandler;
        public Action<ListView>     ValidateHandler;

        int                         miFocusedRowIndex;
        Point                       mHoverPoint;
        public int                  HoveredRowIndex     { get; private set; }
        public ListViewRow          HoveredRow          { get { return HoveredRowIndex != -1 ? Rows[ HoveredRowIndex ] : null; } }

        public ListViewStyle        Style;
        public Color                TextColor;

        //----------------------------------------------------------------------
        public List<Button>         ActionButtons       { get; private set; }
        Button                      mHoveredActionButton;
        bool                        mbIsHoveredActionButtonDown;

        //----------------------------------------------------------------------
        public Scrollbar            Scrollbar           { get; private set; }

        //----------------------------------------------------------------------
        public ListView( Screen _screen )
        : base( _screen )
        {
            Columns = new List<ListViewColumn>();
            Rows    = new List<ListViewRow>();
            SelectedRowIndex    = -1;
            miFocusedRowIndex   = -1;
            HoveredRowIndex   = -1;
            TextColor = Screen.Style.DefaultTextColor;

            Style.ListFrame             = Screen.Style.ListFrame;
            Style.FrameSelected         = Screen.Style.GridBoxFrameSelected;
            Style.FrameSelectedHover    = Screen.Style.GridBoxFrameSelectedHover;
            Style.FrameSelectedFocus    = Screen.Style.GridBoxFrameSelectedFocus;

            Scrollbar = new Scrollbar( this );

            ActionButtons = new List<Button>();

            UpdateContentSize();
        }

        //----------------------------------------------------------------------
        public void AddColumn( string _strText, int _iWidth, Anchor _anchor = Anchor.Center )
        {
            Columns.Add( new ListViewColumn( this, _strText, _iWidth, _anchor ) );
        }

        public void AddColumn( Texture2D _tex, Color _color, int _iWidth, Anchor _anchor = Anchor.Center )
        {
            Columns.Add( new ListViewColumn( this, _tex, _color, _iWidth, _anchor ) );
        }

        //----------------------------------------------------------------------
        public void Clear()
        {
            Rows.Clear();
            SelectedRowIndex    = -1;
            miFocusedRowIndex   = -1;
            HoveredRowIndex   = -1;
        }

        //----------------------------------------------------------------------
        internal override void UpdateContentSize()
        {
            ContentWidth    = Padding.Left + Padding.Right;
            ContentHeight   = Padding.Top + Padding.Bottom;

            base.UpdateContentSize();
        }

        //----------------------------------------------------------------------
        internal override void Update( float _fElapsedTime )
        {
            foreach( Button actionButton in ActionButtons )
            {
                actionButton.Update( _fElapsedTime );
            }

            Scrollbar.Update( _fElapsedTime );

            base.Update( _fElapsedTime );
        }

        //----------------------------------------------------------------------
        internal override void DoLayout( Rectangle _rect )
        {
            base.DoLayout( _rect );
            HitBox = new Rectangle( LayoutRect.X + 10, LayoutRect.Y + 10, LayoutRect.Width - 20, LayoutRect.Height - 20 );
            
            int iColX = 0;
            int iColIndex = 0;
            foreach( ListViewColumn col in Columns )
            {
                if( col.Label != null )
                {
                    col.Label.DoLayout( new Rectangle( LayoutRect.X + 10 + iColX, LayoutRect.Y + 10, col.Width, RowHeight ) );
                }
                else
                {
                    col.Image.DoLayout( new Rectangle( LayoutRect.X + 10 + iColX, LayoutRect.Y + 10, col.Width, RowHeight ) );
                }


                iColX += col.Width + ColSpacing;

                foreach( ListViewRow row in Rows )
                {
                    row.Cells[ iColIndex ].DoLayout( col );
                }

                iColIndex++;
            }

            //------------------------------------------------------------------
            if( HoveredRowIndex != -1 )
            {
                int iButtonX = 0;
                foreach( Button button in ActionButtons.Reverse<Button>() )
                {
                    button.DoLayout( new Rectangle(
                        LayoutRect.Right - 20 - iButtonX - button.ContentWidth,
                        LayoutRect.Y + 10 + GetRowY( HoveredRowIndex ) + RowHeight / 2 - button.ContentHeight / 2,
                        button.ContentWidth, button.ContentHeight )
                    );

                    iButtonX += button.ContentWidth;
                }
            }

            //------------------------------------------------------------------
            ContentHeight = Padding.Vertical + 25 + Rows.Count * ( RowHeight + RowSpacing );
            Scrollbar.DoLayout( LayoutRect, ContentHeight );
        }

        //----------------------------------------------------------------------
        internal override void OnMouseEnter( Point _hitPoint )
        {
            mHoverPoint = _hitPoint;
            UpdateHoveredRow();
        }

        internal override void OnMouseMove( Point _hitPoint )
        {
            mHoverPoint = _hitPoint;
            UpdateHoveredRow();
        }

        void UpdateHoveredRow()
        {
            HoveredRowIndex = ( mHoverPoint.Y - ( LayoutRect.Y + 10 + ( DisplayColumnHeaders ? RowHeight : 0 ) - (int)Scrollbar.LerpOffset ) ) / ( RowHeight + RowSpacing );

            if( HoveredRowIndex >= Rows.Count )
            {
                HoveredRowIndex = -1;

                if( mbIsHoveredActionButtonDown )
                {
                    mHoveredActionButton.ResetPressState();
                    mbIsHoveredActionButtonDown = false;
                }
                mHoveredActionButton = null;
            }
            else
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
        }

        internal override void OnMouseOut( Point _hitPoint )
        {
            HoveredRowIndex = -1;
        }

        internal override void OnMouseWheel( Point _hitPoint, int _iDelta )
        {
            int iNewScrollOffset = (int)MathHelper.Clamp( Scrollbar.Offset - ( _iDelta / 120 * 3 * ( RowHeight + RowSpacing ) ), 0, Math.Max( 0, Scrollbar.Max ) );
            Scrollbar.Offset = iNewScrollOffset;
        }

        //----------------------------------------------------------------------
        internal override void OnMouseDown( Point _hitPoint, int _iButton )
        {
            if( _iButton != 0 ) return;

            mHoverPoint = _hitPoint;

            if( mHoveredActionButton != null )
            {
                mHoveredActionButton.OnActivateDown();
                mbIsHoveredActionButtonDown = true;
            }
            else
            {
                Screen.Focus( this );
                miFocusedRowIndex = Math.Max( 0, ( _hitPoint.Y - ( LayoutRect.Y + 10 + ( DisplayColumnHeaders ? RowHeight : 0 ) ) + (int)Scrollbar.LerpOffset ) / ( RowHeight + RowSpacing ) );
                if( miFocusedRowIndex > Rows.Count - 1 )
                {
                    miFocusedRowIndex = -1;
                }
            }
        }

        internal override void OnMouseUp( Point _hitPoint, int _iButton )
        {
            if( _iButton != 0 ) return;

            mHoverPoint = _hitPoint;

            if( mHoveredActionButton != null )
            {
                if( mbIsHoveredActionButtonDown )
                {
                    mHoveredActionButton.OnMouseUp( _hitPoint, _iButton );
                    mbIsHoveredActionButtonDown = false;
                }
            }
            else
            {
                SelectRowAt( _hitPoint );
            }
        }

        //----------------------------------------------------------------------
        internal override bool OnMouseDoubleClick( Point _hitPoint )
        {
            if( mHoveredActionButton == null && ValidateHandler != null )
            {
                SelectRowAt( _hitPoint );

                if( SelectedRowIndex != -1 )
                {
                    ValidateHandler( this );
                    return true;
                }
            }

            return false;
        }

        void SelectRowAt( Point _hitPoint )
        {
            UpdateHoveredRow();

            if( HoveredRowIndex != SelectedRowIndex )
            {
                if( HoveredRowIndex != -1 || SelectFocusedRow )
                {
                    SelectedRowIndex = HoveredRowIndex;
                    miFocusedRowIndex = SelectedRowIndex;

                    if( SelectHandler != null ) SelectHandler( this );
                }
            }
        }

        internal override void OnActivateUp()
        {
            if( miFocusedRowIndex != SelectedRowIndex && miFocusedRowIndex != -1 )
            {
                SelectedRowIndex = miFocusedRowIndex;
                if( SelectHandler != null ) SelectHandler( this );
            }
            else
            {
                if( ValidateHandler != null ) ValidateHandler( this );
            }
        }

        //----------------------------------------------------------------------
        internal override void OnPadMove( Direction _direction )
        {
            if( _direction == Direction.Up )
            {
                miFocusedRowIndex = Math.Max( 0, miFocusedRowIndex - 1 );

                if( SelectFocusedRow )
                {
                    SelectedRowIndex = miFocusedRowIndex;
                }
            }
            else
            if( _direction == Direction.Down )
            {
                miFocusedRowIndex = Math.Min( Rows.Count - 1, miFocusedRowIndex + 1 );

                if( SelectFocusedRow )
                {
                    SelectedRowIndex = miFocusedRowIndex;
                }
            }
            else
            {
                base.OnPadMove( _direction );
            }
        }

        //----------------------------------------------------------------------
        internal override void OnWindowsKeyPress( System.Windows.Forms.Keys _key )
        {
            switch( _key )
            {
                case System.Windows.Forms.Keys.Enter:
                    if( SelectedRowIndex != -1 && ValidateHandler != null ) ValidateHandler( this );
                    break;
            }
        }

        //----------------------------------------------------------------------
        internal override void OnBlur()
        {
            miFocusedRowIndex = -1;
        }

        int GetRowY( int _iRowIndex )
        {
            return ( ( DisplayColumnHeaders ? 1 : 0 ) + _iRowIndex ) * ( RowHeight + RowSpacing ) - (int)Scrollbar.LerpOffset;
        }

        //----------------------------------------------------------------------
        internal override void Draw()
        {
            Screen.DrawBox( Style.ListFrame, LayoutRect, Screen.Style.GridBoxFrameCornerSize, Color.White );

            if( DisplayColumnHeaders )
            {
                int iColX = 0;
                foreach( ListViewColumn col in Columns )
                {
                    Screen.DrawBox( Screen.Style.GridHeaderFrame, new Rectangle( LayoutRect.X + 10 + iColX, LayoutRect.Y + 10, col.Width, RowHeight ), Screen.Style.GridBoxFrameCornerSize, Color.White );

                    if( col.Label != null )
                    {
                        col.Label.Draw();
                    }
                    else
                    {
                        col.Image.Draw();
                    }
                    iColX += col.Width + ColSpacing;
                }
            }

            Screen.PushScissorRectangle( new Rectangle( LayoutRect.X + 10, LayoutRect.Y + 10 + ( DisplayColumnHeaders ? RowHeight : 0 ), LayoutRect.Width - 20, LayoutRect.Height - 20 - ( DisplayColumnHeaders ? RowHeight : 0 ) ) );

            int iRowIndex = 0;
            foreach( ListViewRow row in Rows )
            {
                int iRowY = GetRowY( iRowIndex );
                if( ( iRowY + RowHeight + RowSpacing < 0 ) || ( iRowY > LayoutRect.Height - 20 ) )
                {
                    iRowIndex++;
                    continue;
                }

                if( MergeColumns )
                {
                    Rectangle rowRect = new Rectangle( LayoutRect.X + 10, LayoutRect.Y + 10 + iRowY, LayoutRect.Width - 20, RowHeight );

                    Screen.DrawBox( SelectedRowIndex == iRowIndex ? Style.FrameSelected : Screen.Style.GridBoxFrame, rowRect, Screen.Style.GridBoxFrameCornerSize, Color.White );

                    if( HasFocus && miFocusedRowIndex == iRowIndex )
                    {
                        if( SelectedRowIndex != iRowIndex )
                        {
                            Screen.DrawBox( Screen.Style.GridBoxFrameFocus, rowRect, Screen.Style.GridBoxFrameCornerSize, Color.White );
                        }
                        else
                        if( Style.FrameSelectedFocus != null )
                        {
                            Screen.DrawBox( Style.FrameSelectedFocus, rowRect, Screen.Style.GridBoxFrameCornerSize, Color.White );
                        }
                    }

                    if( HoveredRowIndex == iRowIndex )
                    {
                        if( SelectedRowIndex != iRowIndex )
                        {
                            Screen.DrawBox( Screen.Style.GridBoxFrameHover, rowRect, Screen.Style.GridBoxFrameCornerSize, Color.White );
                        }
                        else
                        if( Style.FrameSelectedHover != null )
                        {
                            Screen.DrawBox( Style.FrameSelectedHover, rowRect, Screen.Style.GridBoxFrameCornerSize, Color.White );
                        }
                    }
                }

                int iColX = 0;
                for( int i = 0; i < row.Cells.Length; i++ )
                {
                    ListViewColumn col = Columns[i];

                    Rectangle rowRect = new Rectangle( LayoutRect.X + 10 + iColX, LayoutRect.Y + 10 + iRowY, col.Width, RowHeight );

                    if( ! MergeColumns )
                    {
                        Screen.DrawBox( SelectedRowIndex == iRowIndex ? Style.FrameSelected : Screen.Style.GridBoxFrame, rowRect, Screen.Style.GridBoxFrameCornerSize, Color.White );

                        if( HasFocus && miFocusedRowIndex == iRowIndex )
                        {
                            if( SelectedRowIndex != iRowIndex )
                            {
                                Screen.DrawBox( Screen.Style.GridBoxFrameFocus, rowRect, Screen.Style.GridBoxFrameCornerSize, Color.White );
                            }
                            else
                            if( Style.FrameSelectedFocus != null )
                            {
                                Screen.DrawBox( Style.FrameSelectedFocus, rowRect, Screen.Style.GridBoxFrameCornerSize, Color.White );
                            }
                        }

                        if( HoveredRowIndex == iRowIndex )
                        {
                            if( SelectedRowIndex != iRowIndex )
                            {
                                Screen.DrawBox( Screen.Style.GridBoxFrameHover, rowRect, Screen.Style.GridBoxFrameCornerSize, Color.White );
                            }
                            else
                            if( Style.FrameSelectedHover != null )
                            {
                                Screen.DrawBox( Style.FrameSelectedHover, rowRect, Screen.Style.GridBoxFrameCornerSize, Color.White );
                            }
                        }
                    }

                    row.Cells[i].Draw( new Point( LayoutRect.X + 10 + iColX, LayoutRect.Y + iRowY ) );

                    iColX += col.Width + ColSpacing;
                }

                iRowIndex++;
            }

            if( HoveredRowIndex != -1 )
            {
                foreach( Button button in ActionButtons )
                {
                    button.Draw();
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
    }
}
