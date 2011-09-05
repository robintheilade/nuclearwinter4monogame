using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace NuclearWinter.UI
{
    /*
     * BoxGroup allows packing widgets in one direction
     * It takes care of positioning each child widget properly
     */
    public class BoxGroup: Group
    {
        //----------------------------------------------------------------------
        Direction       mDirection;
        int             miSpacing; // FIXME: Not taken into account

        //----------------------------------------------------------------------
        public BoxGroup( Screen _screen, Direction _direction, int _iSpacing )
        : base( _screen )
        {
            mDirection  = _direction;
            miSpacing   = _iSpacing;
        }

        //----------------------------------------------------------------------
        public override Widget GetFirstFocusableDescendant( Direction _direction )
        {
            if( mDirection == Direction.Left || mDirection == Direction.Right )
            {
                switch( _direction )
                {
                    case Direction.Left:
                        return mlChildren[ mlChildren.Count - 1 ].GetFirstFocusableDescendant( _direction );
                    case Direction.Right:
                        return mlChildren[0].GetFirstFocusableDescendant( _direction );
                    default:
                        return mlChildren[0].GetFirstFocusableDescendant( _direction );
                }
            }
            else
            {
                switch( _direction )
                {
                    case Direction.Up:
                        return mlChildren[ mlChildren.Count - 1 ].GetFirstFocusableDescendant( _direction );
                    case Direction.Down:
                        return mlChildren[0].GetFirstFocusableDescendant( _direction );
                    default:
                        return mlChildren[0].GetFirstFocusableDescendant( _direction );
                }
            }
        }

        //----------------------------------------------------------------------
        public override Widget GetSibling( Direction _direction, Widget _child )
        {
            int iIndex = mlChildren.IndexOf( _child );

            if( mDirection == _direction )
            {
                if( iIndex < mlChildren.Count - 1 )
                {
                    return mlChildren[iIndex + 1];
                }
            }
            else
            {
                if( mDirection == Direction.Left || mDirection == Direction.Right )
                {
                    if( _direction == Direction.Left || _direction == Direction.Right )
                    {
                        if( iIndex > 0 )
                        {
                            return mlChildren[iIndex - 1];
                        }
                    }
                }
                else
                {
                    if( _direction == Direction.Up || _direction == Direction.Down )
                    {
                        if( iIndex > 0 )
                        {
                            return mlChildren[iIndex - 1];
                        }
                    }
                }
            }

            return base.GetSibling( _direction, this );
        }

        //----------------------------------------------------------------------
        internal override void UpdateContentSize()
        {
            bool bHorizontal = mDirection == Direction.Left || mDirection == Direction.Right;

            if( bHorizontal )
            {
                ContentWidth    = Padding.Horizontal;
                ContentHeight   = 0;
                foreach( Widget child in mlChildren )
                {
                    ContentWidth += child.ContentWidth;
                    ContentHeight = Math.Max( ContentHeight, child.ContentHeight );
                }

                ContentHeight += Padding.Vertical;

                if( mlChildren.Count > 1 )
                {
                    ContentWidth += miSpacing * ( mlChildren.Count - 1 );
                }
            }
            else
            {
                ContentHeight   = Padding.Vertical;
                ContentWidth    = 0;
                foreach( Widget child in mlChildren )
                {
                    ContentHeight += child.ContentHeight;
                    ContentWidth = Math.Max( ContentWidth, child.ContentWidth );
                }

                ContentWidth += Padding.Horizontal;
                if( mlChildren.Count > 1 )
                {
                    ContentHeight += miSpacing * ( mlChildren.Count - 1 );
                }
            }
        }

        //----------------------------------------------------------------------
        internal override void DoLayout( Rectangle _rect )
        {
            Position        = _rect.Location;
            Size            = new Point( _rect.Width, _rect.Height );

            Debug.Assert( Size.X != 0 && Size.Y != 0 );

            bool bHorizontal = mDirection == Direction.Left || mDirection == Direction.Right;

            int iSize = 0;
            foreach( Widget widget in mlChildren )
            {
                iSize += bHorizontal ? widget.ContentWidth : widget.ContentHeight;
            }

            if( mlChildren.Count > 1 )
            {
                iSize += ( mlChildren.Count - 1 ) * miSpacing;
            }

            Point pWidgetPosition;
                
            switch( mDirection )
            {
                case Direction.Left:
                    pWidgetPosition = new Point( Position.X + Size.X / 2 + iSize / 2, Position.Y );
                    break;
                case Direction.Up:
                    pWidgetPosition = new Point( Position.X, Position.Y + Size.Y / 2 + iSize / 2 );
                    break;
                case Direction.Right:
                    pWidgetPosition = new Point( Position.X + Size.X / 2 - iSize / 2, Position.Y );
                    break;
                case Direction.Down:
                    pWidgetPosition = new Point( Position.X, Position.Y + Size.Y / 2 - iSize / 2 );
                    break;
                default:
                    throw new NotSupportedException();
            }

            foreach( Widget widget in mlChildren )
            {
                if( mDirection == Direction.Left )
                {
                    pWidgetPosition.X -= widget.ContentWidth;
                }
                else
                if( mDirection == Direction.Up )
                {
                    pWidgetPosition.Y -= widget.ContentHeight;
                }

                widget.DoLayout( new Rectangle( pWidgetPosition.X, pWidgetPosition.Y, bHorizontal ? widget.ContentWidth : Size.X, bHorizontal ? Size.Y : widget.ContentHeight ) );
                
                switch( mDirection )
                {
                    case Direction.Right:
                        pWidgetPosition.X += widget.ContentWidth + miSpacing;
                        break;
                    case Direction.Left:
                        pWidgetPosition.X -= miSpacing;
                        break;
                    case Direction.Down:
                        pWidgetPosition.Y += widget.ContentHeight + miSpacing;
                        break;
                    case Direction.Up:
                        pWidgetPosition.Y -= miSpacing;
                        break;
                }
            }

            HitBox = new Rectangle( Position.X, Position.Y, Size.X, Size.Y );
        }
    }
}
