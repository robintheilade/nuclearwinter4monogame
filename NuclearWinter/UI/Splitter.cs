using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace NuclearWinter.UI
{
    //---------------------------------------------------------------------------
    public class Splitter: Widget
    {
        //-----------------------------------------------------------------------
        Widget mFirstPane;
        public Widget           FirstPane
        {
            get { return mFirstPane; }
            set {
                if( mFirstPane != null )
                {
                    Debug.Assert( mFirstPane.Parent == this );

                    mFirstPane.Parent = null;
                }

                mFirstPane = value;

                if( mFirstPane != null )
                {
                    Debug.Assert( mFirstPane.Parent == null );

                    mFirstPane.Parent = this;
                }
            }
        }

        Widget mSecondPane;
        public Widget           SecondPane
        {
            get { return mSecondPane; }
            set {
                if( mSecondPane != null )
                {
                    Debug.Assert( mSecondPane.Parent == this );

                    mSecondPane.Parent = null;
                }

                mSecondPane = value;

                if( mSecondPane != null )
                {
                    Debug.Assert( mSecondPane.Parent == null );

                    mSecondPane.Parent = this;
                }
            }
        }

        public int FirstPaneMinSize = 100;
        public int SecondPaneMinSize = 100;

        Direction       mDirection;
        public int      SplitterOffset;
        const int       SplitterSize    = 10;

        bool            mbIsDragging;
        int             miDragOffset;

        //-----------------------------------------------------------------------
        // NOTE: Splitter is using a Direction instead of an Orientation so
        // it know from which side the offset is computed
        public Splitter( Screen _screen, Direction _direction )
        : base( _screen )
        {
            mDirection = _direction;
        }

        //-----------------------------------------------------------------------
        internal override void DoLayout( Rectangle _rect )
        {
            Position = _rect.Location;
            Size = new Point( _rect.Width, _rect.Height );

            switch( mDirection )
            {
                case Direction.Left:
                case Direction.Right: {
                    if( Size.X > FirstPaneMinSize + SecondPaneMinSize )
                    {
                        SplitterOffset = (int)MathHelper.Clamp( SplitterOffset, FirstPaneMinSize, _rect.Width - SecondPaneMinSize );
                    }

                    HitBox = new Rectangle(
                        _rect.Left + SplitterOffset - SplitterSize / 2,
                        _rect.Top,
                        SplitterSize,
                        _rect.Height );

                    if( mFirstPane != null )
                    {
                        mFirstPane.DoLayout( new Rectangle( _rect.Left, _rect.Top, SplitterOffset, _rect.Height ) );
                    }

                    if( mSecondPane != null )
                    {
                        mSecondPane.DoLayout( new Rectangle( _rect.Left + SplitterOffset, _rect.Top, _rect.Width - SplitterOffset, _rect.Height ) );
                    }
                    break;
                }
                case Direction.Up:
                case Direction.Down: {
                    if( Size.X > FirstPaneMinSize + SecondPaneMinSize )
                    {
                        SplitterOffset = (int)MathHelper.Clamp( SplitterOffset, FirstPaneMinSize, _rect.Height - SecondPaneMinSize );
                    }

                    HitBox = new Rectangle(
                        _rect.Left,
                        _rect.Top + SplitterOffset - SplitterSize / 2,
                        _rect.Width,
                        SplitterSize );

                    if( mFirstPane != null )
                    {
                        mFirstPane.DoLayout( new Rectangle( _rect.Left, _rect.Top, _rect.Width, SplitterOffset ) );
                    }

                    if( mSecondPane != null )
                    {
                        mSecondPane.DoLayout( new Rectangle( _rect.Left, _rect.Top + SplitterOffset, _rect.Width, _rect.Height - SplitterOffset ) );
                    }
                    break;
                }
            }
        }

        public override Widget HitTest( Point _point )
        {
            // The splitter itself
            if( HitBox.Contains( _point ) )
            {
                return this;
            }

            // The panes
            if( mFirstPane != null )
            {
                Widget widget = mFirstPane.HitTest( _point );
                if( widget != null )
                {
                    return widget;
                }
            }

            if( mSecondPane != null )
            {
                Widget widget = mSecondPane.HitTest( _point );
                if( widget != null )
                {
                    return widget;
                }
            }

            return null;
        }

        internal override void OnMouseEnter( Point _hitPoint )
        {
            switch( mDirection )
            {
                case Direction.Left:
                case Direction.Right:
                    Screen.Game.Form.Cursor = System.Windows.Forms.Cursors.SizeWE;
                    break;
                case Direction.Up:
                case Direction.Down:
                    Screen.Game.Form.Cursor = System.Windows.Forms.Cursors.SizeNS;
                    break;
            }
        }

        internal override void OnMouseOut( Point _hitPoint )
        {
            Screen.Game.Form.Cursor = System.Windows.Forms.Cursors.Default;
        }

        internal override void OnMouseMove( Point _hitPoint )
        {
            if( mbIsDragging )
            {
                switch( mDirection )
                {
                    case Direction.Left:
                    case Direction.Right:
                        SplitterOffset = _hitPoint.X + miDragOffset;
                        break;
                    case Direction.Up:
                    case Direction.Down:
                        SplitterOffset += _hitPoint.Y + miDragOffset;
                        break;
                }
            }
        }

        internal override void OnMouseDown( Point _hitPoint, int _iButton )
        {
            mbIsDragging = true;

            switch( mDirection )
            {
                case Direction.Left:
                case Direction.Right:
                    miDragOffset = SplitterOffset - _hitPoint.X;
                    break;
                case Direction.Up:
                case Direction.Down:
                    miDragOffset = SplitterOffset - _hitPoint.Y;
                    break;
            }
        }

        internal override void OnMouseUp( Point _hitPoint, int _iButton )
        {
            mbIsDragging = false;
        }

        //-----------------------------------------------------------------------
        internal override void Draw()
        {
            if( mFirstPane != null )
            {
                mFirstPane.Draw();
            }

            if( mSecondPane != null )
            {
                mSecondPane.Draw();
            }
        }
    }
}
