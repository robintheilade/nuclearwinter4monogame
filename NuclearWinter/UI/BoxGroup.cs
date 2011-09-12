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
        Orientation     mOrientation;
        int             miSpacing; // FIXME: Not taken into account

        //----------------------------------------------------------------------
        public BoxGroup( Screen _screen, Orientation _orientation, int _iSpacing )
        : base( _screen )
        {
            mOrientation  = _orientation;
            miSpacing   = _iSpacing;
        }

        //----------------------------------------------------------------------
        public override Widget GetFirstFocusableDescendant( Direction _direction )
        {
            if( mOrientation == Orientation.Vertical )
            {
                return mlChildren[0].GetFirstFocusableDescendant( _direction );
            }
            else
            {
                return mlChildren[0].GetFirstFocusableDescendant( _direction );
            }
        }

        //----------------------------------------------------------------------
        public override Widget GetSibling( Direction _direction, Widget _child )
        {
            int iIndex = mlChildren.IndexOf( _child );

            if( mOrientation == Orientation.Horizontal )
            {
                if( _direction == Direction.Right )
                {
                    if( iIndex < mlChildren.Count - 1 )
                    {
                        return mlChildren[iIndex + 1];
                    }
                }
                else
                if( _direction == Direction.Left )
                {
                    if( iIndex > 0 )
                    {
                        return mlChildren[iIndex - 1];
                    }
                }
            }
            else
            {
                if( _direction == Direction.Down )
                {
                    if( iIndex < mlChildren.Count - 1 )
                    {
                        return mlChildren[iIndex + 1];
                    }
                }
                else
                if( _direction == Direction.Up )
                {
                    if( iIndex > 0 )
                    {
                        return mlChildren[iIndex - 1];
                    }
                }
            }

            return base.GetSibling( _direction, this );
        }

        //----------------------------------------------------------------------
        internal override void UpdateContentSize()
        {
            if( mOrientation == Orientation.Horizontal )
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

            base.UpdateContentSize();
        }

        //----------------------------------------------------------------------
        internal override void DoLayout( Rectangle _rect )
        {
            Position        = _rect.Location;
            Size            = new Point( _rect.Width, _rect.Height );

            Debug.Assert( Size.X != 0 && Size.Y != 0 );

            int iSize = 0;
            foreach( Widget widget in mlChildren )
            {
                iSize += mOrientation == Orientation.Horizontal ? widget.ContentWidth : widget.ContentHeight;
            }

            if( mlChildren.Count > 1 )
            {
                iSize += ( mlChildren.Count - 1 ) * miSpacing;
            }

            Point pWidgetPosition;
                
            switch( mOrientation )
            {
                case Orientation.Horizontal:
                    pWidgetPosition = new Point( Position.X + Size.X / 2 - iSize / 2, Position.Y );
                    break;
                case Orientation.Vertical:
                    pWidgetPosition = new Point( Position.X, Position.Y + Size.Y / 2 - iSize / 2 );
                    break;
                default:
                    throw new NotSupportedException();
            }

            foreach( Widget widget in mlChildren )
            {
                widget.DoLayout( new Rectangle( pWidgetPosition.X, pWidgetPosition.Y, mOrientation == Orientation.Horizontal ? widget.ContentWidth : Size.X, mOrientation == Orientation.Horizontal ? Size.Y : widget.ContentHeight ) );
                
                switch( mOrientation )
                {
                    case Orientation.Horizontal:
                        pWidgetPosition.X += widget.ContentWidth + miSpacing;
                        break;
                    case Orientation.Vertical:
                        pWidgetPosition.Y += widget.ContentHeight + miSpacing;
                        break;
                }
            }

            HitBox = new Rectangle( Position.X, Position.Y, Size.X, Size.Y );
        }
    }
}
