using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace NuclearWinter.UI
{
    public class GridGroupTile: FixedWidget
    {
        //----------------------------------------------------------------------
        public GridGroupTile( Screen _screen, int _iColumn, int _iRow )
        : base( _screen, AnchoredRect.CreateFull(0) )
        {
            Column  = _iColumn;
            Row     = _iRow;
        }

        public int      Column;
        public int      Row;
    }

    /*
     * GridGroup allows packing widgets in a grid
     * It takes care of positioning each child widget properly
     */
    public class GridGroup: Group
    {
        //----------------------------------------------------------------------
        bool                        mbExpand;
        int                         miSpacing; // FIXME: Not taken into account
        GridGroupTile[,]            maTiles;

        //----------------------------------------------------------------------
        public GridGroup( Screen _screen, int _iCols, int _iRows, bool _bExpand, int _iSpacing )
        : base( _screen )
        {
            mbExpand    = _bExpand;
            miSpacing   = _iSpacing;

            maTiles = new GridGroupTile[ _iCols, _iRows ];

            for( int iRow = 0; iRow < _iRows; iRow++ )
            {
                for( int iColumn = 0; iColumn < _iCols; iColumn++ )
                {
                    GridGroupTile tile = new GridGroupTile( Screen, iColumn, iRow );
                    tile.Parent = this;
                    maTiles[ iColumn, iRow ] = tile;
                }
            }
        }

        public override void AddChild( Widget _widget, int _iIndex )
        {
            throw new NotSupportedException();
        }

        public override void RemoveChild( Widget _widget )
        {
            throw new NotSupportedException();
        }

        public void AddChildAt( Widget _child, int _iColumn, int _iRow )
        {
            maTiles[ _iColumn, _iRow ].Child = _child;
            mlChildren.Add( _child );
        }

        public override Widget GetFirstFocusableDescendant( Direction _direction )
        {
            switch( _direction )
            {
                case Direction.Left:
                    for( int i = maTiles.GetLength(0) - 1; i >= 0; i-- )
                    {
                        for( int j = 0; j < maTiles.GetLength(1); j++ )
                        {
                            Widget widget = maTiles[ i, j ];
                            if( widget != null )
                            {
                                Widget focusableWidget = widget.GetFirstFocusableDescendant( _direction );
                                if( focusableWidget != null )
                                {
                                    return focusableWidget;
                                }
                            }
                        }
                    }
                    break;
                case Direction.Right:
                    for( int i = 0; i < maTiles.GetLength(0); i++ )
                    {
                        for( int j = 0; j < maTiles.GetLength(1); j++ )
                        {
                            Widget widget = maTiles[ i, j ];
                            if( widget != null )
                            {
                                Widget focusableWidget = widget.GetFirstFocusableDescendant( _direction );
                                if( focusableWidget != null )
                                {
                                    return focusableWidget;
                                }
                            }
                        }
                    }
                    break;
                case Direction.Up:
                    for( int j = maTiles.GetLength(1) - 1; j >= 0; j-- )
                    {
                        for( int i = 0; i < maTiles.GetLength(0); i++ )
                        {
                            Widget widget = maTiles[ i, j ];
                            if( widget != null )
                            {
                                Widget focusableWidget = widget.GetFirstFocusableDescendant( _direction );
                                if( focusableWidget != null )
                                {
                                    return focusableWidget;
                                }
                            }
                        }
                    }
                    break;
                case Direction.Down:
                    for( int j = 0; j < maTiles.GetLength(1); j++ )
                    {
                        for( int i = 0; i < maTiles.GetLength(0); i++ )
                        {
                            Widget widget = maTiles[ i, j ];
                            if( widget != null )
                            {
                                Widget focusableWidget = widget.GetFirstFocusableDescendant( _direction );
                                if( focusableWidget != null )
                                {
                                    return focusableWidget;
                                }
                            }
                        }
                    }
                    break;
            }

            return null;
        }

        public override Widget GetSibling( Direction _direction, Widget _child )
        {
            int iIndex = mlChildren.IndexOf( _child );

            Widget tileChild = null;
            GridGroupTile nextTile = (GridGroupTile)_child;
            int iOffset = 0;

            do
            {
                iOffset++;

                switch( _direction )
                {
                    case Direction.Left:
                        if( nextTile.Column - iOffset < 0 ) return base.GetSibling( _direction, this );
                        tileChild = maTiles[ nextTile.Column - iOffset, nextTile.Row ].Child;
                        break;
                    case Direction.Right:
                        if( nextTile.Column + iOffset >= maTiles.GetLength(0) ) return base.GetSibling( _direction, this );
                        tileChild = maTiles[ nextTile.Column + iOffset, nextTile.Row ].Child;
                        break;
                    case Direction.Up:
                        if( nextTile.Row - iOffset < 0 ) return base.GetSibling( _direction, this );
                        tileChild = maTiles[ nextTile.Column, nextTile.Row - iOffset ].Child;
                        break;
                    case Direction.Down:
                        if( nextTile.Row + iOffset >= maTiles.GetLength(1) ) return base.GetSibling( _direction, this );
                        tileChild = maTiles[ nextTile.Column, nextTile.Row + iOffset ].Child;
                        break;
                }
            }
            while( tileChild == null );

            if( tileChild != null )
            {
                return tileChild;
            }
            else
            {
                return base.GetSibling( _direction, this );
            }
        }

        //----------------------------------------------------------------------
        public override void DoLayout( Rectangle _rect )
        {
            Position        = _rect.Location;
            Size            = new Point( _rect.Width, _rect.Height );

            Debug.Assert( Size.X != 0 && Size.Y != 0 );

            if( ! mbExpand )
            {
                int iColumnCount    = maTiles.GetLength(0);
                int iRowCount       = maTiles.GetLength(1);

                Point widgetSize = new Point(
                    Size.X / iColumnCount,
                    Size.Y / iRowCount );

                for( int iRow = 0; iRow < iRowCount; iRow++ )
                {
                    for( int iColumn = 0; iColumn < iColumnCount; iColumn++ )
                    {
                        Point widgetPosition = new Point(
                            Position.X + widgetSize.X * iColumn,
                            Position.Y + widgetSize.Y * iRow );

                        maTiles[ iColumn, iRow ].DoLayout( new Rectangle( widgetPosition.X, widgetPosition.Y, widgetSize.X, widgetSize.Y ) );
                    }
                }

                HitBox = new Rectangle( Position.X, Position.Y, Size.X, Size.Y );
            }
        }
    }
}
