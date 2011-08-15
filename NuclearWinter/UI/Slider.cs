using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using NuclearWinter.Animation;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace NuclearWinter.UI
{
    public class Slider: Widget
    {
        bool                    mbIsHovered;
        bool                    mbIsPressed;

        public int              MinValue;
        public int              MaxValue;
        public int              Step = 1;

        int                     miValue;
        public int              Value
        {
            get {
                return miValue;
            }

            set {
                miValue = (int)MathHelper.Clamp( value, MinValue, MaxValue );
                miValue -= miValue % Step;
            }
        }

        public Action           ChangeHandler;

        public override bool CanFocus { get { return true; } }

        //----------------------------------------------------------------------
        public Slider( Screen _screen, int _iMin, int _iMax, int _iInitialValue, int _iStep )
        : base( _screen )
        {
            Debug.Assert( _iMin < _iMax );

            MinValue    = _iMin;
            MaxValue    = _iMax;
            Value       = _iInitialValue;
            Step        = _iStep;

            UpdateContentSize();
        }

        //----------------------------------------------------------------------
        protected override void UpdateContentSize()
        {
        }

        //----------------------------------------------------------------------
        public override void DoLayout(Rectangle? _rect)
        {
            if( _rect.HasValue )
            {
                Position = _rect.Value.Location;
                Size = new Point( _rect.Value.Width, _rect.Value.Height );
                HitBox = _rect.Value;
            }
        }

        //----------------------------------------------------------------------
        public override void OnMouseEnter( Point _hitPoint )
        {
            mbIsHovered = true;
        }

        public override void OnMouseOut( Point _hitPoint )
        {
            mbIsHovered = false;
        }

        //----------------------------------------------------------------------
        public override void OnMouseDown( Point _hitPoint )
        {
            Screen.Focus( this );
            mbIsPressed = true;

            int handleSize = Size.Y;
            Value = MinValue + (int)( ( MaxValue - MinValue ) * ( _hitPoint.X - Position.X - handleSize / 4 ) / ( Size.X - handleSize ) );
            if( ChangeHandler != null ) ChangeHandler();
        }

        public override void OnMouseMove( Point _hitPoint )
        {
            if( mbIsPressed )
            {
                int handleSize = Size.Y;

                int iValue = MinValue + (int)( ( MaxValue - MinValue ) * ( _hitPoint.X - Position.X - handleSize / 4 ) / ( Size.X - handleSize ) );

                if( iValue != miValue ) 
                {
                    Value = iValue;
                    if( ChangeHandler != null ) ChangeHandler();
                }
            }
        }

        public override void OnMouseUp( Point _hitPoint )
        {
            mbIsPressed = false;
        }

        //----------------------------------------------------------------------
        public override void OnPadMove(Direction _direction)
        {
            if( _direction == Direction.Left )
            {
                Value -= Step;
                if( ChangeHandler != null ) ChangeHandler();
            }
            else
            if( _direction == Direction.Right )
            {
                Value += Step;
                if( ChangeHandler != null ) ChangeHandler();
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

            int handleSize = Size.Y;
            int handleX = Position.X + (int)( ( Size.X - handleSize ) * (float)( Value - MinValue ) / MaxValue );

            Screen.DrawBox( (!mbIsPressed) ? Screen.Style.ButtonFrame : Screen.Style.ButtonFrameDown, new Rectangle( handleX, Position.Y, handleSize, handleSize ), 30, Color.White );
            if( mbIsHovered && ! mbIsPressed )
            {
                Screen.DrawBox( Screen.Style.ButtonFrameHover, new Rectangle( handleX, Position.Y, handleSize, handleSize ), 30, Color.White );
            }

            if( HasFocus && ! mbIsPressed )
            {
                Screen.DrawBox( Screen.Style.ButtonFrameFocused, new Rectangle( handleX, Position.Y, handleSize, handleSize ), 30, Color.White );
            }
        }
    }
}
