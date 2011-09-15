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
        int             miSpacing;

        List<bool>      mlExpandedChildren;

        //----------------------------------------------------------------------
        public BoxGroup( Screen _screen, Orientation _orientation, int _iSpacing )
        : base( _screen )
        {
            mlExpandedChildren = new List<bool>();

            mOrientation    = _orientation;
            miSpacing       = _iSpacing;
        }

        //----------------------------------------------------------------------
        public void AddChild( Widget _widget, int _iIndex, bool _bExpand )
        {
            Debug.Assert( _widget.Parent == null );

            _widget.Parent = this;
            mlExpandedChildren.Insert( _iIndex, _bExpand );
            mlChildren.Insert( _iIndex, _widget );
            UpdateContentSize();
        }

        public override void AddChild( Widget _widget, int _iIndex )
        {
            AddChild( _widget, _iIndex, false );
        }

        public void AddChild( Widget _widget, bool _bExpand )
        {
            AddChild( _widget, mlChildren.Count, _bExpand );
        }

        public override void RemoveChild( Widget _widget )
        {
            Debug.Assert( _widget.Parent == this );

            _widget.Parent = null;

            mlExpandedChildren.RemoveAt( mlChildren.IndexOf( _widget ) );

            mlChildren.Remove( _widget );
            UpdateContentSize();
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

            int iUnexpandedSize = 0;
            int iExpandedChildrenCount = 0;
            for( int iIndex = 0; iIndex < mlChildren.Count; iIndex++ )
            {
                if( ! mlExpandedChildren[iIndex] )
                {
                    iUnexpandedSize += ( mOrientation == Orientation.Horizontal ) ? mlChildren[iIndex].ContentWidth : mlChildren[iIndex].ContentHeight;
                }
                else
                {
                    iExpandedChildrenCount++;
                }
            }

            int iExpandedWidgetSize = 0;
            if( iExpandedChildrenCount > 0 )
            {
                iExpandedWidgetSize = ( ( ( mOrientation == Orientation.Horizontal ) ? Size.X : Size.Y ) - iUnexpandedSize ) / iExpandedChildrenCount;
            }

            if( mlChildren.Count > 1 )
            {
                iUnexpandedSize += ( mlChildren.Count - 1 ) * miSpacing;
            }

            int iActualSize = iExpandedChildrenCount > 0 ? ( ( mOrientation == Orientation.Horizontal ) ? Size.X : Size.Y ) : iUnexpandedSize;

            Point pWidgetPosition;
            
            switch( mOrientation )
            {
                case Orientation.Horizontal:
                    pWidgetPosition = new Point( Position.X + Size.X / 2 - iActualSize / 2, Position.Y );
                    break;
                case Orientation.Vertical:
                    pWidgetPosition = new Point( Position.X, Position.Y + Size.Y / 2 - iActualSize / 2 );
                    break;
                default:
                    throw new NotSupportedException();
            }

            for( int iIndex = 0; iIndex < mlChildren.Count; iIndex++ )
            {
                Widget widget = mlChildren[iIndex];

                int iWidgetSize = ( mOrientation == Orientation.Horizontal ) ? widget.ContentWidth : widget.ContentHeight;
                if( mlExpandedChildren[iIndex] )
                {
                    iWidgetSize = iExpandedWidgetSize;
                }
                widget.DoLayout( new Rectangle( pWidgetPosition.X, pWidgetPosition.Y, mOrientation == Orientation.Horizontal ? iWidgetSize : Size.X, mOrientation == Orientation.Horizontal ? Size.Y : iWidgetSize ) );
                
                switch( mOrientation )
                {
                    case Orientation.Horizontal:
                        pWidgetPosition.X += iWidgetSize + miSpacing;
                        break;
                    case Orientation.Vertical:
                        pWidgetPosition.Y += iWidgetSize + miSpacing;
                        break;
                }
            }

            HitBox = new Rectangle( Position.X, Position.Y, Size.X, Size.Y );
        }
    }
}
