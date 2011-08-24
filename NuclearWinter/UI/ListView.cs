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
        public enum ColumnType
        {
            Text,
            Image
        }

        //----------------------------------------------------------------------
        public ColumnType   Type;
        public Label        Label { get; private set; }
        public int          Width;
        public Anchor       Anchor;

        //----------------------------------------------------------------------
        public ListViewColumn( ListView _listView, ColumnType _type, string _strText, int _iWidth, Anchor _anchor )
        {
            Type    = _type;
            Width   = _iWidth;
            Label   = new UI.Label( _listView.Screen, _strText );
            Anchor  = _anchor;
        }
    }

    public struct ListViewCell
    {
        //----------------------------------------------------------------------
        public string       Text;
        public Texture2D    Image;

        //----------------------------------------------------------------------
        public ListViewCell( string _strText )
        {
            Text    = _strText;
            Image   = null;
        }

        //----------------------------------------------------------------------
        public ListViewCell( Texture2D _image )
        {
            Text    = null;
            Image   = _image;
        }
    }

    /*
     * A set of values for a row in a ListView
     */
    public struct ListViewRow
    {
        public ListViewRow( ListViewCell[] _aCells, object _tag )
        {
            Cells   = _aCells;
            Tag     = _tag;
        }

        public ListViewCell[]       Cells;
        public object               Tag;
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

        public List<ListViewRow>    Rows                { get; private set; }
        public int                  RowHeight = 60;
        public int                  RowSpacing = 0;

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
        public void AddColumn( string _strText, ListViewColumn.ColumnType _type, int _iWidth, Anchor _anchor )
        {
            Columns.Add( new ListViewColumn( this, _type, _strText, _iWidth, _anchor ) );
        }

        public void AddColumn( string _strText, int _iWidth, Anchor _anchor )
        {
            Columns.Add( new ListViewColumn( this, ListViewColumn.ColumnType.Text, _strText, _iWidth, _anchor ) );
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
        public override void DoLayout( Rectangle? _rect )
        {
            if( _rect.HasValue )
            {
                Position = _rect.Value.Location;
                Size = new Point( _rect.Value.Width, _rect.Value.Height );
                HitBox = _rect.Value;
            }

            int iColX = 0;
            foreach( ListViewColumn col in Columns )
            {
                col.Label.DoLayout( new Rectangle( Position.X + 10 + iColX, Position.Y + 10, col.Width, RowHeight ) );
                iColX += col.Width;
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

                    switch( col.Type )
                    {
                        case ListViewColumn.ColumnType.Text:
                            {
                                float fTextWidth = Screen.Style.MediumFont.MeasureString( row.Cells[i].Text ).X;
                                Vector2 vTextPos = new Vector2( Position.X + iColX + 10, Position.Y + 10 + iRowY + RowHeight / 2 - ( Screen.Style.MediumFont.LineSpacing * 0.9f ) / 2f );
                                switch( col.Anchor )
                                {
                                    case Anchor.Start:
                                        vTextPos.X += 10;
                                        break;
                                    case Anchor.Center:
                                        vTextPos.X += col.Width / 2f - fTextWidth / 2f;
                                        break;
                                    case Anchor.End:
                                        vTextPos.X += col.Width - fTextWidth - 10;
                                        break;
                                }

                                Screen.Game.SpriteBatch.DrawString( Screen.Style.MediumFont, row.Cells[i].Text, vTextPos, TextColor );
                            }
                            break;
                        case ListViewColumn.ColumnType.Image:
                            {
                                Texture2D image = row.Cells[i].Image;

                                Vector2 vImagePos = new Vector2( Position.X + iColX + 10 + col.Width / 2f - image.Width / 2f, Position.Y + 10 + iRowY + RowHeight / 2 - image.Height / 2f );
                                switch( col.Anchor )
                                {
                                    case Anchor.Start:
                                        vImagePos.X += 10;
                                        break;
                                    case Anchor.Center:
                                        vImagePos.X += col.Width / 2f - image.Width / 2f;
                                        break;
                                    case Anchor.End:
                                        vImagePos.X += col.Width - image.Width - 10;
                                        break;
                                }
                                Screen.Game.SpriteBatch.Draw( image, vImagePos, Color.White );
                            }
                            break;
                    }


                    iColX += col.Width;
                }

                iRow++;
            }
        }
    }
}
