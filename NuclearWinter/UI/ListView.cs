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
    /*
     * A ListViewColumn defines a column in a ListView
     */
    public class ListViewColumn
    {
        //----------------------------------------------------------------------
        public Label        Label { get; private set; }
        public int          Width;
        public Anchor       Anchor;

        //----------------------------------------------------------------------
        public ListViewColumn( ListView _listView, string _strText, int _iWidth, Anchor _anchor )
        {
            Width   = _iWidth;
            Label   = new UI.Label( _listView.Screen, _strText );
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

        //----------------------------------------------------------------------
        public ListViewCell( ListView _view, string _strText, Texture2D _image )
        {
            mListView = _view;
            Text    = _strText;
            Image   = _image;
        }

        //----------------------------------------------------------------------
        public abstract void DoLayout( ListViewColumn _col );
        public abstract void Draw( Point _location );
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

        }

        //----------------------------------------------------------------------
        public override void DoLayout( ListViewColumn _col )
        {
            mstrText = Text;
            mfTextWidth = mListView.Screen.Style.MediumFont.MeasureString( mstrText ).X + 20 + mListView.ColSpacing;
            if( mstrText != "" && mfTextWidth > _col.Width )
            {
                int iOffset = mstrText.Length;

                while( mfTextWidth > _col.Width )
                {
                    iOffset--;
                    mstrText = Text.Substring( 0, iOffset ) + "...";
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
        public override void Draw( Point _location )
        {
            Vector2 vTextPos = new Vector2( _location.X, _location.Y + 10 + mListView.RowHeight / 2 - ( mListView.Screen.Style.MediumFont.LineSpacing * 0.9f ) / 2f );
            vTextPos += mvTextOffset;

            mListView.Screen.Game.SpriteBatch.DrawString( mListView.Screen.Style.MediumFont, mstrText, vTextPos, mListView.TextColor );
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

        }

        //----------------------------------------------------------------------
        public override void DoLayout( ListViewColumn _col )
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
        public override void Draw( Point _location )
        {
            Vector2 vImagePos = new Vector2( _location.X, _location.Y + 10 + mListView.RowHeight / 2 - Image.Height / 2f );
            vImagePos += mvOffset;

            mListView.Screen.Game.SpriteBatch.Draw( Image, vImagePos, Color.White );
        }
    }

    //--------------------------------------------------------------------------
    /*
     * A set of values for a row in a ListView
     */
    public class ListViewRow
    {
        public ListViewRow( ListViewCell[] _aCells, object _tag )
        {
            Cells           = _aCells;
            Tag             = _tag;
        }

        public ListViewCell[]           Cells           { get; private set; }
        public object                   Tag;
    }

    /*
     * A ListView displays data as a grid
     * Rows can be selected
     */
    public class ListView: Widget
    {
        public struct ListViewStyle
        {
            public Texture2D        FrameSelected;
            public Texture2D        FrameSelectedHover;
            public Texture2D        FrameSelectedFocus;
        }

        public List<ListViewColumn> Columns             { get; private set; }
        public bool                 DisplayColumnHeaders    = true;
        public bool                 MergeColumns            = false;
        public bool                 SelectFocusedRow        = true;

        public List<ListViewRow>    Rows;
        public int                  RowHeight   = 60;
        public int                  RowSpacing  = 0;
        public int                  ColSpacing  = 0;

        public int                  SelectedRowIndex;
        public Action<ListView>     SelectHandler;
        public Action<ListView>     ValidateHandler;

        int                         miFocusedRowIndex;
        bool                        mbIsHovered;
        Point                       mHoverPoint;

        public ListViewStyle        Style;
        public Color                TextColor           = Color.White;

        public override bool CanFocus { get { return true; } }

        //----------------------------------------------------------------------
        public ListView( Screen _screen )
        : base( _screen )
        {
            Columns = new List<ListViewColumn>();
            Rows    = new List<ListViewRow>();
            SelectedRowIndex    = -1;
            miFocusedRowIndex   = -1;

            Style.FrameSelected         = Screen.Style.GridBoxFrameSelected;
            Style.FrameSelectedHover    = Screen.Style.GridBoxFrameSelectedHover;
            Style.FrameSelectedFocus    = Screen.Style.GridBoxFrameSelectedFocus;

            UpdateContentSize();
        }

        //----------------------------------------------------------------------
        public void AddColumn( string _strText, int _iWidth, Anchor _anchor )
        {
            Columns.Add( new ListViewColumn( this, _strText, _iWidth, _anchor ) );
        }

        //----------------------------------------------------------------------
        public void Clear()
        {
            Rows.Clear();
            SelectedRowIndex = -1;
            miFocusedRowIndex = -1;
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
        
            int iColX = 0;
            int iColIndex = 0;
            foreach( ListViewColumn col in Columns )
            {
                col.Label.DoLayout( new Rectangle( Position.X + 10 + iColX, Position.Y + 10, col.Width, RowHeight ) );
                iColX += col.Width;

                foreach( ListViewRow row in Rows )
                {
                    row.Cells[ iColIndex ].DoLayout( col );
                }

                iColIndex++;
            }
        }

        //----------------------------------------------------------------------
        public override void OnMouseEnter( Point _hitPoint )
        {
            mbIsHovered = true;
            mHoverPoint = _hitPoint;
        }

        public override void OnMouseMove(Point _hitPoint)
        {
            mHoverPoint = _hitPoint;
        }

        public override void OnMouseOut( Point _hitPoint )
        {
            mbIsHovered = false;
        }

        //----------------------------------------------------------------------
        public override void OnMouseDown( Point _hitPoint )
        {
            Screen.Focus( this );
            miFocusedRowIndex = Math.Max( 0, ( _hitPoint.Y - ( Position.Y + 10 + ( DisplayColumnHeaders ? RowHeight : 0 ) ) ) / ( RowHeight + RowSpacing ) );
            if( miFocusedRowIndex > Rows.Count - 1 )
            {
                miFocusedRowIndex = -1;
            }
        }

        public override void OnMouseUp( Point _hitPoint )
        {
            int iSelectedRowIndex = Math.Max( 0, ( _hitPoint.Y - ( Position.Y + 10 + ( DisplayColumnHeaders ? RowHeight : 0 ) ) ) / ( RowHeight + RowSpacing ) );

            if( iSelectedRowIndex <= Rows.Count - 1 && iSelectedRowIndex == miFocusedRowIndex && SelectedRowIndex != miFocusedRowIndex )
            {
                SelectedRowIndex = miFocusedRowIndex;
                if( SelectHandler != null ) SelectHandler( this );
            }
        }

        public override void OnActivateUp()
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
        public override void OnPadMove( Direction _direction )
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
        public override void OnKeyPress( Keys _key )
        {
            switch( _key )
            {
                case Keys.Enter:
                    if( SelectedRowIndex != -1 && ValidateHandler != null ) ValidateHandler( this );
                    break;
            }
        }

        //----------------------------------------------------------------------
        public override void OnBlur()
        {
            miFocusedRowIndex = -1;
        }

        //----------------------------------------------------------------------
        public override void Draw()
        {
            Screen.DrawBox( Screen.Style.GridFrame, new Rectangle( Position.X, Position.Y, Size.X, Size.Y ), 30, Color.White );

            if( DisplayColumnHeaders )
            {
                int iColX = 0;
                foreach( ListViewColumn col in Columns )
                {
                    Screen.DrawBox( Screen.Style.GridHeaderFrame, new Rectangle( Position.X + 10 + iColX, Position.Y + 10, col.Width, RowHeight ), 30, Color.White );
                    col.Label.Draw();
                    iColX += col.Width;
                }
            }

            int iHoverRow = -1;
            if( mbIsHovered )
            {
                iHoverRow = ( mHoverPoint.Y - ( Position.Y + 10 + ( DisplayColumnHeaders ? RowHeight : 0 ) ) ) / ( RowHeight + RowSpacing );
            }

            int iRow = 0;
            foreach( ListViewRow row in Rows )
            {
                int iRowY = ( ( DisplayColumnHeaders ? 1 : 0 ) + iRow ) * ( RowHeight + RowSpacing );
                if( MergeColumns )
                {
                    Screen.DrawBox( SelectedRowIndex == iRow ? Style.FrameSelected : Screen.Style.GridBoxFrame, new Rectangle( Position.X + 10, Position.Y + 10 + iRowY, Size.X - 20, RowHeight ), Screen.Style.GridBoxFrameCornerSize, Color.White );

                    if( HasFocus && miFocusedRowIndex == iRow )
                    {
                        if( SelectedRowIndex != iRow )
                        {
                            Screen.DrawBox( Screen.Style.GridBoxFrameFocus, new Rectangle( Position.X + 10, Position.Y + 10 + iRowY, Size.X - 20, RowHeight ), Screen.Style.GridBoxFrameCornerSize, Color.White );
                        }
                        else
                        if( Style.FrameSelectedFocus != null )
                        {
                            Screen.DrawBox( Style.FrameSelectedFocus, new Rectangle( Position.X + 10, Position.Y + 10 + iRowY, Size.X - 20, RowHeight ), Screen.Style.GridBoxFrameCornerSize, Color.White );
                        }
                    }

                    if( iHoverRow == iRow )
                    {
                        if( SelectedRowIndex != iRow )
                        {
                            Screen.DrawBox( Screen.Style.GridBoxFrameHover, new Rectangle( Position.X + 10, Position.Y + 10 + iRowY, Size.X - 20, RowHeight ), Screen.Style.GridBoxFrameCornerSize, Color.White );
                        }
                        else
                        if( Style.FrameSelectedHover != null )
                        {
                            Screen.DrawBox( Style.FrameSelectedHover, new Rectangle( Position.X + 10, Position.Y + 10 + iRowY, Size.X - 20, RowHeight ), Screen.Style.GridBoxFrameCornerSize, Color.White );
                        }
                    }
                }

                int iColX = 0;
                for( int i = 0; i < row.Cells.Length; i++ )
                {
                    ListViewColumn col = Columns[i];

                    if( ! MergeColumns )
                    {
                        Screen.DrawBox( SelectedRowIndex == iRow ? Style.FrameSelected : Screen.Style.GridBoxFrame, new Rectangle( Position.X + 10 + iColX, Position.Y + 10 + iRowY, col.Width, RowHeight ), Screen.Style.GridBoxFrameCornerSize, Color.White );

                        if( HasFocus && miFocusedRowIndex == iRow )
                        {
                            if( SelectedRowIndex != iRow )
                            {
                                Screen.DrawBox( Screen.Style.GridBoxFrameFocus, new Rectangle( Position.X + 10 + iColX, Position.Y + 10 + iRowY, col.Width, RowHeight ), Screen.Style.GridBoxFrameCornerSize, Color.White );
                            }
                            else
                            if( Style.FrameSelectedFocus != null )
                            {
                                Screen.DrawBox( Style.FrameSelectedFocus, new Rectangle( Position.X + 10 + iColX, Position.Y + 10 + iRowY, col.Width, RowHeight ), Screen.Style.GridBoxFrameCornerSize, Color.White );
                            }
                        }

                        if( iHoverRow == iRow )
                        {
                            if( SelectedRowIndex != iRow )
                            {
                                Screen.DrawBox( Screen.Style.GridBoxFrameHover, new Rectangle( Position.X + 10 + iColX, Position.Y + 10 + iRowY, col.Width, RowHeight ), Screen.Style.GridBoxFrameCornerSize, Color.White );
                            }
                            else
                            if( Style.FrameSelectedHover != null )
                            {
                                Screen.DrawBox( Style.FrameSelectedHover, new Rectangle( Position.X + 10 + iColX, Position.Y + 10 + iRowY, col.Width, RowHeight ), Screen.Style.GridBoxFrameCornerSize, Color.White );
                            }
                        }
                    }

                    row.Cells[i].Draw( new Point( Position.X + 10 + iColX, Position.Y + iRowY ) );

                    iColX += col.Width + ColSpacing;
                }

                iRow++;
            }
        }
    }
}
