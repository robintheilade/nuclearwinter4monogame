using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using NuclearWinter.Animation;

namespace NuclearWinter.UI
{
    /*
     * A ListViewColumn defines a column in a ListView
     */
    public class ListViewColumn
    {
        public ListViewColumn( ListView _listView, string _strText, int _iWidth, Anchor _anchor )
        {
            Width   = _iWidth;
            Label   = new UI.Label( _listView.Screen, _strText );
            Anchor  = _anchor;
        }

        public Label    Label { get; private set; }
        public int      Width;
        public Anchor   Anchor;
    }

    /*
     * A set of values for a row in a ListView
     */
    public struct ListViewRow
    {
        public ListViewRow( string[] _aFields, object _tag )
        {
            Fields  = _aFields;
            Tag     = _tag;
        }

        public string[]             Fields;
        public object               Tag;
    }

    /*
     * A ListView displays data as a grid
     * Rows can be selected
     */
    public class ListView: Widget
    {
        public int                  RowHeight = 60;

        public List<ListViewColumn> Columns             { get; private set; }
        public List<ListViewRow>    Rows                { get; private set; }
        public int                  SelectedRowIndex    { get; private set; }

        bool                        mbIsHovered;
        Point                       mHoverPoint;

        public override bool CanFocus { get { return true; } }

        public void AddColumn( string _strText, int _iWidth, Anchor _anchor )
        {
            Columns.Add( new ListViewColumn( this, _strText, _iWidth, _anchor ) );
        }

        //----------------------------------------------------------------------
        public ListView( Screen _screen )
        : base( _screen )
        {
            Columns = new List<ListViewColumn>();
            Rows    = new List<ListViewRow>();
            SelectedRowIndex = -1;

            UpdateContentSize();
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
        }

        public override void OnMouseUp( Point _hitPoint )
        {
            SelectedRowIndex = (int)MathHelper.Clamp( ( _hitPoint.Y - ( Position.Y + 10 + RowHeight ) ) / RowHeight, 0, Rows.Count - 1 );
        }

        //----------------------------------------------------------------------
        public override void OnPadMove( Direction _direction )
        {
            if( _direction == Direction.Up )
            {
                SelectedRowIndex = Math.Max( 0, SelectedRowIndex - 1 );
            }
            else
            if( _direction == Direction.Down )
            {
                SelectedRowIndex = Math.Min( Rows.Count - 1, SelectedRowIndex + 1 );
            }
            else
            {
                base.OnPadMove( _direction );
            }
        }


        //----------------------------------------------------------------------
        public override void Draw()
        {
            Screen.DrawBox( Screen.Style.GridFrame, new Rectangle( Position.X, Position.Y, Size.X, Size.Y ), 30, Color.White );

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
                iHoverRow = ( mHoverPoint.Y - ( Position.Y + 10 + RowHeight ) ) / RowHeight;
            }

            int iRow = 0;
            foreach( ListViewRow row in Rows )
            {
                int iRowY = iRow * RowHeight;
                int iColX = 0;
                for( int i = 0; i < row.Fields.Length; i++ )
                {
                    ListViewColumn col = Columns[i];

                    Screen.DrawBox( SelectedRowIndex == iRow ? Screen.Style.GridBoxFrameSelected : Screen.Style.GridBoxFrame, new Rectangle( Position.X + 10 + iColX, Position.Y + 10 + RowHeight + iRowY, col.Width, RowHeight ), 30, Color.White );

                    if( HasFocus && SelectedRowIndex == iRow )
                    {
                        Screen.DrawBox( Screen.Style.GridBoxFrameFocused, new Rectangle( Position.X + 10 + iColX, Position.Y + 10 + RowHeight + iRowY, col.Width, RowHeight ), 30, Color.White );
                    }

                    if( iHoverRow == iRow )
                    {
                        Screen.DrawBox( Screen.Style.GridBoxFrameHover, new Rectangle( Position.X + 10 + iColX, Position.Y + 10 + RowHeight + iRowY, col.Width, RowHeight ), 30, Color.White );
                    }

                    float fTextWidth = Screen.Style.MediumFont.MeasureString( row.Fields[i] ).X;
                    Vector2 vTextPos = new Vector2( Position.X + iColX + 10, Position.Y + 10 + RowHeight + 30 - ( Screen.Style.MediumFont.LineSpacing * 0.9f ) / 2f + iRowY );
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

                    Screen.Game.SpriteBatch.DrawString( Screen.Style.MediumFont, row.Fields[i], vTextPos, Color.White );
                    iColX += col.Width;
                }

                iRow++;
            }
        }
    }
}
